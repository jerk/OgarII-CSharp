using Ogar_CSharp.worlds;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ogar_CSharp.cells
{
    public class MotherCell : Cell, ISpawner
    {
        public int activePelletFormQueue;
        public int passivePelletFormQueue;
        public MotherCell(World world, float x, float y) : base(world, x, y, world.Settings.motherCellSize, 0xCE6363)
        {

        }
        public override byte Type => 4;

        public override bool IsSpiked => true;

        public override bool IsAgitated => false;

        public override bool AvoidWhenSpawning => true;

        public int PelletCount { get; set; }

        public override CellEatResult GetEatResult(Cell other)
        {
            return CellEatResult.None;
        }
        public override void OnTick()
        {
            base.OnTick();
            var settings = world.Settings;
            var mothercellSize = settings.motherCellSize;
            var pelletSize = settings.pelletMinSize;
            var minSpawnSqSize = mothercellSize * mothercellSize + pelletSize * pelletSize;
            var mathRandom = new Random();
            this.activePelletFormQueue += (int)(settings.motherCellActiveSpawnSpeed * this.world.handle.stepMult);
            this.passivePelletFormQueue += (int)((float)mathRandom.NextDouble() * settings.motherCellPassiveSpawnChance * this.world.handle.stepMult);

            while (this.activePelletFormQueue > 0)
            {
                if (SquareSize > minSpawnSqSize) {
                    SpawnPellet(); 
                    SquareSize -= pelletSize * pelletSize; 
                }
                else if (Size > mothercellSize)
                    Size = mothercellSize;
                this.activePelletFormQueue--;
            }
            while (this.passivePelletFormQueue > 0)
            {
                if (PelletCount < settings.motherCellMaxPellets)
                    SpawnPellet();
                this.passivePelletFormQueue--;
            }
        }
        public void SpawnPellet()
        {
            var random = new Random();
            var angle = (float)(random.NextDouble() * 2 * Math.PI);
            var x = X + Size * (float)Math.Sin(angle);
            var y = Y + Size * (float)Math.Cos(angle);
            var pellet = new Pellet(this.world, this, x, y);
            pellet.boost.dx = (float)Math.Sin(angle);
            pellet.boost.dy = (float)Math.Cos(angle);
            var d = this.world.Settings.motherCellPelletBoost;
            pellet.boost.d = d / 2 + (float)random.NextDouble() * d / 2;
            this.world.AddCell(pellet);
            this.world.SetCellAsBoosting(pellet);
        }
        public override void WhenAte(Cell other)
        {
            base.WhenAte(other);
            Size = Math.Min(Size, world.Settings.motherCellMaxSize);
        }
        public override void WhenEatenBy(Cell other)
        {
            base.WhenEatenBy(other);
            if(other.Type == 0)
            {
                world.PopPlayerCell((PlayerCell)other);
            }
        }
        public override void OnSpawned()
        {
            world.motherCellCount++;
        }
        public override void OnRemoved()
        {
            world.motherCellCount--;
        }
    }
}
