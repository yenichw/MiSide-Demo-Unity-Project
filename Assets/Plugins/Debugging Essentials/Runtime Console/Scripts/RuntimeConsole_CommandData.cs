using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DebuggingEssentials
{
    public enum AccessLevel { Free, Admin, Special }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ConsoleAlias : System.Attribute
    {
        public string alias;

        public ConsoleAlias(string alias)
        {
            this.alias = alias.Trim('.', ' ');
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    public class ConsoleCommand : System.Attribute
    {
        public string alias = string.Empty;
        public string command = string.Empty;
        public string description = string.Empty;
        public AccessLevel accessLevel = AccessLevel.Admin;

        public ConsoleCommand(AccessLevel accessLevel = AccessLevel.Admin)
        {
            this.accessLevel = accessLevel;
        }

        public ConsoleCommand(string command, string description = "", AccessLevel accessLevel = AccessLevel.Admin)
        {
            int index = command.LastIndexOf('.');
            if (index != -1)
            {
                alias = command.Substring(0, index).Trim('.', ' ');
                command = command.Substring(index + 1);
            }

            // Debug.Log($"alias {alias} command {command}");

            this.command = command;
            this.description = description;
            this.accessLevel = accessLevel;
        }

        public bool HasAccess(AccessLevel accessLevel)
        {
            if (accessLevel == AccessLevel.Free && this.accessLevel != AccessLevel.Free) return false;
            else if (accessLevel == AccessLevel.Special && this.accessLevel == AccessLevel.Admin) return false;

            return true;
        }
    }

    public partial class RuntimeConsole
    {
        public class StringDictionarySorted
        {
            public Dictionary<string, CommandData> lookup = new Dictionary<string, CommandData>();
            public string[] names;

            public void Sort()
            {
                names = new string[lookup.Count];
                lookup.Keys.CopyTo(names, 0);
                Array.Sort(names);

                //for (int i = 0; i < names.Length; i++)
                //{
                //    Debug.Log(names[i]);
                //}
            }

            public void Clear()
            {
                lookup.Clear();
                names = null;
            }
        }

        public enum MemberType { Method, Property, Field, Delegate };

        public struct CommandData
        {
            public static CommandData empty;

            public ConsoleCommand consoleCommand;
            public object obj;
            public bool isStatic;
            public string syntax;
            public MemberType memberType;
            public MemberInfo member;
            public ParameterInfo[] paramInfos;
            object[] args;

            public CommandData(ConsoleCommand consoleCommand, object obj, string syntax, MemberType memberType, MemberInfo member, ParameterInfo[] paramInfos, bool isStatic)
            {
                this.consoleCommand = consoleCommand;
                this.obj = obj;
                this.syntax = syntax;
                this.memberType = memberType;
                this.member = member;
                this.isStatic = isStatic;

                if (paramInfos == null || (paramInfos != null && paramInfos.Length == 0))
                {
                    this.paramInfos = null;
                    args = null;
                }
                else
                {
                    this.paramInfos = paramInfos;
                    args = new object[paramInfos.Length];
                }
            }

            public object GetValue()
            {
                object obj;
                if (isStatic) obj = this.obj;
                else
                {
                    int instanceCount = GetInstanceWhenOnlyOne(out obj);
                    if (instanceCount == 0) return "null";
                    else if (instanceCount > 1) return ">";
                }

                if (memberType == MemberType.Method || memberType == MemberType.Delegate) return null;

                if (memberType == MemberType.Field)
                {
                    return ((FieldInfo)member).GetValue(obj);
                }
                if (memberType == MemberType.Property)
                {
                    var prop = (PropertyInfo)member;
                    MethodInfo method = prop.GetGetMethod(true);

                    if (method != null)
                    {
                        return method.Invoke(obj, null);
                    }
                }

                return null;
            }

            public bool IsRegistered()
            {
                if (isStatic) return true;

                HashSet<object> instances;
                registeredInstancesLookup.TryGetValue((Type)obj, out instances);

                return (instances != null && instances.Count > 0);
            }

            public int GetInstanceCount()
            {
                if (isStatic || obj == null) return -1;

                HashSet<object> instances;
                registeredInstancesLookup.TryGetValue((Type)obj, out instances);
                if (instances == null) return 0;
                CheckDestroyedMonoBehaviours(instances);

                return instances.Count;
            }

            public int GetInstanceWhenOnlyOne(out object instance)
            {
                HashSet<object> instances;
                registeredInstancesLookup.TryGetValue((Type)obj, out instances);

                if (instances == null) { instance = null; return 0; }
                CheckDestroyedMonoBehaviours(instances);
                if (instances.Count != 1) { instance = null; return instances.Count; }

                foreach (object newInstance in instances) { instance = newInstance; return 1; }
                instance = null;
                return 0;
            }

            public void Execute(FastQueue<string> arguments, string argumentString)
            {
                // Debug.Log(method.Name + " : " + paramInfos.Length);
                if (memberType == MemberType.Method || memberType == MemberType.Delegate)
                {
                    ExecuteMethodOrDelegate(arguments, argumentString);
                }
                else if (memberType == MemberType.Field)
                {
                    ExecuteField(arguments, argumentString);
                }
                else if (memberType == MemberType.Property)
                {
                    ExecuteProperty(arguments, argumentString);
                }
            }

            void ExecuteMethodOrDelegate(FastQueue<string> arguments, string argumentString)
            {
                if (paramInfos != null)
                {
                    int argumentsCount = arguments.Count;

                    for (int i = 0; i < paramInfos.Length; i++)
                    {
                        // Debug.Log(paramInfos[i].ParameterType.Name);
                        ParameterInfo paramInfo = paramInfos[i];

                        Type type = paramInfo.ParameterType;

                        if (i >= argumentsCount)
                        {
                            if (!paramInfo.IsOptional)
                            {
                                LogResultError("Can't execute because of wrong number of arguments");
                                return;
                            }
                            args[i] = paramInfo.DefaultValue;
                            continue;
                        }

                        // Debug.Log(type.Name + " " + type.IsPrimitive);

                        if (!Parser.TryParse(type, arguments, out args[i]))
                        {
                            return;
                        }
                    }
                }

                if (arguments.Count > 0)
                {
                    LogResultError("Too many arguments");
                    return;
                }

                if (memberType == MemberType.Method)
                {
                    if (isStatic) ExecuteMethod(member, (MethodInfo)member, obj, args, argumentString);
                    else
                    {
                        HashSet<object> instances = registeredInstancesLookup[(Type)obj];
                        CheckDestroyedMonoBehaviours(instances);
                        foreach (object instance in instances)
                        {
                            MonoBehaviour mono = instance as MonoBehaviour;
                            if (mono == null && instance.GetType().IsSubclassOf(typeof(MonoBehaviour))) Debug.Log("Mono = isDestroyed");
                            else ExecuteMethod(member, (MethodInfo)member, instance, args, argumentString);
                        }
                    }
                }
                else
                {
                    if (isStatic) ExecuteDelegateMethod(member, obj, args, argumentString);
                    else
                    {
                        HashSet<object> instances = registeredInstancesLookup[(Type)obj];
                        CheckDestroyedMonoBehaviours(instances);
                        foreach (object instance in instances) ExecuteDelegateMethod(member, instance, args, argumentString);
                    }
                }
            }

            void ExecuteDelegateMethod(MemberInfo member, object obj, object[] args, string argumentString)
            {
                object delegateValue = ((FieldInfo)member).GetValue(obj);
                if (delegateValue == null)
                {
                    LogResultError(obj.ToString() + " : " + member.Name + " delegate is not assigned");
                    return;
                }
                MethodInfo method = delegateValue.GetType().GetMethod("Invoke");
                ExecuteMethod(member, method, delegateValue, args, argumentString);
            }

            void ExecuteMethod(MemberInfo member, MethodInfo method, object obj, object[] args, string argumentString)
            {
                try
                {
                    object result = method.Invoke(obj, args);
                    if (result != null)
                    {
                        LogResult(obj.ToString() + " : " + member.Name + " " + argumentString + " = " + result);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            void ExecuteField(FastQueue<string> arguments, string argumentString)
            {
                var field = (FieldInfo)member;

                if (arguments.Count == 0)
                {
                    if (isStatic) LogValue(field, obj);
                    else
                    {
                        HashSet<object> instances = registeredInstancesLookup[(Type)obj];
                        CheckDestroyedMonoBehaviours(instances);
                        foreach (object instance in instances) LogValue(field, instance);
                    }
                }
                else
                {
                    object arg;
                    if (!Parser.TryParse(field.FieldType, arguments, out arg)) return;
                    if (arguments.Count > 0)
                    {
                        LogResultError("Too many arguments");
                        return;
                    }

                    if (isStatic) SetField(field, obj, arg);
                    else
                    {
                        HashSet<object> instances = registeredInstancesLookup[(Type)obj];
                        CheckDestroyedMonoBehaviours(instances);
                        foreach (object instance in instances) SetField(field, instance, arg);
                    }
                }
            }

            void LogValue(FieldInfo field, object obj)
            {
                try
                {
                    LogResult(obj.ToString() + " : " + field.Name + " = " + field.GetValue(obj));
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            void SetField(FieldInfo field, object obj, object arg)
            {
                try
                {
                    if (arg != null) field.SetValue(obj, arg);
                    LogResult(obj.ToString() + " : " + field.Name + " = " + field.GetValue(obj));
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            void ExecuteProperty(FastQueue<string> arguments, string argumentString)
            {
                var prop = (PropertyInfo)member;

                if (arguments.Count == 0)
                {
                    if (prop.GetGetMethod(true) == null)
                    {
                        LogResultError(prop.Name + " doesn't have a getter");
                        return;
                    }
                    if (isStatic) LogValue(prop, obj);
                    else
                    {
                        HashSet<object> instances = registeredInstancesLookup[(Type)obj];
                        CheckDestroyedMonoBehaviours(instances);
                        foreach (object instance in instances) LogValue(prop, instance);
                    }
                }
                else
                {
                    if (prop.GetSetMethod(true) == null)
                    {
                        LogResultError(prop.Name + " doesn't have a setter");
                        return;
                    }

                    object arg;
                    if (!Parser.TryParse(prop.PropertyType, arguments, out arg)) return;
                    if (arguments.Count > 0)
                    {
                        LogResultError("Too many arguments");
                        return;
                    }

                    if (isStatic) SetProperty(prop, obj, arg);
                    else
                    {
                        HashSet<object> instances = registeredInstancesLookup[(Type)obj];
                        CheckDestroyedMonoBehaviours(instances);
                        foreach (object instance in instances) SetProperty(prop, instance, arg);
                    }
                }
            }

            void LogValue(PropertyInfo prop, object obj)
            {
                try
                {
                    LogResult(obj.ToString() + " : " + prop.Name + " = " + prop.GetValue(obj, null));
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            void SetProperty(PropertyInfo prop, object obj, object arg)
            {
                try
                {
                    if (arg != null) prop.SetValue(obj, arg, null);
                    if (prop.GetGetMethod(true) != null)
                    {
                        LogResult(obj.ToString() + " : " + prop.Name + " = " + prop.GetValue(obj, null));
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            bool CheckParameterCount(int required, int count)
            {
                if (required == count) return true;

                LogResultError("Can't execute because " + required + " arguments are needed");
                return false;
            }
        }

        static void LogResult(string result)
        {
            Log(new LogEntry(result, null, LogType.Log, EntryType.CommandResult, Helper.colCommandResult, instance.logFontSize, FontStyle.Bold));
            if (HtmlDebug.instance) HtmlDebug.instance.UnityDebugLog(result, null, LogType.Log, true, -1, null, EntryType2.CommandResult);
        }

        static public void LogResultError(string result, bool onlyConsoleAndHtml = true)
        {
            if (onlyConsoleAndHtml)
            {
                Log(new LogEntry(result, null, LogType.Log, EntryType.CommandResult, Helper.colCommandResultFailed, instance.logFontSize, FontStyle.Bold));
                if (HtmlDebug.instance) HtmlDebug.instance.UnityDebugLog(result, null, LogType.Log, true, -1, null, EntryType2.CommandFault);
            }
            else Debug.LogError(result);
        }
    }
}