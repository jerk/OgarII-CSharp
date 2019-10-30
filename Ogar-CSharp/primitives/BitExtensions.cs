using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ogar_CSharp.primitives
{
    public static class BitExtensions
    {
        public enum Endian : int
        {
            Little,
            Big
        }
        public static int ToInt24(byte[] buffer, int startIndex, Endian endian = Endian.Little)
        {
            return ((endian == Endian.Little) ? (buffer[startIndex] | buffer[startIndex + 1] << 8 | (sbyte)buffer[startIndex + 2] << 16) : 
                ((sbyte)buffer[startIndex] << 16 | buffer[startIndex + 1] << 8 | buffer[startIndex + 2]));
        }
        public static int ToUInt24(byte[] buffer, int startIndex, Endian endian = Endian.Little)
        {
            return ((endian == Endian.Little) ? (buffer[startIndex] | buffer[startIndex + 1] << 8 | buffer[startIndex + 2] << 16) : 
                (buffer[startIndex] << 16 | buffer[startIndex + 1] << 8 | buffer[startIndex + 2]));
        }
        public static byte[] GetBytesInt24(int value)
        {
            return new byte[3] { (byte)value, (byte)(value >> 8), (byte)(value >> 0x10) };
        }
        public static byte[] GetBytesUInt24(uint value)
        {
            return new byte[3] { (byte)value, (byte)(value >> 8), (byte)(value >> 0x10) };
        }
    }
}
