using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ogar_CSharp.Commands
{
    public delegate void ExecuteCommand<T>(ServerHandle handle, T context, params string[] args);
    public class Command<T>
    {
        public Command(string name, string description, string args, ExecuteCommand<T> executer)
        {
            Name = name;
            Description = description;
            Executer = executer;
            Args = args;
        }
        public readonly string Name;
        public readonly string Description;
        public readonly string Args;
        public readonly ExecuteCommand<T> Executer;
        public static Command<T> GenerateCommand(string args, string description, string name, ExecuteCommand<T> exec)
            => new Command<T>(name, description, name, exec);
    }
    /*
    public class CommandList<T>
    {
        private ServerHandle handle;
        private Dictionary<string, Command<T>> list = new Dictionary<string, Command<T>>();
        public void Register(params Command<T>[] commands)
        {
            for(int i = 0; i < commands.Length; i++)
            {
                var command = commands[i];
                if (list.ContainsKey(command.Name))
                    throw new Exception("command conflicts with another already registered one");
                list.Add(command.Name, command);
            }
        }
        public bool Execute(T context, string input)
        {
            var split = input.Split(" ");
            if (split.Length == 0)
                return false;
            if (list.TryGetValue(split[0].ToLower(), out Command<T> command)) {
                command.Executer(handle, context, split.Skip(1).ToArray());
                return true;
            }
            else
                return false;
        }
    }*/
}
