using Lux.DriverInterface.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Lux.DriverInterface.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EncoderController(Encoder encoder) : Controller
{
    protected Encoder Encoder { get; } = encoder;

    [HttpGet]
    public Encoder Get() => Encoder;

    //Used for testing the motor using a slider on the UI
    [HttpPost]
    public void Post([FromBody] float value)
    {
        Encoder.Percentage = value;
    }
}
