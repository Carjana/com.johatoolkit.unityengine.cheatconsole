using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleUnityCheatConsole
{
    public class ReflectionHelper
    {
        public (MethodInfo, CheatCommandAttribute)[] GetMethodInfos()
        {
            List<(MethodInfo, CheatCommandAttribute)> methodInfos = new();

            Assembly assembly = Assembly.GetExecutingAssembly();
            Type[] types = assembly.GetTypes();

            foreach (Type type in types)
            {
                BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                     BindingFlags.Static;
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

            return methodInfos.ToArray();
        }
    }

}