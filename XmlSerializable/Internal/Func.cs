using System;
using System.Collections.Generic;

namespace Serialization.Xml.Internal {
    public static class Func {
        public static T Identity<T>(T obj) {
            return obj;
        }

        public static bool Null<T>(T obj) where T : class {
            return obj == null;
        }

        public static bool NotNull<T>(T obj) where T : class {
            return obj != null;
        }

        public static IEnumerable<T> Repeat<T>(Func<T> gen) {
            while (true) {
                yield return gen();
            }
        }

        /// <summary>
        /// Partially applies an action with one argument.
        /// </summary>
        /// <remarks>
        /// <c>var act2 = act1.Apply(val)</c> instead of 
        /// <c>var act2 = () => act1(val)</c>
        /// </remarks>
        public static Action Apply<T>(this Action<T> action, T val) {
            return () => action(val);
        }

        /// <summary>
        /// Partially applies an action with two argument.
        /// </summary>
        public static Action<T2> Apply<T1, T2>(this Action<T1, T2> action, T1 val1) {
            return val2 => action(val1, val2);
        }

        /// <summary>
        /// Partially applies an action with two argument.
        /// </summary>
        public static Action Apply<T1, T2>(this Action<T1, T2> action, T1 val1, T2 val2) {
            return () => action(val1, val2);
        }

        /// <summary>
        /// Partially applies an action with three argument.
        /// </summary>
        public static Action<T2, T3> Apply<T1, T2, T3>(this Action<T1, T2, T3> action, T1 val1) {
            return (val2, val3) => action(val1, val2, val3);
        }

        /// <summary>
        /// Partially applies an action with three argument.
        /// </summary>
        public static Action<T3> Apply<T1, T2, T3>(this Action<T1, T2, T3> action, T1 val1, T2 val2) {
            return val3 => action(val1, val2, val3);
        }

        /// <summary>
        /// Partially applies an action with three argument.
        /// </summary>
        public static Action Apply<T1, T2, T3>(this Action<T1, T2, T3> action, T1 val1, T2 val2, T3 val3) {
            return () => action(val1, val2, val3);
        }

        /// <summary>
        /// Partially applies an action with four argument.
        /// </summary>
        public static Action<T2, T3, T4> Apply<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> action, T1 val1) {
            return (val2, val3, val4) => action(val1, val2, val3, val4);
        }

        /// <summary>
        /// Partially applies an action with four argument.
        /// </summary>
        public static Action<T3, T4> Apply<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> action, T1 val1, T2 val2) {
            return (val3, val4) => action(val1, val2, val3, val4);
        }

        /// <summary>
        /// Partially applies an action with four argument.
        /// </summary>
        public static Action<T4> Apply<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> action, T1 val1, T2 val2, T3 val3) {
            return (val4) => action(val1, val2, val3, val4);
        }

        /// <summary>
        /// Partially applies an action with four argument.
        /// </summary>
        public static Action Apply<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> action, T1 val1, T2 val2, T3 val3, T4 val4) {
            return () => action(val1, val2, val3, val4);
        }

        /// <summary>
        /// Partially applies an func with one argument.
        /// </summary>
        /// <remarks>
        /// <c>var act2 = act1.Apply(val)</c> instead of 
        /// <c>var act2 = () => act1(val)</c>
        /// </remarks>
        public static Func<TResult> Apply<T, TResult>(this Func<T, TResult> func, T val) {
            return () => func(val);
        }

        /// <summary>
        /// Partially applies an func with two argument.
        /// </summary>
        public static Func<T2, TResult> Apply<T1, T2, TResult>(this Func<T1, T2, TResult> func, T1 val1) {
            return val2 => func(val1, val2);
        }

        /// <summary>
        /// Partially applies an func with two argument.
        /// </summary>
        public static Func<TResult> Apply<T1, T2, TResult>(this Func<T1, T2, TResult> func, T1 val1, T2 val2) {
            return () => func(val1, val2);
        }

        /// <summary>
        /// Partially applies an func with three argument.
        /// </summary>
        public static Func<T2, T3, TResult> Apply<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> func, T1 val1) {
            return (val2, val3) => func(val1, val2, val3);
        }

        /// <summary>
        /// Partially applies an func with three argument.
        /// </summary>
        public static Func<T3, TResult> Apply<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> func, T1 val1, T2 val2) {
            return val3 => func(val1, val2, val3);
        }

        /// <summary>
        /// Partially applies an func with three argument.
        /// </summary>
        public static Func<TResult> Apply<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> func, T1 val1, T2 val2, T3 val3) {
            return () => func(val1, val2, val3);
        }

        /// <summary>
        /// Partially applies an func with four argument.
        /// </summary>
        public static Func<T2, T3, T4, TResult> Apply<T1, T2, T3, T4, TResult>(this Func<T1, T2, T3, T4, TResult> func, T1 val1) {
            return (val2, val3, val4) => func(val1, val2, val3, val4);
        }

        /// <summary>
        /// Partially applies an func with four argument.
        /// </summary>
        public static Func<T3, T4, TResult> Apply<T1, T2, T3, T4, TResult>(this Func<T1, T2, T3, T4, TResult> func, T1 val1, T2 val2) {
            return (val3, val4) => func(val1, val2, val3, val4);
        }

        /// <summary>
        /// Partially applies an func with four argument.
        /// </summary>
        public static Func<T4, TResult> Apply<T1, T2, T3, T4, TResult>(this Func<T1, T2, T3, T4, TResult> func, T1 val1, T2 val2, T3 val3) {
            return (val4) => func(val1, val2, val3, val4);
        }

        /// <summary>
        /// Partially applies an func with four argument.
        /// </summary>
        public static Func<TResult> Apply<T1, T2, T3, T4, TResult>(this Func<T1, T2, T3, T4, TResult> func, T1 val1, T2 val2, T3 val3, T4 val4) {
            return () => func(val1, val2, val3, val4);
        }
    }
}
