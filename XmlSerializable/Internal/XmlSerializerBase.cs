using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Serialization.Xml.Internal.Reflection;

namespace Serialization.Xml.Internal {
    internal abstract class XmlSerializerBase : IXmlSerializer {
        public const string XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";

        public virtual Boolean CanCreateInstance { get { return true; } }

        Object IXmlSerializer.CreateInstance() { throw new NotImplementedException(); }

        public abstract void Serialize(XmlWriter writer, Object obj, XmlSerializableSettings settings);

        public abstract Object Deserialize(XmlReader reader, Object instance, Action<XmlReader> onUnknownElement = null);

        public static IXmlSerializer ForType(Type t) {
            if (t == null) {
                throw new ArgumentNullException("t");
            }
            t = Nullable.GetUnderlyingType(t) ?? t;
            return (IXmlSerializer)CreateSerializerMethod.MakeGenericMethod(t).Invoke(null, null);
        }

        private static readonly MethodInfo CreateSerializerMethod =
            ReflectionHelper.GetMethod(() => XmlSerializerBase.CreateSerializer<Object>()).GetGenericMethodDefinition();

        private static IXmlSerializer<T> CreateSerializer<T>() {
            return XmlSerializerBase<T>.CreateSerializer();
        }

        internal static XmlNameDeclaration GetRootNameForType(Type t) {
            XmlNameDeclaration result = XmlPrimitiveSerializer.TryGetPrimitiveTypeName(t).Bind(str => new XmlNameDeclaration(str));
            if (result == null) {
                var rootAttr = t.GetCustomAttributes<XmlRootAttribute>(inherit: false).FirstOrDefault();
                if (rootAttr != null) {
                    result = new XmlNameDeclaration(rootAttr.ElementName, rootAttr.Namespace);
                } else {
                    result = new XmlNameDeclaration(GetSimpleTypeName(t));
                }
            }
            return result;
        }

        private static String GetSimpleTypeName(Type t) {
            String result = XmlPrimitiveSerializer.TryGetPrimitiveTypeName(t);
            if (result == null) {
                if (t.IsArray) {
                    result = "ArrayOf" + FirstLetterUpper(GetSimpleTypeName(t.GetElementType()));
                } else if (t.GetInterface("ICollection`1") != null) {
                    // for backward compat
                    var elementType = t.GetInterface("ICollection`1").GetGenericArguments()[0];
                    result = "ArrayOf" + FirstLetterUpper(GetSimpleTypeName(elementType));
                } else if (t.IsGenericType) {
                    result = t.Name.Split('`')[0] + "Of" + String.Join(String.Empty, t.GetGenericArguments().Select(GetSimpleTypeName));
                } else {
                    result = t.Name;
                }
            }
            return result;
        }

        private static string FirstLetterUpper(string type) {
            if (Char.IsLower(type[0])) {
                var sb = new StringBuilder(type.Length);
                sb.Append(Char.ToUpperInvariant(type[0]));
                sb.Append(type, 1, type.Length - 1);
                return sb.ToString();
            } else {
                return type;
            }
        }
    }

    internal abstract class XmlSerializerBase<T> : XmlSerializerBase, IXmlSerializer<T> {
        private static readonly Func<T> Initializer;

        static XmlSerializerBase() {
            if (typeof(T).HasDefaultConstructor(nonPublic: true)) {
                Initializer = () => (T)Activator.CreateInstance(typeof(T), nonPublic: true);
            }
        }

        public override bool CanCreateInstance {
            get {
                return Initializer != null;
            }
        }

        public virtual T CreateInstance() {
            if (Initializer == null) {
                throw new NotSupportedException(
                    String.Format(
                        "The type '{0}' cannot be initialized because it has no parameterless constructor.",
                        typeof(T).FullName
                    )
                );
            }
            return Initializer();
        }

        Object IXmlSerializer.CreateInstance() {
            return this.CreateInstance();
        }

        public abstract void Serialize(XmlWriter writer, T obj, XmlSerializableSettings settings);

        public abstract T Deserialize(XmlReader reader, T instance, Action<XmlReader> onUnknownElement = null);

        public override void Serialize(XmlWriter writer, Object obj, XmlSerializableSettings settings) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }
            this.Serialize(writer, (T)obj, settings);
        }

        public override Object Deserialize(XmlReader reader, Object instance, Action<XmlReader> onUnknownElement = null) {
            return this.Deserialize(reader, (T)(instance ?? default(T)), onUnknownElement);
        }

        public static IXmlSerializer<T> CreateSerializer() {
            // Possibilities:
            //   T is a primitive
            //   T is a type that implements IXmlSerializable 
            //   T is an Array or Collection Type
            //      T has an elementType
            //      T has an elementType w/ known subtypes specified by XmlArrayItem(s)
            //   T is some other type
            //      T has known subtypes specified via XmlInclude
            if (Nullable.GetUnderlyingType(typeof(T)) != null) {
                throw new InvalidOperationException(
                    "Serializers should not be created for nullable types"
                );
            }
            IXmlSerializer<T> serializer = XmlPrimitiveSerializer.TryGetPrimitiveSerializer<T>();
            if (serializer == null) {
                if (typeof(T).IsEnum) {
                    serializer = (IXmlSerializer<T>)XmlEnumSerializer.ForType(typeof(T));
                } else if (typeof(IXmlSerializable).IsAssignableFrom(typeof(T)) && !typeof(XmlSerializable<T>).IsAssignableFrom(typeof(T))) {
                    // Pass through to IXmlSerializable implementation
                    serializer = XmlSerializableSerializer.ForType<T>();
                } else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(typeof(T)) && !typeof(IXmlSerializable).IsAssignableFrom(typeof(T))) {
                    // Collection type
                    serializer = XmlCollectionSerializer.CreateSerializer<T>();
                } else {
                    // Serialize the object's members
                    serializer = new XmlObjectSerializer<T>();
                }
            }
            return serializer;
        }
    }
}
