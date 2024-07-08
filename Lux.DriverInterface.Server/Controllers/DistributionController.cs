using Lux.DriverInterface.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Lux.DriverInterface.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DistributionController(Distribution distribution) : Controller
{
    protected Distribution Distribution { get; } = distribution;

    [HttpGet]
    public Distribution Get() => Distribution;
}
