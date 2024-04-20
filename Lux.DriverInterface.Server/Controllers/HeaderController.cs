using Lux.DriverInterface.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Lux.DriverInterface.Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class HeaderController(Header header) : Controller
	{
		private readonly Header _header = header;

		[HttpGet]
		public ActionResult<Header> Get()
		{

			return _header;
		}
	}
}
