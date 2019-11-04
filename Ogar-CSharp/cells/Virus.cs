using Ogar_CSharp.worlds;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ogar_CSharp.cells
{
    public class Virus : Cell
    {
        public override byte Type => throw new NotImplementedException();

        public override bool IsSpiked => throw new NotImplementedException();

        public override bool IsAgitated => throw new NotImplementedException();

        public override bool AvoidWhenSpawning => throw new NotImplementedException();

        public Virus(World world, int x, int y) : base(world, x, y, world.Settings.virusSize, 0x33FF33)
        {
            
        }
        public CellEatResult GetEjectedEatResult(bool isSelf)
            => world.virusCount >= world.Settings.virusMaxCount ? 
            CellEatResult.None : isSelf ? CellEatResult.Eat : CellEatResult.EatInvd;

        public override CellEatResult GetEatResult(Cell other)
        {
            throw new NotImplementedException();
        }
    }
}
