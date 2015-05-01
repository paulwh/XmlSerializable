using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Serialization.Xml.Internal.Collections;
using Serialization.Xml.Internal.Reflection;

namespace Serialization.Xml.Internal {
    internal class XmlObjectSerializer<T> : XmlSerializerBase<T> {
        private static readonly Type[] SupportedAttributeTypes = new[] {
            typeof(XmlElementAttribute),
            typeof(XmlAttributeAttribute),
            typeof(XmlTextAttribute),
            typeof(XmlIgnoreAttribute),
            typeof(XmlArrayAttribute),
            typeof(XmlAnyElementAttribute),
            typeof(XmlAnyAttributeAttribute),
        };

        // TODO
        private static readonly Type[] UnsupportedAttributeTypes = new[] { 
            typeof(XmlChoiceIdentifierAttribute),
            typeof(XmlEnumAttribute),
            typeof(XmlNamespaceDeclarationsAttribute),
            typeof(XmlSchemaProviderAttribute),
            typeof(XmlTypeAttribute),
        };

        private static readonly IDictionary<XmlNameDeclaration, SerializableMember> AttributeMembers;
        private static readonly SerializableMember AnyAttributeMember;
        private static readonly IList<List<SerializableMember>> ElementMembers;
        private static readonly SerializableMember TextMember;

        private static readonly Dictionary<Type, Tuple<String, IXmlSerializer>> KnownTypeSerializers;
        private static readonly Dictionary<String, IXmlSerializer> KnownTypeSerializersByName;

        static XmlObjectSerializer() {
            var members = ResolveFields(typeof(T))
                .Select(field =>
                    Handle(
                        field,
                        field.FieldType,
                        field.IsPublic
                    )
                ).Concat(ResolveProperties(typeof(T))
                // Ignore index properties
                    .Where(property => property.GetIndexParameters().IsNullOrEmpty())
                    .Select(property =>
                        Handle(
                            property,
                            property.PropertyType,
                            property.CanRead && property.CanWrite && (property.IsPublic() || property.HasPublicSetter())
                        )
                    )
                )
                .Where(Func.NotNull)
                .ToLookup(member => member.NodeType);
            AttributeMembers =
                members[XmlNodeType.Attribute].Where(member => !member.MatchAny).ToDictionary(member => member.MemberName);
            AnyAttributeMember =
                members[XmlNodeType.Attribute].Where(member => member.MatchAny).FirstOrDefault();
            ElementMembers =
                members[XmlNodeType.Element]
                    .GroupBy(member => member.Order)
                    .OrderBy(grp => grp.Key)
                    .Select(grp => grp.ToList())
                    .ToList();
            TextMember = members[XmlNodeType.Text].SingleOrDefault();

            var knownTypes = new Queue<XmlIncludeAttribute>(typeof(T).GetCustomAttributes<XmlIncludeAttribute>(inherit: false));
            if (knownTypes.Any()) {
                KnownTypeSerializers = new Dictionary<Type, Tuple<String, IXmlSerializer>>();
                KnownTypeSerializersByName = new Dictionary<String, IXmlSerializer>();
                while (knownTypes.Any()) {
                    var knownType = knownTypes.Dequeue();
                    var serializer = XmlSerializerBase.ForType(knownType.Type);
                    // TODO figure out the type name mapping for primitives.
                    var typeName = knownType.Type.Name;
                    KnownTypeSerializers.Add(knownType.Type, Tuple.Create(typeName, serializer));
                    KnownTypeSerializersByName.Add(typeName, serializer);

                    foreach (var subKnownType in knownType.Type.GetCustomAttributes<XmlIncludeAttribute>(inherit: false)) {
                        knownTypes.Enqueue(subKnownType);
                    }
                }
            }
        }

