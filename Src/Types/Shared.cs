namespace SharedTypes
{
    public interface IPacket
    {
        dynamic Result { get; set; }
        string Name { get; set; }
        dynamic TunnelData { get; set; }
        dynamic Flags { get; set; }
    }

    public interface IHttpServerMessage
    {
        string Type { get; set; }
        int RequestId { get; set; }
        dynamic Data { get; set; }
    }

    public class Json
    {
        public dynamic? Value { get; }
    }
}