using System.Text;
using Newtonsoft.Json;

namespace H1EmuCore
{
    public class Core
    {
        public static uint Joaat(string str)
        {
            uint hash = 0;
            for (int i = 0; i < str.Length; i++)
            {
                hash += str[i];
                hash += (hash << 10);
                hash ^= (hash >> 6);
            }
            hash += (hash << 3);
            hash ^= (hash >> 11);
            hash += (hash << 15);
            return hash;
        }

        public static string GenerateRandomGuid()
        {
            return Guid.NewGuid().ToString();
        }

        public static float[] Eul2Quat(float[] angle)
        {
            float c1 = (float)Math.Cos(angle[0] / 2);
            float c2 = (float)Math.Cos(angle[1] / 2);
            float c3 = (float)Math.Cos(angle[2] / 2);
            float s1 = (float)Math.Sin(angle[0] / 2);
            float s2 = (float)Math.Sin(angle[1] / 2);
            float s3 = (float)Math.Sin(angle[2] / 2);

            float[] quat = new float[4];
            quat[0] = s1 * c2 * c3 + c1 * s2 * s3;
            quat[1] = c1 * s2 * c3 - s1 * c2 * s3;
            quat[2] = c1 * c2 * s3 - s1 * s2 * c3;
            quat[3] = c1 * c2 * c3 + s1 * s2 * s3;
            return quat;
        }

        public static bool IsPosInRadius(float radius, float[] playerPos, float[] enemyPos)
        {
            float xDiff = playerPos[0] - enemyPos[0];
            float yDiff = playerPos[1] - enemyPos[1];
            float zDiff = playerPos[2] - enemyPos[2];
            float distance = (float)Math.Sqrt(xDiff * xDiff + yDiff * yDiff + zDiff * zDiff);
            return distance <= radius;
        }

        public byte[] AppendCrcLegacy(byte[] data, uint crcSeed)
        {
            uint crc = Crc32Legacy(data, crcSeed);
            byte[] crcData = BitConverter.GetBytes(crc);
            byte[] result = new byte[data.Length + crcData.Length];
            Buffer.BlockCopy(data, 0, result, 0, data.Length);
            Buffer.BlockCopy(crcData, 0, result, data.Length, crcData.Length);
            return result;
        }

        public static uint Crc32Legacy(byte[] data, uint crcSeed)
        {
            uint num1 = uint.MaxValue;
            uint num2 = uint.MaxValue;
            for (int index = 0; index < data.Length; ++index)
            {
                byte num3 = data[index];
                num1 = crcSeed ^ (uint)num3;
                crcSeed = crcSeed >> 8 ^ num2 * num1;
            }
            return crcSeed ^ uint.MaxValue;
        }
    }

    public enum EncryptMethod
    {
        EncryptMethodNone,
        EncryptMethodUserSupplied,
        EncryptMethodUserSupplied2,
        EncryptMethodXorBuffer,
        EncryptMethodXor
    }

    public class RC4 : IDisposable
    {
        private readonly byte[] _key;
        private readonly byte[] _s;
        private int _i;
        private int _j;

        public RC4(byte[] givenKey)
        {
            _key = givenKey;
            _s = new byte[256];
            for (int index = 0; index < _s.Length; ++index)
                _s[index] = (byte)index;
            for (int index = 0, j = 0; index < _s.Length; ++index)
            {
                j = (j + _s[index] + _key[index % _key.Length]) % _s.Length;
                Swap(ref _s[index], ref _s[j]);
            }
            _i = 0;
            _j = 0;
        }

        public void Dispose()
        {
            Array.Clear(_key, 0, _key.Length);
            Array.Clear(_s, 0, _s.Length);
            _i = 0;
            _j = 0;
        }

        public byte[] Encrypt(byte[] data)
        {
            byte[] numArray = new byte[data.Length];
            for (int index = 0; index < data.Length; ++index)
            {
                _i = (_i + 1) % _s.Length;
                _j = (_j + _s[_i]) % _s.Length;
                Swap(ref _s[_i], ref _s[_j]);
                int num = _s[(_s[_i] + _s[_j]) % _s.Length];
                numArray[index] = (byte)(data[index] ^ num);
            }
            return numArray;
        }

        public byte[] Decrypt(byte[] data)
        {
            return Encrypt(data);
        }

        private void Swap(ref byte a, ref byte b)
        {
            byte num = a;
            a = b;
            b = num;
        }
    }

    public class SoeProtocol
    {
        public void Free()
        {
        }

        // TEMPORARY WORK AROUND FOR NOW...
        public SoeProtocol(bool use_crc, int crc_seed)
        {
            // empty...
        }

