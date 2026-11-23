namespace InterCase.Service.Interfaces
{
    public interface IServiceManager
    {
        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken ct = default);

        Task BeginTransactionAsync(CancellationToken ct = default);
        Task CommitTransactionAsync(CancellationToken ct = default);
        Task RollbackTransactionAsync();
    }
}
