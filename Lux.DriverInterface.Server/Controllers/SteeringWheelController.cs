using Lux.DriverInterface.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Lux.DriverInterface.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SteeringWheelController(SteeringWheel steering) : Controller
{
    protected SteeringWheel SteeringWheel { get; } = steering;

    [HttpGet]
    public SteeringWheel Get() => SteeringWheel;
}
