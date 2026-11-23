using InterCase.API.Helpers;
using InterCase.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterCase.API.Controllers
{
    [ApiController]
    [Authorize]
    public abstract class BaseController : ControllerBase
    {
        private IServiceManager? _serviceManager;

        protected IServiceManager ServiceManager =>
            _serviceManager ??= HttpContext.RequestServices.GetRequiredService<IServiceManager>();

        protected ActionResult? DenyIfNotOwner(Guid userId) =>
            User.Owns(userId) ? null : Forbid();

        protected ActionResult? DenyIfNotFoundOrNotOwner<TResource>(TResource? resource, Func<TResource, Guid> ownerOf)
            where TResource : class =>
            resource is null ? NotFound() : DenyIfNotOwner(ownerOf(resource));
    }
}
