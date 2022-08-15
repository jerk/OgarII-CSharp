using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Unicode;

namespace Ogar_CSharp
{
    /// <summary>
    /// Used for reading data sent over the network
    /// </summary>
    public ref struct DataReader
    {
        public ReadOnlySpan<byte> buffer;
        public int Length => buffer.Length;
        public int Offset { get; private set; }
        public DataReader(ReadOnlySpan<byte> buffer, int offset = 0)
        {
            this.buffer = buffer;
            this.Offset = offset;
        }
        public byte ReadByte()
            => buffer[Offset++];
        public unsafe T Read<T>() where T : unmanaged
        {
            T ret;
            fixed(byte* unsafeBuf = &buffer[Offset])
                ret = *(T*)unsafeBuf;
            Offset += sizeof(T);
            return ret;
        }
        public void Reset()
        {
            Offset = 0;
        }
        public void Skip(int count)
        {
            Offset += count;
        }
        public string ReadUTF16String()
        {
            int start = this.Offset, index = this.Offset;
            while ((index + 2) < Length && this.Read<ushort>() != 0) 
                index += 2;
            return Encoding.Unicode.GetString(buffer.Slice(start, index - start)); 
        }
        public string ReadUTF8String()
        {
            int start = this.Offset, index = this.Offset;
            while ((index + 1) < Length && buffer[Offset++] != 0)
                index++;
            return Encoding.UTF8.GetString(buffer.Slice(start, index - start)); 
        }
    }
}

