using System;
using System.Collections.Generic;
using System.Text;
namespace Ogar_CSharp
{
    
    public static class Misc
    {
        public const string version = "1.3.5";
        public const float SQRT_1_3 = 1.140175425099138f;
        public const float SQRT_2 = 1.414213562373095f;
        private static Random randomMath = new Random();
        private static int Random()
            => randomMath.Next(0, 1);

        private static double RandomDobule()
            => randomMath.NextDouble();
        public static int RandomColor()
        {
            switch(~~(Random()) * 6)
            {
                case 0:
                    return (~~(Random() * 0x100) << 16) | (0xFF << 8) | 0x10;
                case 1: 
                    return (~~(Random() * 0x100) << 16) | (0x10 << 8) | 0xFF;
                case 2: 
                    return (0xFF << 16) | (~~(Random() * 0x100) << 8) | 0x10;
                case 3: 
                    return (0x10 << 16) | (~~(Random() * 0x100) << 8) | 0xFF;
                case 4: 
                    return (0x10 << 16) | (0xFF << 8) | ~~(Random() * 0x100);
                case 5: 
                    return (0xFF << 16) | (0x10 << 8) | ~~(Random() * 0x100);
                default:
                    throw new Exception("This is not suppose to happen");
            }
        }
       /* public static int GrayScaleColor(int color)
        {
            int weight;
            if (color != 0) weight = ~~(0.299 * (color & 0xFF) + 0.587 * ((color.g >> 8) & 0xFF) + 0.114 * (color.b >> 16));
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
        public static bool Intersects(Rect a, Rect b)
        {
            return a.x - a.w <= b.x + b.w &&
            a.x + a.w >= b.x - b.w &&
            a.y - a.h <= b.y + b.h &&
            a.y + a.h >= b.y - b.h;
        }
        public static bool FunnyIntersects(Rect a, Rect b)
        {
            return a.x - a.w >= b.x + b.w &&
              a.x + a.w <= b.x - b.w &&
              a.y - a.h >= b.y + b.h &&
              a.y + a.h <= b.y - b.h;
        }
        public static (bool t, bool b, bool l, bool r) GetQuadFullIntersect(Rect a, Rect b) 
        {
            return (
                t: a.y - a.h < b.y && a.y + a.h < b.y,
                b: a.y - a.h > b.y && a.y + a.h > b.y,
                l: a.x - a.w < b.x && a.x + a.w < b.x,
                r: a.x - a.w > b.x && a.x + a.w > b.x
            );
        }
        public static (bool t, bool b, bool l, bool r) GetQuadIntersect(Rect a, Rect b)
        {
            return (
                t: a.y - a.h < b.y || a.y + a.h < b.y,
                b: a.y - a.h > b.y || a.y + a.h > b.y,
                l: a.x - a.w < b.x || a.x + a.w < b.x,
                r: a.x - a.w > b.x || a.x + a.w > b.x
            );
        }
    }
}