        public override void Serialize(XmlWriter writer, T obj, XmlSerializableSettings settings) {
            if (obj != null) {
                var actualType = obj.GetType();
                Tuple<String, IXmlSerializer> subTypeSerializer = null;
                if (actualType == typeof(T) ||
                    KnownTypeSerializers == null ||
                    !TryGetKnownTypeSerializer(actualType, out subTypeSerializer)) {

                    // We're the actual serializer that should be used for this type.
                    // Here comes the heavy lifting.
                    foreach (var attributeMember in AttributeMembers.Values) {
                        attributeMember.SerializeMember(writer, obj, settings);
                    }

                    foreach (var elementMemberGroup in ElementMembers) {
                        foreach (var elementMember in elementMemberGroup) {
                            elementMember.SerializeMember(writer, obj, settings);
                        }
                    }

                    if (TextMember != null) {
                        TextMember.SerializeMember(writer, obj, settings);
                    }
                } else {
                    writer.WriteAttributeString("xsi", "type", "http://www.w3.org/2001/XMLSchema-instance", subTypeSerializer.Item1);
                    subTypeSerializer.Item2.Serialize(writer, obj, settings);
                }
            }
        }

        public override T Deserialize(XmlReader reader, T instance, Action<XmlReader> onUnknownElement = null) {
            // See if we should be using a subtype serializer
            if (KnownTypeSerializersByName != null) {
                var xsiType = reader.GetAttribute("type", "http://www.w3.org/2001/XMLSchema-instance");
                if (xsiType != null) {
                    IXmlSerializer subTypeSerializer;
                    if (KnownTypeSerializersByName.TryGetValue(xsiType, out subTypeSerializer)) {
                        return (T)subTypeSerializer.Deserialize(reader, subTypeSerializer.CreateInstance());
                    }
                }
            }

            // We're the actual serializer that should be used for this element.
            // Here comes the heavy lifting.
            var startElement = reader.Name;
            while (reader.MoveToNextAttribute()) {
                SerializableMember member;
                if (AttributeMembers.TryGetValue(reader.GetQualifiedName(), out member) ||
                    AttributeMembers.TryGetValue(new XmlQualifiedName(reader.LocalName), out member)) {
                    instance = member.DeserializeMember(reader, instance);
                } else if (AnyAttributeMember != null) {
                    instance = AnyAttributeMember.DeserializeMember(reader, instance);
                }
            }
            reader.MoveToElement();
            if (!reader.IsEmptyElement) {
                // Advance to the first child
                reader.Read();
                var pos = 0;

                while (reader.NodeType != XmlNodeType.EndElement) {
                    switch (reader.NodeType) {
                        case XmlNodeType.Element:
                            SerializableMember member = null;
                            var nextPos = pos;
                            while (member == null && nextPos < ElementMembers.Count) {
                                member =
                                    ElementMembers[nextPos].SingleOrDefault(
                                        mem => !mem.MatchAny && mem.MemberName.Equals(reader.GetQualifiedName())
                                    );

                                if (member == null) {
                                    member = ElementMembers[nextPos].SingleOrDefault(
                                        mem => mem.MatchAny
                                    );
                                }

                                if (member == null) nextPos++;
                            }

                            if (member != null) {
                                pos = nextPos;
                                var xsiNil = reader.GetAttribute("nil", XsiNamespace);
                                if (xsiNil != null && String.Equals(xsiNil, "true", StringComparison.OrdinalIgnoreCase)) {
                                    if (member.Setter != null) {
                                        instance = member.Setter(instance, member.MemberType.GetDefaultValue());
                                    }
                                } else {
                                    instance = member.DeserializeMember(reader, instance);
                                }
                            } else {
                                // Ignore unexpected elements
                                if (onUnknownElement != null) {
                                    onUnknownElement(reader);
                                } else {
                                    reader.Skip();
                                }
                            }
                            break;
                        case XmlNodeType.Text:
                        case XmlNodeType.CDATA:
                            if (TextMember != null) {
                                instance = TextMember.DeserializeMember(reader, instance);
                            } else {
                                reader.Skip();
                            }
                            break;
                        case XmlNodeType.None:
                            throw new XmlException("Unexpected end of document.");
                        default:
                            reader.Skip();
                            break;
                    }
                }
                if (reader.NodeType != XmlNodeType.EndElement || reader.Name != startElement) {
                    throw new XmlException("Unexpected end element found.");
                } else {
                    reader.ReadEndElement();
                }
            } else {
                // Done
                reader.Skip();
            }

            return instance;
        }

