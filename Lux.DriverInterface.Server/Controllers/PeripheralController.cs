using Lux.DriverInterface.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Lux.DriverInterface.Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PeripheralController(Peripheral peripheral) : Controller
	{
		private readonly Peripheral _peripheral = peripheral;

		[HttpGet]
		public ActionResult<Peripheral> Get()
		{

			return _peripheral;
		}
	}
}
