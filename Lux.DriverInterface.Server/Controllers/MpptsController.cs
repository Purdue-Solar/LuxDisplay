using Lux.DriverInterface.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Lux.DriverInterface.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MpptsController(MpptCollection mppts) : Controller
{
	protected MpptCollection Mppts { get; } = mppts;

	[HttpGet]
	public Mppt Get(int deviceId)
	{
		if (deviceId >= Mppts.Count)
			return null;
		return Mppts[(byte)deviceId];
	}
}
