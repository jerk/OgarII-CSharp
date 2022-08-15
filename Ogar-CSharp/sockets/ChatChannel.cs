using Ogar_CSharp.Other;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ogar_CSharp.Sockets
{
    public class ChatChannel
    {
        public struct ChatEntry
        {
            public readonly ChatSource Source;
            public readonly string Message;
        }
        public class ChatSource
        {
            public static readonly ChatSource ServerSource = new ChatSource() { color = 0x3F3FC0, isServer = true, name = "server" };            
            public string name;
            public bool isServer;
            public OgarColor color;
            public static ChatSource GetSourceFromConnection(Connection connection)
            {
                return new ChatSource()
                {
                    name = connection.Player.chatName,
                    color = connection.Player.chatColor
                };
            }
        }
        public ChatChannel(Listener listener)
        {
            this.listener = listener;
        }
        public readonly Listener listener;
        private readonly List<Connection> connections = new List<Connection>();
        public void Add(Connection connection)
        {
            connections.Add(connection);
        }
        public void Remove(Connection connection)
        {
            connections.Remove(connection);
        }
        public void Broadcast(Connection source, string message)
        {
            if (ShouldFilter(message))
                return;
            var sourceInfo = source == null ? ChatSource.ServerSource : ChatSource.GetSourceFromConnection(source);
            for (int i = 0, l = connections.Count; i < l; i++)
                connections[i].protocol.OnChatMessage(sourceInfo, message);
        }
        public bool ShouldFilter(string message)
        {
            return false;
        }
    }
}
