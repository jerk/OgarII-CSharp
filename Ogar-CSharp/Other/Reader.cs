using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Unicode;

namespace Ogar_CSharp
{
    /// <summary>
    /// Used for reading data sent over the network
    /// </summary>
    public class DataReader
    {
        public readonly byte[] Data;
        private int _offset;
        public readonly int length;
        public int Offset { get => _offset; set => _offset = value; }
        public DataReader(byte[] data, int offset = 0)
        {
            this.Data = data;
            this._offset = offset;
            this.length = data.Length;
        }
        public byte ReadByte()
            => Data[_offset++];
        public unsafe T Read<T>() where T : unmanaged
        {

            int size = sizeof(T);
            T ret;
            byte* t = (byte*)&ret;
            for (int i = 0; i < size; i++)
                t[i] = Data[_offset++];

            return ret;
        }
        public void Skip(int count)
        {
            _offset += count;
        }
        public string ReadUTF16String()
        {
            int start = this._offset, index = this._offset;
            while (index + 2 < this.length && this.Read<ushort>() != 0) 
                index += 2;
            return Encoding.Unicode.GetString(Data, start, index - start); 
        }
        public string ReadUTF8String()
        {
            int start = this._offset, index = this._offset;
            while (index + 1 < this.length && this.ReadByte() != 0)
                index++;
            return Encoding.UTF8.GetString(Data, start, index - start); 
        }
    }
}

