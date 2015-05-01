using System;

namespace Serialization.Xml.Internal {
    public static class EnumEx {
        public static Boolean TryParse<TEnum>(String str, ref TEnum val, Boolean ignoreCase = false) where TEnum : struct {
            if (!typeof(TEnum).IsEnum) {
                throw new ArgumentException("The type used is not an enum", "val");
            }
            TEnum tmp;
            if (Enum.TryParse<TEnum>(str, ignoreCase, out tmp)) {
                val = tmp;
                return true;
            } else {
                return false;
            }
        }

        public static TEnum Parse<TEnum>(String str, Boolean ignoreCase = false) where TEnum : struct {
            TEnum tmp;
            if (Enum.TryParse<TEnum>(str, ignoreCase, out tmp)) {
                return tmp;
            } else {
                throw new ArgumentException("The value was not a member of the enum.", "str");
            }
        }
    }
}
