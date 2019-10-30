using Spatial;
using System;
using System.Collections.Generic;
using System.Text;
namespace Ogar_CSharp.primitives
{
    public static class Misc
    {
        public const string version = "1.3.5";
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
        public static bool Intersects(Rect2 a, Rect2 b)
        {
            return a.X - a.Width <= b.X + b.Width &&
                a.X + a.Width >= b.X - b.Width &&
                a.Y - a.Height <= b.Y + b.Height &&
                a.Y + a.Height >= b.Y - b.Height;
        }
        public static bool FunnyIntersects(Rect2 a, Rect2 b)
        {
            return a.X - a.Width >= b.X + b.Width &&
               a.X + a.Width <= b.X - b.Width &&
               a.Y - a.Height >= b.Y + b.Height &&
               a.Y + a.Height <= b.Y - b.Height;
        }
        public static (bool t, bool b, bool l, bool r) GetQuadFullIntersect(Rect2 a, Rect2 b) 
        {
            return (
            t: a.Y - a.Height < b.Y && a.Y + a.Height < b.Y,
            b: a.Y - a.Height > b.Y && a.Y + a.Height > b.Y,
            l: a.X- a.Width < b.X && a.X + a.Width < b.X,
            r: a.X - a.Width > b.X && a.X + a.Width > b.X
            );
        }
        public static (bool t, bool b, bool l, bool r) GetQuadIntersect(Rect2 a, Rect2 b)
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
