namespace InterCase.Common.Dtos.Orders
{
    // Id = entity'nin PublicId'si; long Id API'ye çıkmaz
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
