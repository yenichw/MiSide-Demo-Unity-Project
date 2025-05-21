using System;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;

namespace DebuggingEssentials
{
    [DefaultExecutionOrder(-5000000)]
    public class HtmlDebug : MonoBehaviour
    {
        public static HtmlDebug instance;
        public static System.Diagnostics.Stopwatch timeSinceStartup = new System.Diagnostics.Stopwatch();
        public static float frameTime;
        public static int currentFrame = 0;
        public static int mainThreadId;

        // Inspector 
        // ======================================================
        // [Header("SETTINGS")
        [Tooltip("Automatically deletes build Log files that are older than x days.")]
        public bool deleteBuildLogs = true;
        public int deleteBuildLogsAfterDays = 7;
        [Tooltip("Open the HTML Log file manually by code if you want to give your own path and file name")]
        public bool openLogManually = false;
        [Tooltip("Only shows the first line of the stack trace with Debub.Log.\n\n(Only if Stack Trace is enabled in the Player Settings)")]
        public bool normalLogOnlyFirstLineStackTrace = true;
        
        // [Header("FONT SIZES")
        public int titleFontSize = 30;
        public int frameFontSize = 16;
        public int logFontSize = 15;
        public int stackFontSize = 13;
        // ======================================================

        [NonSerialized] public string logPathIncludingFilename;
        [NonSerialized] public string logPath;

        FastList<Log> logsThread = new FastList<Log>();
        FastList<Log> logs = new FastList<Log>();

        int lastFrame = -1;
        bool isLogEnabled;
        bool isEditor;
        bool isDebugBuild;

        StackTraceLogType logStackTraceLogType;
        StackTraceLogType assertStackTraceLogType;
        StackTraceLogType warningStackTraceLogType;
        StackTraceLogType errorStackTraceLogType;
        StackTraceLogType exceptionStackTraceLogType;

        string frameFontSizeString;
        string stackFontSizeString;
        string logFontSizeString;

        WaitCallback logCallBack;
        StreamWriter sw;

        bool isLogging = false;
        bool updateLogCallFromMainThread = true;
        bool isQuitting;
        
#if UNITY_EDITOR
        const int skipFrames = 6;
#else
#if ENABLE_IL2CPP
        const int skipFrames = 5;
#else
        const int skipFrames = 6;
#endif
#endif

        public static void ResetStatic()
        {
            frameTime = 0;
            currentFrame = 0;

            mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        void Awake()
        {
            if (instance != null)
            {
                Debug.LogError("You have more than 1 HtmlDebug GameObject, make sure to only use 1");
                return;
            }

            instance = this;
            
            logCallBack = new WaitCallback(WriteLogs);

            if (!openLogManually) OpenLog(Helper.GetConsoleLogPath());

            // DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
            if (!isQuitting) CloseLog("OnDestroy");
        }

        void OnApplicationQuit()
        {
            isQuitting = true;
            CloseLog("OnApplicationQuit");
        }

        void CloseLog(string closeReason)
        {
            while (isLogging) { }
            updateLogCallFromMainThread = false;
            UnityDebugLogThread(closeReason, string.Empty, LogType.Log);
            WriteLogs(logCallBack);
            CloseLog();
        }

        public static int GetThreadId()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            if (threadId != mainThreadId) return threadId; else return -1;
        }

        void DeleteBuildLogsAfterXDays(string logPath)
        {
            if (!deleteBuildLogs) return;

            string[] files = Directory.GetFiles(logPath);

            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);

