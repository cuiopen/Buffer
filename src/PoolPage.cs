// Copyright (c) 2016 Yamool. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

namespace Yamool.Buffers
{
    using System;

    internal class PoolPage
    {
        private int _memoryMapIdx;
        private int _runOffset;
        private int _pageSize;
        private long[] _bitmap;
        private int _maxNumElems;
        private int _bitmapLength;
        private int _nextAvail;
        private int _numAvail;
        private int _elemSize;
        private PoolChunk _chunk;
        private bool _doNotDestroy;

        public PoolPage(int pageSize)
        {
            _pageSize = pageSize;
        }

        public PoolPage(PoolChunk chunk, int memoryMapIdx, int runOffset, int pageSize, int elemSize)
        {
            _chunk = chunk;
            _memoryMapIdx = memoryMapIdx;
            _runOffset = runOffset;
            _pageSize = pageSize;
            _bitmap = new long[pageSize >> 10];
            this.Init(elemSize);
        }

        public PoolPage Prev
        {
            get;
            set;
        }

        public PoolPage Next
        {
            get;
            set;
        }

        public PoolChunk Chunk
        {
            get
            {
                return _chunk;
            }
        }

        public int ElemSize
        {
            get
            {
                return _elemSize;
            }
        }

        public long Allocate()
        {
            if (_elemSize == 0)
            {
                return this.ToHandle(0);
            }
            if (_numAvail == 0 || !_doNotDestroy)
            {
                return -1;
            }
            var bitmapIdx = this.GetNextAvail();
            var q = bitmapIdx >> 6;          
            var r = bitmapIdx & 63;
            _bitmap[q] |= 1L << r;

            if (--_numAvail == 0)
            {
                this.RemoveFromPool();
            }

            return this.ToHandle(bitmapIdx);
        }

        public bool Free(int bitmapIdx)
        {
            if (_elemSize == 0)
            {
                return true;
            }
            var q = bitmapIdx >> 6;
            var r = bitmapIdx & 63;
            _bitmap[q] ^= 1L << r;
            if (_numAvail++ == 0)
            {
                _nextAvail = bitmapIdx;
                this.AddToPool();
                return true;
            }
            if (_numAvail < _maxNumElems)
            {
                return true;
            }
            else
            { 
                if (Prev == Next)
                {
                    // Do not remove if this subpage is the only one left in the pool.
                    return true;
                }
                // Remove this subpage from the pool if there are other subpages left in the pool.
                _doNotDestroy = false;
                this.RemoveFromPool();
                return false;
            }
        }

        private void Init(int elemSize)
        {
            _doNotDestroy = true;
            _elemSize = elemSize;
            if (elemSize != 0)
            {
                _maxNumElems = _numAvail = _pageSize / elemSize;
                _nextAvail = 0;               
                _bitmapLength = _maxNumElems >> 6;
                if ((_maxNumElems & 63) != 0)
                {
                    _bitmapLength++;
                }
                for (var i = 0; i < _bitmapLength; i++)
                {
                    _bitmap[i] = 0;
                }
            }
            this.AddToPool();
        }

        private int GetNextAvail()
        {           
            if (_nextAvail >= 0)
            {                
                return _nextAvail--;
            }
            return this.FindNextAvail();
        }

        private int FindNextAvail()
        {          
            for (var i = 0; i < _bitmapLength; i++)
            {
                var bits = _bitmap[i];
                if (~bits != 0)
                {
                    return this.findNextAvail0(i, bits);
                }
            }
            return -1;
        }

        private int findNextAvail0(int i, long bits)
        {
            var baseVal = i << 6;

            for (var j = 0; j < 64; j++)
            {
                if ((bits & 1) == 0)
                {
                    var val = baseVal | j;
                    if (val < _maxNumElems)
                    {
                        return val;
                    }
                    else
                    {
                        break;
                    }
                }
                bits >>= 1;
            }
            return -1;
        }

        private void AddToPool()
        {
            var head = _chunk.Arena.FindSubpagePoolHead(_elemSize);
            this.Prev = head;
            Next = head.Next;
            Next.Prev = this;
            head.Next = this;
        }

        private void RemoveFromPool()
        {
            Prev.Next = this.Next;
            Next.Prev = this.Prev;
            this.Next = null;
            this.Prev = null;
        }

        private long ToHandle(int bitmapIdx)
        {
            // 0x4000000000000000L=1<<62
            return 0x4000000000000000L | (long)bitmapIdx << 32 | (uint)_memoryMapIdx;
        }
    }
}
