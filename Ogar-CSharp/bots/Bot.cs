using Ogar_CSharp.Sockets;
using Ogar_CSharp.Worlds;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ogar_CSharp.Bots
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
            listener.handle.RemovePlayer(Player.Id);
            disconnected = true;
            disconnectionTick = listener.handle.tick;
        }
    }
}
