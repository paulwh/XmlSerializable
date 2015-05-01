using System;
using System.IO;
using System.Xml;

namespace Serialization.Xml {
    public class XmlBinaryStream : Stream {
        public override bool CanRead {
            get { return this.reader != null; }
        }

        public override bool CanSeek {
            get { return this.writer != null; }
        }

        public override bool CanWrite {
            get { return false; }
        }

        public override long Length {
            get { throw new NotSupportedException(); }
        }

        private long position;
        public override long Position {
            get {
                return this.position;
            }
            set {
                throw new NotSupportedException();
            }
        }

        private XmlReader reader;
        private XmlWriter writer;
        private Func<byte[], int, int, int> readOperation;
        private Action<byte[], int, int> writeOperation;
        private bool done;

        public XmlBinaryStream(XmlReader reader, XmlBinaryFormat format = XmlBinaryFormat.Base64) {
            this.reader = reader;
            switch (reader.NodeType) {
                case XmlNodeType.Element:
                    switch (format) {
                        case XmlBinaryFormat.Base64:
                            this.readOperation = reader.ReadElementContentAsBase64;
                            break;
                        case XmlBinaryFormat.BinHex:
                            this.readOperation = reader.ReadElementContentAsBinHex;
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                    break;
                case XmlNodeType.Attribute:
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                    switch (format) {
                        case XmlBinaryFormat.Base64:
                            this.readOperation = reader.ReadContentAsBase64;
                            break;
                        case XmlBinaryFormat.BinHex:
                            this.readOperation = reader.ReadContentAsBinHex;
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                    break;
                default:
                    throw new ArgumentException("The reader is not in a state where binary data can be read.", "reader");
            }
        }

        public XmlBinaryStream(XmlWriter writer, XmlBinaryFormat format = XmlBinaryFormat.Base64) {
            this.writer = writer;
            switch (format) {
                case XmlBinaryFormat.Base64:
                    this.writeOperation = writer.WriteBase64;
                    break;
                case XmlBinaryFormat.BinHex:
                    this.writeOperation = writer.WriteBinHex;
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count) {
            if (this.readOperation == null) {
                throw new InvalidOperationException("This stream cannot be used for reading.");
            }
            if (!this.done) {
                var len = this.readOperation(buffer, offset, count);
                if (len == 0) {
                    this.position += len;
                    this.done = true;
                }
                return len;
            } else {
                return 0;
            }
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotSupportedException();
        }

        public override void SetLength(long value) {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            if (this.writeOperation == null) {
                throw new InvalidOperationException("This stream cannot be used for writing.");
            }
            this.writeOperation(buffer, offset, count);
        }

        public override void Flush() {
            if (this.writer == null) {
                throw new InvalidOperationException("This stream cannot be used for writing.");
            }
            this.writer.Flush();
        }

        public override void Close() {
            base.Close();
        }
    }

    public enum XmlBinaryFormat {
        Base64 = 0,
        BinHex
    }
}
