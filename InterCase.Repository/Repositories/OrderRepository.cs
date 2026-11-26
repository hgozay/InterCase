using InterCase.Common;
using InterCase.Repository.Context;
using InterCase.Repository.Entities;
using InterCase.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InterCase.Repository.Repositories
{
    public sealed class OrderRepository(AppDbContext context) : BaseRepository<Order>(context), IOrderRepository
    {
        protected override IQueryable<Order> Query(bool trackChanges) =>
            base.Query(trackChanges)
                .Include(o => o.User)
                .Include(o => o.Notifications);

        public Task<List<Order>> GetByUserIdAsync(Guid userPublicId, CancellationToken ct = default) =>
            Query(trackChanges: false)
                .Where(o => o.User.PublicId == userPublicId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(ct);

        public Task<(List<Order> Items, int TotalCount)> GetPageByUserIdAsync(Guid userPublicId, OrderStatus? status, int page, int pageSize, CancellationToken ct = default) =>
            ToPageAsync(
                Query(trackChanges: false)
                    .Where(o => o.User.PublicId == userPublicId && (status == null || o.Status == status))
                    .OrderByDescending(o => o.CreatedAt)
                    .ThenByDescending(o => o.Id),
                page,
                pageSize,
                ct);

        public Task<List<Order>> GetStalePendingAsync(DateTime olderThanUtc, CancellationToken ct = default) =>
            Query(trackChanges: false)
                .Where(o => o.Status == OrderStatus.Pending && o.CreatedAt < olderThanUtc)
                .ToListAsync(ct);

        public async Task<bool> TryChangeStatusAsync(long id, OrderStatus status, DateTime changedAt, CancellationToken ct = default)
        {
            DateTime? processedAt = status == OrderStatus.Processed ? changedAt : null;

            var updated = await Context.Set<Order>()
                .Where(o => o.Id == id && !o.IsDeleted && o.Status != OrderStatus.Processed)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(o => o.Status, status)
                    .SetProperty(o => o.UpdatedAt, changedAt)
                    .SetProperty(o => o.ProcessedAt, o => processedAt ?? o.ProcessedAt), ct);

            return updated == 1;
        }
    }
}
