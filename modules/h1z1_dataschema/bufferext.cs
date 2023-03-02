namespace DataSchema.Bufferext
{
    public static class ExtensionMethods
    {
        public static uint ReadUInt24LE(this byte[] buffer, int offset, bool NoAssert = false)
        {
            return BitConverter.ToUInt32(
                new byte[] { buffer[offset], buffer[offset + 1], buffer[offset + 2], 0 },
                0
            );
        }

        public static void WriteUInt24LE(
            this byte[] buffer,
            uint value,
            int offset,
            bool NoAssert = false
        )
        {
            buffer[offset] = (byte)(value & 0xFF);
            buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
            buffer[offset + 2] = (byte)((value >> 16) & 0xFF);
        }

        public static uint ReadUInt24BE(this byte[] buffer, int offset, bool NoAssert = false)
        {
            return BitConverter.ToUInt32(
                new byte[] { 0, buffer[offset], buffer[offset + 1], buffer[offset + 2] },
                0
            );
        }

        public static void WriteUInt24BE(
            this byte[] buffer,
            uint value,
            int offset,
            bool NoAssert = false
        )
        {
            buffer[offset] = (byte)((value >> 16) & 0xFF);
            buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
            buffer[offset + 2] = (byte)(value & 0xFF);
        }

        public static string ReadPrefixedStringLE(
            this byte[] buffer,
            int offset,
            string encoding = "utf8",
            bool NoAssert = false
        )
        {
            var Length = BitConverter.ToUInt32(buffer, offset);
            var value = System.Text.Encoding
                .GetEncoding(encoding)
                .GetString(buffer, offset + 4, (int)Length);
            return value;
        }
        public static void WritePrefixedStringLE(
            this byte[] buffer,
            string String,
            int offset,
            string encoding = "utf8"
        )
        {
            var lengthBytes = BitConverter.GetBytes((uint)String.Length);
            Array.Copy(lengthBytes, 0, buffer, offset, 4);
            var stringBytes = System.Text.Encoding.GetEncoding(encoding).GetBytes(String);
            Array.Copy(stringBytes, 0, buffer, offset + 4, String.Length);
        }

        public static string ReadPrefixedStringBE(
            this byte[] buffer,
            int offset,
            string encoding = "utf8",
            bool NoAssert = false
        )
        {
            var Length = BitConverter.ToUInt32(buffer, offset);
            var value = System.Text.Encoding
                .GetEncoding(encoding)
                .GetString(buffer, offset + 4, (int)Length);
            return value;
        }
        public static void WritePrefixedStringBE(
            this byte[] buffer,
            string String,
            int offset,
            string encoding = "utf8"
        )
        {
            var lengthBytes = BitConverter.GetBytes((uint)String.Length);
            Array.Reverse(lengthBytes);
            Array.Copy(lengthBytes, 0, buffer, offset, 4);
            var stringBytes = System.Text.Encoding.GetEncoding(encoding).GetBytes(String);
            Array.Copy(stringBytes, 0, buffer, offset + 4, String.Length);
        }

        public static string ReadNullTerminatedString(this byte[] buffer, int offset)
        {
            return System.Text.Encoding.ASCII
                .GetString(buffer, offset, buffer.Length - offset)
                .TrimEnd('\0');
        }

        public static void WriteNullTerminatedString(this byte[] buffer, string String, int offset)
        {
            if (offset + String.Length >= buffer.Length)
            {
                throw new ArgumentOutOfRangeException("String is too long to fit in the buffer.");
            }

            for (int i = 0; i < String.Length; i++)
            {
                buffer[offset + i] = (byte)String[i];
            }
            buffer[offset + String.Length] = 0;
        }

        public static bool ReadBoolean(this byte[] buffer, int offset, bool NoAssert = false)
        {
            return buffer[offset] != 0;
        }

        public static void WriteBoolean(this byte[] buffer, bool value, int offset)
        {
            buffer[offset] = (byte)(value ? 1 : 0);
        }
    }
}