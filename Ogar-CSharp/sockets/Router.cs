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
        public Player Player { get; set; }
        protected Router(Listener listener)
        {
            this.listener = listener;
            ejectTick = listener.handle.tick;
            listener.AddRouter(this);
        }
        public abstract bool IsExternal { get; }
        public abstract string Type { get; }
        public abstract bool SeparateInTeams { get; }
        public ServerHandle Handle => listener.handle;
        public Settings Settings => listener.Settings;
        public virtual void CreatePlayer()
        {
            if (hasPlayer)
                return;
            Player = listener.handle.CreatePlayer(this);
            hasPlayer = true;
        }
        public void DestroyPlayer()
        {
            if (!hasPlayer)
                return;
            listener.handle.RemovePlayer(Player.id);
            Player = null;
        }
        public virtual void OnWorldSet() { }
        public virtual void OnWorldReset() { }
        public virtual void OnNewOwnedCell(PlayerCell cell) { }
        public virtual void OnSpawnRequest()
        {
            if (!hasPlayer)
                return;
            string name;
            if (spawningName.Length > Settings.playerMaxNameLength)
                name = spawningName.Substring(0, Settings.playerMaxNameLength);
            else
                name = spawningName;
            string skin = null;
            listener.handle.gamemode.OnPlayerSpawnRequest(Player, name, skin);
        }
        public virtual void OnSpectateRequest()
        {
            if (hasPlayer)
                Player.UpdateState(PlayerState.Spectating);
        }
        public virtual void OnQPress()
        {
            if (hasPlayer)
                listener.handle.gamemode.WhenPlayerPressQ(Player);
        }
        public virtual void AttemptSplit()
        {
            if (hasPlayer)
                listener.handle.gamemode.WhenPlayerSplit(Player);
        }
        public virtual void AttemptEject()
        {
            if (hasPlayer)
                listener.handle.gamemode.WhenPlayerEject(Player);
        }
        public virtual void Close() => listener.RemoveRouter(this);
        public abstract bool ShouldClose { get; }
        public abstract void Update();
    }
}
