using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.WaveSculptor.Command;
public readonly struct Drive(float velocity, float currentPercent) : IWriteableCanPacket<Drive>
{
	public static uint CanId => WaveSculptorBase.CommandBaseId + (uint)CommandId.Drive;
	public uint Id => CanId;
	public static bool IsExtended => false;
	public static int Size => 8;

	///<summary>
	/// Desired motor velocity set point (RPM)
	///</summary>
	public float MotorVelocity { get; } = velocity;
	///<summary>
	///Desired motor current set point as a percentage of maximum current setting (0-100%)
	///</summary>
	public float MotorCurrent { get; } = currentPercent;

	public static bool IsValidId(uint id, bool extended) => !extended && id == CanId;

	public bool TryWrite(Span<byte> buffer, out int written)
	{
		written = 0;
		if (buffer.Length < Size)
			return false;

		written = Size;
		BinaryPrimitives.WriteSingleLittleEndian(buffer, MotorVelocity);
		BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(sizeof(float)), MotorCurrent);

		return true;
	}
}
