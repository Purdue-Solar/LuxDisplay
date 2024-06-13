using Lux.DriverInterface.Shared.CanPackets.Wavescupltor.Broadcast;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared;
public class CanDecoder
{
	public delegate void DecodeCallback(IReadableCanPacket value);
	public delegate void DecodeCallback<T>(T value);
	public delegate bool TryReadFunc(uint id, bool isExtended, ReadOnlySpan<byte> data, out IReadableCanPacket packet);

	protected Dictionary<Type, (TryReadFunc TryRead, DecodeCallback Callback)> Decoders { get; } = new Dictionary<Type, (TryReadFunc, DecodeCallback)>();

	public void AddPacketDecoder<T>(DecodeCallback<T> onDecodeFunction) where T : struct, IReadableCanPacket<T>
	{
		DecodeCallback callback = (IReadableCanPacket value) => onDecodeFunction((T)value);
		Decoders.Add(typeof(T), (T.TryRead, callback));
	}

	static void HandleStatus(CanPackets.Peripherals.Status status)
	{
		Console.WriteLine(status);
	}

	public static void Test()
	{ 		
		CanDecoder decoder = new CanDecoder();
		decoder.AddPacketDecoder<CanPackets.Peripherals.Status>(HandleStatus);

		decoder.AddPacketDecoder((BackEmf backEmf) => Console.WriteLine(backEmf));
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
			if (decoder.Value.TryRead(id, isExtended, data, out IReadableCanPacket packet))
			{
				decoder.Value.Callback(packet);
				return true;
			}
		}

		return false;
	}
}

public static class CanDecoderExtensions
{
	public static IServiceCollection AddCanDecoder(this IServiceCollection services)
	{
		return services;
	}
}
