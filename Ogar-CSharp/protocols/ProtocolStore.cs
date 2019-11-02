
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Ogar_CSharp.sockets;
using System.Linq.Expressions;

namespace Ogar_CSharp.protocols
{
    public class ProtocolStore
    {
        /*public void Register(params Type[] protocols)
        {
            foreach(var protocol in protocols)
            {
                if (deciders.ContainsKey(protocol))
                    continue;
                deciders.Add(protocol, GetActivator<Protocol>(protocol.GetConstructor(new Type[] { typeof(Connection) })));
            }
        }*/
        public Protocol Decide(Connection connection, Reader reader)
        {

            var generated = new LegacyProtocol(connection);
            if (generated.Distinguishes(reader))
            {
                return generated;
            }
            return null;
        }
    }
}
