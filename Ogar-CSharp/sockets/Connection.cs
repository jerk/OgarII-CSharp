using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Ogar_CSharp.cells;
using Ogar_CSharp.protocols;
using static Ogar_CSharp.sockets.Listener;

namespace Ogar_CSharp.sockets
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
        //public Minion[] minions;
        public bool minionsFrozen;
        public bool controllingMinions;
        public Connection(Listener listener, ClientSocket socket) : base(listener)
        {
            webSocket = socket;
            remoteAddress = socket.Context.UserEndPoint.Address;
            connectionTime = DateTime.Now;
            lastActivityTime = DateTime.Now;
            lastChatTime = DateTime.Now;
            webSocket.onClose = (x) => Close();
        }

        public override bool ShouldClose => throw new NotImplementedException();
        public override void Close()
        {
            if (!socketDisconnected)
            {
                return;
            }
            base.Close();
            disconnected = true;
            disconnectionTick = Handle.tick;
            webSocket.o
        }
        public override void OnNewOwnedCell(PlayerCell cell)
        {
            throw new NotImplementedException();
        }

        public override void OnWorldReset()
        {
            throw new NotImplementedException();
        }

        public override void OnWorldSet()
        {
            throw new NotImplementedException();
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }
    }
}
