namespace InterCase.Common
{
    public sealed record OrderPlacedMessage(
        long OrderId,
        string UserId,
        string ProductId,
        int Quantity,
        PaymentMethod PaymentMethod,
        DateTime CreatedAt,
        string CorrelationId);
}
