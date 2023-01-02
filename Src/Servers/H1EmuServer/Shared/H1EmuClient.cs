namespace Shared.H1EmuClient
{
    using System.Net;

    class H1emuClient
    {
        public int sessionId { get; set; } = 0;
        public string address { get; set; }
        public int port { get; set; }
        public int? serverId { get; set; }
        public string clientId { get; set; }
        public int lastPing { get; set; } = 0;

        public H1emuClient(IPEndPoint remote)
        {
            this.address = remote.Address.ToString();
            this.port = remote.Port;
            this.clientId = $"{remote.Address}:{remote.Port}";
        }
    }
}