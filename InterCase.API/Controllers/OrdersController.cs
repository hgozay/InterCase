using InterCase.Common.Dtos.Orders;
using InterCase.Common.Dtos.Paging;
using InterCase.Common.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace InterCase.API.Controllers
{
    [Route("orders")]
    public sealed class OrdersController : BaseController
    {
        [HttpPost]
        public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request, CancellationToken ct) =>
            DenyIfNotOwner(request.UserId!.Value) ?? Accepted(await ServiceManager.Order.CreateAsync(request, ct));

        [HttpGet("{userId}")]
        public async Task<ActionResult<IReadOnlyList<OrderResponse>>> GetByUserId(
            [FromRoute, NotEmptyGuid(ErrorMessage = Localization.Keys.UserIdEmpty)] Guid userId,
            CancellationToken ct) =>
            DenyIfNotOwner(userId) ?? Ok(await ServiceManager.Order.GetByUserIdAsync(userId, ct));

        [HttpGet("{userId}/logs")]
        public async Task<ActionResult<PagedResponse<OrderLogResponse>>> GetLogsByUserId(
            [FromRoute, NotEmptyGuid(ErrorMessage = Localization.Keys.UserIdEmpty)] Guid userId,
            [FromQuery] PageRequest paging,
            CancellationToken ct) =>
            DenyIfNotOwner(userId) ?? Ok(await ServiceManager.Order.GetLogsByUserIdAsync(userId, paging, ct));

        [HttpGet("by-id/{id}")]
        public async Task<ActionResult<OrderResponse>> GetById(
            [FromRoute, NotEmptyGuid(ErrorMessage = Localization.Keys.OrderIdEmpty)] Guid id,
            CancellationToken ct)
        {
            var order = await ServiceManager.Order.GetByPublicIdAsync(id, ct);
            return DenyIfNotFoundOrNotOwner(order, o => o.UserId) ?? Ok(order);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<OrderResponse>> Update(
            [FromRoute, NotEmptyGuid(ErrorMessage = Localization.Keys.OrderIdEmpty)] Guid id,
            UpdateOrderRequest request,
            CancellationToken ct)
        {
            var existing = await ServiceManager.Order.GetByPublicIdAsync(id, ct);
            return DenyIfNotFoundOrNotOwner(existing, o => o.UserId) ?? Ok(await ServiceManager.Order.UpdateAsync(id, request, ct));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(
            [FromRoute, NotEmptyGuid(ErrorMessage = Localization.Keys.OrderIdEmpty)] Guid id,
            CancellationToken ct)
        {
            var existing = await ServiceManager.Order.GetByPublicIdAsync(id, ct);
            if (DenyIfNotFoundOrNotOwner(existing, o => o.UserId) is { } denied)
                return denied;

            await ServiceManager.Order.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}
