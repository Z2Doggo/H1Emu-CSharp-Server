namespace Data.DataSchema
{
    using System.Buffers;
    using System.ComponentModel;
    using System.Text;
    using System.Xml.Linq;

    public static class DataSchema
    {
        interface IH1Z1Buffer : IMemoryOwner<byte>
        {
            void WriteBytes(byte[] value, int offset, int length);
            void WritePrefixedStringLE(string value, int offset);
            void WriteUInt64String(string value, int offset);
            void WriteInt64String(string value, int offset);
            void WriteNullTerminatedString(string value, int offset);
            byte[] ReadBytes(int offset, int length);
            string ReadPrefixedStringLE(int offset);
            string ReadUInt64String(int offset);
            string ReadInt64String(int offset);
            string ReadNullTerminatedString(int offset);
        }

        public static object Parse(dynamic fields, byte[] dataToParse, int offset)
        {
            var data = dataToParse;
            var startOffset = offset;
            var result = new Dictionary<string, object>();
            fields = fields ?? new List<object>();
            object element = null;
            byte[] bytes = new byte[0];
            dynamic elements = new List<object>();
            dynamic elementSchema = new List<object>();

            for (int index = 0; index < ((List<object>)fields).Count; index++)
            {
                var field = ((List<object>)fields)[index];
                switch ((string)((Dictionary<string, object>)field)["type"])
                {
                    case "schema":
                        element = Parse(
                            ((Dictionary<string, object>)field)["fields"],
                            data,
                            offset
                        );
                        offset += (int)((Dictionary<string, object>)element)["length"];
                        result[(string)((Dictionary<string, object>)field)["name"]] = (
                            (Dictionary<string, object>)element
                        )["result"];
                        break;
                    case "array":
                    case "array8":
                        elements = new List<object>();
                        int numElements = 0;
                        if (TypeDescriptor.GetProperties(field).Find("length", true) != null)
                        {
                            numElements = (int)((Dictionary<string, object>)field)["length"];
                        }
                        else
                        {
                            if ((string)((Dictionary<string, object>)field)["type"] == "array")
                            {
                                numElements = BitConverter.ToInt32(data, offset);
                                offset += 4;
                            }
                            else if (
                                (string)((Dictionary<string, object>)field)["type"] == "array8"
                            )
                            {
                                numElements = data[offset];
                                offset += 1;
                            }
                        }
                        if (TypeDescriptor.GetProperties(field).Find("debuglength", true) != null)
                        {
                            numElements = (int)((Dictionary<string, object>)field)["debuglength"];
                        }
                        if (TypeDescriptor.GetProperties(field).Find("fields", true) != null)
                        {
                            for (int j = 0; j < numElements; j++)
                            {
                                element = Parse(
                                    ((Dictionary<string, object>)field)["fields"],
                                    data,
                                    offset
                                );
                                offset += (int)((Dictionary<string, object>)element)["length"];
                                elements.Add(((Dictionary<string, object>)element)["result"]);
                            }
                        }
                        else if (
                            TypeDescriptor.GetProperties(field).Find("elementType", true) != null
                        )
                        {
                            elementSchema = new List<object>
                            {
                                new Dictionary<string, object>
                                {
                                    { "name", "element" },
                                    { "type", ((Dictionary<string, object>)field)["elementType"] }
                                }
                            };
                            for (int j = 0; j < numElements; j++)
                            {
                                element = Parse(elementSchema, data, offset);
                                offset += (int)((Dictionary<string, object>)element)["length"];
                                elements.Add(
                                    (
                                        (Dictionary<string, object>)(
                                            (Dictionary<string, object>)element
                                        )["result"]
                                    )["element"]
                                );
                            }
                        }
                        result[(string)((Dictionary<string, object>)field)["name"]] = elements;
                        break;
                    case "debugoffset":
                        result[(string)((Dictionary<string, object>)field)["name"]] = offset;
                        break;
                    case "debugbytes":
                        result[(string)((Dictionary<string, object>)field)["name"]] = data.Skip(
                                offset
                            )
                            .Take((int)((Dictionary<string, object>)field)["length"])
                            .ToArray();
                        break;
                    case "bytes":
                        bytes = data.Skip(offset)
                            .Take((int)((Dictionary<string, object>)field)["length"])
                            .ToArray();
                        if (bytes.Length > 20)
                        {
                            string ToJSON() => $"[{bytes.Length} bytes]";
                        }
                        result[(string)((Dictionary<string, object>)field)["name"]] = bytes;
                        offset += (int)((Dictionary<string, object>)field)["length"];
                        break;
                    case "byteswithlength":
                        var length = BitConverter.ToInt32(data, offset);
                        offset += 4;
                        if (length > 0)
                        {
                            if (TypeDescriptor.GetProperties(field).Find("fields", true) != null)
                            {
                                element = Parse(
                                    ((Dictionary<string, object>)field)["fields"],
                                    data,
                                    offset
                                );
                                if (element != null)
                                {
                                    result[(string)((Dictionary<string, object>)field)["name"]] = (
                                        (Dictionary<string, object>)element
                                    )["result"];
                                }
                            }
                            else
                            {
                                bytes = data.Skip(offset).Take(length).ToArray();
                                result[(string)((Dictionary<string, object>)field)["name"]] = bytes;
                            }
                            offset += length;
                        }
                        break;
                    case "uint32":
                        result[(string)((Dictionary<string, object>)field)["name"]] =
                            BitConverter.ToUInt32(data, offset);
                        offset += 4;
                        break;
                    case "int32":
                        result[(string)((Dictionary<string, object>)field)["name"]] =
                            BitConverter.ToInt32(data, offset);
                        offset += 4;
                        break;
                    case "uint16":
                        result[(string)((Dictionary<string, object>)field)["name"]] =
                            BitConverter.ToUInt16(data, offset);
                        offset += 2;
                        break;
                    case "int16":
                        result[(string)((Dictionary<string, object>)field)["name"]] =
                            BitConverter.ToInt16(data, offset);
                        offset += 2;
                        break;
                    case "uint8":
                        result[(string)((Dictionary<string, object>)field)["name"]] = data[offset];
                        offset += 1;
                        break;
                    case "int8":
                        result[(string)((Dictionary<string, object>)field)["name"]] = (sbyte)data[
                            offset
                        ];
                        offset += 1;
                        break;
                    case "rgb":
                        result[(string)((Dictionary<string, object>)field)["name"]] = new
                        {
                            r = (sbyte)data[offset],
                            g = (sbyte)data[offset + 1],
                            b = (sbyte)data[offset + 2]
                        };
                        offset += 3;
                        break;
                    case "string":
                        var stringLength = data[offset];
                        offset += 1;
                        result[(string)((Dictionary<string, object>)field)["name"]] =
                            Encoding.UTF8.GetString(data, offset, stringLength);
                        offset += stringLength;
                        break;
                    case "stringWithLength":
                        var strLength = BitConverter.ToInt32(data, offset);
                        offset += 4;
                        result[(string)((Dictionary<string, object>)field)["name"]] =
                            Encoding.UTF8.GetString(data, offset, strLength);
                        offset += strLength;
                        break;
                    case "float":
                        result[(string)((Dictionary<string, object>)field)["name"]] =
                            BitConverter.ToSingle(data, offset);
                        offset += 4;
                        break;
                    case "double":
                        result[(string)((Dictionary<string, object>)field)["name"]] =
                            BitConverter.ToDouble(data, offset);
                        offset += 8;
                        break;
                    case "boolean":
                        result[(string)((Dictionary<string, object>)field)["name"]] =
                            BitConverter.ToBoolean(data, offset);
                        offset += 1;
                        break;
                    case "boolean8":
                        result[(string)((Dictionary<string, object>)field)["name"]] =
                            data[offset] > 0;
                        offset += 1;
                        break;
                    case "boolean32":
                        result[(string)((Dictionary<string, object>)field)["name"]] =
                            BitConverter.ToInt32(data, offset) > 0;
                        offset += 4;
                        break;
                }
            }
            return new Dictionary<string, object>
            {
                { "result", result },
                { "length", offset - startOffset }
            };
        }
    }
}