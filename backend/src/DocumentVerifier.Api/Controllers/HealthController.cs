using Microsoft.AspNetCore.Mvc;

namespace DocumentVerifier.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult<object> Get()
    {
        return Ok(new
        {
            status = "ok",
            service = "DocumentVerifier.Api",
            time = DateTimeOffset.UtcNow
        });
    }
}
