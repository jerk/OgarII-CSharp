﻿using Ogar_CSharp.cells;
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
        public int mouseX;
        public int mouseY;
        public string spawningName;
        public bool requestingSpectate;
        public bool isPressingQ;
        public bool hasProcessedQ;
        public long splitAttempts;
        public long ejectAttempts;
        public float ejectTick;
        public bool hasPlayer = false;
        public Player player;
        protected Router(Listener listener)
        {
            this.listener = listener;
            ejectTick = listener.handle.tick;
            listener.AddRouter(this);
        }
        public ServerHandle Handle
            => listener.handle;
        public Settings Settings
            => listener.Settings;
        public void CreatePlayer()
        {
            if (hasPlayer)
                return;
            hasPlayer = true;
            player = listener.handle.CreatePlayer(this);
        }
        public void DestroyPlayer()
        {
            if (!hasPlayer)
                return;
            listener.handle.RemovePlayer(player.id);
            player = null;
        }
        public abstract void OnWorldSet();
        public abstract void OnWorldReset();
        public abstract void OnNewOwnedCell(PlayerCell cell);
        public virtual void OnSpawnRequest()
        {
            if (!hasPlayer)
                return;
            string name = spawningName.Substring(0, Settings.playerMaxNameLength);
        }
        public virtual void OnSpectateRequest()
        {
            if (!hasPlayer)
                return;
            player.UpdateState(Player.PlayerState.Spectating);
        }
        public virtual void OnQPress()
        {
            if (!hasPlayer)
                return;
            //doStuff
        }
        public virtual void AttemptSplit()
        {
            if (!hasPlayer)
                return;
            //dostuff
        }
        public virtual void AttemptEject()
        {
            if (!hasPlayer)
                return;
            //doStuff
        }
        public virtual void Close() => listener.RemoveRouter(this);
        public abstract bool ShouldClose { get; }
        public abstract void Update();
    }
}