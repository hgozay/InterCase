using System.Text.Json;
using InterCase.Common;
using InterCase.Common.Dtos.Paging;
using InterCase.UI.Helpers;
using InterCase.UI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace InterCase.UI.Controllers
{
    public sealed class OrdersController : BaseController
    {
        [HttpGet("")]
        [HttpGet("orders")]
        public async Task<IActionResult> Index(int ordersPage = 1, CancellationToken ct = default)
        {
            if (User.GetUserId() is not { } userId)
                return await ExpireSessionAsync();

            var createdOrder = TempData[UiTempDataKeys.CreatedOrder] is string json
                ? JsonSerializer.Deserialize<CreatedOrderNotice>(json)
                : null;

            var list = await LoadListAsync(userId, createdOrder, ordersPage, ct);
            if (list is null)
                return await ExpireSessionAsync();

            return View(new OrdersPageViewModel
            {
                Form = new OrderFormModel
                {
                    UserId = userId.ToString(),
                    ProductId = Guid.NewGuid().ToString(),
                    Quantity = 1,
                    PaymentMethod = PaymentMethod.CreditCard
                },
                List = list
            });
        }

        [HttpPost("orders")]
        public async Task<IActionResult> Create([Bind(Prefix = nameof(OrdersPageViewModel.Form))] OrderFormModel form, CancellationToken ct)
        {
            if (User.GetUserId() is not { } userId)
                return await ExpireSessionAsync();

            form.UserId = userId.ToString();
            var result = await ApiClient.CreateOrderAsync(form, ct);
            if (result.IsUnauthorized)
                return await ExpireSessionAsync();

            if (result.IsSuccess)
            {
                TempData[UiTempDataKeys.CreatedOrder] = JsonSerializer.Serialize(new CreatedOrderNotice(result.Value!.Id, result.CorrelationId));
                return RedirectToAction(nameof(Index), null, null, "orders");
            }

            ShowApiErrors(
                result.Problem,
                nameof(OrdersPageViewModel.Form),
                nameof(OrderFormModel.UserId),
                nameof(OrderFormModel.ProductId),
                nameof(OrderFormModel.Quantity),
                nameof(OrderFormModel.PaymentMethod));

            var list = await LoadListAsync(userId, null, 1, ct);
            if (list is null)
                return await ExpireSessionAsync();

            return View(nameof(Index), new OrdersPageViewModel
            {
                Form = form,
                List = list,
                FormError = DescribeFailure(result),
                FormCorrelationId = result.CorrelationId
            });
        }

        [HttpGet("orders/list")]
        public async Task<IActionResult> List(Guid? highlight, string? correlationId, int ordersPage = 1, CancellationToken ct = default)
        {
            var createdOrder = highlight is { } orderId ? new CreatedOrderNotice(orderId, correlationId) : null;
            var list = User.GetUserId() is { } userId ? await LoadListAsync(userId, createdOrder, ordersPage, ct) : null;
            if (list is null)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Unauthorized();
            }

            return PartialView("_OrderList", list);
        }

        private async Task<OrderListViewModel?> LoadListAsync(Guid userId, CreatedOrderNotice? createdOrder, int page, CancellationToken ct)
        {
            var result = await ApiClient.GetOrdersAsync(userId, ct);
            if (result.IsUnauthorized)
                return null;

            if (!result.IsSuccess)
            {
                return new OrderListViewModel
                {
                    UserId = userId.ToString(),
                    Error = DescribeError(result),
                    CorrelationId = result.CorrelationId
                };
            }

            var orders = result.Value!.OrderByDescending(o => o.CreatedAt).ToList();
            var totalPages = Math.Max(1, (int)Math.Ceiling(orders.Count / (double)PageRequest.DefaultPageSize));
            var currentPage = Math.Clamp(page, 1, totalPages);

            return new OrderListViewModel
            {
                UserId = userId.ToString(),
                Orders = orders.Skip((currentPage - 1) * PageRequest.DefaultPageSize).Take(PageRequest.DefaultPageSize).ToList(),
                Page = currentPage,
                TotalPages = totalPages,
                TotalCount = orders.Count,
                CreatedOrder = createdOrder
            };
        }
    }
}
