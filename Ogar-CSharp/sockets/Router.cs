using Ogar_CSharp.cells;
using Ogar_CSharp.worlds;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ogar_CSharp.sockets
{
    public abstract class Router
    {
        public Listener listener;
        public bool disconnected;
        public float disconnectionTick;
        public float mouseX;
        public float mouseY;
        public string spawningName;
        public bool requestingSpectate;
        public bool isPressingQ;
        public bool hasProcessedQ;
        public int splitAttempts;
        public byte ejectAttempts;
        public float ejectTick;
        public bool hasPlayer = false;
        private Player player;
        private object locker = new object();
        public Player Player { 
            get 
            {
                lock (locker)
                    return player;
            } 
            set => player = value; }
        protected Router(Listener listener)
        {
            this.listener = listener;
            ejectTick = listener.handle.tick;
            listener.AddRouter(this);
        }
        public virtual bool IsExternal => throw new Exception("Must be overriden"); 
        public virtual string Type => throw new Exception("Must be overriden");
        public virtual bool SeparateInTeams => throw new Exception("Must be overriden");
        public ServerHandle Handle => listener.handle;
        public Settings Settings => listener.Settings;
        public virtual void CreatePlayer()
        {
            if (hasPlayer)
                return;
            Console.WriteLine("creating player");
            player = listener.handle.CreatePlayer(this);
            hasPlayer = true;
            Console.WriteLine("is player null ? :" + (player == null));
        }
        public void DestroyPlayer()
        {
            if (!hasPlayer)
                return;
            listener.handle.RemovePlayer(player.id);
            player = null;
        }
        public virtual void OnWorldSet() { }
        public virtual void OnWorldReset() { }
        public virtual void OnNewOwnedCell(PlayerCell cell) { }
        public virtual void OnSpawnRequest()
        {
            if (!hasPlayer)
                return;
            string name = spawningName.Substring(0, Settings.playerMaxNameLength);
            string skin = null;
            listener.handle.gamemode.OnPlayerSpawnRequest(player, name, skin);
        }
        public virtual void OnSpectateRequest()
        {
            if (!hasPlayer)
                return;
            player.UpdateState(PlayerState.Spectating);
        }
        public virtual void OnQPress()
        {
            if (!hasPlayer)
                return;
            listener.handle.gamemode.WhenPlayerPressQ(player);
        }
        public virtual void AttemptSplit()
        {
            if (!hasPlayer)
                return;
            listener.handle.gamemode.WhenPlayerSplit(player);
        }
        public virtual void AttemptEject()
        {
            if (!hasPlayer)
                return;
            listener.handle.gamemode.WhenPlayerEject(player);
        }
        public virtual void Close() => listener.RemoveRouter(this);
        public abstract bool ShouldClose { get; }
        public abstract void Update();
    }
}
