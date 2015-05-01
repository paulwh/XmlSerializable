using System;
using System.Collections.Generic;
using System.Xml;

namespace Serialization.Xml.Internal {
    /// <summary>
    /// Compares XmlQualifiedName such that the null namespace acts like a wildcard.
    /// </summary>
    public class XmlQualifiedNameComparer : IEqualityComparer<XmlQualifiedName> {
        public Boolean Equals(XmlQualifiedName x, XmlQualifiedName y) {
            if (x == null) {
                return y == null;
            } else if (y == null) {
                return false;
            } else {
                return x.Name == y.Name &&
                    (x.Namespace == null ||
                     y.Namespace == null ||
                     x.Namespace == y.Namespace);
            }
        }

        public Int32 GetHashCode(XmlQualifiedName obj) {
            return obj.Name.GetHashCode();
        }
    }
}
