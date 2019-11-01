using Ogar_CSharp.cells;
using Ogar_CSharp.primitives;
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
        public abstract Func<Reader, Protocol> decider { get; }
        public Listener Listener => connection.listener;
        public ServerHandle Handle => connection.listener.handle;
        //logger() { return this.connection.listener.handle.logger; }
        public Settings Settings => connection.listener.handle.Settings;

        public abstract bool Distinguishes(Reader reader);
        public abstract void OnSocketMessage(Reader reader);
        //public abstract void OnChatMessage(ChatSource source, string message);
        public abstract void OnNewOwnedCell(PlayerCell cell);
        public abstract void OnNewWorldBounds(World world, bool includeServerInfo);
        public abstract void OnWorldReset();
        //more to be implemented
    }
}
