using Microsoft.AspNetCore.Mvc;

namespace InterCase.UI.Controllers
{
    public sealed class SystemController : BaseController
    {
        [HttpGet("system/worker-status")]
        public IActionResult WorkerStatus() => ViewComponent("WorkerStatus");
    }
}
