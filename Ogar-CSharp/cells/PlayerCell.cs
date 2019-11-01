using Ogar_CSharp.worlds;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ogar_CSharp.cells
{
    public class PlayerCell : Cell
    {
        public float MoveSpeed
            => (float)(88 * Math.Pow(Size, -0.4396754) * owner.Settings.playerMoveMult);
        public bool CanMerge { get; set; }
        public override byte Type => 0;
        public override bool IsSpiked => false;
        public override bool IsAgitated => false;
        public override bool AvoidWhenSpawning => true;
        public PlayerCell(Player owner, float x, float y, short size) : base(owner.world, x, y, size, owner.cellColor)
        {

        }
        public override CellEatResult GetEatResult(Cell other)
        {
            if(other.Type == 0)
            {
                PlayerCell otherPlayer = (PlayerCell)other;
                var delay = world.Settings.playerNoCollideDelay;
                if(otherPlayer.owner.id == owner.id)
                {
                    if (otherPlayer.Age < delay || Age < delay)
                        return (CellEatResult)0;
                    if (CanMerge && otherPlayer.CanMerge)
                        return (CellEatResult)2;
                    return (CellEatResult)1;
                }
                if (other.owner.team == owner.team && owner.team != null)
                    return (CellEatResult)((other.Age < delay || Age < delay) ? 0 : 1);
            }
            if (other.Type == 4 && other.Size > Size * Misc.SQRT_1_3)
                return (CellEatResult)3;
            if (other.Type == 1)
                return (CellEatResult)2;
            return GetDefaultEatResult(other);
        }
        public CellEatResult GetDefaultEatResult(Cell other) => (CellEatResult)(other.Size * Misc.SQRT_1_3 > Size ? 0 : 2);
        public override void OnTick()
        {
            base.OnTick();
            if (Name != owner.cellName)
                Name = owner.cellName;
            if (Skin != owner.cellSkin)
                Skin = owner.cellSkin;
            if (Color != owner.cellColor)
                Color = owner.cellColor;
            var settings = world.Settings;
            var delay = settings.playerNoMergeDelay;
            if(settings.playerMergeTime > 0)
            {
                var initial = Math.Round(25 * settings.playerMergeTime);
                var increase = Math.Round(25 * Mass * settings.playerMergeTimeIncrease);
                delay = (float)(Math.Max(delay, (settings.playerMergeVersion == Settings.MergeVersion.New) ? Math.Max(initial, increase) : initial + increase));
            }
            CanMerge = Age >= delay;
        }
    }
}
