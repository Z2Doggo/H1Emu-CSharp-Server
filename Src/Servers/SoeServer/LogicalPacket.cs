// TODO... FIX THIS CODE

namespace Soe.LogicalPacket
{
    public class LogicalPacket
    {
        int? Sequence { get; set; }
        byte[]? Data { get; set; }
        bool IsReliable { get; set; }
        public static void Constructor(byte[] Data, int? Sequence = null)
        {
            bool IsReliable = Data[1] == 9 || Data[1] == 13;
        }
    }
}