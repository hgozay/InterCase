using AutoMapper;
using Hangfire;
using InterCase.Common;
using InterCase.Repository.Interfaces;
using InterCase.Service.Interfaces;
using InterCase.Service.Services.DbServices;
using InterCase.Service.Services.OtherServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InterCase.Service
{
    public sealed class ServiceManager(
        IRepositoryManager repositoryManager,
        IMapper mapper,
        ICacheService cache,
        IQueueService queue,
        ICorrelationContext correlation,
        IOptions<CacheOptions> cacheOptions,
        IOptions<ProcessingOptions> processingOptions,
        TimeProvider timeProvider,
        JobStorage jobStorage,
        ILoggerFactory loggerFactory) : IServiceManager
    {
        private readonly Lazy<IUserService> _user = new(() => new UserService(
            repositoryManager,
            mapper,
            timeProvider,
            loggerFactory.CreateLogger<UserService>()));

        private readonly Lazy<IOrderService> _order = new(() => new OrderService(
            repositoryManager,
            mapper,
            cache,
            queue,
            correlation,
            cacheOptions,
            processingOptions,
            timeProvider,
            loggerFactory.CreateLogger<OrderService>()));

        private readonly Lazy<INotificationService> _notification = new(() => new NotificationService(
            repositoryManager,
            mapper));

        private readonly Lazy<IWorkerStatusService> _worker = new(() => new WorkerStatusService(
            jobStorage,
            timeProvider));

        public IUserService User => _user.Value;
        public IOrderService Order => _order.Value;
        public INotificationService Notification => _notification.Value;
        public ICacheService Cache => cache;
        public IQueueService Queue => queue;
        public IWorkerStatusService Worker => _worker.Value;

        public int SaveChanges() => repositoryManager.SaveChanges();

        public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
            repositoryManager.SaveChangesAsync(ct);

        public Task BeginTransactionAsync(CancellationToken ct = default) =>
            repositoryManager.BeginTransactionAsync(ct);

        public Task CommitTransactionAsync(CancellationToken ct = default) =>
            repositoryManager.CommitTransactionAsync(ct);

        public Task RollbackTransactionAsync() =>
            repositoryManager.RollbackTransactionAsync();
    }
}
