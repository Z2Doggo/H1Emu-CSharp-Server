namespace Protocols.H1Emu
{
    using System.Collections.Generic;

    public class H1EmuProtocol
    {
        public class Field
        {
            public string name;
            public string type;
            public object defaultValue;
        }

        public class Packet
        {
            public string name;
            public byte id;
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
                    new Packet
                    {
                        name = "SessionRequest",
                        id = PacketTypes.SessionRequest,
                        fields = new List<Field>
                        {
                            new Field { name = "serverId", type = "uint32", defaultValue = 0 },
                            new Field { name = "h1emuVersion", type = "string", defaultValue = "" },
                        },
                    },
                    new Packet
                    {
                        name = "SessionReply",
                        id = PacketTypes.SessionReply,
                        fields = new List<Field>
                        {
                            new Field { name = "status", type = "uint8", defaultValue = 0 },
                        },
                    },
                    new Packet
                    {
                        name = "Ping",
                        id = PacketTypes.Ping,
                        fields = new List<Field>(),
                    },
                    new Packet { name = "Ack", id = PacketTypes.Ack, fields = new List<Field>(), },
                    new Packet
                    {
                        name = "CharacterCreateRequest",
                        id = PacketTypes.CharacterCreateRequest,
                        fields = new List<Field>
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
                    new Packet
                    {
                        name = "CharacterCreateReply",
                        id = PacketTypes.CharacterCreateReply,
                        fields = new List<Field>
                        {
                            new Field { name = "reqId", type = "uint32", defaultValue = 0 },
                            new Field { name = "status", type = "boolean", defaultValue = 0 },
                        },
                    },
                    new Packet
                    {
                        name = "CharacterDeleteRequest",
                        id = PacketTypes.CharacterDeleteRequest,
                        fields = new List<Field>
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
                    new Packet
                    {
                        name = "CharacterDeleteReply",
                        id = PacketTypes.CharacterDeleteReply,
                        fields = new List<Field>
                        {
                            new Field { name = "reqId", type = "uint32", defaultValue = 0 },
                            new Field { name = "status", type = "boolean", defaultValue = 0 },
                        },
                    },
                    new Packet
                    {
                        name = "UpdateZonePopulation",
                        id = PacketTypes.UpdateZonePopulation,
                        fields = new List<Field>
                        {
                            new Field { name = "population", type = "uint8", defaultValue = 0 },
                        },
                    },
                    new Packet
                    {
                        name = "ZonePingRequest",
                        id = PacketTypes.ZonePingRequest,
                        fields = new List<Field>
                        {
                            new Field { name = "reqId", type = "uint32", defaultValue = 0 },
                            new Field { name = "address", type = "string", defaultValue = 0 },
                        },
                    },
                    new Packet
                    {
                        name = "ZonePingReply",
                        id = PacketTypes.ZonePingReply,
                        fields = new List<Field>
                        {
                            new Field { name = "reqId", type = "uint32", defaultValue = 0 },
                            new Field { name = "status", type = "boolean", defaultValue = 0 },
                        },
                    },
                    new Packet
                    {
                        name = "CharacterExistRequest",
                        id = PacketTypes.CharacterExistRequest,
                        fields = new List<Field>
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
                    new Packet
                    {
                        name = "CharacterExistReply",
                        id = PacketTypes.CharacterExistReply,
                        fields = new List<Field>
                        {
                            new Field { name = "reqId", type = "uint32", defaultValue = 0 },
                            new Field { name = "status", type = "boolean", defaultValue = 0 },
                        },
                    },
                };
            }
        }
    }
}