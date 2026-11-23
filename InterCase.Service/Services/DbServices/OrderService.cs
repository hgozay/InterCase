using AutoMapper;
using InterCase.Common;
using InterCase.Common.Dtos.Orders;
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
        ILogger<OrderService> logger)
        : BaseService<Order, OrderResponse, CreateOrderRequest>(repositoryManager, mapper, logger), IOrderService
    {
        protected override IBaseRepository<Order> Repository => RepositoryManager.Order;

        protected override async Task OnCreatedAsync(Order order, CancellationToken ct)
        {
            try
            {
                var message = Mapper.Map<OrderPlacedMessage>(order, options =>
                    options.Items[MappingProfile.CorrelationIdKey] = correlation.CorrelationId);
                await queue.PublishAsync(QueueNames.OrderPlaced, message, message.CorrelationId, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Publishing OrderPlaced failed for order {OrderId}", order.Id);
            }

            await cache.RemoveAsync(CacheKeys.OrdersByUser(order.UserId));
            Logger.LogInformation("Order {OrderId} ({PublicId}) created for {UserId}", order.Id, order.PublicId, order.UserId);
        }

        protected override Task OnUpdatedAsync(Order order, CancellationToken ct) =>
            cache.RemoveAsync(CacheKeys.OrdersByUser(order.UserId));

        protected override Task OnDeletedAsync(Order order, CancellationToken ct) =>
            cache.RemoveAsync(CacheKeys.OrdersByUser(order.UserId));

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
    }
}
