// Copyright (c) 2016 Yamool. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

namespace Yamool.Buffers
{
    using System;
    using System.IO;

    /// <summary>    
    /// Implemented a <see cref="Stream"/> for the <see cref="ByteBuffer"/> class.
    /// </summary>
    public class BufferStream : Stream
    {
        private readonly ByteBuffer _buffer;        

        private BufferStream() { }

        public BufferStream(ByteBuffer buffer)
        {
            _buffer = buffer;
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Length
        {
            get
            {
                return _buffer.WriterIndex;
            }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!this.CanRead)
            {
                throw new NotSupportedException("This stream does not supoprt read operations.");
            }
            var readeableBytes = Math.Min(count, _buffer.ReadableBytes);
            if (readeableBytes == 0)
            {
                return 0;
            }
            _buffer.ReadBytes(buffer, offset, readeableBytes);
            return readeableBytes;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            _buffer.SetCapacity((int)value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!this.CanWrite)
            {
                throw new NotSupportedException("This stream does not supoprt write operations.");
            }
            _buffer.WriteBytes(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _buffer.Dispose();
            }
        }
    }
}
