using Ogar_CSharp.worlds;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ogar_CSharp.cells
{
    public class Pellet : Cell
    {
        public ISpawner spawner;
        public Pellet(World world, ISpawner spawner, float x, float y) : base(world, x, y, world.Settings.pelletMinSize, Misc.RandomColor())
        {

            this.spawner = spawner;
            lastGrowTick = birthTick;
        }
        public override byte Type => 1;

        public override bool IsSpiked => false;

        public override bool IsAgitated => false;

        public override bool AvoidWhenSpawning => false;

        public override CellEatResult GetEatResult(Cell other) => CellEatResult.None;
        public int lastGrowTick;
        public override void OnTick()
        {
            base.OnTick();
            if (Size >= world.Settings.pelletMaxSize)
                return;
            if(world.handle.tick - lastGrowTick > world.Settings.pelletGrowTicks / world.handle.stepMult)
            {
                lastGrowTick = world.handle.tick;
                Mass++;
            }
        }
        public override void OnRemoved()
        {
            spawner.PelletCount--;
        }
        public override void OnSpawned()
        {
            spawner.PelletCount++;
        }
    }
}