        private static Boolean TryGetKnownTypeSerializer(Type actualType, out Tuple<String, IXmlSerializer> subTypeSerializer) {
            if (actualType == typeof(T)) {
                subTypeSerializer = null;
                return false;
            } else if (KnownTypeSerializers.TryGetValue(actualType, out subTypeSerializer)) {
                return true;
            } else if (actualType.BaseType != null) {
                return TryGetKnownTypeSerializer(actualType.BaseType, out subTypeSerializer);
            } else {
                return false;
            }
        }

        private static IEnumerable<FieldInfo> ResolveFields(Type t) {
            while (t != typeof(Object)) {
                foreach (var field in t.GetFields(BindingFlagsEx.AllInstance)) {
                    if (field.DeclaringType == t) {
                        yield return field;
                    }
                }
                t = t.BaseType;
            }
        }

        private static IEnumerable<PropertyInfo> ResolveProperties(Type t) {
            // If a property is virtual, only return the final override
            var existing = new HashSet<Tuple<Type, String>>();
            while (t != typeof(Object)) {
                foreach (var prop in t.GetProperties(BindingFlagsEx.AllInstance)) {
                    if (prop.DeclaringType == t) {
                        if (prop.IsVirtual()) {
                            var key = Tuple.Create(prop.PropertyType, prop.Name);
                            if (!existing.Contains(key)) {
                                yield return prop;
                                existing.Add(key);
                            }
                        } else {
                            yield return prop;
                        }
                    }
                }
                t = t.BaseType;
            }
        }

        private static SerializableMember Handle<TMember>(TMember member, Type memberType, Boolean includeByDefault) where TMember : MemberInfo {
            var xmlAttribute =
                Attribute.GetCustomAttributes(member, inherit: false)
                    .Where(attr => XmlObjectSerializer<T>.SupportedAttributeTypes.Contains(attr.GetType()) ||
                                   XmlObjectSerializer<T>.UnsupportedAttributeTypes.Contains(attr.GetType()))
                    .SingleOrDefault();

            SerializableMember result;

            if (xmlAttribute == null) {
                if (includeByDefault) {
                    result = new SerializableMember(XmlNodeType.Element, new XmlNameDeclaration(member.Name), -1, member);
                } else {
                    // Skip
                    result = null;
                }
            } else if (xmlAttribute is XmlElementAttribute) {
                var elem = (XmlElementAttribute)xmlAttribute;
                if (XmlCollectionSerializer.IsCollection(memberType) && !typeof(IXmlSerializable).IsAssignableFrom(memberType)) {
                    result = new SerializableMember(
                        XmlNodeType.Element,
                        new XmlNameDeclaration(
                            elem.ElementName.EmptyAsNull() ?? member.Name,
                            elem.Namespace.EmptyAsNull()
                        ),
                        elem.Order,
                        member,
                        // If an XmlElement is specified on a collection type the members of the collection are serialized without a containing element.
                        wrapCollection: false
                    );
                } else {
                    result = new SerializableMember(
                        XmlNodeType.Element,
                        new XmlNameDeclaration(
                            elem.ElementName.EmptyAsNull() ?? member.Name,
                            elem.Namespace.EmptyAsNull()
                        ),
                        elem.Order,
                        member
                    );
                }
            } else if (xmlAttribute is XmlAnyElementAttribute) {
                var elem = (XmlAnyElementAttribute)xmlAttribute;
                result = new SerializableMember(
                    XmlNodeType.Element,
                    elem.Order,
                    member
                );
            } else if (xmlAttribute is XmlArrayAttribute) {
                if (!XmlCollectionSerializer.IsCollection(memberType)) {
                    throw new InvalidOperationException(
                        String.Format(
                            "The XmlArrayAttribute was used on a member of type '{0}' which does not implement ICollection<>.",
                            memberType.FullName
                        )
                    );
                }
                var array = (XmlArrayAttribute)xmlAttribute;
                var arrayItems =
                    member.GetCustomAttributes<XmlArrayItemAttribute>().EmptyAsNull();
                result = new SerializableMember(
                    XmlNodeType.Element,
                    new XmlNameDeclaration(
                        array.ElementName.EmptyAsNull() ?? member.Name,
                        array.Namespace.EmptyAsNull()
                    ),
                    array.Order,
                    member,
                    arrayItems: arrayItems
                );
            } else if (xmlAttribute is XmlAttributeAttribute) {
                var attr = (XmlAttributeAttribute)xmlAttribute;
                // Attribute children do not inherit the namespace of their parent.
                result = new SerializableMember(
                    XmlNodeType.Attribute,
                    new XmlNameDeclaration(
                        attr.AttributeName.EmptyAsNull() ?? member.Name,
                        attr.Namespace.EmptyAsNull()
                    ),
                    -1,
                    member
                );
            } else if (xmlAttribute is XmlAnyAttributeAttribute) {
                result = new SerializableMember(
                    XmlNodeType.Attribute,
                    -1,
                    member
                );
            } else if (xmlAttribute is XmlTextAttribute) {
                // var txt = (XmlTextAttribute)xmlAttribute;
                // TODO: support DataType/Type/TypeId parameters
                result = new SerializableMember(XmlNodeType.Text, null, -1, member);
            } else if (xmlAttribute is XmlIgnoreAttribute) {
                // Skip
                result = null;
            } else {
                throw new NotSupportedException(
                    String.Format(
                        "The XML serialization attribute '{0}' is not yet supported",
                        xmlAttribute.GetType().Name
                    )
                );
            }

            return result;
        }

