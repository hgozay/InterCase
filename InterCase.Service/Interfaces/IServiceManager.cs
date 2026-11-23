namespace InterCase.Service.Interfaces
{
    public interface IServiceManager
    {
        IUserService User { get; }
        IOrderService Order { get; }
        INotificationService Notification { get; }
        ICacheService Cache { get; }
        IQueueService Queue { get; }

        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken ct = default);

        Task BeginTransactionAsync(CancellationToken ct = default);
        Task CommitTransactionAsync(CancellationToken ct = default);
        Task RollbackTransactionAsync();
    }
}
