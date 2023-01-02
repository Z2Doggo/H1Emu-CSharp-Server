namespace Shared.H1EmuServer
{
    using Protocols.H1Emu;
    using H1EmuClient;
    using System.Threading;
    using System.Net.Sockets;
    using System.Net;
    using System.Text;

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
            _Thread = new Thread(() => Run(serverPort));
            _Thread.Start();
        }

        public void Run(int port)
        {
            UdpClient listener = new UdpClient(port);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, port);

            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting for broadcast");
                    byte[] bytes = listener.Receive(ref groupEP);

                    Console.WriteLine($"Received broadcast from {groupEP} :");
                    Console.WriteLine($" {Encoding.ASCII.GetString(bytes, 0, bytes.Length)}");
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                listener.Close();
            }
        }
    }
}
