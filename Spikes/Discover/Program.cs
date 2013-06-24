using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Discover
{
    static class Program
    {
        static void Main(string[] args)
        {
            // To discover empegs connected over Ethernet, we need to broadcast a '?', and listen for the responses.
            var dgram = new byte[] { 0x3F }; // ASCII '?'
            var client = new UdpClient(new IPEndPoint(IPAddress.Any, 8300));
            client.EnableBroadcast = true;
            client.Client.ReceiveTimeout = 500;
            client.Send(dgram, dgram.Length, new IPEndPoint(IPAddress.Broadcast, 8300));

            const int retryCount = 10;
            for (int i = 0; i < retryCount; ++i)
            {
                try
                {
                    IPEndPoint from = null;
                    var bytes = client.Receive(ref from);
                    Console.WriteLine(BitConverter.ToString(bytes));
                    var response = Encoding.ASCII.GetString(bytes);
                    Console.WriteLine("Received {0} from {1}.", response, from);
                }
                catch (SocketException)
                {
                    break;
                }
            }
        }
    }
}
