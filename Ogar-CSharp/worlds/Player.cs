﻿using Ogar_CSharp.Cells;
using Ogar_CSharp.Sockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace Ogar_CSharp.Worlds
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
        public uint Id { get; }
        public Router router;
        public bool exists;
        public string leaderBoardName;
        public string cellName;
        public string chatName = "Spectator";
        public string cellSkin;
        public uint cellColor = 0x7F7F7F;
        public uint chatColor = 0x7F7F7F;
        public PlayerState currentState = PlayerState.Idle;
        public bool hasWorld;
        public World world;
        public int? team;
        public float score = float.NaN;
        public List<PlayerCell> ownedCells = new List<PlayerCell>();
        public Dictionary<uint, Cell> visibleCells = new Dictionary<uint, Cell>(150);
        public Dictionary<uint, Cell> lastVisibleCells = new Dictionary<uint, Cell>(150);
        public ViewArea viewArea;
        public Settings Settings => handle.Settings;
        public Player(ServerHandle handle, uint id, Router router)
        {
            this.handle = handle;
            Id = id;
            this.router = router;
            exists = true;
            viewArea = new ViewArea(0, 0, 1920 / 2 * handle.Settings.playerViewScaleMult, 1080 / 2 * handle.Settings.playerViewScaleMult, 1);
        }
        public void Destroy()
        {
            if (hasWorld)
                world.RemovePlayer(this);
            exists = false;
        }
        public void UpdateState(PlayerState targetState)
        {
            if (world == null)
                currentState = PlayerState.Idle;
            else if (ownedCells.Count > 0)
                currentState = PlayerState.Alive;
            else if (targetState == PlayerState.Idle)
                currentState = PlayerState.Idle;
            else if (world.largestPlayer == null)
                currentState = PlayerState.Roaming;
            else if (currentState == PlayerState.Spectating && targetState == PlayerState.Roaming)
                currentState = PlayerState.Roaming;
            else
                currentState = PlayerState.Spectating;
        }
        public void UpdateViewArea()
        {
            if (world == null)
                return;
            float s;
            switch (currentState)
            {
                case PlayerState.Idle:
                    this.score = float.NaN;
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
                    viewArea.y = y / l;
                    this.score = score;
                    s = viewArea.s = MathF.Pow(MathF.Min(64 / s, 1), 0.4f);
                    viewArea.w = 1920 / s / 2 * Settings.playerViewScaleMult;
                    viewArea.h = 1080 / s / 2 * Settings.playerViewScaleMult;
                    break;
                case PlayerState.Spectating:
                    this.score = float.NaN;
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
                    float d = (float)MathF.Sqrt(dx * dx + dy * dy);
                    float D = (float)MathF.Min(d, Settings.playerRoamSpeed);
                    if (D < 1) break;
                    dx /= d;
                    dy /= d;
                    var border = this.world.border;
                    viewArea.x = MathF.Max(border.X - border.Width, MathF.Min(this.viewArea.x + dx * D, border.X + border.Width));
                    viewArea.y = MathF.Max(border.Y - border.Height, MathF.Min(this.viewArea.y + dy * D, border.Y + border.Height));
                    s = this.viewArea.s = Settings.playerRoamViewScale;
                    this.viewArea.w = 1920 / s / 2 * Settings.playerViewScaleMult;
                    this.viewArea.h = 1080 / s / 2 * Settings.playerViewScaleMult;
                    break;
            }
        }
        public void UpdateCell(Cell cell, bool isVisible)
        {
            if (isVisible)
            {
                visibleCells[cell.Id] = cell;
            }
            else
            {
                lastVisibleCells[cell.Id] = cell;
            }
        }
        public void UpdateVisibleCells()
        {
            if (world == null)
                return;
            lastVisibleCells = visibleCells;
            visibleCells = new Dictionary<uint, Cell>(150); //have an initial capacity for better perfomance.
            for (int i = 0; i < ownedCells.Count; i++)
                UpdateCell(ownedCells[i], true);
            world.finder.Search(new RectangleF(viewArea.x, viewArea.y, viewArea.w, viewArea.h),
                (cell) =>
                {
                    visibleCells[cell.Id] = cell;
                });
        }
        public void CheckExistence()
        {
            if (!router.disconnected)
                return;
            if(currentState != PlayerState.Alive)
            {
                handle.RemovePlayer(Id);
                return;
            }
            int disposeDelay = Settings.worldPlayerDisposeDelay;
            if (disposeDelay > 0 && handle.tick - router.disconnectionTick >= disposeDelay)
                handle.RemovePlayer(Id);
        }
    }
}
