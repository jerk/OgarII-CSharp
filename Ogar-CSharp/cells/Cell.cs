using Ogar_CSharp.primitives;
using Ogar_CSharp.worlds;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ogar_CSharp.cells
{
    public enum CellEatResult
    {
        None,
        Rigid,
        Eat,
        EatInvd
    }
    public abstract class Cell
    {
        public int id;
        public World world;
        public int birthTick;
        public bool exists;
        public Cell eatenBy;
        public Rect range;
        public bool isBoosting;
        //public Boost boost;
        public Player owner;
        private double x;
        private double y;
        private double size;
        private int color;
        private string name;
        private string skin;
        public bool posChanged, sizeChanged, colorChanged, nameChanged, skinChanged;
        public abstract short Type { get; }
        public abstract bool IsSpiked { get; }
        public abstract bool IsAgitated { get; }
        public abstract bool AvoidWhenSpawning { get; }
        public virtual bool ShouldUpdate { get => posChanged || sizeChanged || colorChanged || nameChanged || skinChanged; }
        public int Age => (world.handle.tick - birthTick) * world.handle.stepMult;
        public double X { get => x; set { x = value; posChanged = true; } }
        public double Y { get => y; set { y = value; posChanged = true; } }
        public double Size { get => size; set { Misc.ThrowIfBadOrNegativeNumber(value); size = value; sizeChanged = true; } }
        public double SquareSize { get => size * size; set => Size = Math.Sqrt(100 * value); }
        public double Mass { get => size * size / 100; set => Size = Math.Sqrt(100 * value); }
        public int Color { get => color; set { color = value; colorChanged = true; } }
        public string Name { get => name; set { name = value; nameChanged = true; } }
        public string Skin { get => skin; set { skin = value; skinChanged = true; } }
        protected Cell(World world, int x, int y, int size, int color)
        {
            this.world = world;
            id = world._nextCellId;
            birthTick = world.handle.tick;
            this.x = x;
            this.y = y;
            this.size = size;
            this.color = color;
        }
        public abstract CellEatResult GetEatResult(Cell other);
        public virtual void OnSpawned() { }
        public virtual void OnTick() 
        {
            posChanged = sizeChanged = colorChanged = nameChanged = skinChanged = true;
        }
        public virtual void WhenAte(Cell other)
        {
            SquareSize += other.SquareSize;
        }
        public virtual void WhenEatenBy(Cell other)
        {
            eatenBy = other;
        }
        public virtual void OnRemoved() { }
    }
}
