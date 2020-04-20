using Ogar_CSharp.Cells;
using Ogar_CSharp.Other;
using Ogar_CSharp.Sockets;
using Ogar_CSharp.Worlds;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Ogar_CSharp.Protocols
{
    public abstract class Protocol
    {
        public Connection connection;
        public Protocol(Connection connection)
            => this.connection = connection;
        public abstract string Type { get; }
        public abstract string SubType { get; }
        public Listener Listener => connection.listener;
        public ServerHandle Handle => connection.listener.handle;
        //logger() { return this.connection.listener.handle.logger; }
        public Settings Settings => connection.listener.handle.Settings;
        public abstract void OnSocketMessage(DataReader reader);
        //public abstract void OnChatMessage(ChatSource source, string message);
        public abstract void OnNewOwnedCell(PlayerCell cell);
        public abstract void OnNewWorldBounds(RectangleF range, bool includeServerInfo);
        public abstract void OnWorldReset();
        public abstract void OnSpectatePosition(ViewArea area);
        public abstract void OnVisibleCellUpdate(IList<Cell> add, IList<Cell> upd, IList<Cell> eat, IList<Cell> del);
        public abstract void OnLeaderboardUpdate(LeaderboardType type, IEnumerable<LeaderBoardEntry> data, LeaderBoardEntry selfData);
        public void Send(byte[] data) => connection.Send(data);
        public void Fail(ushort code, string reason) => connection.CloseSocket(code, reason);
    }
}
