namespace SoeServerTypes
{
    public class SoePacket
    {
        public dynamic? Value { get; set; }
    }

    public static class CrcLengthOptions
    {
        public const int Zero = 0;
        public const int Two = 2;
    }

    public class DataCache : Dictionary<int, Tuple<byte[]?, bool>>
    {
    }
}