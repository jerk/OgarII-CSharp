using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace Ogar_CSharp
{
#pragma warning disable CA1815 // Override equals and operator equals on value types
    public struct Boost
#pragma warning restore CA1815 // Override equals and operator equals on value types
    {
        public float dx;
        public float dy;
        public float d;
    }
    public static class Misc
    {
        public const string version = "1.3.5";
        public const float SQRT_1_3 = 1.140175425099138f;
        public const float SQRT_2 = 1.414213562373095f;
        private static readonly Random randomMath = new Random();
        public static uint RandomColor()
        {
            switch (Math.Floor(randomMath.NextDouble() * 6))
            {
                case 0:
                    return (uint)((int)Math.Floor(randomMath.NextDouble() * 0x100) << 16) | (0xFF << 8) | 0x10;
                case 1:
                    return (uint)((int)Math.Floor(randomMath.NextDouble() * 0x100) << 16) | (0x10 << 8) | 0xFF;
                case 2:
                    return (uint)((0xFF << 16) | ((int)Math.Floor(randomMath.NextDouble() * 0x100) << 8) | 0x10);
                case 3:
                    return (uint)((0x10 << 16) | ((int)Math.Floor(randomMath.NextDouble() * 0x100) << 8) | 0xFF);
                case 4:
                    return (uint)((0x10 << 16) | (0xFF << 8) | (int)Math.Floor(randomMath.NextDouble() * 0x100));
                case 5:
                    return (uint)((0xFF << 16) | (0x10 << 8) | (int)Math.Floor(randomMath.NextDouble() * 0x100));
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
            return a.X - a.Width <= b.X + b.Width &&
            a.X + a.Width >= b.X - b.Width &&
            a.Y - a.Height <= b.Y + b.Height &&
            a.Y + a.Height >= b.Y - b.Height;
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
