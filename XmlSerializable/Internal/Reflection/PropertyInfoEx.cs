using System;
using System.Reflection;

namespace Serialization.Xml.Internal.Reflection {
    public static class PropertyInfoEx {
        public static Boolean IsVirtual(this PropertyInfo property) {
            return property.CanRead && property.GetGetMethod(true).Attributes.HasFlag(MethodAttributes.Virtual) ||
                property.CanWrite && property.GetSetMethod(true).Attributes.HasFlag(MethodAttributes.Virtual);
        }

        public static Boolean HasPublicSetter(this PropertyInfo property) {
            var getter = property.GetGetMethod();
            return getter != null && getter.IsPublic;
        }
    }
}
