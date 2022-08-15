﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using net.vieapps.Components.WebSockets;
using Ogar_CSharp.Bots;
using Ogar_CSharp.Cells;
using Ogar_CSharp.Protocols;
using Ogar_CSharp.Worlds;
using static Ogar_CSharp.Sockets.Listener;

namespace Ogar_CSharp.Sockets
{
    public class Connection : Router
    {
        public ManagedWebSocket WebSocket { get; private set; }
        public IPAddress RemoteAddress { get; }
        public List<Minion> Minions;
        public readonly long ConnectionTime;
        public DateTime lastChatTime;
        public long lastActivityTime;
        public int upgradeLevel;
        public Protocol protocol = null;
        public bool socketDisconnected = false;
        public ushort closeCode;
        public string closeReason = null;
        public bool minionsFrozen;
        public bool controllingMinions;
        public Connection(Listener listener , ManagedWebSocket webSocket) : base(listener)
        {
            WebSocket = webSocket;
            RemoteAddress = (webSocket.RemoteEndPoint as IPEndPoint).Address;
            ConnectionTime = Handle.ServerTimeInMilliseconds;
            lastActivityTime = ConnectionTime;
            lastChatTime = DateTime.Now;
        }

        public override bool ShouldClose => socketDisconnected;
        public override bool IsExternal => true;
        public override bool SeparateInTeams => true;
        public override string Type => "connection";
        public void CloseSocket(ushort errorCode, string reason)
            => WebSocket.CloseAsync((System.Net.WebSockets.WebSocketCloseStatus)errorCode, 
                reason, CancellationToken.None);
        public override void OnNewOwnedCell(PlayerCell cell)
            => protocol.OnNewOwnedCell(cell);
        public override void OnWorldSet()
            => protocol.OnNewWorldBounds(Player.world.border, true);
        public override void OnWorldReset() 
            => protocol.OnWorldReset();
        public override void CreatePlayer()
        {
            base.CreatePlayer();
            if (Settings.chatEnabled)
                listener.globalChat.Add(this);
            Handle.worlds[0].AddPlayer(Player);

        }
        public override void Close()
        {
            if (!socketDisconnected)
                return;
            base.Close();
            disconnected = true;
            disconnectionTick = Handle.tick;
        }
        public void Send(ReadOnlySpan<byte> data)
        {
            if (socketDisconnected)
                return;
            WebSocket.SendAsync(data.ToArray());
        }
        public void OnSocketClose(ushort code, string reason)
        {
            if (socketDisconnected)
                return;
            Console.WriteLine($"connection from {RemoteAddress} has disconnected");
            socketDisconnected = true;
            closeCode = code;
            closeReason = reason;
            WebSocket.Dispose();
            WebSocket = null;
        }
        public void OnSocketMessage(ReadOnlySpan<byte> data)
        {
            lastActivityTime = Handle.ServerTimeInMilliseconds;
            if (data.Length > 512 || data.Length == 0)
            {
                CloseSocket(1003, "Unexpected message size");
                return;
            }
            else
            {
                if (protocol != null)
                    protocol.OnSocketMessage(new DataReader(data, 0));
                else
                {
                    protocol = ProtocolStore.Decide(this, new DataReader(data, 0));
                    if (protocol == null)
                    {
                        CloseSocket(1003, "Ambiguous protocol");
                        return;
                    }
                }
            }
        }
        public void OnChatMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;
            var globalChat = listener.globalChat;
            var lastChatTime = this.lastChatTime;
            this.lastChatTime = DateTime.Now;
            if(message.Length >= 2 && message[0] == '/')
            {
                //if(!Handle.ch)
                //to implement.
            }
            else if (true) // should be Date.now() - lastChatTime >= this.settings.chatCooldown
            {
                globalChat.Broadcast(this, message);
            }
        }

        public override void Tick()
        {
            if (!hasPlayer)
                return;
            if (!this.Player.hasWorld)
            {
                if (spawningName != null)
                    Handle.matchMaker.ToggleQueued(this);
                spawningName = null;
                splitAttempts = 0;
                ejectAttempts = 0;
                requestingSpectate = false;
                isPressingQ = false;
                isPressingQ = false;
                hasProcessedQ = false;
                return;
            }
            this.Player.UpdateVisibleCells();
            List<Cell> add = new List<Cell>(), upd = new List<Cell>(), eat = new List<Cell>(), del = new List<Cell>();
            var player = this.Player;
            var visible = player.visibleCells;
            var lastVisible = player.lastVisibleCells;
            foreach (var item in visible)
            {
                if (!lastVisible.ContainsKey(item.Key))
                    add.Add(item.Value);
                else if (item.Value.ShouldUpdate)
                    upd.Add(item.Value);
            }
            foreach (var item1 in lastVisible)
            {
                if (visible.ContainsKey(item1.Key))
                    continue;
                if (item1.Value.eatenBy != null)
                    eat.Add(item1.Value);
                del.Add(item1.Value);
            }
            if (player.currentState == PlayerState.Spectating || player.currentState == PlayerState.Roaming)
                protocol.OnSpectatePosition(player.viewArea);
            if (Handle.tick % 4 == 0)
                Handle.gamemode.SendLeaderboard(this);
            protocol.OnVisibleCellUpdate(add, upd, eat, del);
        }

    }
}
