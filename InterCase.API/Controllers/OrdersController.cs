using InterCase.API.Helpers;
using InterCase.Common.Dtos.Orders;
using InterCase.Common.Helpers;
using InterCase.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterCase.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("orders")]
    public sealed class OrdersController(IServiceManager serviceManager) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request, CancellationToken ct)
        {
            if (!User.Owns(request.UserId!.Value))
                return Forbid();

            var order = await serviceManager.Order.CreateAsync(request, ct);
            return Accepted(order);
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<IReadOnlyList<OrderResponse>>> GetByUserId(
            [FromRoute, NotEmptyGuid(ErrorMessage = Localization.Keys.UserIdEmpty)] Guid userId,
            CancellationToken ct)
        {
            if (!User.Owns(userId))
                return Forbid();

            var orders = await serviceManager.Order.GetByUserIdAsync(userId, ct);
            return Ok(orders);
        }

        [HttpGet("by-id/{id}")]
        public async Task<ActionResult<OrderResponse>> GetById(
            [FromRoute, NotEmptyGuid(ErrorMessage = Localization.Keys.OrderIdEmpty)] Guid id,
            CancellationToken ct)
        {
            var order = await serviceManager.Order.GetByPublicIdAsync(id, ct);
            if (order is null)
                return NotFound();

            return User.Owns(order.UserId) ? Ok(order) : Forbid();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<OrderResponse>> Update(
            [FromRoute, NotEmptyGuid(ErrorMessage = Localization.Keys.OrderIdEmpty)] Guid id,
            UpdateOrderRequest request,
            CancellationToken ct)
        {
            var existing = await serviceManager.Order.GetByPublicIdAsync(id, ct);
            if (existing is null)
                return NotFound();

            if (!User.Owns(existing.UserId))
                return Forbid();

            var order = await serviceManager.Order.UpdateAsync(id, request, ct);
            return Ok(order);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(
            [FromRoute, NotEmptyGuid(ErrorMessage = Localization.Keys.OrderIdEmpty)] Guid id,
            CancellationToken ct)
        {
            var existing = await serviceManager.Order.GetByPublicIdAsync(id, ct);
            if (existing is null)
                return NotFound();

            if (!User.Owns(existing.UserId))
                return Forbid();

            await serviceManager.Order.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}
