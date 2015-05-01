using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Serialization.Xml.Internal {
    public static class HashCode {
        public static Int32 FromList(IEnumerable list) {
            return FromHashCodes(
                list.Cast<Object>().Select(obj => (obj ?? "<<<<null>>>>").GetHashCode())
            );
        }

        public static Int32 FromHashCodes(IEnumerable<Int32> hashCodes) {
            return hashCodes.Aggregate(73, (hash, next) => hash * 37 + next);
        }

        public static Int32 From(params Object[] objects) {
            return FromList(objects);
        }
    }
}
