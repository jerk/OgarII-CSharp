using System;
using System.Collections.Generic;
using System.Text;

namespace Ogar_CSharp
{
    public class Settings
    {
        public int listeningPort;
        public int listenerMaxConnections;
        public List<string> listenerAcceptedOrigins;
        public List<string> listenerForbiddenIPs;
        public int listenerMaxConnectionsPerIP;
        public int serverFrequency;
        public byte playerMaxNameLength;
    }
}
