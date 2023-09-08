namespace Packets.PacketTable
{
    public class PacketTableBuild
    {
        public static Tuple<Dictionary<string, uint>, Dictionary<uint, PacketDescriptor>> Run(object[][] packets)
        {
            var packetTypes = new Dictionary<string,
              uint>();
            var packetDescriptors = new Dictionary<uint,
              PacketDescriptor>();
            for (var i = 0; i < packets.Length; i++)
            {
                var packet = packets[i];
                var name = packet[0].ToString();
                var type = (uint)packet[1];
                var packetDesc = (Dictionary<string, object>)packet[2];
                packetTypes[name] = type;
                packetDescriptors[type] = new PacketDescriptor
                {
                    Type = type,
                    Name = name,
                    Schema = (object[])packetDesc["fields"],
                    Fn = (Func<object, object>)packetDesc["fn"],
                    Parse = (Func<object, object>)packetDesc["parse"],
                    Pack = (Func<object, object>)packetDesc["pack"]
                };
            }
            return Tuple.Create(packetTypes, packetDescriptors);
        }

        public class PacketDescriptor
        {
            public uint Type
            {
                get;
                set;
            }
            public string? Name
            {
                get;
                set;
            }
            public object[]? Schema
            {
                get;
                set;
            }
            public Func<object, object>? Fn
            {
                get;
                set;
            }
            public Func<object, object>? Parse
            {
                get;
                set;
            }
            public Func<object, object>? Pack
            {
                get;
                set;
            }

        }
    }
}