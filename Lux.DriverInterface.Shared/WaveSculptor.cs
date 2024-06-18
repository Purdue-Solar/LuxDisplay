using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lux.DriverInterface.Shared.CanPackets.Wavesculptor;
using ErrorFlags = Lux.DriverInterface.Shared.CanPackets.Wavesculptor.Broadcast.Status.ErrorFlags;
using LimitFlags = Lux.DriverInterface.Shared.CanPackets.Wavesculptor.Broadcast.Status.LimitFlags;

namespace Lux.DriverInterface.Shared;
public class WaveSculptor
{
    public const float MpsToMph = 2.23693629f;

    public ErrorFlags ErrorFlags { get; set; }
    public LimitFlags LimitFlags { get; set; }
    public float BusCurrent { get; set; }
    public float BusVoltage { get; set; }
    public float VehicleVelocity { get; set; }
    public float MotorVelocity { get; set; }
    public float PhaseCCurrent { get; set; }
    public float PhaseBCurrent { get; set; }
    public float Vd { get; set; }
    public float Vq { get; set; }
    public float CurrentD { get; set; }
    public float CurrentQ { get; set; }
    public float BemfD { get; set; }
    public float BemfQ { get; set; }
    public float Voltage15 { get; set; }
    public float Voltage1V9 { get; set; }
    public float Voltage3V3 { get; set; }
    public float HeatsinkTemp { get; set; }
    public float MotorTemp { get; set; }
    public float DspBoardTemp { get; set; }
    public float DcBusAmpHrs { get; set; }
    public float Odometer { get; set; }
    public float SlipSpeed { get; set; }
}