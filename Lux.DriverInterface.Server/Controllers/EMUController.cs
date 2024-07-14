//using Lux.DriverInterface.Shared;
//using Microsoft.AspNetCore.Mvc;

//namespace Lux.DriverInterface.Server.Controllers
//{
//	[Route("api/[controller]")]
//	[ApiController]
//	public class EMUCriticalController(EMU emu) : Controller
//	{
//		private readonly EMU _emu = emu;

//		[HttpGet]
//		public EMU.EMUCritical Get()
//		{
//			return _emu.Critical;
//		}
//	}

//	[Route("api/[controller]")]
//	[ApiController]
//	public class EMUController(EMU emu) : Controller
//	{
//		private readonly EMU _emu = emu;

//		[HttpGet]
//		public EMU Get()
//		{
//			return _emu;
//		}
//	}
//}
