using Microsoft.AspNetCore.Mvc;

namespace SportsCenter.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ActivityTypesController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new List<ActivityType>());
    }
}
