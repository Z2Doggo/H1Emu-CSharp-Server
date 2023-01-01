namespace Servers.SoeServer
{
    using System;
    using System.Buffers;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;
    using Events;
    using H1EmuCore;
    using LogicalPacket;
    using SharedTypes;
    using SOEClient;
    using Newtonsoft.Json;
    using Workers.UdpServerWorker;
    using System.Net.Sockets;
    using DnsClient;
    using Newtonsoft.Json.Linq;

    public class SOEServer : EventEmitter
    {
        public class MessageEventArgs : EventArgs
        {
            public IPEndPoint Remote { get; set; }
            public byte[] Data { get; set; }
        }

        class Connection
        {
            public static void PostMessage(object message)
            {
                // Convert the message object to a byte array
                byte[] messageBytes = Encoding.UTF8.GetBytes(message.ToString());

                // Create a UdpClient and send the message to the localhost on port 12345
                using (UdpClient client = new UdpClient())
                {
                    client.Send(messageBytes, messageBytes.Length, "127.0.0.1", 1115);
                }
            }
        }

        public async Task Start()
        {
            // code to start the connection goes here
            Console.WriteLine("Connection started");
            _Connection = Task.CompletedTask;
        }

        private readonly int _serverPort;
        private byte[] _CryptoKey;
        private SoeProtocol _protocol;
        private int _udpLength = 512;
        private readonly bool _useEncryption = true;
        private readonly Dictionary<string, SOEClient> _clients = new Dictionary<
            string,
            SOEClient
        >();
        private SynchronizationContext _syncContext;
        private Task _Connection;

        private int _crcSeed = 0;
        public enum CrcLengthOptions // TEMPORARY WORKAROUND...
        {
            Zero = 0,
            Two = 2,
        }
        private CrcLengthOptions _crcLength = CrcLengthOptions.Two;
        private readonly int _waitQueueTimeMs = 50;
        private readonly int _pingTimeoutTime = 60000;
        private readonly bool _usePingTimeout = false;
        private readonly int _maxMultiBufferSize;
        private Action<Action, int> _soeClientRoutineLoopMethod;
        private readonly int _resendTimeout = 300;
        private readonly int _packetRatePerClient = 500;
        private int _ackTiming = 80;
        private bool _allowRawDataReception = true;
        private int _routineTiming = 3;
        private readonly bool _allowedRawDataReception = true;

        public SOEServer(int serverPort, byte[] cryptoKey, bool? disableAntiDdos = false) : base()
        {
            int bufferSize = 8192 * 4;
            byte[] buffer = new byte[bufferSize];
            _serverPort = serverPort;
            _CryptoKey = cryptoKey;
            _maxMultiBufferSize = (int)(_udpLength - 4 - _crcLength);
            _Connection = new Task(
                () =>
                {
                    var worker = new System.Threading.Thread(
                        () =>
                        {
                            var workerData = new
                            {
                                ServerPort = serverPort,
                                DisableAntiDdos = disableAntiDdos
                            };
                            var workerThread = new UdpServerWorker(
                                workerData.ServerPort,
                                (bool)workerData.DisableAntiDdos
                            );
                        }
                    );
                    worker.Start();
                }
            );
            Timer timer = new Timer(
                (state) =>
                {
                    ResetPacketsSent();
                },
                null,
                0,
                1000
            );
            timer.Dispose();
        }

        private void ResetPacketsSent()
        {
            foreach (var client in _clients.Values)
            {
                client.PacketsSentThisSec = 0;
            }
        }

        private void _SendPhysicalPacket(SOEClient Client, byte[] Packet)
        {
            Client.PacketsSentThisSec++;
            Client.Stats.TotalPacketSent++;

            // Create an anonymous object with the packet data and client information
            var messageData = new
            {
                Type = "sendPacket",
                Data = new { packetData = Packet, port = Client.Port, address = Client.Address }
            };

            // Send the message data using the synchronous context
            _syncContext.Send(new SendOrPostCallback(OnMessageReceived), messageData);
        }

        private void OnMessageReceived(object state)
        {
            // Extract the message data from the state object
            var messageData = (dynamic)state;
            // Process the message data as needed
            // ...
        }

        private void SendOutQueue(SOEClient client)
        {
            Debug.WriteLine("Sending out queue");
            while (client.PacketsSentThisSec < _packetRatePerClient)
            {
                if (client.OutQueue.Count > 0)
                {
                    var logicalPacket = client.OutQueue[0];
                    client.OutQueue.RemoveAt(0);

                    // if is a reliable packet
                    if (logicalPacket.IsReliable && logicalPacket.Sequence != null)
                    {
                        DateTime currentTime = DateTime.Now;
                        int value = Convert.ToInt32(currentTime);
                        client.UnAckData[logicalPacket.Sequence] = value;
                    }
                    _SendPhysicalPacket(client, logicalPacket.Data);
                }
                else
                {
                    break;
                }
            }
        }

        // Send pending packets from client, in priority ones from the priority queue
        private void CheckClientOutQueues(SOEClient client)
        {
            if (client.OutQueue.Count > 0)
            {
                SendOutQueue(client);
            }
        }
        private void SoeRoutine()
        {
            foreach (var client in _clients.Values)
            {
                SoeClientRoutine(client);
            }
            _soeClientRoutineLoopMethod(() => SoeRoutine(), _routineTiming);
        }

        // Executed at the same rate for every client
        private void SoeClientRoutine(SOEClient client)
        {
            var lastAckTime = DateTime.FromFileTimeUtc(client.LastAckTime);
            if (lastAckTime.AddMilliseconds(_ackTiming) < DateTime.Now)
            {
                // Acknowledge received packets
                CheckAck(client);
                CheckOutOfOrderQueue(client);
                client.LastAckTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            }
            // Send pending packets
            CheckResendQueue(client);
            CheckClientOutQueues(client);
        }

        // If a packet hasn't been acknowledged in the timeout time, then resend it via the priority queue
        private void CheckResendQueue(SOEClient client)
        {
            var currentTime = DateTime.Now;
            foreach (var kvp in client.UnAckData)
            {
                var sequence = kvp.Key;
                var time = DateTime.FromFileTimeUtc(kvp.Value);
                if (time.AddMilliseconds(_resendTimeout) < currentTime)
                {
                    client.OutputStream.ResendData(sequence);
                    client.UnAckData.Remove(sequence);
                }
            }
        }

        // Use the lastAck value to acknowlege multiple packets as a time
        // This function could be called less often but rn it will stick that way
        private void CheckAck(SOEClient client)
        {
            if (client.LastAck.Get() != client.NextAck.Get())
            {
                client.LastAck.Set(client.NextAck.Get());
                JObject packet = JObject.FromObject(new { sequence = client.NextAck.Get() });

                _SendLogicalPacket(client, "Ack", packet);
            }
        }

        private void ResetPacketsQueue(PacketsQueue queue)
        {
            queue.Packets = new List<LogicalPacket>();
            queue.CurrentByteLength = 0;
        }

        private void SetupResendForQueuedPackets(SOEClient client, PacketsQueue queue)
        {
            for (int index = 0; index < queue.Packets.Count; index++)
            {
                LogicalPacket packet = queue.Packets[index];
                if (packet.IsReliable)
                {
                    client.UnAckData[packet.Sequence] = (int)(
                        DateTime.Now.Ticks + _waitQueueTimeMs
                    );
                }
            }
        }

        // send the queued packets
        private void SendClientWaitQueue(SOEClient client, PacketsQueue queue)
        {
            if (queue.Timer != null)
            {
                queue.Timer.Dispose();
                queue.Timer = null;
            }
            if (queue.Packets.Count > 0)
            {
                JObject packet = JObject.FromObject(
                    new { Sub_packets = queue.Packets.Select(p => p.Data.ToArray()).ToList() }
                );

                _SendLogicalPacket(client, "MultiPacket", packet);
                // if a packet in the waiting queue is a reliable packet, then we need to set the timeout
                SetupResendForQueuedPackets(client, queue);
                ResetPacketsQueue(queue);
            }
        }

        // If some packets are received out of order then we Acknowledge then one by one
        private void CheckOutOfOrderQueue(SOEClient client)
        {
            if (client.OutOfOrderPackets.Count > 0)
            {
                for (int i = 0; i < client.OutOfOrderPackets.Count; i++)
                {
                    int sequence = (dynamic)client.OutOfOrderPackets[0];
                    client.OutOfOrderPackets.RemoveAt(0);
                    if (sequence > client.LastAck)
                    {
                        JObject packet = JObject.FromObject(new { Sequence = sequence });

                        _SendLogicalPacket(client, "OutOfOrder", packet);
                    }
                }
            }
        }

        private SOEClient _createClient(string clientId, IPEndPoint remote)
        {
            var client = new SOEClient(remote, _crcSeed, _CryptoKey);
            _clients.Add(clientId, client);
            return client;
        }

        private void HandlePacket(SOEClient client, dynamic packet)
        {
            switch (packet.name)
            {
                case "SessionRequest":
                    Console.WriteLine(
                        "Received session request from " + client.Address + ":" + client.Port
                    );
                    client.SessionId = packet.session_id;
                    client.ClientUdpLength = packet.udp_length;
                    client.ProtocolName = packet.protocol;
                    client.ServerUdpLength = _udpLength;
                    client.CrcSeed = _crcSeed;
                    client.CrcLength = (int)_crcLength;
                    client.InputStream.setEncryption(_useEncryption);
                    client.OutputStream.SetEncryption(_useEncryption);
                    client.OutputStream.SetFragmentSize(client.ClientUdpLength - 7); // TODO: 7? calculate this based on crc enabled / compression etc
                    if (_usePingTimeout)
                    {
                        client.LastPingTimer = new Timer(
                            (e) =>
                            {
                                Emit("disconnect", client);
                            },
                            null,
                            _pingTimeoutTime,
                            Timeout.Infinite
                        );
                    }
                    packet = JObject.FromObject(
                        new
                        {
                            session_id = client.SessionId,
                            crc_seed = client.CrcSeed,
                            crc_length = client.CrcLength,
                            encrypt_method = 0,
                            udp_length = client.ServerUdpLength,
                        }
                    );

                    _SendLogicalPacket(client, "SessionReply", packet, true);
                    break;
                case "Disconnect":
                    Console.WriteLine("Received disconnect from client");
                    Emit("disconnect", client);
                    break;
                case "MultiPacket":
                    for (int i = 0; i < packet.sub_packets.Length; i++)
                    {
                        var subPacket = packet.sub_packets[i];
                        HandlePacket(client, subPacket);
                    }
                    break;
                case "Ping":
                    Console.WriteLine("Received ping from client");
                    if (_usePingTimeout)
                    {
                        client.LastPingTimer.Change(_pingTimeoutTime, Timeout.Infinite);
                    }
                    _SendLogicalPacket(client, "Ping", new JObject { }, true);
                    break;
                case "NetStatusRequest":
                    Console.WriteLine("Received net status request from client");
                    break;
                case "Data":
                    MemoryStream stream1 = new MemoryStream(packet.data);
                    byte[] data1 = new byte[stream1.Length];
                    Array.Copy(stream1.GetBuffer(), data1, data1.Length);
                    break;
                case "DataFragment":
                    MemoryStream stream2 = new MemoryStream(packet.data);
                    byte[] data2 = new byte[stream2.Length];
                    Array.Copy(stream2.GetBuffer(), data2, data2.Length);
                    break;
                case "OutOfOrder":
                    client.UnAckData.Remove(packet.sequence);
                    client.OutputStream.RemoveFromCache(packet.sequence);
                    break;
                case "Ack":
                    client.OutputStream.Ack(packet.sequence, client.UnAckData);
                    break;
                default:
                    Console.WriteLine($"Unknown SOE packet received from {client.SessionId}");
                    Console.WriteLine(packet);
                    break;
            }
        }

        public class MyClass
        {
            private Action<object> _onAction;
            public IPEndPoint remote { get; set; }
            public byte[] data { get; set; }

            public void On(object param, Action<IPEndPoint> value)
            {
                _onAction?.Invoke(param);
            }
        }

        public void Start(CrcLengthOptions? crcLength = null, int? udpLength = null)
        {
            if (crcLength != null)
            {
                _crcLength = crcLength.Value;
            }
            _protocol = new SoeProtocol(_crcLength != 0, _crcSeed);
            if (udpLength != null)
            {
                _udpLength = udpLength.Value;
            }
            // Assign a value to the _soeClientRoutineLoopMethod field using a lambda expression
            _soeClientRoutineLoopMethod = (callback, delay) =>
            {
                Timer timer = new Timer(state => callback(), null, delay, Timeout.Infinite);
            };

            // Use the _soeClientRoutineLoopMethod field here
            _soeClientRoutineLoopMethod(() => SoeRoutine(), _routineTiming);

            MyClass myClass = new MyClass();
            myClass.remote = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1115);
            myClass.data = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            myClass.On(
                "message",
                (message) =>
                {
                    byte[] data = myClass.data;
                    try
                    {
                        SOEClient client;

                        string clientId = myClass.remote + ":" + myClass.remote.Port;
                        Debug.WriteLine(data.Length + " bytes from " + clientId);
                        // if doesn't know the client
                        if (!_clients.ContainsKey(clientId))
                        {
                            if (data[1] != 1)
                            {
                                return;
                            }
                            client = _createClient(clientId, myClass.remote);

                            client.InputStream.On(
                                "appdata",
                                (data) =>
                                {
                                    Emit("appdata", client, data);
                                }
                            );

                            client.InputStream.On(
                                "error",
                                (err) =>
                                {
                                    Console.Error.WriteLine(err);
                                    Emit("disconnect", client);
                                }
                            );

                            client.InputStream.On(
                                "ack",
                                (sequence) =>
                                {
                                    ushort seq = Convert.ToUInt16(sequence);
                                    client.NextAck.Set(seq);
                                }
                            );

                            List<int> OutOfOrderPackets = new List<int>();

                            client.InputStream.On(
                                "outoforder",
                                (outOfOrderSequence) =>
                                {
                                    client.Stats.PacketsOutOfOrder++;
                                    int outOfOrderSequenceInt = Convert.ToInt32(outOfOrderSequence);
                                    OutOfOrderPackets.Add(outOfOrderSequenceInt);
                                }
                            );

                            client.OutputStream.On1(
                                "data",
                                (data, sequence, fragment, unbuffered) =>
                                {
                                    var packet = JObject.FromObject(
                                        new Dictionary<string, object>
                                        {
                                            { "sequence", sequence },
                                            { "data", data }
                                        }
                                    );

                                    _SendLogicalPacket(
                                        client,
                                        fragment ? "DataFragment" : "Data",
                                        packet,
                                        unbuffered
                                    );
                                }
                            );

                            // the only difference with the event "data" is that resended data is send via the priority queue
                            client.OutputStream.On2(
                                "dataResend",
                                (data, sequence, fragment) =>
                                {
                                    client.Stats.PacketResend++;
                                    var packet = JObject.FromObject(
                                        new Dictionary<string, object>
                                        {
                                            { "sequence", sequence },
                                            { "data", data }
                                        }
                                    );

                                    _SendLogicalPacket(
                                        client,
                                        fragment ? "DataFragment" : "Data",
                                        packet
                                    );
                                }
                            );
                        }
                        else
                        {
                            client = _clients[clientId];
                        }
                        if (data[0] == 0x00)
                        {
                            string rawParsedData = SoeProtocol.Parse(data);
                            if (rawParsedData != null)
                            {
                                Dictionary<string, object> parsedData =
                                    JsonConvert.DeserializeObject<Dictionary<string, object>>(
                                        rawParsedData
                                    );
                                if (parsedData["name"].ToString() == "Error")
                                {
                                    Console.Error.WriteLine(parsedData["error"]);
                                }
                                else
                                {
                                    HandlePacket(client, parsedData);
                                }
                            }
                            else
                            {
                                Console.Error.WriteLine(
                                    "Unmanaged packet from client " + clientId + " " + data
                                );
                            }
                        }
                        else
                        {
                            if (_allowRawDataReception)
                            {
                                Debug.WriteLine(
                                    "Raw data received from client " + clientId + " " + data
                                );
                                Emit("appdata", client, data, true); // Unreliable + Unordered
                            }
                            else
                            {
                                Debug.WriteLine(
                                    "Raw data received from client but raw data reception isn't enabled "
                                        + clientId
                                        + " "
                                        + data
                                );
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Environment.ExitCode = 1;
                    }
                }
            );
            Connection.PostMessage(new Dictionary<string, object> { { "type", "bind" } });
        }

        public void Stop()
        {
            Connection.PostMessage(new { type = "close" });
            Environment.Exit(0);
        }

        private byte[] PackLogicalData(string packetName, JObject packet)
        {
            byte[] logicalData;
            switch (packetName)
            {
                case "SessionRequest":
                    logicalData = _protocol.PackSessionRequestPacket(
                        packet["session_id"].Value<int>(),
                        packet["crc_length"].Value<int>(),
                        packet["udp_length"].Value<int>(),
                        packet["protocol"].Value<string>()
                    );
                    break;
                case "SessionReply":
                    logicalData = _protocol.PackSessionReplyPacket(
                        packet["session_id"].Value<int>(),
                        packet["crc_seed"].Value<int>(),
                        packet["crc_length"].Value<int>(),
                        packet["encrypt_method"].Value<int>(),
                        packet["udp_length"].Value<int>()
                    );
                    break;
                case "MultiPacket":
                    logicalData = _protocol.PackMultiFromjs(
                        packet.ToObject<Dictionary<string, object>>()
                    );
                    break;
                case "Ack":
                    logicalData = _protocol.PackAckPacket(packet["sequence"].Value<int>());
                    break;
                case "OutOfOrder":
                    logicalData = _protocol.PackOutOfOrderPacket(packet["sequence"].Value<int>());
                    break;
                case "Data":
                    logicalData = _protocol.PackDataPacket(
                        packet["data"].ToObject<byte[]>(),
                        packet["sequence"].Value<int>()
                    );
                    break;
                case "DataFragment":
                    logicalData = _protocol.PackFragmentDataPacket(
                        packet["data"].ToObject<byte[]>(),
                        packet["sequence"].Value<int>()
                    );
                    break;
                default:
                    logicalData = _protocol.Pack(packetName, JsonConvert.SerializeObject(packet));
                    break;
            }
            return logicalData;
        }

        // Build the logical packet via the soeprotocol
        private LogicalPacket CreateLogicalPacket(string packetName, JObject packet)
        {
            try
            {
                var logicalPacket = new LogicalPacket(
                    PackLogicalData(packetName, packet),
                    packet["sequence"].Value<int>()
                );
                return logicalPacket;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(
                    $"Failed to create {packetName} packet data : {JsonConvert.SerializeObject(packet, Formatting.Indented)}"
                );
                Console.Error.WriteLine(e);
                Environment.ExitCode = 444;
                return null;
            }
        }

        private void AddPacketToQueue(LogicalPacket logicalPacket, PacketsQueue queue)
        {
            var fullBufferedPacketLen = logicalPacket.Data.Length + 1; // the additionnal byte is the length of the packet written in the buffer when assembling the packet
            queue.Packets.Add(logicalPacket);
            queue.CurrentByteLength += fullBufferedPacketLen;
        }

        private bool CanBeBuffered(LogicalPacket logicalPacket, PacketsQueue queue)
        {
            return _waitQueueTimeMs > 0
                && logicalPacket.Data.Length < 255
                && queue.CurrentByteLength + logicalPacket.Data.Length <= _maxMultiBufferSize;
        }

        private void AddPacketToBuffer(
            SOEClient client,
            LogicalPacket logicalPacket,
            PacketsQueue queue
        )
        {
            AddPacketToQueue(logicalPacket, queue);
            if (queue.Timer == null)
            {
                queue.Timer = new Timer(
                    state => SendClientWaitQueue(client, queue),
                    null,
                    _waitQueueTimeMs,
                    Timeout.Infinite
                );
            }
        }

        // The packets is builded from schema and added to one of the queues
        private void _SendLogicalPacket(
            SOEClient client,
            string packetName,
            JObject packet,
            bool unbuffered = false
        )
        {
            var logicalPacket = CreateLogicalPacket(packetName, packet);
            if (
                !unbuffered
                && packetName != "MultiPacket"
                && CanBeBuffered(logicalPacket, (PacketsQueue)client.WaitingQueue)
            )
            {
                AddPacketToBuffer(client, logicalPacket, (PacketsQueue)client.WaitingQueue);
            }
            else
            {
                if (packetName != "MultiPacket")
                {
                    SendClientWaitQueue(client, (PacketsQueue)client.WaitingQueue);
                }
                client.OutQueue.Add(logicalPacket);
            }
        }

        // Called by the application to send data to a client
        public void SendAppData(SOEClient client, byte[] data)
        {
            if (client.OutputStream.IsUsingEncryption())
            {
                Debug.WriteLine("Sending app data: " + data.Length + " bytes with encryption");
            }
            else
            {
                Debug.WriteLine("Sending app data: " + data.Length + " bytes");
            }
            client.OutputStream.Write(data);
        }

        public void SendUnbufferedAppData(SOEClient client, byte[] data)
        {
            if (client.OutputStream.IsUsingEncryption())
            {
                Debug.WriteLine(
                    "Sending unbuffered app data: " + data.Length + " bytes with encryption"
                );
            }
            else
            {
                Debug.WriteLine("Sending unbuffered app data: " + data.Length + " bytes");
            }
            client.OutputStream.Write(data, true);
        }

        public void SetEncryption(SOEClient client, bool value)
        {
            client.OutputStream.SetEncryption(value);
            client.InputStream.setEncryption(value);
        }

        public void ToggleEncryption(SOEClient client)
        {
            client.OutputStream.ToggleEncryption();
            client.InputStream.toggleEncryption();
        }

        public void DeleteClient(SOEClient client)
        {
            _clients.Remove(client.Address + ":" + client.Port);
            Debug.WriteLine("client connection from port : " + client.Port + " deleted");
        }
    }
}
