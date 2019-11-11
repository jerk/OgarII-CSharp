using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Ogar_CSharp.Bots;
using Ogar_CSharp.Cells;
using Ogar_CSharp.Protocols;
using Ogar_CSharp.Worlds;
using WebSocketSharp;
using static Ogar_CSharp.Sockets.Listener;

namespace Ogar_CSharp.Sockets
{
    public class Connection : Router 
    {
        public IPAddress remoteAddress;
        public ClientSocket webSocket;
        public DateTime connectionTime;
        public DateTime lastActivityTime;
        public DateTime lastChatTime;
        public int upgradeLevel;
        public Protocol protocol = null;
        public bool socketDisconnected = false;
        public ushort closeCode;
        public string closeReason = null;
        public List<Minion> minions = new List<Minion>();
        public bool minionsFrozen;
        public bool controllingMinions;
        public Connection(Listener listener, ClientSocket socket) : base(listener)
        {
            webSocket = socket;
            remoteAddress = socket.Context.UserEndPoint.Address;
            connectionTime = DateTime.Now;
            lastActivityTime = DateTime.Now;
            lastChatTime = DateTime.Now;
            webSocket.onClose = (x) => OnSocketClose(x.Code, x.Reason);
            webSocket.onMessage = (x) => OnSocketMessage(x);
        }

        public override bool ShouldClose => socketDisconnected;
        public override bool IsExternal => true;
        public override bool SeparateInTeams => true;
        public override string Type => "connection";
        public void CloseSocket(ushort errorCode, string reason)
            => webSocket.CloseSocket(errorCode, reason);
        public override void OnNewOwnedCell(PlayerCell cell)
            => protocol.OnNewOwnedCell(cell);
        public override void OnWorldSet()
            => protocol.OnNewWorldBounds(Player.world.border, true);
        public override void OnWorldReset() 
            => protocol.OnWorldReset();
        public override void CreatePlayer()
        {
            base.CreatePlayer();
            //if(Settings.chatEnabled) chat stuff
            //else
            Handle.worlds[0].AddPlayer(Player);
        }
        public override void Close()
        {
            if (!socketDisconnected)
                return;
            base.Close();
            disconnected = true;
            disconnectionTick = Handle.tick;
            listener.OnDisconnection(this, closeCode, closeReason);
            webSocket.RemoveAllListeners();
        }
        public void Send(byte[] data)
        {
            if (socketDisconnected)
                return;
            webSocket.Send(data);
        }
        public void OnSocketClose(ushort code, string reason)
        {
            if (socketDisconnected)
                return;
            Console.WriteLine($"connection from {remoteAddress} has disconnected");
            socketDisconnected = true;
            closeCode = code;
            closeReason = reason;
        }
        public void OnSocketMessage(MessageEventArgs data)
        {
            if (data.IsText || data.IsPing)
            {
                CloseSocket(1003, "Unexpected message format");
                return;
            }
            var bytes = data.RawData;
            if (bytes.Length > 512 || bytes.Length == 0)
            {
                CloseSocket(1003, "Unexpected message size");
                return;
            }
            else
            {
                if (protocol != null)
                    protocol.OnSocketMessage(new Reader(bytes, 0));
                else
                {
                    protocol = ProtocolStore.Decide(this, new Reader(bytes, 0));
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
            return;
            message = message?.Trim();
            if (message == null)
                return;
            var globalChat = listener.globalChat;
            var lastChatTime = this.lastChatTime;
            this.lastChatTime = DateTime.Now;
            if(message.Length >= 2 && message[0] == '/')
            {
                //if(!Handle.ch)
                //to implement.
            }
        }
        public override async Task PerformAsyncTick()
        {
            Player.UpdateVisibleCells();
            //Update();
        }
        public override void Update()
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
            //this.Player.UpdateVisibleCells();
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
