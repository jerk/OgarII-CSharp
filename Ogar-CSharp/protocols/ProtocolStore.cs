
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Ogar_CSharp.Sockets;
using System.Linq.Expressions;

namespace Ogar_CSharp.Protocols
{
    public class ProtocolStore
    {
        public delegate Protocol Decider(Reader reader, Connection connection);
        public static void RegisterProtocols(params Decider[] deciders)
        {
            currentDeciders.AddRange(deciders);
        }
        private static List<Decider> currentDeciders = new List<Decider>() { LegacyProtocol.Decider, ModernProtocol.Decider };
        public static Protocol Decide(Connection connection, Reader reader)
        {
            foreach (var decider in currentDeciders)
            {
                Protocol protocol = decider(reader, connection);
                reader.offset = 0;
                if(protocol != null)
                    return protocol;
            }
            return null;
        }
    }
}
