using InterCase.Repository.Entities;

namespace InterCase.Repository.Interfaces
{
    public interface IOrderRepository : IBaseRepository<Order>
    {
        Task<List<Order>> GetByUserIdAsync(Guid userPublicId, CancellationToken ct = default);
        Task<List<Order>> GetStalePendingAsync(DateTime olderThanUtc, CancellationToken ct = default);
    }
}
