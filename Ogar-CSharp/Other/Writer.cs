using Ogar_CSharp.protocols;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ogar_CSharp
{
    /// <summary>
    /// Designed for use with the modern protocol
    /// </summary>
    public interface ISerializeable
    {
        public byte[] Serialize(byte? flags);
        public void Deserialize(byte[] buffer);
    }
    public class Writer
    {
        public List<byte> buf = new List<byte>();
        public int offset = 0;
        public void WriteByte(byte a)
        {
            buf.Add(a);
        }
        public void WriteSByte(sbyte a)
            => buf.Add((byte)a);
        public void WriteUShort(ushort a)
            => buf.AddRange(BitConverter.GetBytes(a));
        public void WriteShort(short a)
            => buf.AddRange(BitConverter.GetBytes(a));
        public void WriteInt24(int a)
            => buf.AddRange(BitExtensions.GetBytesInt24(a));
        public void WriteUInt24(uint a)
            => buf.AddRange(BitExtensions.GetBytesUInt24(a));
        public void WriteInt(int a)
            => buf.AddRange(BitConverter.GetBytes(a));
        public void WriteUInt(uint a)
            => buf.AddRange(BitConverter.GetBytes(a));
        public void WriteFloat(float a)
            => buf.AddRange(BitConverter.GetBytes(a));
        public void WriteDouble(double a)
            => buf.AddRange(BitConverter.GetBytes(a));
        public void WriteUTF8String(string a)
        {
            var tbuf = Encoding.UTF8.GetBytes(a);
            buf.AddRange(tbuf);
            offset += tbuf.Length;
            buf.Add(0);
        }
        public void WriteUTF16String(string a)
        {
            var tbuf = Encoding.Unicode.GetBytes(a);
            buf.AddRange(tbuf);
            offset += tbuf.Length;
            buf.Add(0);
            buf.Add(0);
        }
        public void WriteColor(uint a)
        {
            buf.AddRange(BitExtensions.GetBytesUInt24(((a & 0xFF) << 16) | (((a >> 8) & 0xFF) << 8) | (a >> 16)));
        }
        public byte[] RawBuffer
            => buf.ToArray();
    }
}
