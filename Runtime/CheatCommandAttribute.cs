using System;

namespace JoHaToolkit.UnityEngine.CheatConsole
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CheatCommandAttribute : Attribute
    {
        public string CommandName { get; private set; }
        public string Description { get; private set; }

        public CheatCommandAttribute()
        {
            CommandName = null;
            Description = null;
        }

        public CheatCommandAttribute(string commandName)
        {
            CommandName = commandName.Replace(" ", string.Empty);
            Description = null;
        }

        public CheatCommandAttribute(string commandName, string description)
        {
            CommandName = commandName.Replace(" ", string.Empty);
            Description = description;
        }
    }

}