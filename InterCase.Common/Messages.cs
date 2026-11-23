namespace InterCase.Common
{
    public sealed record OrderPlacedMessage(
        long OrderId,
        Guid UserId,
        Guid ProductId,
        int Quantity,
        PaymentMethod PaymentMethod,
        DateTime CreatedAt,
        string CorrelationId);
}
