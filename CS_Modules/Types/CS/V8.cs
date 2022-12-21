namespace CS_Modules.V8
{
    public class V8
    {
        public interface IHeapSpaceInfo
        {
            string Space_Name { get; set; }
            int Space_Size { get; set; }
            int Space_Used_Size { get; set; }
            int Space_Available_Size { get; set; }
            int Physical_Space_Size { get; set; }
        }

        public enum DoesZapCodeSpaceFlag
        {
            Zero = 0,
            One = 1
        }

        public interface IHeapInfo
        {
            int Total_Heap_Size { get; set; }
            int Total_Heap_Size_Executable { get; set; }
            int Total_Physical_Size { get; set; }
            int Total_Available_Size { get; set; }
            int Used_Heap_Size { get; set; }
            int Heap_Size_Limit { get; set; }
            int MAlloced_Memory { get; set; }
            int Peak_MAlloced_Memory { get; set; }
            DoesZapCodeSpaceFlag Does_Zap_Garbage { get; set; }
            int Number_Of_Native_Contexts { get; set; }
            int Number_Of_Detached_Contexts { get; set; }
        }

        public interface IHeapCodeStatistics
        {
            int Code_And_Metadata_Size { get; set; }
            int Bytecode_And_Metadata_size { get; set; }
            int External_Script_Source_Size { get; set; }
        }

        public static int CachedDataVersionTag()
        {
            return 0;
        }

        public static IHeapInfo GetHeapStatistics(long HeapSize, long PeakHeapSize)
        {
            HeapSize = GC.GetTotalMemory(false); 
            PeakHeapSize = GC.GetTotalMemory(true);

            return GetHeapStatistics(HeapSize, PeakHeapSize);
        }

        public static IHeapSpaceInfo[] GetHeapSpaceStatistics()
        {
            return GetHeapSpaceStatistics();
        }

        public static void SetFlagsFromStrings(string Flags)
        {
            // empty...
        }

        public StreamReader GetHeapSnapshot()
        {
            return GetHeapSnapshot();
        }

        public static string WriteHeapSnapshot(string? Filename)
        {
            return Filename;
        }

        public static IHeapCodeStatistics GetHeapCodeStatistics()
        {
            return GetHeapCodeStatistics();
        }

        public class Serializer
        {
            public void WriteHeader()
            {
                // empty...
            }

            public dynamic WriteValue(dynamic Val)
            {
                return false || true;
            }

            public byte[] ReleaseBuffer()
            {
                return ReleaseBuffer();
            }

            public void TransferArrayBuffer(int Id, byte[] ArrayBuffer)
            {
                // empty...
            }

            public void WriteUInt32(int Value)
            {
                // empty...
            }

            public void WriteUInt64(int Hi, int Lo)
            {
                // empty...
            }

            public void WriteDouble(int Value)
            {
                // empty...
            }

            public void WriteRawBytes(byte[] Buffer)
            {
                // empty...
            }
        }

        public class DefaultSerializer : Serializer { }

        public class Deserializer
        {
            interface IConstructor
            {
                // empty for now... gotta figure how to translate this one...
            }


        }
    }
}