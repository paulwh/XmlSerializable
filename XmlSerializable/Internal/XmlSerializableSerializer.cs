using System;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Serialization.Xml.Internal.Reflection;

namespace Serialization.Xml.Internal {
    internal static class XmlSerializableSerializer {
        public static IXmlSerializer<T> ForType<T>() {
            if (!typeof(IXmlSerializable).IsAssignableFrom(typeof(T))) {
                throw new InvalidOperationException(
                    String.Format(
                        "The specified type '{0}' cannot be used with the XmlSerializableSerializer because it does not implement IXmlSerializable.",
                        typeof(T).FullName
                    )
                );
            }

            return (IXmlSerializer<T>)CreateSerializerMethod.MakeGenericMethod(typeof(T)).Invoke(null, null);
        }

        private static readonly MethodInfo CreateSerializerMethod =
            ReflectionHelper.GetMethod(() => XmlSerializableSerializer.CreateSerializer<IXmlSerializable>()).GetGenericMethodDefinition();

        private static XmlSerializableSerializer<T> CreateSerializer<T>() where T : IXmlSerializable {
            return new XmlSerializableSerializer<T>();
        }
    }

    internal interface IXmlSerializableSerializer : IXmlSerializer {
    }

    internal class XmlSerializableSerializer<T> : XmlSerializerBase<T>, IXmlSerializableSerializer where T : IXmlSerializable {
        public override void Serialize(XmlWriter writer, T obj, XmlSerializableSettings settings) {
            obj.WriteXml(writer);
        }

        public override T Deserialize(XmlReader reader, T instance, Action<XmlReader> onUnknownElement = null) {
            instance.ReadXml(reader);
            return instance;
        }
    }
}
