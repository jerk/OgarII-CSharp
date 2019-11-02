using Ogar_CSharp.cells;
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
        public int id;
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
        public float score;
        public List<PlayerCell> ownedCells = new List<PlayerCell>();
        public Dictionary<int, Cell> visibleCells = new Dictionary<int, Cell>();
        public Dictionary<int, Cell> lastVisibleCells = new Dictionary<int, Cell>();
        public ViewArea viewArea;
        public Settings Settings => handle.Settings;
        public Player(ServerHandle handle, int id, Router router)
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
                this.state = PlayerState.Idle;
            else if (ownedCells.Count > 0)
                this.state = PlayerState.Alive;
            else if (state == PlayerState.Idle)
                this.state = PlayerState.Idle;
            else if (this.world.largestPlayer == null)
                this.state = PlayerState.Roaming;
            else if (this.state == PlayerState.Spectating && state == PlayerState.Roaming)
                this.state = PlayerState.Roaming;
            else
                this.state = PlayerState.Spectating;
        }
        public void UpdateViewArea()
        {
            if (world == null)
                return;
            float s = 0;
            switch (state)
            {
                case PlayerState.Idle:
                    this.score = 0;
                    break;
                case PlayerState.Alive:
                    float x = 0, y = 0, score = 0;
                    s = 0;
                    var l = ownedCells.Count;
                    for (int i = 0; i < l; i++)
                    {
                        var cell = this.ownedCells[i];
                        x += cell.X;
                        y += cell.Y;
                        s += cell.Size;
                        score += cell.Mass;
                    }
                    viewArea.x = x / l;
                    viewArea.y = y / 2;
                    this.score = score;
                    s = viewArea.s = (float)Math.Pow(Math.Min(64 / s, 1), 0.4);
                    viewArea.w = 1920 / s / 2 * Settings.playerViewScaleMult;
                    viewArea.h = 1080 / s / 2 * Settings.playerViewScaleMult;
                    break;
                case PlayerState.Spectating:
                    score = float.NaN;
                    var spectating = world.largestPlayer;
                    viewArea.x = spectating.viewArea.x;
                    viewArea.y = spectating.viewArea.y;
                    viewArea.s = spectating.viewArea.s;
                    viewArea.w = spectating.viewArea.w;
                    viewArea.h = spectating.viewArea.h;
                    break;
                case PlayerState.Roaming:
                    this.score = float.NaN;
                    float dx = this.router.mouseX - this.viewArea.x;
                    float dy = this.router.mouseY - this.viewArea.y;
                    float d = (float)Math.Sqrt(dx * dx + dy * dy);
                    float D = (float)Math.Min(d, Settings.playerRoamSpeed);
                    if (D < 1) break; 
                    if (D < 1) break; 
                    dx /= d; 
                    dy /= d;
                    var border = this.world.border;
                    viewArea.x = Math.Max(border.x - border.w, Math.Min(this.viewArea.x + dx * D, border.x + border.w));
                    viewArea.y = Math.Max(border.y - border.h, Math.Min(this.viewArea.y + dy * D, border.y + border.h));
                    s = this.viewArea.s = Settings.playerRoamViewScale;
                    this.viewArea.w = 1920 / s / 2 * Settings.playerViewScaleMult;
                    this.viewArea.h = 1080 / s / 2 * Settings.playerViewScaleMult;
                    break;
            }
        }
        public void UpdateVisibleCells()
        {
            if (world == null)
                return;
            lastVisibleCells = null;
            this.visibleCells.Clear();
            for (int i = 0, l = this.ownedCells.Count; i < l; i++)
            {
                var cell = this.ownedCells[i];
                if (!visibleCells.ContainsKey(cell.id))
                    visibleCells.Add(cell.id, cell);
                else
                    visibleCells[cell.id] = cell;
            }
            this.world.finder.Search(new Rect(viewArea.x, viewArea.y, viewArea.w, viewArea.h), (cell) =>
            {
                if (!visibleCells.ContainsKey(cell.item.id))
                    visibleCells.Add(cell.item.id, cell.item);
                else
                    visibleCells[cell.item.id] = cell.item;
            });
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
            int disposeDelay = Settings.worldPlayerDisposeDelay;
            if (disposeDelay > 0 && handle.tick - router.disconnectionTick >= disposeDelay)
                handle.RemovePlayer(id);
        }
    }
}
