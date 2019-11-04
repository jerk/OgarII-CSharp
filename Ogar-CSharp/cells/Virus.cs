using Ogar_CSharp.worlds;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ogar_CSharp.cells
{
    public class Virus : Cell
    {
        public float splitAngle = float.NaN;
        public int fedTimes;
        public Virus(World world, float x, float y) : base(world, x, y, world.Settings.virusSize, 0x33FF33)
        {

        }
        public override byte Type => 2;

        public override bool IsSpiked => true;

        public override bool IsAgitated => false;

        public override bool AvoidWhenSpawning => true;

        public Virus(World world, int x, int y) : base(world, x, y, world.Settings.virusSize, 0x33FF33)
        {
            
        }
        public CellEatResult GetEjectedEatResult(bool isSelf)
            => world.virusCount >= world.Settings.virusMaxCount ? 
            CellEatResult.None : isSelf ? CellEatResult.Eat : CellEatResult.EatInvd;

        public override CellEatResult GetEatResult(Cell other)
        {
            if (other.Type == 3)
                return GetEjectedEatResult(true);
            if (other.Type == 4)
                return CellEatResult.EatInvd;
            return CellEatResult.None;
        }
        public override void WhenAte(Cell cell)
        {
            var settings = this.world.Settings;
            if (settings.virusPushing)
            {
                var newD = this.boost.d + settings.virusPushBoost;
                this.boost.dx = (this.boost.dx * this.boost.d + cell.boost.dx * settings.virusPushBoost) / newD;
                this.boost.dy = (this.boost.dy * this.boost.d + cell.boost.dy * settings.virusPushBoost) / newD;
                this.boost.d = newD;
                this.world.SetCellAsBoosting(this);
            }
            else
            {
                splitAngle = (float)Math.Atan2(cell.boost.dx, cell.boost.dy);
                if (++this.fedTimes >= settings.virusFeedTimes)
                {
                    this.fedTimes = 0;
                    Size = settings.virusSize;
                    this.world.SplitVirus(this);
                }
                else base.WhenAte(cell);
            }
        }
        public override void WhenEatenBy(Cell other)
        {
            base.WhenEatenBy(other);
            if (other.Type == 0)
                world.PopPlayerCell((PlayerCell)other);
        }
        public override void OnRemoved()
        {
            world.virusCount--;
        }
        public override void OnSpawned()
        {
            world.virusCount++;
        }
    }
}
