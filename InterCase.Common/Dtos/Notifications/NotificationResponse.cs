namespace InterCase.Common.Dtos.Notifications
{
    public sealed record NotificationResponse(
        Guid Id,
        Guid OrderId,
        string Channel,
        string Message,
        DateTime SentAt);
}
