using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared;
public enum WarningType
{
	Warning,
	Critical
}

public readonly record struct Warning(WarningType WarningType, string Message);

public interface IWarningGenerator
{
	List<Warning> GetWarnings();
}
