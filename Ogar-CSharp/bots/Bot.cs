using Ogar_CSharp.sockets;
using Ogar_CSharp.worlds;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ogar_CSharp.bots
{
    public abstract class Bot : Router
    {
        public Bot(World world) : base(world.handle.listener)
        {
            CreatePlayer();
            world.AddPlayer(Player);
        }
        public override bool IsExternal => false;
        public override void Close()
        {
            base.Close();
            listener.handle.RemovePlayer(Player.id);
            disconnected = true;
            disconnectionTick = listener.handle.tick;
        }
    }
}
