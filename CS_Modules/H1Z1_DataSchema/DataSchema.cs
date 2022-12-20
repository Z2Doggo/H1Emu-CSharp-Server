namespace DataSchema.DataSchema
{
    public static class ExtensionMethods
    {
        public static uint ReadUInt24LE(this byte[] Buffer, int Offset)
        {
            var Value = Buffer[Offset] + (Buffer[Offset + 1] << 8) + (Buffer[Offset + 2] << 16);
            return (uint)Convert.ToInt32(Value);
        }

        public static void WriteUInt24LE(this byte[] Buffer, uint Value, int Offset)
        {
            Buffer[Offset] = (byte)(Value & 0xFF);
            Buffer[Offset + 1] = (byte)((Value >> 8) & 0xFF);
            Buffer[Offset + 2] = (byte)((Value >> 16) & 0xFF);
        }

        public static uint ReadUInt24BE(this byte[] Buffer, int Offset)
        {
            var Value = (Buffer[Offset] << 16) + (Buffer[Offset + 1] << 8) + Buffer[Offset + 2];
            return (uint)Convert.ToInt32(Value);
        }

        public static void WriteUInt24BE(this byte[] Buffer, uint Value, int Offset)
        {
            Buffer[Offset] = (byte)((Value >> 16) & 0xFF);
            Buffer[Offset + 1] = (byte)((Value >> 8) & 0xFF);
            Buffer[Offset + 2] = (byte)(Value & 0xFF);
        }

        public static string ReadPrefixedStringLE(this byte[] Buffer, int Offset, string Encoding = "utf8")
        {
            BitConverter.ToUInt32(Buffer, Offset);
            var Value = System.Text.Encoding.GetEncoding(Encoding).GetString(Buffer, Offset + 4, Offset);
            return Value;
        }

        public static void WritePrefixedStringLE(this byte[] Buffer, string String, int Offset, string Encoding = "utf8")
        {
            var LengthBytes = BitConverter.GetBytes((uint)String.Length);
            Array.Copy(LengthBytes, 0, Buffer, Offset, 4);
            var StringBytes = System.Text.Encoding.GetEncoding(Encoding).GetBytes(String);
            Array.Copy(StringBytes, 0, Buffer, Offset + 4, Encoding.Length);
        }

        public static string ReadPrefixedStringBE(this byte[] Buffer, int Offset, string Encoding = "utf8")
        {
            BitConverter.ToUInt32(Buffer, Offset);
            var Value = System.Text.Encoding.GetEncoding(Encoding).GetString(Buffer, Offset + 4, Offset);
            return Value;
        }
        public static void WritePrefixedStringBE(this byte[] Buffer, string String, int Offset, string Encoding = "utf8")
        {
            var LengthBytes = BitConverter.GetBytes((uint)String.Length);
            Array.Reverse(LengthBytes);
            Array.Copy(LengthBytes, 0, Buffer, Offset, 4);
            var StringBytes = System.Text.Encoding.GetEncoding(Encoding).GetBytes(String);
            Array.Copy(StringBytes, 0, Buffer, Offset + 4, Encoding.Length);
        }

        public static string ReadNullTerminatedString(this byte[] Buffer, int Offset)
        {
            var Value = "";
            for (int i = Offset; i < Buffer.Length; i++)
            {
                if (Buffer[i] == 0)
                {
                    break;
                }
                Value += (char)Buffer[i];
            }
            return Value;
        }

        public static void WriteNullTerminatedString(this byte[] Buffer, string String, int Offset)
        {
            for (int i = 0; i < String.Length; i++)
            {
                Buffer[Offset + i] = (byte)String[i];
            }
            Buffer[Offset + String.Length] = 0;
        }

        public static bool ReadBoolean(this byte[] Buffer, int Offset)
        {
            var Value = Buffer[Offset];
            return Value != 0;
        }

        public static void WriteBoolean(this byte[] Buffer, bool Value, int Offset)
        {
            Buffer[Offset] = Value ? (byte)1 : (byte)0;
        }

        public static byte[] ReadBytes(this byte[] Buffer, int Offset, int length)
        {
            var dst = new byte[length];
            Array.Copy(Buffer, Offset, dst, 0, length);
            return dst;
        }
    }
}