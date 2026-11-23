using InterCase.Common.Dtos.Notifications;
using InterCase.Common.Dtos.Paging;
using InterCase.Common.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace InterCase.API.Controllers
{
    [Route("notifications")]
    public sealed class NotificationsController : BaseController
    {
        [HttpGet("{userId}")]
        public async Task<ActionResult<PagedResponse<NotificationResponse>>> GetByUserId(
            [FromRoute, NotEmptyGuid(ErrorMessage = Localization.Keys.UserIdEmpty)] Guid userId,
            [FromQuery] PageRequest paging,
            CancellationToken ct) =>
            DenyIfNotOwner(userId) ?? Ok(await ServiceManager.Notification.GetByUserIdAsync(userId, paging, ct));
    }
}
