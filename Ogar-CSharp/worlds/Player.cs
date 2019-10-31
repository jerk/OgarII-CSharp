using Ogar_CSharp.sockets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ogar_CSharp.worlds
{
    public class Player
    {
        public enum PlayerState
        {
            Dead = -1,
            Alive = 0,
            Spectating = 1,
            FreeRoamSpectating = 2
        }
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
        public PlayerState state = PlayerState.Dead;
        public bool hasWorld;
        public World world;
        //public Team team
        public long score;
        //public PlayerCell[] ownedCells;
        //public confusion visibleCells;
        //public confusion lastVisibleCells;
        //public ViewArea viewArea = unknown;
        public Settings Settings => handle.Settings;
        public Player(ServerHandle handle, short id, Router router)
        {
            this.handle = handle;
            this.id = id;
            this.router = router;
            exists = true;
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
                case PlayerState.Dead:
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
                case PlayerState.FreeRoamSpectating:
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
