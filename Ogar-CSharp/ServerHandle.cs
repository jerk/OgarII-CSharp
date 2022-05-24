﻿using Ogar_CSharp.Protocols;
using Ogar_CSharp.Sockets;
using Ogar_CSharp.Worlds;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using Ogar_CSharp.Gamemodes;
using Ogar_CSharp.Bots;

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
        public Gamemode gamemode;
        //commands = new commandlist(this);
        //chatcommnds = new commandlist(this);
        public bool running = false;
        public DateTimeOffset startTime;
        public double avargateTickTime;
        public int tick;
        public int tickDelay;
        public float stepMult;
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
            gamemode = new Teams(this);
            CreateWorld();
            SetSettings(settings);           
        }
        public void SetSettings(Settings settings)
        {
            this.Settings = settings;
            tickDelay = (1000 / settings.serverFrequency);
            ticker.step = tickDelay;
            stepMult = tickDelay / 40;
        }
        public bool Start()
        {
            if (running)
                return false;
            Console.WriteLine("Starting");
            gamemode = this.GetGamemode(Settings.serverGamemode);
            startTime = DateTimeOffset.Now;
            avargateTickTime = tick = 0;
            running = true;
            listener.Open();
            ticker.Start();
            Console.WriteLine($"Ogar-CSharp II {Misc.version}");
            Console.WriteLine("Game Mode: " + gamemode.Name);
            return true;
        }
        public bool Stop()
        {
            if (!running)
                return false;
            Console.WriteLine("Stopping");
            ticker.Stop();
            //foreach world = remove
            //foreach player = remove
            //foreach router = close
            //gamemode stop handle
            listener.Close();
            avargateTickTime = tick = 0;
            running = false;
            Console.WriteLine("Ticker stop");
            return true;
        }
        public World CreateWorld()
        {
            uint id = 0;
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
            uint id = 1;
            while (players.Any(x => x.Id == ++id)) ;
            var newPlayer = new Player(this, id, router);
            players.Add(newPlayer);
            router.Player = newPlayer;
            gamemode.OnNewPlayer(newPlayer);
            if (router is not Bot)
            Console.WriteLine($"added a player with id {id}");
            return newPlayer;
        }
        public bool RemovePlayer(uint id)
        {
            if (!players.Any(x => x.Id == id)) 
                return false;
            var player = this.players.First(x => x.Id == id);
            this.gamemode.OnPlayerDestroy(player);
            player.Destroy();
            player.exists = false;
            players.Remove(player);
            Console.WriteLine($"removed a player with id {id}");
            return true;
        }
        public void OnTick()
        {
            stopWatch.Restart();
            tick++;
            foreach(var world in worlds)
                world.Update();
            listener.Update();
            matchMaker.Update();
            gamemode.OnHandleTick();
            avargateTickTime = stopWatch.Elapsed.TotalMilliseconds;
            stopWatch.Reset();
        }
    }
}
