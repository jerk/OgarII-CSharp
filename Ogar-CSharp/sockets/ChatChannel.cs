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
            public string name;
            public string isServer;
            public int color;
        }
    }
}
