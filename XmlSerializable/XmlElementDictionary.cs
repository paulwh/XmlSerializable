using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Serialization.Xml.Internal;

namespace Serialization.Xml {
    public class XmlElementDictionary : Dictionary<string, object>, IXmlSerializable {
        public XmlSchema GetSchema() {
            return null;
        }

        public void ReadXml(XmlReader reader) {
            if (reader.IsEmptyElement) {
                reader.Skip();
            } else {
                reader.ReadStartElement();
                while (reader.NodeType != XmlNodeType.EndElement) {
                    if (reader.NodeType == XmlNodeType.Element) {
                        var key = reader.LocalName;
                        object value = null;
                        if (!reader.IsEmptyElement) {
                            value = ReadValue(reader);
                        } else {
                            reader.Skip();
                        }
                        this[key] = value;
                    } else {
                        reader.Skip();
                    }
                }
                reader.ReadEndElement();
            }
        }

        private object ReadValue(XmlReader reader) {
            reader.ReadStartElement();
            object value = null;
            if (reader.NodeType != XmlNodeType.EndElement) {
                // treat as string
                while (reader.NodeType == XmlNodeType.Whitespace || reader.NodeType == XmlNodeType.SignificantWhitespace) {
                    reader.Read();
                }
                if (reader.NodeType == XmlNodeType.Text) {
                    var sb = new StringBuilder();
                    do {
                        sb.Append(reader.Value);
                        reader.Read();
                    } while (reader.NodeType == XmlNodeType.Text || reader.NodeType == XmlNodeType.Whitespace || reader.NodeType == XmlNodeType.SignificantWhitespace);

                    if (reader.NodeType != XmlNodeType.EndElement) {
                        throw new NotSupportedException("Mixed mode content is not supported.");
                    }
                    value = sb.ToString();
                } else {
                    // treat as dictionary
                    var dict = new XmlElementDictionary();
                    value = dict;
                    do {
                        switch (reader.NodeType) {
                            case XmlNodeType.Element:
                                // Child member
                                var key = reader.LocalName;
                                dict[key] = ReadValue(reader);
                                break;
                            case XmlNodeType.Text:
                            case XmlNodeType.CDATA:
                                throw new NotSupportedException("Mixed mode content is not supported.");
                            default:
                                // comments, whitespace, etc.
                                reader.Skip();
                                break;
                        }
                    } while (reader.NodeType != XmlNodeType.EndElement);
                }
            }
            reader.ReadEndElement();
            return value;
        }

        public void WriteXml(XmlWriter writer) {
            foreach (var kvp in this) {
                writer.WriteStartElement(kvp.Key);
                if (kvp.Value != null) {
                    var childSerializer =
                        XmlPrimitiveSerializer.TryGetPrimitiveSerializer(kvp.Value.GetType());
                    if (childSerializer != null) {
                        childSerializer.Serialize(writer, kvp.Value, XmlSerializableSettings.Default);
                    } else {
                        XmlSerializable.Serialize(writer, kvp.Value);
                    }
                } else {
                    writer.WriteAttributeString("nil", XmlSerializerBase.XsiNamespace, "true");
                }
                writer.WriteEndElement();
            }
        }
    }
}
