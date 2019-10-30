using System;
using Ogar_CSharp.primitives;

namespace Ogar_CSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var writer = new Writer();
            writer.WriteColor(12);
            writer.WriteUTF8String("dcer1Fg1");
            var reader = new Reader(writer.RawBuffer);
            Console.WriteLine(reader.ReadColor());
            Console.WriteLine(reader.ReadUTF8String());
            Console.ReadKey();
        }
    }
}