        private class SerializableMember {
            public XmlNodeType NodeType { get; private set; }
            public XmlNameDeclaration MemberName { get; private set; }

            public Boolean MatchAny { get; private set; }
            public Int32 Order { get; private set; }
            public Type MemberType { get; private set; }

            private Boolean isCollection;
            // If false, the children of a collection or array are serialized
            // directly without a containing element
            private Boolean wrapCollection = true;

            /// <summary>
            /// An initializer function that either creates a new instance of
            /// the member type, or retrieves it from the parent if it cannot
            /// be auto-initialized.
            /// </summary>
            private Func<T, Object> getOrCreateInstance;

            /// <summary>
            /// Gets the current value of the member from the parent object
            /// </summary>
            public Func<T, Object> Getter { get; private set; }

            /// <summary>
            /// Sets the value of the member on the parent object and returns
            /// the resulting instance of the parent object. In the case where
            /// the parent is a struct, the delegate must return the mutated
            /// struct since it is not possible to modify the struct by
            /// reference.
            /// </summary>
            public Func<T, Object, T> Setter { get; private set; }

            // The serializer for this member
            private IXmlSerializer serializer;

            // Standard Member
            public SerializableMember(
                XmlNodeType nodeType,
                XmlNameDeclaration memberName,
                Int32 order,
                MemberInfo member) {

                this.MemberName = memberName;

                this.NodeType = nodeType;
                this.Order = order;
                if (member is PropertyInfo) {
                    this.CreateAccessors((PropertyInfo)member);
                } else if (member is FieldInfo) {
                    this.CreateAccessors((FieldInfo)member);
                } else {
                    throw new ArgumentException("Member type not supported.", "member");
                }

                this.serializer = XmlSerializerBase.ForType(this.MemberType);
                if (nodeType == XmlNodeType.Attribute &&
                    !(this.serializer is IXmlPrimitiveSerializer) &&
                    !(this.serializer is IXmlSerializableSerializer) &&
                    !(this.serializer is IXmlEnumSerializer)) {
                    throw new NotSupportedException("Complex types cannot be serialized as xml attributes");
                }
                this.SelectInitializer();
            }

            // XmlAnyAttribute or XmlAnyElement member
            public SerializableMember(
                XmlNodeType nodeType,
                Int32 order,
                MemberInfo member) {

                this.NodeType = nodeType;
                this.Order = order;
                if (member is PropertyInfo) {
                    CreateAccessors((PropertyInfo)member);
                } else if (member is FieldInfo) {
                    CreateAccessors((FieldInfo)member);
                } else {
                    throw new ArgumentException("Member type not supported.", "member");
                }

                this.MatchAny = true;
                this.serializer = XmlSerializerBase.ForType(this.MemberType);
                this.SelectInitializer();
            }

