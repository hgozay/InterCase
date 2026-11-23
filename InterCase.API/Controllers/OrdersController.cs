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
        [ProducesResponseType<OrderResponse>(StatusCodes.Status202Accepted)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request, CancellationToken ct)
        {
            var order = await serviceManager.Order.CreateAsync(request, ct);
            return Accepted(order);
        }

        [HttpGet("{userId}")]
        [ProducesResponseType<IReadOnlyList<OrderResponse>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IReadOnlyList<OrderResponse>>> GetByUserId(
            [FromRoute, NotEmptyGuid(ErrorMessage = Localization.Keys.UserIdEmpty)] Guid userId,
            CancellationToken ct)
        {
            var orders = await serviceManager.Order.GetByUserIdAsync(userId, ct);
            return Ok(orders);
        }
    }
}
