using Ogar_CSharp.Protocols;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Buffers;
using System.IO;
using System.Diagnostics.CodeAnalysis;

namespace Ogar_CSharp
{
    /// <summary>
    /// Used for writing data to send over the network
    /// </summary>
    public class Writer
    {
        public enum EncodingType
        {
            UNKNOWN,
            UTF8,
            UTF16
        }
        public Writer()
        {
            mem = ArrayPool<byte>.Shared.Rent(RESIZE_RATE);
        }
        private byte[] mem;
        private int _pos;
        private const int RESIZE_RATE = 512;
        public int Length => _pos;
        private void CheckArraySize(int incomingByteCount)
        {
            int size = mem.Length;
            int requiredSize = incomingByteCount + _pos;
            if (requiredSize > size)
            {
                while (requiredSize >= size)
                {
                    size += RESIZE_RATE;
                }
                byte[] oldMem = mem;
                ResizeArray(ArrayPool<byte>.Shared.Rent(size));
                ArrayPool<byte>.Shared.Return(oldMem);
            }
        }
        private void ResizeArray(byte[] newArray)
        {
            Buffer.BlockCopy(mem, 0, newArray, 0, _pos);
            mem = newArray;
        }
        public void WriteBytes(ReadOnlySpan<byte> bytes)
        {
            CheckArraySize(bytes.Length);
            bytes.CopyTo(mem.AsSpan(_pos));
            _pos += bytes.Length;
        }
        public void WriteByte(byte @byte)
        {
            CheckArraySize(1);
            mem[_pos++] = @byte;
        }
        public ReadOnlySpan<byte> GetBytes()
        {
            return mem.AsSpan()[0.._pos];
        }
        public void CopyTo(Span<byte> buffer)
        {
            if (buffer.Length != _pos)
                throw new ArgumentOutOfRangeException(nameof(buffer.Length), "buffer does not match written byte count");
            mem.AsSpan(0, _pos).CopyTo(buffer);
        }
        public Span<byte> GetWritableChunk(int length)
        {
            CheckArraySize(length);
            var span = mem.AsSpan(_pos, length);
            _pos += length;
            return span;
        }
        public unsafe void WriteString(string a, EncodingType encoding)
        {
            
            if (a == null)
                throw new ArgumentNullException(nameof(a));
            int encoderCharSize = 0;
            Encoding encoder;
            switch (encoding)
            {
                case EncodingType.UTF8:
                    encoderCharSize = 1;
                    encoder = Encoding.UTF8;
                    break;
                case EncodingType.UTF16:
                    encoderCharSize = 2;
                    encoder = Encoding.Unicode;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(encoding));
            }
            if (a.Length > 0)
            {
                int maxBytes = encoder.GetMaxByteCount(a.Length);
                maxBytes += encoderCharSize;
                CheckArraySize(maxBytes);

                int bytesCopied = encoder.GetBytes(a, mem.AsSpan(_pos, maxBytes));
                _pos += bytesCopied;

            }
            else
                CheckArraySize(encoderCharSize);
            Span<byte> temp = stackalloc byte[encoderCharSize];
            temp.CopyTo(mem.AsSpan(_pos, temp.Length));
            _pos += temp.Length;
        }
        public unsafe void WriteUTF8String([NotNull] string a)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));
            WriteString(a, EncodingType.UTF8);
        }
        public void WriteUTF16String([NotNull] string a)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));
            WriteString(a, EncodingType.UTF16);
        }
        public unsafe void Write<T>(T t) where T : unmanaged
        {
            if (sizeof(T) == 1)
                WriteByte(*(byte*)&t);
            else
                WriteBytes(new ReadOnlySpan<byte>(&t, sizeof(T)));
        }
        ~Writer()
        {
            ArrayPool<byte>.Shared.Return(mem);
        }
    }
}
