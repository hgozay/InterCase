using InterCase.Common.Dtos.Notifications;
using InterCase.Common.Dtos.Paging;

namespace InterCase.Service.Interfaces
{
    public interface INotificationService
    {
        Task<PagedResponse<NotificationResponse>> GetByUserIdAsync(Guid userId, PageRequest paging, CancellationToken ct = default);
    }
}
