using Ogar_CSharp.primitives;
using Spatial;
using System;
using System.Collections.Generic;
using System.Text;
using Ogar_CSharp.cells;
namespace Ogar_CSharp.worlds
{
    public class World
    {
        public class WorldStats
        {
            public int Limit;
            public int _internal;
            public int external;
            public int playing;
            public int spectating;
            public string name;
            //public string gamemode;
            public decimal uptime;
        }
        public struct Point
        {
            public int x;
            public int y;
            public Point(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }
        public int id;
        public ServerHandle handle;
        public bool frozen;
        public int _nextCellId = 1;
        public HashSet<cells.Cell> cells = new HashSet<Cell>();
        public HashSet<cells.Cell> boostingCells = new HashSet<Cell>();
        public int pelletCount;
        public int motherCellCount;
        public int virusCount;
        public HashSet<cells.Cell> ejectedCells = new HashSet<cells.Cell>();
        public HashSet<cells.PlayerCell> playerCells = new HashSet<cells.PlayerCell>();
        public HashSet<Player> players = new HashSet<Player>();
        public Player largestPlayer;
        //public chatchannel worldchat
        public Rect2 border;
        public QuadTree<cells.Cell> finder;
        public Settings Settings => handle.Settings;
        public World(ServerHandle handle, int id)
        {
            this.handle = handle;
            this.id = id;
        }
        public void AfterCreation()
        {

        }
        public void Destroy()
        {

        }
        public void SetBorder(Rect2 range)
        {

        }
        public void AddCell(Cell cell)
        {

        }
        public void SetCellAsBoosting(Cell cell)
        {

        }
        public void SetCellAsNotBoosting(Cell cell)
        {

        }
        public void UpdateCell(Cell cell)
        {

        }
        public void RemoveCell(Cell cell)
        {

        }
        public void AddPlayer(Player player)
        {

        }
        public void RemovePlayer(Player player)
        {

        }
        public (int x, int y) GetRandomPos(int cellSize)
        {
            var random = new System.Random();
            return ((int)(border.X - border.Width + cellSize + random.NextDouble() * (2 * this.border.Width - cellSize)),
                (int)(border.Y - border.Height + cellSize + random.NextDouble() * (2 * border.Height - cellSize)));
        }
        public bool IsSafeSpawnPos(Rect2 range)
        {

        }
        public (int x, int y) GetSafeSpawnPos(int cellSize)
        {
            var tries = this.Settings.worldSafeSpawnTries;
            while(--tries >= 0)
            {
                var pos = GetRandomPos(cellSize);
                if (IsSafeSpawnPos(new Rect2(pos.x, pos.y, cellSize, cellSize)))
                    return pos;
            }
            return GetRandomPos(cellSize);
        }
        public (int color, int x, int y) GetPlayerSpawn(int cellSize)
        {

        }
        public void SpawnPlayer(Player player, Point pos, int size)
        {
            var playerCell = new PlayerCell(player, pos.x, pos.y, size);
            AddCell(playerCell);
            player.UpdateState(Player.PlayerState.Alive);
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

        }
        public void FrozenUpdate()
        {

        }
        public void ResolveRigidCheck(Cell a, Cell b)
        {

        }
        public void ResolveEatCheck(Cell a, Cell b)
        {

        }
        public void BoostCell(Cell cell)
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
