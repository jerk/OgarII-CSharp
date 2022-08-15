using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using net.vieapps.Components.WebSockets;
using System.Threading;
using System.Net.WebSockets;
using WebSocket = net.vieapps.Components.WebSockets.WebSocket;

namespace Ogar_CSharp.Sockets
{
    public class Listener
    {
        static Listener()
        {
            WebSocket.ReceiveBufferSize = 1024;
        }
        public WebSocket listenerSocket;
        public ServerHandle handle;
        public ChatChannel globalChat;
        public List<Router> routers = new List<Router>();
        public List<Connection> connections = new List<Connection>();
        public Listener(ServerHandle handle)
        {
            listenerSocket = new WebSocket() { NoDelay = true };
            globalChat = new ChatChannel(this);
            this.handle = handle;
        }
        public int ConnectionCountForIP(string ipAddress)
            => connections.Count((x) => x.RemoteAddress.ToString() == ipAddress);
        public Settings Settings
            => handle.Settings;
        public bool Open()
        {
            if (listenerSocket == null)
                return false;
            Console.WriteLine($"Listener opening at {Settings.listeningPort}");
            listenerSocket.OnConnectionEstablished += OnConnection;
            listenerSocket.OnMessageReceived += OnData;
            listenerSocket.OnConnectionBroken += OnDisconnection;
            listenerSocket.StartListen(Settings.listeningPort);
            return true;
        }
        public bool Close()
        {
            if (listenerSocket == null)
                return false;
            Console.WriteLine("Listener Closing");
            listenerSocket.StopListen();
            return true;
        }
        public bool VerifyClient(ManagedWebSocket socket)
        {
            var address = (socket.RemoteEndPoint as IPEndPoint).Address.ToString();
            if (connections.Count > Settings.listenerMaxConnections)
            {
                Console.WriteLine("listenerMaxConnections reached, dropping new connections");
                return false;
            }
            var acceptedOrigins = Settings.listenerAcceptedOrigins;
            if (acceptedOrigins.Count > 0 && acceptedOrigins.Contains(socket.RequestUri.ToString()))
            {
                Console.WriteLine($"listenerAcceptedOrigins doesn't contain {socket.RequestUri.ToString()}");
                return false;
            }
            if (Settings.listenerForbiddenIPs.Contains(address))
            {
                Console.WriteLine($"listenerForbiddenIPs contains {address}, dropping connection");
                return false;
            }
            if (Settings.listenerMaxConnectionsPerIP > 0)
            {
                var count = ConnectionCountForIP(address);
                if (count != 0 && count >= Settings.listenerMaxConnectionsPerIP)
                {
                    Console.WriteLine($"listenerMaxConnectionsPerIP reached for '{address}', dropping its new connections");
                    return false;
                }
            }
            return true;
        }
        public void AddRouter(Router router)
            => routers.Add(router);
        public void RemoveRouter(Router router)
           => routers.Remove(router);
        public void OnConnection(ManagedWebSocket client)
        {
            if (!VerifyClient(client))
                client.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.InternalServerError, "Connection rejected", CancellationToken.None);
            var newConnection = new Connection(this, client);
            client.Set("agar", newConnection);
            Console.WriteLine($"CONNECTION FROM {newConnection.RemoteAddress}");
            connections.Add(newConnection);
        }
        public void OnDisconnection(ManagedWebSocket ws)
        {
            Connection connection = ws.Get<Connection>("agar");
            Console.WriteLine($"DISCONNECTION FROM {connection.RemoteAddress}");
            connection.OnSocketClose(0, null);
            connections.Remove(connection);
            globalChat.Remove(connection);
        }
        public void OnData(ManagedWebSocket ws, WebSocketReceiveResult res, byte[] data)
        {
            Connection connection = ws.Get<Connection>("agar");
            if (res.MessageType != WebSocketMessageType.Binary)
            {
                connection.CloseSocket(1003, "Invalid message type");
            }
            connection.OnSocketMessage(data);
        }
        public void Update()
        {
            int l;
            int i;
            for (i = 0, l = this.routers.Count; i < l; i++)
            {
                var router = this.routers[i];
                if (!router.ShouldClose) continue;
                router.Close(); i--; l--;
            }
            for (i = 0; i < l; i++)
                this.routers[i].Tick();
            for (i = 0, l = this.connections.Count; i < l; i++)
            {
                var connection = connections[i];
                if (Settings.listenerForbiddenIPs.Contains(connection.RemoteAddress.ToString()))
                    connection.CloseSocket(1003, "Remote address is forbidden");
                else if(connection.protocol == null && ((handle.ServerTimeInMilliseconds - connection.ConnectionTime) > 5_000))
                    connection.CloseSocket(1003, "never received protocol init");
               // else if ((handle.ServerTimeInMilliseconds - connection.lastActivityTime) >= (Settings.listenerMaxClientDormancy / 10_000))
                 //   connection.CloseSocket(1003, "Maximum dormancy time exceeded");
            }
        }
    }
}
