using System;

namespace Serialization.Xml {
    public class XmlSerializableSettings {
        public Boolean OmitNulls { get; private set; }

        public XmlSerializableSettings(Boolean omitDefaults = false) {
            this.OmitNulls = omitDefaults;
        }

        public static readonly XmlSerializableSettings Default = new XmlSerializableSettings();
    }
}
