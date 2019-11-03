using Ogar_CSharp.gamemodes;
using Ogar_CSharp.protocols;
using Ogar_CSharp.sockets;
using Ogar_CSharp.worlds;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
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
        //public game gamemodes = new gamemodelist(this);
        public GameMode gamemode;
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
        public Listener listener;
        public MatchMaker matchMaker;
        public List<World> worlds = new List<World>();
        public List<Player> players = new List<Player>();
        public string Version => Misc.version;
        public ServerHandle(Settings settings)
        {
            Settings = settings;
            matchMaker = new MatchMaker(this);
            listener = new Listener(this);
            //setSettings(settings);
            ticker.Add(OnTick);
            gamemode = new FFA(this);
            CreateWorld();
            SetSettings(settings);
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
            while (this.worlds.Any(x => x.id == ++id)) ;
            var newWorld = new World(this, id);
            this.worlds.Add(newWorld);
            this.gamemode.OnNewWorld(newWorld);
            newWorld.AfterCreation();
            Console.WriteLine($"added a world with id {id}");
            return newWorld;
        }
        public bool RemoveWorld(short id)
        {
            if (!worlds.Any(x => x.id == id))
                return false;
            var world = worlds.First(x => x.id == id);
            gamemode.OnWorldDestroy(world);
            world.Destroy();
            worlds.Remove(world);
            Console.WriteLine($"removed a world with id {id}");
            return true;
        }
        public Player CreatePlayer(Router router)
        {
            int id = 0;
            while (players.Any(x => x.id == ++id)) ;
            var newPlayer = new Player(this, id, router);
            players.Add(newPlayer);
            router.Player = newPlayer;
            gamemode.OnNewPlayer(newPlayer);
            Console.WriteLine($"added a player with id {id}");
            return newPlayer;
        }
        public bool RemovePlayer(int id)
        {
            Console.WriteLine($"removed a player with id {id}");
            if (!players.Any(x => x.id == id)) 
                return false;
            var player = this.players.First(x => x.id == id);
            this.gamemode.OnPlayerDestroy(player);
            player.Destroy();
            player.exists = false;
            players.Remove(player);
            Console.WriteLine($"removed a player with id {id}");
            return true;
        }
        public void OnTick()
        {
            stopWatch.Start();
            tick++;
            foreach(var world in worlds)
                world.Update();
            listener.Update();
            matchMaker.Update();
            gamemode.OnHandleTick();
            avargateTickTime = (int)stopWatch.Elapsed.Ticks;
            stopWatch.Reset();
        }
    }
}
