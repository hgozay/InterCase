namespace InterCase.Common.Dtos.Orders
{
    public sealed record OrderLogResponse(
        Guid OrderId,
        OrderStatus Status,
        DateTime CreatedAt,
        DateTime? ProcessedAt,
        string? Log);
}
