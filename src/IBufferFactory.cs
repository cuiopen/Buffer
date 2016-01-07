// Copyright (c) 2016 Yamool. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

namespace Yamool.Buffers
{
    using System;

    /// <summary>
    /// Defines a buffer factory to create a new instance of <see cref="ByteBuffer"/>.
    /// </summary>
    public interface IBufferFactory
    {
        /// <summary>
        /// Create a new instance of buffer with the specified length.
        /// </summary>
        /// <param name="length">The initial size in bytes of the buffer.</param>
        /// <param name="maxCapacity">The max capacity to writable.</param>
        /// <returns>The new instance of <see cref="ByteBuffer"/></returns>
        ByteBuffer NewBuffer(int length, int maxCapacity);
    }
}
