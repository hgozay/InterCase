using InterCase.Repository.Entities;

namespace InterCase.Repository.Interfaces
{
    public interface INotificationRepository : IBaseRepository<Notification>
    {
        Task<(List<Notification> Items, int TotalCount)> GetPageByUserIdAsync(Guid userPublicId, int page, int pageSize, CancellationToken ct = default);
    }
}
