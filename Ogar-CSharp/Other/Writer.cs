using Ogar_CSharp.Protocols;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Ogar_CSharp
{
    /// <summary>
    /// Used for writing data to send over the network
    /// </summary>
    public class Writer
    {
        public Writer(int writerInitCapacity = 100)
        {
            buf = new List<byte>(writerInitCapacity);
        }
        private readonly List<byte> buf;
        public byte[] ToArray() => buf.ToArray();
        public void WriteBytes(IEnumerable<byte> bytes) => buf.AddRange(bytes);
        public void WriteByte(byte a) => buf.Add(a);
        public void WriteUTF8String(string a)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));
            var tbuf = Encoding.UTF8.GetBytes(a);
            buf.AddRange(tbuf); 
            buf.Add(0);
        }
        public void WriteUTF16String(string a)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));
            var tbuf = Encoding.Unicode.GetBytes(a);
            buf.AddRange(tbuf); buf.Add(0); buf.Add(0);
        }
        public void WriteColor(uint a) 
            => buf.AddRange(BitExtensions.GetBytesUInt24(((a & 0xFF) << 16) | (((a >> 8) & 0xFF) << 8) | (a >> 16)));
        public unsafe void Write<T>(T t) where T : unmanaged
        {
            byte[] arr = new byte[sizeof(T)];
            fixed (byte* ptr = arr)
                *(T*)ptr = t;
            buf.AddRange(arr);
        }
    }
}
