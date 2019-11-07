using Newtonsoft.Json;
using System;
using System.IO;

namespace Ogar_CSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            Settings settings;
            if (!File.Exists("settings.json"))
                File.WriteAllText("settings.json", JsonConvert.SerializeObject(settings = new Settings(), Formatting.Indented));
            else
                settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));
            var handle = new ServerHandle(settings);
            handle.Start();
            Console.ReadKey();
        }
    }
}
