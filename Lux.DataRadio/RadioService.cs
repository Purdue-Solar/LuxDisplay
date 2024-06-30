using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Lux.DriverInterface.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SocketCANSharp;

namespace Lux.DataRadio;

public class RadioService(IConfiguration config, ILogger<RadioService> logger, IPacketQueue packetQueue) : IDisposable
{
	public bool OutputEnabled { get; } = config.GetValue($"{nameof(RadioService)}:{nameof(OutputEnabled)}", true);
	public string OutputDevice { get; } = config.GetValue($"{nameof(RadioService)}:{nameof(OutputDevice)}", "/dev/ttyS0") ?? "/dev/ttyS0";
	public bool InputEnabled { get; } = config.GetValue($"{nameof(RadioService)}:{nameof(InputEnabled)}", false);
	public string InputDevice { get; } = config.GetValue($"{nameof(RadioService)}:{nameof(InputDevice)}", "/dev/ttyS0") ?? "/dev/ttyS0";

	public int BaudRate { get; } = config.GetValue($"{nameof(RadioService)}:{nameof(BaudRate)}", 115200);
	public Parity Parity { get; } = config.GetValue($"{nameof(RadioService)}:{nameof(Parity)}", Parity.None);
	public int DataBits { get; } = config.GetValue($"{nameof(RadioService)}:{nameof(DataBits)}", 8);
	public StopBits StopBits { get; } = config.GetValue($"{nameof(RadioService)}:{nameof(StopBits)}", StopBits.One);

	private static readonly string DefaultPath = Environment.OSVersion.Platform == PlatformID.Unix ? "/logs/can" : @".\logs\can";
	private string LogFile { get; } = Path.Combine(config.GetValue($"{nameof(RadioService)}:{nameof(LogFile)}", DefaultPath) ?? DefaultPath, $"candump-{DateTime.UtcNow:O}.bin").Replace(':', '_');

	protected ILogger<RadioService> Logger { get; } = logger;
	protected IPacketQueue PacketQueue { get; } = packetQueue;

	protected SerialPort? OutputPort { get; private set; }
	protected SerialPort? InputPort { get; private set; }

	private FileInfo? LogFileInfo { get; set; }
	private ArrayPool<byte> CanPool { get; } = ArrayPool<byte>.Create(SerialPacket.Size, 128);
	private CancellationTokenSource FileThreadCancellation { get; } = new();

	private DateTime StartTime { get; } = DateTime.UtcNow.RoundDown(TimeSpan.FromMinutes(1));
	private ushort Timestamp
	{
		get
		{
			double min = (DateTime.UtcNow - StartTime).TotalMinutes;
			return (ushort)(60 * (min - Math.Floor(min)));
		}
	}

	private int PacketCount = 0;
	public const int SyncRate = 31;

	private const int ReadBufferSize = 3 * (SyncRate + 1) * SerialPacket.Size;
	private byte[] ReadBuffer { get; } = new byte[ReadBufferSize];

	private const int AlignedBufferSize = SyncRate * SerialPacket.Size;
	private byte[] AlignedPacketBuffer { get; } = new byte[AlignedBufferSize];
	
	private int ReceivedFrames { get; set; } = 0;
	private int ReadBufferOffset { get; set; } = 0;

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

		if (!TryOpenSerialPorts())
			Logger.LogWarning("Failed to open one or more serial ports");

