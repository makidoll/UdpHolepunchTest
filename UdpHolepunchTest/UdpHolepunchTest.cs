using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace UdpHolepunchTest
{
    public class UdpHolepunchTest
    {
        // private readonly IPEndPoint _holepunchServer = new(Dns.GetHostAddresses("tivoli.space")[0], 5971);
        private readonly IPEndPoint _holepunchServer = new(IPAddress.Parse("142.93.250.50"), 5971);

        private bool _hosting;
        private string _instanceId;

        private UdpClient _udpClient;

        private int _incrementingNumberForTestMessages;

        public UdpHolepunchTest(string[] args)
        {
            Init(args);
        }

        private async void Init(string[] args)
        {
            if (args.Length < 2 || (args[0] != "host" && args[0] != "client"))
            {
                Console.WriteLine("dotnet run <host,client> <instance id>");
                Environment.Exit(1);
            }

            _hosting = args[0] == "host";
            _instanceId = args[1];

            _udpClient = new UdpClient();
            _udpClient.ExclusiveAddressUse = false;
            _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            if (_hosting)
            {
                await StartHost();
            }
            else
            {
                await StartClient();
            }

            var heartbeatTimer = new Timer();
            heartbeatTimer.Elapsed += Heartbeat;
            heartbeatTimer.Interval = 1000;
            heartbeatTimer.Start();
            
            ReceiveLoop(_holepunchServer, data =>
            {
                var ipAndPort = Encoding.UTF8.GetString(data).Split(' ');
                var peerEndpoint = new IPEndPoint(IPAddress.Parse(ipAndPort[0]), int.Parse(ipAndPort[1]));
                
                if (_hosting)
                {
                    Console.WriteLine("new client: " + peerEndpoint);
                }
                else
                {
                    Console.WriteLine("new host: " + peerEndpoint);
                }

                Console.WriteLine("listening and writing to: " + peerEndpoint);

                ReceiveLoop(peerEndpoint,
                    peerData => { Console.WriteLine("got data! " + Encoding.UTF8.GetString(peerData)); });

                KeepSendingTestMessagesToPeer(peerEndpoint);
            });

            Console.WriteLine("press enter to close");
            Console.ReadLine();
        }

        private void KeepSendingTestMessagesToPeer(IPEndPoint peerEndpoint)
        {
            var peerTimer = new Timer();
            peerTimer.Elapsed += async (_, _) =>
            {
                // Console.WriteLine("sending message to: " + peerEndpoint);
                var message = Encoding.UTF8.GetBytes("yay hi how are you! msg " + _incrementingNumberForTestMessages);
                await _udpClient.SendAsync(message, message.Length, peerEndpoint);
                _incrementingNumberForTestMessages++;
            };
            peerTimer.Interval = 200;
            peerTimer.Start();
        }

        private void ReceiveLoop(IPEndPoint address, Action<byte[]> onData)
        {
            _udpClient.BeginReceive(asyncResult =>
            {
                var receiveAddress = address;
                var result = _udpClient.EndReceive(asyncResult, ref receiveAddress);
                onData.Invoke(result);
                ReceiveLoop(address, onData);
            }, null);
        }
        
        private async Task StartHost()
        {
            Console.WriteLine("start host");
            var message = Encoding.UTF8.GetBytes("host " + _instanceId);
            await _udpClient.SendAsync(message, message.Length, _holepunchServer);
        }

        private async Task StartClient()
        {
            Console.WriteLine("start client");
            var message = Encoding.UTF8.GetBytes("client " + _instanceId);
            await _udpClient.SendAsync(message, message.Length, _holepunchServer);
        }

        private void Heartbeat(object sender, ElapsedEventArgs e)
        {
            // Console.WriteLine("heartbeat");
            _udpClient.SendAsync(new byte[] {0}, 1, _holepunchServer);
        }
    }
}