using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Serialization.Xml {
    public class XmlFragment : IXmlSerializable {
        public XmlDocumentFragment Fragment { get; private set; }

        private XmlDocument Document {
            get { return this.Fragment.OwnerDocument; }
        }

        public XmlFragment()
            : this(new XmlDocument().CreateDocumentFragment()) {
        }

        public XmlFragment(XmlDocumentFragment fragment) {
            if (fragment == null) {
                throw new ArgumentNullException("fragment");
            }
            this.Fragment = fragment;
        }

        public XmlSchema GetSchema() {
            return null;
        }

        public void ReadXml(XmlReader reader) {
            ReadCurrent(reader, this.Fragment);
        }

        private void ReadCurrent(XmlReader reader, XmlNode root) {
            var done = false;
            var parents = new Stack<XmlNode>();
            var parent = root;
            var namespaces = new Dictionary<string, string>();
            do {
                switch (reader.NodeType) {
                    case XmlNodeType.Element:
                        var ns = reader.NamespaceURI;
                        var pref = reader.Prefix;
                        var name = reader.Name;
                        var isEmpty = reader.IsEmptyElement;
                        var attributes = new List<XmlAttribute>();
                        if (reader.MoveToFirstAttribute()) {
                            do {
                                var skip = false;
                                if (reader.Name == "xmlns" && reader.Prefix =="") {
                                    namespaces[""] = reader.Value;
                                    if (String.IsNullOrEmpty(pref)) {
                                        // Our parent element already has this namespace associated, no need to explicitly add the attribute.
                                        skip = true;
                                    }
                                } else if (reader.Prefix == "xmlns") {
                                    namespaces[reader.Name] = reader.Value;
                                    if (pref == reader.Name) {
                                        // Our parent element already has this namespace associated, no need to explicitly add the attribute.
                                        skip = true;
                                    }
                                }
                                if (!skip) {
                                    var attr = this.Document.CreateAttribute(reader.Prefix, reader.Name, reader.NamespaceURI);
                                    attr.Value = reader.Value;
                                    attributes.Add(attr);
                                }
                            } while (reader.MoveToNextAttribute());
                        }

                        if (string.IsNullOrEmpty(ns)) {
                            namespaces.TryGetValue(reader.Prefix ?? "", out ns);
                        }
                        var elem = this.Document.CreateElement(pref, name, ns);
                        parent.AppendChild(elem);
                        attributes.ForEach(attr => elem.Attributes.Append(attr));

                        if (!isEmpty) {
                            parents.Push(parent);
                            parent = elem;
                        }
                        break;
                    case XmlNodeType.EndElement:
                        if (parents.Count > 0) {
                            parent = parents.Pop();
                        } else {
                            done = true;
                        }
                        break;
                    case XmlNodeType.XmlDeclaration:
                    case XmlNodeType.ProcessingInstruction:
                        throw new NotImplementedException("TODO");
                    case XmlNodeType.CDATA:
                        var cdata = this.Document.CreateCDataSection(reader.Value);
                        parent.AppendChild(cdata);
                        break;
                    case XmlNodeType.Comment:
                        var comment = this.Document.CreateComment(reader.Value);
                        parent.AppendChild(comment);
                        break;
                    case XmlNodeType.SignificantWhitespace:
                    case XmlNodeType.Text:
                    case XmlNodeType.Whitespace:
                        var text = this.Document.CreateTextNode(reader.Value);
                        parent.AppendChild(text);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            } while (!done && reader.Read());
        }

        public void WriteXml(XmlWriter writer) {
            this.Fragment.WriteTo(writer);
        }
    }
}
