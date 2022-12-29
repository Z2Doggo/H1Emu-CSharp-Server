namespace Servers.SoeServer
{
    using System.Buffers;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using Events;
    using H1EmuCore;
    using LogicalPacket;
    using SharedTypes;
    using SOEClient;
    public class SOEServer : EventEmitter
    {
        private readonly int _serverPort;
        private readonly byte[] _cryptoKey;
        private readonly SoeProtocol _protocol;
        private readonly int _udpLength = 512;
        private readonly bool _useEncryption = true;
        private readonly Dictionary<string, SOEClient> _clients = new Dictionary<string, SOEClient>();
        private readonly Thread _workerThread;
        private readonly int _crcSeed = 0;
        public enum CrcLengthOptions // TEMPORARY WORKAROUND...
        {
            Zero = 0,
            Two = 2,
        }
        private readonly CrcLengthOptions _crcLength = CrcLengthOptions.Two;
        private readonly int _waitQueueTimeMs = 50;
        private readonly int _pingTimeoutTime = 60000;
        private readonly bool _usePingTimeout = false;
        private readonly int _maxMultiBufferSize;
        private readonly Action<Action, int> _soeClientRoutineLoopMethod;
        private readonly int _resendTimeout = 300;
        private readonly int _packetRatePerClient = 500;
        private readonly int _ackTiming = 80;
        private readonly int _routineTiming = 3;
        private readonly bool _allowedRawDataReception = true;

        public SOEServer(int serverPort, byte[] cryptoKey, bool? disableAntiDdos = false) : base()
        {
            int bufferSize = 8192 * 4;
            byte[] buffer = new byte[bufferSize];
            _serverPort = serverPort;
            _cryptoKey = cryptoKey;
            _maxMultiBufferSize = (int)(_udpLength - 4 - _crcLength);
            _workerThread = new Thread(() =>
            {
                // TODO: Replace this with the actual worker thread code
            });
            _workerThread.Start();

            Timer timer = new Timer((state) =>
            {
                ResetPacketsSent();
            }, null, 0, 1000); 
            timer.Dispose();
        }

        private void ResetPacketsSent()
        {
            foreach (var client in _clients.Values)
            {
                client.PacketsSentThisSec = 0;
            }
        }
    }
}