using Ogar_CSharp.Cells;
using Ogar_CSharp.Worlds;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ogar_CSharp.Bots
{
    public class PlayerBot : Bot
    {
        public int splitCooldownTicks;
        public Cell target;
        public override string Type => "playerbot";
        public override bool SeparateInTeams => true;
        public override bool ShouldClose => !hasPlayer
            || !Player.exists || !Player.hasWorld;
        public PlayerBot(World world) : base(world) { }
        public override void Tick()
        {
            if (splitCooldownTicks > 0)
                splitCooldownTicks--;
            else
                target = null;
            Player.UpdateVisibleCells();
            if (Player.currentState == PlayerState.Idle)
            {
                var names = listener.Settings.worldPlayerBotNames;
                var skins = listener.Settings.worldPlayerBotSkins;
                var random = new Random();
                if (names.Count > 0)
                    spawningName = names[new Random().Next(0, names.Count - 1)] ?? "Player bot";
                spawningName = "Player bot";
                if (spawningName.Contains("<*>"))
                {
                    spawningName = spawningName.Replace("<*>", $"<{skins[~~(int)Math.Round(random.NextDouble() * skins.Count)]}>");
                }
                OnSpawnRequest();
                spawningName = null;
            }
            PlayerCell cell = null;
            for (int i = 0, l = Player.ownedCells.Count; i < l; i++)
                if (cell == null || Player.ownedCells[i].Size > cell.Size)
                    cell = Player.ownedCells[i];
            if (cell == null)
                return;
            if (target != null)
            {
                if (target.exists || !CanEat(cell.Size, target.Size))
                    target = null;
                else
                {
                    this.mouseX = target.X;
                    this.mouseY = target.Y;
                    return;
                }
            }
            bool atMaxCells = Player.ownedCells.Count >= listener.Settings.playerMaxCells;
            bool willingToSplit = Player.ownedCells.Count <= 2;
            int cellCount = Player.visibleCells.Count;
            float mouseX = 0;
            float mouseY = 0;
            Cell bestPrey = null;
            bool splitkillObstacleNearby = false;
            foreach (var item in Player.visibleCells)
            {
                var check = item.Value;
                float truncatedInfluence = MathF.Log10(cell.SquareSize);
                float dx = check.X - cell.X;
                float dy = check.Y - cell.Y;
                float dSplit = MathF.Max(1, MathF.Sqrt(dx * dx + dy * dy));
                float d = MathF.Max(1, dSplit - cell.Size - check.Size);
                float influence = 0;
                switch (check.Type)
                {
                    case 0:
                        if (Player.Id == check.owner.Id)
                            break;
                        if (Player.team != null && Player.team == check.owner.team)
                            break;
                        if (CanEat(cell.Size, check.Size))
                        {
                            influence = truncatedInfluence;
                            if (CanSplitKill(cell.Size, check.Size, dSplit))
                                break;
                            if (bestPrey == null || check.Size > bestPrey.Size)
                                bestPrey = check;
                        }
                        else
                        {
                            influence = (CanEat(check.Size, cell.Size) ? -truncatedInfluence * cellCount : -1);
                            splitkillObstacleNearby = true;
                        }
                        break;
                    case 1:
                        influence = 1;
                        break;
                    case 2:
                        if (atMaxCells)
                            influence = truncatedInfluence;
                        else if (CanEat(cell.Size, check.Size))
                        {
                            influence = -1 * cellCount;
                            if (CanSplitKill(cell.Size, check.Size, dSplit))
                                splitkillObstacleNearby = true;
                        }
                        break;
                    case 3:
                        if (CanEat(cell.Size, check.Size))
                            influence = truncatedInfluence * cellCount;
                        break;
                    case 4:
                        if (CanEat(check.Size, cell.Size))
                            influence = -1;
                        else if (CanEat(cell.Size, check.Size))
                            if (atMaxCells)
                                influence = truncatedInfluence * cellCount;
                            else
                                influence = -1;
                        break;
                }
                if (influence == 0)
                    continue;
                if (d == 0)
                    d = 1;
                dx /= d;
                dy /= d;
                mouseX += dx * influence / d;
                mouseY += dy * influence / d;
            }
            if(willingToSplit && !splitkillObstacleNearby && splitCooldownTicks <= 0 &&
                bestPrey != null && bestPrey.Size * 2 > cell.Size)
            {
                this.target = bestPrey;
                this.mouseX = bestPrey.X;
                this.mouseY = bestPrey.Y;
                this.splitAttempts++;
                this.splitCooldownTicks = 25;
            }
            else
            {
                var d = MathF.Max(1, MathF.Sqrt(mouseX * mouseX + mouseY * mouseY));
                this.mouseX = cell.X + mouseX / d * Player.viewArea.w;
                this.mouseY = cell.Y + mouseY / d * Player.viewArea.h;
            }
        }
        public bool CanEat(double aSize, double bSize)
            => aSize > bSize * Handle.Settings.worldEatMult;
        public bool CanSplitKill(double aSize, double bSize, double d)
        {
            var splitDistance = Math.Max(2 * aSize / Handle.Settings.playerSplitSizeDiv / 2, listener.Settings.playerSplitBoost);
            return aSize / Handle.Settings.playerSplitSizeDiv > bSize * Handle.Settings.worldEatMult && d - splitDistance <= aSize - bSize / 2;
        }
    }
}
