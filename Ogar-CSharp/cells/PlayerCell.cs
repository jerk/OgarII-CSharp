using Ogar_CSharp.worlds;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ogar_CSharp.cells
{
    public class PlayerCell : Cell
    {
        public double MoveSpeed
            => 88 * Math.Pow(Size, -0.4396754) * owner.Settings.playerMoveMult;
        public bool CanMerge { get; set; }
        public override short Type => 0;
        public override bool IsSpiked => false;
        public override bool IsAgitated => false;
        public override bool AvoidWhenSpawning => true;
        public PlayerCell(Player owner, int x, int y, int size) : base(owner.world, x, y, size, owner.cellColor)
        {

        }
        public override int GetEatResult(Cell other)
        {
            if(other.Type == 0)
            {
                var delay =
            }
        }
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
                delay = Math.Max(delay, (settings.playerMergeVersion == Settings.MergeVersion.New) ? Math.Max(initial, increase) : initial + increase);
            }
            CanMerge = Age >= delay;
        }
    }
}
