using Lux.DriverInterface.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Lux.DriverInterface.Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class WaveSculptorCriticalController(WaveSculptor ws) : Controller
	{
		private readonly WaveSculptor _ws = ws;

		[HttpGet]
		public WaveSculptor.WaveSculptorCritical Get()
		{
			return _ws.Critical;
		}
	}

	[Route("api/[controller]")]
	[ApiController]
	public class WaveSculptorController(WaveSculptor ws) : Controller
	{
		private readonly WaveSculptor _ws = ws;

		[HttpGet]
		public WaveSculptor Get()
		{
			return _ws;
		}
    }
}
