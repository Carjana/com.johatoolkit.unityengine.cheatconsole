using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace JoHaToolkit.UnityEngine.CheatConsole
{
    public static class CheatCommandExecutor
    {
        private static readonly ReflectionHelper ReflectionHelper = new();
        public static Dictionary<string, BaseCheatCommand> CheatCommands { get; } = new();

        static CheatCommandExecutor()
        {
            CheatCommands.Add("help", new ZeroParameterCheatCommand("help", "Print All possible Commands", PrintHelp));
            GenerateCheatCommandsList();
        }

        private static void PrintHelp()
        {
            foreach (BaseCheatCommand cheatCommand in CheatCommands.Values)
            {
                StringBuilder stringBuilder = new();
                stringBuilder.Append($"{cheatCommand.CommandName} ");

                if (cheatCommand.ParameterNames != null && cheatCommand.ParameterTypes != null)
                    for (int i = 0; i < cheatCommand.ParameterNames.Length; i++)
                    {
                        stringBuilder.Append(
                            $"<{GetParameterTypeName(cheatCommand.ParameterTypes[i])}> {cheatCommand.ParameterNames[i]}");
                        if (i != cheatCommand.ParameterNames.Length - 1)
                            stringBuilder.Append(", ");
                    }

                stringBuilder.Append($" - {cheatCommand.Description ?? "<Missing Description>"}");

                DebugConsole.Instance.AddLog(stringBuilder.ToString(), Color.grey);
            }
        }

        private static void GenerateCheatCommandsList()
        {
            (MethodInfo, CheatCommandAttribute)[] methodInfos = ReflectionHelper.GetMethodInfos();
            foreach ((MethodInfo, CheatCommandAttribute) reflectionInfo in methodInfos)
            {
                if (!reflectionInfo.Item1.IsStatic)
                {
                    Debug.LogWarning($"CheatMethod {reflectionInfo.Item1.DeclaringType}.{reflectionInfo.Item1.Name} must be static!");
                    continue;
                }

                MethodInfoCheatCommand cheatCommand = new(reflectionInfo.Item2.CommandName ?? reflectionInfo.Item1.Name,
                    reflectionInfo.Item2.Description, reflectionInfo.Item1);

                if (CheatCommands.ContainsKey(cheatCommand.CommandName))
                {
                    Debug.LogWarning($"{cheatCommand.CommandName} already exists in the cheat commands list! Check the command names! ({cheatCommand.MethodInfo.DeclaringType}.{cheatCommand.MethodInfo.Name})");
                    return;
                }

                CheatCommands.Add(cheatCommand.CommandName, cheatCommand);
            }
        }

        public static bool IsValidCommandName(string commandName) => CheatCommands.ContainsKey(commandName);

        public static bool IsValidCommand(string command)
        {
            string[] commandParts = command.Split(' ').Where(part => part.Trim() != "").ToArray();
            if (commandParts.Length == 0)
                return false;

            if (!IsValidCommandName(commandParts.First()))
                return false;

            Type[] parameterTypes = CheatCommands.GetValueOrDefault(commandParts.First()).ParameterTypes;

            if (parameterTypes == null || parameterTypes.Length == 0)
                return commandParts.Length == 1;

            return IsValidCommandParams(commandParts.Skip(1).ToArray(), parameterTypes);
        }

        private static bool IsValidCommandParams(string[] parameters, Type[] parameterTypes)
        {
            if (parameters.Length != parameterTypes.Length)
                return false;

            for (int i = 0; i < parameters.Length; i++)
            {
                TypeConverter typeConverter = TypeDescriptor.GetConverter(parameterTypes[i]);
                if (!typeConverter.IsValid(parameters[i]))
                    return false;
            }

            return true;
        }

        public static void Execute(string command)
        {
            if (!IsValidCommand(command))
            {
                DebugConsole.Instance.AddLog($"Invalid Command! {command} (Check Params)", Color.yellow);
                return;
            }

            string[] commandParts = command.Split(' ').Where(part => part.Trim() != "").ToArray();

            BaseCheatCommand baseCheatCommand = CheatCommands.GetValueOrDefault(commandParts.First());
            try
            {

                if (baseCheatCommand is MethodInfoCheatCommand methodInfoCheatCommand)
                {
                    ExecuteCheatCommand(commandParts, methodInfoCheatCommand);
                }
                else
                {
                    baseCheatCommand.Execute();
                }

                DebugConsole.Instance.AddLog($"Executed Command \"{command}\" Successfully!", Color.green);
            }
            catch (Exception e)
            {
                DebugConsole.Instance.AddLog($"Failed to execute Command {baseCheatCommand.CommandName} \n{e}", Color.red);
                return;
            }
        }

        private static void ExecuteCheatCommand(string[] commandParts, MethodInfoCheatCommand methodInfoCheatCommand)
        {
            string[] inputParameters = commandParts.Skip(1).ToArray();

            object[] parameters = new object[inputParameters.Length];

            for (int index = 0; index < inputParameters.Length; index++)
            {
                Type parameterType = methodInfoCheatCommand.ParameterTypes[index];

                TypeConverter typeConverter = TypeDescriptor.GetConverter(parameterType);

                parameters[index] = typeConverter.ConvertFromString(inputParameters[index]);
            }

            methodInfoCheatCommand.Execute(null, parameters);
        }

        private static string GetParameterTypeName(Type parameterType) => parameterType == typeof(float) ? "float" : parameterType.Name;

        public static BaseCheatCommand[] GetPossibleCommands(string command)
        {
            string[] commandParts = command.Split(' ').Where(part => part.Trim() != "").ToArray();

            if (commandParts.Length == 0)
                return CheatCommands.Values.ToArray();

            BaseCheatCommand[] possibleCommandsByName = CheatCommands.Values.Where(cheatCommand => cheatCommand.CommandName.StartsWith(commandParts.First())).ToArray();
            return possibleCommandsByName;
        }
    }

}