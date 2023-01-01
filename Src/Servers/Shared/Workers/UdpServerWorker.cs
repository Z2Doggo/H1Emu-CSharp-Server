namespace Workers.UdpServerWorker
{
    using System;
    using System.Threading;
    public class UdpServerWorker
    {
        private Thread _Thread;
        private int _ServerPort;
        private bool _DisableAntiDdos;

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