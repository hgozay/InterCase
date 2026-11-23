using InterCase.Repository.Context;
using InterCase.Repository.Entities;
using InterCase.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InterCase.Repository.Repositories
{
    public sealed class NotificationRepository(AppDbContext context)
        : BaseRepository<Notification>(context), INotificationRepository
    {
        protected override IQueryable<Notification> Query(bool trackChanges) =>
            base.Query(trackChanges).Include(n => n.Order);

        public Task<(List<Notification> Items, int TotalCount)> GetPageByUserIdAsync(Guid userPublicId, int page, int pageSize, CancellationToken ct = default) =>
            ToPageAsync(
                Query(trackChanges: false)
                    .Where(n => n.User.PublicId == userPublicId)
                    .OrderByDescending(n => n.SentAt)
                    .ThenByDescending(n => n.Id),
                page,
                pageSize,
                ct);
    }
}
