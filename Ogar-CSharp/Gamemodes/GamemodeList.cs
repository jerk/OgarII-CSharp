using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Ogar_CSharp.Gamemodes
{
    public static class GamemodeList
    {
        static GamemodeList()
        {
            RegisterGamemodes(typeof(FFA), typeof(Teams));
        }
        public static readonly Dictionary<string, Func<ServerHandle, Gamemode>> gamemodes = new Dictionary<string, Func<ServerHandle, Gamemode>>();
        public static void RegisterGamemodes(params Type[] newGamemodes)
        {

            for (int i = 0; i < newGamemodes.Length; i++)
            {
                var gamemode = newGamemodes[i];
                gamemodes.Add(gamemode.Name.ToLower(), (x) => (Gamemode)Activator.CreateInstance(gamemode, new object[] { x }));
            }
        }
        public static T GetGamemode<T>(this ServerHandle handle) where T : Gamemode
        {
            var gamemodename = typeof(T).Name;
            if (gamemodes.TryGetValue(gamemodename.ToLower(), out Func<ServerHandle, Gamemode> gamemode))
            {
                return (T)gamemode(handle);
            }
            else
                throw new Exception($"Gamemode '{gamemodename}' does not exist");
        }
        public static Gamemode GetGamemode(this ServerHandle handle, string name)
        {
            if (gamemodes.TryGetValue(name.ToLower(), out Func<ServerHandle, Gamemode> gamemode))
            {
                return gamemode(handle);
            }
            else
                throw new Exception($"Gamemode '{name}' does not exist");
        }
    }
}
