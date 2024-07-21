using Lux.DriverInterface.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Lux.DriverInterface.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BatteryController(Battery battery) : Controller
{
    protected Battery Battery { get; } = battery;

    [HttpGet]
    public Battery Get() => Battery;
}
