using InterCase.Common.Dtos.Orders;
using InterCase.Common.Dtos.Paging;

namespace InterCase.Service.Interfaces
{
    public interface IOrderService : IBaseService<OrderResponse, CreateOrderRequest, UpdateOrderRequest>
    {
        Task<IReadOnlyList<OrderResponse>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task<PagedResponse<OrderLogResponse>> GetLogsByUserIdAsync(Guid userId, PageRequest paging, CancellationToken ct = default);
        Task ProcessAsync(long orderId, CancellationToken ct = default);
        Task MarkFailedAsync(long orderId, CancellationToken ct = default);
        Task<int> RepublishStalePendingAsync(CancellationToken ct = default);
    }
}
