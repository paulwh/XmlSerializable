using System;
using System.Xml;

namespace Serialization.Xml {
    public static class XmlReaderExtensions {

        public static void CopyTo(this XmlReader reader, XmlWriter writer) {
            while (reader.Read()) {
                switch (reader.NodeType) {
                    case XmlNodeType.ProcessingInstruction:
                        writer.WriteProcessingInstruction(reader.Name, reader.Value);
                        break;
                    case XmlNodeType.Element:
                        writer.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                        while (reader.MoveToNextAttribute()) {
                            writer.WriteAttributeString(reader.Prefix, reader.LocalName, reader.NamespaceURI, reader.Value);
                        }
                        reader.MoveToElement();
                        if (reader.IsEmptyElement) {
                            writer.WriteEndElement();
                        }
                        break;
                    case XmlNodeType.EndElement:
                        writer.WriteEndElement();
                        break;
                    case XmlNodeType.Text:
                    case XmlNodeType.Whitespace:
                        writer.WriteString(reader.Value);
                        break;
                    case XmlNodeType.SignificantWhitespace:
                        writer.WriteRaw(reader.Value);
                        break;
                    case XmlNodeType.CDATA:
                        writer.WriteCData(reader.Value);
                        break;
                    case XmlNodeType.EntityReference:
                        writer.WriteEntityRef(reader.Name);
                        break;
                    case XmlNodeType.Comment:
                        writer.WriteComment(reader.Value);
                        break;
                    case XmlNodeType.XmlDeclaration:
                        break;
                    default:
                        throw new NotSupportedException("The node type " + reader.NodeType + " is not supported.");
                }
            }
        }

        public static XmlQualifiedName GetQualifiedName(this XmlReader reader) {
            return new XmlQualifiedName(reader.LocalName, reader.NamespaceURI);
        }

        public static bool IsStartElement(this XmlReader reader, XmlNameDeclaration name) {
            return reader.IsStartElement() && reader.Name == name.Name &&
                (name.Namespace == null || reader.NamespaceURI == name.Namespace);
        }

        public static void ReadXmlWith(this XmlReader reader, Action handleChild, Action handleAttribute = null) {
            if (reader == null) {
                throw new ArgumentNullException("reader");
            } else if (handleChild == null) {
                throw new ArgumentNullException("handleElement");
            }
            var startElement = reader.Name;

            if (handleAttribute != null) {
                while (reader.MoveToNextAttribute()) {
                    handleAttribute();
                }
                reader.MoveToElement();
            }

            if (!reader.IsEmptyElement) {
                // Consume the start element
                var done = !reader.Read();
                while (!done) {
                    switch (reader.NodeType) {
                        case XmlNodeType.Element:
                            handleChild();
                            break;
                        case XmlNodeType.EndElement:
                            if (startElement != reader.Name) {
                                throw new XmlException(
                                    String.Format(
                                        "The end element '{0}' did not match the expected '{1}'.",
                                        reader.Name,
                                        startElement
                                    )
                                );
                            }
                            done = true;
                            // Consume the end element
                            reader.Read();
                            break;
                        default:
                            done = !reader.Read();
                            // Ignore other XML
                            break;
                    }
                }
            } else {
                reader.Skip();
            }
        }

        public static Boolean ReadNodeContentAsBoolean(this XmlReader reader) {
            switch (reader.NodeType) {
                case XmlNodeType.Element:
                    return reader.ReadElementContentAsBoolean();
                case XmlNodeType.Attribute:
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                    return reader.ReadContentAsBoolean();
                default:
                    throw new XmlException(
                        String.Format(
                            "Cannot read content from the node type {0}",
                            reader.NodeType
                        )
                    );
            }
        }

        public static Int32 ReadNodeContentAsInt(this XmlReader reader) {
            switch (reader.NodeType) {
                case XmlNodeType.Element:
                    return reader.ReadElementContentAsInt();
                case XmlNodeType.Attribute:
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                    return reader.ReadContentAsInt();
                default:
                    throw new XmlException(
                        String.Format(
                            "Cannot read content from the node type {0}",
                            reader.NodeType
                        )
                    );
            }
        }

