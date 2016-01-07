// Copyright (c) 2016 Yamool. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

namespace Yamool.Buffers
{
    using System;
    using System.Text;

    public static class ByteBufferExtensions
    {      
        /// <summary>
        /// Gets a boolean value at the specified position index in the <see cref="ByteBuffer"/>. 
        /// </summary>
        /// <param name="buffer">The byte buffer to get.</param>
        /// <param name="index">The position index of buffer which begin to read.</param>
        /// <returns>A boolean value.</returns>
        /// <remarks>This method does not modify the readerIndex or writerIndex of this buffer.</remarks>
        public static bool GetBoolean(this ByteBuffer buffer, int index)
        {            
            return GetByte(buffer, index) != 0;
        }

        /// <summary>
        /// Gets a 8-bit unsigned integer at the specified absolute index in this buffer. 
        /// </summary>
        /// <param name="buffer">The byte buffer to get.</param>
        /// <param name="index">The position index of buffer which begin to read.</param>
        /// <returns>A 8-bit unsigned integer.</returns>
        /// <remarks>This method does not modify readerIndex or writerIndex of this buffer.</remarks>
        public static byte GetByte(this ByteBuffer buffer, int index)
        {
            var bytes = buffer.InternalGetBytes(index, 1);
            return bytes[0];
        }

        /// <summary>
        /// Gets a Unicode character value at the specified absolute index in this buffer.         
        /// </summary>
        /// <param name="buffer">The byte buffer to get.</param>
        /// <param name="index">The position index of buffer which begin to read.</param>
        /// <returns>The Unicode character value.</returns>
        /// <remarks>This method does not modify readerIndex or writerIndex of this buffer.</remarks>
        public static char GetChar(this ByteBuffer buffer, int index)
        {
            return (char)GetShort(buffer, index);
        }

        /// <summary>
        /// Gets a 64-bit floating point number at the specified absolute index in this buffer. 
        /// </summary>
        /// <param name="buffer">The byte buffer to get.</param>
        /// <param name="index">The position index of buffer which begin to read.</param>
        /// <returns>A 64-bit floating point number.</returns>
        /// <remarks>This method does not modify readerIndex or writerIndex of this buffer.</remarks>
        public static double GetDouble(this ByteBuffer buffer, int index)
        {
            var value = buffer.GetLong(index);
            return BitConverter.Int64BitsToDouble(value);
        }

        /// <summary>
        /// Gets a single-precision floating point number at the specified absolute index in this buffer. 
        /// This method does not modify readerIndex or writerIndex of this buffer.
        /// </summary>
        /// <param name="buffer">The byte buffer to get.</param>
        /// <param name="index">The position index of buffer which begin to read.</param>
        /// <returns>A single-precision floating point number.</returns>
        /// <remarks>This method does not modify readerIndex or writerIndex of this buffer.</remarks>
        public unsafe static float GetFloat(this ByteBuffer buffer, int index)
        {
            var value = buffer.GetInt(index);
            return *(float*)(&value);
        }

        /// <summary>
        /// Gets a 32-bit singed integer at the specified absolute index in this buffer.
        /// </summary>
        /// <param name="buffer">The byte buffer to get.</param>
        /// <param name="index">The position index of buffer which begin to read.</param>
        /// <returns>A 32-bit signed integer.</returns>
        /// <remarks>This method does not modify readerIndex or writerIndex of this buffer.</remarks>
        public static int GetInt(this ByteBuffer buffer, int index)
        {
            var bytes = buffer.InternalGetBytes(index, 4);
            return (bytes[0] & 0xff) << 24 | (bytes[1] & 0xff) << 16 | (bytes[2] & 0xff) << 8 | bytes[3] & 0xff;
        }

