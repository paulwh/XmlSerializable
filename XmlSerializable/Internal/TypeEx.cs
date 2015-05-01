using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Serialization.Xml.Internal.Reflection;

namespace Serialization.Xml.Internal {
    public static class TypeEx {
        public static string GetReadableName(this Type type) {
            if (type == null) {
                throw new ArgumentNullException("type");
            }

            var sb = new StringBuilder();
            sb.Append(type.FullName.Split('`').First());
            sb.Append('<');
            sb.Append(
              String.Join(
                ", ",
                type.GetGenericArguments().Select(GetReadableName)
              )
            );
            sb.Append('>');
            return sb.ToString();
        }

        public static Boolean HasDefaultConstructor(this Type type, Boolean nonPublic = false) {
            if (type == null) {
                throw new ArgumentNullException("type");
            }

            return type.GetConstructor(BindingFlagsEx.AllInstance, null, Type.EmptyTypes, null) != null;
        }

        public static IEnumerable<Type> GetBaseTypes(this Type type) {
            if (type == null) {
                throw new ArgumentNullException("type");
            }

            while (type.BaseType != null) {
                yield return type.BaseType;
                type = type.BaseType;
            }
        }

        public static Action<Object, Object> MakeSetter(this Type type, String memberName) {
            if (type == null) {
                throw new ArgumentNullException("type");
            }

            var members = type.GetMember(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(mi => mi is PropertyInfo || mi is FieldInfo).ToList();
            if (members.Count == 0) {
                throw new ArgumentException(
                  String.Format("Member '{0}' on type '{1}' not found.", memberName, type.GetReadableName()),
                  "member"
                );
            } else if (members.Count > 1) {
                throw new ArgumentException(
                  String.Format("More than one field or property with the name '{0}' were found on type '{1}'.", memberName, type.GetReadableName())
                );
            }
            var member = members.Single();
            if (member is PropertyInfo) {
                return ((PropertyInfo)member).MakeSetter<Object, Object>();
            } else {
                return ((FieldInfo)member).MakeSetter<Object, Object>();
            }
        }

        public static Func<Object, Object> MakeGetter(this Type type, string memberName) {
            if (type == null) {
                throw new ArgumentNullException("type");
            }

            var members = type.GetMember(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(mi => mi is PropertyInfo || mi is FieldInfo).ToList();
            if (members.Count == 0) {
                throw new ArgumentException(
                  String.Format("Member '{0}' on type '{1}' not found.", memberName, type.GetReadableName()),
                  "member"
                );
            } else if (members.Count > 1) {
                throw new ArgumentException(
                  String.Format("More than one field or property with the name '{0}' were found on type '{1}'.", memberName, type.GetReadableName())
                );
            }
            var member = members.Single();
            if (member is PropertyInfo) {
                return ((PropertyInfo)member).MakeGetter<Object, Object>();
            } else {
                return ((FieldInfo)member).MakeGetter<Object, Object>();
            }
        }

        public static Object GetDefaultValue(this Type type) {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}
