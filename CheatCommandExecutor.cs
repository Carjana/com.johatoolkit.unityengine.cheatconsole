using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace SimpleUnityCheatConsole
{
    public class CheatCommandExecutor
    {
        private readonly ReflectionHelper _reflectionHelper = new();
        public Dictionary<string, BaseCheatCommand> CheatCommands { get; } = new();

        public CheatCommandExecutor()
        {
            CheatCommands.Add("help", new ZeroParameterCheatCommand("help", "Print All possible Commands", PrintHelp));
            GenerateCheatCommandsList();
        }

        private void PrintHelp()
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

                Debug.Log(stringBuilder.ToString());
            }
        }

        private void GenerateCheatCommandsList()
        {
            (MethodInfo, CheatCommandAttribute)[] methodInfos = _reflectionHelper.GetMethodInfos();
            foreach ((MethodInfo, CheatCommandAttribute) reflectionInfo in methodInfos)
            {
                if (!reflectionInfo.Item1.IsStatic)
                {
                    Debug.LogWarning(
                        $"CheatMethod {reflectionInfo.Item1.DeclaringType}.{reflectionInfo.Item1.Name} must be static!");
                    continue;
                }

                MethodInfoCheatCommand cheatCommand = new(reflectionInfo.Item2.CommandName ?? reflectionInfo.Item1.Name,
                    reflectionInfo.Item2.Description, reflectionInfo.Item1);

                if (CheatCommands.ContainsKey(cheatCommand.CommandName))
                {
                    Debug.LogWarning(
                        $"{cheatCommand.CommandName} already exists in the cheat commands list! Check the command names! ({cheatCommand.MethodInfo.DeclaringType}.{cheatCommand.MethodInfo.Name})");
                    return;
                }

                CheatCommands.Add(cheatCommand.CommandName, cheatCommand);
            }
        }

        public bool IsValidCommandName(string commandName) => CheatCommands.ContainsKey(commandName);

        public bool IsValidCommand(string command)
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

        private bool IsValidCommandParams(string[] parameters, Type[] parameterTypes)
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

        public void Execute(string command)
        {
            if (!IsValidCommand(command))
            {
                Debug.LogWarning($"Invalid Command! {command} (Check Params)");
                return;
            }

            string[] commandParts = command.Split(' ').Where(part => part.Trim() != "").ToArray();

            BaseCheatCommand baseCheatCommand = CheatCommands.GetValueOrDefault(commandParts.First());

            if (baseCheatCommand is MethodInfoCheatCommand methodInfoCheatCommand)
            {
                ExecuteCheatCommand(commandParts, methodInfoCheatCommand);
            }
            else
            {
                baseCheatCommand.Execute();
            }
        }

        private void ExecuteCheatCommand(string[] commandParts, MethodInfoCheatCommand methodInfoCheatCommand)
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

        private string GetParameterTypeName(Type parameterType) =>
            parameterType == typeof(float) ? "float" : parameterType.Name;

        public BaseCheatCommand[] GetPossibleCommands(string command)
        {
            string[] commandParts = command.Split(' ').Where(part => part.Trim() != "").ToArray();

            if (commandParts.Length == 0)
                return CheatCommands.Values.ToArray();

            BaseCheatCommand[] possibleCommandsByName = CheatCommands.Values
                .Where(cheatCommand => cheatCommand.CommandName.StartsWith(commandParts.First())).ToArray();
            return possibleCommandsByName;
        }
    }

}