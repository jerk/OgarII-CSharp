using Ogar_CSharp.worlds;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ogar_CSharp.cells
{
    public class PlayerCell : Cell
    {
        public Player owner;
        public int x;
        public int y;
        public int size;
        public int color;
        public bool __canMerge;
        public double MoveSpeed
            => 88 * Math.Pow(size, -0.4396754) * owner.Settings.playerMoveMult;
        public bool CanMerge => __canMerge;
        public override short Type => 0;
        public bool IsSpiked => false;
        public bool IsAgitated => false;
        public bool AvoidWhenSpawning => true;
        public PlayerCell(Player owner, int x, int y, int size) : base(owner.world, x, y, size, owner.cellColor)
        {

        }
        public int GetEatResult(Cell other)
        {
            if(other.Type == 0)
            {
                var delay =
            }
        }
    }
}
