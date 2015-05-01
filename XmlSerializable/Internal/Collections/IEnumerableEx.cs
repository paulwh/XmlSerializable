using System;
using System.Collections.Generic;
using System.Linq;

namespace Serialization.Xml.Internal.Collections {
    public static class IEnumerableEx {
        public static Boolean IsEquivalentTo<T>(this IEnumerable<T> col1, IEnumerable<T> col2) {
            if (col1 == null) {
                throw new ArgumentNullException("col1");
            } else if (col2 == null) {
                throw new ArgumentNullException("col2");
            }
            IEnumerator<T> iter1 = col1.GetEnumerator();
            IEnumerator<T> iter2 = col2.GetEnumerator();

            Boolean hasNext1 = iter1.MoveNext(), hasNext2 = iter2.MoveNext();
            while (hasNext1 && hasNext2) {
                if (iter1.Current == null) {
                    if (iter2.Current != null) return false;
                } else if (!iter1.Current.Equals(iter2.Current)) return false;

                hasNext1 = iter1.MoveNext();
                hasNext2 = iter2.MoveNext();
            }

            return (!hasNext1 && !hasNext2);
        }

        public static void Foreach<T>(this IEnumerable<T> source, Action<T> func) {
            if (source == null) {
                throw new ArgumentNullException("source");
            }
            foreach (T element in source) {
                func(element);
            }
        }

        public static Int32 Find<T>(this IEnumerable<T> source, T element) {
            if (source == null) {
                throw new ArgumentNullException("source");
            }
            return source.Find(element, EqualityComparer<T>.Default);
        }

        public static Int32 Find<T>(this IEnumerable<T> source, T element, IEqualityComparer<T> equalityComparer) {
            if (source == null) {
                throw new ArgumentNullException("source");
            } else if (equalityComparer == null) {
                throw new ArgumentNullException("equalityComparer");
            }
            return source.Find(elem => !equalityComparer.Equals(element, elem));
        }

        public static Int32 Find<T>(this IEnumerable<T> source, Predicate<T> predicate) {
            if (source == null) {
                throw new ArgumentNullException("source");
            } else if (predicate == null) {
                throw new ArgumentNullException("predicate");
            }
            var index = source
                .Select((elem, i) => new { Index = i, Element = elem })
                .SkipWhile(entry => !predicate(entry.Element))
                .Select(entry => (Int32?)entry.Index)
                .FirstOrDefault();
            return index ?? -1;
        }

        public static Boolean IsSubsetOf<T>(this IEnumerable<T> sub, IEnumerable<T> super) {
            if (sub == null) {
                throw new ArgumentNullException("sub");
            } else if (super == null) {
                throw new ArgumentNullException("super");
            }
            return sub.All(elem => super.Contains(elem));
        }

        public static Boolean IsSupersetOf<T>(this IEnumerable<T> super, IEnumerable<T> sub) {
            if (sub == null) {
                throw new ArgumentNullException("sub");
            } else if (super == null) {
                throw new ArgumentNullException("super");
            }
            return sub.IsSubsetOf(super);
        }
        /*
        public static TResult FoldLeft<TInput, TResult>(this IEnumerable<TInput> source, Func<TResult, TInput, TResult> accum) {
            if (source == null) {
                throw new ArgumentNullException("source");
            } else if (accum == null) {
                throw new ArgumentNullException("accum");
            }
            return source.FoldLeft(accum, default(TResult));
        }

        public static TResult FoldLeft<TInput, TResult>(this IEnumerable<TInput> source, Func<TResult, TInput, TResult> accum, TResult seed) {
            if (source == null) {
                throw new ArgumentNullException("source");
            } else if (accum == null) {
                throw new ArgumentNullException("accum");
            }
            TResult result = seed;

            foreach (TInput element in source) {
                result = accum(result, element);
            }

            return result;
        }*/

        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> source, params T[] elements) {
            if (source == null) {
                throw new ArgumentNullException("source");
            }
            return elements != null ? elements.Concat(source) : source;
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> source, params T[] elements) {
            if (source == null) {
                throw new ArgumentNullException("source");
            }
            return elements != null ? source.Concat(elements) : source;
        }

