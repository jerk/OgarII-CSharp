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
            public string name = "";
            public string gamemode = "";
            public long uptime;
            public float loadTime;
        }
        public int id;
        public ServerHandle handle;
        public bool frozen;
        public long _nextCellId = 1;
        public List<Cell> cells = new List<Cell>();
        public List<Cell> boostingCells = new List<Cell>();
        public int motherCellCount;
        public int virusCount;
        public List<EjectedCell> ejectedCells = new List<EjectedCell>();
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
            SetBorder(new Rect(Settings.worldMapX, Settings.worldMapY, Settings.worldMapW, Settings.worldMapH));
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
                finder.Insert(cell);
                if (!Misc.FullyIntersects(border, cell.range))
                    RemoveCell(cell);
            }
        }
        public void AddCell(Cell cell)
        {
            cell.exists = true;
            cell.range = new Rect(cell.X, cell.Y, cell.Size, cell.Size);
            cells.Add(cell);
            finder.Insert(cell);
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
            finder.Update(cell);
        }
        public void RemoveCell(Cell cell)
        {
            this.handle.gamemode.OnCellRemove(cell);
            cell.OnRemoved();
            finder.Remove(cell);
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
        public (float x, float y) GetRandomPos(int cellSize)
        {
            var random = new Random();
            return ((float)(border.x - border.w + cellSize + random.NextDouble() * (2 * this.border.w - cellSize)),
                (float)(border.y - border.h + cellSize + random.NextDouble() * (2 * border.h - cellSize)));
        }
        public bool IsSafeSpawnPos(Rect range)
        {
            return !finder.ContainsAny(range, (item) => ((Cell)item).AvoidWhenSpawning);
        }
        public (float x, float y) GetSafeSpawnPos(int cellSize)
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
                while (--tries >= 0)
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
        public void SpawnPlayer(Player player, Point pos, float size)
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
            while (PelletCount < Settings.pelletCount)
            {
                var pos = GetSafeSpawnPos(Settings.pelletMinSize);
                AddCell(new Pellet(this, this, pos.x, pos.y));
             }

            for (int i = 0, l = this.boostingCells.Count; i < l;)
            {
                if (!this.BoostCell(this.boostingCells[i])) l--;
                else i++;
            }
            for (int i = 0; i < this.boostingCells.Count; i++)
            {
                var cell = this.boostingCells[i];
                if (cell.Type != 2 && cell.Type != 3) 
                    continue;
                this.finder.Search(cell.range, (other) =>
                {
                    if (cell.id == ((Cell)other).id) 
                        return;
                    switch (cell.GetEatResult(((Cell)other)))
                    {
                       case CellEatResult.Rigid: rigid.AddRange(new Cell[2] { cell, ((Cell)other) }); break;
                        case CellEatResult.Eat: eat.AddRange(new Cell[2] { cell, ((Cell)other) }); break;
                        case CellEatResult.EatInvd: eat.AddRange(new Cell[2] { ((Cell)other), cell }); break;
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
                    if (cell.id == ((Cell)other).id) return;
                    switch (cell.GetEatResult(((Cell)other)))
                    {
                        case CellEatResult.Rigid: 
                            rigid.AddRange(new Cell[2] { cell, ((Cell)other) }); break;
                        case CellEatResult.Eat: 
                            eat.AddRange(new Cell[2] { cell, ((Cell)other) }); break;
                        case CellEatResult.EatInvd: 
                            eat.AddRange(new Cell[2] { ((Cell)other), cell }); break;
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
            if (stats.external <= 1 && handle.worlds.Count > Settings.worldMinCount)
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
            float dy = b.Y - a.Y;
            int d = (int)Math.Sqrt(dx * dx + dy * dy);
            var m = a.Size + b.Size - d;
            if (m <= 0)
                return;
            if (d == 0)
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
            var d = cell.boost.d / 9 * handle.stepMult;
            cell.X += cell.boost.dx * d;
            cell.Y += cell.boost.dy * d;
            BounceCell(cell, true);
            UpdateCell(cell);
            if ((cell.boost.d -= d) >= 1) 
                return true;
            SetCellAsNotBoosting(cell);
            return false;
        }
        public void BounceCell(Cell cell, bool bounce)
        {
            var r = cell.Size / 2;
            var b = border;
            if (cell.X <= b.x - b.w + r)
            {
                cell.X = b.x - b.w + r;
                if (bounce) cell.boost.dx = -cell.boost.dx;
            }
            if (cell.X >= b.x + b.w - r)
            {
                cell.X = b.x + b.w - r;
                if (bounce) cell.boost.dx = -cell.boost.dx;
            }
            if (cell.Y <= b.y - b.h + r)
            {
                cell.Y = b.y - b.h + r;
                if (bounce) cell.boost.dy = -cell.boost.dy;
            }
            if (cell.Y >= b.y + b.h - r)
            {
                cell.Y = b.y + b.h - r;
                if (bounce) cell.boost.dy = -cell.boost.dy;
            }
        }
        //public void SplitVirus(v)
        public void MovePlayerCell(PlayerCell cell)
        {
            var router = cell.owner.router;
            if (router.disconnected)
                return;
            var dx = router.mouseX - cell.X;
            var dy = router.mouseY - cell.Y;
            var d = (float)Math.Sqrt(dx * dx + dy * dy);
            if (d < 1)
                return;
            dx /= d;
            dy /= d;
            var m = Math.Min(cell.MoveSpeed, d) * handle.stepMult;
            cell.X += dx * m;
            cell.Y += dy * m;
        }
        public void DecayPlayerCell(PlayerCell cell)
        {
            var newSize = cell.Size - cell.Size * handle.gamemode.GetDecayMult(cell) / 50 * handle.stepMult;
            cell.Size = Math.Max(newSize, Settings.playerMinSize);
        }
        public void LaunchPlayerCell(PlayerCell cell, float size, Boost boost)
        {
            cell.SquareSize -= size * size;
            var x = cell.X + 20 * boost.dx;
            var y = cell.Y + 20 * boost.dy;
            var newCell = new PlayerCell(cell.owner, x, y, size);
            newCell.boost.dx = boost.dx;
            newCell.boost.dy = boost.dy;
            newCell.boost.d = boost.d;
            AddCell(newCell);
            SetCellAsBoosting(newCell);
        }
        public void AutoSplitPlayerCell(PlayerCell cell)
        {
            var minSplit = Settings.playerMaxSize * Settings.playerMaxSize;
            var cellsLeft = 1 * Settings.playerMaxCells - cell.owner.ownedCells.Count;
            var overflow = (float)Math.Ceiling(cell.SquareSize / minSplit);
            if (overflow == 1 || cellsLeft <= 0)
                return;
            var splitTimes = (int)Math.Min(overflow, cellsLeft);
            var splitSize = (float)Math.Min(Math.Sqrt(cell.SquareSize / splitTimes), Settings.playerMaxSize);
            var random = new Random();
            for (int i = 1; i < splitTimes; i++)
            {
                var angle = (float)(random.NextDouble() * 2 * Math.PI);
                this.LaunchPlayerCell(cell, splitSize, new Boost
                {
                    dx = (float)Math.Sin(angle),
                    dy = (float)Math.Cos(angle),
                    d = Settings.playerSplitBoost
                });
            }
            cell.Size = splitSize;
        }
        public void SplitPlayer(Player player)
        {
            var router = player.router; 
            var l = player.ownedCells.Count;
            for (int i = 0; i < l; i++)
            {
                if (player.ownedCells.Count >= Settings.playerMaxCells)
                    break;
                var cell = player.ownedCells[i];
                if (cell.Size < Settings.playerMinSplitSize)
                    continue;
                float dx = router.mouseX - cell.X;
                float dy = router.mouseY - cell.Y;
                float d = (float)Math.Sqrt(dx * dx + dy * dy);
                if (d < 1)
                {
                    dx = 1;
                    dy = 0;
                    d = 1;
                }
                else
                {
                    dx /= d; 
                    dy /= d;
                }
                this.LaunchPlayerCell(cell, cell.Size / Misc.SQRT_2, new Boost
                {
                    dx = dx,
                    dy = dy,
                    d = Settings.playerSplitBoost
                });
            }
        }
        public void EjectFromPlayer(Player player)
        {
            var dispersion = Settings.ejectDispersion;
            var loss = Settings.ejectingLoss * Settings.ejectingLoss;
            var router = player.router;
            var l = player.ownedCells.Count;
            for (int i = 0; i < l; i++)
            {
                var cell = player.ownedCells[i];
                if (cell.Size < Settings.playerMinEjectSize)
                    continue;
                var dx = router.mouseX - cell.X;
                var dy = router.mouseY - cell.Y;
                var d = (float)Math.Sqrt(dx * dx + dy * dy);
                if (d < 1) { dx = 1; dy = 0; d = 1; }
                else { dx /= d; dy /= d; }
                var sx = cell.X + dx * cell.Size;
                var sy = cell.Y + dy * cell.Size;
                var random = new Random();
                var newCell = new EjectedCell(this, player, sx, sy, cell.Color);
                var a = (float)Math.Atan2(dx, dy) - dispersion + random.NextDouble() * 2 * dispersion;
                newCell.boost.dx = (float)Math.Sin(a);
                newCell.boost.dy = (float)Math.Cos(a);
                newCell.boost.d = Settings.ejectedCellBoost;
                this.AddCell(newCell);
                this.SetCellAsBoosting(newCell);
                cell.SquareSize -= loss;
                this.UpdateCell(cell);
            }
        }
        public void PopPlayerCell(PlayerCell cell)
        {

        }
        public float[] DistributeCellMass(PlayerCell cell)
        {
            return null;
        }
        public void CompileStatistics()
        {
            stats = new WorldStats();
            int _internal = 0, external = 0, playing = 0, spectating = 0;
            for (int i = 0, l = this.players.Count; i < l; i++)
            {
                var player = this.players[i];
                if (!player.router.IsExternal) 
                { 
                    _internal++; 
                    continue; 
                }
                external++;
                if (player.state == 0) 
                    playing++;
                else if (player.state == PlayerState.Spectating || player.state == PlayerState.Roaming)
                    spectating++;
            }
            this.stats.limit = Settings.listenerMaxConnections - this.handle.listener.connections.Count + external;
            this.stats._internal = _internal;
            this.stats.external = external;
            this.stats.playing = playing;
            this.stats.spectating = spectating;
            this.stats.name = Settings.serverName;
            this.stats.gamemode = this.handle.gamemode.Name;
            this.stats.loadTime = this.handle.avargateTickTime / this.handle.stepMult;
            this.stats.uptime = (long)Math.Floor((double)((DateTime.Now.Ticks - this.handle.startTime.Value.Ticks) / 1000));
        }
            
    }
}
