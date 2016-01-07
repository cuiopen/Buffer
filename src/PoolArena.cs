// Copyright (c) 2016 Yamool. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

namespace Yamool.Buffers
{
    using System;

    internal class PoolArena
    {
        private readonly object _sync = new object();
        private readonly string _name;
        private readonly int _pageSize;
        private readonly int _maxOrder;
        private readonly int _chunkSize;
        private readonly int _pageShifts;
        private readonly int _subpageOverflowMask;
        private readonly PoolPage[] _tinySubpagePools;
        private readonly PoolPage[] _smallSubpagePools;
        private ChunkPool _chunkPools;

        public PoolArena(string name, int pageSize, int maxOrder, int chunkSize)
        {
            _name = name;
            _pageSize = pageSize;
            _maxOrder = maxOrder;
            _chunkSize = chunkSize;
            _pageShifts = Utils.Log2(pageSize);
            _subpageOverflowMask = ~(pageSize - 1);
            _tinySubpagePools = new PoolPage[32];
            for (var i = 0; i < _tinySubpagePools.Length; i++)
            {
                _tinySubpagePools[i] = this.CreateSubpagePoolHead();
            }
            _smallSubpagePools = new PoolPage[_pageShifts - 9];
            for (var i = 0; i < _smallSubpagePools.Length; i++)
            {
                _smallSubpagePools[i] = this.CreateSubpagePoolHead();
            }
            _chunkPools = new ChunkPool();
        }

        public void Allocate(PooledByteBuffer buffer, int reqCapacity)
        {
            var normCapacity = this.NormalizeCapacity(reqCapacity);
            if (this.IsTinyOrSmall(normCapacity))
            {
                var tableIdx = 0;
                PoolPage[] table;
                if ((normCapacity & 0xFFFFFE00) == 0)
                {
                    tableIdx = normCapacity >> 4;
                    table = _tinySubpagePools;
                }
                else
                {
                    tableIdx = 0;
                    var i = normCapacity >> 10;//1024
                    while (i != 0)
                    {
                        i >>= 1;
                        tableIdx++;
                    }
                    table = _smallSubpagePools;
                }
                lock (_sync)
                {
                    var head = table[tableIdx];
                    var s = head.Next;
                    if (s != head)
                    {
                        long handle = s.Allocate();
                        s.Chunk.InitBufferWithSubpage(buffer, handle, reqCapacity);
                        return;
                    }
                }
            }
            else if (normCapacity > _chunkSize)
            {
                var unpooledChunk = new PoolChunk(this, normCapacity);
                buffer.Init(unpooledChunk, 0, 0, reqCapacity, normCapacity);
                return;
            }
            this.Allocate(buffer, reqCapacity, normCapacity);
        }

        public void Reallocate(PooledByteBuffer buffer, int newCapacity, bool freeOldMemory)
        {
            if (newCapacity < 0 || newCapacity > buffer.MaxCapacity)
            {
                throw new ArgumentException("newCapacity less than 0 or greater than the max capacity.");
            }
            var oldCapacity = buffer.Capacity;
            if (oldCapacity == newCapacity)
            {
                return;
            }
            var oldBuffer = buffer.BaseArray;
            var oldChunk = buffer._chunk;
            var oldHandle = buffer._handle;
            int oldOffset = buffer.BaseOffset;
            int oldMaxLength = buffer.MaxLength;
            int readerIndex = buffer.ReaderIndex;
            int writerIndex = buffer.WriterIndex;
            this.Allocate(buffer, newCapacity);
            Buffer.BlockCopy(oldBuffer, oldOffset, buffer.BaseArray, buffer.BaseOffset, oldCapacity);
            buffer.SetIndex(readerIndex, writerIndex);
            if (freeOldMemory)
            {
                this.Free(oldChunk, oldHandle, oldMaxLength);
            }
        }

        public void Free(PoolChunk chunk, long handle, int maxLength)
        {
            lock (_sync)
            {
                if (!chunk.Unpooled)
                {
                    chunk.Free(handle);
                }
            }
        }

