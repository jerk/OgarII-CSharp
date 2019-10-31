using Ogar_CSharp.worlds;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ogar_CSharp.cells
{
    public abstract class Cell
    {
        public abstract short Type { get; }
        protected Cell(World world, int x, int y, int size, int color)
        {

        }
    }
}
