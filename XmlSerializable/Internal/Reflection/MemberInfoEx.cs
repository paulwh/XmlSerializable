using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Serialization.Xml.Internal.Reflection {
    public static class MemberInfoEx {
        public static Action<T, TProperty> MakeSetter<T, TProperty>(this PropertyInfo property) {
            var obj = Expression.Parameter(typeof(T), "obj");
            var val = Expression.Parameter(typeof(TProperty), "val");
            var setter = Expression.Lambda<Action<T, TProperty>>(
                Expression.Assign(
                    Expression.Property(
                        property.DeclaringType != typeof(T) ?
                            Expression.Convert(obj, property.DeclaringType) :
                            (Expression)obj,
                        property
                    ),
                    Expression.Convert(val, property.PropertyType)
                ),
                obj,
                val
            );
            return setter.Compile();
        }

        public static Action<T, TField> MakeSetter<T, TField>(this FieldInfo field) {
            var obj = Expression.Parameter(typeof(T), "obj");
            var val = Expression.Parameter(typeof(TField), "val");
            var setter = Expression.Lambda<Action<T, TField>>(
                Expression.Assign(
                    Expression.Field(
                        field.DeclaringType != typeof(T) ?
                            Expression.Convert(obj, field.DeclaringType) :
                            (Expression)obj,
                        field
                    ),
                    Expression.Convert(val, field.FieldType)
                ),
                obj,
                val
            );
            return setter.Compile();
        }

        public static Func<T, TProperty, T> MakeStructSafeSetter<T, TProperty>(this PropertyInfo property) {
            var obj = Expression.Parameter(typeof(T), "obj");
            var val = Expression.Parameter(typeof(TProperty), "val");
            var setter = Expression.Lambda<Func<T, TProperty, T>>(
                Expression.Block(
                    Expression.Assign(
                        Expression.Property(obj, property),
                        Expression.Convert(val, property.PropertyType)
                    ),
                    obj
                ),
                obj,
                val
            );
            return setter.Compile();
        }

        public static Func<T, TField, T> MakeStructSafeSetter<T, TField>(this FieldInfo field) {
            var obj = Expression.Parameter(typeof(T), "obj");
            var val = Expression.Parameter(typeof(TField), "val");
            var setter = Expression.Lambda<Func<T, TField, T>>(
                Expression.Block(
                    Expression.Assign(
                        Expression.Field(obj, field),
                        Expression.Convert(val, field.FieldType)
                    ),
                    obj
                ),
                obj,
                val
            );
            return setter.Compile();
        }

        public static Func<T, TProperty> MakeGetter<T, TProperty>(this PropertyInfo property) {
            var obj = Expression.Parameter(typeof(T), "obj");
            var getter = Expression.Lambda<Func<T, TProperty>>(
                Expression.Convert(
                    Expression.Property(
                        property.DeclaringType != typeof(T) ?
                            Expression.Convert(obj, property.DeclaringType) :
                            (Expression)obj,
                        property
                    ),
                    typeof(TProperty)
                ),
                obj
            );
            return getter.Compile();
        }

        public static Func<T, TField> MakeGetter<T, TField>(this FieldInfo field) {
            var obj = Expression.Parameter(typeof(T), "obj");
            var getter = Expression.Lambda<Func<T, TField>>(
                Expression.Convert(
                  Expression.Field(
                    field.DeclaringType != typeof(T) ?
                        Expression.Convert(obj, field.DeclaringType) :
                        (Expression)obj,
                    field
                  ),
                  typeof(TField)
                ),
                obj
            );
            return getter.Compile();
        }

        public static Boolean IsPublic(this MemberInfo member) {
            switch (member.MemberType) {
                case MemberTypes.Constructor:
                case MemberTypes.Method:
                    return ((MethodBase)member).IsPublic;
                case MemberTypes.Event:
                    var ev = (EventInfo)member;
                    return ev.GetAddMethod().IsPublic && ev.GetRemoveMethod().IsPublic;
                case MemberTypes.Field:
                    return ((FieldInfo)member).IsPublic;
                case MemberTypes.Property:
                    var accessors = ((PropertyInfo)member).GetAccessors(nonPublic: true);
                    return accessors.Any() && accessors.All(mi => mi.IsPublic);
                case MemberTypes.NestedType:
                    return ((Type)member).IsPublic;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
