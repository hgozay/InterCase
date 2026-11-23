namespace InterCase.Common
{
    public static class CacheKeys
    {
        public static string OrdersByUser(Guid userId) => $"orders:user:{userId}";
        public static string OrderLog(long orderId) => $"order:{orderId}:log";
    }

    public static class QueueNames
    {
        public const string OrderPlaced = "order-placed";
    }

    public static class AppHeaderNames
    {
        public const string CorrelationId = "X-Correlation-ID";
    }

    public static class JobRetryAttempts
    {
        public const int OrderProcessing = 3;
    }

    public static class RecurringJobIds
    {
        public const string RepublishStaleOrders = "republish-stale-orders";
    }

    public static class NotificationChannels
    {
        public const string Console = "Console";
    }
}
