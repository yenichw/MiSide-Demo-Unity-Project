using UnityEngine;

namespace DebuggingEssentials
{
    using System;

    public partial class RuntimeConsole
    {
        public enum AccessMode { EnabledOnConsoleCommand, Enabled, Disabled };

        // [Header("VISIBLE")]
        public bool showConsoleOnStart = true;
        [ConsoleCommand] public bool showConsoleOnWarning;
        [ConsoleCommand] public bool showConsoleOnError;
        [ConsoleCommand] public bool showConsoleOnException;
        [ConsoleCommand] public bool showConsoleWhenInvokingMethod = true;
        [ConsoleCommand] public bool disableUnityDevelopmentConsole = true;

        // [Header("CONTROLS")]
        public bool useSameEditorAsBuildShowKey = true;
        public AdvancedKey showToggleKeyEditor = new AdvancedKey(KeyCode.F9);
        public AdvancedKey showToggleKeyBuild = new AdvancedKey(KeyCode.F9);

        // [Header("SETTINGS")]
        [Tooltip("This ignores cases in commands")]
        [ConsoleCommand]
        public bool ignoreCasesInCommands = false;

        [Tooltip("This ignores cases for auto-complete lookup, but still cases are needed for commands")]
        [ConsoleCommand]
        public bool ignoreCasesInAutoCompleteInput = true;
        public AccessMode adminModeInBuild;
        public string adminModeConsoleCommand = "*GetAdminAccess*";

        public AccessMode specialModeInBuild;
        public string specialModeConsoleCommand = "*GetSpecialAccess*";

        [Tooltip("This will disable admin/special commands in Unity Editor")]
        public bool testOnlyFreeConsoleCommands = false;

        // [Header("SEARCH CONSOLE")]
        public char searchCommandPrefix = '!';
        [ConsoleCommand]
        public bool ignoreCasesInSearch = true;

        // [Header("NETWORK COMMAND PREFIX")]
        public char executeOnAllPrefix = '#';
        public char executeOnlyOnRemotePrefix = '$';

        // [Header("FONT SIZES")]
        public int titleFontSize = 18;
        public int frameFontSize = 14;
        public int logFontSize = 14;
        public int stackFontSize = 12;

        // [Header("REFERENCES")]
        public SO_ConsoleWindow windowData;

        Color selectColor = new Color(0.25f, 0.8f, 0.25f);
        Color backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.9f);

        Vector2 GetDockPos()
        {
            float scale = WindowManager.instance.windowData.guiScale.value * 2;
            return new Vector2((((Screen.width / scale) - (consoleWindow.rect.width / 2)) / Screen.width), 0);
        }
        Rect rectAutoComplete;
        Rect rectScroll;

        string lastAutoCompleteCommand;
        double scrollViewEndHeight;
        bool drawLogsScrollBar;

        void CalcDraw(bool reset)
        {
            LogEntry.showCommandLogs = logs[commandLogs].show.Value;
            LogEntry.showUnityLogs = logs[unityLogs].show.Value;
            LogEntry.showWarningLogs = logs[warningLogs].show.Value;
            LogEntry.showErrorLogs = logs[errorLogs].show.Value;
            LogEntry.showExceptionLogs = logs[exceptionLogs].show.Value;
            LogEntry.showStack = _showStack.Value;

            if (reset) LogEntry.lastLog = null;

            cullLists.Clear();

            if (logs[commandLogs].show.Value) cullLists.Add(logs[commandLogs]);
            if (showUnityLogs.Value)
            {
                for (int i = frameLogs; i <= exceptionLogs; i++)
                {
                    if (logs[i].show.Value) cullLists.Add(logs[i]);
                }
            }

            cullGroup.CalcDraw(reset, cullLists);
        }

