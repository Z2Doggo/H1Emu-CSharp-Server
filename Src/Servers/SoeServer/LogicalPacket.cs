namespace Servers.LogicalPacket
{
    public class LogicalPacket
    {
        public int? Sequence { get; }
        public byte[] Data { get; }
        public bool IsReliable { get; }

        public LogicalPacket(byte[] data, int? sequence = null)
        {
            Sequence = sequence;
            Data = data;
            IsReliable = data[1] == 9 || data[1] == 13;
        }
    }
}