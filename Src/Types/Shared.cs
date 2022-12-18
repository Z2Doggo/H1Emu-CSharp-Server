namespace SharedTypes
{
    public interface IPacket
    {
        dynamic Result { get; }
        string Name { get; }
        dynamic TunnelData { get; }
        dynamic Flags { get; }
    }

    public interface IHttpServerMessage
    {
        string Type { get; }
        int RequestId { get; }
        dynamic Data { get; }
    }

    public class Json
    {
        public dynamic? Value { get;}
    }
}