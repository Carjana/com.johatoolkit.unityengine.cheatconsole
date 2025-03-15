using System;
using JetBrains.Annotations;

namespace SimpleUnityCheatConsole
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CheatCommandAttribute : Attribute
    {
        [CanBeNull] public string CommandName { get; private set; }
        [CanBeNull] public string Description { get; private set; }

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