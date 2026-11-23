using InterCase.Repository.Entities;

namespace InterCase.Repository.Interfaces
{
    public interface IRepositoryManager
    {
        IUserRepository User { get; }
        IBaseRepository<UserLogin> UserLogin { get; }
        IOrderRepository Order { get; }
        INotificationRepository Notification { get; }

        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken ct = default);

        Task BeginTransactionAsync(CancellationToken ct = default);
        Task CommitTransactionAsync(CancellationToken ct = default);
        Task RollbackTransactionAsync();
    }
}
