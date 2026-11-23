using InterCase.Common.Dtos.Orders;

namespace InterCase.Service.Interfaces
{
    public interface IOrderService : IBaseService<OrderResponse, CreateOrderRequest>
    {
        Task<IReadOnlyList<OrderResponse>> GetByUserIdAsync(string userId, CancellationToken ct = default);
    }
}
