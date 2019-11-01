using System;
using System.Collections.Generic;
using System.Text;

namespace Ogar_CSharp
{
    public struct Rect
    {
        public Rect(float x, float y, float w, float h)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
        }
        public float x;
        public float y;
        public float w;
        public float h;
    }
    public struct Point
    {
        public Point(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        public float x;
        public float y;
    }
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
    }
}