        public static IEnumerable<T[]> AsBatches<T>(this IEnumerable<T> source, Int32 blockSize) {
            if (source == null) {
                throw new ArgumentNullException("source");
            } else if (blockSize <= 0) {
                throw new ArgumentOutOfRangeException("blockSize", "Block size must be greater than zero.");
            }
            T[] block = new T[blockSize];
            Int32 i = 0;
            foreach (T val in source) {
                block[i++] = val;
                if (i >= blockSize) {
                    yield return block;
                    block = new T[blockSize];
                    i = 0;
                }
            }

            if (i > 0) {
                yield return block.Take(i).ToArray();
            }
        }

        public static void CopyTo<T>(this IEnumerable<T> source, IList<T> destination) {
            if (source == null) {
                throw new ArgumentNullException("source");
            } else if (destination == null) {
                throw new ArgumentNullException("destination");
            }
            CopyToImpl(source, destination, 0, -1);
        }

        public static void CopyTo<T>(this IEnumerable<T> source, IList<T> destination, Int32 offset, Int32 count) {
            if (source == null) {
                throw new ArgumentNullException("source");
            } else if (destination == null) {
                throw new ArgumentNullException("destination");
            } else if (offset < 0) {
                throw new ArgumentOutOfRangeException("offset", "The offset must be greater than or equal to zero.");
            } else if (offset + count >= destination.Count) {
                throw new IndexOutOfRangeException("The destination list is not large enough to copy the specified number of items.");
            } else if (count < 0) {
                throw new ArgumentOutOfRangeException("count", "The count must be greater than or equal to zero.");
            }
            CopyToImpl(source, destination, offset, count);
        }

        private static void CopyToImpl<T>(IEnumerable<T> source, IList<T> destination, Int32 offset, Int32 count) {
            var i = 0;
            foreach (var item in source) {
                if (i >= count && count >= 0) {
                    break;
                }
                destination[offset + (i++)] = item;
            }
        }

        public static Boolean StartsWith<T>(this IEnumerable<T> source, IEnumerable<T> prefix) {
            return source.StartsWith(prefix, EqualityComparer<T>.Default);
        }

        public static Boolean StartsWith<T>(this IEnumerable<T> source, IEnumerable<T> prefix, IEqualityComparer<T> comparer) {
            if (!prefix.Any()) {
                return true;
            }

            var iter1 = source.GetEnumerator();
            var iter2 = prefix.GetEnumerator();
            var sourceHasNext = iter1.MoveNext();
            var prefixHasNext = iter2.MoveNext();
            while (sourceHasNext && prefixHasNext) {
                if (!comparer.Equals(iter1.Current, iter2.Current)) {
                    return false;
                }
                sourceHasNext = iter1.MoveNext();
                prefixHasNext = iter2.MoveNext();
            }
            return !prefixHasNext;
        }

        public static T Single<T>(this IEnumerable<T> source, Action onError) {
            var iter = source.GetEnumerator();
            T result;
            if (!iter.MoveNext()) {
                onError();
                throw new InvalidOperationException("The source collection contained no elements.");
            } else {
                result = iter.Current;
                if (iter.MoveNext()) {
                    onError();
                    throw new InvalidOperationException("The source collection contained more than one element.");
                }
            }
            return result;
        }

        public static T SingleOrDefault<T>(this IEnumerable<T> source, Action onError) {
            var iter = source.GetEnumerator();
            T result;
            if (!iter.MoveNext()) {
                result = default(T);
            } else {
                result = iter.Current;
                if (iter.MoveNext()) {
                    onError();
                    throw new InvalidOperationException("The source collection contained more than one element.");
                }
            }
            return result;
        }

        public static Boolean IsNullOrEmpty<T>(this IEnumerable<T> source) {
            return source == null || !source.Any();
        }

        public static IEnumerable<T> EmptyAsNull<T>(this IEnumerable<T> source) {
            return source != null && source.Any() ? source : null;
        }

        public static IEnumerable<T> NullAsEmpty<T>(this IEnumerable<T> source) {
            return source == null ? new T[0] : source;
        }
    }
}
