using InterCase.UI.Helpers;
using InterCase.UI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace InterCase.UI.Controllers
{
    public sealed class ToolsController(IOptions<RabbitMqPanelOptions> rabbitMqPanelOptions) : BaseController
    {
        [HttpGet("tools/rabbitmq")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult RabbitMq()
        {
            var options = rabbitMqPanelOptions.Value;
            if (!options.IsConfigured)
                return NotFound();

            var username = User.FindFirst(UiClaimTypes.Username)?.Value ?? User.Identity?.Name ?? "intercase-ui";
            var token = RabbitMqTokenHelper.CreateToken(options, username, DateTime.UtcNow);

            return View(new RabbitMqLoginViewModel($"{options.Url!.TrimEnd('/')}/login", token));
        }
    }
}
