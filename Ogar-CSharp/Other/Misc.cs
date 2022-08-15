using KUM.Shared;
using Ogar_CSharp.Other;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ogar_CSharp
{
    public struct Boost
    {
        public float dx;
        public float dy;
        public float d;
    }
    public static class Parallel
    {
        static Parallel()
        {
            main = new WorkerThreads();
            main.Start();
        }
        private static readonly WorkerThreads main;
        public static void ForEach<T>(IList<T> list, Action<T> callback)
        {
            int count = list.Count;
            var _ref = new WorkerThreads.ReferenceInt() { MAX = count };
            var hanlde = new EventWaitHandle(false, EventResetMode.ManualReset);
            for (int i = 0; i < count; i++)
            {
                main.EnqueueJob(list[i], (x) => callback((T)x), hanlde, _ref);
            }
            Console.WriteLine(hanlde.WaitOne());
        }
    }
    public static class Misc
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static unsafe extern int memcmp(byte* b1, byte* b2, long count);
        public unsafe class BinaryComparer<T> : IEqualityComparer<T> where T : unmanaged
        {
            private BinaryComparer() { }
            public static readonly BinaryComparer<T> Instance = new BinaryComparer<T>();
            public bool Equals(T x, T y)
            {

                return memcmp((byte*)&x, (byte*)&y, sizeof(T)) == 0;
            }

            public int GetHashCode([DisallowNull] T obj)
            {
                if(sizeof(T) <= 4)
                {
                    return (int)&obj;
                }
                return obj.GetHashCode();
            }
        }
        public const string version = "1.3.6";
        private static readonly Random randomMath = new Random();
        public static double RandomDouble() => randomMath.NextDouble();
        public static OgarColor RandomColor()
        {
            switch (Math.Floor(randomMath.NextDouble() * 6))
            {
                case 0:
                    return new OgarColor((byte)Math.Floor(randomMath.NextDouble() * 0x100) ,0xFF , 0x10);
                case 1:
                    return new OgarColor((byte)Math.Floor(randomMath.NextDouble() * 0x100),  0x10, 0xFF);
                case 2:
                    return new OgarColor(0xFF, (byte)Math.Floor(randomMath.NextDouble() * 0x100), 0x10);
                case 3:
                    return new OgarColor(0x10 , (byte)Math.Floor(randomMath.NextDouble() * 0x100) ,0xFF);
                case 4:
                    return new OgarColor(0x10, 0xFF, (byte)Math.Floor(randomMath.NextDouble() * 0x100));
                case 5:
                    return new OgarColor(0xFF, 0x10, (byte)Math.Floor(randomMath.NextDouble() * 0x100));
                default:
                    throw new Exception("This is not suppose to happen");
            }
        }
       /*public static int GrayScaleColor(int color)
        {
            int weight;
            if (color != 0) weight = Math.Floor(0.299 * (color & 0xFF) + 0.587 * ((color.g >> 8) & 0xFF) + 0.114 * (color.b >> 16));
            else weight = 0x7F + ~~(Random() * 0x80);
            return (weight << 16) | (weight << 8) | weight;
        }*/
        public static void ThrowIfBadNumber(params double[] numbers)
        {
            for (int i = 0; i < numbers.Length; i++)
                if (numbers[i] == double.NaN || double.IsInfinity(numbers[i]))
                    throw new Exception($"Bad number ({numbers[i]}, index {i}");
        }
        public static void ThrowIfBadOrNegativeNumber(params double[] numbers)
        {
            for (int i = 0; i < numbers.Length; i++)
                if (numbers[i] == double.NaN || double.IsInfinity(numbers[i]) || numbers[i] < 0)
                    throw new Exception($"Bad or negative number ({numbers[i]}, index {i}");
        }
        public static bool Intersects(RectangleF a, RectangleF b)
        {
            return a.X - a.Width < b.X + b.Width &&
            a.X + a.Width > b.X - b.Width &&
            a.Y - a.Height < b.Y + b.Height &&
            a.Y + a.Height > b.Y - b.Height;
        }
        public static bool FullyIntersects(RectangleF a, RectangleF b)
        {
            return a.X - a.Width >= b.X + b.Width &&
               a.X + a.Width <= b.X - b.Width &&
               a.Y - a.Height >= b.Y + b.Height &&
               a.Y + a.Height <= b.Y - b.Height;
        }
        public static (bool t, bool b, bool l, bool r) GetQuadFullIntersect(RectangleF a, RectangleF b) 
        {
            return (
                t: a.Y - a.Height < b.Y && a.Y + a.Height < b.Y,
                b: a.Y - a.Height > b.Y && a.Y + a.Height > b.Y,
                l: a.X - a.Width < b.X && a.X + a.Width < b.X,
                r: a.X - a.Width > b.X && a.X + a.Width > b.X
                );
        }
        public static (bool t, bool b, bool l, bool r) GetQuadIntersect(RectangleF a, RectangleF b)
        {
            return (
                t: a.Y - a.Height < b.Y || a.Y + a.Height < b.Y,
                b: a.Y - a.Height > b.Y || a.Y + a.Height > b.Y,
                l: a.X - a.Width < b.X || a.X + a.Width < b.X,
                r: a.X - a.Width > b.X || a.X + a.Width > b.X
            );
        }
    }
}
