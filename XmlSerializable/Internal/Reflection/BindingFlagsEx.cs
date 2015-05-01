using System.Reflection;

namespace Serialization.Xml.Internal.Reflection {
    public static class BindingFlagsEx {
        public const BindingFlags AnyVisibility = BindingFlags.Public | BindingFlags.NonPublic;
        public const BindingFlags AllInstance = BindingFlags.Instance | AnyVisibility;
        public const BindingFlags AllStatic = BindingFlags.Static | AnyVisibility;
    }
}