        public byte[] Pack(string packet_name, string packet)
        {
            return Encoding.UTF8.GetBytes(packet);
        }

        public byte[] PackSessionRequest(string packet)
        {
            return Encoding.UTF8.GetBytes(packet);
        }

        public byte[] PackSessionRequestFromjs(string js_object)
        {
            return Encoding.UTF8.GetBytes(js_object.ToString());
        }

        public byte[] PackSessionRequestPacket(int session_id, int crc_length, int udp_length, string protocol)
        {
            return Encoding.UTF8.GetBytes(string.Format("{0},{1},{2},{3}", session_id, crc_length, udp_length, protocol));
        }

        public byte[] PackSessionReply(string packet)
        {
            return Encoding.UTF8.GetBytes(packet);
        }

        public byte[] PackSessionReplyFromjs(string js_object)
        {
            return Encoding.UTF8.GetBytes(js_object.ToString());
        }

        public byte[] PackSessionReplyPacket(int session_id, int crc_seed, int crc_length, int encrypt_method, int udp_length)
        {
            return Encoding.UTF8.GetBytes(string.Format("{0},{1},{2},{3},{4}", session_id, crc_seed, crc_length, encrypt_method, udp_length));
        }

        public byte[] PackNetStatusRequest(string packet)
        {
            return Encoding.UTF8.GetBytes(packet);
        }

        public byte[] PackNetStatusRequestFromjs(string js_object)
        {
            return Encoding.UTF8.GetBytes(js_object.ToString());
        }

        public byte[] PackNetStatusReply(string packet)
        {
            return Encoding.UTF8.GetBytes(packet);
        }

        public byte[] PackNetStatusReplyFromjs(string js_object)
        {
            return Encoding.UTF8.GetBytes(js_object.ToString());
        }

        public byte[] PackMulti(string packet)
        {
            return Encoding.UTF8.GetBytes(packet);
        }

        public byte[] PackMultiFromjs(string js_object)
        {
            return Encoding.UTF8.GetBytes(js_object.ToString());
        }

        public byte[] PackGroup(string packet)
        {
            return Encoding.UTF8.GetBytes(packet);
        }

        public byte[] PackGroupFromjs(string js_object)
        {
            return Encoding.UTF8.GetBytes(js_object.ToString());
        }

        public byte[] PackData(string packet)
        {
            return Encoding.UTF8.GetBytes(packet);
        }

        public byte[] PackDataFromJS(object jsObject)
        {
            string json = JsonConvert.SerializeObject(jsObject);
            return Encoding.UTF8.GetBytes(json);
        }

        public byte[] PackDataPacket(byte[] data, int sequence)
        {
            var result = new List<byte>();
            result.AddRange(BitConverter.GetBytes(sequence));
            result.AddRange(data);
            return result.ToArray();
        }

        public byte[] PackFragmentData(string packet)
        {
            return Encoding.UTF8.GetBytes(packet);
        }

        public byte[] PackFragmentDataFromJS(object jsObject)
        {
            string json = JsonConvert.SerializeObject(jsObject);
            return Encoding.UTF8.GetBytes(json);
        }

        public byte[] PackFragmentDataPacket(byte[] data, int sequence)
        {
            var result = new List<byte>();
            result.AddRange(BitConverter.GetBytes(sequence));
            result.AddRange(data);
            return result.ToArray();
        }

        public byte[] PackOutOfOrder(string packet)
        {
            return Encoding.UTF8.GetBytes(packet);
        }

        public byte[] PackOutOfOrderFromJS(object jsObject)
        {
            string json = JsonConvert.SerializeObject(jsObject);
            return Encoding.UTF8.GetBytes(json);
        }

        public byte[] PackOutOfOrderPacket(int sequence)
        {
            return BitConverter.GetBytes(sequence);
        }

        public byte[] PackAck(string packet)
        {
            return Encoding.UTF8.GetBytes(packet);
        }

        public byte[] PackAckFromJS(object jsObject)
        {
            string json = JsonConvert.SerializeObject(jsObject);
            return Encoding.UTF8.GetBytes(json);
        }

        public byte[] PackAckPacket(int sequence)
        {
            return BitConverter.GetBytes(sequence);
        }

        public static string Parse(byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }

        private int crcSeed = 0;
        private bool useCrc = true;

        public int GetCrcSeed()
        {
            return crcSeed;
        }

        public bool IsUsingCrc()
        {
            return useCrc;
        }

        public void DisableCrc()
        {
            useCrc = false;
        }

        public void EnableCrc()
        {
            useCrc = true;
        }

        internal byte[] PackMultiFromjs(Dictionary<string, object>? dictionary)
        {
            throw new NotImplementedException();
        }
    }
}

