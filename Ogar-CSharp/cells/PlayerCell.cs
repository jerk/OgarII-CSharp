using Ogar_CSharp.Worlds;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ogar_CSharp.Cells
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
        public PlayerCell(Player owner, float x, float y, float size) : base(owner.world, x, y, size, owner.cellColor)
        {
            this.owner = owner;
            Name = owner.cellName ?? "";
            Skin = owner.cellName ?? "";
            CanMerge = false;
        }
        public override void OnRemoved()
        {
            this.world.playerCells.Remove(this);
            this.owner.ownedCells.Remove(this);
            owner.UpdateState(PlayerState.Idle);
        }
        public override void OnSpawned()
        {
            this.owner.router.OnNewOwnedCell(this);
            this.owner.ownedCells.Add(this);
            this.world.playerCells.Insert(0, this);
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
                        return CellEatResult.None;
                    if (CanMerge && otherPlayer.CanMerge)
                        return CellEatResult.Eat;
                    return CellEatResult.Rigid;
                }
                if (other.owner.team == owner.team && owner.team != null)
                    return ((other.Age < delay || Age < delay) ? CellEatResult.None : CellEatResult.Rigid);
            }
            if (other.Type == 4 && other.Size > Size * Misc.SQRT_1_3)
                return CellEatResult.EatInvd;
            if (other.Type == 1)
                return CellEatResult.Eat;
            return GetDefaultEatResult(other);
        }
        public CellEatResult GetDefaultEatResult(Cell other) => (other.Size * Misc.SQRT_1_3 > Size ? CellEatResult.None : CellEatResult.Eat);
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
