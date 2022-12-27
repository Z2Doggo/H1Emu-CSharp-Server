namespace Servers.SoeServer
{
    using Events;
    using H1EmuCore;
    using SOEClient;
    using LogicalPacket;
    using SharedTypes;
    using System.Threading;
    using System.Net.Sockets;

    public class SOEServer : EventEmitter
    {
        int _ServerPort { get; set; }
        byte[] _CryptoKey { get; set; }
        SoeProtocol _Protocol { get; set; }
        int _UdpLength { get; set; } = 512;
        bool _UseEncryption { get; set; } = true;
        private Dictionary<string, SOEClient> _Clients = new Dictionary<string, SOEClient>();
        private Thread _Connection;
        int _CrcSeed { get; set; } = 0;
        public enum CrcLengthOptions // TEMPORARY WORKAROUND...
        {
            Zero = 0,
            Two = 2,
        }
        CrcLengthOptions _CrcLength = CrcLengthOptions.Two;
        int _WaitQueueTimeMs { get; set; } = 50;
        int _PingTimeoutTime { get; set; } = 60000;
        bool _UsePingTimeout { get; set; } = false;
        private int _MaxMultiBufferSize { get; set; }
        private readonly Timer _SoeClientRoutineLoopMethod;
        private int _ResendTimeout { get; set; } = 300;
        int PacketRatePerClient { get; set; } = 500;
        private int _AckTiming { get; set; } = 80;
        private int _RoutineTiming { get; set; } = 3;
        bool _AllowedRawDataReception { get; set; } = true;

        public SOEServer(int ServerPort, byte[] CryptoKey, bool DisableAntiDdos = false)
        {
            _ServerPort = ServerPort;
            _CryptoKey = CryptoKey;
            _MaxMultiBufferSize = (int)(_UdpLength - 4 - _CrcLength);
            _SoeClientRoutineLoopMethod = new Timer(ResetPacketsSent, null, 1000, Timeout.Infinite);
        }

        private void ResetPacketsSent(object State)
        {
            _SoeClientRoutineLoopMethod.Change(1000, Timeout.Infinite);
        }
    }
}