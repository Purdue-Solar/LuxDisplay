namespace Lux.DriverInterface.Shared;

public class Peripheral(byte deviceId)
{
	public string Name { get; set; } = string.Empty;
	public byte DeviceId { get; set; } = deviceId;
	public CanPackets.Peripherals.Outputs Outputs { get; set; }
}