                if (fi.LastWriteTime < DateTime.Now.AddDays(-deleteBuildLogsAfterDays))
                {
                    if (fi.Name.Contains(".html") || fi.Name.Contains(".log")) fi.Delete();
                }
            }
        }

        public static void OpenLog(string logPathIncludingFileName)
        {
            instance.OpenLogInternal(logPathIncludingFileName);
        }

        void OpenLogInternal(string logPathIncludingFileName)
        {
            if (isLogEnabled)
            {
                Debug.LogError("Html Debug Logs is already opened. Make sure if you call OpenLog manually to enable 'Open Log Manually' in the HTML Debug Inspector");
                return;
            }
            isLogEnabled = true;
            
            logStackTraceLogType = Application.GetStackTraceLogType(LogType.Log);
            assertStackTraceLogType = Application.GetStackTraceLogType(LogType.Assert);
            warningStackTraceLogType = Application.GetStackTraceLogType(LogType.Warning);
            errorStackTraceLogType = Application.GetStackTraceLogType(LogType.Error);
            exceptionStackTraceLogType = Application.GetStackTraceLogType(LogType.Exception);

            isEditor = Application.isEditor;
            isDebugBuild = Debug.isDebugBuild;

            frameFontSizeString = "font-size:" + frameFontSize + "px;\">";
            stackFontSizeString = "font-size:" + stackFontSize.ToString() + "px;\">";
            logFontSizeString = "font-size:" + logFontSize.ToString() + "px;\">";

            try
            {
                logPathIncludingFileName = logPathIncludingFileName.Replace(".log", ".html");
                this.logPathIncludingFilename = logPathIncludingFileName;
                logPath = logPathIncludingFileName.Substring(0, logPathIncludingFileName.LastIndexOf("/"));

#if UNITY_EDITOR
                if (File.Exists(logPathIncludingFileName))
                {
                    File.Copy(logPathIncludingFileName, logPathIncludingFileName.Replace(".html", "-prev.html"), true);
                }
#else
                Directory.CreateDirectory(Path.GetDirectoryName(logPathIncludingFileName));
#endif

                sw = new StreamWriter(logPathIncludingFileName, false, Encoding.ASCII, 8192);

                sw.Write("<html>");
                sw.Write("<body style=\"font-family:consolas; font-size:100%; background-color:#1E1E1E;\" >");
                sw.Write("<strong><span style=\"color:#DCDCDC;");
                sw.Write("font-size:");
                sw.Write(titleFontSize.ToString());
                sw.Write("px;\">");
                sw.Write(Helper.GetApplicationInfo());
                sw.Write("</span></strong><br><br><br>");
                sw.Write("<span style=\"color:#DCDCDC;");
                sw.Write(frameFontSizeString);
                sw.Write("Unity version ");
                sw.Write(Application.unityVersion);
                sw.Write("</span><ul>");

                Application.logMessageReceivedThreaded += UnityDebugLogThread;

#if !UNITY_EDITOR
                DeleteBuildLogsAfterXDays(logPath);
#endif
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        void CloseLog()
        {
            isLogEnabled = false;
            Application.logMessageReceivedThreaded -= UnityDebugLogThread;

            if (sw == null) return;

            sw.Write("</ul></body>");
            sw.Write("</html>");
            sw.Close();
        }

        bool UseStackTrace(LogType logType)
        {
            if (logType == LogType.Log && logStackTraceLogType != StackTraceLogType.None) return true;
            else if (logType == LogType.Assert && assertStackTraceLogType != StackTraceLogType.None) return true;
            else if (logType == LogType.Warning && warningStackTraceLogType != StackTraceLogType.None) return true;
            else if (logType == LogType.Error && errorStackTraceLogType != StackTraceLogType.None) return true;
            else if (logType == LogType.Exception && exceptionStackTraceLogType != StackTraceLogType.None) return true;
            else return false;
        }

        public void UpdateLogs()
        {
            if (isLogging || logsThread.Count == 0) return;
            isLogging = true;
            ThreadPool.QueueUserWorkItem(logCallBack);
        }

        void WriteLogs(object callback)
        {
            try
            {
                logs.GrabListThreadSafe(logsThread, true);

                for (int i = 0; i < logs.Count; i++)
                {
                    Log log = logs.items[i];
                    UnityDebugLog(log.logString, log.stackTraceString, log.logType, log.isMainThread, log.threadId, log.stackTrace);
                }

                logs.FastClear();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                isLogging = false;
            }
        }

        void UnityDebugLogThread(string logString, string stackTraceString, LogType logType)
        {
            if (!isLogEnabled || sw == null) return;

            int threadId = Thread.CurrentThread.ManagedThreadId;
            bool isMainThread = (threadId == mainThreadId);

            bool useStackTrace = UseStackTrace(logType);

            logsThread.AddThreadSafe(new Log() { logString = logString, stackTraceString = stackTraceString, logType = logType, isMainThread = isMainThread, threadId = threadId, stackTrace = (useStackTrace ? new System.Diagnostics.StackTrace(skipFrames, true) : null) });

            if (isMainThread && updateLogCallFromMainThread) UpdateLogs();
        }

        public void UnityDebugLog(string logString, string stackTraceString, LogType logType, bool isMainThread, int threadId = -1, System.Diagnostics.StackTrace stackTrace = null, EntryType2 entryType = EntryType2.Unity, bool closeLi = true)
        {
            if (!isLogEnabled || sw == null) return;

            // float realTime = timeSinceStartup.ElapsedMilliseconds / 1000f;

            if (currentFrame != lastFrame)
            {
                lastFrame = currentFrame;

                sw.Write("</ul><br><strong><span style=\"color:#508EA1;");
                sw.Write(frameFontSizeString);
                sw.Write("[Frame ");
                sw.Write(currentFrame.ToString("D6"));
                sw.Write("][Time ");
                sw.Write(Helper.ToTimeFormat(frameTime));
                // sw.Write("] ***********************************************************************************************");
                sw.Write("] -----------------------------------------------------------------------------------------------");
                sw.Write("</span></strong><ul>");
            }

            if (logType == LogType.Error) sw.Write("<li style =\"color:#FF0000;");
            else if (logType == LogType.Exception) sw.Write("<li style =\"color:#9D00FF;");
            else if (logType == LogType.Warning) sw.Write("<li style =\"color:#FFFF00;");
            else if (entryType == EntryType2.Unity) sw.Write("<li style =\"color:#F0F0F0;");
            else
            {
                if (!closeLi) sw.Write("<li style =\"color:#");
                else sw.Write("<span style =\"color:#");

                if (entryType == EntryType2.Command) sw.Write(ColorUtility.ToHtmlStringRGB(Color.green) + ";");
                else if (entryType == EntryType2.CommandResult) sw.Write(ColorUtility.ToHtmlStringRGB(Helper.colCommandResult) + ";");
                else if (entryType == EntryType2.CommandFault) sw.Write(ColorUtility.ToHtmlStringRGB(Helper.colCommandResultFailed) + ";");
            }

            sw.Write(logFontSizeString);
            if (entryType != EntryType2.Unity) sw.Write("<strong>");
            // sw.Write(logString.Replace("\n", "<br>"));
            sw.Write(logString);
            if (entryType != EntryType2.Unity) sw.Write("</strong>");

            if (!isMainThread)
            {
                sw.Write(" <i>[Thread ");
                sw.Write(threadId);
                sw.Write("]</i>");
            }
            sw.Write("</br>");

            string[] lines = null;

            if (logType == LogType.Exception)
            {
                sw.Write("<span style=\"color:#7D00DF;");
                sw.Write(stackFontSizeString);

                lines = stackTraceString.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    sw.Write(lines[i]);
                    if (i < lines.Length - 1) sw.Write("<br>");
                }
                sw.Write("</span>");
            }
            else
            {
                bool useStackTrace = (entryType == EntryType2.Unity && UseStackTrace(logType));

                if (useStackTrace)
                {
                    if (logType == LogType.Error) sw.Write("<span style=\"color:#A00000;");
                    else if (logType == LogType.Warning) sw.Write("<span style=\"color:#A0A000;");
                    else sw.Write("<span style=\"color:#909090;");
                    sw.Write(stackFontSizeString);

                    int count;
                    if (logType == LogType.Log && normalLogOnlyFirstLineStackTrace) count = 1;
                    else count = stackTrace.FrameCount;

                    if (RuntimeConsole.instance) lines = new string[count];

                    for (int i = 0; i < count; i++)
                    {
                        var frame = stackTrace.GetFrame(i);
                        if (frame != null)
                        {
                            var method = frame.GetMethod();
                            string name = method.DeclaringType.Name;
                            sw.Write(name);
                            sw.Write(".");
                            sw.Write(method);
                            if (isEditor || isDebugBuild)
                            {
                                sw.Write(":");
                                int lineNumber = frame.GetFileLineNumber();
                                sw.Write(lineNumber);
                                if (RuntimeConsole.instance) lines[i] = name + "." + method + ":" + lineNumber;
                            }
                            else if (RuntimeConsole.instance) lines[i] = name + "." + method;

                            sw.Write("<br>");
                        }
                    }

                    sw.Write("</style></span>");
                }
            }

            if (entryType == EntryType2.Unity)
            {
                if (RuntimeConsole.instance) RuntimeConsole.Log(logString, lines, logType, Color.white, threadId);
            }

            if (closeLi)
            {
                sw.Write("</li>");
                sw.Write("<span style=\"font-size:9px;\"><br></span>");
            }
            else sw.Write("</span>");

            sw.Flush();
        }
    }

    public struct Log
    {
        public string logString;
        public string stackTraceString;
        public LogType logType;
        public bool isMainThread;
        public int threadId;
        public System.Diagnostics.StackTrace stackTrace;
    }
}
