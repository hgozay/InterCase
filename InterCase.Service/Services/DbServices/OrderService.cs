using System.Globalization;
using AutoMapper;
using InterCase.Common;
using InterCase.Common.Dtos.Orders;
using InterCase.Common.Helpers;
using InterCase.Repository.Entities;
using InterCase.Repository.Interfaces;
using InterCase.Service.Interfaces;
using InterCase.Service.Mappings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InterCase.Service.Services.DbServices
{
    public sealed class OrderService(
        IRepositoryManager repositoryManager,
        IMapper mapper,
        ICacheService cache,
        IQueueService queue,
        ICorrelationContext correlation,
        IOptions<CacheOptions> cacheOptions,
        IOptions<ProcessingOptions> processingOptions,
        TimeProvider timeProvider,
        ILogger<OrderService> logger)
        : BaseService<Order, OrderResponse, CreateOrderRequest, UpdateOrderRequest>(repositoryManager, mapper, logger), IOrderService
    {
        protected override IBaseRepository<Order> Repository => RepositoryManager.Order;

        protected override async Task OnCreatingAsync(Order order, CreateOrderRequest request, CancellationToken ct)
        {
            order.User = await RepositoryManager.User.GetByPublicIdAsync(request.UserId!.Value, trackChanges: true, ct)
                ?? throw new NotFoundException($"User {request.UserId} was not found.");
        }

        protected override async Task OnCreatedAsync(Order order, CancellationToken ct)
        {
            try
            {
                await PublishPlacedAsync(order);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Publishing OrderPlaced failed for order {OrderId}", order.Id);
            }

            await cache.RemoveAsync(CacheKeys.OrdersByUser(order.User.PublicId));
            Logger.LogInformation("Order {OrderId} ({PublicId}) created for {UserId}", order.Id, order.PublicId, order.User.PublicId);
        }

        protected override void EnsureCanUpdate(Order order) => EnsurePending(order);

        protected override void EnsureCanDelete(Order order) => EnsurePending(order);

        protected override Task OnUpdatedAsync(Order order, CancellationToken ct) =>
            cache.RemoveAsync(CacheKeys.OrdersByUser(order.User.PublicId));

        protected override Task OnDeletedAsync(Order order, CancellationToken ct) =>
            cache.RemoveAsync(CacheKeys.OrdersByUser(order.User.PublicId));

        public async Task<IReadOnlyList<OrderResponse>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            var key = CacheKeys.OrdersByUser(userId);
            var cached = await cache.GetAsync<List<OrderResponse>>(key);
            if (cached is not null)
            {
                Logger.LogDebug("Cache hit for {CacheKey}", key);
                return cached;
            }

            var orders = await RepositoryManager.Order.GetByUserIdAsync(userId, ct);
            var response = orders.Select(ToResponse).ToList();

            await cache.SetAsync(key, response, TimeSpan.FromSeconds(cacheOptions.Value.OrdersTtlSeconds));
            Logger.LogDebug("Cache miss for {CacheKey}, stored {Count} orders", key, response.Count);

            return response;
        }

        public async Task ProcessAsync(long orderId, CancellationToken ct = default)
        {
            var order = await RepositoryManager.Order.GetByIdAsync(orderId, trackChanges: true, ct);
            if (order is null)
            {
                Logger.LogWarning("Order {OrderId} was not found; nothing to process", orderId);
                return;
            }

            if (order.Status == OrderStatus.Processed)
            {
                Logger.LogInformation("Order {OrderId} was already processed", orderId);
            }
            else
            {
                order.Status = OrderStatus.Processing;
                await RepositoryManager.SaveChangesAsync(ct);

                await Task.Delay(TimeSpan.FromSeconds(processingOptions.Value.DelaySeconds), timeProvider, ct);

                var processedAt = timeProvider.GetUtcNow().UtcDateTime;
                var notification = new Notification
                {
                    UserId = order.UserId,
                    Channel = NotificationChannels.Console,
                    Message = string.Format(
                        CultureInfo.InvariantCulture,
                        Localization.Get(Localization.Keys.OrderProcessedNotification, Localization.DefaultLanguage),
                        order.PublicId),
                    SentAt = processedAt
                };

                order.Status = OrderStatus.Processed;
                order.ProcessedAt = processedAt;
                order.Notifications.Add(notification);
                await RepositoryManager.SaveChangesAsync(ct);

                Logger.LogInformation("Notification sent to user {UserId} via {Channel}: {Message}", order.User.PublicId, notification.Channel, notification.Message);
            }

            await cache.SetStringAsync(
                CacheKeys.OrderLog(order.Id),
                $"Order {order.Id} processed at {order.ProcessedAt!.Value.ToString("O", CultureInfo.InvariantCulture)}");
            await cache.RemoveAsync(CacheKeys.OrdersByUser(order.User.PublicId));
        }

        public async Task MarkFailedAsync(long orderId, CancellationToken ct = default)
        {
            var order = await RepositoryManager.Order.GetByIdAsync(orderId, trackChanges: true, ct);
            if (order is null || order.Status == OrderStatus.Processed)
                return;

            order.Status = OrderStatus.Failed;
            await RepositoryManager.SaveChangesAsync(ct);
            await cache.RemoveAsync(CacheKeys.OrdersByUser(order.User.PublicId));

            Logger.LogWarning("Order {OrderId} marked as Failed", orderId);
        }

        public async Task<int> RepublishStalePendingAsync(CancellationToken ct = default)
        {
            var olderThan = timeProvider.GetUtcNow().UtcDateTime.AddMinutes(-processingOptions.Value.StalePendingMinutes);
            var staleOrders = await RepositoryManager.Order.GetStalePendingAsync(olderThan, ct);
            var published = 0;

            foreach (var order in staleOrders)
            {
                try
                {
                    await PublishPlacedAsync(order);
                    published++;
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Republishing OrderPlaced failed for order {OrderId}", order.Id);
                }
            }

            if (staleOrders.Count > 0)
                Logger.LogInformation("Republished {Published} of {Stale} orders pending since before {OlderThan}", published, staleOrders.Count, olderThan);

            return published;
        }

        private static void EnsurePending(Order order)
        {
            if (order.Status != OrderStatus.Pending)
                throw new ConflictException(Localization.Keys.OrderNotPending);
        }

        private async Task PublishPlacedAsync(Order order)
        {
            var message = Mapper.Map<OrderPlacedMessage>(order, options =>
                options.Items[MappingProfile.CorrelationIdKey] = correlation.CorrelationId);
            await queue.PublishAsync(QueueNames.OrderPlaced, message, message.CorrelationId, CancellationToken.None);
        }
    }
}
