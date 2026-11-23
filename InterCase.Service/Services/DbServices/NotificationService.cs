using AutoMapper;
using InterCase.Common.Dtos.Notifications;
using InterCase.Common.Dtos.Paging;
using InterCase.Repository.Interfaces;
using InterCase.Service.Interfaces;

namespace InterCase.Service.Services.DbServices
{
    public sealed class NotificationService(
        IRepositoryManager repositoryManager,
        IMapper mapper) : INotificationService
    {
        public async Task<PagedResponse<NotificationResponse>> GetByUserIdAsync(Guid userId, PageRequest paging, CancellationToken ct = default)
        {
            var (notifications, totalCount) = await repositoryManager.Notification.GetPageByUserIdAsync(userId, paging.Page, paging.PageSize, ct);
            var items = mapper.Map<List<NotificationResponse>>(notifications);

            return new PagedResponse<NotificationResponse>(items, paging.Page, paging.PageSize, totalCount);
        }
    }
}
