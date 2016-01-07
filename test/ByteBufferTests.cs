namespace Yamool.Buffers.Tests
{
    using System;
    using System.IO;
    using NUnit.Framework;
    
    public abstract class ByteBufferTests
    {
        [Test]
        public void TestGetBoolean()
        {
            var buffer = this.GetBuffer(2);
            buffer.SetBoolean(0, true);
            buffer.SetBoolean(1, false);
            Assert.IsTrue(buffer.GetBoolean(0));
            Assert.IsFalse(buffer.GetBoolean(1));
        }

        [Test]
        public void TestGetChar()
        {
            var buffer = this.GetBuffer(2);
            buffer.SetChar(0, 'A');
            Assert.AreEqual('A', buffer.GetChar(0));
        }

        [Test]
        public void TestGetInt()
        {
            var buffer = this.GetBuffer(4);
            buffer.SetInt(0, 100);
            Assert.AreEqual(100, buffer.GetInt(0));
        }

        [Test]
        public void TestGetLong()
        {
            var buffer = this.GetBuffer(8);
            buffer.SetLong(0, 100);
            Assert.AreEqual(100, buffer.GetLong(0));
        }

        [Test]
        public void TestGetDouble()
        {
            var buffer = this.GetBuffer(8);
            buffer.SetDouble(0, 100.0d);
            Assert.AreEqual(100.0d, buffer.GetDouble(0));
        }

        [Test]
        public void TestGetFloat()
        {
            var buffer = this.GetBuffer(4);
            buffer.SetFloat(0, 100.0f);
            Assert.AreEqual(100.0f, buffer.GetFloat(0));
        }

        [Test]
        public void TestSetBytes()
        {
            var src = new byte[] { 65, 65, 65, 65, 65, 65, 65, 65, 65, 65 };
            var buffer = this.GetBuffer(20);
            buffer.SetBytes(0, src, 0, 10);
            var src2 = new byte[] { 66, 66, 66, 66, 66, 66, 66, 66, 66, 66 };
            buffer.SetBytes(10, src2, 0, 10);
            Assert.AreEqual(66, buffer.GetByte(19));
        }

        [Test]
        public void TestReadBytes()
        {
            var src = new byte[] { 65, 65, 65, 65, 65, 65, 65, 65, 65, 65 };
            var buffer = this.GetBuffer(10);
            buffer.WriteBytes(src, 0, 10);
            Assert.AreEqual(10, buffer.Capacity);
            var dst = new byte[10];
            buffer.ReadBytes(dst, 0, 10);
            Assert.AreEqual(src[0], dst[0]);
            Assert.AreEqual(src[9], dst[9]);
            Assert.AreEqual(10, buffer.ReaderIndex);
        }

        [Test]
        public void TestWriteBoolean()
        {
            var buffer = this.GetBuffer(0);
            buffer.Write(true);
            buffer.Write(false);
            Assert.IsTrue(buffer.ReadBoolean());
            Assert.IsFalse(buffer.ReadBoolean());
        }

        [Test]
        public void TestWriteDouble()
        {
            var buffer = this.GetBuffer(0);
            buffer.Write(100.0d);
            Assert.AreEqual(100.0d, buffer.ReadDouble());
        }

        [Test]
        public void TestWriteInt()
        {
            var buffer = this.GetBuffer(0);
            buffer.Write(1);
            Assert.AreEqual(1, buffer.ReadInt());
        }

        [Test]
        public void TestWriteLong()
        {
            var buffer = this.GetBuffer(0);
            buffer.Write(0L);
            Assert.AreEqual(0L, buffer.ReadLong());
        }

        [Test]
        public void TestWriteFloat()
        {
            var buffer = this.GetBuffer(0);
            buffer.Write(0f);
            Assert.AreEqual(0f, buffer.ReadShort());
        }

        [Test]
        public void TestWriteBytes()
        {
            var src = new byte[10];
            var buffer = this.GetBuffer(10);
            for (var i = 0; i < 10; i++)
            {
                buffer.WriteBytes(src, 0, 10);                
            }
            Assert.AreEqual(100, buffer.Capacity);            
        }

        [Test]
        public void TestSetIndex()
        {
            var buffer = this.GetBuffer(10);
            buffer.SetIndex(0, 5);
            buffer.Write(1);
            buffer.SetIndex(5, buffer.WriterIndex);
            Assert.AreEqual(1, buffer.ReadInt());
        }

        [Test]
        public void TestOverMaxCapacityException()
        {
            var buffer = this.GetBuffer(0, 100);
            var src = new byte[101];
            Assert.Throws<ArgumentException>(() => buffer.WriteBytes(src, 0, 101));
        }

        [Test]
        public void TestObjectDisposedException()
        {
            var buffer = this.GetBuffer(1);            
            buffer.Dispose();
            Assert.Throws<BufferAccessException>(() => buffer.GetChar(0));
        }

        [Test]
        public void TestReferenceCount()
        {
            var buffer = this.GetBuffer(0);
            Assert.AreEqual(1, buffer.RefCount);
            buffer.Retain();
            Assert.AreEqual(2, buffer.RefCount);
            buffer.Release();
            Assert.AreEqual(1, buffer.RefCount);
            buffer.Release();
            Assert.AreEqual(0, buffer.RefCount);
        }

        [Test]
        public void TestStream()
        {          
            var buffer = this.GetBuffer(0);
            var stream = new BufferStream(buffer);
            var bytes = new byte[1024];
            //1048576
            for (var i = 0; i < 1024; i++)
            {
                stream.Write(bytes, 0, bytes.Length);
            }          
            Assert.AreEqual(1048576, stream.Length);
            var readableBytes = 0;
            var count = 0;
            while ((count = stream.Read(bytes, 0, bytes.Length)) > 0)
            {
                readableBytes += count;
            }
            Assert.AreEqual(1048576, readableBytes);
            Assert.AreEqual(buffer.ReaderIndex, buffer.WriterIndex);
        }

        protected abstract ByteBuffer GetBuffer(int length, int maxCapacity = int.MaxValue);
    }

    [TestFixture]
    public class UnpooledBufferTests : ByteBufferTests
    {
        private UnpooledBufferFactory _bufferFactory;

        [SetUp]
        public void InitFactory()
        {
            _bufferFactory = new UnpooledBufferFactory();
        }

        protected override ByteBuffer GetBuffer(int length, int maxCapacity = int.MaxValue)
        {
            return _bufferFactory.NewBuffer(length, maxCapacity);
        }
    }

    [TestFixture]
    public class PooledBufferTests : ByteBufferTests
    {
        private PooledBufferFactory _bufferFactory;

        [SetUp]
        public void InitFactory()
        {
            _bufferFactory = new PooledBufferFactory();
        }

        protected override ByteBuffer GetBuffer(int length, int maxCapacity = int.MaxValue)
        {
            return _bufferFactory.NewBuffer(length, maxCapacity);
        }
    }
}