		IsInitialized = true;
	}

	private bool TryOpenSerialPorts()
	{
		if (!(InputEnabled || OutputEnabled))
			return true;

		SerialPort? outputPort = OutputPort;
		if (OutputEnabled && !TryOpenPort(OutputDevice, ref outputPort))
		{
			Logger.LogWarning("Failed to open output serial port {portname}", OutputDevice);
			return false;
		}

		OutputPort = outputPort;

		if (InputEnabled && InputDevice == OutputDevice)
		{
			InputPort = OutputPort;
			return true;
		}
		
		SerialPort? inputPort = InputPort;
		if (InputEnabled && !TryOpenPort(InputDevice, ref inputPort))
		{
			Logger.LogWarning("Failed to open input serial port {portname}", InputDevice);
			return false;
		}

		InputPort = inputPort;
		if (InputPort is not null)
			InputPort.DataReceived += Read;

		return true;
	}

	private bool TryOpenPort(string deviceName, ref SerialPort? port)
	{
		if (port is not null && port.IsOpen)
			return true;

		try
		{
			port?.Dispose();

			port = new SerialPort(deviceName, BaudRate, Parity, DataBits, StopBits);
			port.Open();

			return true;
		}
		catch
		{
			port?.Dispose();
			port = null;
			return false;
		}
	}

	private void Read(object sender, SerialDataReceivedEventArgs e)
	{
		if (InputPort is null)
			return;

		Span<byte> readBuffer = MemoryMarshal.CreateSpan(ref ReadBuffer[0], ReadBufferSize);
		Span<byte> alignedBuffer = MemoryMarshal.CreateSpan(ref AlignedPacketBuffer[0], AlignedBufferSize);

		for (int val = InputPort.ReadByte(); val != -1 && InputPort.BytesToRead > 0; val = InputPort.ReadByte())
		{
			readBuffer[ReadBufferOffset++] = (byte)val;
			if (ReadBufferOffset >= ReadBufferSize)
				ReadBufferOffset = AlignedBufferSize;

			int startIndex = ReadBufferOffset - SerialPacket.Size;
			if (startIndex < 0)
				startIndex += ReadBufferSize;

			if (ScanForSync(readBuffer.Slice(startIndex)))
			{
				int copyStart = startIndex - AlignedBufferSize;
				if (copyStart < 0)
					continue;

				// Reset offset of read buffer by size of frame
				ReadBufferOffset = AlignedBufferSize;

				readBuffer.Slice(copyStart, AlignedBufferSize).CopyTo(alignedBuffer);
				readBuffer.Clear(); // Empty the read buffer

				QueuePackets(alignedBuffer);	// Queue new packets
			}
		}
	}

	private void QueuePackets(ReadOnlySpan<byte> alignedBuffer)
	{
		for (int i = 0; i < AlignedBufferSize; i += SerialPacket.Size)
		{
			if (!SerialPacket.TryRead(alignedBuffer.Slice(i, SerialPacket.Size), out SerialPacket packet))
				continue;

			if (!packet.ValidChecksum)
				continue;

			byte[] payload = packet.Data.ToArray();

			CanFrame frame = new CanFrame
			{
				Data = payload,
				CanId = packet.Id,
				Length = packet.Length
			};

			PacketQueue.Enqueue(frame);
		}
	}

	private static bool ScanForSync(ReadOnlySpan<byte> buffer)
	{
		if (buffer.Length < SerialPacket.Size)
			return false;

		return MemoryMarshal.CreateReadOnlySpan(in buffer[0], SerialPacket.Size).SequenceEqual(SerialPacket.SyncBytes);
	}

	public void Write(CanFrame canFrame)
	{
		if (!IsInitialized)
			throw new InvalidOperationException("RadioService is not initialized");

		try
		{
			byte[] buffer = CanPool.Rent(SerialPacket.Size);

			uint id = canFrame.CanId & 0x1FFFFFFF;
			bool isExtended = (canFrame.CanId & 0x80000000) != 0;

			SerialPacket packet = new SerialPacket(id, isExtended, Timestamp, canFrame.Length, canFrame.Data);

			packet.TryWrite(buffer);

			_fileBuffer.Enqueue(buffer);

			// Do nothing if output is disabled
			if (!OutputEnabled)
				return;

			if (OutputPort is not null && OutputPort.IsOpen)
				OutputPort.BaseStream.Write(buffer.AsSpan(0, SerialPacket.Size));

			if (++PacketCount == SyncRate)
			{
				if (OutputPort is null || !OutputPort.IsOpen)
					TryOpenSerialPorts();

				Span<byte> syncBuffer = stackalloc byte[SerialPacket.Size];
				PacketCount = 0;

				SerialPacket.SyncPacket.TryWrite(syncBuffer);
				if (OutputPort is not null && OutputPort.IsOpen)
					OutputPort.BaseStream.Write(syncBuffer);
			}
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "Failed to write CAN frame to serial port");
			OutputPort?.Dispose();
			OutputPort = null;
		}
	}

	private readonly ConcurrentQueue<byte[]> _fileBuffer = new ConcurrentQueue<byte[]>();
	private void FileLoggingThread()
	{
		FileStream fi = LogFileInfo!.Open(FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
		while (!FileThreadCancellation.IsCancellationRequested)
		{
			if (!_fileBuffer.IsEmpty)
			{
				while (_fileBuffer.TryDequeue(out byte[]? buffer))
				{
					// Align to 16-byte boundary
					if (fi.Position % SerialPacket.Size != 0)
						fi.Seek(2 * SerialPacket.Size - (fi.Position % SerialPacket.Size), SeekOrigin.Current);

					if (buffer is not null)
					{
						fi.Write(buffer.AsSpan(0, SerialPacket.Size));
						CanPool.Return(buffer);
					}
				}

				fi.Flush();
			}

			Thread.Sleep(10);
		}

		fi.Close();
		fi.Dispose();
	}

	public void Dispose()
	{
		FileThreadCancellation.Cancel();

		OutputPort?.Dispose();
		OutputPort = null;
		
		GC.SuppressFinalize(this);
	}

	private readonly ref struct SerialPacket(uint id, bool isExtended, ushort timestamp, byte length, ReadOnlySpan<byte> data)
	{
		public uint Id { get; private init; } = isExtended ? 0x8000000 | id : id;
		public bool IsExtended => (Id & 0x80000000) != 0;
		public ushort Timestamp { get; private init; } = timestamp;
		public byte Length { get; private init; } = length;
		public byte Checksum { get; private init; } = GetChecksum(id, isExtended, timestamp, length, data);

		public bool ValidChecksum => CalculatedSum() == 0;

		public static byte GetChecksum(uint id, bool isExtended, ushort timestamp, byte length, ReadOnlySpan<byte> data)
		{
			int sum = 0;

			if (isExtended)
				id |= 0x80000000;

			sum += (byte)id;
			sum += (byte)(id >> 8);
			sum += (byte)(id >> 16);
			sum += (byte)(id >> 24);
			
			sum += (byte)timestamp;
			sum += (byte)(timestamp >> 8);

			sum += length;

			foreach (byte b in data)
				sum += b;

			return (byte)~sum;
		}

		public byte CalculatedSum()
		{
			int sum = 0;

			sum += (byte)Id;
			sum += (byte)(Id >> 8);
			sum += (byte)(Id >> 16);
			sum += (byte)(Id >> 24);

			sum += (byte)Timestamp;
			sum += (byte)(Timestamp >> 8);

			sum += Checksum;

			sum += Length;

			foreach (byte b in Data)
				sum += b;

			return (byte)sum;
		}

		public ReadOnlySpan<byte> Data { get; private init; } = data;

		public const int Size = 16;
		
		// Sync Packet = FF FF FF FF FF FF FF FF 00 00 00 00 00 00 00 00

		private static readonly byte[] _syncBytes = [0, 0, 0, 0, 0, 0, 0, 0];

		public static SerialPacket SyncPacket => new SerialPacket { Id = 0xFFFFFFFF, Timestamp = 0xFFFF, Length = 0xFF, Checksum = 0xFF, Data = _syncBytes };

		internal static byte[] SyncBytes { get; } = [0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

		public static bool TryRead(ReadOnlySpan<byte> data, out SerialPacket packet)
		{
			if (data.Length < Size)
			{
				packet = default;
				return false;
			}

			// Hack to avoid range checks
			ReadOnlySpan<byte> a = MemoryMarshal.CreateReadOnlySpan(in data[0], Size);

			uint id = BinaryPrimitives.ReadUInt32LittleEndian(a);
			ushort timestamp = BinaryPrimitives.ReadUInt16LittleEndian(a.Slice(4));
			byte length = a[6];
			byte checksum = a[7];
			
			packet = new SerialPacket { Id = id, Timestamp = timestamp, Length = length, Checksum = checksum, Data = a.Slice(8, 8) };
			return true;
		}

		public bool TryWrite(Span<byte> buffer)
		{
			if (buffer.Length < Size)
				return false;

			// Hack to avoid range checks
			Span<byte> a = MemoryMarshal.CreateSpan(ref buffer[0], Size);

			BinaryPrimitives.WriteUInt32LittleEndian(a, Id);
			BinaryPrimitives.WriteUInt16LittleEndian(a.Slice(4), Timestamp);
			a[6] = Length;
			a[7] = Checksum;

			for (int i = 8; i < Size; i++)
			{
				int j = i - 8;
				a[i] = j < Data.Length ? Data[j] : (byte)0;
			}

			return true;
		}
	}
}
