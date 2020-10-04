using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Xml.Serialization;

namespace Client
{
    public class MyMessage
    {
        public string StringProperty { get; set; }
        public int intProperty { get; set; }
    }

    class Program
    {
        static async Task SendAsync<T>(NetworkStream networkstream, T message)
        {
            var (header, body) = Encode(message);

            await networkstream.WriteAsync(header, 0, header.Length).ConfigureAwait(false);
            await networkstream.WriteAsync(body, 0, body.Length).ConfigureAwait(false);
        }

        static async Task<T> Receive<T>(NetworkStream networkstream)
        {
            var headerBytes = await ReadAsync(networkstream, 4);
            var bodyLength = IPAddress.NetworkToHostOrder( BitConverter.ToInt32(headerBytes) );

            var bodyBytes = await ReadAsync(networkstream, bodyLength);

            return Decode<T>(bodyBytes);
           
        }

        /*static async Task<T> ReceiveAsync<T>(NetworkStream networkStream)
        {

            var headerBytes = await ReadAsync(networkStream, 4);
            var bodyLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(headerBytes));

            var bodyBytes = await ReadAsync(networkStream, bodyLength);

            return Decode<T>(bodyBytes);
        }*/

        static (byte[] header, byte[] body) Encode<T>(T message)
        {
            var xs = new XmlSerializer(typeof(T));
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);
            xs.Serialize(sw, message);

            var bodyBytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            var headerBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(bodyBytes.Length));

            return (headerBytes, bodyBytes);
        }

        static T Decode<T>(byte[ ] body)
         {
             var str = System.Text.Encoding.UTF8.GetString(body);
             var sr = new StringReader(str);
             var xs = new XmlSerializer(typeof(T));

             return ( T ) xs.Deserialize(sr);
         }

        static async Task<byte[]> ReadAsync(NetworkStream networkStream ,int bytesToRead)
        {
            var buffer = new byte[bytesToRead];
            var bytesRead = 0;
            while(bytesRead < bytesToRead)
            {
                var bytesReceived = await networkStream.ReadAsync(buffer, bytesRead, (bytesToRead - bytesRead) ).ConfigureAwait(false);
                if(bytesReceived == 0)
                {
                    throw new Exception("Socket Closed");
                }
                bytesRead += bytesReceived;
            }
            return buffer;
        }

        static async Task Main(string[] args)
        {
            Console.WriteLine("Pressione qualquer tecla para conectar");
            Console.ReadLine();

            var endPoint = new IPEndPoint(IPAddress.Loopback, 9000);
            var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            socket.Connect(endPoint);

            var networkStream = new NetworkStream(socket, true);

            var myMessage = new MyMessage
            {
                intProperty = 404,
                StringProperty = "Hello World"
            };

            Console.WriteLine("Sending");
            Print(myMessage);

            await SendAsync(networkStream, myMessage).ConfigureAwait(false);

            var responseMsg = await Receive<MyMessage>(networkStream).ConfigureAwait(false);

            Console.WriteLine("Received");
            Print(responseMsg);

            Console.ReadLine();
        }
        static void Print(MyMessage m) => Console.WriteLine($"MyMessage.IntProperty = {m.intProperty}, MyMessage.StringProperty = {m.StringProperty}");
    }
}
