namespace InterCase.Common.Dtos.Orders
{
    public sealed record OrderResponse(
        Guid Id,
        string UserId,
        string ProductId,
        int Quantity,
        PaymentMethod PaymentMethod,
        OrderStatus Status,
        DateTime CreatedAt,
        DateTime? ProcessedAt);
}
