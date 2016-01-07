// Copyright (c) 2016 Yamool. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

namespace Yamool.Buffers
{
    using System;

    /// <summary>
    /// The abstract class provides a random and sequential accessible sequence of zero or more bytes.
    /// </summary>
    public abstract class ByteBuffer : ReferenceCounted, IEquatable<ByteBuffer>
    {
        /// <summary>        
        /// Gets the underlying buffer of this buffer to exposed access.
        /// </summary>
        /// <remarks>This property can help implement zero-copy.</remarks>
        public abstract byte[] BaseArray
        {
            get;
        }

        /// <summary>
        /// Gets the offset to begin write or read in the underlying buffer of this buffer.
        /// </summary>
        public abstract int BaseOffset
        {
            get;
        }

        /// <summary>
        /// Gets the number of bytes this buffer can contain.
        /// </summary>
        public abstract int Capacity
        {
            get;
        }

        /// <summary>
        /// Gets the writer index of this buffer.
        /// </summary>
        public abstract int WriterIndex
        {
            get;
        }

        /// <summary>
        /// Gets the reader index of this buffer.
        /// </summary>
        public abstract int ReaderIndex
        {
            get;
        }

        /// <summary>
        /// Gets the bool value that indicates this buffer can readable.
        /// </summary>
        public bool IsReadable
        {
            get
            {
                return this.WriterIndex - this.ReaderIndex > 0;
            }
        }

        /// <summary>
        /// Gets the bool value that indicates this buffer can writable.
        /// </summary>
        public bool IsWritable
        {
            get
            {
                return this.Capacity - this.WriterIndex > 0;
            }
        }

        /// <summary>
        /// Gets the number of readable bytes in this buffer.
        /// </summary>
        public int ReadableBytes
        {
            get
            {
                return this.WriterIndex - this.ReaderIndex;
            }
        }

        /// <summary>
        /// Gets the number of writable bytes in this buffer.
        /// </summary>
        public int WritableBytes
        {
            get
            {
                return this.Capacity - this.WriterIndex;
            }
        }

        public virtual int MaxCapacity
        {
            get
            {
                return int.MaxValue;
            }
        }

        /// <summary>
        /// Sets the reader index and writer index of this buffer. 
        /// </summary>
        /// <param name="readerIndex">The reader index to buffer..</param>
        /// <param name="writerIndex">The writer index to buffer.</param>
        public abstract void SetIndex(int readerIndex, int writerIndex);

        /// <summary>
        /// Increase this buffer capacity.
        /// </summary>
        /// <param name="capacity"></param>
        public abstract void SetCapacity(int capacity);

        /// <summary>
        /// Transfers this buffer's data to the specified destination starting at the specified absolute index. 
        /// </summary>
        /// <param name="index">The first index of this buffer which to begin transfer.</param>
        /// <param name="dst">The destination buffer.</param>
        /// <param name="dstIndex">The first index of the destination.</param>
        /// <param name="length">The number of bytes to transfer.</param>
        /// <remarks>This method does not modify readerIndex or writerIndex of this buffer.</remarks>
        public abstract void GetBytes(int index, byte[] dst, int dstIndex, int length);

        internal void InternalSetBytes(int index, byte[] src)
        {
            var length = src.Length;
            this.CheckIndex(index, length);
            this.SetBytes(index, src, 0, length);
        }

        internal byte[] InternalGetBytes(int index, int length)
        {
            this.CheckIndex(index, length);
            var bytes = new byte[length];
            this.GetBytes(index, bytes, 0, length);
            return bytes;
        }

        /// <summary>
        /// Transfers the specified source buffer's data to this buffer starting at the specified absolute index. 
        /// </summary>
        /// <param name="index">The first index of this buffer which to begin transfer.</param>
        /// <param name="src">The source of transfer.</param>
        /// <param name="srcIndex">The first index of the source which to begin transfer.</param>
        /// <param name="length">The number of bytes to transfer.</param>
        /// <remarks>This method does not modify readerIndex or writerIndex of both the source (i.e. this) and the destination.</remarks>
        public abstract void SetBytes(int index, byte[] src, int srcIndex, int length);

        /// <summary>
        /// Transfers this buffer's data to the specified destination starting at the current reader index 
        /// and increases the reader index by the number of the transferred bytes.
        /// </summary>
        /// <param name="dst">The destination buffer.</param>
        /// <param name="dstIndex">The first index of the destination which to begin transfer.</param>
        /// <param name="length">The number of bytes to transfer.</param>
        public virtual void ReadBytes(byte[] dst, int dstIndex, int length)
        {
            if (dst == null)
            {
                throw new ArgumentNullException("src");
            }
            if (dstIndex < 0 || length < 0)
            {
                throw new ArgumentException("srcIndex or length less than 0.");
            }
            this.CheckReadableBytes(length);
            this.GetBytes(this.ReaderIndex, dst, dstIndex, length);
            this.SetReaderIndex(this.ReaderIndex + length);
        }

