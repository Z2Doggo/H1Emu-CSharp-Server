namespace DataSchema.DataSchema
{
    public static class ExtensionMethods
    {
        public static uint ReadUInt24LE(this byte[] buffer, int offset, bool noAssert = false)
        {
            var value = buffer[offset] + (buffer[offset + 1] << 8) + (buffer[offset + 2] << 16);
            return (uint)Convert.ToInt32(value);
        }

        public static void WriteUInt24LE(this byte[] buffer, uint value, int offset, bool noAssert = false)
        {
            buffer[offset] = (byte)(value & 0xFF);
            buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
            buffer[offset + 2] = (byte)((value >> 16) & 0xFF);
        }

        public static uint ReadUInt24BE(this byte[] buffer, int offset, bool noAssert = false)
        {
            var value = (buffer[offset] << 16) + (buffer[offset + 1] << 8) + buffer[offset + 2];
            return (uint)Convert.ToInt32(value);
        }

        public static void WriteUInt24BE(this byte[] buffer, uint value, int offset, bool noAssert = false)
        {
            buffer[offset] = (byte)((value >> 16) & 0xFF);
            buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
            buffer[offset + 2] = (byte)(value & 0xFF);
        }

        public static string ReadPrefixedStringLE(this byte[] buffer, int offset, string encoding = "utf8", bool noAssert = false)
        {
            var length = BitConverter.ToUInt32(buffer, offset);
            var value = System.Text.Encoding.GetEncoding(encoding).GetString(buffer, offset + 4, offset);
            return value;
        }

        public static void WritePrefixedStringLE(this byte[] buffer, string String, int offset, string encoding = "utf8")
        {
            var lengthBytes = BitConverter.GetBytes((uint)String.Length);
            Array.Copy(lengthBytes, 0, buffer, offset, 4);
            var stringBytes = System.Text.Encoding.GetEncoding(encoding).GetBytes(String);
            Array.Copy(stringBytes, 0, buffer, offset + 4, encoding.Length);
        }

        public static string ReadPrefixedStringBE(this byte[] buffer, int offset, string encoding = "utf8", bool noAssert = false)
        {
            var length = BitConverter.ToUInt32(buffer, offset);
            var value = System.Text.Encoding.GetEncoding(encoding).GetString(buffer, offset + 4, offset);
            return value;
        }
        public static void WritePrefixedStringBE(this byte[] buffer, string String, int offset, string encoding = "utf8")
        {
            var lengthBytes = BitConverter.GetBytes((uint)String.Length);
            Array.Reverse(lengthBytes);
            Array.Copy(lengthBytes, 0, buffer, offset, 4);
            var stringBytes = System.Text.Encoding.GetEncoding(encoding).GetBytes(String);
            Array.Copy(stringBytes, 0, buffer, offset + 4, encoding.Length);
        }

        public static string ReadNullTerminatedString(this byte[] buffer, int offset)
        {
            var value = "";
            for (int i = offset; i < buffer.Length; i++)
            {
                if (buffer[i] == 0)
                {
                    break;
                }
                value += (char)buffer[i];
            }
            return value;
        }

        public static void WriteNullTerminatedString(this byte[] buffer, string String, int offset)
        {
            for (int i = 0; i < String.Length; i++)
            {
                buffer[offset + i] = (byte)String[i];
            }
            buffer[offset + String.Length] = 0;
        }

        public static bool ReadBoolean(this byte[] buffer, int offset, bool noAssert = false)
        {
            var value = buffer[offset];
            return value != 0;
        }

        public static void WriteBoolean(this byte[] buffer, bool value, int offset, bool noAssert = false)
        {
            buffer[offset] = value ? (byte)1 : (byte)0;
        }

        public static byte[] ReadBytes(this byte[] buffer, int offset, int length)
        {
            var dst = new byte[length];
            Array.Copy(buffer, offset, dst, 0, length);
            return dst;
        }
    }
}