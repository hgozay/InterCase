using InterCase.Common.Dtos.Notifications;
using InterCase.Common.Dtos.Orders;

namespace InterCase.UI.Models
{
    public sealed class PagedListViewModel<T>
    {
        public IReadOnlyList<T> Items { get; init; } = [];
        public int Page { get; init; } = 1;
        public int TotalPages { get; init; } = 1;
        public int TotalCount { get; init; }
        public string? Error { get; init; }
        public string? CorrelationId { get; init; }
    }

    public sealed class ActivityPageViewModel
    {
        public PagedListViewModel<NotificationResponse> Notifications { get; init; } = new();
        public PagedListViewModel<OrderLogResponse> Logs { get; init; } = new();
    }

    public sealed record PagerViewModel(int Page, int TotalPages, string Label, Func<int, string> UrlFor)
    {
        private const int Window = 2;

        public IEnumerable<int?> Slots()
        {
            var from = Math.Max(1, Page - Window);
            var to = Math.Min(TotalPages, Page + Window);

            if (from > 1)
            {
                yield return 1;
                if (from > 2)
                    yield return null;
            }

            for (var page = from; page <= to; page++)
                yield return page;

            if (to < TotalPages)
            {
                if (to < TotalPages - 1)
                    yield return null;
                yield return TotalPages;
            }
        }
    }
}
