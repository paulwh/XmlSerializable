using System;
using System.Collections.Generic;

namespace Serialization.Xml.Internal.Collections {
    public static class List {
        public static void Copy<T>(IList<T> source, Int32 sourceIndex, IList<T> destination, Int32 destinationIndex, Int32 count) {
            for (var i = 0; i < count; i++) {
                destination[destinationIndex + i] = source[sourceIndex + i];
            }
        }
    }
}
