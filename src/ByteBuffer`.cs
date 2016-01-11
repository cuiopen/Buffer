// Copyright (c) 2016 Yamool. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

namespace Yamool.Buffers
{
    using System;
    using System.Text;

    public partial class ByteBuffer
    {      
        /// <summary>
        /// Gets a boolean value at the specified position index in the <see cref="ByteBuffer"/>. 
        /// </summary>
        /// <param name="index">The position index of buffer which begin to read.</param>
        /// <returns>A boolean value.</returns>
        /// <remarks>This method does not modify the readerIndex or writerIndex of this buffer.</remarks>
        public bool GetBoolean(int index)
        {            
            return this.GetByte(index) != 0;
        }

        /// <summary>
        /// Gets a 8-bit unsigned integer at the specified absolute index in this buffer. 
        /// </summary>
        /// <param name="buffer">The byte buffer to get.</param>
        /// <param name="index">The position index of buffer which begin to read.</param>
        /// <returns>A 8-bit unsigned integer.</returns>
        /// <remarks>This method does not modify readerIndex or writerIndex of this buffer.</remarks>
        public byte GetByte(int index)
        {
            var bytes = this.InternalGetBytes(index, 1);
            return bytes[0];
        }

        /// <summary>
        /// Gets a Unicode character value at the specified absolute index in this buffer.         
        /// </summary>
        /// <param name="index">The position index of buffer which begin to read.</param>
        /// <returns>The Unicode character value.</returns>
        /// <remarks>This method does not modify readerIndex or writerIndex of this buffer.</remarks>
        public char GetChar(int index)
        {
            return (char)GetShort(index);
        }

        /// <summary>
        /// Gets a 64-bit floating point number at the specified absolute index in this buffer. 
        /// </summary>
        /// <param name="index">The position index of buffer which begin to read.</param>
        /// <returns>A 64-bit floating point number.</returns>
        /// <remarks>This method does not modify readerIndex or writerIndex of this buffer.</remarks>
        public double GetDouble(int index)
        {
            var value = this.GetLong(index);
            return BitConverter.Int64BitsToDouble(value);
        }

        /// <summary>
        /// Gets a single-precision floating point number at the specified absolute index in this buffer. 
        /// This method does not modify readerIndex or writerIndex of this buffer.
        /// </summary>
        /// <param name="index">The position index of buffer which begin to read.</param>
        /// <returns>A single-precision floating point number.</returns>
        /// <remarks>This method does not modify readerIndex or writerIndex of this buffer.</remarks>
        public unsafe float GetFloat(int index)
        {
            var value = this.GetInt(index);
            return *(float*)(&value);
        }

        /// <summary>
        /// Gets a 32-bit singed integer at the specified absolute index in this buffer.
        /// </summary>
        /// <param name="index">The position index of buffer which begin to read.</param>
        /// <returns>A 32-bit signed integer.</returns>
        /// <remarks>This method does not modify readerIndex or writerIndex of this buffer.</remarks>
        public int GetInt(int index)
        {
            var bytes = this.InternalGetBytes(index, 4);
            return (bytes[0] & 0xff) << 24 | (bytes[1] & 0xff) << 16 | (bytes[2] & 0xff) << 8 | bytes[3] & 0xff;
        }

        /// <summary>
        /// Gets a 64-bit signed integer at the specified absolute index in this buffer.         
        /// </summary>
        /// <param name="index">The position index of buffer which begin to read.</param>
        /// <returns>A 64-bit long integer </returns>
        /// <remarks>This method does not modify readerIndex or writerIndex of this buffer.</remarks>     
        public long GetLong(int index)
        {
            var bytes = this.InternalGetBytes(index, 8);
            return (long)((bytes[0] & 0xff)) << 56 |
                    (long)((bytes[1] & 0xff)) << 48 |
                    (long)((bytes[2] & 0xff)) << 40 |
                    (long)((bytes[3] & 0xff)) << 32 |
                    (long)((bytes[4] & 0xff)) << 24 |
                    (long)((bytes[5] & 0xff)) << 16 |
                    (long)((bytes[6] & 0xff)) << 8 |
                    (long)(bytes[7] & 0xff) << 0;
        }

        /// <summary>
        /// Gets a 16-bit signed integer at the specified absolute index in this buffer.         
        /// </summary>
        /// <param name="index">The position index of buffer which begin to read.</param>
        /// <returns>A 16-bit signed integer</returns>
        /// <remarks>This method does not modify readerIndex or writerIndex of this buffer.</remarks>
        public short GetShort(int index)
        {
            var bytes = this.InternalGetBytes(index, 2);
            return (short)((bytes[0] << 8 | bytes[1]) & 0xff);
        }

        /// <summary>
        /// Sets a boolean value at specified absolute index of the buffer.
        /// </summary>
        /// <param name="buffer">The byte buffer to set.</param>
        /// <param name="index">The position index within buffer begin to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetBoolean(int index, bool value)
        {            
            this.SetByte(index, value ? (byte)1 : (byte)0);
        }

        /// <summary>
        /// Sets a byte value at specified absolute index of the buffer.
        /// </summary>
        /// <param name="index">The position index within buffer begin to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetByte(int index, byte value)
        {
            this.InternalSetBytes(index, new byte[] { value });
        }

        /// <summary>
        /// Sets a unicode character at the specified absolute index in this buffer.      
        /// </summary>
        /// <param name="index">The position index within buffer begin to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetChar(int index, char value)
        {
            this.SetShort(index, (short)value);
        }

        /// <summary>
        /// Sets a double-precision floating-point number at the specified absolute index in this buffer.      
        /// </summary>
        /// <param name="index">The position index within buffer begin to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetDouble(int index, double value)
        {
            this.SetLong(index, BitConverter.DoubleToInt64Bits(value));
        }

        /// <summary>
        /// Sets a single-precision floating-point number at the specified absolute index in this buffer.      
        /// </summary>
        /// <param name="index">The position index within buffer begin to set.</param>
        /// <param name="value">The value to set.</param>
        public unsafe void SetFloat(int index, float value)
        {
            this.SetInt(index, *(int*)(&value));
        }

        /// <summary>
        /// Sets a 64-bit signed integer at the specified absolute index in this buffer.      
        /// </summary>
        /// <param name="index">The position index within buffer begin to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetLong(int index, long value)
        {
            var bytes = new byte[8]{
                (byte)(value>>56),
                (byte)(value>>48),
                (byte)(value>>40),
                (byte)(value>>32),
                (byte)(value>>24),
                (byte)(value>>16),
                (byte)(value>>8),
                (byte)value
            };
            this.InternalSetBytes(index, bytes);
        }

        public void SetInt(int index, int value)
        {
            var bytes = new byte[4]{
                (byte)(value>>24),
                (byte)(value>>16),
                (byte)(value>>8),
                (byte)value
            };
            this.InternalSetBytes(index, bytes);
        }

        /// <summary>
        /// Sets a 16-bit singed integer at the specified absolute index in this buffer.      
        /// </summary>
        /// <param name="index">The position index within buffer begin to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetShort(int index, short value)
        {
            var bytes = new byte[2]{
                (byte)(value >> 8),
                (byte)value
            };
            this.InternalSetBytes(index, bytes);
        }

        /// <summary>
        /// Gets a boolean value at the current readerIndex and increases the readerIndex by 1.
        /// </summary>
        /// <returns>A boolean value.</returns>
        public bool ReadBoolean()
        {
            return this.ReadByte() != 0;
        }

        /// <summary>
        /// Gets a byte value at the current readerIndex and increases the readerIndex by 1.
        /// </summary>
        /// <returns>A 16-bit unsigned integer.</returns>
        public byte ReadByte()
        {
            if (this.ReadableBytes < 1)
            {
                throw new InvalidOperationException("readerIndex has been reached writerIndex.");
            }
            var value = this.GetByte(this.ReaderIndex);
            this.SetReaderIndex(this.ReaderIndex + 1);
            return value;
        }

        /// <summary>
        /// Gets a unicode character at the current readerIndex and increases the readerIndex by 2.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public char ReadChar()
        {
            return (char)this.ReadShort();
        }

        /// <summary>
        /// Gets a 64-bit double-precesion floating-pointer number at the current readerIndex and increases the readerIndex by 8.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public double ReadDouble()
        {
            return BitConverter.Int64BitsToDouble(this.ReadLong());
        }

        /// <summary>
        /// Gets a 32-bit single precesion floating-pointer number at the current readerIndex and increases the readerIndex by 8.
        /// </summary>
        /// <returns></returns>
        public unsafe float ReadFloat()
        {
            var value = this.ReadInt();
            return *(float*)(&value);
        }

        /// <summary>
        /// Gets a 32-bit signed integer at the current readerIndex and increases the readerIndex by 4.
        /// </summary>
        /// <returns></returns>
        public int ReadInt()
        {
            if (this.ReadableBytes < 4)
            {
                throw new InvalidOperationException("readerIndex has been reached writerIndex.");
            }
            var value = this.GetInt(this.ReaderIndex);
            this.SetReaderIndex(this.ReaderIndex + 4);
            return value;
        }

        /// <summary>
        /// Gets a 64-bit signed integer at the current readerIndex and increases the readerIndex by 8.
        /// </summary>
        /// <returns></returns>
        public long ReadLong()
        {
            if (this.ReadableBytes < 8)
            {
                throw new InvalidOperationException("readerIndex has been reached writerIndex.");
            }
            var value = this.GetLong(this.ReaderIndex);
            this.SetReaderIndex(this.ReaderIndex + 8);
            return value;
        }

        /// <summary>
        /// Gets a 8-bit signed integer at the current readerIndex and increases the readerIndex by 2.
        /// </summary>
        /// <returns></returns>
        public short ReadShort()
        {
            if (this.ReadableBytes < 2)
            {
                throw new InvalidOperationException("readerIndex has been reached writerIndex.");
            }
            var value = this.GetShort(this.ReaderIndex);
            this.SetReaderIndex(this.ReaderIndex + 2);
            return value;
        }

        /// <summary>
        /// Write a boolean value at the current writerIndex and increases the writerIndex by 1.
        /// </summary>
        /// <param name="value">The value to be write.</param>
        public void Write( bool value)
        {
            this.Write(value ? (byte)1 : (byte)0);
        }

        /// <summary>
        /// Write a boolean value at the current writerIndex and increases the writerIndex by 1.
        /// </summary>
        /// <param name="value"></param>
        public void Write(byte value)
        {
            this.EnsureWritable(1);
            this.SetByte(this.WriterIndex, value);
            this.SetWriterIndex(this.WriterIndex + 1);
        }

        /// <summary>
        /// Write a 16-bit signed integer at the current writerIndex and increases the writerIndex by 2.
        /// </summary>
        /// <param name="value"></param>
        public void Write(short value)
        {
            this.EnsureWritable(2);
            this.SetShort(this.WriterIndex, value);
            this.SetWriterIndex(this.WriterIndex + 2);
        }

        /// <summary>
        /// Write a unicode charater at the current writerIndex and increases the writerIndex by 2.
        /// </summary>
        /// <param name="value"></param>
        public void Write(char value)
        {
            this.Write((short)value);
        }

        /// <summary>
        /// Write a 32-bit signed integer at the current writerIndex and increases the writerIndex by 4.
        /// </summary>
        /// <param name="value"></param>
        public void Write(int value)
        {
            this.EnsureWritable(4);
            this.SetInt(this.WriterIndex, value);
            this.SetWriterIndex(this.WriterIndex + 4);
        }

        /// <summary>
        /// Write a 64-bit signed integer at the current writerIndex and increases the writerIndex by 8.
        /// </summary>
        /// <param name="value"></param>
        public void Write(long value)
        {
            this.EnsureWritable(8);
            this.SetLong(this.WriterIndex, value);
            this.SetWriterIndex(this.WriterIndex + 8);
        }

        /// <summary>
        /// Write a double-precision floating number at the current writerIndex and increases the writerIndex by 8.
        /// </summary>
        /// <param name="value"></param>
        public void Write(double value)
        {
            this.Write(BitConverter.DoubleToInt64Bits(value));
        }

        /// <summary>
        /// Write a single precision floating number at the current writerIndex and increases the writerIndex by 4.
        /// </summary>
        /// <param name="value"></param>
        public unsafe void Write(float value)
        {
            this.Write(*(int*)(&value));
        }
    }
}
