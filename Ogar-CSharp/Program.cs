using System;

namespace Ogar_CSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var handle = new ServerHandle();
            handle.Start();
            Console.ReadKey();
        }
    }
}
