using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared;
public readonly struct PsrCanId(byte dst, byte src, byte messageId, CanIds.DeviceType deviceType, CanIds.MessagePriority priority) : 
	IEquatable<PsrCanId>, IBitwiseOperators<PsrCanId, PsrCanId, PsrCanId>
{
	public byte Destination { get; } = dst;
	public byte Source { get; } = src;
	public byte MessageId { get; } = messageId;
	public CanIds.DeviceType DeviceType { get; } = deviceType;
	public CanIds.MessagePriority Priority { get; } = priority;

	public const byte MulticastDestination = 0xFF;

	public static PsrCanId DestinationMask => new PsrCanId(DestinationBitMask, 0, 0, 0, 0);
	public static PsrCanId SourceMask => new PsrCanId(0, SourceBitMask, 0, 0, 0);
	public static PsrCanId MessageIdMask => new PsrCanId(0, 0, MessageIdBitMask, 0, 0);
	public static PsrCanId DeviceTypeMask => new PsrCanId(0, 0, 0, (CanIds.DeviceType)DeviceTypeBitMask, 0);
	public static PsrCanId PriorityMask => new PsrCanId(0, 0, 0, 0, (CanIds.MessagePriority)PriorityBitMask);

	private const int DestinationBits = 8;
	private const int SourceBits = 8;
	private const int MessageIdBits = 6;
	private const int DeviceTypeBits = 5;
	private const int PriorityBits = 2;

	private const int DestinationOffset = 0;
	private const int SourceOffset = DestinationOffset + DestinationBits;
	private const int MessageIdOffset = SourceOffset + SourceBits;
	private const int DeviceTypeOffset = MessageIdOffset + MessageIdBits;
	private const int PriorityOffset = DeviceTypeOffset + DeviceTypeBits;

	private const byte DestinationBitMask = (1 << DestinationBits) - 1;
	private const byte SourceBitMask = (1 << SourceBits) - 1;
	private const byte MessageIdBitMask = (1 << MessageIdBits) - 1;
	private const byte DeviceTypeBitMask = (1 << DeviceTypeBits) - 1;
	private const byte PriorityBitMask = (1 << PriorityBits) - 1;

	public static PsrCanId FromInteger(uint id)
	{
		byte dst = (byte)((id >> DestinationOffset) & DestinationBitMask);
		byte src = (byte)((id >> SourceOffset) & SourceBitMask);
		byte messageId = (byte)((id >> MessageIdOffset) & MessageIdBitMask);
		CanIds.DeviceType deviceType = (CanIds.DeviceType)((id >> DeviceTypeOffset) & DeviceTypeBitMask);
		CanIds.MessagePriority priority = (CanIds.MessagePriority)((id >> PriorityOffset) & PriorityBitMask);

		return new PsrCanId(dst, src, messageId, deviceType, priority);
	}

	public uint ToInteger()
	{
		uint id = 0;
		id |= (uint)(Destination & DestinationBitMask) << DestinationOffset;
		id |= (uint)(Source & SourceBitMask) << SourceOffset;
		id |= (uint)(MessageId & MessageIdBitMask) << MessageIdOffset;
		id |= ((uint)DeviceType & DeviceTypeBitMask) << DeviceTypeOffset;
		id |= ((uint)Priority & PriorityBitMask) << PriorityOffset;

		return id;
	}

	public static uint ToInteger(byte dst, byte src, byte messageId, CanIds.DeviceType deviceType, CanIds.MessagePriority priority)
	{
		uint id = 0;
		id |= (uint)(dst & DestinationBitMask) << DestinationOffset;
		id |= (uint)(src & SourceBitMask) << SourceOffset;
		id |= (uint)(messageId & MessageIdBitMask) << MessageIdOffset;
		id |= ((uint)deviceType & DeviceTypeBitMask) << DeviceTypeOffset;
		id |= ((uint)priority & PriorityBitMask) << PriorityOffset;

		return id;
	}

	public bool Equals(PsrCanId other)
	{
		return Destination == other.Destination &&
			   Source == other.Source &&
			   MessageId == other.MessageId &&
			   DeviceType == other.DeviceType &&
			   Priority == other.Priority;
	}

	public override bool Equals(object obj) => obj is PsrCanId other && Equals(other);

	public override int GetHashCode() => HashCode.Combine(Destination, Source, MessageId, DeviceType, Priority);

	public static bool operator ==(PsrCanId left, PsrCanId right) => left.Equals(right);
	public static bool operator !=(PsrCanId left, PsrCanId right) => !left.Equals(right);

	public static PsrCanId operator &(PsrCanId left, PsrCanId right)
	{
		byte dst = (byte)((left.Destination & right.Destination) & DestinationBitMask);
		byte src = (byte)((left.Source & right.Source) & SourceBitMask);
		byte messageId = (byte)((left.MessageId & right.MessageId) & MessageIdBitMask);
		CanIds.DeviceType deviceType = (CanIds.DeviceType)((byte)((byte)left.DeviceType & (byte)right.DeviceType) & DeviceTypeBitMask);
		CanIds.MessagePriority priority = (CanIds.MessagePriority)((byte)((byte)left.Priority & (byte)right.Priority) & PriorityBitMask);

		return new PsrCanId(dst, src, messageId, deviceType, priority);
	}

	public static PsrCanId operator |(PsrCanId left, PsrCanId right)
	{
		byte dst = (byte)((left.Destination | right.Destination) & DestinationBitMask);
		byte src = (byte)((left.Source | right.Source) & SourceBitMask);
		byte messageId = (byte)((left.MessageId | right.MessageId) & MessageIdBitMask);
		CanIds.DeviceType deviceType = (CanIds.DeviceType)((byte)((byte)left.DeviceType | (byte)right.DeviceType) & DeviceTypeBitMask);
		CanIds.MessagePriority priority = (CanIds.MessagePriority)((byte)((byte)left.Priority | (byte)right.Priority) & PriorityBitMask);

		return new PsrCanId(dst, src, messageId, deviceType, priority);
	}

	public static PsrCanId operator ^(PsrCanId left, PsrCanId right)
	{
		byte dst = (byte)((left.Destination ^ right.Destination) & DestinationBitMask);
		byte src = (byte)((left.Source ^ right.Source) & SourceBitMask);
		byte messageId = (byte)((left.MessageId ^ right.MessageId) & MessageIdBitMask);
		CanIds.DeviceType deviceType = (CanIds.DeviceType)((byte)((byte)left.DeviceType ^ (byte)right.DeviceType) & DeviceTypeBitMask);
		CanIds.MessagePriority priority = (CanIds.MessagePriority)((byte)((byte)left.Priority ^ (byte)right.Priority) & PriorityBitMask);

		return new PsrCanId(dst, src, messageId, deviceType, priority);
	}

	public static PsrCanId operator ~(PsrCanId value)
	{
		byte dst = (byte)~value.Destination;
		byte src = (byte)~value.Source;
		byte messageId = (byte)(~value.MessageId & MessageIdBitMask);
		CanIds.DeviceType deviceType = (CanIds.DeviceType)((byte)~value.DeviceType & DeviceTypeBitMask);
		CanIds.MessagePriority priority = (CanIds.MessagePriority)((byte)~value.Priority & PriorityBitMask);

		return new PsrCanId(dst, src, messageId, deviceType, priority);
	}

	public static explicit operator uint(PsrCanId value) => value.ToInteger();
	public static implicit operator PsrCanId(uint value) => FromInteger(value);
}
