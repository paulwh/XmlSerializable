using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Serialization.Xml.Internal;
using Serialization.Xml.Internal.IO;

namespace Serialization.Xml {
    public abstract class XmlSerializable : IXmlSerializable {
        protected XmlSerializableSettings Settings { get; set; }

        public virtual XmlSchema GetSchema() {
            return null;
        }

        public void ReadXml(XmlReader reader) {
            this.ReadXml(reader, validateRoot: false);
        }

        protected abstract void ReadXml(XmlReader reader, Boolean validateRoot);

        public void WriteXml(XmlWriter writer) {
            this.WriteXml(writer, emitRoot: false);
        }

        protected abstract void WriteXml(XmlWriter writer, Boolean emitRoot);

        public static void Serialize<T>(
            Stream stream,
            T instance,
            XmlSerializableSettings settings = null) {

            using (var writer = new StreamWriter(new SharedStream(stream))) {
                Serialize(writer, instance, settings);
            }
        }

        private static readonly XmlWriterSettings WriterSettings = new XmlWriterSettings {
            Indent = true,
            IndentChars = "    ",
        };

        public static void Serialize<T>(
            TextWriter writer,
            T instance,
            XmlSerializableSettings settings = null) {

            using (var xmlWriter = XmlTextWriter.Create(writer, WriterSettings)) {
                Serialize(xmlWriter, instance, settings);
            }
        }

        public static void Serialize<T>(
            XmlWriter writer,
            T instance,
            XmlSerializableSettings settings = null) {

            if (typeof(XmlSerializable).IsAssignableFrom(typeof(T))) {
                var serializable = ((XmlSerializable)(object)instance);
                serializable.Settings = settings ?? serializable.Settings;
                serializable.WriteXml(writer, emitRoot: true);
            } else {
                new XmlSerializable<T>(instance, settings).WriteXml(writer, emitRoot: true);
            }
        }

        public static T Deserialize<T>(Stream stream) {
            using (var reader = new StreamReader(new SharedStream(stream))) {
                return Deserialize<T>(reader);
            }
        }

        public static T Deserialize<T>(TextReader reader) {
            using (var xmlReader = XmlTextReader.Create(reader)) {
                return Deserialize<T>(xmlReader);
            }
        }

        public static T Deserialize<T>(XmlReader reader) {
            if (typeof(XmlSerializable).IsAssignableFrom(typeof(T))) {
                var obj = (T)Activator.CreateInstance(typeof(T), nonPublic: true);
                ((XmlSerializable)(object)obj).ReadXml(reader, validateRoot: true);
                return obj;
            } else {
                var serializer = new XmlSerializable<T>(
                    (T)Activator.CreateInstance(typeof(T), nonPublic: true)
                );
                serializer.ReadXml(reader, validateRoot: true);
                return serializer.Instance;
            }
        }
    }

    public class XmlSerializable<T> : XmlSerializable {
        public static XmlNameDeclaration RootName { get; private set; }
        // The primary serializer for the type T
        private static IXmlSerializer Serializer;

        internal T Instance { get; set; }

        static XmlSerializable() {
            Serializer = XmlSerializerBase<T>.CreateSerializer();
            RootName = XmlSerializerBase.GetRootNameForType(typeof(T));
        }

        public XmlSerializable(T instance)
            : this(instance, XmlSerializableSettings.Default) {
        }

        public XmlSerializable(T instance, XmlSerializableSettings settings) {
            if (instance == null) {
                throw new ArgumentNullException("instance");
            }
            this.Instance = instance;
            this.Settings = settings ?? XmlSerializableSettings.Default;
        }

        protected XmlSerializable()
            : this(XmlSerializableSettings.Default) {
        }

        protected XmlSerializable(XmlSerializableSettings settings) {
            if (settings == null) {
                throw new ArgumentNullException("settings");
            }
            if (typeof(T) != this.GetType()) {
                throw new InvalidOperationException(
                    "Any type T deriving from XmlSerializable<> must derive from XmlSerializable<T>."
                );
            }
            this.Instance = (T)(Object)this;
            this.Settings = settings;
        }

        protected override void ReadXml(XmlReader reader, bool validateRoot) {
            if (!reader.IsStartElement()) {
                throw new XmlException("Unexpected xml node.");
            }

            if (validateRoot) {
                if (reader.LocalName != RootName.Name ||
                    !String.IsNullOrEmpty(RootName.Namespace) && reader.NamespaceURI != RootName.Namespace) {
                    throw new XmlException(
                        String.Format(
                            "Unexpected xml element. Expected <{0}{1}>, found <{2}{3}>.",
                            RootName.Name,
                            RootName.Namespace.Bind(ns => " xmlns=\"" + ns + "\"") ?? String.Empty,
                            reader.LocalName,
                            reader.NamespaceURI.Bind(ns => " xmlns=\"" + ns + "\"") ?? String.Empty
                        )
                    );
                }
            }

            this.Instance = (T)Serializer.Deserialize(
                reader,
                this.Instance,
                rdr => this.OnUnknownElement(rdr)
            );
            this.OnDeserializationComplete();
        }

        protected override void WriteXml(XmlWriter writer, bool emitRoot) {
            if (emitRoot) {
                writer.WriteStartElement(RootName.Name, RootName.Namespace);
            }

            Serializer.Serialize(writer, this.Instance, this.Settings);

            if (emitRoot) {
                writer.WriteEndElement();
            }
        }

        protected virtual void OnUnknownElement(XmlReader reader) {
            reader.Skip();
        }

        protected virtual void OnDeserializationComplete() {
        }
    }
}
