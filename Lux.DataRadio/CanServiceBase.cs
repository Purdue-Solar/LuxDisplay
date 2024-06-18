using Lux.DriverInterface.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SocketCANSharp;
using SocketCANSharp.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DataRadio;

public interface ICanServiceBase
{
	public bool IsInitialized { get; }
	public void Init();

	/// <inheritdoc cref="SocketCANSharp.Network.RawCanSocket.Read(out CanFrame)"/>
	/// <exception cref="InvalidOperationException"></exception>
	public int Read(out CanFrame canFrame);
	/// <inheritdoc cref="SocketCANSharp.Network.RawCanSocket.Read(out CanFrame)"/>
	/// <exception cref="InvalidOperationException"></exception>
	public int Write(CanFrame frame);
}

public class UnixCanServiceBase(IConfiguration config, RadioService radio) : ICanServiceBase, IDisposable
{
	protected RawCanSocket RawCan { get; } = new RawCanSocket();
	protected RadioService Radio { get; } = radio;
	protected string CanInterfaceName { get; } = config.GetValue("CanService:Interface", "can0") ?? "can0";

	public bool IsInitialized { get; private set; } = false;

	public void Init()
	{
		if (!IsInitialized)
		{
			if (Environment.OSVersion.Platform != PlatformID.Unix)
				throw new PlatformNotSupportedException("This service is only supported on Unix platforms");

			Radio.Init();

			CanNetworkInterface can = CanNetworkInterface.GetAllInterfaces(true).First(iface => iface.Name.Equals(CanInterfaceName));
			RawCan.Bind(can);

			IsInitialized = true;
		}
	}

	/// <inheritdoc cref="SocketCANSharp.Network.RawCanSocket.Read(out CanFrame)"/>
	/// <exception cref="InvalidOperationException"></exception>
	public int Read(out CanFrame canFrame)
	{
		if (!IsInitialized)
			throw new InvalidOperationException("CanService is not initialized");

		int read = RawCan.Read(out CanFrame frame);

		_ = Task.Run(() => Radio.Write(frame));

		canFrame = frame;
		return read;
	}

	/// <inheritdoc cref="SocketCANSharp.Network.RawCanSocket.Write(CanFrame)"/>
	/// <exception cref="InvalidOperationException"></exception>
	public int Write(CanFrame frame)
	{
		if (!IsInitialized)
			throw new InvalidOperationException("CanService is not initialized");

		_ = Task.Run(() => Radio.Write(frame));

		return RawCan.Write(frame);
	}

	public virtual void Dispose()
	{
		RawCan?.Dispose();
		GC.SuppressFinalize(this);
	}
}

public class DummyCanServiceBase(IConfiguration config, RadioService radio, ILogger<ICanServiceBase> logger, IPacketQueue queue) : ICanServiceBase
{
	protected ILogger Logger { get; } = logger;
	protected RadioService Radio { get; } = radio;
	protected string CanInterfaceName { get; } = config.GetValue("CanService:Interface", "can0") ?? "can0";

	private IPacketQueue PacketQueue { get; } = queue;

	public bool IsInitialized { get; private set; } = false;

	public void Init()
	{
		if (!IsInitialized)
		{
			Radio.Init();
			IsInitialized = true;
		}
	}

	public int Read(out CanFrame canFrame)
	{
		if (!IsInitialized)
			throw new InvalidOperationException("CanService is not initialized");

		if (PacketQueue.TryDequeue(out CanFrame frame))
		{
			_ = Task.Run(() => Radio.Write(frame));

			canFrame = frame;
			return canFrame.Length;
		}

		canFrame = default;
		return 0;
	}

	public int Write(CanFrame frame)
	{
		if (!IsInitialized)
			throw new InvalidOperationException("CanService is not initialized");

		Logger.LogInformation("{interface}: {frame}", CanInterfaceName, frame);
		return frame.Length;
	}
}