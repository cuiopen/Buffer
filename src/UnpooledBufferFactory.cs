// Copyright (c) 2016 Yamool. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

namespace Yamool.Buffers
{
    using System;
    
    /// <summary>
    /// This buffer factory create a <see cref="ByteBuffer"/> that recycle based on the .NET GC.
    /// </summary>
    public sealed class UnpooledBufferFactory : IBufferFactory
    {
        public ByteBuffer NewBuffer(int length, int maxCapacity = int.MaxValue)
        {
            if (length < 0)
            {
                throw new ArgumentException("length less than 0.");
            }
            return new UnpooledByteBuffer(length, maxCapacity);
        }

        private class UnpooledByteBuffer : ByteBuffer
        {
            private int _capacity;
            private int _maxCapacity;
            private byte[] _buffer;
            private int _writerIndex;
            private int _readerIndex;

            public UnpooledByteBuffer(int length, int maxCapacity)
            {
                _capacity = length;
                _maxCapacity = maxCapacity;
                _buffer = new byte[length];
            }

            public override byte[] BaseArray
            {
                get
                {
                    return _buffer;
                }
            }

            public override int BaseOffset
            {
                get
                {
                    return 0;
                }
            }

            public override int Capacity
            {
                get
                {
                    return _capacity;
                }
            }

            public override int MaxCapacity
            {
                get
                {
                    return _maxCapacity;
                }
            }

            public override int WriterIndex
            {
                get
                {
                    return _writerIndex;
                }
            }

            public override int ReaderIndex
            {
                get
                {
                    return _readerIndex;
                }
            }

            public override void SetIndex(int readerIndex, int writerIndex)
            {
                this.EnsureAccessible();
                if (readerIndex < 0 || writerIndex < 0)
                {
                    throw new ArgumentException("readerIndex or writerIndex less than 0.");
                }
                if (writerIndex < readerIndex)
                {
                    throw new ArgumentException("writerIndex less than readerIndex.");
                }
                if (writerIndex > _capacity)
                {
                    throw new ArgumentException("writerIndex greater than length.");
                }                
                _readerIndex = readerIndex;
                _writerIndex = writerIndex;
            }

            public override void SetCapacity(int capacity)
            {
                this.EnsureAccessible();
                if (capacity < 0)
                {
                    throw new ArgumentException("capacity less than 0.");
                }
                if (capacity <= _capacity)
                {                    
                    return;
                }
                var newCapacity = capacity << 1;
                var newBytes = new byte[newCapacity];
                Buffer.BlockCopy(_buffer, 0, newBytes, 0, _capacity);
                _buffer = newBytes;
                _capacity = newCapacity;
            }

            public override void GetBytes(int index, byte[] dst, int dstIndex, int length)
            {
                this.EnsureAccessible();
                if (dst == null)
                {
                    throw new ArgumentNullException("dst");
                }
                if (index < 0 || dstIndex<0 || length<0)
                {
                    throw new ArgumentException("index or dstIndex or length less than 0.");
                }
                if (dstIndex + length > dst.Length || _capacity - index < length - dstIndex)
                {
                    throw new ArgumentException("dstIndex and length were out of bounds of dst or the length is greater than this buffer readable length.");
                }
                Buffer.BlockCopy(_buffer, index, dst, dstIndex, length);
            }

            public override void SetBytes(int index, byte[] src, int srcIndex, int length)
            {
                this.EnsureAccessible();
                if (src == null)
                {
                    throw new ArgumentNullException("src");
                }
                if (index < 0 || srcIndex < 0 || length < 0)
                {
                    throw new ArgumentException("index or srcIndex or length less than 0.");
                }
                if (srcIndex + length > src.Length || _capacity - index < length - srcIndex)
                {
                    throw new ArgumentException("srcIndex and length were out of bounds of src or the length is greater than this buffer writeable length.");
                }
                Buffer.BlockCopy(src, srcIndex, _buffer, index, length);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    //nothing to do.
                }
            }
        }
    }
}
