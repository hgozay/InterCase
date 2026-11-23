using InterCase.Common;
using InterCase.Repository.Context;
using InterCase.Repository.Entities;
using InterCase.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InterCase.Repository.Repositories
{
    public sealed class OrderRepository(AppDbContext context) : BaseRepository<Order>(context), IOrderRepository
    {
        public Task<List<Order>> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
            Query(trackChanges: false)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(ct);

        public Task<List<Order>> GetStalePendingAsync(DateTime olderThanUtc, CancellationToken ct = default) =>
            Query(trackChanges: false)
                .Where(o => o.Status == OrderStatus.Pending && o.CreatedAt < olderThanUtc)
                .ToListAsync(ct);
    }
}
