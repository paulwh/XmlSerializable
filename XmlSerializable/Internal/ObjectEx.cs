using System;
using System.Collections.Generic;

namespace Serialization.Xml.Internal {
    public static class ObjectEx {
        public static IEnumerable<T> Replicate<T>(this T obj, Int32 i) {
            while (i-- > 0) {
                yield return obj;
            }
        }

        public static IEnumerable<T> EnumerableOf<T>(this T obj) {
            yield return obj;
        }

        public static T[] ArrayOf<T>(this T obj) {
            return new[] { obj };
        }

        public static List<T> ListOf<T>(this T obj) {
            return new List<T>(new[] { obj });
        }

        public static TResult Bind<T, TResult>(this T obj, Func<T, TResult> func)
            where T : class
            where TResult : class {
            if (obj != null) {
                return func(obj);
            } else {
                return null;
            }
        }

        public static TResult? Bind<T, TResult>(this T? obj, Func<T, TResult?> func)
            where T : struct
            where TResult : struct {

            if (obj.HasValue) {
                return func(obj.Value);
            } else {
                return null;
            }
        }

        public static TResult Bind<T, TResult>(this T? obj, Func<T, TResult> func)
            where T : struct
            where TResult : class {

            if (obj.HasValue) {
                return func(obj.Value);
            } else {
                return null;
            }
        }

        public static TResult? FMap<T, TResult>(this T? obj, Func<T, TResult> func)
            where T : struct
            where TResult : struct {

            if (obj.HasValue) {
                return func(obj.Value);
            } else {
                return null;
            }
        }

        public static Boolean IsNull<T>(this T obj) where T : class {
            return Object.ReferenceEquals(obj, null);
        }

        public static T With<T>(this T obj, Action<T> action) {
            action(obj);
            return obj;
        }

        public static T With<T>(this T obj, Func<T, T> func) {
            return func(obj);
        }
    }
}
