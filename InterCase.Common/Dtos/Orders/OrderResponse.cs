namespace InterCase.Common.Dtos.Orders
{
    public sealed record OrderResponse(
        Guid Id,
        Guid UserId,
        Guid ProductId,
        int Quantity,
        PaymentMethod PaymentMethod,
        OrderStatus Status,
        DateTime CreatedAt,
        DateTime? ProcessedAt);
}
