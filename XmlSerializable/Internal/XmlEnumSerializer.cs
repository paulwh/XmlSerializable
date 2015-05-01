using System;
using System.Reflection;
using System.Xml;
using Serialization.Xml.Internal.Reflection;

namespace Serialization.Xml.Internal {
    internal static class XmlEnumSerializer {
        public static IXmlSerializer ForType(Type enumType) {
            return (IXmlSerializer)CreateSerializerMethod.MakeGenericMethod(enumType).Invoke(null, null);
        }

        private static readonly MethodInfo CreateSerializerMethod =
            ReflectionHelper.GetMethod(() => XmlEnumSerializer.CreateSerializer<DateTimeKind>()).GetGenericMethodDefinition();

        private static XmlEnumSerializer<T> CreateSerializer<T>() where T : struct {
            return new XmlEnumSerializer<T>();
        }
    }

    internal interface IXmlEnumSerializer { }

    internal class XmlEnumSerializer<T> : XmlSerializerBase<T>, IXmlEnumSerializer where T : struct {
        static XmlEnumSerializer() {
            if (!typeof(T).IsEnum) {
                throw new InvalidOperationException(
                    String.Format(
                        "XmlEnumSerializer cannot be used with they type '{0}' becuase it is not an enum.",
                        typeof(T).FullName
                    )
                );
            }
        }

        public override Boolean CanCreateInstance {
            get {
                return true;
            }
        }

        public override T CreateInstance() {
            return default(T);
        }

        public override void Serialize(XmlWriter writer, T obj, XmlSerializableSettings settings) {
            writer.WriteString(obj.ToString());
        }

        public override T Deserialize(XmlReader reader, T instance, Action<XmlReader> onUnknownElement = null) {
            return EnumEx.Parse<T>(reader.ReadNodeContentAsString());
        }
    }
}
