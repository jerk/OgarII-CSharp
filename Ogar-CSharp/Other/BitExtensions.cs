using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ogar_CSharp
{
    public static class BitExtensions
    {
        public static int ToInt24(byte[] buffer, int startIndex)
        {
            int integer = (buffer[startIndex++]) << 16;
            integer |= (buffer[startIndex++]) << 8;
            integer |= buffer[startIndex++];
            return integer;
        }
        public static uint ToUInt24(byte[] buffer, int startIndex)
        {
            uint integer = (uint)(buffer[startIndex++]) << 16;
            integer |= (uint)(buffer[startIndex++]) << 8;
            integer |= buffer[startIndex++];
            return integer;
        }
        public static byte[] GetBytesInt24(int value)
        {
            var bytes = new byte[3];
            bytes[0] = (byte)(value & 0xff);
            bytes[1] = (byte)((value >> 8) & 0xff);
            bytes[2] = (byte)((value >> 16) & 0xff);
            return bytes;
        }
        public static byte[] GetBytesUInt24(uint value)
        {
            var bytes = new byte[3];
            bytes[0] = (byte)(value & 0xff);
            bytes[1] = (byte)((value >> 8) & 0xff);
            bytes[2] = (byte)((value >> 16) & 0xff);
            return bytes;
        }
    }
}
