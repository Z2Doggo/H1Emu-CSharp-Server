namespace Shared.H1EmuServer
{
    using Protocols.H1Emu;
    using H1EmuClient;
    using System.Threading;

    abstract class H1emuServer
    {
        private int? _serverPort;
        private H1EmuProtocol _protocol;
        private int _udpLength = 512;
        private Dictionary<string, H1emuClient> _clients = new Dictionary<string, H1emuClient>();
        private Thread _connection;
        private int _pingTime = 5000;
        private int _pingTimeout = 12000;
        private Timer _pingTimer;

        protected H1emuServer(int? serverPort = null)
        {
            _serverPort = serverPort;
            _protocol = new H1EmuProtocol();
            _connection = new Thread(() => UdpServerWorker((int)_serverPort, false));
            _connection.Start();
        }

        private Thread _Thread;
        private int _ServerPort;
        private bool _DisableAntiDdos;

        public void UdpServerWorker(int serverPort, bool disableAntiDdos)
        {
            _ServerPort = serverPort;
            _DisableAntiDdos = disableAntiDdos;
            _Thread = new Thread(Run);
            _Thread.Start();
        }

        private void Run()
        {
            // code to run the UDP server goes here
            Console.WriteLine("Running UDP server on port " + _ServerPort + " with anti-DDOS " + (_DisableAntiDdos ? "disabled" : "enabled"));
        }
    }
}
