using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UdpServer
{
    class Program
    {
        static Task Main()
        {
            // Create a new UDP socket
            Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // Bind the socket to the localhost and port 1115
            IPEndPoint localEndPoint = new(IPAddress.Parse("127.0.0.1"), 1115);
            socket.Bind(localEndPoint);

            // Start listening for incoming UDP datagrams
            while (true)
            {
                // Create a buffer to store the incoming data
                byte[] buffer = new byte[512];

                // Create an EndPoint to capture the sender's address
                EndPoint senderEndPoint = new IPEndPoint(IPAddress.Any, 0);

                // Receive the incoming data
                int bytesReceived = socket.ReceiveFrom(buffer, ref senderEndPoint);

                // Print the received data
                Console.WriteLine($"Received {bytesReceived} bytes from {senderEndPoint}: {Encoding.UTF8.GetString(buffer, 0, bytesReceived)}");
            }
        }
    }
}