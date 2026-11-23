using System.Text.Json;
using InterCase.Common;
using InterCase.UI.Clients;
using InterCase.UI.Helpers;
using InterCase.UI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterCase.UI.Controllers
{
    [Authorize]
    public sealed class OrdersController(ApiClient apiClient) : Controller
    {
        [HttpGet("")]
        [HttpGet("orders")]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            if (User.GetUserId() is not { } userId)
                return await ExpireSessionAsync();

            var createdOrder = TempData[UiTempDataKeys.CreatedOrder] is string json
                ? JsonSerializer.Deserialize<CreatedOrderNotice>(json)
                : null;

            var list = await LoadListAsync(userId, createdOrder?.OrderId, ct);
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
                List = list,
                CreatedOrder = createdOrder
            });
        }

        [HttpPost("orders")]
        public async Task<IActionResult> Create([Bind(Prefix = nameof(OrdersPageViewModel.Form))] OrderFormModel form, CancellationToken ct)
        {
            if (User.GetUserId() is not { } userId)
                return await ExpireSessionAsync();

            form.UserId = userId.ToString();
            var result = await apiClient.CreateOrderAsync(form, ct);
            if (result.IsUnauthorized)
                return await ExpireSessionAsync();

            if (result.IsSuccess)
            {
                TempData[UiTempDataKeys.CreatedOrder] = JsonSerializer.Serialize(new CreatedOrderNotice(result.Value!.Id, result.CorrelationId));
                return RedirectToAction(nameof(Index), null, null, "orders");
            }

            ModelState.Clear();
            ModelState.AddApiErrors(
                result.Problem,
                nameof(OrdersPageViewModel.Form),
                nameof(OrderFormModel.UserId),
                nameof(OrderFormModel.ProductId),
                nameof(OrderFormModel.Quantity),
                nameof(OrderFormModel.PaymentMethod));

            var list = await LoadListAsync(userId, null, ct);
            if (list is null)
                return await ExpireSessionAsync();

            return View(nameof(Index), new OrdersPageViewModel
            {
                Form = form,
                List = list,
                FormError = result.IsUnavailable
                    ? UiText.ApiUnavailable
                    : result.Problem?.Title ?? result.Problem?.Detail ?? UiText.UnexpectedApiResponse,
                FormCorrelationId = result.CorrelationId
            });
        }

        [HttpGet("orders/list")]
        public async Task<IActionResult> List(Guid? highlight, CancellationToken ct)
        {
            var list = User.GetUserId() is { } userId ? await LoadListAsync(userId, highlight, ct) : null;
            if (list is null)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Unauthorized();
            }

            return PartialView("_OrderList", list);
        }

        private async Task<OrderListViewModel?> LoadListAsync(Guid userId, Guid? highlightedOrderId, CancellationToken ct)
        {
            var result = await apiClient.GetOrdersAsync(userId, ct);
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

            return new OrderListViewModel
            {
                UserId = userId.ToString(),
                Orders = result.Value!.OrderByDescending(o => o.CreatedAt).ToList(),
                HighlightedOrderId = highlightedOrderId
            };
        }

        private async Task<IActionResult> ExpireSessionAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(AccountController.Login), "Account", new { expired = true });
        }

        private static string DescribeError<T>(ApiResult<T> result)
        {
            if (result.IsUnavailable)
                return UiText.ApiUnavailable;

            var fieldMessage = result.Problem?.Errors?.Values.SelectMany(m => m).FirstOrDefault();
            return fieldMessage ?? result.Problem?.Detail ?? result.Problem?.Title ?? UiText.UnexpectedApiResponse;
        }
    }
}
