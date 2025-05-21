/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
//using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static class Reflection
    {
        public const BindingFlags FullLookup = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
        public const BindingFlags InstanceLookup = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        public const BindingFlags StaticLookup = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        private const BindingFlags DefaultLookup = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

        private static Dictionary<Assembly, Type[]> cache;
        private static Assembly _editorAssembly;

        public static Assembly editorAssembly
        {
            get
            {
                if (_editorAssembly == null) _editorAssembly = Assembly.Load("UnityEditor");
                return _editorAssembly;
            }
        }

        public static bool CheckIfAnonymousType(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            return IsGenericType(type)
                   && (type.Name.Contains("AnonymousType") || type.Name.Contains("AnonType"))
                   && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                   && (GetAttributes(type) & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }

        public static Assembly GetAssembly(string name)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int i = 0; i < assemblies.Length; i++)
            {
                Assembly a = assemblies[i];
                if (a.GetName().Name == name)
                {
                    return a;
                }
            }

            return null;
        }

        public static TypeAttributes GetAttributes(Type type)
        {
#if !NETFX_CORE
            return type.Attributes;
#else
            return type.GetTypeInfo().Attributes;
#endif
        }

        public static Type GetEditorTypeFromAssembly(string name, string assemblyName, string @namespace = "UnityEditor")
        {
            Assembly assembly = GetAssembly(assemblyName);
            if (assembly == null) return null;
            return assembly.GetType(@namespace + "." + name);
        }

        public static Type GetEditorType(string name, string @namespace = "UnityEditor")
        {
            return editorAssembly.GetType(@namespace + "." + name);
        }

        public static FieldInfo GetField(Type type, string name, bool searchInParents, BindingFlags bindingAttr = InstanceLookup)
        {
            if (type == null) return null;

            FieldInfo[] fields = type.GetFields(bindingAttr);
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].Name == name) return fields[i];
            }

            if (searchInParents) return GetField(type.BaseType, name, true, bindingAttr);
            return null;
        }

        public static object GetFieldValue(Type type, string fieldName, object obj, BindingFlags bindingAttr = InstanceLookup)
        {
            FieldInfo field = type.GetField(fieldName, bindingAttr);
            if (field == null) return null;
            return field.GetValue(obj);
        }

        public static object GetFieldValue(object obj, string fieldName, BindingFlags bindingAttr = InstanceLookup)
        {
            FieldInfo field = obj.GetType().GetField(fieldName, bindingAttr);
            if (field == null) return null;
            return field.GetValue(obj);
        }

        public static T GetFieldValue<T>(Type type, string fieldName, object obj, BindingFlags bindingAttr = InstanceLookup)
        {
            FieldInfo field = type.GetField(fieldName, bindingAttr);
            if (field == null) return default(T);
            return (T)field.GetValue(obj);
        }

        public static IEnumerable<FieldInfo> GetFields(Type type, BindingFlags bindingAttr = DefaultLookup)
        {
#if !NETFX_CORE
            return type.GetFields(bindingAttr);
#else
            return type.GetTypeInfo().DeclaredFields;
#endif
        }

        public static Type[] GetGenericArguments(Type type)
        {
#if !NETFX_CORE
            return type.GetGenericArguments();
#else
            return type.GetTypeInfo().GenericTypeArguments;
#endif
        }

        public static List<T> GetInheritedItems<T>() where T: class
        {
            List<T> items = new List<T>();

            Type itemType = typeof(T);
            Assembly assembly = itemType.Assembly;

            GetInheritedItemsFromAssembly(assembly, items, itemType);

            string[] extraAssemblies =
            {
                "Assembly-CSharp-Editor-firstpass",
                "Assembly-CSharp-Editor",
            };

            foreach (string assemblyName in extraAssemblies)
            {
                try
                {
                    assembly = Assembly.Load(assemblyName);
                    if (assembly != null) GetInheritedItemsFromAssembly(assembly, items, itemType);
                }
                catch
                {

                }
            }

            if (typeof(T).GetInterface("ISortableLayoutItem") != null) items = items.OrderBy(i => (i as ISortableLayoutItem).order).ToList();
            return items;
        }

        private static void GetInheritedItemsFromAssembly<T>(Assembly assembly, List<T> items, Type itemType) where T : class
        {
            Type[] types;
            if (cache == null) cache = new Dictionary<Assembly, Type[]>();
            if (!cache.TryGetValue(assembly, out types)) types = assembly.GetTypes();

            if (types == null) return;

            foreach (Type type in types)
            {
                if (!type.IsAbstract && type.IsSubclassOf(itemType))
                {
                    items.Add(Activator.CreateInstance(type, true) as T);
                }
            }
        }

        public static List<Type> GetInheritedTypes<T>() where T : class
        {
            List<Type> items = new List<Type>();

            Type itemType = typeof(T);
            Assembly assembly = itemType.Assembly;

            GetInheritedTypesFromAssembly(assembly, items, itemType);

            string[] extraAssemblies =
            {
                "Assembly-CSharp-Editor-firstpass",
                "Assembly-CSharp-Editor",
            };

            foreach (string assemblyName in extraAssemblies)
            {
                try
                {
                    assembly = Assembly.Load(assemblyName);
                    if (assembly != null) GetInheritedTypesFromAssembly(assembly, items, itemType);
                }
                catch
                {

                }
            }

            return items;
        }

        private static void GetInheritedTypesFromAssembly(Assembly assembly, List<Type> items, Type itemType)
        {
            Type[] types;
            if (cache == null) cache = new Dictionary<Assembly, Type[]>();
            if (!cache.TryGetValue(assembly, out types)) types = assembly.GetTypes();

            if (types == null) return;

            foreach (Type type in types)
            {
                if (!type.IsAbstract && type.IsSubclassOf(itemType)) items.Add(type);
            }
        }

        public static IEnumerable<MemberInfo> GetMembers(Type type, BindingFlags bindingAttr = DefaultLookup)
        {
#if !NETFX_CORE
            return type.GetMembers(bindingAttr);
#else
            return type.GetTypeInfo().DeclaredMembers;
#endif
        }


        public static MethodInfo GetMethod(Type type, string name, BindingFlags bindingFlags = FullLookup)
        {
#if !NETFX_CORE
            return type.GetMethod(name, bindingFlags);
#else
            return type.GetTypeInfo().GetDeclaredMethod(name);
#endif
        }

        public static MethodInfo GetMethod(Type type, string name, Type[] types, BindingFlags bindingFlags = FullLookup)
        {
#if !NETFX_CORE
            return type.GetMethod(name, bindingFlags, null, types, null);
#else
            var methods = type.GetTypeInfo().GetDeclaredMethods(name);
            foreach(var m in methods)
            {
                var parms = m.GetParameters();
                if (parms != null && parms.Length == types.Length && parms[0].ParameterType == typeof(string))
                {
                    bool success = true;
                    for(int i = 0; i < parms.Length; i++)
                    {
                        if (parms[i].ParameterType != types[i])
                        {
                            success = false;
                            break;
                        }
                    }
                    if (success) return m;
                }
            }
            return null;
#endif
        }

        public static PropertyInfo GetProperty(Type type, string name, bool searchInParents, BindingFlags bindingAttr = InstanceLookup)
        {
            if (type == null) return null;

            PropertyInfo[] properties = type.GetProperties(bindingAttr);
            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i].Name == name) return properties[i];
            }

            if (searchInParents) return GetProperty(type.BaseType, name, true, bindingAttr);
            return null;
        }

        public static T GetPropertyValue<T>(object obj, string propertyName, BindingFlags bindingAttr = InstanceLookup)
        {
            PropertyInfo field = obj.GetType().GetProperty(propertyName, bindingAttr);
            if (field == null) return default(T);
            return (T)field.GetValue(obj, new object[0]);
        }

        public static T GetStaticFieldValue<T>(Type type, string fieldName, BindingFlags bindingAttr = StaticLookup)
        {
            FieldInfo field = type.GetField(fieldName, bindingAttr);
            if (field == null) return default(T);
            return (T)field.GetValue(null);
        }

        public static T GetStaticPropertyValue<T>(Type type, string propertyName, BindingFlags bindingAttr = StaticLookup)
        {
            PropertyInfo field = type.GetProperty(propertyName, bindingAttr);
            if (field == null) return default(T);
            return (T)field.GetValue(null, new object[0]);
        }

        public static object InvokeMethod(Type type, string name, object obj, BindingFlags bindingAttr = InstanceLookup)
        {
            MethodInfo method = type.GetMethod(name, bindingAttr);
            if (method != null) return method.Invoke(obj, new object[0]);
            return null;
        }

        public static object InvokeMethod(Type type, string name, object obj, Type[] types, object[] parameters, BindingFlags bindingAttr = InstanceLookup)
        {
            MethodInfo method = type.GetMethod(name, bindingAttr, null, types, null);
            if (method != null) return method.Invoke(obj, parameters);
            return null;
        }

        public static T InvokeStaticMethod<T>(Type type, string name, BindingFlags bindingAttr = StaticLookup)
        {
            return (T) InvokeStaticMethod(type, name, bindingAttr);
        }

        public static object InvokeStaticMethod(Type type, string name, BindingFlags bindingAttr = StaticLookup)
        {
            MethodInfo method = type.GetMethod(name, bindingAttr);
            if (method != null) return method.Invoke(null, new object[0]);
            return null;
        }

        public static object InvokeStaticMethod(Type type, string name, Type[] types, object[] parameters, BindingFlags bindingAttr = StaticLookup)
        {
            MethodInfo method = type.GetMethod(name, bindingAttr, null, types, null);
            if (method != null) return method.Invoke(null, parameters);
            return null;
        }

        public static bool IsGenericType(Type type)
        {
#if !NETFX_CORE
            return type.IsGenericType;
#else
            return type.GetTypeInfo().IsGenericType;
#endif
        }

        public static bool IsValueType(Type type)
        {
#if !NETFX_CORE
            return type.IsValueType;
#else
            return type.GetTypeInfo().IsValueType;
#endif
        }

        public static void SetFieldValue(object obj, string fieldName, object value, BindingFlags bindingAttr = InstanceLookup)
        {
            FieldInfo field = obj.GetType().GetField(fieldName, bindingAttr);
            if (field == null) return;
            field.SetValue(obj, value);
        }

        public static void SetPropertyValue(object obj, string propName, object value, BindingFlags bindingAttr = InstanceLookup)
        {
            PropertyInfo prop = obj.GetType().GetProperty(propName, bindingAttr);
            if (prop != null) prop.SetValue(obj, value);
        }

        public static void SetStaticFieldValue(Type type, string fieldName, object value, BindingFlags bindingAttr = StaticLookup)
        {
            FieldInfo field = type.GetField(fieldName, bindingAttr);
            if (field != null) field.SetValue(null, value);
        }
    }
}