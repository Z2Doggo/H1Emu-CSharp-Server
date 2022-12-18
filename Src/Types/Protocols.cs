namespace ProtocolTypes
{
    public interface ILoginProtocolReadingFormat
    {
        int ServerId { get; set; }
        int Unknown { get; set; }
        string SubPacketName { get; set; }
        int PacketLength { get; set; }
        string Name { get; set; }
        dynamic Result { get; set; }
        int Type { get; set; }
    }

    public interface IH1Z1ProtocolReadingFormat
    {
        string Name { get; set; }
        dynamic Data { get; set; }
    }

    public interface IGatewayProtocolReadingFormat
    {
        int Type { get; set; }
        int Flags { get; set; }
        bool FromClient { get; set; }
        byte[] TunnelData { get; set; }
        string Name { get; set; }
        dynamic Result { get; set; }
    }

    public interface IH1emuProtocolReadingFormat
    {
        int Type { get; set; }
        string Name { get; set; }
        dynamic Data { get; set; }
    }
}