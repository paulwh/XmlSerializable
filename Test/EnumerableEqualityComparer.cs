using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Serialization.Xml.Internal;

namespace Serialization.Xml.Test {
    public class EnumerableEqualityComparer : IEqualityComparer {
        private static readonly IEqualityComparer defaultInstance =
            new EnumerableEqualityComparer(EqualityComparer<Object>.Default);

        public static IEqualityComparer Default {
            get { return defaultInstance; }
        }

        private IEqualityComparer elementComparer;

        public EnumerableEqualityComparer(IEqualityComparer elementComparer) {
            this.elementComparer = elementComparer;
        }

        public new bool Equals(object x, object y) {
            var iter1 = ((IEnumerable)x).GetEnumerator();
            var iter2 = ((IEnumerable)x).GetEnumerator();

            var iter1HasCurrent = iter1.MoveNext();
            var iter2HasCurrent = iter2.MoveNext();

            while (iter1HasCurrent && iter2HasCurrent) {
                if (!this.elementComparer.Equals(iter1.Current, iter2.Current)) {
                    break;
                }
                iter1HasCurrent = iter1.MoveNext();
                iter2HasCurrent = iter2.MoveNext();
            }

            return !iter1HasCurrent && !iter2HasCurrent;
        }

        public int GetHashCode(object obj) {
            return HashCode.FromHashCodes(
                ((IEnumerable)obj).Cast<object>().Select(this.elementComparer.GetHashCode)
            );
        }
    }

    public class EnumerableEqualityComparer<T> : IEqualityComparer<IEnumerable<T>> {
        private static readonly IEqualityComparer<IEnumerable<T>> defaultInstance =
            new EnumerableEqualityComparer<T>(EqualityComparer<T>.Default);

        public static IEqualityComparer<IEnumerable<T>> Default {
            get { return defaultInstance; }
        }

        private IEqualityComparer<T> elementComparer;

        public EnumerableEqualityComparer(IEqualityComparer<T> elementComparer) {
            this.elementComparer = elementComparer;
        }

        public bool Equals(IEnumerable<T> x, IEnumerable<T> y) {
            var iter1 = x.GetEnumerator();
            var iter2 = y.GetEnumerator();

            var iter1HasCurrent = iter1.MoveNext();
            var iter2HasCurrent = iter2.MoveNext();

            while (iter1HasCurrent && iter2HasCurrent) {
                if (!this.elementComparer.Equals(iter1.Current, iter2.Current)) {
                    break;
                }
                iter1HasCurrent = iter1.MoveNext();
                iter2HasCurrent = iter2.MoveNext();
            }

            return !iter1HasCurrent && !iter2HasCurrent;
        }

        public int GetHashCode(IEnumerable<T> obj) {
            return HashCode.FromHashCodes(
                obj.Select(this.elementComparer.GetHashCode)
            );
        }
    }
}
