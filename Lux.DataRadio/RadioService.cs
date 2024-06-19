using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Lux.DriverInterface.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SocketCANSharp;

namespace Lux.DataRadio;

public class RadioService(IConfiguration config, ILogger<RadioService> logger) : IDisposable
{
	public string Device { get; } = config.GetValue($"{nameof(RadioService)}:{nameof(Device)}", "/dev/ttyS0") ?? "/dev/ttyS0";
	public int BaudRate { get; } = config.GetValue($"{nameof(RadioService)}:{nameof(BaudRate)}", 115200);
	public Parity Parity { get; } = config.GetValue($"{nameof(RadioService)}:{nameof(Parity)}", Parity.None);
	public int DataBits { get; } = config.GetValue($"{nameof(RadioService)}:{nameof(DataBits)}", 8);
	public StopBits StopBits { get; } = config.GetValue($"{nameof(RadioService)}:{nameof(StopBits)}", StopBits.One);

	protected IConfiguration Configuration { get; } = config;
	protected ILogger<RadioService> Logger { get; } = logger;
	protected SerialPort? SerialPort { get; private set; }

	private DateTime StartTime { get; } = DateTime.UtcNow;
	private static readonly string DefaultPath = Environment.OSVersion.Platform == PlatformID.Unix ? "/logs/can" : @".\logs\can";
	private string LogFile { get; } = Path.Combine(config.GetValue($"{nameof(RadioService)}:{nameof(LogFile)}", DefaultPath) ?? DefaultPath, $"candump-{DateTime.UtcNow:O}.bin").Replace(':', '_');
	private FileInfo? LogFileInfo { get; set; }
	private ArrayPool<byte> CanPool { get; } = ArrayPool<byte>.Create(SerialPacket.Size, 128);
    private CancellationTokenSource FileThreadCancellation { get; } = new();

	private ushort Timestamp => (ushort)(DateTime.UtcNow - StartTime).TotalMilliseconds;
	private int PacketCount = 0;

	public const int SyncRate = 31;

	public bool IsInitialized { get; private set; } = false;

	public void Init()
	{
		if (IsInitialized)
			return;
		
		LogFileInfo = new FileInfo(LogFile);
		if (!LogFileInfo.Directory?.Exists ?? false)
            LogFileInfo.Directory!.Create();

		Logger.LogInformation("Logging CAN frames to {LogFile}", LogFileInfo.FullName);

		new Thread(FileLoggingThread).Start();

        if (!TryOpenSerial())
			Logger.LogWarning("Failed to open serial port {portname}", Device);

        IsInitialized = true;
	}

	private bool TryOpenSerial()
	{
		if (SerialPort is not null)
            return true;

		try
		{
			SerialPort = new SerialPort(Device, BaudRate, Parity, DataBits, StopBits);
			SerialPort.Open();
			
			return true;
		}
		catch
		{
			SerialPort?.Dispose();
			SerialPort = null;
			return false;
		}
    }

	public void Write(CanFrame canFrame)
	{
		if (!IsInitialized || SerialPort is null)
			throw new InvalidOperationException("RadioService is not initialized");

		byte[] buffer = CanPool.Rent(SerialPacket.Size);
		
		uint id = canFrame.CanId & 0x1FFFFFFF;
		
		ushort timestamp = Timestamp;
		if (timestamp == 0xFFFF)
			timestamp = 0xFFFE;

		byte isExtended = (byte)((canFrame.CanId & 0x80000000) != 0 ? 1 : 0);
		SerialPacket packet = new SerialPacket(id, timestamp, isExtended, canFrame.Length, canFrame.Data);
		
		packet.TryWrite(buffer);

		SerialPort?.BaseStream.Write(buffer.AsSpan(0, SerialPacket.Size));
		_fileBuffer.Enqueue(buffer);

		if (++PacketCount == SyncRate)
		{
			if (SerialPort is null)
				TryOpenSerial();

			Span<byte> syncBuffer = stackalloc byte[SerialPacket.Size];
			PacketCount = 0;
			
			SerialPacket.SyncPacket.TryWrite(syncBuffer);
			SerialPort?.BaseStream.Write(syncBuffer);
        }
	}

	private readonly ConcurrentQueue<byte[]> _fileBuffer = new ConcurrentQueue<byte[]>();
	private void FileLoggingThread()
	{
        while (!FileThreadCancellation.IsCancellationRequested)
        {
			if (!_fileBuffer.IsEmpty)
			{
				FileStream fi = LogFileInfo!.Open(FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
				while (_fileBuffer.TryDequeue(out byte[]? buffer))
				{
					if (fi.Position % SerialPacket.Size != 0)
						fi.Seek(2 * SerialPacket.Size - (fi.Position % SerialPacket.Size), SeekOrigin.Current);

					if (buffer is not null)
					{
						fi.Write(buffer.AsSpan(0, SerialPacket.Size));
						CanPool.Return(buffer);
					}
                }

				fi.Flush();
				fi.Close();
				fi.Dispose();
			}

			Thread.Sleep(10);
        }
    }

	public void Dispose()
	{
		SerialPort?.Dispose();
		SerialPort = null;
		
		GC.SuppressFinalize(this);
	}

	private readonly ref struct SerialPacket(uint id, ushort timestamp, byte isExtended, byte length, ReadOnlySpan<byte> data)
	{
		public uint Id { get; } = id;
		public ushort Timestamp { get; } = timestamp;
		public byte IsExtended { get; } = isExtended;
		public byte Length { get; } = length;
		public ReadOnlySpan<byte> Data { get; } = data;

		public const int Size = 16;

		private static readonly byte[] _syncBytes = [0, 0, 0, 0, 0, 0, 0, 0];

		public static SerialPacket SyncPacket => new SerialPacket(uint.MaxValue, ushort.MaxValue, byte.MaxValue, byte.MaxValue, _syncBytes);

		public bool TryWrite(Span<byte> buffer)
		{
			if (buffer.Length < Size)
				return false;

			BinaryPrimitives.WriteUInt32LittleEndian(buffer, Id);
			BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(4), Timestamp);
			buffer[6] = IsExtended;
			buffer[7] = Length;
			Data.CopyTo(buffer.Slice(8));

			return true;
		}
	}
}
