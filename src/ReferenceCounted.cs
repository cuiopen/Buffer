// Copyright (c) 2016 Yamool. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

namespace Yamool.Buffers
{
    using System;
    using System.Threading;

    /// <summary>
    /// Abstract class that counting a object reference times.
    /// </summary>
    public abstract class ReferenceCounted : IDisposable
    {
        private int _refCounted = 1;
        
        /// <summary>
        /// Gets the reference count of this object.
        /// If value is <c>0</c> that means this object has been deallocated.
        /// </summary>
        public int RefCount
        {
            get
            {
                return _refCounted;
            }
        }

        /// <summary>
        /// Increases the reference count by the specified  increment.
        /// </summary>
        public void Retain(int increment = 1)
        {
            if (increment <= 0)
            {
                throw new ArgumentException("increment less than or equal to zero(0).");
            }
            if (_refCounted == 0)
            {
                throw new ReferenceCountException(_refCounted);
            }
            if (_refCounted > int.MaxValue - increment)
            {
                throw new ReferenceCountException(_refCounted, increment);
            }
            Interlocked.Add(ref _refCounted, increment);
        }

        /// <summary>
        /// Decreases the reference count by the specified decrement.        
        /// </summary>
        /// <remarks>If the reference count are equal to 0 then will auto disposing.</remarks>
        public bool Release(int decrement = 1)
        {
            if (decrement <= 0)
            {
                throw new ArgumentException("decrement less than or equal to zero(0).");
            }
            if (_refCounted == 0)
            {
                throw new ReferenceCountException(_refCounted);
            }
            if (_refCounted < decrement)
            {
                throw new ReferenceCountException(_refCounted, -decrement);
            }
            if (Interlocked.Add(ref _refCounted, -decrement) == 0)
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            this.Release();
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }

    public sealed class ReferenceCountException : Exception
    {
        public ReferenceCountException(int refCnt)
            : this("refCnt:" + refCnt)
        {
        }

        public ReferenceCountException(int refCnt, int increment)
            : this("refCnt: " + refCnt + ", " + (increment > 0 ? "increment: " + increment : "decrement: " + -increment))
        {
        }

        public ReferenceCountException(string message) : base(message) { }
    }
}