        public void MyOnGUI()
        {
            var currentEvent = Event.current;

            if (Helper.IsShowKeyPressed(currentEvent, useSameEditorAsBuildShowKey, showToggleKeyEditor, showToggleKeyBuild))
            {
                SetActive(!show);
                return;
            }

            if (!show) return;

            if (currentEvent.type == EventType.Layout)
            {
                if (calcDraw)
                {
                    calcDraw = false;
                    CalcDraw(true);
                }
            }

            if (consoleWindow.isDocked.Value)
            {
                if (consoleWindow.drag == 1)
                {
                    if (consoleWindow.position != GetDockPos()) consoleWindow.isDocked.Value = false;
                }
                else consoleWindow.position = GetDockPos();
            }

            consoleWindow.Update(700, 94);
            GUI.skin = windowData.skin;

            if (WindowManager.instance.useCanvas) GUI.backgroundColor = new Color(1, 1, 1, 0);
            else GUI.backgroundColor = windowData.color;

            Helper.DrawWindow(23423022, consoleWindow, DrawConsole);
            DrawAutoComplete();
        }

        void DrawConsole(int windowId)
        {
            WindowManager.BeginRenderTextureGUI();

            var currentEvent = Event.current;

            GUISkin skin = windowData.skin;

            skin.box.fontStyle = FontStyle.Bold;

            Rect window = consoleWindow.rect;

            GUI.backgroundColor = Helper.GetColor(windowData, backgroundColor);
            GUI.Box(new Rect(0, 0, window.width, window.height), "");
            GUILayout.Box("Console");
            Helper.Drag(consoleWindow, null, null, true, true);

            GUI.backgroundColor = Color.white;

            // GUILayout.Space(-26);
            DrawButtons();
            DrawLogs(currentEvent, window);

            GUILayout.Space(1);
            DrawInput(currentEvent, window);
            drawLogsScrollBar = DrawLogsScrollBar(currentEvent);

            GUILayout.Space(-1);
            Helper.Drag(consoleWindow, null, null, false, true);

            WindowManager.SetToolTip(windowId);

            WindowManager.EndRenderTextureGUI();
        }

        void DrawButtons()
        {
            GUISkin skin = windowData.skin;
            int buttonSize = 80;

            GUILayout.BeginHorizontal();
            if (Helper.DrawShowButton(windowData, Helper.GetGUIContent("Commands", "Show/Hide Command Logs"), logs[commandLogs].show, selectColor, buttonSize)) calcDraw = true;

            if (Helper.DrawShowButton(windowData, Helper.GetGUIContent("Unity Log", "Show/Hide Debug Logs"), showUnityLogs, selectColor, buttonSize)) calcDraw = true;

            if (showUnityLogs.Value)
            {
                var logContent = windowData.logIcon.GetGUIContent();
                logContent.tooltip = "Show/Hide Debug Logs";
                float contentSize = skin.button.CalcSize(logContent).x + 2;
                if (Helper.DrawShowButton(windowData, logContent, logs[unityLogs].show, selectColor, contentSize)) calcDraw = true;

                var warningContent = windowData.warningIcon.GetGUIContent();
                warningContent.tooltip = "Show/Hide Debug Warnings";
                contentSize = skin.button.CalcSize(warningContent).x + 2;
                if (Helper.DrawShowButton(windowData, warningContent, logs[warningLogs].show, selectColor, contentSize)) calcDraw = true;

                var errorContent = windowData.errorIcon.GetGUIContent();
                errorContent.tooltip = "Show/Hide Debug Errors";
                contentSize = skin.button.CalcSize(errorContent).x + 2;
                if (Helper.DrawShowButton(windowData, errorContent, logs[errorLogs].show, selectColor, contentSize)) calcDraw = true;

                var exceptionContent = windowData.exceptionIcon.GetGUIContent();
                exceptionContent.tooltip = "Show/Hide Debug Exceptions";
                contentSize = skin.button.CalcSize(exceptionContent).x + 2;
                if (Helper.DrawShowButton(windowData, exceptionContent, logs[exceptionLogs].show, selectColor, contentSize)) calcDraw = true;

                if (Helper.DrawShowButton(windowData, Helper.GetGUIContent("Stack", _showStack.Value ? windowData.texStackOn : windowData.texStackOff, "Show/Hide Stack Trace"), _showStack, selectColor, buttonSize)) calcDraw = true;
            }

            if (GUILayout.Button(Helper.GetGUIContent("Html Log", "Open Html Log in browser"), GUILayout.Width(buttonSize)))
            {
                if (HtmlDebug.instance)
                {
                    Application.OpenURL("file://" + HtmlDebug.instance.logPathIncludingFilename);
                }
                else
                {
                    Debug.LogError("Can't find Html Debug, make sure you have the 'Html Debug' GameObject in your Scene and that it's enabled");
                }
            }
            if (GUILayout.Button(Helper.GetGUIContent("Logs Folder", "Open Logs Folder in File Exlorer"), GUILayout.Width(buttonSize)))
            {
                if (HtmlDebug.instance)
                {
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.RevealInFinder(Helper.GetConsoleLogPath());
#else
                    if (HtmlDebug.instance) Application.OpenURL("file://" + HtmlDebug.instance.logPath);
                    else Application.OpenURL("file://" + Application.persistentDataPath);
#endif
                }
                else
                {
                    Debug.LogError("Can't find Html Debug, make sure you have the 'Html Debug' GameObject in your Scene and that it's enabled");
                }
            }

            if (!consoleWindow.isDocked.Value)
            {
                if (GUILayout.Button(Helper.GetGUIContent("Dock", "Centers the window in the top of the screen"), GUILayout.Width(buttonSize)))
                {
                    consoleWindow.isDocked.Value = true;
                }
            }

            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
        }

