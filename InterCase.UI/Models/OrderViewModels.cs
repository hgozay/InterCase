using InterCase.Common;
using InterCase.Common.Dtos.Orders;

namespace InterCase.UI.Models
{
    public sealed class OrderFormModel
    {
        public string? UserId { get; set; }
        public string? ProductId { get; set; }
        public int? Quantity { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
    }

    public sealed record CreatedOrderNotice(Guid OrderId, string? CorrelationId);

    public sealed class OrderListViewModel
    {
        public string? UserId { get; init; }
        public IReadOnlyList<OrderResponse> Orders { get; init; } = [];
        public string? Error { get; init; }
        public string? CorrelationId { get; init; }
        public CreatedOrderNotice? CreatedOrder { get; init; }
        public int Page { get; init; } = 1;
        public int TotalPages { get; init; } = 1;
        public int TotalCount { get; init; }

        public Guid? HighlightedOrderId => CreatedOrder?.OrderId;
        public OrderResponse? HighlightedOrder => Orders.FirstOrDefault(o => o.Id == HighlightedOrderId);
        public bool HasUser => !string.IsNullOrEmpty(UserId);
        public bool HasActiveOrders => Orders.Any(o => o.Status is OrderStatus.Pending or OrderStatus.Processing);
    }

    public sealed class OrdersPageViewModel
    {
        public OrderFormModel Form { get; init; } = new();
        public OrderListViewModel List { get; init; } = new();
        public string? FormError { get; init; }
        public string? FormCorrelationId { get; init; }
    }
}
