using InterCase.Common.Dtos.Paging;
using InterCase.UI.Helpers;
using InterCase.UI.Models;
using Microsoft.AspNetCore.Mvc;

namespace InterCase.UI.Controllers
{
    public sealed class ActivityController : BaseController
    {
        [HttpGet("activity")]
        public async Task<IActionResult> Index(int notificationsPage = 1, int logsPage = 1, CancellationToken ct = default)
        {
            if (User.GetUserId() is not { } userId)
                return await ExpireSessionAsync();

            notificationsPage = Math.Max(1, notificationsPage);
            logsPage = Math.Max(1, logsPage);

            var notificationsTask = ApiClient.GetNotificationsAsync(userId, notificationsPage, PageRequest.DefaultPageSize, ct);
            var logsTask = ApiClient.GetOrderLogsAsync(userId, logsPage, PageRequest.DefaultPageSize, ct);
            await Task.WhenAll(notificationsTask, logsTask);

            var notifications = await notificationsTask;
            var logs = await logsTask;
            if (notifications.IsUnauthorized || logs.IsUnauthorized)
                return await ExpireSessionAsync();

            var lastNotificationsPage = LastPage(notifications);
            var lastLogsPage = LastPage(logs);
            if (notificationsPage > lastNotificationsPage || logsPage > lastLogsPage)
            {
                return RedirectToAction(nameof(Index), new
                {
                    notificationsPage = Math.Min(notificationsPage, lastNotificationsPage),
                    logsPage = Math.Min(logsPage, lastLogsPage)
                });
            }

            return View(new ActivityPageViewModel
            {
                Notifications = ToList(notifications, notificationsPage),
                Logs = ToList(logs, logsPage)
            });
        }

        private static int LastPage<T>(ApiResult<PagedResponse<T>> result) =>
            result.IsSuccess ? Math.Max(1, result.Value!.TotalPages) : int.MaxValue;

        private static PagedListViewModel<T> ToList<T>(ApiResult<PagedResponse<T>> result, int page)
        {
            if (!result.IsSuccess)
            {
                return new PagedListViewModel<T>
                {
                    Page = page,
                    Error = DescribeError(result),
                    CorrelationId = result.CorrelationId
                };
            }

            var value = result.Value!;
            return new PagedListViewModel<T>
            {
                Items = value.Items,
                Page = value.Page,
                TotalPages = Math.Max(1, value.TotalPages),
                TotalCount = value.TotalCount
            };
        }
    }
}
