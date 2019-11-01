using Ogar_CSharp.cells;
using Ogar_CSharp.Other;
using Ogar_CSharp.sockets;
using Ogar_CSharp.worlds;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ogar_CSharp.protocols
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

        public abstract bool Distinguishes(Reader reader);
        public abstract void OnSocketMessage(Reader reader);
        //public abstract void OnChatMessage(ChatSource source, string message);
        public abstract void OnNewOwnedCell(PlayerCell cell);
        public abstract void OnNewWorldBounds(Rect range, bool includeServerInfo);
        public abstract void OnWorldReset();
        public abstract void OnSpectatePosition(ViewArea area);
        public abstract void OnVisibleCellUpdate(IEnumerable<Cell> add, IEnumerable<Cell> upd, IEnumerable<Cell> eat, IEnumerable<Cell> del);
        public abstract void OnLeaderboardUpdate(LeaderboardType type, List<LeaderBoardEntry> data, LeaderBoardEntry selfData);
        public void Send(byte[] data) => connection.Send(data);
        public void Fail(ushort code, string reason)
        {
            connection.CloseSocket(code, reason);
        }
    }
}
