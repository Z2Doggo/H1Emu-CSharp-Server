namespace Workers.UdpServerWorker
{
    using System;
    using System.Threading;
    public class UdpServerWorker
    {
        private readonly Thread _Thread;
        private readonly int _ServerPort;
        private readonly bool _DisableAntiDdos;

        public UdpServerWorker(int serverPort, bool disableAntiDdos)
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