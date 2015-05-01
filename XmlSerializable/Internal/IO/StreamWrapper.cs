using System;
using System.IO;

namespace Serialization.Xml.Internal.IO {
    public class StreamWrapper : Stream {
        protected Stream InnerStream { get; private set; }

        protected StreamWrapper(Stream inner) {
            this.InnerStream = inner;
        }

        public override bool CanRead {
            get { return this.InnerStream.CanRead; }
        }

        public override bool CanSeek {
            get { return this.InnerStream.CanSeek; }
        }

        public override bool CanWrite {
            get { return this.InnerStream.CanWrite; }
        }

        public override bool CanTimeout {
            get { return this.InnerStream.CanTimeout; }
        }

        public override int ReadTimeout {
            get {
                return this.InnerStream.ReadTimeout;
            }
            set {
                this.InnerStream.ReadTimeout = value;
            }
        }

        public override int WriteTimeout {
            get {
                return this.InnerStream.WriteTimeout;
            }
            set {
                this.InnerStream.WriteTimeout = value;
            }
        }

        public override void Flush() {
            this.InnerStream.Flush();
        }

        public override long Length {
            get { return this.InnerStream.Length; }
        }

        public override long Position {
            get {
                return this.InnerStream.Position;
            }
            set {
                this.InnerStream.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count) {
            return this.InnerStream.Read(buffer, offset, count);
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) {
            return this.InnerStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult) {
            return this.InnerStream.EndRead(asyncResult);
        }

        public override int ReadByte() {
            return this.InnerStream.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin) {
            return this.InnerStream.Seek(offset, origin);
        }

        public override void SetLength(long value) {
            this.InnerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count) {
            this.InnerStream.Write(buffer, offset, count);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) {
            return this.InnerStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult) {
            this.InnerStream.EndWrite(asyncResult);
        }

        public override void WriteByte(byte value) {
            this.InnerStream.WriteByte(value);
        }

        public override System.Runtime.Remoting.ObjRef CreateObjRef(Type requestedType) {
            return this.InnerStream.CreateObjRef(requestedType);
        }

        public override object InitializeLifetimeService() {
            return this.InnerStream.InitializeLifetimeService();
        }

        public override void Close() {
            this.InnerStream.Close();
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                this.InnerStream.Dispose();
            }
        }
    }
}
