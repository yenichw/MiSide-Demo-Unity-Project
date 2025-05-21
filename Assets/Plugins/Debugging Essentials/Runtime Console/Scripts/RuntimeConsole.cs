using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DebuggingEssentials
{
    public delegate void SetActiveMethod(bool active);

    [DefaultExecutionOrder(-4000000)]
    public partial class RuntimeConsole : MonoBehaviour
    {
        public delegate void RemoteCommand(string command);
        public static event RemoteCommand onRemoteCommand;
        public static event SetActiveMethod onSetActive;

        public static RuntimeConsole instance;
        public static AccessLevel accessLevel;
        public static bool show;

        static Dictionary<Type, HashSet<object>> registeredInstancesLookup = new Dictionary<Type, HashSet<object>>();
        static StringDictionarySorted commandsTable = new StringDictionarySorted();
        static FastList<AutoComplete> autoCompleteList = new FastList<AutoComplete>();
        static FastQueue<string> inputCommands = new FastQueue<string>(32);
        static FastList<LogEntry> threadLogEntries = new FastList<LogEntry>(256);
        static FastList<LogEntry> logEntries = new FastList<LogEntry>(256);

        SortedFastList<string> commands = new SortedFastList<string>();
        
        const int logSize = 8192;
        
        const int commandLogs = 0;
        const int frameLogs = 1;
        const int unityLogs = 2;
        const int warningLogs = 3;
        const int errorLogs = 4;
        const int exceptionLogs = 5;

        static bool setFocus;
        static int lastFrame = -1;
        static LogEntry lastFrameEntry;

        WindowSettings consoleWindow;

        int commandIndex;
        int moveInputCursor;
        int autoCompleteIndex;
        string inputCommand;

        GUIChangeBool _showStack = new GUIChangeBool(true);
        GUIChangeBool showUnityLogs = new GUIChangeBool(true);
        GUIChangeBool showLastLog = new GUIChangeBool(true);

        Vector2 autoCompleteScrollView;
        Double2 scrollView;
        Double2 scrollViewHeights;

        CullGroup cullGroup;
        FastList<CullList> cullLists = new FastList<CullList>();
        static CullList[] logs;
        bool calcDraw;
        bool isEnabled;

        int oldAutoCompleteCount = -1;
        bool updateAutoCompleteScrollView;
#if !UNITY_EDITOR
        bool isDebugBuild;
#endif

        void ResetStatic()
        {
            onRemoteCommand = null;
            accessLevel = AccessLevel.Admin;
            show = false;
            // registeredInstancesLookup.Clear();
            // commandsTable.Clear();
            autoCompleteList.Clear();
            inputCommands.Clear();
            threadLogEntries.Clear();
            logEntries.Clear();
            commands.Clear();
            LogEntry.ResetStatic();
            logs = null;
        }

        void Awake()
        {
#if !UNITY_EDITOR
            isDebugBuild = Debug.isDebugBuild;
#endif
            ResetStatic();
            consoleWindow = windowData.consoleWindow;

            logs = new CullList[6];
            cullGroup = new CullGroup(logSize * logs.Length);
            
            for (int i = 0; i < logs.Length; i++)
            {
                logs[i] = new CullList(logSize);
            }

            SetActive(showConsoleOnStart);

            inputCommand = string.Empty;

            logs[commandLogs].cullItems.Add(new LogEntry("-------------------------------------------------------------------------------", Color.white, titleFontSize, FontStyle.Bold));
            logs[commandLogs].cullItems.Add(new LogEntry(Helper.GetApplicationInfo(), Color.white, titleFontSize, FontStyle.Bold));
            logs[commandLogs].cullItems.Add(new LogEntry("-------------------------------------------------------------------------------", Color.white, titleFontSize, FontStyle.Bold));
            logs[commandLogs].cullItems.Add(new LogEntry(string.Empty, Color.white, titleFontSize, FontStyle.Bold));
            logs[commandLogs].cullItems.Add(new LogEntry("Type '?' to list all commands", Color.white, titleFontSize, FontStyle.Bold));
            logs[commandLogs].cullItems.Add(new LogEntry(string.Empty, Color.white, titleFontSize, FontStyle.Bold));

            GUIChangeBool.ApplyUpdates();

            Register(this);

            CalcDraw(true);

#if UNITY_EDITOR
            if (testOnlyFreeConsoleCommands) accessLevel = AccessLevel.Free;
            else accessLevel = AccessLevel.Admin;
#else
            if (adminModeInBuild == AccessMode.Enabled) accessLevel = AccessLevel.Admin;
#endif
        }

        void OnEnable()
        {
            isEnabled = true;
        }

        void OnDisable()
        {
            isEnabled = false;
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
            Unregister(this);
        }

        public void ManualUpdate()
        {
#if !UNITY_EDITOR
            if (isDebugBuild && disableUnityDevelopmentConsole)
            {
                Debug.developerConsoleVisible = false;
            }
#endif
            UpdateLogs();
            if (isEnabled && EventInput.isMouseButtonUp0) consoleWindow.drag = 0;
        }

        public static void SetActive(bool active)
        {
            instance.consoleWindow.drag = 0;
            show = active;
            instance.gameObject.SetActive(active);

            if (show) setFocus = true;

            if (onSetActive != null) onSetActive.Invoke(show);

            WindowManager.CheckMouseCursorState();
        }

        public static void Log(string logString, bool showConsole)
        {
            if (instance == null) return;

            AddLog(new LogEntry(logString, null, LogType.Log, EntryType.Console, Color.white));

            if (showConsole && !show) SetActive(true);
        }

        public static void Log(string logString, Color color, bool showConsole)
        {
            if (instance == null) return;

            AddLog(new LogEntry(logString, null, LogType.Log, EntryType.Console, color));

            if (showConsole && !show) SetActive(true);
        }

        public static void Log(string logString, LogType logType = LogType.Log, Color color = default(Color), bool showConsole = false)
        {
            if (instance == null) return;

            AddLog(new LogEntry(logString, null, logType, EntryType.Console, color));

            if (showConsole && !show) SetActive(true);
        }

        public static void Log(string logString, string[] lines, LogType logType = LogType.Log, Color color = default(Color), int threadId = -1)
        {
            if (instance == null) return;

            AddLog(new LogEntry(logString, lines, logType, EntryType.Unity, color, instance.logFontSize, FontStyle.Normal, threadId));
        }

        public static void Log(LogEntry logEntry)
        {
            if (instance == null) return;

            AddLog(logEntry);
        }

        static void AddLog(LogEntry logEntry)
        {
            if (CheckNewFrame()) logEntry.id = LogEntry.currentId++;

            threadLogEntries.AddThreadSafe(logEntry);
        }

        void UpdateLogs()
        {
            logEntries.GrabListThreadSafe(threadLogEntries);

            for (int i = 0; i < logEntries.Count; i++)
            {
                LogEntry logEntry = logEntries.items[i];

                if (logEntry.entryType == EntryType.Unity)
                {
                    if (logEntry.logType == LogType.Log)
                    {
                        instance.windowData.logIcon.count++;
                        lastFrameEntry.flag |= (int)Flag.Log;
                        logs[unityLogs].cullItems.Add(logEntry);
                    }
                    else if (logEntry.logType == LogType.Warning)
                    {
                        if (instance.showConsoleOnWarning && !show) SetActive(true);
                        instance.windowData.warningIcon.count++;
                        lastFrameEntry.flag |= (int)Flag.Warning;
                        logs[warningLogs].cullItems.Add(logEntry);
                    }
                    else if (logEntry.logType == LogType.Error)
                    {
                        if (instance.showConsoleOnError && !show) SetActive(true);
                        instance.windowData.errorIcon.count++;
                        lastFrameEntry.flag |= (int)Flag.Error;
                        logs[errorLogs].cullItems.Add(logEntry);
                    }
                    else if (logEntry.logType == LogType.Exception)
                    {
                        if (instance.showConsoleOnException && !show) SetActive(true);
                        instance.windowData.exceptionIcon.count++;
                        lastFrameEntry.flag |= (int)Flag.Exception;
                        logs[exceptionLogs].cullItems.Add(logEntry);
                    }
                }
                else if (logEntry.entryType == EntryType.Frame)
                {
                    logs[frameLogs].cullItems.Add(lastFrameEntry);
                }
                else
                {
                    lastFrameEntry.flag |= (int)Flag.Command;
                    logs[commandLogs].cullItems.Add(logEntry);
                }
            }

            logEntries.Clear();

            instance.CalcDraw(false);
        }

        static bool CheckNewFrame()
        {
            if (HtmlDebug.currentFrame != lastFrame)
            {
                lastFrame = HtmlDebug.currentFrame;

                lastFrameEntry = new LogEntry("[Frame " + lastFrame.ToString("D6") + "][Time " + Helper.ToTimeFormat(HtmlDebug.frameTime) +
                    "] -----------------------------------------------------------------------------------------------",
                    null, LogType.Log, EntryType.Frame, new Color(0.31f, 0.55f, 0.63f), instance.frameFontSize, FontStyle.Bold);

                threadLogEntries.AddThreadSafe(lastFrameEntry);
                return true;
            }
            return false;
        }

        static public void SortCommandsTable()
        {
            commandsTable.Sort();
        }

        public static void Register(object instance)
        {
            if (!Application.isPlaying) return;

            Type type = instance.GetType();

            HashSet<object> instances;
            if (!registeredInstancesLookup.TryGetValue(type, out instances))
            {
                instances = new HashSet<object>();
                registeredInstancesLookup[type] = instances;
            }
            else if (instances.Contains(instance))
            {
                Debug.LogError("Instance " + type.Name + " is already registered");
                return;
            }

            instances.Add(instance);
        }

        public static void Unregister(object instance)
        {
            Type type = instance.GetType();

            HashSet<object> instances;
            registeredInstancesLookup.TryGetValue(type, out instances);

            if (instance == null || !instances.Contains(instance))
            {
                Debug.LogError("Instance " + type.Name + " is not registered");
                return;
            }

            instances.Remove(instance);
            if (instances.Count == 0) registeredInstancesLookup.Remove(type);
        }

        static public void RegisterStaticType(Type objType)
        {
            if (instance == null) return;

            var consoleAlias = (ConsoleAlias[])objType.GetCustomAttributes(typeof(ConsoleAlias), true);

            if (consoleAlias.Length > 0)
            {
                commandsTable.lookup[consoleAlias[0].alias] = CommandData.empty;
            }

            const BindingFlags registerBindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Default;// | BindingFlags.FlattenHierarchy;

            MemberInfo[] members = objType.GetMembers(registerBindingFlags);

            for (int i = 0; i < members.Length; i++)
            {
                MemberInfo member = members[i];

                var consoleCommands = (ConsoleCommand[])member.GetCustomAttributes(typeof(ConsoleCommand), false);

                if (consoleCommands.Length == 0) continue;

                ConsoleCommand consoleCommand = consoleCommands[0];

                if (consoleAlias.Length > 0) consoleCommand.alias = consoleAlias[0].alias + (consoleCommand.alias.Length > 0 ? "." + consoleCommand.alias : string.Empty);
                // Debug.Log("Register " + objType.ToString() + " " + members.Length);

                if (AddMethod(objType, consoleCommand, member, member as MethodInfo, MemberType.Method, member.Name)) continue;

                FieldInfo field = member as FieldInfo;
                if (field != null)
                {
                    Type type = field.FieldType;

                    if (typeof(Delegate).IsAssignableFrom(type))
                    {
                        AddMethod(objType, consoleCommand, member, type.GetMethod("Invoke"), MemberType.Delegate, field.Name);
                        continue;
                    }

                    if (!ValidParam(objType, field, type)) continue;

                    if (type.IsEnum && consoleCommand.description == string.Empty)
                    {
                        consoleCommand.description = string.Join(", ", Enum.GetNames(type));
                    }

                    string syntax = field.Name + " " + type.Name;
                    AddCommand(member, consoleCommand, member.Name, new CommandData(consoleCommand, objType, syntax, MemberType.Field, member, null, field.IsStatic));
                    continue;
                }

                PropertyInfo prop = member as PropertyInfo;
                if (prop != null)
                {
                    Type type = prop.PropertyType;

                    // Debug.Log(consoleCommand.alias + "." + consoleCommand.command);
                    if (!ValidParam(objType, prop, type)) continue;

                    MethodInfo getMethod = prop.GetGetMethod(true);
                    MethodInfo setMethod = prop.GetSetMethod(true);

                    bool hasGet = getMethod != null;
                    bool hasSet = setMethod != null;
                    bool isStatic;

                    if (hasGet) isStatic = getMethod.IsStatic;
                    else if (hasSet) isStatic = setMethod.IsStatic;
                    else
                    {
                        Debug.Log("Property has no getter or setter");
                        continue;
                    }

                    string syntax = prop.Name + " " + type.Name;
                    if (hasGet && !hasSet) syntax += " (get)";
                    else if (hasGet && hasSet) syntax += " (get set)";
                    else syntax += " (set)";

                    AddCommand(member, consoleCommand, member.Name, new CommandData(consoleCommand, objType, syntax, MemberType.Property, member, null, isStatic));
                    continue;
                }
            }
        }

        static bool AddMethod(Type objType, ConsoleCommand consoleCommand, MemberInfo member, MethodInfo method, MemberType memberType, string methodName, bool logFailed = true)
        {
            if (method != null)
            {
                ParameterInfo[] paramInfos = method.GetParameters();

                if (!ValidParams(objType, method, paramInfos)) return true;

                // Debug.Log("Add Method " + methodName);
                string syntax = method.ToString();
                AddCommand(member, consoleCommand, methodName, new CommandData(consoleCommand, objType, syntax, memberType, member, paramInfos, method.IsStatic), logFailed);
                return true;
            }
            return false;
        }

        static void AddCommand(MemberInfo member, ConsoleCommand consoleCommand, string commandName, CommandData commandData, bool logFailed = true)
        {
            if (instance.ignoreCasesInCommands) commandName = commandName.ToLower();

            if (consoleCommand.command == string.Empty) consoleCommand.command = commandName;
            else
            {
                commandName = consoleCommand.command;
                if (instance.ignoreCasesInCommands) commandName = commandName.ToLower();
            }

            if (consoleCommand.alias != string.Empty)
            {
                if (!commandsTable.lookup.ContainsKey(consoleCommand.alias))
                {
                    commandsTable.lookup[consoleCommand.alias] = CommandData.empty;
                }
                commandName = consoleCommand.alias + "." + consoleCommand.command;
            }

            // Debug.Log("=> " + commandName + " " + (commandData.paramInfos == null ? "null " : commandData.paramInfos.Length.ToString()));

            if (commandsTable.lookup.ContainsKey(commandName))
            {
                if (logFailed) Debug.LogError("Duplicate command: `" + commandName + "` on " + member.Name + " in " + commandData.obj.ToString() + " class ");
                return;
            }

            commandsTable.lookup[commandName] = commandData;
        }

        static public bool ValidParams(Type objType, MethodInfo method, ParameterInfo[] paramInfos, bool logFailed = true)
        {
            for (int i = 0; i < paramInfos.Length; i++)
            {
                ParameterInfo param = paramInfos[i];
                Type paramType = param.ParameterType;
                if (!ValidParam(objType, param.Member, paramType, logFailed)) return false;
            }

            return true;
        }

        static bool ValidParam(Type objType, MemberInfo member, Type type, bool logFailed = true)
        {
            bool valid = false;

            if (type.IsPrimitive) valid = true;
            else if (type == typeof(decimal)) valid = true;
            else if (type == typeof(string)) valid = true;
            else if (type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector4)) valid = true;
            else if (type.IsEnum) valid = true;

            if (!valid)
            {
                if (logFailed) Debug.LogError("Cannot register: " + objType.Name + " => " + member.Name + " method contains parameter of type " + type.Name + ". Only primitive and string parameters are allowed at the moment.");
                return false;
            }

            return true;
        }

        static void CheckDestroyedMonoBehaviours(HashSet<object> instances)
        {
            instances.RemoveWhere(s => {
                return ((s as MonoBehaviour) == null && s.GetType().IsSubclassOf(typeof(MonoBehaviour)));
            });
        }

        static int IsRemoteCommand(ref string command)
        {
            int isRemoteCommand;

            if (command[0] == instance.executeOnAllPrefix)
            {
                isRemoteCommand = 1;
                command = command.Substring(1);
            }
            else if (command[0] == instance.executeOnlyOnRemotePrefix)
            {
                isRemoteCommand = 2;
                command = command.Substring(1);
            }
            else isRemoteCommand = 0;

            return isRemoteCommand;
        }

        public static void ExecuteCommand(string command)
        {
            command = command.Replace(',', ' ');
            command = command.Trim(' ');

            if (command == string.Empty) return;
            if (command == instance.adminModeConsoleCommand && (instance.adminModeInBuild == AccessMode.EnabledOnConsoleCommand || instance.testOnlyFreeConsoleCommands))
            {
                accessLevel = AccessLevel.Admin;
                LogResult("AccessLevel = Admin");
                return;
            }
            if (command == instance.specialModeConsoleCommand && (instance.specialModeInBuild == AccessMode.EnabledOnConsoleCommand || instance.testOnlyFreeConsoleCommands))
            {
                accessLevel = AccessLevel.Special;
                LogResult("AccessLevel = Special");
                return;
            }

            if (command[0] == instance.searchCommandPrefix) return;

            int isRemoteCommand = IsRemoteCommand(ref command);

            string argumentString;

            int spaceIndex = command.IndexOf(" ");
            if (spaceIndex != -1) argumentString = command.Substring(spaceIndex + 1);
            else argumentString = string.Empty;

            GetArguments(command, inputCommands);

            // inputCommands.EnqueueRange(command.Split(' '));

            string firstCommand = inputCommands.Dequeue();

            CommandData commandData;
            if (!commandsTable.lookup.TryGetValue(firstCommand, out commandData) || commandData.obj == null)
            {
                CannotFindCommand(command, firstCommand);
                return;
            }

            if (commandData.consoleCommand != null && !commandData.consoleCommand.HasAccess(accessLevel))
            {
                CannotFindCommand(command, firstCommand);
                return;
            }

            if (!commandData.IsRegistered())
            {
                LogResultError("There is no registered instance for command '" + firstCommand + "'");
                return;
            }

            if (isRemoteCommand > 0)
            {
                if (onRemoteCommand != null) onRemoteCommand.Invoke(command);
                if (isRemoteCommand == 2) command = "Execute only on remote '" + command + "'";
                else command = "Execute on all '" + command + "'";
            }

            Log(new LogEntry(command, null, LogType.Log, EntryType.Command, Color.green, instance.logFontSize, FontStyle.Bold));
            if (HtmlDebug.instance) HtmlDebug.instance.UnityDebugLog(command, null, LogType.Log, true, -1, null, EntryType2.Command, false);

            if (isRemoteCommand != 2) commandData.Execute(inputCommands, argumentString);
        }

        static public void GetArguments(string argumentString, FastQueue<string> inputCommands)
        {
            argumentString = argumentString.Trim(' ');
            inputCommands.FastClear();

            int startIndex = 0;
            for (int i = 0; i < argumentString.Length; i++)
            {
                char c = argumentString[i];

                if (i == argumentString.Length - 1)
                {
                    inputCommands.Enqueue(argumentString.Substring(startIndex, (i - startIndex) + 1).Trim());
                    break;
                }

                if (c == ' ')
                {
                    if (i < argumentString.Length - 1)
                    {
                        if (argumentString[i + 1] == ' ') continue;
                    }
                    int spaceIndex = argumentString.IndexOf(' ', i + 1);
                    int length = i - startIndex;
                    inputCommands.Enqueue(argumentString.Substring(startIndex, length).Trim());
                    if (spaceIndex == -1)
                    {
                        inputCommands.Enqueue(argumentString.Substring(i + 1).Trim());
                        break;
                    }
                    startIndex += length + 1;
                }
                if (c == '\"')
                {
                    int stringIndex = argumentString.IndexOf('\"', i + 1);
                    if (stringIndex == -1)
                    {
                        LogCommandFailed(argumentString, "String closing \" is missing");
                        return;
                    }
                    int length = stringIndex - startIndex;
                    inputCommands.Enqueue(argumentString.Substring(startIndex + 1, length - 1).Trim());
                    startIndex += length + 1;
                    i = stringIndex + 1;
                }
            }
        }

        static void CannotFindCommand(string command, string firstCommand)
        {
            string result = "Cannot find command '" + firstCommand + "'";
            LogCommandFailed(command, result);
        }

        static void LogCommandFailed(string command, string result)
        {
            Log(new LogEntry(command, null, LogType.Log, EntryType.Command, Color.green, instance.logFontSize, FontStyle.Bold));
            Log(new LogEntry(result, null, LogType.Log, EntryType.CommandResult, Helper.colCommandResultFailed, instance.logFontSize, FontStyle.Bold));

            if (HtmlDebug.instance) HtmlDebug.instance.UnityDebugLog(result, null, LogType.Log, true, -1, null, EntryType2.CommandFault, true);
        }

        public static bool FindCommand(string command)
        {
            string[] commandStrings = command.Split(' ');

            CommandData commandData;
            if (!commandsTable.lookup.TryGetValue(commandStrings[0], out commandData) || commandData.obj == null)
            {
                return false;
            }

            return true;
        }
    }
}