        /// <summary>
        /// Transfers the specified source array's data to this buffer starting at the current writer index 
        /// and increases the writer index by the number of the transferred bytes.
        /// </summary>
        /// <param name="src">The source of transfer.</param>
        /// <param name="srcIndex">The first index of the source which to begin transfer.</param>
        /// <param name="length">The number of bytes to transfer.</param>
        public virtual void WriteBytes(byte[] src, int srcIndex, int length)
        {
            if (src == null)
            {
                throw new ArgumentNullException("src");
            }
            if (srcIndex < 0 || length < 0)
            {
                throw new ArgumentException("srcIndex or length less than 0.");
            }
            this.EnsureWritable(length);            
            this.SetBytes(this.WriterIndex, src, srcIndex, length);
            this.SetWriterIndex(this.WriterIndex + length);
        }

        /// <summary>
        /// Skip a specified length in this buffer and increases the readerIndex.
        /// </summary>
        /// <param name="length">The length to skip.</param>
        public void Skip(int length)
        {
            if (length > this.ReadableBytes)
            {
                throw new ArgumentException("The length of skip exceeds the readable bytes of this buffer.");
            }
            this.SetReaderIndex(this.ReaderIndex + length);
        }

        public bool Equals(ByteBuffer other)
        {
            var length = this.ReadableBytes;
            if (length != other.ReadableBytes)
            {
                return false;
            }
            var longCount = length >> 3;
            var byteCount = length & 7;
            var aIndex = this.ReaderIndex;
            var bIndex = other.ReaderIndex;
            for (var i = longCount; i > 0; i--)
            {
                if (this.GetLong(aIndex) != other.GetLong(bIndex))
                {
                    return false;
                }
                aIndex += 8;
                bIndex += 8;
            }
            for (var i = byteCount; i > 0; i--)
            {
                if (this.GetByte(aIndex) != other.GetByte(bIndex))
                {
                    return false;
                }
                aIndex++;
                bIndex++;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ByteBuffer))
            {
                return false;
            }
            return this.Equals((ByteBuffer)obj);
        }

        public override int GetHashCode()
        {
            var length = this.ReadableBytes;
            var intCount = length >> 2;
            var byteCount = length & 3;
            var hashCode = 1;
            var arrayIndex = this.ReaderIndex;
            for (var i = intCount; i > 0; i--)
            {
                hashCode = 31 * hashCode + this.GetInt(arrayIndex);
                arrayIndex += 4;
            }
            for (var i = byteCount; i > 0; i--)
            {
                hashCode = 31 * hashCode + this.GetByte(arrayIndex++);
            }
            if (hashCode == 0)
            {
                hashCode = 1;
            }
            return hashCode;
        }

        public static bool operator ==(ByteBuffer a, ByteBuffer b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ByteBuffer a, ByteBuffer b)
        {
            return !a.Equals(b);
        }

        internal protected void SetWriterIndex(int newIndex)
        {
            this.SetIndex(this.ReaderIndex, newIndex);
        }

        internal protected void SetReaderIndex(int newIndex)
        {
            this.SetIndex(newIndex, this.WriterIndex);           
        }

        protected void EnsureAccessible()
        {
            if (this.RefCount == 0)
            {
                throw new BufferAccessException();
            }
        }

        private void CheckReadableBytes(int minimumReadableBytes)
        {
            this.EnsureAccessible();
            if (this.ReaderIndex > this.WriterIndex - minimumReadableBytes)
            {
                throw new ArgumentException("The number of bytes at the readerIndex to read exceeds writeIndex of this buffer.");
            }
        }

        internal void EnsureWritable(int minWritableBytes)
        {
            this.EnsureAccessible();
            if (minWritableBytes < 0)
            {
                throw new ArgumentException("minWritableBytes less than zero.");
            }
            if (minWritableBytes <= this.WritableBytes)
            {
                return;
            }
            if (minWritableBytes > this.MaxCapacity - this.WriterIndex)
            {
                throw new ArgumentException("The number of bytes to write at the writerIndex exceeds max capacity of this buffer.");
            }
            this.SetCapacity(this.WriterIndex + minWritableBytes);
        }

        private void CheckIndex(int index, int fieldLength)
        {
            this.EnsureAccessible();
            if (fieldLength < 0)
            {
                throw new ArgumentException("fieldLength less than zero(0).");
            }
            if (index < 0 || index > this.Capacity - fieldLength)
            {
                throw new ArgumentException("index less than zero or greater than length of this buffer.");
            }
        }
    }

    /// <summary>
    /// The exception that is thrown when an attempts to access a reference count is ZERO of the ByteBuffer object.
    /// </summary>
    public sealed class BufferAccessException : SystemException
    {       
    }
}
