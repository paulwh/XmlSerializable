using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Serialization.Xml.Internal {
    public static class StringEx {
        public static String EmptyAsNull(this String str) {
            return String.IsNullOrEmpty(str) ? null : str;
        }

        public static String NullAsEmpty(this String str) {
            return str == null ? String.Empty : str;
        }

        public static Boolean IsNullOrEmpty(this String str) {
            return String.IsNullOrEmpty(str);
        }

        public static String Strip(this String str, params Char[] characters) {
            return str.StripL().StripR();
        }

        public static String StripL(this String str, params Char[] chars) {
            Int32 fc = 0;

            // Remove characters from the beginning
            for (; fc < str.Length; fc++) {
                if (chars.Any(c => c != str[fc])) break;
            }

            if (fc == str.Length) return String.Empty;
            else return str.Substring(fc);
        }

        public static String StripR(this String str, params Char[] chars) {
            Int32 lc = str.Length - 1;

            // Remove characters from the end
            for (; lc >= 0; lc--) {
                if (chars.Any(c => c != str[lc])) break;
            }

            if (lc < 0) return String.Empty;
            else return str.Substring(0, lc + 1);
        }


        /// <summary>
        /// Counts the occurances of a character in a string.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Int32 CountOccurances(this String str, Char c) {
            Int32 count = 0;

            foreach (Char s in str) {
                if (s == c) {
                    count++;
                }
            }

            return count;
        }

        public static String Join(String seperator, IEnumerable collection) {
            StringBuilder sb = new StringBuilder();
            Boolean first = true;

            foreach (Object obj in collection) {
                if (!first) {
                    sb.Append(seperator);
                } else {
                    first = false;
                }

                if (obj != null) {
                    if (obj.GetType().IsArray) {
                        sb.Append(obj.GetType().Name);
                        sb.Append(" { ");
                        sb.Append(StringEx.Join(", ", (Array)obj));
                        sb.Append(" }");
                    } else {
                        sb.Append(obj.ToString());
                    }
                }
            }

            return sb.ToString();
        }

        public static String Join<T>(String separator, IEnumerable<T> objects, Converter<T, String> toString) {
            StringBuilder sb = new StringBuilder();

            Boolean first = true;

            foreach (T o in objects) {
                if (!first) {
                    sb.Append(separator);
                } else {
                    first = false;
                }

                sb.Append(toString(o));
            }

            return sb.ToString();
        }

        public static String Join(String separator, IEnumerable objects, String displayMember) {
            StringBuilder sb = new StringBuilder();

            Boolean first = true;

            foreach (Object o in objects) {
                if (!first) {
                    sb.Append(separator);
                } else {
                    first = false;
                }

                if (o != null) {
                    PropertyInfo member = o.GetType().GetProperty(displayMember);

                    if (member == null) {
                        throw new ArgumentException("Display Member Not Found.", "displayMember");
                    }

                    Object memberValue = member.GetValue(o, new Object[] { });

                    if (memberValue != null) {
                        sb.Append(memberValue);
                    }
                }
            }

            return sb.ToString();
        }

        public static Boolean IsSubstringAt(this String str, String sub, Int32 offset) {
            Int32 i = 0;
            for (; i < sub.Length && i + offset < str.Length; i++) {
                if (str[offset + i] != sub[i]) return false;
            }

            return i == sub.Length;
        }

        public static String Escape(this String str, Char escape, params Char[] toEscape) {
            Array.Sort(toEscape);
            var sb = new StringBuilder();
            var j = 0;
            for (var i = 0; i < str.Length; i++) {
                var c = str[i];
                if (c == escape || Array.BinarySearch(toEscape, c) >= 0) {
                    if (i > j) {
                        sb.Append(str.Substring(j, i - j));
                    }

                    sb.Append(escape);
                    sb.Append(str[i]);
                    j = i + 1;
                }
            }

            if (j < str.Length) {
                sb.Append(str.Substring(j));
            }
            return sb.ToString();
        }

        public static Boolean StartsWith(this String str, Char c) {
            return str.Length > 0 && str[0] == c;
        }

        public static Boolean StartsWith(this String str, Char c, StringComparison options) {
            return str.StartsWith(new String(c, 1), options);
        }

        public static Boolean ContainsAny(this String str, params Char[] chars) {
            foreach (var c in str) {
                if (chars.Contains(c)) {
                    return true;
                }
            }

            return false;
        }

        public static String Quote(this String str, Char quote = '"', Char escape = '\\') {
            var builder = new StringBuilder(str.Length + 2);
            builder.Append(quote);
            var start = 0;
            var i = 0;
            for (; i < str.Length; i++) {
                if (str[i] == quote || str[i] == escape) {
                    if (i > start) {
                        builder.Append(str, start, i - start);
                    }
                    builder.Append(escape);
                    builder.Append(str[i]);
                    start = i + 1;
                }
            }
            if (i > start) {
                builder.Append(str, start, i - start);
            }
            builder.Append(quote);

            return builder.ToString();
        }

        public static Boolean EqualsOrdinal(this String str1, String str2, Boolean ignoreCase = false) {
            return str1.Equals(
                str2,
                ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal
            );
        }
    }
}
