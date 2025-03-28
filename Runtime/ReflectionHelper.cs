using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JoHaToolkit.UnityEngine.CheatConsole
{
    public class ReflectionHelper
    {
        public (MethodInfo, CheatCommandAttribute)[] GetMethodInfos(string[] assembliesToSearch, bool searchAllAssemblies = false)
        {
            List<(MethodInfo, CheatCommandAttribute)> methodInfos = new();
            Assembly[] assemblies = searchAllAssemblies? AppDomain.CurrentDomain.GetAssemblies() : AppDomain.CurrentDomain.GetAssemblies().Where(a => IsAssemblie(a.FullName, assembliesToSearch)).ToArray();

            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
                    foreach (MethodInfo methodInfo in type.GetMethods(flags))
                    {
                        if (methodInfo.CustomAttributes.ToArray().Length <= 0)
                            continue;

                        CheatCommandAttribute attribute = methodInfo.GetCustomAttribute<CheatCommandAttribute>();
                        if (attribute == null)
                            continue;
                        methodInfos.Add((methodInfo, attribute));
                    }
                }
            }

            return methodInfos.ToArray();
        }
        private bool IsAssemblie(string assemblyName, string[] assembliesToSearch)
        {
            return assembliesToSearch.Any(assemblyName.StartsWith);
        }
    }
    

}