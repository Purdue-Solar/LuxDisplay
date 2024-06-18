using Lux.DriverInterface.Shared;
using Microsoft.Extensions.Hosting;
using SocketCANSharp;

//using SocketCANSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DataRadio;

public class CanSendService(ICanServiceBase serviceBase) : IDisposable
{
	protected ICanServiceBase ServiceBase { get; } = serviceBase;

	public bool IsInitialized { get; private set; } = false;

	public void Init()
	{
		ServiceBase.Init();
	}

	/// <summary>
	/// Send a packet over the CAN bus
	/// </summary>
	/// <param name="id">the 29 or 11 bit CAN id</param>
	/// <param name="isExtended">whether the id is extended or not</param>
	/// <param name="length">the number of bytes to send</param>
	/// <param name="data">the buffer containing the data to send</param>
	/// <returns cref="int">the number of bytes sent</returns>
	/// <exception cref="ArgumentOutOfRangeException">The length specified is less than 0 or greater than 8</exception>
	/// <exception cref="ArgumentException">The buffer length is less than the specified length</exception>
	public int SendPacket(uint id, bool isExtended, int length, ReadOnlySpan<byte> data)
	{
		if (length < 0 || length > 8)
			throw new ArgumentOutOfRangeException(nameof(length), "Length must be between 0 and 8");
		if (data.Length < length)
			throw new ArgumentException("Data length must bet at least specified length", nameof(data));

		byte[] buffer = data.Slice(0, Math.Min(data.Length, length)).ToArray();

		if (isExtended)
			id = (id & ((1u << 29) - 1)) | (1u << 31);
		else
			id &= 0x7FF;

		CanFrame frame = new CanFrame(id, buffer);

		return ServiceBase.Write(frame);
	}

	public int SendPacket<T>(T packet) where T : struct, IWriteableCanPacket<T>
	{
		Span<byte> buffer = stackalloc byte[8];
		if (!packet.TryWrite(buffer, out _))
			throw new Exception("Failed to write packet");

        return SendPacket(packet.Id, T.IsExtended, T.Size, buffer);
    }

	public virtual void Dispose()
	{
		if (ServiceBase is IDisposable disposable)
			disposable.Dispose();
		GC.SuppressFinalize(this);
	}
}
