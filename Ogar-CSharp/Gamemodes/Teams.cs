using System;
using System.Collections.Generic;
using System.Text;
using Ogar_CSharp.Sockets;
using Ogar_CSharp.Worlds;
using System.Drawing;
namespace Ogar_CSharp.Gamemodes
{ 
    public class Teams : Gamemode
    {
        public Teams(ServerHandle handle) : base(handle){ }
        private const byte highlightBase = 231,
            lowlightBase = 23,
            highlightDiff = 24,
            lowlightDiff = 24;
        private static readonly int[] teamColorsInt = new int[]
        {
            0xFF0000,
            0x00FF00,
            0x0000FF
        };
        private static readonly Color[] teamColors = new Color[]
        {
            Color.FromArgb(highlightBase, lowlightBase, lowlightBase),
            Color.FromArgb(lowlightBase, highlightBase, lowlightBase),
            Color.FromArgb(lowlightBase, lowlightBase, highlightBase)
        };
        private static int TeamCount => teamColors.Length;
        private static int GetTeamColor(int index)
        {
            Color color = teamColors[index];
            float random = (float)new Random().NextDouble();
            int highlight = (int)(highlightBase + (float)Math.Floor(random * highlightDiff));
            int lowlight = (int)(lowlightBase - (float)Math.Floor(random * lowlightDiff));
            var r = color.R == highlightBase ? highlight : lowlight;
            var g = color.G == highlightBase ? highlight : lowlight;
            var b = color.B == highlightBase ? highlight : lowlight;
            return (r << 16) | (g << 8) | b;
        }
        public override byte Type => 2;

        public override string Name => "Teams";
        public override void OnNewWorld(World world)
        {
            for (int i = 0; i < TeamCount; i++)
                world.teams.Add(new List<Player>());
        }
        public override void OnPlayerJoinWorld(Player player, World world)
        {
            if (!player.router.SeparateInTeams)
                return;
            int team = 0;
            for (int i = 0; i < TeamCount; i++)
                team = (world.teams[i].Count < world.teams[team].Count) ? i : team;
            world.teams[team].Add(player);
            player.team = team;
            player.chatColor = GetTeamColor(player.team.Value);
        }
        public override void OnPlayerLeaveWorld(Player player, World world)
        {
            if (!player.router.SeparateInTeams)
                return;
            world.teams[player.team.Value].Remove(player);
            player.team = null;
        }

        public override void CompileLeaderboard(World world)
        {
            var teams = world.teamsLeaderboard = new List<Other.PieLeaderboardEntry>();
            for (int i = 0; i < TeamCount; i++)
            {
                teams.Add(new Other.PieLeaderboardEntry(0, teamColorsInt[i]));
            }
            float sum = 0;
            for (int i = 0; i < world.playerCells.Count; i++)
            {
                var cell = world.playerCells[i];
                if (cell.owner.team == null)
                    continue;
                teams[cell.owner.team.Value].weight += cell.SquareSize;
                sum += cell.SquareSize;
            }
            for (int i = 0; i < TeamCount; i++)
                teams[i].weight /= sum;
        }

        public override void OnPlayerSpawnRequest(Player player, string name, string skin)
        {
            if (player.currentState == PlayerState.Alive || !player.hasWorld)
                return;
            float size = player.router.Type == "minion" ? handle.Settings.minionSpawnSize : handle.Settings.playerSpawnSize;
            var pos = player.world.GetSafeSpawnPos(size);
            var color = player.router.SeparateInTeams ? GetTeamColor(player.team.Value) : Misc.RandomColor();
            player.cellName = player.chatName = player.leaderBoardName = name;
            player.cellSkin = null;
            player.chatColor = player.cellColor = color;
            player.world.SpawnPlayer(player, pos, size);
        }

        public override void SendLeaderboard(Connection connection)
        {
            connection.protocol.OnLeaderboardUpdate(Other.LeaderboardType.Pie, connection.Player.world.teamsLeaderboard, null);
        }
    }
}