        void DrawLogs(Event currentEvent, Rect window)
        {
            GUISkin skin = windowData.skin;
            rectScroll = new Rect(4, 54, window.width - (drawLogsScrollBar ? 22 : 10), window.height - 80);

            bool showStack = _showStack.Value;

            CullList logList = cullGroup.cullList;

            if (currentEvent.type == EventType.Layout)
            {
                scrollViewEndHeight = 0;
                if (logList.cullItems.Count > 0) scrollViewEndHeight = logList.cullItems.items[logList.cullItems.Count - 1].endHeight;
                double endHeightBegin = scrollViewEndHeight - rectScroll.height;

                if (scrollViewEndHeight > rectScroll.height)
                {
                    if (showLastLog.Value)
                    {
                        scrollView.y = endHeightBegin;
                    }
                }

                scrollViewHeights = new Double2(scrollView.y, scrollView.y + rectScroll.height);
                logList.Cull(scrollViewHeights);
            }

            if (logList.cullItems.Count == 0) return;

            GUILayout.BeginArea(rectScroll);
            double startSpace = logList.cullItems.items[logList.startIndex].startHeight - scrollView.y;
            GUILayout.Space((float)startSpace);

            for (int i = logList.startIndex; i <= logList.endIndex; i++)
            {
                var log = (LogEntry)logList.cullItems.items[i];

                skin.label.fontSize = log.fontSize;
                skin.label.fontStyle = log.fontStyle;

                if (log.entryType == EntryType.Frame)
                {
                    GUILayout.Space(20);
                }

                GUILayout.BeginHorizontal();
                if (log.entryType == EntryType.Unity)
                {
                    if (log.logType == LogType.Warning) GUI.color = Color.yellow;
                    else if (log.logType == LogType.Error) GUI.color = Color.red;
                    else if (log.logType == LogType.Exception) GUI.color = Color.magenta;
                    else if (log.logType == LogType.Log) GUI.color = Color.white;
                }
                else GUI.color = log.color;

                if (log.entryType == EntryType.Unity || log.entryType == EntryType.Console || log.entryType == EntryType.Command || log.entryType == EntryType.CommandResult)
                {
                    GUILayout.Space(30);
                }

                if (log.threadString != null)
                {
                    float width = GUI.skin.label.CalcSize(Helper.GetGUIContent(log.logString)).x;
                    GUILayout.Label(log.logString, GUILayout.Width(width));
                }
                else GUILayout.Label(log.logString);// + " " + log.tStamp);// + " " + (log.endHeight - log.startHeight));
                                                    // GUILayout.Label((GUILayoutUtility.GetLastRect().y + scrollView.y).ToString(), GUILayout.Width(70));

                if (log.entryType == EntryType.Unity || log.entryType == EntryType.Console || log.entryType == EntryType.Command)
                {
                    Rect rect = GUILayoutUtility.GetLastRect();
                    rect.x -= 12;
                    rect.y += log.fontSize / 2;
                    if (log.entryType == EntryType.Unity)
                    {
                        rect.width = 4;
                        rect.height = 4;
                        GUI.DrawTexture(rect, windowData.texDot);
                    }
                    else
                    {
                        rect.y -= 2;
                        rect.width = 8;
                        rect.height = 8;
                        GUI.DrawTexture(rect, windowData.texArrow);
                    }
                }
                if (log.threadString != null)
                {
                    GUI.skin.label.fontStyle = FontStyle.Italic;
                    GUILayout.Label(log.threadString);
                    GUI.skin.label.fontStyle = FontStyle.Normal;
                }
                GUILayout.EndHorizontal();

                if (showStack && log.stackLines != null)
                {
                    skin.label.fontSize = stackFontSize;
                    GUILayout.Space(-3);
                    GUI.color = GUI.color * 0.825f;

                    for (int j = 0; j < log.stackLines.Length; j++)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(30);
                        GUILayout.Label(log.stackLines[j]);
                        GUILayout.EndHorizontal();
                    }
                }

                GUI.color = Color.white;
                if (!logs[commandLogs].show.RealValue && !showUnityLogs.RealValue) showUnityLogs.Value = true;
            }

