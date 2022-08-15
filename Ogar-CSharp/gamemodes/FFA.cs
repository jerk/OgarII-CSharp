﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Ogar_CSharp.Sockets;
using Ogar_CSharp.Worlds;
using Ogar_CSharp.Other;

namespace Ogar_CSharp.Gamemodes
{
    public class FFA : Gamemode
    {
        public static FFALeaderboardEntry GetLeaderboardData(Player player, Player requesting, short index)
        {
            return new FFALeaderboardEntry(player.leaderBoardName, requesting.Id == player.Id, player.ownedCells[0].Id, (short)(1 + index));
        }
        public FFA(ServerHandle handle) : base(handle)
        {

        }
        public override byte Type => 0;
        public override string Name => "FFA";

        public override void OnPlayerSpawnRequest(Player player, string name, string skin)
        {
            if (player.currentState == PlayerState.Alive || !player.hasWorld)
                return;
            int size = ((player.router.Type == "minion") ? handle.Settings.minionSpawnSize : handle.Settings.playerSpawnSize);
            var spawnInfo = player.world.GetPlayerSpawn(size);
            OgarColor color = spawnInfo.color ?? Misc.RandomColor();
            player.cellName = player.chatName = player.leaderBoardName = name ?? "";
            player.cellSkin = skin ?? "";
            player.chatColor = player.cellColor = color;
            player.world.SpawnPlayer(player, spawnInfo.pos, size); // has two more arguments using 'name' and 'null'
        }
        public override void CompileLeaderboard(World world)
        {
            world.leaderboard.Clear();
            world.leaderboard.AddRange(world.players.Where(x => !float.IsNaN(x.score)).OrderByDescending(x => x.score).Take(10));
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
            var data = leaderboard.Select((x) => GetLeaderboardData(x, player, index++));
            var selfData = data.FirstOrDefault(x => x.highlighted);
            connection.protocol.OnLeaderboardUpdate(LeaderboardType.FFA, data.ToList(), selfData);
        }
    }
}
