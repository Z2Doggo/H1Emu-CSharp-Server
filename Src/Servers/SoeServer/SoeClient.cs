using Servers.SOEInputStream;
using Servers.SOEOutputStream;
using Soe.LogicalPacket;
using SoeServerTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace SOEClient
{
    public interface ISOEClientStats
    {
        int TotalPacketSent { get; set; }
        int PacketResend { get; set; }
        int PacketsOutOfOrder { get; set; }
    }

    public interface IPacketsQueue
    {
        List<LogicalPacket> Packets { get; set; }
        int CurrentByteLength { get; set; }
        Timer Timer { get; set; }
    }

    public class SOEClient
    {
        public int SessionId { get; set; } = 0;
        public string Address { get; }
        public int Port { get; }
        public int CrcSeed { get; }
        public int CrcLength { get; set; } = 2;
        public int ClientUdpLength { get; set; } = 512;
        public int ServerUdpLength { get; set; } = 512;
        public int PacketsSentThisSec { get; set; } = 0;
        public bool UseEncryption { get; set; } = true;
        public IPacketsQueue WaitingQueue { get; set; } =
            new PacketsQueue { Packets = new List<LogicalPacket>(), CurrentByteLength = 0 };
        public List<LogicalPacket> OutQueue { get; set; } = new List<LogicalPacket>();
        public string ProtocolName { get; set; } = "unset";
        public ConcurrentDictionary<int, int> UnAckData { get; } =
            new ConcurrentDictionary<int, int>();
        public List<SoePacket> OutOfOrderPackets { get; set; } = new List<SoePacket>();
        public WrappedUint16 NextAck { get; set; } = new WrappedUint16(1);
        public WrappedUint16 LastAck { get; set; } = new WrappedUint16(1);
        public SOEInputStream InputStream { get; }
        public SOEOutputStream OutputStream { get; }
        public string SoeClientId { get; }
        public Timer LastPingTimer { get; set; }
        public bool IsDeleted { get; set; } = false;
        public ISOEClientStats Stats { get; set; } =
            new SOEClientStats { TotalPacketSent = 0, PacketsOutOfOrder = 0, PacketResend = 0 };
        public int LastAckTime { get; set; } = 0;

        public SOEClient(IPEndPoint remote, int crcSeed, byte[] cryptoKey)
        {
            Address = remote.Address.ToString();
            Port = remote.Port;
            CrcSeed = crcSeed;
            InputStream = new SOEInputStream(cryptoKey);
            OutputStream = new SOEOutputStream(cryptoKey);
            SoeClientId = $"{Address}:{Port}";
        }
        public string[] GetNetworkStats()
        {
            var totalPacketSent = Stats.TotalPacketSent;
            var packetResend = Stats.PacketResend;
            var packetsOutOfOrder = Stats.PacketsOutOfOrder;
            var packetLossRate = (double)packetResend / totalPacketSent * 100;
            var packetOutOfOrderRate = (double)packetsOutOfOrder / totalPacketSent * 100;
            return new[]
            {
                $"Packet loss rate {packetLossRate:F3}%",
                $"Packet outOfOrder rate {packetOutOfOrderRate:F3}%"
            };
        }
    }

    public class WrappedUint16
    {
        private ushort _value;
        public WrappedUint16(ushort value)
        {
            _value = value;
        }

        public static WrappedUint16 operator ++(WrappedUint16 value)
        {
            value._value++;
            if (value._value > ushort.MaxValue)
            {
                value._value = 0;
            }
            return value;
        }

        public static implicit operator ushort(WrappedUint16 value)
        {
            return value._value;
        }
    }

    public class PacketsQueue : IPacketsQueue
    {
        public List<LogicalPacket> Packets { get; set; }
        public int CurrentByteLength { get; set; }
        public Timer Timer { get; set; }
    }

    public class SOEClientStats : ISOEClientStats
    {
        public int TotalPacketSent { get; set; }
        public int PacketResend { get; set; }
        public int PacketsOutOfOrder { get; set; }
    }
}


