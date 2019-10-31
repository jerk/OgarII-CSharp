using Ogar_CSharp.primitives;
using Ogar_CSharp.protocols;
using Ogar_CSharp.sockets;
using Ogar_CSharp.worlds;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Ogar_CSharp
{
    public class ServerHandle
    {
        public Settings Settings
        {
            get;
            private set;
        }
        public ProtocolStore protocols = new ProtocolStore();
        //gamemodes = new gamemodelist(this);
        //gamemode = null;
        //commands = new commandlist(this);
        //chatcommnds = new commandlist(this);
        public bool running = false;
        public DateTime? startTime = default;
        public int avargateTickTime;
        public int tick;
        public short tickDelay;
        public short stepMult;
        //stepMult = NaN;
        Ticker ticker = new Ticker(40);
        Stopwatch stopWatch = new Stopwatch();
        //logger = new Logger();
        Listener listener;
        //matchMaker = new MatchMaker(this);
        List<World> worlds = new List<World>();
        List<Player> players = new List<Player>();
        public ServerHandle()
        {
            listener = new Listener(this);
            //setSettings(settings);
            ticker.Add(OnTick);
        }
        public void SetSettings(Settings settings)
        {
            this.Settings = settings;
            tickDelay = (short)(1000 / settings.serverFrequency);
            ticker.step = tickDelay;
            stepMult = (short)(tickDelay / 40);
        }
        public bool Start()
        {
            if (running)
                return false;
            Console.WriteLine("Starting");
            //GameMode.setGaMEMODE(SERVER SETTINGS GAMEMODE);
            startTime = DateTime.Now;
            avargateTickTime = tick = 0;
            running = true;
            listener.Open();
            ticker.Start();
            //gamemode on handle start();
            Console.WriteLine("ticker begin");
            Console.WriteLine($"Ogar-CSharp II {Misc.version}");
            Console.WriteLine("gamemode ??");
            return true;
        }
        public bool Stop()
        {
            if (!running)
                return false;
            Console.WriteLine("Stopping");
            if (ticker.running)
                ticker.Stop();
            //foreach world = remove
            //foreach player = remove
            //foreach router = close
            //gamemode stop handle
            listener.Close();
            startTime = null;
            avargateTickTime = tick = 0;
            running = false;
            Console.WriteLine("Ticker stop");
            return true;
        }
        public World CreateWorld()
        {
            int id = 0;
            //stuff
            Console.WriteLine($"added a world with id {id}");
            return null; // will return newWorld
        }
        public bool RemoveWorld(short id)
        {
            
        }
        public Player CreatePlayer(Router router)
        {

        }
        public bool RemovePlayer(short id)
        {

        }
        public void OnTick()
        {
            stopWatch.Start();
            tick++;
            //doStuff
        }
    }
}
