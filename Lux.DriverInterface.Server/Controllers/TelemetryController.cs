using Lux.DriverInterface.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Lux.DriverInterface.Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TelemetryController(Telemetry telemetry) : Controller
	{
		protected Telemetry Telemetry { get; } = telemetry;

		[HttpGet]
		public Telemetry Get() => Telemetry;
	}
}
