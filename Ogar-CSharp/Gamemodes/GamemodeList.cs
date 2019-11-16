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
        private static readonly Dictionary<string, Func<ServerHandle, Gamemode>> gamemodes = new Dictionary<string, Func<ServerHandle, Gamemode>>();
        /// <summary>
        /// Register new gamemodes.
        /// </summary>
        /// <param name="newGamemodes">Gamemode types to be passed</param>
        public static void RegisterGamemodes(params Type[] newGamemodes)
        {

            for (int i = 0; i < newGamemodes.Length; i++)
            {
                var gamemode = newGamemodes[i];
                gamemodes.Add(gamemode.Name.ToLower(), (x) => (Gamemode)Activator.CreateInstance(gamemode, new object[] { x }));
            }
        }
        /// <summary>
        /// Gets a gamemode based off of its type.
        /// </summary>
        /// <typeparam name="T">The gamemode type.</typeparam>
        /// <param name="handle"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Gets a gamemode bast off of its name, it is not case sensitive.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="name"></param>
        /// <returns></returns>
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
