using Ogar_CSharp.Protocols;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Ogar_CSharp
{
    public class Writer : IEnumerable
    {
        private List<byte> buf = new List<byte>();
        public byte[] RawBuffer => buf.ToArray();
        public void WriteByte(byte a) => buf.Add(a);
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
            var tbuf = Encoding.UTF8.GetBytes(a ?? "");
            buf.AddRange(tbuf); buf.Add(0);
        }
        public void WriteUTF16String(string a)
        {
            var tbuf = Encoding.Unicode.GetBytes(a);
            buf.AddRange(tbuf); buf.Add(0); buf.Add(0);
        }
        public void WriteColor(uint a) 
            => buf.AddRange(BitExtensions.GetBytesUInt24(((a & 0xFF) << 16) | (((a >> 8) & 0xFF) << 8) | (a >> 16)));


        public void Add(object obj, bool? shouldUseUTF16 = null)
        {
            Type objType = obj.GetType();
            if (objType == typeof(sbyte))
                WriteSByte((sbyte)obj);
            else if (objType == typeof(byte))
                WriteByte((byte)obj);
            else if (objType == typeof(ushort))
                WriteUShort((ushort)obj);
            else if (objType == typeof(short))
                WriteShort((short)obj);
            else if (objType == typeof(uint))
                WriteUInt((uint)obj);
            else if (objType == typeof(int))
                WriteInt((int)obj);
            else if (objType == typeof(float))
                WriteFloat((float)obj);
            else if (objType == typeof(double))
                WriteDouble((double)obj);
            else if (objType == typeof(string))
                if (shouldUseUTF16 != null && shouldUseUTF16.Value)
                    WriteUTF16String((string)obj);
                else 
                    WriteUTF8String((string)obj);
        }

        public IEnumerator GetEnumerator()
        {
            return null;
        }
    }
}
