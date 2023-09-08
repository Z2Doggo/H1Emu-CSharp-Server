using Data.DataSchema;

namespace Protocols.H1Emu
{
    public class H1EmuProtocol
    {
        public class Field
        {
            public string? name;
            public dynamic? type;
            public object? defaultValue;
        }

        public class Packet
        {
            public string? name;
            public dynamic? id;
            public List<Field> fields;
        }

        public class H1emuPackets
        {
            public class PacketTypes
            {
                public static readonly byte SessionRequest = 0x01;
                public static readonly byte SessionReply = 0x02;
                public static readonly byte Ping = 0x03;
                public static readonly byte Ack = 0x04;
                public static readonly byte CharacterCreateRequest = 0x05;
                public static readonly byte CharacterCreateReply = 0x06;
                public static readonly byte CharacterDeleteRequest = 0x07;
                public static readonly byte CharacterDeleteReply = 0x08;
                public static readonly byte UpdateZonePopulation = 0x09;
                public static readonly byte ZonePingRequest = 0x10;
                public static readonly byte ZonePingReply = 0x11;
                public static readonly byte CharacterExistRequest = 0x12;
                public static readonly byte CharacterExistReply = 0x13;
            }

            public class Packets
            {
                public static readonly Packet[] packets = new Packet[]
                {
                    new() {
                        name = "SessionRequest",
                        id = PacketTypes.SessionRequest,
                        fields =
                        {
                            new Field { name = "serverId", type = "uint32", defaultValue = 0 },
                            new Field { name = "h1emuVersion", type = "string", defaultValue = "" },
                        },
                    },
                    new() {
                        name = "SessionReply",
                        id = PacketTypes.SessionReply,
                        fields =
                        {
                            new() { name = "status", type = "uint8", defaultValue = 0 },
                        },
                    },
                    new()
                    {
                        name = "Ping",
                        id = PacketTypes.Ping,
                        fields = {},
                    },
                    new() { name = "Ack", id = PacketTypes.Ack, fields = new List<Field>(), },
                    new()
                    {
                        name = "CharacterCreateRequest",
                        id = PacketTypes.CharacterCreateRequest,
                        fields =
                        {
                            new Field { name = "reqId", type = "uint32", defaultValue = 0 },
                            new Field
                            {
                                name = "characterObjStringify",
                                type = "string",
                                defaultValue = ""
                            },
                        },
                    },
                    new()
                    {
                        name = "CharacterCreateReply",
                        id = PacketTypes.CharacterCreateReply,
                        fields =
                        {
                            new Field { name = "reqId", type = "uint32", defaultValue = 0 },
                            new Field { name = "status", type = "boolean", defaultValue = 0 },
                        },
                    },
                    new()
                    {
                        name = "CharacterDeleteRequest",
                        id = PacketTypes.CharacterDeleteRequest,
                        fields =
                        {
                            new Field { name = "reqId", type = "uint32", defaultValue = 0 },
                            new Field
                            {
                                name = "characterId",
                                type = "uint64string",
                                defaultValue = 0
                            },
                        },
                    },
                    new()
                    {
                        name = "CharacterDeleteReply",
                        id = PacketTypes.CharacterDeleteReply,
                        fields =
                        {
                            new Field { name = "reqId", type = "uint32", defaultValue = 0 },
                            new Field { name = "status", type = "boolean", defaultValue = 0 },
                        },
                    },
                    new()
                    {
                        name = "UpdateZonePopulation",
                        id = PacketTypes.UpdateZonePopulation,
                        fields =
                        {
                            new Field { name = "population", type = "uint8", defaultValue = 0 },
                        },
                    },
                    new()
                    {
                        name = "ZonePingRequest",
                        id = PacketTypes.ZonePingRequest,
                        fields =
                        {
                            new Field { name = "reqId", type = "uint32", defaultValue = 0 },
                            new Field { name = "address", type = "string", defaultValue = 0 },
                        },
                    },
                    new()
                    {
                        name = "ZonePingReply",
                        id = PacketTypes.ZonePingReply,
                        fields =
                        {
                            new Field { name = "reqId", type = "uint32", defaultValue = 0 },
                            new Field { name = "status", type = "boolean", defaultValue = 0 },
                        },
                    },
                    new()
                    {
                        name = "CharacterExistRequest",
                        id = PacketTypes.CharacterExistRequest,
                        fields =
                        {
                            new Field { name = "reqId", type = "uint32", defaultValue = 0 },
                            new Field
                            {
                                name = "characterId",
                                type = "uint64string",
                                defaultValue = 0
                            },
                        },
                    },
                    new() {
                        name = "CharacterExistReply",
                        id = PacketTypes.CharacterExistReply,
                        fields =
                        {
                            new Field { name = "reqId", type = "uint32", defaultValue = 0 },
                            new Field { name = "status", type = "boolean", defaultValue = 0 },
                        },
                    },
                };
            };
        };
    };
};