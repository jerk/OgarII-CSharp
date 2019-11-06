using System;
using System.Collections.Generic;
using System.Text;
using Ogar_CSharp.Sockets;
using Ogar_CSharp.Worlds;
using Ogar_CSharp.Cells;
namespace Ogar_CSharp.Gamemodes
{
    public abstract class Gamemode
    {
        public ServerHandle handle;
        public Gamemode(ServerHandle handle) => this.handle = handle;
        public abstract byte Type { get; }
        public abstract string Name { get; }
        public virtual void OnHandleStart() { }
        public virtual void OnHandleStop() { }
        public virtual void OnHandleTick() { }

        public virtual bool CanJoinWorld(World world)
            => !world.frozen;
        public virtual void OnPlayerJoinWorld(Player player, World world) { }
        public virtual void OnPlayerLeaveWorld(Player player, World world) { }
        public virtual void OnNewWorld(World world) { }
        public virtual void OnWorldTick(World world) { }
        public abstract void CompileLeaderboard(World world);
        public abstract void SendLeaderboard(Connection connection);
        public virtual void OnWorldDestroy(World world) { }
        public virtual void OnNewPlayer(Player player) { }
        public virtual void WhenPlayerPressQ(Player player) 
        {
            if (!player.hasWorld)
                return;
            player.UpdateState(PlayerState.Roaming);
        }
        public virtual void WhenPlayerEject(Player player) 
        {
            if (!player.hasWorld)
                return;
            player.world.EjectFromPlayer(player);
        }
        public virtual void WhenPlayerSplit(Player player) 
        {
            if (!player.hasWorld)
                return;
            player.world.SplitPlayer(player);
        }
        public abstract void OnPlayerSpawnRequest(Player player, string name, string skin);
        public virtual void OnPlayerDestroy(Player player) { }
        public virtual void OnNewCell(Cell cell) { }
        public virtual bool CanEat(Cell a, Cell b) => true;
        public virtual float GetDecayMult(PlayerCell cell) => cell.world.Settings.playerDecayMult;
        public virtual void OnCellRemove(Cell cell) { }
    }
}
