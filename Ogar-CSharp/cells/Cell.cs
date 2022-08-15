using Ogar_CSharp.Other;
using Ogar_CSharp.Worlds;
using RBush;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
namespace Ogar_CSharp.Cells
{
    public enum CellEatResult
    {
        None,
        Rigid,
        Eat,
        EatInvd
    }
    public abstract class Cell : IQuadItem
    {
        public uint Id { get; }
        public RectangleF range;
        public Boost boost = default;
        public World world;
        public int birthTick;
        public bool exists;
        public Cell eatenBy;
        public bool isBoosting;
        public Player owner;
        private float x = float.NaN;
        private float y = float.NaN;
        private float size = float.NaN;
        private OgarColor color;
        private string name;
        private string skin;
        public bool posChanged, sizeChanged, colorChanged, nameChanged, skinChanged;
        public RectangleF Range => range;
        public abstract byte Type { get; }
        public abstract bool IsSpiked { get; }
        public abstract bool IsAgitated { get; }
        public abstract bool AvoidWhenSpawning { get; }
        public virtual bool ShouldUpdate { get => posChanged || sizeChanged || colorChanged || nameChanged || skinChanged; }
        public float Age => (world.handle.tick - birthTick) * world.handle.stepMult;
        public float X { get => x; set { x = value; posChanged = true; } }
        public float Y { get => y; set { y = value; posChanged = true; } }
        public float Size { get => size; set { Misc.ThrowIfBadOrNegativeNumber(value); size = value; sizeChanged = true; } }
        public float SquareSize { get => Size * Size; set => Size = MathF.Sqrt(value); }
        public float Mass { get => Size * Size / 100; set => Size = MathF.Sqrt(100 * value); }
        public OgarColor Color { get => color; set { color = value; colorChanged = true; } }
        public string Name { get => name; set { name = value; nameChanged = true; } }
        public string Skin { get => skin; set { skin = value; skinChanged = true; } }
        protected Cell(World world, float x, float y, float size, OgarColor color)
        {
            this.world = world;
            Id = world._nextCellId++;
            birthTick = world.handle.tick;
            X = x;
            Y = y;
            Size = size;
            Color = color;
            Name = string.Empty;
            Skin = string.Empty;
        }
        public abstract CellEatResult GetEatResult(Cell other);
        public virtual void OnSpawned() { }
        public virtual void OnTick() 
        {
            posChanged = sizeChanged = colorChanged = nameChanged = skinChanged = false;
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
