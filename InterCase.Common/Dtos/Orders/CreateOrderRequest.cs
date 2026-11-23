namespace InterCase.Common.Dtos.Orders
{
    public sealed record CreateOrderRequest
    {
        public required string UserId { get; init; }
        public required string ProductId { get; init; }
        public required int Quantity { get; init; }
        public required PaymentMethod PaymentMethod { get; init; }
    }
}
