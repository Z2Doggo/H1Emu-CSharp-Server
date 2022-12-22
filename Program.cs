using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

using H1EmuCore;

namespace UDPServer
{
    using System;
    using System.Collections.Generic;

    public class EventEmitter
    {
        private readonly Dictionary<string, List<Action<object[]>>> events = new Dictionary<string, List<Action<object[]>>>();

        public void On(string eventName, Action<object[]> callback)
        {
            if (!events.ContainsKey(eventName))
            {
                events[eventName] = new List<Action<object[]>>();
            }

            events[eventName].Add(callback);
        }

        public void Off(string eventName, Action<object[]> callback)
        {
            if (events.ContainsKey(eventName))
            {
                events[eventName].Remove(callback);
            }
        }

        public void Emit(string eventName, params object[] args)
        {
            if (events.ContainsKey(eventName))
            {
                foreach (var callback in events[eventName])
                {
                    callback(args);
                }
            }
        }
    }
    class Program
    {
        static void Main()
        {
            IPEndPoint endPoint = new(IPAddress.Parse("127.0.0.1"), 1115);

            using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            socket.Bind(endPoint);

            byte[] buffer = new byte[512];

            EndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                try
                {
                    int bytesReceived = socket.ReceiveFrom(buffer, ref clientEndPoint);

                    string data = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
                    Console.WriteLine("Received: {0}", data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred: {0}", ex.Message);
                }
            }
        }
    }
}
