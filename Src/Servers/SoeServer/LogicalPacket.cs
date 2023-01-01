namespace Servers.LogicalPacket
{
    public class LogicalPacket
    {
        public int Sequence { get; set; }
        public byte[] Data { get; set; }
        public bool IsReliable { get; set; }

        public LogicalPacket(byte[] data, int? sequence = null)
        {
            Sequence = (int)sequence;
            Data = data;
            IsReliable = data[1] == 9 || data[1] == 13;
        }
    }
}