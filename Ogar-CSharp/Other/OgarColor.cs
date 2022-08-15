using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Ogar_CSharp.Other
{
    [StructLayout(LayoutKind.Explicit, Size = 3)]
    public struct OgarColor
    {
        public OgarColor(byte r, byte g, byte b)
        {
            this.R = r;
            this.G = g;
            this.B = b;
        }
        [FieldOffset(0)]
        public readonly byte R;
        [FieldOffset(1)]
        public readonly byte G;
        [FieldOffset(2)]
        public readonly byte B;
        public static implicit operator Color(OgarColor color)
        {
            return Color.FromArgb(color.R, color.G, color.B);
        }
        public static implicit operator OgarColor(Color color)
        {
            return new OgarColor(color.R, color.G, color.B);
        }
        public static implicit operator uint(OgarColor agarColor)
        {
            return (agarColor.R) | (uint)(agarColor.G << 8) | ((uint)agarColor.B << 16);
        }
        public static implicit operator OgarColor(uint color)
        {
            return new OgarColor((byte)(color & 255), (byte)((color >> 8) & 255), (byte)(color >> 16));
        }
    }
}
