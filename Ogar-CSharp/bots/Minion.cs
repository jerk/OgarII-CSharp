﻿using Ogar_CSharp.Sockets;
using Ogar_CSharp.Worlds;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ogar_CSharp.Bots
{
    public class Minion : Bot
    {
        public Connection following;
        public Minion(Connection following) : base(following.Player.world)
        {
            this.following = following;
            following.Minions.Add(this);
        }
        public override string Type => "minion";
        public override bool SeparateInTeams => false;
        public override void Close()
        {
            base.Close();
            following.Minions.Remove(this);
        }
        public override void Tick()
        {
            if (Player.currentState == PlayerState.Idle && following.Player.currentState == PlayerState.Alive)
            {
                spawningName = ((listener.Settings.minionName == "*") ? $"*{following.Player.leaderBoardName}" : listener.Settings.minionName);
                OnSpawnRequest();
                spawningName = null;
            }
            mouseX = following.minionsFrozen ? Player.viewArea.x : following.mouseX;
            mouseY = following.minionsFrozen ? Player.viewArea.y : following.mouseY;
        }
        public override bool ShouldClose
        {
            get
            {
                bool isf = !hasPlayer
                || !Player.exists
                || !Player.hasWorld || following.socketDisconnected
                || following.disconnected || !following.hasPlayer || !following.Player.exists
                || (following.Player.world != Player.world);
                Console.WriteLine(isf);
                return isf;
            }
        }
    }
}
