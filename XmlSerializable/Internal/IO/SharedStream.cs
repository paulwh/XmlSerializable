using System.IO;

namespace Serialization.Xml.Internal.IO {
    public class SharedStream : StreamWrapper {
        public SharedStream(Stream inner) : base(inner) {
        }

        public override void Close() {
            // leave the inner stream open.
        }

        protected override void Dispose(bool disposing) {
            // leave the inner stream undisposed. This stream has no resources to free.
        }
    }
}
