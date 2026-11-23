using InterCase.Common.Dtos.Workers;
using Microsoft.AspNetCore.Mvc;

namespace InterCase.API.Controllers
{
    [Route("system")]
    public sealed class SystemController : BaseController
    {
        [HttpGet("worker")]
        public ActionResult<WorkerStatusResponse> GetWorkerStatus() =>
            Ok(ServiceManager.Worker.GetStatus());
    }
}
