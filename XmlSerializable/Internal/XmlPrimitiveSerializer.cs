using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Serialization.Xml.Internal.IO;
using Serialization.Xml.Internal.Reflection;

namespace Serialization.Xml.Internal {

    internal static class XmlPrimitiveSerializer {
        public static IXmlPrimitiveSerializer TryGetPrimitiveSerializer(Type type) {
            return (IXmlPrimitiveSerializer)
                TryGetPrimitiveSerializerMethod.MakeGenericMethod(type).Invoke(null, null);
        }

        private static MethodInfo TryGetPrimitiveSerializerMethod =
            ReflectionHelper.GetMethod(() => TryGetPrimitiveSerializer<Object>()).GetGenericMethodDefinition();

        public static XmlPrimitiveSerializer<T> TryGetPrimitiveSerializer<T>() {
            IXmlPrimitiveSerializer result;
            return PrimitiveSerializers.TryGetValue(typeof(T), out result) ? (XmlPrimitiveSerializer<T>)result : null;
        }

        public static String TryGetPrimitiveTypeName(Type type) {
            IXmlPrimitiveSerializer result;
            return PrimitiveSerializers.TryGetValue(type, out result) ? result.TypeName : null;
        }

        private static Dictionary<Type, IXmlPrimitiveSerializer> PrimitiveSerializers;

        static XmlPrimitiveSerializer() {
            PrimitiveSerializers =
                new Dictionary<Type, IXmlPrimitiveSerializer> {
                    {
                        typeof(Boolean),
                        new XmlPrimitiveSerializer<Boolean>(
                            "boolean",
                            reader => ParseBool(reader.ReadNodeContentAsString()),
                            (writer, obj) => writer.WriteValue((Boolean)obj)
                        )
                    },
                    {
                        typeof(Char),
                        new XmlPrimitiveSerializer<Char>(
                            "char",
                            reader => {
                                var str = reader.ReadNodeContentAsString();
                                return (Char)Int32.Parse(str);
                            },
                            (writer, obj) => writer.WriteString(((Int32)obj).ToString())
                        )
                    },
                    {
                        typeof(SByte),
                        new XmlPrimitiveSerializer<SByte>(
                            "byte",
                            reader => SByte.Parse(reader.ReadNodeContentAsString()),
                            (writer, obj) => writer.WriteString(obj.ToString())
                        )
                    },
                    {
                        typeof(Byte),
                        new XmlPrimitiveSerializer<Byte>(
                            "unsignedByte",
                            reader => Byte.Parse(reader.ReadNodeContentAsString()),
                            (writer, obj) => writer.WriteString(obj.ToString())
                        )
                    },
                    {
                        typeof(Int16),
                        new XmlPrimitiveSerializer<Int16>(
                            "short",
                            reader => Int16.Parse(reader.ReadNodeContentAsString()),
                            (writer, obj) => writer.WriteString(obj.ToString())
                        )
                    },
                    {
                        typeof(UInt16),
                        new XmlPrimitiveSerializer<UInt16>(
                            "unsignedShort",
                            reader => UInt16.Parse(reader.ReadNodeContentAsString()),
                            (writer, obj) => writer.WriteString(obj.ToString())
                        )
                    },
                    {
                        typeof(Int32),
                        new XmlPrimitiveSerializer<Int32>(
                            "int",
                            reader => Int32.Parse(reader.ReadNodeContentAsString()),
                            (writer, obj) => writer.WriteString(obj.ToString())
                        )
                    },
                    {
                        typeof(UInt32),
                        new XmlPrimitiveSerializer<UInt32>(
                            "unsignedInt",
                            reader => UInt32.Parse(reader.ReadNodeContentAsString()),
                            (writer, obj) => writer.WriteString(obj.ToString())
                        )
                    },
                    {
                        typeof(Int64),
                        new XmlPrimitiveSerializer<Int64>(
                            "long",
                            reader => Int64.Parse(reader.ReadNodeContentAsString()),
                            (writer, obj) => writer.WriteString(obj.ToString())
                        )
                    },
                    {
                        typeof(UInt64),
                        new XmlPrimitiveSerializer<UInt64>(
                            "unsignedLong",
                            reader => UInt64.Parse(reader.ReadNodeContentAsString()),
                            (writer, obj) => writer.WriteString(obj.ToString())
                        )
                    },
                    {
                        typeof(Single),
                        new XmlPrimitiveSerializer<Single>(
                            "float",
                            reader => Single.Parse(reader.ReadNodeContentAsString()),
                            (writer, obj) => writer.WriteString(obj.ToString("r"))
                        )
                    },
                    {
                        typeof(Double),
                        new XmlPrimitiveSerializer<Double>(
                            "double",
                            reader => Double.Parse(reader.ReadNodeContentAsString()),
                            (writer, obj) => writer.WriteString(obj.ToString("r"))
                        )
                    },
                    {
                        typeof(Decimal),
                        new XmlPrimitiveSerializer<Decimal>(
                            "decimal",
                            reader => Decimal.Parse(reader.ReadNodeContentAsString()),
                            (writer, obj) => writer.WriteString(obj.ToString())
                        )
                    },
                    {
                        typeof(DateTime),
                        new XmlPrimitiveSerializer<DateTime>(
                            "dateTime",
                            reader => reader.ReadNodeContentAsDateTime(),
                            (writer, obj) => writer.WriteValue(obj)
                        )
                    },
                    {
                        typeof(TimeSpan),
                        new XmlPrimitiveSerializer<TimeSpan>(
                            "TimeSpan",
                            reader => TimeSpan.Parse(reader.ReadNodeContentAsString()),
                            (writer, obj) => writer.WriteString(obj.ToString())
                        )
                    },
                    {
                        typeof(Guid),
                        new XmlPrimitiveSerializer<Guid>(
                            "guid",
                            reader => Guid.Parse(reader.ReadNodeContentAsString()),
                            (writer, obj) => writer.WriteString(obj.ToString())
                        )
                    },
                    {
                        typeof(Uri),
                        new XmlPrimitiveSerializer<Uri>(
                            "Uri",
                            reader => new Uri(reader.ReadNodeContentAsString()),
                            (writer, obj) => writer.WriteString(obj.ToString())
                        )
                    },
                    {
                        typeof(Version),
                        new XmlPrimitiveSerializer<Version>(
                            "Version",
                            reader => Version.Parse(reader.ReadNodeContentAsString()),
                            (writer, obj) => writer.WriteString(obj.ToString())
                        )
                    },
                    {
                        typeof(String),
                        new XmlPrimitiveSerializer<String>(
                            "string",
                            reader => reader.ReadNodeContentAsString(),
                            (writer, obj) => writer.WriteString(obj)
                        )
                    },
                    {
                        typeof(Byte[]),
                        new XmlPrimitiveSerializer<Byte[]>(
                            "base64Binary",
                            reader => {
                                using (var stream = DesieralizeToStream(reader)) {
                                    return stream.ReadAllBytes();
                                }
                            },
                            (writer, obj) => {
                                var array = (Byte[])obj;
                                writer.WriteBase64(array, 0, array.Length);
                            }
                        )
                    },
                    {
                        typeof(Stream),
                        new XmlPrimitiveSerializer<Stream>(
                            "Stream",
                            reader => {
                                using (var stream = DesieralizeToStream(reader)) {
                                    return new MemoryStream(stream.ReadAllBytes());
                                }
                            },
                            (writer, obj) => SerializeStream(writer, obj)
                        )
                    }
                };
        }

