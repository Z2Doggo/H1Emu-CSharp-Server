using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UDPServer
{
    class Program
    {
        static void Main()
        {
            IPEndPoint endPoint = new(IPAddress.Parse("127.0.0.1"), 1115);

            // Create a new UDP socket
            using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            // Bind the socket to the endpoint
            socket.Bind(endPoint);

            // Create a buffer to store incoming data
            byte[] buffer = new byte[1024];

            // Create an EndPoint object to store the client's endpoint information
            EndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);

            // Start listening for incoming connections
            while (true)
            {
                try
                {
                    // Receive data from the client
                    int bytesReceived = socket.ReceiveFrom(buffer, ref clientEndPoint);

                    // Convert the data to a string and print it to the console
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
