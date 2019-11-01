using System;
using System.Collections.Generic;
using System.Text;
using Ogar_CSharp.cells;
using Ogar_CSharp.bots;
using Ogar_CSharp.sockets;

namespace Ogar_CSharp.worlds
{
    public class World : ISpawner
    {
        public class WorldStats
        {
            public int limit;
            public int _internal;
            public int external;
            public int playing;
            public int spectating;
            public string name;
            public string gamemode;
            public int uptime;
            public int loadTime;
        }
        public int id;
        public ServerHandle handle;
        public bool frozen;
        public long _nextCellId = 1;
        public List<Cell> cells = new List<Cell>();
        public List<Cell> boostingCells = new List<Cell>();
        public int motherCellCount;
        public int virusCount;
        public List<Cell> ejectedCells = new List<cells.Cell>();
        public List<PlayerCell> playerCells = new List<cells.PlayerCell>();
        public List<Player> players = new List<Player>();
        public Player largestPlayer;
        public List<Player> leaderboard = new List<Player>();
        //public chatchannel worldchat
        public Rect border;
        public WorldStats stats;
        public QuadTree<cells.Cell> finder;
        public Settings Settings => handle.Settings;

        public int PelletCount { get; set; }

        public World(ServerHandle handle, int id)
        {
            this.handle = handle;
            this.id = id;
        }
        public long NextCellId => (_nextCellId >= 4294967296) ? (_nextCellId = 1) : _nextCellId++;
        public void AfterCreation()
        {
            for (int i = 0; i < Settings.worldPlayerBotsPerWorld; i++)
                new PlayerBot(this);
        }
        public void Destroy()
        {
            while (players.Count > 0)
                RemovePlayer(players[0]);
            while (cells.Count > 0)
                RemoveCell(cells[0]);
        }
        public void SetBorder(Rect range)
        {
            this.border.x = range.x;
            this.border.y = range.y;
            this.border.w = range.w;
            this.border.h = range.h;
            if (finder != null)
                finder.Destroy();
            finder = new QuadTree<Cell>(border, Settings.worldFinderMaxLevel, Settings.worldFinderMaxItems, null);
            foreach (var cell in cells)
            {
                if (cell.Type == 0)
                    continue;
                finder.Insert(new QuadItem<Cell>(cell));
                if (!Misc.FunnyIntersects(border, cell.range))
                    RemoveCell(cell);
            }
        }
        public void AddCell(Cell cell)
        {
            cell.exists = true;
            cell.range = new Rect(cell.X, cell.Y, cell.Size, cell.Size);
            cells.Add(cell);
            finder.Insert(new QuadItem<Cell>(cell));
            cell.OnSpawned();
            handle.gamemode.OnNewCell(cell);
        }
        public bool SetCellAsBoosting(Cell cell)
        {
            if (cell.isBoosting)
                return false;
            cell.isBoosting = true;
            boostingCells.Add(cell);
            return true;
        }
        public bool SetCellAsNotBoosting(Cell cell)
        {
            if (!cell.isBoosting)
                return false;
            cell.isBoosting = false;
            boostingCells.Remove(cell);
            return true;
        }
        public void UpdateCell(Cell cell)
        {
            cell.range.x = cell.X;
            cell.range.y = cell.Y;
            cell.range.w = cell.Size;
            cell.range.h = cell.Size;
            finder.Update(cell.item);
        }
        public void RemoveCell(Cell cell)
        {
            this.handle.gamemode.OnCellRemove(cell);
            cell.OnRemoved();
            finder.Remove(cell.item);
            cell.range = default;
            this.SetCellAsNotBoosting(cell);
            this.cells.Remove(cell);
            cell.exists = false;
        }
        public void AddPlayer(Player player)
        {
            players.Add(player);
            player.world = this;
            player.hasWorld = true;
            //worldchat stuff
            handle.gamemode.OnPlayerJoinWorld(player, this);
            player.router.OnWorldSet();
            Console.WriteLine($"player {player.id} has been added to world {this.id}");
            if (!player.router.IsExternal)
                return;
            for (int i = 0; i < this.Settings.worldMinionsPerPlayer; i++)
                new Minion((Connection)player.router);
        }
        public void RemovePlayer(Player player)
        {
            players.Remove(player);
            handle.gamemode.OnPlayerLeaveWorld(player, this);
            player.world = null;
            player.hasWorld = false;
            //world chat stuff
            while (player.ownedCells.Count > 0)
                RemoveCell(player.ownedCells[0]);
            player.router.OnWorldReset();
            Console.WriteLine($"player {player.id} has been removed from world {this.id}");
        }
        public (int x, int y) GetRandomPos(int cellSize)
        {
            var random = new System.Random();
            return ((int)(border.x - border.w + cellSize + random.NextDouble() * (2 * this.border.w - cellSize)),
                (int)(border.y - border.h + cellSize + random.NextDouble() * (2 * border.h - cellSize)));
        }
        public bool IsSafeSpawnPos(Rect range)
        {
            return !finder.ContainsAny(range, (item) => item.item.AvoidWhenSpawning);
        }
        public (int x, int y) GetSafeSpawnPos(int cellSize)
        {
            var tries = this.Settings.worldSafeSpawnTries;
            while(--tries >= 0)
            {
                var pos = GetRandomPos(cellSize);
                if (IsSafeSpawnPos(new Rect(pos.x, pos.y, cellSize, cellSize)))
                    return pos;
            }
            return GetRandomPos(cellSize);
        }
        public (int? color, float x, float y) GetPlayerSpawn(int cellSize)
        {
            var random = new Random();
            if(Settings.worldSafeSpawnFromEjectedChange > (float)random.NextDouble() && ejectedCells.Count > 0)
            {
                var tries = Settings.worldSafeSpawnTries;
                while(--tries >= 0)
                {
                    var cell = ejectedCells[~~(int)(random.NextDouble() * ejectedCells.Count)];
                    if (IsSafeSpawnPos(new Rect(cell.X, cell.Y, cellSize, cellSize)))
                    {
                        RemoveCell(cell);
                        return (cell.Color, cell.X, cell.Y);
                    }
                }
            }
            var pos = GetSafeSpawnPos(cellSize);
            return (null, pos.x, pos.y);
        }
        public void SpawnPlayer(Player player, Point pos, short size)
        {
            var playerCell = new PlayerCell(player, pos.x, pos.y, size);
            AddCell(playerCell);
            player.UpdateState(PlayerState.Alive);
        }
        public void Update()
        {
            if (frozen)
                FrozenUpdate();
            else
                LiveUpdate();
        }
        public void LiveUpdate()
        {
            handle.gamemode.OnWorldTick(this);
            var self = this;
            List<Cell> eat = new List<Cell>(), rigid = new List<Cell>();
            foreach (var _cell in cells)
                _cell.OnTick();
            /*while(PelletCount < Settings.pelletCount)
            {
                var post = GetSafeSpawnPos(Settings.pelletMinSize);
                //AddCell(new p)
            }*/
            //viruses
            //mothercells
            for (int i = 0, l = this.boostingCells.Count; i < l;)
            {
                if (!this.BoostCell(this.boostingCells[i])) l--;
                else i++;
            }
            for (int i = 0; i < this.boostingCells.Count; i++)
            {
                var cell = this.boostingCells[i];
                if (cell.Type != 2 && cell.Type != 3) continue;
                this.finder.Search(cell.range, (other) =>
                {
                    if (cell.id == other.item.id) return;
                    switch (cell.GetEatResult(other.item))
                    {
                       case CellEatResult.Rigid: rigid.AddRange(new Cell[2] { cell, other.item }); break;
                        case CellEatResult.Eat: eat.AddRange(new Cell[2] { cell, other.item }); break;
                        case CellEatResult.EatInvd: eat.AddRange(new Cell[2] { other.item, cell }); break;
                    }
                });
            }
            for (int i = 0, l = this.playerCells.Count; i < l; i++)
            {
                var cell = this.playerCells[i];
                this.AutoSplitPlayerCell(cell);
                this.MovePlayerCell(cell);
                this.DecayPlayerCell(cell);
                this.BounceCell(cell, false);
                this.UpdateCell(cell);
            }
            for (int i = 0, l = playerCells.Count; i < l; i++)
            {
                var cell = this.playerCells[i];
                this.finder.Search(cell.range, (other) => {
                    if (cell.id == other.item.id) return;
                    switch (cell.GetEatResult(other.item))
                    {
                        case CellEatResult.Rigid: rigid.AddRange(new Cell[2] { cell, other.item }); break;
                        case CellEatResult.Eat: eat.AddRange(new Cell[2] { cell, other.item }); break;
                        case CellEatResult.EatInvd: eat.AddRange(new Cell[2] { other.item, cell }); break;
                    }
                });
            }
            for (int i = 0, l = rigid.Count; i < l;)
                this.ResolveRigidCheck(rigid[i++], rigid[i++]);
            for (int i = 0, l = eat.Count; i < l;)
                this.ResolveEatCheck(eat[i++], eat[i++]);
            largestPlayer = null;
            for (int i = 0, l = this.players.Count; i < l; i++)
            {
                var player = this.players[i];
                if (!float.IsNaN(player.score) && (this.largestPlayer == null || player.score > this.largestPlayer.score))
                    this.largestPlayer = player;
            }
            for (int i = 0, l = this.players.Count; i < l; i++)
            {
                var player = this.players[i];
                player.CheckExistence();
                if (!player.exists) { i--; l--; continue; }
                if (player.state == PlayerState.Spectating && this.largestPlayer == null)
                    player.UpdateState(PlayerState.Roaming);
                var router = player.router;
                for (int j = 0, k = this.Settings.playerSplitCap; j < k && router.splitAttempts > 0; j++)
                {
                    router.AttemptSplit();
                    router.splitAttempts--;
                }
                var nextEjectTick = this.handle.tick - this.Settings.playerEjectDelay;
                if (router.ejectAttempts > 0 && nextEjectTick >= router.ejectTick)
                {
                    router.AttemptEject();
                    router.ejectAttempts = 0;
                    router.ejectTick = this.handle.tick;
                }
                if (router.isPressingQ)
                {
                    if (!router.hasProcessedQ)
                        router.OnQPress();
                    router.hasProcessedQ = true;
                }
                else router.hasProcessedQ = false;
                if (router.requestingSpectate)
                {
                    router.OnSpectateRequest();
                    router.requestingSpectate = false;
                }
                if (router.spawningName != null)
                {
                    router.OnSpawnRequest();
                    router.spawningName = null;
                }
                player.UpdateViewArea();
            }
            CompileStatistics();
            handle.gamemode.CompileLeaderboard(this);
            if (stats.external <= 0 && handle.worlds.Count > Settings.worldMinCount)
                handle.RemovePlayer((short)id);
        }
        public void FrozenUpdate()
        {
            for (int i = 0, l = this.players.Count; i < l; i++)
            {
                var router = this.players[i].router;
                router.splitAttempts = 0;
                router.ejectAttempts = 0;
                if (router.isPressingQ)
                {
                    if (!router.hasProcessedQ)
                        router.OnQPress();
                    router.hasProcessedQ = true;
                }
                else router.hasProcessedQ = false;
                router.requestingSpectate = false;
                router.spawningName = null;
            }
        }
        public void ResolveRigidCheck(Cell a, Cell b)
        {
            float dx = b.X - a.X;
            float dy = b.X - a.Y;
            int d = (int)Math.Sqrt(dx * dx + dy * dy);
            var m = a.Size + b.Size - d;
            if (m <= 0)
                return;
            if(d == 0)
            {
                d = 1;
                dx = 1;
                dy = 0;
            }
            else
            {
                dx /= d;
                dy /= d;
            }
            var M = a.SquareSize + b.SquareSize;
            var aM = b.SquareSize / M;
            var bM = a.SquareSize / M;
            a.X -= dx * m * aM;
            a.Y -= dy * m * aM;
            b.X += dx * m * bM;
            b.Y += dy * m * bM;
            this.BounceCell(a, false);
            this.BounceCell(b, false);
            this.UpdateCell(a);
            this.UpdateCell(b);
        }
        public void ResolveEatCheck(Cell a, Cell b)
        {
            if (!a.exists || !b.exists)
                return;
            var dx = b.X - a.X;
            var dy = b.Y - a.Y;
            var d = (float)Math.Sqrt(dx * dx + dy * dy);
            if (d > a.Size - b.Size / 3)
                return;
            if (!handle.gamemode.CanEat(a, b))
                return;
            a.WhenAte(b);
            b.WhenEatenBy(a);
            RemoveCell(b);
            UpdateCell(a);
        }
        public bool BoostCell(Cell cell)
        {

        }
        public void BounceCell(Cell cell, bool bounce)
        {

        }
        //public void SplitVirus(v)
        public void MovePlayerCell(PlayerCell cell)
        {

        }
        public void DecayPlayerCell(PlayerCell cell)
        {

        }
        //public void LaunchPlayerCell
        public void AutoSplitPlayerCell(PlayerCell cell)
        {

        }
        public void SplitPlayer(Player player)
        {

        }
        public void EjectFromPlayer(Player player)
        {

        }
        public void PopPlayerCell(PlayerCell cell)
        {

        }
        public int[] DistributeCellMass(PlayerCell cell)
        {

        }
        public void CompileStatistics()
        {

        }
            
    }
}
