using Lux.DriverInterface.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Lux.DriverInterface.Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class EncoderController(Encoder amt) : Controller
	{
		private readonly Encoder _amt = amt;

		[HttpGet]
		public Encoder Get()
		{
			return _amt;
		}
	}
}