        /// <summary>
        /// Gets a 64-bit signed integer at the specified absolute index in this buffer.         
        /// </summary>
        /// <param name="buffer">The byte buffer to get.</param>
        /// <param name="index">The position index of buffer which begin to read.</param>
        /// <returns>A 64-bit long integer </returns>
        /// <remarks>This method does not modify readerIndex or writerIndex of this buffer.</remarks>     
        public static long GetLong(this ByteBuffer buffer, int index)
        {
            var bytes = buffer.InternalGetBytes(index, 8);
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
        /// <param name="buffer">The byte buffer to get.</param>
        /// <param name="index">The position index of buffer which begin to read.</param>
        /// <returns>A 16-bit signed integer</returns>
        /// <remarks>This method does not modify readerIndex or writerIndex of this buffer.</remarks>
        public static short GetShort(this ByteBuffer buffer, int index)
        {
            var bytes = buffer.InternalGetBytes(index, 2);
            return (short)((bytes[0] << 8 | bytes[1]) & 0xff);
        }

        /// <summary>
        /// Sets a boolean value at specified absolute index of the buffer.
        /// </summary>
        /// <param name="buffer">The byte buffer to set.</param>
        /// <param name="index">The position index within buffer begin to set.</param>
        /// <param name="value">The value to set.</param>
        public static void SetBoolean(this ByteBuffer buffer, int index, bool value)
        {            
            buffer.SetByte(index, value ? (byte)1 : (byte)0);
        }

        /// <summary>
        /// Sets a byte value at specified absolute index of the buffer.
        /// </summary>
        /// <param name="buffer">The byte buffer to set.</param>
        /// <param name="index">The position index within buffer begin to set.</param>
        /// <param name="value">The value to set.</param>
        public static void SetByte(this ByteBuffer buffer, int index, byte value)
        {
            buffer.InternalSetBytes(index, new byte[] { value });
        }

        /// <summary>
        /// Sets a unicode character at the specified absolute index in this buffer.      
        /// </summary>
        /// <param name="buffer">The byte buffer to set.</param>
        /// <param name="index">The position index within buffer begin to set.</param>
        /// <param name="value">The value to set.</param>
        public static void SetChar(this ByteBuffer buffer, int index, char value)
        {
            buffer.SetShort(index, (short)value);
        }

        /// <summary>
        /// Sets a double-precision floating-point number at the specified absolute index in this buffer.      
        /// </summary>
        /// <param name="buffer">The byte buffer to set.</param>
        /// <param name="index">The position index within buffer begin to set.</param>
        /// <param name="value">The value to set.</param>
        public static void SetDouble(this ByteBuffer buffer, int index, double value)
        {
            buffer.SetLong(index, BitConverter.DoubleToInt64Bits(value));
        }

        /// <summary>
        /// Sets a single-precision floating-point number at the specified absolute index in this buffer.      
        /// </summary>
        /// <param name="buffer">The byte buffer to set.</param>
        /// <param name="index">The position index within buffer begin to set.</param>
        /// <param name="value">The value to set.</param>
        public unsafe static void SetFloat(this ByteBuffer buffer, int index, float value)
        {
            buffer.SetInt(index, *(int*)(&value));
        }

        /// <summary>
        /// Sets a 64-bit signed integer at the specified absolute index in this buffer.      
        /// </summary>
        /// <param name="buffer">The byte buffer to set.</param>
        /// <param name="index">The position index within buffer begin to set.</param>
        /// <param name="value">The value to set.</param>
        public static void SetLong(this ByteBuffer buffer, int index, long value)
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
            buffer.InternalSetBytes(index, bytes);
        }

        public static void SetInt(this ByteBuffer buffer, int index, int value)
        {
            var bytes = new byte[4]{
                (byte)(value>>24),
                (byte)(value>>16),
                (byte)(value>>8),
                (byte)value
            };
            buffer.InternalSetBytes(index, bytes);
        }

        /// <summary>
        /// Sets a 16-bit singed integer at the specified absolute index in this buffer.      
        /// </summary>
        /// <param name="buffer">The byte buffer to set.</param>
        /// <param name="index">The position index within buffer begin to set.</param>
        /// <param name="value">The value to set.</param>
        public static void SetShort(this ByteBuffer buffer, int index, short value)
        {
            var bytes = new byte[2]{
                (byte)(value >> 8),
                (byte)value
            };
            buffer.InternalSetBytes(index, bytes);
        }

        /// <summary>
        /// Gets a boolean value at the current readerIndex and increases the readerIndex by 1.
        /// </summary>
        /// <returns>A boolean value.</returns>
        public static bool ReadBoolean(this ByteBuffer buffer)
        {
            return buffer.ReadByte() != 0;
        }

        /// <summary>
        /// Gets a byte value at the current readerIndex and increases the readerIndex by 1.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns>A 16-bit unsigned integer.</returns>
        public static byte ReadByte(this ByteBuffer buffer)
        {
            if (buffer.ReadableBytes < 1)
            {
                throw new InvalidOperationException("readerIndex has been reached writerIndex.");
            }
            var value = buffer.GetByte(buffer.ReaderIndex);
            buffer.SetReaderIndex(buffer.ReaderIndex + 1);
            return value;
        }

        /// <summary>
        /// Gets a unicode character at the current readerIndex and increases the readerIndex by 2.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static char ReadChar(this ByteBuffer buffer)
        {
            return (char)buffer.ReadShort();
        }

