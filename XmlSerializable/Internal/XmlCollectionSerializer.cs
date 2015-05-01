using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Serialization.Xml.Internal.Reflection;

namespace Serialization.Xml.Internal {
    internal static class XmlCollectionSerializer {
        public static IXmlSerializer ForType(Type collectionType, XmlNameDeclaration elementName = null, IEnumerable<XmlArrayItemAttribute> arrayItems = null) {
            return (IXmlSerializer)
                CreateSerializerMethod
                    .MakeGenericMethod(collectionType)
                    .Invoke(null, new object[] { elementName, arrayItems });
        }

        private static readonly MethodInfo CreateSerializerMethod =
            ReflectionHelper.GetGenericMethodDefinition(() => XmlCollectionSerializer.CreateSerializer<List<String>>(null, null));

        public static IXmlSerializer<TCollection> CreateSerializer<TCollection>(XmlNameDeclaration elementName = null, IEnumerable<XmlArrayItemAttribute> arrayItems = null) {
            var collectionInterface = typeof(TCollection).GetInterface("ICollection`1");
            if (collectionInterface == null) {
                throw new InvalidOperationException(
                    "XmlCollectionSerializer only supports collection that implement the generic ICollection<> interface."
                );
            }

            var elementType = collectionInterface.GetGenericArguments()[0];
            return (IXmlSerializer<TCollection>)
                CreateSerializerMethod2
                    .MakeGenericMethod(typeof(TCollection), elementType)
                    .Invoke(null, new object[] { elementName, arrayItems });
        }

        private static readonly MethodInfo CreateSerializerMethod2 =
            ReflectionHelper.GetGenericMethodDefinition(() => XmlCollectionSerializer.CreateSerializer<List<String>, String>(null, null));

        public static XmlCollectionSerializer<TCollection, TElement> CreateSerializer<TCollection, TElement>(XmlNameDeclaration elementName = null, IEnumerable<XmlArrayItemAttribute> arrayItems = null)
            where TCollection : ICollection<TElement> {
            return new XmlCollectionSerializer<TCollection, TElement>(elementName, arrayItems);
        }

        public static Boolean IsCollection(Type type) {
            return type.GetInterface("ICollection`1") != null;
        }
    }

    internal class XmlCollectionSerializer<TCollection, TElement> : XmlSerializerBase<TCollection> where TCollection : ICollection<TElement> {
        private static readonly IXmlSerializer<TElement> BaseSerializer;
        private static XmlNameDeclaration DefaultElementName {
            get {
                return XmlSerializerBase.GetRootNameForType(typeof(TElement));
            }
        }
        private static readonly Boolean IsArray;
        private static readonly Func<TCollection> CollectionInitializer;

        private XmlNameDeclaration elementName;
        private Dictionary<Type, Tuple<XmlNameDeclaration, IXmlSerializer>> itemSerializers;
        private Dictionary<XmlNameDeclaration, IXmlSerializer> itemSerializerByName;
        private Boolean isWrapped;

        static XmlCollectionSerializer() {
            BaseSerializer = XmlSerializerBase<TElement>.CreateSerializer();
            IsArray = typeof(TCollection).IsArray;

            var interfaceInitializers =
                new Dictionary<Type, Func<TCollection>> {
                    { typeof(IEnumerable<>), () => (TCollection)(Object)new LinkedList<TElement>() },
                    { typeof(ICollection<>), () => (TCollection)(Object)new LinkedList<TElement>() },
                    { typeof(IList<>), () => (TCollection)(Object)new List<TElement>() },
                    { typeof(ISet<>), () => (TCollection)(Object)new HashSet<TElement>() },
                };

            if (IsArray) {
                CollectionInitializer = () => (TCollection)(Object)new TElement[0];
            } else if (typeof(TCollection).IsInterface) {
                if (typeof(TCollection).GetGenericTypeDefinition() == typeof(IDictionary<,>)) {
                    var dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(TElement).GetGenericArguments());
                    CollectionInitializer = () => (TCollection)Activator.CreateInstance(dictionaryType);
                } else {
                    interfaceInitializers.TryGetValue(
                        typeof(TCollection).GetGenericTypeDefinition(),
                        out CollectionInitializer
                    );
                }
            } else if (typeof(TCollection).HasDefaultConstructor(nonPublic: true)) {
                CollectionInitializer = () => (TCollection)Activator.CreateInstance(typeof(TCollection), nonPublic: true);
            }
        }

