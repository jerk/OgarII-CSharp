using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Ogar_CSharp.sockets;
using Ogar_CSharp.worlds;
using Ogar_CSharp.Other;

namespace Ogar_CSharp.gamemodes
{

    public class FFA : GameMode
    {
        public static FFALeaderboardEntry GetLeaderboardData(Player player, Player requesting, short index)
        {
            return new FFALeaderboardEntry(player.leaderBoardName, requesting.id == player.id, player.ownedCells[0].id, (short)(1 + index));
        }
        public FFA(ServerHandle handle) : base(handle)
        {

        }
        public override byte Type => 0;
        public override string Name => "FFA";

        public override void OnPlayerSpawnRequest(Player player, string name, string skin)
        {
            if (player.state == 0 || !player.hasWorld)
                return;
            int size = ((player.router.Type == "minion") ? handle.Settings.minionSpawnSize : handle.Settings.playerSpawnSize);
            var spawnInfo = player.world.GetPlayerSpawn(size);
            int color = spawnInfo.color ?? Misc.RandomColor();
            player.cellName = player.chatName = player.leaderBoardName = name ?? "";
            player.cellSkin = skin ?? "";
            player.chatColor = player.cellColor = color;
            player.world.SpawnPlayer(player, new Point(spawnInfo.x, spawnInfo.y), size); // has two more arguments using 'name' and 'null'
        }
        public override void CompileLeaderboard(World world)
        {
            world.leaderboard = world.players.Where(x => !float.IsNaN(x.score)).OrderBy(x => x.score).ToList();
        }
        public override void SendLeaderboard(Connection connection)
        {
            if (!connection.hasPlayer)
                return;
            var player = connection.Player;
            if (!player.hasWorld)
                return;
            if (player.world.frozen)
                return;
            var leaderboard = player.world.leaderboard;
            short index = 0;
            var data = leaderboard.Select((x) => GetLeaderboardData(x, player, index++)).Cast<LeaderBoardEntry>().ToList();
            var selfData = float.IsNaN(player.score) ? null : data[leaderboard.IndexOf(player)];
            connection.protocol.OnLeaderboardUpdate(LeaderboardType.FFA, data, selfData);
        }
    }
}
