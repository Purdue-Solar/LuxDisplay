using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using System.Buffers.Binary;
using System.Device.Spi;
using SocketCANSharp;
using SocketCANSharp.Network;

namespace Lux.DataRadio
{
	public class EncoderService(DriverInterface.Shared.Encoder amt) : BackgroundService
	{
		private readonly DriverInterface.Shared.Encoder _amt = amt;
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			using PeriodicTimer timer = new(TimeSpan.FromSeconds(.1));

			//Set zero position for encoder
			var settings = new SpiConnectionSettings(1, 0)
			{
				ClockFrequency = 100000, // .1MHz
				Mode = SpiMode.Mode0,
				DataBitLength = 8,
			};

			ushort ZeroValue = 1;
			using (SpiDevice device = SpiDevice.Create(settings))
			{
				byte[] writeBuffer = new byte[] { 0x00, 0x00 };
				byte[] readBuffer = new byte[2] { 0x00, 0x70 };
				device.TransferFullDuplex(writeBuffer, readBuffer);

				ZeroValue = (ushort)(BinaryPrimitives.ReadUInt16BigEndian(readBuffer) & 0x3FFF);
			}
			using (var rawCanSocket = new RawCanSocket())
			{
				CanNetworkInterface can0 = CanNetworkInterface.GetAllInterfaces(true).First(iface => iface.Name.Equals("can0"));
				rawCanSocket.Bind(can0);
				while (await timer.WaitForNextTickAsync(stoppingToken))
				{
                    /*
					using (SpiDevice device = SpiDevice.Create(settings))
					{
						// Send a byte and read the response
						byte[] writeBuffer = new byte[] { 0x00, 0x00 };
						byte[] readBuffer = new byte[2] { 0x00, 0x00 };
						device.TransferFullDuplex(writeBuffer, readBuffer);

						//Setting Motor Current
						_amt.Value = BinaryPrimitives.ReadUInt16BigEndian(readBuffer);

						float CurrentPercent = ((ushort)(_amt.Value & 0x3FFF) - ZeroValue) / 1365.0f;

						//clamping
						if (CurrentPercent < 0)
						{
							CurrentPercent = 0;
						}
						else if (CurrentPercent > 1)
						{
							CurrentPercent = 1;
						}

						_amt.Percentage = CurrentPercent * 100;

						byte[] message_drive = new byte[8];

						BinaryPrimitives.WriteSingleLittleEndian(message_drive.AsSpan().Slice(0, 4), 10000);
						BinaryPrimitives.WriteSingleLittleEndian(message_drive.AsSpan().Slice(4, 4), CurrentPercent);
						CanFrame frame_drive = new CanFrame();
						frame_drive.CanId = 0x501;
						frame_drive.Data = message_drive;
						frame_drive.Length = 8;
						frame_drive.Len8Dlc = 8;

						byte[] message_power = new byte[8];
						BinaryPrimitives.WriteSingleLittleEndian(message_power.AsSpan().Slice(0, 4), 0);
						BinaryPrimitives.WriteSingleLittleEndian(message_power.AsSpan().Slice(4, 4), 1f);
						CanFrame frame_power = new CanFrame();
						frame_power.CanId = 0x502;
						frame_power.Data = message_power;
						frame_power.Length = 8;
						frame_power.Len8Dlc = 8;
						//LuxDataRadio.Shared.CANQueue.Enqueue(frame);

						rawCanSocket.Write(frame_drive);
						rawCanSocket.Write(frame_power);
					}
					*/

                    //Setting Motor Current

                    byte[] message_drive = new byte[8];

                    BinaryPrimitives.WriteSingleLittleEndian(message_drive.AsSpan().Slice(0, 4), 10000);
                    BinaryPrimitives.WriteSingleLittleEndian(message_drive.AsSpan().Slice(4, 4), _amt.Percentage);
                    CanFrame frame_drive = new CanFrame();
                    frame_drive.CanId = 0x501;
                    frame_drive.Data = message_drive;
                    frame_drive.Length = 8;
                    frame_drive.Len8Dlc = 8;

                    byte[] message_power = new byte[8];
                    BinaryPrimitives.WriteSingleLittleEndian(message_power.AsSpan().Slice(0, 4), 0);
                    BinaryPrimitives.WriteSingleLittleEndian(message_power.AsSpan().Slice(4, 4), 1f);
                    CanFrame frame_power = new CanFrame();
                    frame_power.CanId = 0x502;
                    frame_power.Data = message_power;
                    frame_power.Length = 8;
                    frame_power.Len8Dlc = 8;
                    //LuxDataRadio.Shared.CANQueue.Enqueue(frame);

                    rawCanSocket.Write(frame_drive);
                    rawCanSocket.Write(frame_power);
                }
            }
		}
	}
}
