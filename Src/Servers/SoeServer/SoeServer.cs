using H1EmuCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UDPServer;

namespace SOEServer
{
    public class SOEServer : EventEmitter
    {
        private readonly int _serverPort;
        private readonly byte[] _cryptoKey;
        private Soeprotocol _protocol;
        private readonly int _udpLength = 512;
        private readonly bool _useEncryption = true;
        private readonly ConcurrentDictionary<string, SOEClient.SOEClient> _clients = new ConcurrentDictionary<string, SOEClient.SOEClient>();
        private readonly UdpClient _connection;
        private int _crcSeed = 0;
        private readonly Crc_length_options _crcLength = Crc_length_options.Two;
        private readonly int _waitQueueTimeMs = 50;
        private readonly int _pingTimeoutTime = 60000;
        private readonly bool _usePingTimeout = false;
        private readonly int _maxMultiBufferSize;
        private readonly Timer _soeClientRoutineLoopMethod;
        private readonly int _resendTimeout = 300;
        public int packetRatePerClient = 500;
        private readonly int _ackTiming = 80;
        private readonly int _routineTiming = 3;
        private readonly bool _allowRawDataReception = true;

        public SOEServer(int serverPort, byte[] cryptoKey, bool disableAntiDdos = false)
        {
            _serverPort = serverPort;
            _cryptoKey = cryptoKey;
            _maxMultiBufferSize = (int)(_udpLength - 4 - _crcLength);
            _connection = new UdpClient(_serverPort);
            _soeClientRoutineLoopMethod = new Timer(ResetPacketsSent, null, 1000, Timeout.Infinite);
        }

        private void ResetPacketsSent(object state)
        {
            // Reset packets sent
            _soeClientRoutineLoopMethod.Change(1000, Timeout.Infinite);
        }
    }

    public enum Crc_length_options
    {
        Zero= 0,
        Two = 2,
    }
}
