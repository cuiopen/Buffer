// Copyright (c) 2016 Yamool. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

namespace Yamool.Buffers
{
    using System;
    using System.Threading;
    /// <summary>
    /// This buffer factory create a <see cref="ByteBuffer"/> object that can be reused when it release and return to the pooling.
    /// </summary>
    public sealed class PooledBufferFactory : IBufferFactory
    {
        private static readonly int DEFAULT_NUM_ARENA = Math.Max(4, Environment.ProcessorCount);
        private static readonly int DEFAULT_PAGE_SIZE = 4096;
        private static readonly int DEFAULT_MAX_ORDER = 11;
        private static readonly int MAX_CHUNK_SIZE = 1024 * 1024 * 1024;

        private PoolArena[] _arenas;
        private int _seqNum;

        /// <summary>
        /// Initialize new instance of the <see cref="IBufferFactory"/>.
        /// </summary>
        public PooledBufferFactory()
            : this(DEFAULT_NUM_ARENA, DEFAULT_PAGE_SIZE, DEFAULT_MAX_ORDER)
        {
        }

        /// <summary>
        /// Initialize new instance of the <see cref="IBufferFactory"/>.
        /// </summary>
        /// <param name="numberArenas"></param>
        /// <param name="pageSize"></param>
        /// <param name="maxOrder"></param>
        public PooledBufferFactory(int numberArenas, int pageSize, int maxOrder)
        {
            if (numberArenas < 0)
            {
                throw new ArgumentException("numberArenas less than 0.");
            }
            if (pageSize < 4096 || (pageSize & pageSize - 1) != 0)
            {
                throw new ArgumentException("pageSize less than 4096 or not is power of 2.");
            }
            if ((pageSize << maxOrder) > MAX_CHUNK_SIZE)
            {
                throw new ArgumentException(string.Format("The size of each of chunk cannot be greater than {0} bytes.", MAX_CHUNK_SIZE), "pageSize and maxOrder");
            }
            if (maxOrder < 0)
            {
                throw new ArgumentException("maxOrder less than 0.");
            }
            _arenas = new PoolArena[numberArenas];
            for (var i = 0; i < numberArenas; i++)
            {
                _arenas[i] = new PoolArena("arena_" + i, pageSize, maxOrder, pageSize << maxOrder);
            }
        }

        public ByteBuffer NewBuffer(int length, int maxCapacity = int.MaxValue)
        {
            var p = (int)Math.Abs(Interlocked.Increment(ref _seqNum) % _arenas.Length);
            var arena = _arenas[p];
            var buffer = new PooledByteBuffer(maxCapacity);
            arena.Allocate(buffer, length);
            return buffer;
        }
    }
}
