using System;
using System.Collections.Generic;
using System.Text;

namespace Ogar_CSharp
{
    public struct ViewArea
    {
        public ViewArea(float x, float y, float w, float h, float s)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
            this.s = s;
        }
        public float x;
        public float y;
        public float w;
        public float h;
        public float s;

        public override bool Equals(object obj)
        {
            ViewArea other = (ViewArea)obj;
            return this.x == other.x && 
                this.y == other.y && 
                this.h == other.h && 
                this.w == other.w && 
                this.s == other.s;
        }

        public override int GetHashCode() => base.GetHashCode();
        public static bool operator ==(ViewArea left, ViewArea right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ViewArea left, ViewArea right)
        {
            return !(left == right);
        }
    }
}