        private const int BufferSize = 1024;

        private static void SerializeStream(XmlWriter writer, Stream stream) {
            int len;
            var buffer = new byte[BufferSize];
            while (0 < (len = stream.Read(buffer, 0, BufferSize))) {
                writer.WriteBase64(buffer, 0, len);
            }
        }

        private static Stream DesieralizeToStream(XmlReader reader) {
            return new XmlBinaryStream(reader);
        }

        private static readonly String[] TrueValues = new[] {
                "true", "yes", "on"
            };

        private static readonly String[] FalseValues = new[] {
                "false", "no", "off"
            };

        private static Boolean ParseBool(String str) {
            if (String.IsNullOrEmpty(str)) {
                throw new FormatException("Unable to parse content as boolean value.");
            } else if (TrueValues.Contains(str, StringComparer.OrdinalIgnoreCase)) {
                return true;
            } else if (FalseValues.Contains(str, StringComparer.OrdinalIgnoreCase)) {
                return false;
            } else {
                Int64 intval;
                if (Int64.TryParse(str, out intval)) {
                    return intval != 0;
                } else {
                    throw new FormatException("Unable to parse content as boolean value.");
                }
            }
        }
    }

    internal interface IXmlPrimitiveSerializer : IXmlSerializer {
        String TypeName { get; }
    }

    internal class XmlPrimitiveSerializer<T> : XmlSerializerBase<T>, IXmlPrimitiveSerializer {
        public String TypeName { get; private set; }
        private Func<XmlReader, T> deserialize;
        private Action<XmlWriter, T> serialize;

        public XmlPrimitiveSerializer(
            String typeName,
            Func<XmlReader, T> deserialize,
            Action<XmlWriter, T> serialize) {

            this.TypeName = typeName;
            this.deserialize = deserialize;
            this.serialize = serialize;
        }

        public override T CreateInstance() {
            return default(T);
        }

        public override void Serialize(XmlWriter writer, T obj, XmlSerializableSettings settings) {
            this.serialize(writer, obj);
        }

        public override T Deserialize(XmlReader reader, T instance, Action<XmlReader> onUnknownElement = null) {
            return this.deserialize(reader);
        }
    }
}