        internal PoolPage FindSubpagePoolHead(int elemSize)
        {
            var tableIdx = 0;
            PoolPage[] table;
            if (IsTiny(elemSize))
            { // < 512
                tableIdx = elemSize >> 4;
                table = _tinySubpagePools;
            }
            else
            {
                tableIdx = 0;
                elemSize >>= 10;
                while (elemSize != 0)
                {
                    elemSize >>= 1;
                    tableIdx++;
                }
                table = _smallSubpagePools;
            }

            return table[tableIdx];
        }

        private void Allocate(PooledByteBuffer buffer, int reqCapacity, int normCapacity)
        {
            lock (_sync)
            {
                if (_chunkPools.Allocate(buffer, reqCapacity, normCapacity))
                {
                    return;
                }
                var chunk = new PoolChunk(this, _pageSize, _maxOrder, _pageShifts, _chunkSize);
                var handle = chunk.Allocate(normCapacity);
                chunk.InitBuffer(buffer, handle, reqCapacity);
                _chunkPools.Add(chunk);
            }
        }

        private int NormalizeCapacity(int reqCapacity)
        {
            if (reqCapacity < 0)
            {
                throw new ArgumentException("reqCapacity less than 0.");
            }
            if (reqCapacity >= _chunkSize)
            {
                return reqCapacity;
            }

            if (!this.IsTiny(reqCapacity))
            {
                //make sure the new capacity is double capacity of request.
                var normalizedCapacity = reqCapacity;
                normalizedCapacity--;
                normalizedCapacity |= normalizedCapacity >> 1;
                normalizedCapacity |= normalizedCapacity >> 2;
                normalizedCapacity |= normalizedCapacity >> 4;
                normalizedCapacity |= normalizedCapacity >> 8;
                normalizedCapacity |= normalizedCapacity >> 16;
                normalizedCapacity++;
                if (normalizedCapacity < 0)
                {
                    normalizedCapacity >>= 1;
                }
                return normalizedCapacity;
            }
            // Quantum-spaced
            if ((reqCapacity & 15) == 0)
            {
                return reqCapacity;
            }
            return (reqCapacity & ~15) + 16;
        }

        public bool IsTiny(int normCapacity)
        {
            return (normCapacity & 0xFFFFFE00) == 0;
        }

        public bool IsTinyOrSmall(int normCapacity)
        {
            return (normCapacity & _subpageOverflowMask) == 0;
        }

        private PoolPage CreateSubpagePoolHead()
        {
            var head = new PoolPage(_pageSize);
            head.Prev = head;
            head.Next = head;
            return head;
        }

        private class ChunkPool
        {
            private int _totalChunks;
            private PoolChunk _head;

            public int TotalChunks
            {
                get
                {
                    return _totalChunks;
                }
            }

            public bool Allocate(PooledByteBuffer buffer, int reqCapacity, int normCapacity)
            {
                if (_head == null)
                {
                    return false;
                }
                var cur = _head;
                do
                {
                    var handle = cur.Allocate(normCapacity);
                    if (handle >= 0)
                    {
                        cur.InitBuffer(buffer, handle, reqCapacity);
                        return true;
                    }
                } while ((cur = cur.Next) != null);
                return false;
            }

            public void Add(PoolChunk chunk)
            {
                if (_head == null)
                {
                    _head = chunk;
                    chunk.Prev = chunk.Next = null;
                }
                else
                {
                    chunk.Prev = null;
                    chunk.Next = _head;
                    _head.Prev = chunk;
                    _head = chunk;
                }
                _totalChunks++;
            }

            private void Remove(PoolChunk cur)
            {
                if (cur == _head)
                {
                    _head = cur.Next;
                    if (_head != null)
                    {
                        _head.Prev = null;
                    }
                }
                else
                {
                    var next = cur.Next;
                    cur.Prev.Next = next;
                    if (next != null)
                    {
                        next.Prev = cur.Prev;
                    }
                }
                _totalChunks--;
            }
        }
    }
}
