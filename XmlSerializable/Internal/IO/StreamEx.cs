using System;
using System.IO;
using System.Text;

namespace Serialization.Xml.Internal.IO {
    public static class StreamEx {
        public const Int32 DefaultBufferSize = 1024;

        public static void WriteString(this Stream stream, String str) {
            stream.WriteString(str, Encoding.UTF8);
        }

        public static void WriteString(this Stream stream, String str, Encoding encoding) {
            Int32 strBytesLen = encoding.GetByteCount(str);

            Byte[] buffer = new Byte[sizeof(Int32) + strBytesLen];

            BitConverter.GetBytes(strBytesLen).CopyTo(buffer, 0);
            encoding.GetBytes(str, 0, str.Length, buffer, sizeof(Int32));

            stream.Write(buffer, 0, buffer.Length);
        }

        public static String ReadString(this Stream stream) {
            return stream.ReadString(Encoding.UTF8);
        }

        public static String ReadString(this Stream stream, Encoding encoding) {
            Int32 strBytesLen = BitConverter.ToInt32(stream.ReadExact(sizeof(Int32)), 0);

            return encoding.GetString(stream.ReadExact(strBytesLen));
        }

        public static Byte[] ReadExact(this Stream stream, Int32 byteCount) {
            Byte[] buffer = new Byte[byteCount];

            Int32 read = stream.Read(buffer, 0, byteCount);
            Int32 len = read;
            while (len < byteCount && read > 0) {
                read = stream.Read(buffer, len, byteCount - len);
                len += read;
            }

            if (read == 0) {
                throw new InvalidOperationException("Unexpected End of Stream.");
            }

            return buffer;
        }

        public static Byte[] ReadAllBytes(this Stream stream, Int32 bufferSize = DefaultBufferSize) {
            var result = new ArrayBuilder<Byte>();

            var buffer = new Byte[bufferSize];
            Int32 len;
            while (0 < (len = stream.Read(buffer, 0, bufferSize))) {
                result.Append(buffer, 0, len);
            }

            return result.ToArray();
        }

        public static Int32 BufferedCopy(this Stream src, Stream dst) {
            return src.BufferedCopy(dst, DefaultBufferSize, 0);
        }

        public static Int32 BufferedCopy(this Stream src, Stream dst, Int32 bufferSize) {
            return src.BufferedCopy(dst, bufferSize, 0);
        }

        public static Int32 BufferedCopy(this Stream src, Stream dst, Int32 bufferSize, Int32 maxLen) {
            Byte[] buffer = new Byte[bufferSize];

            Int32 len = src.Read(buffer, 0, bufferSize);
            Int32 total = len;
            while (len > 0) {
                if (maxLen > 0 && total > maxLen) {
                    throw new OverflowException("Maximum Length Exceeded");
                }

                dst.Write(buffer, 0, len);
                len = src.Read(buffer, 0, bufferSize);
                total += len;
            }

            return total;
        }
    }
}
