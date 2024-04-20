using Lux.DriverInterface.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Lux.DriverInterface.Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TelemetryController(Telemetry telemetry) : Controller
	{
		private readonly Telemetry _telemetry = telemetry;

		[HttpGet]
		public ActionResult<Telemetry> Get()
		{

			return _telemetry;
		}
	}
}
