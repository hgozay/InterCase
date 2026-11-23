using InterCase.UI.Clients;
using InterCase.UI.Models;
using Microsoft.AspNetCore.Mvc;

namespace InterCase.UI.ViewComponents
{
    public sealed class WorkerStatusViewComponent(ApiClient apiClient) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var result = await apiClient.GetWorkerStatusAsync(HttpContext.RequestAborted);
            return View(new WorkerStatusViewModel(result.IsSuccess ? result.Value : null));
        }
    }
}
