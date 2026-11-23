using InterCase.Repository.Interfaces;
using InterCase.Service.Interfaces;

namespace InterCase.Service
{
    public sealed class ServiceManager(IRepositoryManager repositoryManager) : IServiceManager
    {
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
