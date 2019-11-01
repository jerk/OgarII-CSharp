using System;
using System.Collections.Generic;
using System.Text;

namespace Ogar_CSharp.primitives
{
    public struct Rect
    {
        public Rect(double x, double y, double w, double h)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
        }
        public double x;
        public double y;
        public double w;
        public double h;
    }
    public struct Point
    {
        public Point(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
        public double x;
        public double y;
    }
    public struct ViewArea
    {
        public ViewArea(double x, double y, double w, double h, double s)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
            this.s = s;
        }
        public double x;
        public double y;
        public double w;
        public double h;
        public double s;
    }
}
