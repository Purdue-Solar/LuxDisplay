using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared
{
	public class Shared
	{
		public static string ApiUrl_motor { get; set; } = "http://localhost:8080/api/motor";
		public static string ApiUrl_header { get; set; } = "http://localhost:8080/api/header";

		public struct Data
		{
			public Data(Header _Header)
			{
				Header = _Header;
			}
			public Header Header { get; init; }
		}
	}
}
