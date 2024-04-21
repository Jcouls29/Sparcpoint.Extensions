using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sparcpoint.Extensions.Multitenancy.Tests.Web.Controllers
{
    [ApiController]
    public class MultitenantController : ControllerBase
    {
        [HttpGet("/tenant")]
        public string GetNone([FromServices] TenantSpecificService service)
        {
            return string.Empty;
        }

        [HttpGet("/tenant/{accountId}")]
        public string Get([FromRoute] string accountId, [FromServices] TenantSpecificService service)
        {
            return service.Tenant?.AccountId ?? string.Empty;
        }
    }
}
