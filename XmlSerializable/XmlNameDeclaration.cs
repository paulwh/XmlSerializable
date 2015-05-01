using System;
using System.Xml;
using Serialization.Xml.Internal;

namespace Serialization.Xml {
    /// <summary>
    /// Similar to XmlQualifiedName but permits null namespaces which act like a wildcard when matching.
    /// </summary>
    public class XmlNameDeclaration {
        public String Name { get; private set; }

        public String Namespace { get; private set; }

        public XmlNameDeclaration(String name) {
            this.Name = name;
            this.Namespace = null;
        }

        public XmlNameDeclaration(String name, String @namespace) {
            this.Name = name;
            this.Namespace = @namespace;
        }

        public static implicit operator XmlNameDeclaration(XmlQualifiedName qualifiedName) {
            return new XmlNameDeclaration(qualifiedName.Name, qualifiedName.Namespace);
        }

        public override Boolean Equals(Object obj) {
            var other = obj as XmlNameDeclaration ?? (XmlNameDeclaration)(obj as XmlQualifiedName);
            return !Object.ReferenceEquals(other, null) &&
                this.Name == other.Name &&
                (this.Namespace == null ||
                other.Namespace == null ||
                this.Namespace == other.Namespace);
        }

        public override Int32 GetHashCode() {
            // Namespace cannot be used in our hashcode because two objects may
            // be equal despite having differen Namepsaces so long as one of
            // them is null.
            return HashCode.From(typeof(XmlNameDeclaration), this.Name);
        }

        public override String ToString() {
            return this.Namespace != null ?
                String.Format("{0}:{1}", this.Namespace, this.Name) :
                this.Name;
        }
    }
}