        public XmlCollectionSerializer(XmlNameDeclaration elementName = null, IEnumerable<XmlArrayItemAttribute> arrayItems = null) {
            this.elementName = elementName ?? DefaultElementName;
            if (this.elementName == null) {
                throw new InvalidOperationException("DefaultElementName must not be null.");
            }
            this.isWrapped = elementName == null;

            if (arrayItems != null) {
                this.itemSerializers = new Dictionary<Type, Tuple<XmlNameDeclaration, IXmlSerializer>>();
                this.itemSerializerByName = new Dictionary<XmlNameDeclaration, IXmlSerializer>();
                foreach (var arrayItem in arrayItems) {
                    var type = arrayItem.Type ?? typeof(TElement);
                    if (arrayItem.Type != null && !typeof(TElement).IsAssignableFrom(arrayItem.Type)) {
                        throw new InvalidOperationException(
                            String.Format(
                                "The specified XmlArrayItem Type '{0}' does not derive from the container's element type '{1}'.",
                                arrayItem.Type.FullName,
                                typeof(TElement).FullName
                            )
                        );
                    }
                    var serializer = XmlSerializerBase.ForType(type);
                    var arrayItemName =
                        !String.IsNullOrEmpty(arrayItem.ElementName) ?
                        new XmlNameDeclaration(arrayItem.ElementName, arrayItem.Namespace) :
                        XmlSerializerBase.GetRootNameForType(type);

                    this.itemSerializers.Add(type, Tuple.Create(arrayItemName, serializer));
                    this.itemSerializerByName.Add(arrayItemName, serializer);
                }
            }
        }

        public override bool CanCreateInstance {
            get {
                return CollectionInitializer != null;
            }
        }

        public override TCollection CreateInstance() {
            if (CollectionInitializer == null) {
                throw new NotSupportedException(
                    String.Format(
                        "The type '{0}' cannot be initialized because it has no parameterless constructor.",
                        typeof(TCollection).FullName
                    )
                );
            }
            return CollectionInitializer();
        }

        public override void Serialize(XmlWriter writer, TCollection obj, XmlSerializableSettings settings) {
            foreach (var item in obj) {
                if (this.itemSerializers != null) {
                    Tuple<XmlNameDeclaration, IXmlSerializer> itemSerializer;
                    if (item == null) {
                        // Ignore nulls, since we cannot determine their
                        // type, and this is what XmlSerializer does
                    } else if (this.TryGetItemSerializerForType(item.GetType(), out itemSerializer)) {
                        writer.WriteStartElement(itemSerializer.Item1.Name, itemSerializer.Item1.Namespace);
                        itemSerializer.Item2.Serialize(writer, item, settings);
                        writer.WriteEndElement();
                    } else {
                        throw new InvalidOperationException("The collection contained a type that was not listed as an XmlArrayItem");
                    }
                } else {
                    writer.WriteStartElement(this.elementName.Name, this.elementName.Namespace);

                    if (item != null) {
                        BaseSerializer.Serialize(writer, item, settings);
                    } else {
                        writer.WriteAttributeString("xsi", "nil", XsiNamespace, "true");
                    }

                    writer.WriteEndElement();
                }
            }
        }

        public override TCollection Deserialize(XmlReader reader, TCollection instance, Action<XmlReader> onUnknownElement = null) {
            XmlQualifiedName rootName = null;
            if (this.isWrapped) {
                // The items of this collection are wrapped in a containing
                // element that needs to be consumed
                if (reader.IsEmptyElement) {
                    reader.Skip();
                    return instance;
                } else {
                    rootName = reader.GetQualifiedName();
                    reader.ReadStartElement();
                }
            }
            var collection = IsArray ? (ICollection<TElement>)new LinkedList<TElement>() : instance;
            var done = false;
            while (!done) {
                switch (reader.NodeType) {
                    case XmlNodeType.Element:
                        if (this.itemSerializers != null) {
                            IXmlSerializer itemSerializer;
                            if (this.itemSerializerByName.TryGetValue(reader.GetQualifiedName(), out itemSerializer)) {
                                instance.Add((TElement)itemSerializer.Deserialize(reader, itemSerializer.CreateInstance()));
                            } else {
                                // If the collection is wrapped then we can safely ignore unrecognized elements
                                if (onUnknownElement != null) {
                                    onUnknownElement(reader);
                                } else {
                                    reader.Skip();
                                }
                            }
                        } else if (reader.IsStartElement(this.elementName)) {
                            // As long as we haven't reached the end element, let the child serializer deal with the element.
                            collection.Add(BaseSerializer.Deserialize(reader, BaseSerializer.CreateInstance()));
                        } else if (this.isWrapped) {
                            // If the collection is wrapped then we can safely ignore unrecognized elements
                            reader.Skip();
                        } else {
                            done = true;
                        }

                        break;
                    case XmlNodeType.EndElement:
                        done = true;
                        break;
                    case XmlNodeType.Comment:
                    case XmlNodeType.ProcessingInstruction:
                    case XmlNodeType.Whitespace:
                    default:
                        reader.Skip();
                        break;
                }
            }
            if (this.isWrapped) {
                if (reader.NodeType != XmlNodeType.EndElement) {
                    throw new XmlException(
                        String.Format(
                            "Expected end element: </ {0}> not found.",
                            rootName
                        )
                    );
                }
                // consume the end element
                reader.Read();
            }

            return IsArray ? (TCollection)(Object)collection.ToArray() : instance;
        }

        private Boolean TryGetItemSerializerForType(Type type, out Tuple<XmlNameDeclaration, IXmlSerializer> serializerAndName) {
            serializerAndName = null;
            while (serializerAndName == null && type != null) {
                this.itemSerializers.TryGetValue(type, out serializerAndName);
                type = type.BaseType;
            }
            return serializerAndName != null;
        }
    }
}
