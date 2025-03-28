using System;
using System.Linq;
using System.Reflection;

namespace JoHaToolkit.UnityEngine.CheatConsole
{
    public abstract class BaseCheatCommand
    {
        public string CommandName { get; set; }
        public string Description { get; set; }
        public Type[] ParameterTypes { get; protected set; }
        public string[] ParameterNames { get; protected set; }

        protected BaseCheatCommand(string commandName, string description)
        {
            CommandName = commandName;
            Description = description;
        }

        public abstract void Execute(object target = null, object[] parameters = null);
    }

    public class MethodInfoCheatCommand : BaseCheatCommand
    {
        public MethodInfo MethodInfo { get; }
        public ParameterInfo[] Parameters { get; }

        public MethodInfoCheatCommand(string commandName, string description, MethodInfo methodInfo) : base(commandName, description)
        {
            MethodInfo = methodInfo;
            Parameters = MethodInfo.GetParameters();
            ParameterTypes = Parameters.Select(parameter => parameter.ParameterType).ToArray();
            ParameterNames = Parameters.Select(parameter => parameter.Name).ToArray();
        }

        public override void Execute(object target, object[] parameters)
        {
            MethodInfo.Invoke(target, parameters);
        }
    }

    public class ZeroParameterCheatCommand : BaseCheatCommand
    {
        private readonly Action _action;

        public ZeroParameterCheatCommand(string commandName, string description, Action action) : base(commandName, description)
        {
            _action = action;
        }

        public override void Execute(object target = null, object[] parameters = null)
        {
            _action.Invoke();
        }
    }

}