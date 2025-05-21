using System;
using UnityEngine;

namespace DebuggingEssentials
{
    using System.Reflection;

    [ConsoleAlias("console")]
    public partial class RuntimeConsole
    {
        [ConsoleCommand("", "Clears all logs in the Console Window")]
        static void Clear()
        {
            for (int i = 0; i < logs.Length; i++)
            {
                logs[i].cullItems.Clear();
            }

            instance.CalcDraw(true);

            instance.windowData.logIcon.count = 0;
            instance.windowData.warningIcon.count = 0;
            instance.windowData.errorIcon.count = 0;
            instance.windowData.exceptionIcon.count = 0;
        }

        [ConsoleCommand("", "Change the height of the Console Window based on line height")]
        static void Lines(int count = 20) { instance.consoleWindow.rect.height = 25 * count; }

        [ConsoleCommand("", "Open the Console Window")]
        static void Open() { SetActive(true); }

        [ConsoleCommand("", "Close the Console Window")]
        static void Close() { SetActive(false); }

        [ConsoleCommand("", "Unload unused Assets")]
        static void UnloadUnusedAssets() { Resources.UnloadUnusedAssets(); }

        [ConsoleCommand("", "Runs Garbage Collect")]
        static void GarbageCollect() { GC.Collect(); }

        [ConsoleCommand("", "Search for methods in assemblies")]
        static void SearchMethod(string name)
        {
            FastList<CustomAssembly> customAssemblies = RuntimeInspector.customAssemblies;

            const BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Default | BindingFlags.FlattenHierarchy | BindingFlags.DeclaredOnly;

            for (int i = 0; i < customAssemblies.Count; i++)
            {
                var customAssembly = customAssemblies.items[i];
                var allTypes = customAssembly.allTypes;

                if (customAssembly.type != AssemblyType.Unity) continue;

                for (int j = 0; j < allTypes.Count; j++)
                {
                    Type type = allTypes.items[j].type;

                    MethodInfo[] methods = type.GetMethods(bindingFlags);

                    for (int k = 0; k < methods.Length; k++)
                    {
                        MethodInfo method = methods[k];

                        string methodName = method.ToString();
                        if (methodName.IndexOf(name, StringComparison.CurrentCultureIgnoreCase) != -1)
                        {
                            Log(customAssembly.name + "." + type.Name + "." + methodName);
                        }
                    }
                }
            }
        }
    }
}
