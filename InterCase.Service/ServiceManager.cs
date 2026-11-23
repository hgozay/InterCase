using AutoMapper;
using InterCase.Common;
using InterCase.Repository.Interfaces;
using InterCase.Service.Interfaces;
using InterCase.Service.Services.DbServices;
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
        ILoggerFactory loggerFactory) : IServiceManager
    {
        private readonly Lazy<IOrderService> _order = new(() => new OrderService(
            repositoryManager,
            mapper,
            cache,
            queue,
            correlation,
            cacheOptions,
            loggerFactory.CreateLogger<OrderService>()));

        public IOrderService Order => _order.Value;
        public ICacheService Cache => cache;
        public IQueueService Queue => queue;

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
