using Ogar_CSharp.Worlds;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ogar_CSharp.Cells
{
    public class EjectedCell : Cell
    {
        public EjectedCell(World world, Player owner, float x, float y, int color) : base(world, x, y, world.Settings.ejectedSize, color)
        {
            this.owner = owner;
        }
        public override byte Type => 3;

        public override bool IsSpiked => false;

        public override bool IsAgitated => false;

        public override bool AvoidWhenSpawning => false;

        public override CellEatResult GetEatResult(Cell other)
        {
            if (other.Type == 2) 
                return ((Virus)other).GetEjectedEatResult(false);
            if (other.Type == 4) 
                return CellEatResult.EatInvd;
            if (other.Type == 3)
            {
                if (!other.isBoosting) other.world.SetCellAsBoosting(other);
                return CellEatResult.Rigid;
            }
            return CellEatResult.None;
        }
        public override void OnRemoved()
        {
            world.ejectedCells.Remove(this);
        }
        public override void OnSpawned()
        {
            world.ejectedCells.Add(this);
        }
    }
}