            // Collection member
            public SerializableMember(
                XmlNodeType nodeType,
                XmlNameDeclaration memberName,
                Int32 order,
                MemberInfo member,
                Boolean wrapCollection = true,
                IEnumerable<XmlArrayItemAttribute> arrayItems = null) {

                this.MemberName = memberName;
                this.NodeType = nodeType;
                this.Order = order;
                if (member is PropertyInfo) {
                    CreateAccessors((PropertyInfo)member);
                } else if (member is FieldInfo) {
                    CreateAccessors((FieldInfo)member);
                } else {
                    throw new ArgumentException("Member type not supported.", "member");
                }

                this.isCollection = true;
                this.wrapCollection = wrapCollection;
                this.serializer = this.wrapCollection || arrayItems != null ?
                    XmlCollectionSerializer.ForType(this.MemberType, arrayItems: arrayItems) :
                    XmlCollectionSerializer.ForType(this.MemberType, elementName: this.MemberName);
                this.SelectInitializer();
            }

            private void CreateAccessors(PropertyInfo property) {
                this.MemberType = property.PropertyType;

                this.Getter = property.MakeGetter<T, object>();
                this.Setter = property.CanWrite ? property.MakeStructSafeSetter<T, object>() : null;
            }

            private void CreateAccessors(FieldInfo field) {
                this.MemberType = field.FieldType;

                this.Getter = field.MakeGetter<T, object>();
                this.Setter = field.MakeStructSafeSetter<T, object>();
            }

            private void SelectInitializer() {
                if (this.serializer.CanCreateInstance && this.Setter != null) {
                    this.getOrCreateInstance = _ => this.serializer.CreateInstance();
                } else {
                    this.getOrCreateInstance = this.Getter;
                }
            }

            public void SerializeMember(XmlWriter writer, T instance, XmlSerializableSettings settings) {
                var val = this.Getter(instance);
                if (val != null) {
                    switch (this.NodeType) {
                        case XmlNodeType.Attribute:
                            if (val != null) {
                                writer.WriteStartAttribute(this.MemberName.Name, this.MemberName.Namespace);
                                this.serializer.Serialize(writer, val, settings);
                                writer.WriteEndAttribute();
                            }
                            break;
                        case XmlNodeType.Element:
                            if (this.isCollection) {
                                if (val != null || !settings.OmitNulls) {
                                    if (this.wrapCollection) {
                                        writer.WriteStartElement(this.MemberName.Name, this.MemberName.Namespace);
                                    }

                                    if (val != null) {
                                        this.serializer.Serialize(writer, val, settings);
                                    } else if (this.wrapCollection) {
                                        writer.WriteAttributeString("xsi", "nil", XsiNamespace, "true");
                                    }

                                    if (this.wrapCollection) {
                                        writer.WriteEndElement();
                                    }
                                }
                            } else {
                                if (val != null || !settings.OmitNulls) {
                                    writer.WriteStartElement(this.MemberName.Name, this.MemberName.Namespace);
                                    if (val != null) {
                                        this.serializer.Serialize(writer, val, settings);
                                    } else {
                                        writer.WriteAttributeString("xsi", "nil", XsiNamespace, "true");
                                    }
                                }
                                writer.WriteEndElement();
                            }
                            break;
                        case XmlNodeType.Text:
                            if (val != null) {
                                this.serializer.Serialize(writer, val, settings);
                            }
                            break;
                        default:
                            throw new NotSupportedException(
                                String.Format("The node type {0} is not supported.", this.NodeType)
                            );
                    }
                }
            }

            public T DeserializeMember(XmlReader reader, T instance) {
                var obj = this.getOrCreateInstance(instance);
                obj = this.serializer.Deserialize(reader, obj);
                if (this.Setter != null) {
                    instance = this.Setter(instance, obj);
                }
                return instance;
            }
        }
    }
}
