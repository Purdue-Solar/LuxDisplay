using Lux.DriverInterface.Shared.CanPackets.WaveSculptor.Broadcast;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared;

public class CanDecoder(ILogger<CanDecoder> logger)
{
	protected ILogger Logger { get; } = logger;

	public delegate void DecodeCallback(IReadableCanPacket value);
	public delegate void DecodeCallback<T>(T value) where T : struct, IReadableCanPacket<T>;
	public delegate bool TryReadFunc(uint id, bool isExtended, ReadOnlySpan<byte> data, [NotNullWhen(true)] out IReadableCanPacket? packet);

	protected Dictionary<Type, (TryReadFunc TryRead, DecodeCallback Callback)> Decoders { get; } = [];

	public void AddPacketDecoder<T>(DecodeCallback<T> onDecodeFunction) where T : struct, IReadableCanPacket<T>
	{
		void Callback(IReadableCanPacket value) => onDecodeFunction((T)value);

		Decoders.Add(typeof(T), (T.TryRead, Callback));

		Logger.LogDebug("Added packet decoder for {name}", typeof(T).Name);
	}

	/// <summary>
	/// Attempts to remove a packet decoder
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public bool RemovePacketDecoder<T>() where T : IReadableCanPacket
	{
		return Decoders.Remove(typeof(T));
	}

	/// <summary>
	/// Attempts to decode a CAN packet and call the appropriate callback
	/// </summary>
	/// <param name="id">the 29 or 11 bit CAN id</param>
	/// <param name="isExtended">whether the id is 29 or 11 bit</param>
	/// <param name="data">the payload</param>
	/// <returns>Whether the packet was properly handled.</returns>
	public bool HandleCanPacket(uint id, bool isExtended, ReadOnlySpan<byte> data)
	{
		foreach (var decoder in Decoders)
		{
			if (decoder.Value.TryRead(id, isExtended, data, out IReadableCanPacket? packet))
			{
				decoder.Value.Callback(packet);
				return true;
			}
		}

		return false;
	}
}
