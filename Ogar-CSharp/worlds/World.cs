using Ogar_CSharp.primitives;
using Spatial;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ogar_CSharp.worlds
{
    public class World
    {
        public int id;
        public ServerHandle handle;
        public bool frozen;
        public int _nextCellId = 1;
        public HashSet<cells.Cell> cells = new HashSet<cells.Cell>();
        public HashSet<cells.Cell> boostingCells = new HashSet<cells.Cell>();
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
        public World(ServerHandle handle, int id)
        {
            this.handle = handle;
            this.id = id;

        }
    }
}
