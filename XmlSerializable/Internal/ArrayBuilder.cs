using System;
using System.Collections.Generic;
using Serialization.Xml.Internal.Collections;

namespace Serialization.Xml.Internal {
    /// <summary>
    /// Efficiently builds an array from chunks with copying, minimizing new allocations be creating fixed sized chunks.
    /// </summary>
    public class ArrayBuilder<T> : IEnumerable<T> {
        private const Int32 ChunkSize = 1024;
        private LinkedList<T[]> chunks;
        private T[] currentChunk;
        private Int32 chunkPosition = 0;

        public Int32 Length { get; private set; }

        public ArrayBuilder() {
            this.chunks = new LinkedList<T[]>();
            this.chunks.AddLast(this.currentChunk = new T[ChunkSize]);
        }

        /// <summary>
        /// Appends an entire array.
        /// </summary>
        /// <remarks>The source array is copied.</remarks>
        public void Append(T[] array) {
            this.AppendImpl(array, 0, array.Length);
        }

        /// <summary>
        /// Appends a part of an array.
        /// </summary>
        /// <remarks>The specified section of the source array is copied.</remarks>
        public void Append(T[] array, Int32 offset, Int32 count) {
            if (array == null) throw new ArgumentNullException("array");
            else if (offset < 0) throw new ArgumentOutOfRangeException("offset", "The offset must be greater than or equal to zero.");
            else if (offset + count > array.Length) throw new ArgumentOutOfRangeException("count", "The source array is not long enough to copy the specified count");
            this.AppendImpl(array, offset, count);
        }

        public void Append(IList<T> list) {
            this.AppendImpl(list, 0, list.Count);
        }

        private void AppendImpl(IList<T> list, Int32 offset, Int32 count) {
            var copied = Math.Min(count, ChunkSize - this.chunkPosition);
            List.Copy(list, offset, this.currentChunk, this.chunkPosition, copied);
            this.chunkPosition += copied;
            if (this.chunkPosition == ChunkSize) {
                var remainingCount = count - copied;
                // We've filled the chunk
                if (remainingCount > ChunkSize) {
                    // rather than breaking the remaining items into chunks, make a single extra large chunk
                    var bigChunk = new T[remainingCount];
                    List.Copy(list, offset + copied, bigChunk, 0, remainingCount);
                    this.chunks.AddLast(bigChunk);
                    this.chunks.AddLast(this.currentChunk = new T[ChunkSize]);
                    this.chunkPosition = 0;
                } else {
                    // Make a new standard chunk and copy what's left
                    this.chunks.AddLast(this.currentChunk = new T[ChunkSize]);
                    List.Copy(list, offset + copied, this.currentChunk, 0, remainingCount);
                    this.chunkPosition = remainingCount;
                }
            }

            this.Length += count;
        }

        /// <summary>
        /// Appends an IEnumerable.
        /// </summary>
        public void Append(IEnumerable<T> collection) {
            var enumerator = collection.GetEnumerator();
            bool hasNext = true;
            while (hasNext) {
                while ((hasNext = enumerator.MoveNext()) && this.chunkPosition < ChunkSize) {
                    this.currentChunk[this.chunkPosition++] = enumerator.Current;
                }
                if (this.chunkPosition == ChunkSize) {
                    this.chunks.AddLast(this.currentChunk = new T[ChunkSize]);
                    this.chunkPosition = 0;

                    if (hasNext) {
                        this.currentChunk[this.chunkPosition++] = enumerator.Current;
                    }
                }
            }
        }

        /// <summary>
        /// Appends an individual item.
        /// </summary>
        public void Append(T item) {
            this.currentChunk[this.chunkPosition++] = item;
            if (this.chunkPosition == ChunkSize) {
                this.chunks.AddLast(this.currentChunk = new T[ChunkSize]);
                this.chunkPosition = 0;
            }
        }

        public T[] ToArray() {
            var buffer = new T[this.Length];
            var pos = 0;
            foreach (var chunk in this.chunks) {
                var copyLen = Math.Min(this.Length - pos, chunk.Length);
                Array.Copy(chunk, 0, buffer, pos, copyLen);
                pos += copyLen;
            }
            return buffer;
        }

        public IEnumerator<T> GetEnumerator() {
            var pos = 0;
            foreach (var chunk in this.chunks) {
                foreach (var item in chunk) {
                    if (pos++ < this.Length) {
                        yield return item;
                    } else {
                        break;
                    }
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return (System.Collections.IEnumerator)this.GetEnumerator();
        }
    }
}