        public static Int64 ReadNodeContentAsLong(this XmlReader reader) {
            switch (reader.NodeType) {
                case XmlNodeType.Element:
                    return reader.ReadElementContentAsLong();
                case XmlNodeType.Attribute:
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                    return reader.ReadContentAsLong();
                default:
                    throw new XmlException(
                        String.Format(
                            "Cannot read content from the node type {0}",
                            reader.NodeType
                        )
                    );
            }
        }

        public static Decimal ReadNodeContentAsDecimal(this XmlReader reader) {
            switch (reader.NodeType) {
                case XmlNodeType.Element:
                    return reader.ReadElementContentAsDecimal();
                case XmlNodeType.Attribute:
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                    return reader.ReadContentAsDecimal();
                default:
                    throw new XmlException(
                        String.Format(
                            "Cannot read content from the node type {0}",
                            reader.NodeType
                        )
                    );
            }
        }

        public static Single ReadNodeContentAsFloat(this XmlReader reader) {
            switch (reader.NodeType) {
                case XmlNodeType.Element:
                    return reader.ReadElementContentAsFloat();
                case XmlNodeType.Attribute:
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                    return reader.ReadContentAsFloat();
                default:
                    throw new XmlException(
                        String.Format(
                            "Cannot read content from the node type {0}",
                            reader.NodeType
                        )
                    );
            }
        }

        public static Double ReadNodeContentAsDouble(this XmlReader reader) {
            switch (reader.NodeType) {
                case XmlNodeType.Element:
                    return reader.ReadElementContentAsDouble();
                case XmlNodeType.Attribute:
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                    return reader.ReadContentAsDouble();
                default:
                    throw new XmlException(
                        String.Format(
                            "Cannot read content from the node type {0}",
                            reader.NodeType
                        )
                    );
            }
        }

        public static DateTime ReadNodeContentAsDateTime(this XmlReader reader) {
            switch (reader.NodeType) {
                case XmlNodeType.Element:
                    return reader.ReadElementContentAsDateTime();
                case XmlNodeType.Attribute:
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                    return reader.ReadContentAsDateTime();
                default:
                    throw new XmlException(
                        String.Format(
                            "Cannot read content from the node type {0}",
                            reader.NodeType
                        )
                    );
            }
        }

        public static String ReadNodeContentAsString(this XmlReader reader) {
            switch (reader.NodeType) {
                case XmlNodeType.Element:
                    if (reader.IsEmptyElement) {
                        reader.Skip();
                        return null;
                    } else {
                        return reader.ReadElementContentAsString();
                    }
                case XmlNodeType.Attribute:
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                    return reader.ReadContentAsString();
                default:
                    throw new XmlException(
                        String.Format(
                            "Cannot read content from the node type {0}",
                            reader.NodeType
                        )
                    );
            }
        }

        public static void ReadNodeContentAsBase64(this XmlReader reader, byte[] buffer, Action<int> onRead) {
            var bufferSize = buffer.Length;
            Func<byte[], int, int, int> readOp;
            switch (reader.NodeType) {
                case XmlNodeType.Element:
                    readOp = reader.ReadElementContentAsBase64;
                    break;
                case XmlNodeType.Attribute:
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                    readOp = reader.ReadContentAsBase64;
                    break;
                default:
                    throw new XmlException(
                        String.Format(
                            "Cannot read Base64 content from the node type {0}",
                            reader.NodeType
                        )
                    );
            }
            int len;
            while (0 < (len = readOp(buffer, 0, bufferSize))) {
                onRead(len);
            }
        }

        public static void ReadNodeContentAsBinHex(this XmlReader reader, byte[] buffer, Action<int> onRead) {
            var bufferSize = buffer.Length;
            Func<byte[], int, int, int> readOp;
            switch (reader.NodeType) {
                case XmlNodeType.Element:
                    readOp = reader.ReadElementContentAsBinHex;
                    break;
                case XmlNodeType.Attribute:
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                    readOp = reader.ReadContentAsBinHex;
                    break;
                default:
                    throw new XmlException(
                        String.Format(
                            "Cannot read BinHex content from the node type {0}",
                            reader.NodeType
                        )
                    );
            }
            int len;
            while (0 < (len = readOp(buffer, 0, bufferSize))) {
                onRead(len);
            }
        }
    }
}
