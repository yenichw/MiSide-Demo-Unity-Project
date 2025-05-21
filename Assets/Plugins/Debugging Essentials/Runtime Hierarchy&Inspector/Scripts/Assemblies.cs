using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DebuggingEssentials
{
    public enum AssemblyType { Unity, System, Mono, Other };

    public class Parent
    {
        public object parent;
    }

    public class CustomAssembly : IComparable<CustomAssembly>
    {
        public string name;
        public Assembly assembly;
        public AssemblyType type;
        public FastSortedDictionary<string, NamespaceTypes> namespaceLookup = new FastSortedDictionary<string, NamespaceTypes>(128);
        public FastList<CustomType> types = new FastList<CustomType>();
        public FastList<CustomType> allTypes = new FastList<CustomType>();

        public void Sort()
        {
            Array.Sort(types.items, 0, types.Count);
            namespaceLookup.Sort(CompareMode.Key);
        }

        public int CompareTo(CustomAssembly other)
        {
            return string.Compare(name, other.name);
        }

        static public void InitAssemblies(ref bool hasInitAssemblies, FastList<CustomAssembly> customAssemblies, Dictionary<NamespaceTypes, RuntimeInspector.DrawInfo> namespaceTypesLookup, Dictionary<string, FastList<Type>> typeNameLookup)
        {
            hasInitAssemblies = true;
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int i = 0; i < assemblies.Length; i++)
            {
                CustomAssembly customAssembly = new CustomAssembly();
                Assembly assembly = customAssembly.assembly = assemblies[i];
                customAssemblies.Add(customAssembly);

                string name = assembly.GetName().Name;
                customAssembly.name = name;

                bool isEditorAssembly = false;

                if (name.IndexOf("unity", StringComparison.OrdinalIgnoreCase) != -1) customAssembly.type = AssemblyType.Unity;
                else if (name.IndexOf("system", StringComparison.OrdinalIgnoreCase) != -1 || name == "mscorlib") customAssembly.type = AssemblyType.System;
                else if (name.IndexOf("mono", StringComparison.OrdinalIgnoreCase) != -1) customAssembly.type = AssemblyType.Mono;
                else
                {
                    customAssembly.type = AssemblyType.Other;
                    if (name == "Assembly-CSharp-Editor" || name == "Assembly-CSharp-Editor-firstpass") isEditorAssembly = true;
                }

                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types.Where(x => x != null).ToArray();
                }

                for (int j = 0; j < types.Length; j++)
                {
                    Type type = types[j];

                    if (customAssembly.type == AssemblyType.Other && !isEditorAssembly)
                    {
                        RuntimeConsole.RegisterStaticType(type);
                    }

                    CustomType customType = new CustomType(customAssembly, type);
                    customAssembly.allTypes.Add(customType);

                    string namespaceString = type.Namespace;

                    if (namespaceString == null)
                    {
                        customAssembly.types.Add(customType);
                    }
                    else
                    {
                        NamespaceTypes namespaceTypes;
                        if (!customAssembly.namespaceLookup.lookup.TryGetValue(namespaceString, out namespaceTypes))
                        {
                            namespaceTypes = new NamespaceTypes(customAssembly, namespaceString);
                            customAssembly.namespaceLookup.Add(namespaceString, namespaceTypes);
                        }

                        namespaceTypes.types.Add(new CustomType(namespaceTypes, type));
                    }

                    string typeName = type.Name;

                    FastList<Type> typeList;
                    if (!typeNameLookup.TryGetValue(typeName, out typeList))
                    {
                        if (typeList == null) typeList = new FastList<Type>();
                        typeNameLookup[typeName] = typeList;
                    }
                    typeList.Add(type);
                }
            }

            Array.Sort(customAssemblies.items, 0, customAssemblies.Count);

            for (int i = 0; i < customAssemblies.Count; i++)
            {
                customAssemblies.items[i].Sort();
            }

            RuntimeConsole.SortCommandsTable();
        }
    }

    public class NamespaceTypes : Parent, IComparable<NamespaceTypes>
    {
        public FastList<CustomType> types = new FastList<CustomType>();
        public string name;

        public NamespaceTypes(object parent, string name)
        {
            this.parent = parent;
            this.name = name;
        }

        public void Sort()
        {
            Array.Sort(types.items, 0, types.Count);
        }

        public int CompareTo(NamespaceTypes other)
        {
            return string.Compare(name, other.name);
        }
    }

    public class CustomType : Parent, IComparable<CustomType>
    {
        public string name;
        public Type type;

        public CustomType(object parent, Type type)
        {
            this.parent = parent;
            this.type = type;
            name = type.Name;
        }

        public int CompareTo(CustomType other)
        {
            return string.Compare(name, other.name);
        }
    }
}