        /// <summary>
        /// Gets a 64-bit double-precesion floating-pointer number at the current readerIndex and increases the readerIndex by 8.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static double ReadDouble(this ByteBuffer buffer)
        {
            return BitConverter.Int64BitsToDouble(buffer.ReadLong());
        }

        /// <summary>
        /// Gets a 32-bit single precesion floating-pointer number at the current readerIndex and increases the readerIndex by 8.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public unsafe static float ReadFloat(this ByteBuffer buffer)
        {
            var value = buffer.ReadInt();
            return *(float*)(&value);
        }

        /// <summary>
        /// Gets a 32-bit signed integer at the current readerIndex and increases the readerIndex by 4.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static int ReadInt(this ByteBuffer buffer)
        {
            if (buffer.ReadableBytes < 4)
            {
                throw new InvalidOperationException("readerIndex has been reached writerIndex.");
            }
            var value = buffer.GetInt(buffer.ReaderIndex);
            buffer.SetReaderIndex(buffer.ReaderIndex + 4);
            return value;
        }

        /// <summary>
        /// Gets a 64-bit signed integer at the current readerIndex and increases the readerIndex by 8.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static long ReadLong(this ByteBuffer buffer)
        {
            if (buffer.ReadableBytes < 8)
            {
                throw new InvalidOperationException("readerIndex has been reached writerIndex.");
            }
            var value = buffer.GetLong(buffer.ReaderIndex);
            buffer.SetReaderIndex(buffer.ReaderIndex + 8);
            return value;
        }

        /// <summary>
        /// Gets a 8-bit signed integer at the current readerIndex and increases the readerIndex by 2.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static short ReadShort(this ByteBuffer buffer)
        {
            if (buffer.ReadableBytes < 2)
            {
                throw new InvalidOperationException("readerIndex has been reached writerIndex.");
            }
            var value = buffer.GetShort(buffer.ReaderIndex);
            buffer.SetReaderIndex(buffer.ReaderIndex + 2);
            return value;
        }

        /// <summary>
        /// Write a boolean value at the current writerIndex and increases the writerIndex by 1.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="value">The value to be write.</param>
        public static void Write(this ByteBuffer buffer, bool value)
        {
            buffer.Write(value ? (byte)1 : (byte)0);
        }

        /// <summary>
        /// Write a boolean value at the current writerIndex and increases the writerIndex by 1.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="value"></param>
        public static void Write(this ByteBuffer buffer, byte value)
        {
            buffer.EnsureWritable(1);
            buffer.SetByte(buffer.WriterIndex, value);
            buffer.SetWriterIndex(buffer.WriterIndex + 1);
        }

        /// <summary>
        /// Write a 16-bit signed integer at the current writerIndex and increases the writerIndex by 2.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="value"></param>
        public static void Write(this ByteBuffer buffer, short value)
        {
            buffer.EnsureWritable(2);
            buffer.SetShort(buffer.WriterIndex, value);
            buffer.SetWriterIndex(buffer.WriterIndex + 2);
        }

        /// <summary>
        /// Write a unicode charater at the current writerIndex and increases the writerIndex by 2.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="value"></param>
        public static void Write(this ByteBuffer buffer, char value)
        {
            buffer.Write((short)value);
        }

        /// <summary>
        /// Write a 32-bit signed integer at the current writerIndex and increases the writerIndex by 4.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="value"></param>
        public static void Write(this ByteBuffer buffer, int value)
        {
            buffer.EnsureWritable(4);
            buffer.SetInt(buffer.WriterIndex, value);
            buffer.SetWriterIndex(buffer.WriterIndex + 4);
        }

        /// <summary>
        /// Write a 64-bit signed integer at the current writerIndex and increases the writerIndex by 8.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="value"></param>
        public static void Write(this ByteBuffer buffer, long value)
        {
            buffer.EnsureWritable(8);
            buffer.SetLong(buffer.WriterIndex, value);
            buffer.SetWriterIndex(buffer.WriterIndex + 8);
        }

        /// <summary>
        /// Write a double-precision floating number at the current writerIndex and increases the writerIndex by 8.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="value"></param>
        public static void Write(this ByteBuffer buffer, double value)
        {
            buffer.Write(BitConverter.DoubleToInt64Bits(value));
        }

        /// <summary>
        /// Write a single precision floating number at the current writerIndex and increases the writerIndex by 4.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="value"></param>
        public unsafe static void Write(this ByteBuffer buffer, float value)
        {
            buffer.Write(*(int*)(&value));
        }
    }
}
