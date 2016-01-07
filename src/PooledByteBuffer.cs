// Copyright (c) 2016 Yamool. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

namespace Yamool.Buffers
{
    using System;

    internal class PooledByteBuffer : ByteBuffer
    {
        private readonly int _maxCapacity;
        private byte[] _buffer;
        private int _offset;
        private int _capacity;
        private int _maxLength;

        private int _writerIndex;
        private int _readerIndex;

        internal long _handle;
        internal PoolChunk _chunk;


        public PooledByteBuffer(int maxCapacity)
        {
            _maxCapacity = maxCapacity;
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
                return _offset;
            }
        }

        public override int Capacity
        {
            get
            {
                return _capacity;
            }
        }

        internal int MaxLength
        {
            get
            {
                return _maxLength;
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

        public override int MaxCapacity
        {
            get
            {
                return _maxCapacity;
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
            if (capacity > _capacity)
            {
                if (capacity <= _maxLength)
                {
                    _capacity = capacity;
                    return;
                }
            }
            else if (capacity < _capacity)
            {
                if (capacity > _maxLength >> 1)
                {
                    if (_maxLength <= 512)
                    {
                        if (capacity > _maxLength - 16)
                        {
                            _capacity = capacity;
                            this.SetIndex(Math.Min(_readerIndex, capacity), Math.Min(_writerIndex, capacity));
                            return;
                        }
                    }
                    else
                    {
                        _capacity = capacity;
                        this.SetIndex(Math.Min(_readerIndex, capacity), Math.Min(_writerIndex, capacity));
                        return;
                    }
                }
            }
            _chunk.Arena.Reallocate(this, capacity, true);
        }

        public override void GetBytes(int index, byte[] dst, int dstIndex, int length)
        {
            this.EnsureAccessible();
            if (dst == null)
            {
                throw new ArgumentNullException("dst");
            }
            if (index < 0 || dstIndex < 0 || length < 0)
            {
                throw new ArgumentException("index or dstIndex or length less than 0.");
            }
            if (dstIndex + length > dst.Length || _capacity - index < length - dstIndex)
            {
                throw new ArgumentException("dstIndex and length were out of bounds of dst or the length is greater than this buffer readable length.");
            }
            Buffer.BlockCopy(_buffer, this.Idx(index), dst, dstIndex, length);
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
            Buffer.BlockCopy(src, srcIndex, _buffer, this.Idx(index), length);
        }

        internal void Init(PoolChunk chunk, long handle, int offset, int length, int maxLength)
        {           
            _chunk = chunk;
            _buffer = chunk.Buffer;
            _handle = handle;
            _offset = offset;
            _capacity = length;
            _maxLength = maxLength;
            this.SetIndex(0, 0);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _chunk.Arena.Free(_chunk, _handle, _maxLength);            
            }
        }

        private int Idx(int index)
        {
            return _offset + index;
        }
    }
}
