using Ogar_CSharp.cells;
using Ogar_CSharp.primitives;
using Ogar_CSharp.sockets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ogar_CSharp.worlds
{
    public enum PlayerState
    {
        Idle = -1,
        Alive = 0,
        Spectating = 1,
        Roaming = 2
    }
    public class Player
    {
        public ServerHandle handle;
        public short id;
        public Router router;
        public bool exists;
        public string leaderBoardName;
        public string cellName;
        public string chatName = "Spectator";
        public string cellSkin;
        public int cellColor = 0x7F7F7F;
        public int chatColor = 0x7F7F7F;
        public PlayerState state = PlayerState.Idle;
        public bool hasWorld;
        public World world;
        public string team; //CHANGE THIS WHEN POSSIBLE!!
        public long score;
        public List<PlayerCell> ownedCells = new List<PlayerCell>();
        public Dictionary<string, Cell> visibleCells = new Dictionary<string, Cell>();
        public Dictionary<string, Cell> lastVisibleCells = new Dictionary<string, Cell>();
        public ViewArea viewArea;
        public Settings Settings => handle.Settings;
        public Player(ServerHandle handle, short id, Router router)
        {
            this.handle = handle;
            this.id = id;
            this.router = router;
            exists = true;
            viewArea = new ViewArea(0, 0, 1920 / 2 * handle.Settings.playerViewScaleMult, 1080 / 2 * handle.Settings.playerViewScaleMult, 1);
        }
        public void Destroy()
        {
            //if(hasWorld)
            //world removePlayer
            exists = false;
        }
        public void UpdateState(PlayerState state)
        {
            if (world == null)
                return;
            int s = 0;
            switch (state)
            {
                case PlayerState.Idle:
                    this.score = 0;
                    break;
                case PlayerState.Alive:
                    int x = 0, y = 0, score = 0;
                    //int length = ownedCells.length;
                    //Dostuff
                    break;
                case PlayerState.Spectating:
                    //spectate largest player
                    //doStuff
                    break;
                case PlayerState.Roaming:
                    this.score = 0;
                    //doStuff
                    break;
            }
        }
        public void UpdateVisibleCells()
        {
            if (world == null)
                return;
            //dostuff
        }
        public void CheckExistence()
        {
            if (!router.disconnected)
                return;
            if(state != PlayerState.Alive)
            {
                handle.RemovePlayer(id);
                return;
            }
            int disposeDelay = 0; //settings.worldPlayerDisposeDelay;
            if (disposeDelay > 0 && handle.tick - router.disconnectionTick >= disposeDelay)
                handle.RemovePlayer(id);
        }
    }
}
