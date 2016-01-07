// Copyright (c) 2016 Yamool. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

namespace Yamool.Buffers
{
    using System;

    internal class PoolChunk
    {
        private byte[] _memoryMap;
        private byte[] _depthMap;
        private byte[] _buffer;
        private PoolPage[] _subpages;
        private bool _unpooled;
        private int _pageSize;
        private int _pageShifts;
        private int _chunkSize;
        private int _maxSubpageAllocs;
        private int _freeBytes;
        private int _log2ChunkSize;
        private Byte _unusable;
        private int _maxOrder;
        private readonly PoolArena _arena;

        public PoolChunk(PoolArena arena, int pageSize, int maxOrder, int pageShifts, int chunkSize)
        {
            _arena = arena;
            _buffer = new byte[chunkSize];         
            _maxOrder = maxOrder;                     
            _pageSize = pageSize;
            _pageShifts = pageShifts;
            _freeBytes = _chunkSize = chunkSize;                         
            _unusable = (byte)(maxOrder + 1); 
            _maxSubpageAllocs = 1 << maxOrder;
            _memoryMap = new byte[_maxSubpageAllocs << 1];
            _depthMap = new byte[_memoryMap.Length];
            var memoryMapIndex = 1;
            for (var d = 0; d <= maxOrder; d++)
            { 
                var depth = 1 << d;
                for (int p = 0; p < depth; p++)
                {
                    _memoryMap[memoryMapIndex] = (byte)d;
                    _depthMap[memoryMapIndex] = (byte)d;
                    memoryMapIndex++;
                }
            }
            _subpages = new PoolPage[_maxSubpageAllocs];
            _log2ChunkSize = Utils.Log2(chunkSize);
            _unpooled = false;
        }

        public PoolChunk(PoolArena arena, int chunkSize)
        {
            _unpooled = true;
            _arena = arena;
            _buffer = new byte[chunkSize];
        }

        public PoolChunk Prev
        {
            get;
            set;
        }

        public PoolChunk Next
        {
            get;
            set;
        }

        public PoolArena Arena
        {
            get
            {
                return _arena;
            }
        }

        public byte[] Buffer
        {
            get
            {
                return _buffer;
            }
        }

        public int Usage
        {
            get
            {
                if (_freeBytes == 0)
                {
                    return 100;
                }

                var freePercentage = (int)(_freeBytes * 100L / _chunkSize);
                if (freePercentage == 0)
                {
                    return 99;
                }
                return 100 - freePercentage;
            }
        }

        public bool Unpooled
        {
            get
            {
                return _unpooled;
            }
        }

        public long Allocate(int normCapacity)
        {
            if (_arena.IsTinyOrSmall(normCapacity))
            {
                return this.AllocatePage(normCapacity);
            }
            return this.AllocateRun(normCapacity);
        }

        public void Free(long handle)
        {
            var memoryMapIdx = (int)handle;
            var bitmapIdx = (int)(handle >> 32);
            if (bitmapIdx != 0)
            {
                // free a subpage
                var subpage = _subpages[this.PageIdx(memoryMapIdx)];
                if (subpage.Free(bitmapIdx & 0x3FFFFFFF))
                {
                    return;
                }
            }
            _freeBytes += this.RunLength(memoryMapIdx);
            _memoryMap[memoryMapIdx] = _depthMap[memoryMapIdx];
            this.UpdateParentsFree(memoryMapIdx);
        }

        public void InitBuffer(PooledByteBuffer buffer, long handle, int reqCapacity)
        {
            int memoryMapIdx = (int)handle;
            int bitmapIdx = (int)(handle >> 32);
            if (bitmapIdx == 0)
            {
                //=pagesize
                buffer.Init(this, handle, this.RunOffset(memoryMapIdx), reqCapacity, this.RunLength(memoryMapIdx));
            }
            else
            {
                this.InitBufferWithSubpage(buffer, handle, bitmapIdx, reqCapacity);
            }
        }

        public void InitBufferWithSubpage(PooledByteBuffer buf, long handle, int reqCapacity)
        {
            InitBufferWithSubpage(buf, handle, (int)(handle >> 32), reqCapacity);
        }

        private void InitBufferWithSubpage(PooledByteBuffer buf, long handle, int bitmapIdx, int reqCapacity)
        {
            var memoryMapIdx = (int)handle;
            var idx = this.PageIdx(memoryMapIdx);
            var subpage = _subpages[idx];
            buf.Init(this, handle, this.RunOffset(memoryMapIdx) + (bitmapIdx & 0x3FFFFFFF) * subpage.ElemSize, reqCapacity, subpage.ElemSize);
        }

        private long AllocateRun(int normCapacity)
        {
            var d = _maxOrder - (Utils.Log2(normCapacity) - _pageShifts);
            var id = this.AllocateNode(d);
            if (id < 0)
            {
                return id;
            }
            _freeBytes -= this.RunLength(id);
            return id;
        }

        private long AllocatePage(int normCapacity)
        {
            var id = this.AllocateNode(_maxOrder);
            if (id < 0)
            {
                return id;
            }
            _freeBytes -= _pageSize;

            int subpageIdx = this.PageIdx(id);
            var subpage = _subpages[subpageIdx];
            if (subpage == null)
            {
                subpage= new PoolPage(this, id, this.RunOffset(id), _pageSize, normCapacity);
                _subpages[subpageIdx] = subpage;
            }
            return subpage.Allocate();
        }

        private int AllocateNode(int d)
        {
            var id = 1;           
            var initial = -(1 << d); 
            var val = _memoryMap[id];
            if (val > d)
            { // unusable
                return -1;
            }
            while (val < d || (id & initial) == 0)
            { 
                id <<= 1;
                val = _memoryMap[id];
                if (val > d)
                {
                    id ^= 1;
                    val = _memoryMap[id];
                }
            }
            _memoryMap[id] = _unusable; // mark as unusable
            this.UpdateParentsAlloc(id);
            return id;
        }

        private void UpdateParentsAlloc(int id)
        {
            while (id > 1)
            {
                var parentId = id >> 1;
                var val1 = _memoryMap[id];
                var val2 = _memoryMap[id ^ 1];
                var val = val1 < val2 ? val1 : val2;
                _memoryMap[parentId] = val;
                id = parentId;
            }
        }

        private void UpdateParentsFree(int id)
        {
            var logChild = _depthMap[id] + 1;
            while (id > 1)
            {
                var parentId = id >> 1;
                var val1 = _memoryMap[id];
                var val2 = _memoryMap[id ^ 1];
                logChild -= 1; // in first iteration equals log, subsequently reduce 1 from logChild as we traverse up

                if (val1 == logChild && val2 == logChild)
                {
                    _memoryMap[parentId] = (byte)(logChild - 1);
                }
                else
                {
                    var val = val1 < val2 ? val1 : val2;
                    _memoryMap[parentId] = val;
                }
                id = parentId;
            }
        }

        private int RunOffset(int id)
        {
            var shift = id ^ 1 << _depthMap[id];
            return shift * this.RunLength(id);
        }

        private int RunLength(int id)
        {
            return 1 << _log2ChunkSize - _depthMap[id];
        }

        private int PageIdx(int memoryMapIdx)
        {
            return memoryMapIdx ^ _maxSubpageAllocs;
        }
    }
}
