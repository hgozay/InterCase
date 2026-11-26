using InterCase.Common;
using InterCase.Repository.Entities;

namespace InterCase.Repository.Interfaces
{
    public interface IOrderRepository : IBaseRepository<Order>
    {
        Task<List<Order>> GetByUserIdAsync(Guid userPublicId, CancellationToken ct = default);
        Task<(List<Order> Items, int TotalCount)> GetPageByUserIdAsync(Guid userPublicId, OrderStatus? status, int page, int pageSize, CancellationToken ct = default);
        Task<List<Order>> GetStalePendingAsync(DateTime olderThanUtc, CancellationToken ct = default);
        Task<bool> TryChangeStatusAsync(long id, OrderStatus status, DateTime changedAt, CancellationToken ct = default);
    }
}
