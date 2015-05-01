using System;
using System.Xml;

namespace Serialization.Xml.Internal {
    internal interface IXmlSerializer {
        Boolean CanCreateInstance { get; }
        Object CreateInstance();
        void Serialize(XmlWriter writer, Object obj, XmlSerializableSettings settings);
        Object Deserialize(XmlReader reader, Object instance, Action<XmlReader> onUnknownElement = null);
    }

    internal interface IXmlSerializer<T> : IXmlSerializer {
        new T CreateInstance();
        void Serialize(XmlWriter writer, T obj, XmlSerializableSettings settings);
        T Deserialize(XmlReader reader, T instance, Action<XmlReader> onUnknownElement = null);
    }
}
