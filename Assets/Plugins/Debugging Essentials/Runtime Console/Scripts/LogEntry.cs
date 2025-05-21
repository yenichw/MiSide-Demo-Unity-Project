using System;
using UnityEngine;

namespace DebuggingEssentials
{
    public enum EntryType2 { Unity, Command, CommandResult, CommandFault }
    public enum EntryType { Console, Unity, Frame, Command, CommandResult };
    public enum Flag : int { Log = 1, Warning = 2, Error = 4, Exception = 8, Command = 16 };

    public class LogEntry : CullItem
    {
        static public string search = string.Empty;
        static public bool ignoreCasesInSearch;
        static public bool showCommandLogs;
        static public bool showUnityLogs, showWarningLogs, showErrorLogs, showExceptionLogs, showStack;
        static public int currentId = 0;
        static public LogEntry lastLog;

        public string logString;
        public string threadString;
        public string[] stackLines;
        public int stackLineCount;
        public LogType logType;
        public EntryType entryType;
        public byte flag;
        public Color color;
        public int fontSize;
        public FontStyle fontStyle;

        public static void ResetStatic()
        {
            search = string.Empty;
            ignoreCasesInSearch = false;
            showCommandLogs = false;
            showUnityLogs = showWarningLogs = showErrorLogs = showExceptionLogs = showStack = false;
            currentId = 0;
            lastLog = null;
        }

        public LogEntry(string logString, string[] stackLines = null, LogType logType = LogType.Log, EntryType entryType = EntryType.Unity, Color color = default(Color), int fontSize = 12, FontStyle fontStyle = FontStyle.Normal, int threadId = -1) : base(currentId++)
        {
            this.logString = logString;
            if (entryType != EntryType.Frame) CheckThread(threadId);

            this.stackLines = stackLines;
            if (stackLines != null) stackLineCount = stackLines.Length;
            else stackLineCount = 0;

            this.logType = logType;
            this.entryType = entryType;
            this.color = (color == default(Color) ? Color.white : color);
            this.fontSize = fontSize;
            this.fontStyle = fontStyle;

            flag = 0;

            if (entryType == EntryType.Frame) isHeader = true;
        }

        public LogEntry(string logString, Color color, int fontSize, FontStyle fontStyle, int threadId = -1) : base(currentId++)
        {
            this.logString = logString;
            if (entryType != EntryType.Frame) CheckThread(threadId);

            this.color = color;
            this.fontSize = fontSize;
            this.fontStyle = fontStyle;

            flag = 0;
            stackLines = null;
            stackLineCount = 0;
            logType = LogType.Log;
            entryType = EntryType.Console;
        }

        void CheckThread(int threadId)
        {
            if (threadId == -1) threadId = HtmlDebug.GetThreadId();
            if (threadId != -1 && threadId != HtmlDebug.mainThreadId) threadString = " [Thread " + threadId + "]";
        }

        public override DrawResult DoDraw()
        {
            if (entryType == EntryType.Frame)
            {
                bool shouldDraw = false;

                     if ((flag & (int)Flag.Log) != 0 && showUnityLogs) shouldDraw = true;
                else if ((flag & (int)Flag.Warning) != 0 && showWarningLogs) shouldDraw = true;
                else if ((flag & (int)Flag.Error) != 0 && showErrorLogs) shouldDraw = true;
                else if ((flag & (int)Flag.Exception) != 0 && showExceptionLogs) shouldDraw = true;
                else if ((flag & (int)Flag.Command) != 0 && showCommandLogs) shouldDraw = true;

                if (shouldDraw)
                {
                    if (lastLog != null && lastLog.entryType == EntryType.Frame) return DrawResult.DrawHeaderAndRemoveLastHeader;
                    lastLog = this;
                    return DrawResult.DrawHeader;
                }

                return DrawResult.DontDraw;
            }

            if (search.Length > 0)
            {
                if (ignoreCasesInSearch)
                {
                    if (logString.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) == -1) return DrawResult.DontDraw;
                }
                else if (logString.IndexOf(search) == -1) return DrawResult.DontDraw;
            }

            lastLog = this;
            return DrawResult.Draw;
        }

        public override float CalcHeight()
        {
            float size = (fontSize + 6);
            int lineCount = Helper.CalcStringLines(logString);
            if (lineCount > 1) size += (fontSize + 2) * (lineCount - 1);

            if (entryType == EntryType.Frame) size += 20;
            else if (showStack && entryType == EntryType.Unity && stackLineCount > 0)
            {
                size += (stackLineCount * (RuntimeConsole.instance.stackFontSize + 6f)) - 3;
                // Debug.LogError(logString + " " + fontSize + " " + stackLineCount);
            }

            return size;
        }
    }
}