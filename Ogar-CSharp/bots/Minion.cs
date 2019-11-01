using Ogar_CSharp.sockets;
using Ogar_CSharp.worlds;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ogar_CSharp.bots
{
    public class Minion : Bot
    {
        public Connection following;
        public Minion(Connection following) : base(following.player.world)
        {
            this.following = following;
            following.minions.Add(this);
        }
        public override string Type => "minion";
        public override bool SeparateInTeams => false;
        public override void Close()
        {
            base.Close();
            following.minions.Remove(this);
        }
        public override void Update()
        {
            if (player.state == PlayerState.Idle && following.player.state == PlayerState.Alive)
            {
                spawningName = ((listener.Settings.minionName == "*") ? $"*{following.player.leaderBoardName}" : listener.Settings.minionName);
                OnSpawnRequest();
                spawningName = null;
            }
            mouseX = following.minionsFrozen ? player.viewArea.x : following.mouseX;
            mouseY = following.minionsFrozen ? player.viewArea.y : following.mouseY;
        }
        public override bool ShouldClose => !hasPlayer 
            || !player.exists 
            || !player.hasWorld || following.socketDisconnected 
            || following.disconnected || !following.hasPlayer || !following.player.exists 
            || following.player.world != player.world;
    }
}