            skin.label.fontSize = 12;
            skin.label.fontStyle = FontStyle.Normal;
            GUILayout.EndArea();

            // GUILayout.Label("scrollHeight " + rectScroll.height + " posY " + scrollView.y + " height " + height + " => " + (scrollView.y + rectScroll.height));
        }

        bool DrawLogsScrollBar(Event currentEvent)
        {
            // CullList logList = cullGroup.cullList;

            Rect newRect = new Rect();
            newRect.y = 55;
            newRect.x = rectScroll.width + 5;
            newRect.width = 20;
            newRect.height = consoleWindow.rect.height - 80;

            double endHeightBegin = scrollViewEndHeight - rectScroll.height;

            if (scrollViewEndHeight > rectScroll.height)
            {
                if (currentEvent.isScrollWheel && autoCompleteList.Count == 0)
                {
                    scrollView.y += currentEvent.delta.y * 10;
                    showLastLog.Value = (scrollView.y >= endHeightBegin);
                }

                if (showLastLog.Value)
                {
                    scrollView.y = endHeightBegin;
                }

                GUI.changed = false;
                scrollView.y = GUI.VerticalScrollbar(newRect, (float)scrollView.y, rectScroll.height, 0, (float)scrollViewEndHeight);
                if (GUI.changed)
                {
                    showLastLog.Value = (scrollView.y >= endHeightBegin - 0.1f);
                }
                return true;
            }
            else scrollView.y = 0;
            return false;
        }

        bool addInput;

        void DrawInput(Event currentEvent, Rect window)
        {
            GUILayout.Space(window.height - 82);
            int buttonSize = 45;

            TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);

            int cursorIndex;

            if (editor != null) cursorIndex = editor.cursorIndex;
            else cursorIndex = 0;

            bool hasInputFocus = (GUI.GetNameOfFocusedControl() == "ConsoleInput");

            if (hasInputFocus)
            {
                if (currentEvent.type == EventType.KeyDown && currentEvent.keyCode == KeyCode.Return)
                {
                    showLastLog.Value = true;

                    commands.Add(inputCommand);
                    commandIndex = commands.Count;

                    if (!FindCommand(inputCommand))
                    {
                        AutoCompleteInputCommand();
                    }

                    ExecuteCommand(inputCommand);

                    inputCommand = string.Empty;
                    setFocus = true;
                }

                if (currentEvent.type == EventType.KeyDown)
                {
                    if (currentEvent.keyCode == KeyCode.DownArrow && cursorIndex > 0) { autoCompleteIndex++; updateAutoCompleteScrollView = true; }
                    else if (currentEvent.keyCode == KeyCode.UpArrow && cursorIndex > 0) { moveInputCursor = 1; autoCompleteIndex--; updateAutoCompleteScrollView = true; }
                    else if (currentEvent.keyCode != KeyCode.Tab) { autoCompleteIndex = 0; updateAutoCompleteScrollView = true; }
                }
            }

            autoCompleteIndex = Helper.Repeat(autoCompleteIndex, autoCompleteList.Count);

            GUILayout.BeginHorizontal();
            GUILayout.Space(4);
            GUI.SetNextControlName("ConsoleInput");

            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;

            GUI.changed = false;

            if (currentEvent.type == EventType.KeyDown)
            {
                if (currentEvent.keyCode == showToggleKeyEditor.keyCode) addInput = false;
                else
                {
                    if (addInput)
                    {
                        inputCommand = GUILayout.TextField(inputCommand, GUILayout.Width(window.width - 8 - (((buttonSize + 4) * 2) + 18)));
                    }
                    else addInput = true;
                }
            }
            else GUILayout.TextField(inputCommand, GUILayout.Width(window.width - 8 - (((buttonSize + 4) * 2) + 18)));

            if (GUI.changed && addInput)
            {
                if (inputCommand.Length >= 2 && inputCommand[0] == searchCommandPrefix)
                {
                    LogEntry.search = inputCommand.Substring(1);
                    LogEntry.ignoreCasesInSearch = ignoreCasesInSearch;
                    CalcDraw(true);
                }
                else if (LogEntry.search != string.Empty)
                {
                    LogEntry.search = string.Empty;
                    CalcDraw(true);
                }
            }

            if (moveInputCursor != 0 && editor != null)
            {
                if (moveInputCursor == 1) editor.MoveTextEnd();
                else if (moveInputCursor == -1) editor.MoveTextStart();

                moveInputCursor = 0;
            }

            if (GUILayout.Button(Helper.GetGUIContent("Clear", "Clear all Logs"), GUILayout.Width(buttonSize))) Clear();

            Helper.DrawShowButton(windowData, Helper.GetGUIContent("Last", "Scroll to last Log"), showLastLog, selectColor, buttonSize);

            Rect rect = GUILayoutUtility.GetLastRect();
            rect.x += buttonSize + 7;
            rect.y += 9;
            rect.width = 10;
            rect.height = 10;

            bool onFocusCornerScale = new Rect(rect.x, rect.y, 16, 16).Contains(currentEvent.mousePosition);
            if (onFocusCornerScale) GUI.color = Color.white; else GUI.color = Color.grey;
            GUI.Label(rect, Helper.GetGUIContent(windowData.texCornerScale, "Resize console window"));
            GUI.color = Color.white;

            GUILayout.EndHorizontal();

            if (commands.Count > 0 && cursorIndex == 0)
            {
                if (currentEvent.keyCode == KeyCode.UpArrow && currentEvent.type == EventType.KeyUp)
                {
                    --commandIndex;
                    if (commandIndex < 0) commandIndex = 0;
                    inputCommand = commands.items[commandIndex];
                }
                else if (currentEvent.keyCode == KeyCode.DownArrow)
                {
                    if (currentEvent.type == EventType.KeyUp)
                    {
                        ++commandIndex;
                        if (commandIndex > commands.Count - 1)
                        {
                            commandIndex = commands.Count - 1;
                            inputCommand = string.Empty;
                        }
                        else inputCommand = commands.items[commandIndex];
                    }
                    moveInputCursor = -1;
                }
            }

            if (hasInputFocus) GetAutoCompleteList(inputCommand);
            else ClearAutoCompleteList();
            
            if (currentEvent.type == EventType.Repaint)
            {
                if (setFocus)
                {
                    setFocus = false;
                    GUI.FocusControl("ConsoleInput");
                }
            }
        }

        void DrawAutoComplete()
        {
            Event currentEvent = Event.current;

            if (autoCompleteList.Count != oldAutoCompleteCount && currentEvent.type == EventType.Layout)
            {
                oldAutoCompleteCount = autoCompleteList.Count;
                updateAutoCompleteScrollView = true;
            }

            if (autoCompleteList.Count > 0 && autoCompleteList.Count == oldAutoCompleteCount)
            {
                rectAutoComplete = consoleWindow.rect;
                rectAutoComplete.y += rectAutoComplete.height;
                rectAutoComplete.height = Mathf.Min((autoCompleteList.Count * 25) + 2, 302);
                rectAutoComplete.width += 10;
                GUI.skin = windowData.skinAutoComplete;
                GUILayout.Window(23423023, rectAutoComplete, DrawAutoComplete, GUIContent.none);
                GUI.skin = windowData.skin;
            }

            if (currentEvent.type == EventType.KeyDown && currentEvent.keyCode == KeyCode.Tab)
            {
                AutoCompleteInputCommand();
            }
        }

        void DrawAutoComplete(int windowId)
        {
            WindowManager.BeginRenderTextureGUI();

            GUISkin skin = windowData.skin;

            GUI.backgroundColor = backgroundColor;
            GUI.Box(new Rect(0, 0, rectAutoComplete.width - 10, rectAutoComplete.height), string.Empty);
            GUI.backgroundColor = Color.white;

            if (updateAutoCompleteScrollView)
            {
                updateAutoCompleteScrollView = false;
                autoCompleteScrollView.y = Mathf.Max((autoCompleteIndex * 25) - (rectAutoComplete.height / 2), 0);
            }

            autoCompleteScrollView = GUILayout.BeginScrollView(autoCompleteScrollView);

            float prefix1 = 0, prefix2 = 0, prefix3 = 0, prefix4 = 0;

            for (int i = 0; i < autoCompleteList.Count; i++)
            {
                AutoComplete autoComplete = autoCompleteList.items[i];
                CommandData commandData = autoComplete.commandData;

                float size = skin.button.CalcSize(Helper.GetGUIContent(autoComplete.command)).x;
                if (size > prefix1) prefix1 = size;
                if (commandData.memberType == MemberType.Field || commandData.memberType == MemberType.Property)
                {
                    object value = commandData.GetValue();

                    if (value != null)
                    {
                        size = skin.label.CalcSize(Helper.GetGUIContent(value.ToString())).x;
                        if (size > prefix3) prefix3 = size;
                    }
                }
                size = skin.label.CalcSize(Helper.GetGUIContent(commandData.syntax)).x;
                if (size > prefix2) prefix2 = size;

                int instanceCount = commandData.GetInstanceCount();

                if (instanceCount > 0)
                {
                    size = skin.label.CalcSize(Helper.GetGUIContent("Instances: " + instanceCount)).x;
                    if (size > prefix4) prefix4 = size;
                }
            }

            prefix2 += 15;
            if (prefix3 > 0) prefix3 += 15;
            if (prefix4 > 0) prefix4 += 15;
            GUILayout.Space(-2);

            for (int i = 0; i < autoCompleteList.Count; i++)
            {
                AutoComplete autoComplete = autoCompleteList.items[i];
                CommandData commandData = autoComplete.commandData;

                string autoCompleteCommand = autoComplete.command;

                if (i % 2 == 0)
                {
                    Rect rect = GUILayoutUtility.GetRect(0, 27);
                    rect.width = rectAutoComplete.width - 11;
                    rect.y += 2;
                    rect.height -= 2;
                    GUILayout.Space(-27);
                    GUI.backgroundColor = new Color(0.0675f, 0.0675f, 0.0675f, 1);
                    GUI.Box(rect, string.Empty);
                    GUI.backgroundColor = Color.white;
                }

                GUILayout.BeginHorizontal();
                GUI.backgroundColor = (i == autoCompleteIndex ? Color.green : Color.white);
                if (GUILayout.Button(autoCompleteCommand, GUILayout.Width(prefix1)))
                {
                    inputCommand = autoCompleteCommand;
                    moveInputCursor = 1;
                }

                GUILayout.BeginVertical();
                GUILayout.Space(7);
                GUILayout.BeginHorizontal();
                GUILayout.Label(commandData.syntax, GUILayout.Width(prefix2));
                if (prefix3 > 0)
                {
                    if (commandData.memberType == MemberType.Field || commandData.memberType == MemberType.Property)
                    {
                        object value = commandData.GetValue();

                        if (value != null)
                        {
                            GUI.color = Color.green;
                            GUILayout.Label(value.ToString(), GUILayout.Width(prefix3));
                            GUI.color = Color.white;
                        }
                        else GUILayout.Space(prefix3);
                    }
                    else GUILayout.Space(prefix3);
                }
                if (prefix4 > 0)
                {
                    int instanceCount = commandData.GetInstanceCount();

                    if (instanceCount > 0)
                    {
                        GUI.color = new Color(0.3f, 0.5f, 1, 1);
                        GUILayout.Label("Instances: " + instanceCount, GUILayout.Width(prefix4));
                        GUI.color = Color.white;
                    }
                    else GUILayout.Space(prefix4);
                }
                if (commandData.consoleCommand != null && commandData.consoleCommand.description != string.Empty)
                {
                    GUI.color = Color.yellow;
                    GUILayout.Label(commandData.consoleCommand.description);
                    GUI.color = Color.white;
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(-7);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            GUI.backgroundColor = Color.white;
            GUILayout.EndScrollView();

            WindowManager.EndRenderTextureGUI();
        }

        void AutoCompleteInputCommand()
        {
            if (autoCompleteList.Count > 0)
            {
                inputCommand = inputCommand.TrimStart(' ');

                int isRemoteCommand = IsRemoteCommand(ref inputCommand);

                string args = string.Empty;
                int index = inputCommand.IndexOf(' ');
                if (index != -1) args = inputCommand.Substring(index);

                inputCommand = autoCompleteList.items[autoCompleteIndex].command + args;
                if (isRemoteCommand == 1) inputCommand = "#" + inputCommand;
                else if (isRemoteCommand == 2) inputCommand = "$" + inputCommand;

                if (inputCommand[inputCommand.Length - 1] == '.') inputCommand = inputCommand.Substring(0, inputCommand.Length - 1);

                moveInputCursor = 1;
            }
        }

        void ClearAutoCompleteList()
        {
            lastAutoCompleteCommand = string.Empty;
            autoCompleteList.FastClear();
        }

        void GetAutoCompleteList(string command)
        {
            if (command == lastAutoCompleteCommand)
            {
                if (command == string.Empty && autoCompleteList.Count > 0) ClearAutoCompleteList();
                return;
            }

            lastAutoCompleteCommand = command;

            ClearAutoCompleteList();

            command = command.Trim(' ');

            if (command == string.Empty) return;
            if (command[0] == searchCommandPrefix) return;

            int spaceIndex = command.IndexOf(" ");
            if (spaceIndex != -1) command = command.Substring(0, spaceIndex);

            IsRemoteCommand(ref command);
            if (command == string.Empty) return;

            if (command[0] == '?')
            {
                command = command.Substring(1);
            }

            string[] names = commandsTable.names;

            for (int i = 0; i < names.Length; i++)
            {
                if (ignoreCasesInAutoCompleteInput ? names[i].StartsWith(command, StringComparison.CurrentCultureIgnoreCase) : names[i].StartsWith(command))
                {
                    string name = names[i];
                    CommandData commandData = commandsTable.lookup[name];

                    if (commandData.consoleCommand == null)
                    {
                        if (accessLevel != AccessLevel.Admin) continue;

                        bool found = false;
                        for (int j = i + 1; j < names.Length; j++)
                        {
                            if (ignoreCasesInAutoCompleteInput ? names[j].StartsWith(name, StringComparison.CurrentCultureIgnoreCase) : names[j].StartsWith(name))
                            {
                                CommandData commandData2 = commandsTable.lookup[names[j]];
                                if (commandData2.consoleCommand == null || !commandData2.consoleCommand.HasAccess(accessLevel) || !commandData2.IsRegistered()) continue;
                                found = true;
                                break;
                            }
                            else break;
                        }
                        if (!found) continue;
                    }
                    else
                    {
                        if (!commandData.consoleCommand.HasAccess(accessLevel) || !commandData.IsRegistered()) continue;
                    }

                    autoCompleteList.Add(new AutoComplete(name, commandData));
                }
            }
        }

        public struct AutoComplete
        {
            public string command;
            public CommandData commandData;

            public AutoComplete(string command, CommandData commandData)
            {
                this.command = command;
                this.commandData = commandData;
            }
        }
    }
}