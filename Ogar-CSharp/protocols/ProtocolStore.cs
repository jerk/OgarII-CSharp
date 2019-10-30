using Ogar_CSharp.primitives;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Ogar_CSharp.sockets;
using System.Linq.Expressions;
using static Ogar_CSharp.primitives.ReflectionMisc;

namespace Ogar_CSharp.protocols
{
    public class ProtocolStore
    {
        public Dictionary<Type, ObjectActivator<Protocol>> deciders = new Dictionary<Type, ObjectActivator<Protocol>>();
        public void Register(params Type[] protocols)
        {
            foreach(var protocol in protocols)
            {
                if (deciders.ContainsKey(protocol))
                    continue;
                deciders.Add(protocol, GetActivator<Protocol>(protocol.GetConstructor(new Type[] { typeof(Connection) })));
            }
        }
        public Connection Decide(Connection connection, Reader reader)
        {
            foreach(var decider in deciders.Values)
            {
                var generated = decider(connection);
                if (!generated.Distinguishes(reader))
                {
                    reader.offset = 0;
                    continue;
                }
                return connection.s
            }
        }
    }
}
