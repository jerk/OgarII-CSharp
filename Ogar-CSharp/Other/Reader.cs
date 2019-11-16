using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Unicode;

namespace Ogar_CSharp
{
    /// <summary>
    /// Used for reading data sent over the network
    /// </summary>
    public class Reader
    {
        public readonly byte[] data;
        public int offset;
        public readonly int length;
        public Reader(byte[] data, int offset = 0)
        {
            this.data = data;
            this.offset = offset;
            this.length = data.Length;
        }
        public T Read<T>(bool? isColor = null, bool? isUTF16 = null)
        {
            object data = null;
            var type = typeof(T);
            if (type == typeof(byte))
                data = ReadByte();
            else if (type == typeof(sbyte))
                data = ReadSByte();
            else if (type == typeof(ushort))
                data = ReadUShort();
            else if (type == typeof(short))
                data = ReadShort();
            else if (type == typeof(uint))
                data = ReadUInt();
            else if (type == typeof(int))
                data = ReadInt();
            else if (type == typeof(float))
                data = ReadFloat();
            else if (type == typeof(double))
                data = ReadDouble();
            else if (isColor != null && isColor.Value)
                data = ReadColor();
            else if (type == typeof(string))
                if (isUTF16 != null && isUTF16.Value)
                    data = ReadUTF16String();
                else
                    data = ReadUTF8String();
            return (T)data;
        }
        public byte ReadByte()
            => data[offset++];
        public sbyte ReadSByte()
        {
            byte a = data[this.offset];
            return (sbyte)(a < 0x7F ? a : -a + 0x7F);
        }
        public ushort ReadUShort()
        {
            var a = BitConverter.ToUInt16(data, offset);
            offset += 2;
            return a;
        }
        public short ReadShort()
        {
            var a = BitConverter.ToInt16(data, offset);
            offset += 2;
            return a;
        }
        public uint ReadUInt()
        {
            var a = BitConverter.ToUInt32(data, offset);
            offset += 4;
            return a;
        }
        public int ReadInt()
        {
            var a = BitConverter.ToInt32(data, offset);
            offset += 4;
            return a;
        }
        public float ReadFloat()
        {
            var a = BitConverter.ToSingle(data, offset);
            offset += 4;
            return a;
        }
        public double ReadDouble()
        {
            var a = BitConverter.ToDouble(data, offset);
            offset += 8;
            return a;
        }
        public int ReadInt24()
        {
            var a = BitExtensions.ToInt24(data, offset);
            offset += 3;
            return a;
        }
        public uint ReadUInt24()
        {
            var a = BitExtensions.ToUInt24(data, offset);
            offset += 3;
            return a;
        }
        public void Skip(int count)
        {
            offset += count;
        }
        public string ReadUTF16String()
        {
            int start = this.offset, index = this.offset;
            while (index + 2 < this.length && this.ReadUShort() != 0) 
                index += 2;
            return Encoding.Unicode.GetString(data, start, index - start); 
        }
        public string ReadUTF8String()
        {
            int start = this.offset, index = this.offset;
            while (index + 1 < this.length && this.ReadByte() != 0)
                index++;
            return Encoding.UTF8.GetString(data, start, index - start); 
        }
        public int ReadColor()
        {
            var a = (int)ReadUInt24();
            return (a & 0xFF) | ((a >> 8) & 0xFF) | (a >> 16);
        }
    }
}

