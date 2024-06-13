using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared;
public interface ICanPacket
{
	uint Id { get; }
	static abstract bool IsExtended { get; }
	static abstract int Size { get; }

	static abstract bool IsValidId(uint id, bool extended);
}

public interface ICanPacket<T> where T : struct, ICanPacket<T>
{
}

public interface IReadableCanPacket : ICanPacket
{
	/// <summary>
	/// Try to read a CAN packet from a span of bytes
	/// </summary>
	/// <param name="id">the CAN ID</param>
	/// <param name="extended">whether the CAN ID is extended</param>
	/// <param name="data">the buffer to read from</param>
	/// <param name="packet">the packet that was read</param>
	/// <returns>whether the packet was successfully read</returns>
	static abstract bool TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out IReadableCanPacket packet);
}
public interface IReadableCanPacket<T> : IReadableCanPacket, ICanPacket<T> where T : struct, IReadableCanPacket<T>
{
	/// <summary>
	/// Try to read a CAN packet from a span of bytes
	/// </summary>
	/// <param name="id">the CAN ID</param>
	/// <param name="extended">whether the CAN ID is extended</param>
	/// <param name="data">the buffer to read from</param>
	/// <param name="packet">the packet that was read</param>
	/// <returns>whether the packet was successfully read</returns>
	static abstract bool TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out T packet);
}

public interface IWriteableCanPacket : ICanPacket
{
	/// <summary>
	/// Try to write a can packet to a span of bytes
	/// </summary>
	/// <param name="data">The buffer to write to</param>
	/// <param name="written">The number of bytes written</param>
	/// <returns>whether the packet was successfully written</returns>
	bool TryWrite(Span<byte> data, out int written);
}

public interface IWriteableCanPacket<T> : IWriteableCanPacket where T : struct, IWriteableCanPacket<T>
{
	/// <summary>
	/// Try to write a can packet to a span of bytes
	/// </summary>
	/// <param name="data">The buffer to write to</param>
	/// <param name="written">The number of bytes written</param>
	/// <returns>whether the packet was successfully written</returns>
	new bool TryWrite(Span<byte> data, out int written);
}
