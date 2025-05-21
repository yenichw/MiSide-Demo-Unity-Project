using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public static class Debug
//{
//    public static bool developerConsoleVisuble
//    {
//        get => UnityEngine.Debug.developerConsoleVisible;
//        set => UnityEngine.Debug.developerConsoleVisible = value;
//    }

//    public static bool isDebugBuild
//    {
//        get => UnityEngine.Debug.isDebugBuild;
//    }

//    public static ILogger unityLogger
//    {
//        get => UnityEngine.Debug.unityLogger;
//    }

//    [Obsolete("Assert(bool, string, params object[]) is obsolete. Use AssertFormat(bool, string, params object[]) (UnityUpgradable) -> AssertFormat(*)", true)]
//    public static void Assert(bool condition, string format, params object[] args) => UnityEngine.Debug.Assert(condition, format, args);
//    public static void Assert(bool condition, string message, UnityEngine.Object context) => UnityEngine.Debug.Assert(condition, message, context);
//    public static void Assert(bool condition) => UnityEngine.Debug.Assert(condition);
//    public static void Assert(bool condition, object message, UnityEngine.Object context) => UnityEngine.Debug.Assert(condition, message, context);
//    public static void Assert(bool condition, string message) => UnityEngine.Debug.Assert(condition, message);
//    public static void Assert(bool condition, object message) => UnityEngine.Debug.Assert(condition, message);
//    public static void Assert(bool condition, UnityEngine.Object context) => UnityEngine.Debug.Assert(condition, context);
//    public static void AssertFormat(bool condition, UnityEngine.Object context, string format, params object[] args) => UnityEngine.Debug.AssertFormat(condition, context, format, args);
//    public static void AssertFormat(bool condition, string format, params object[] args) => UnityEngine.Debug.AssertFormat(condition, format, args);
//    public static void Break() => UnityEngine.Debug.Break();
//    public static void ClearDeveloperConsole() => UnityEngine.Debug.ClearDeveloperConsole();
//    public static void DebugBreak() => UnityEngine.Debug.DebugBreak();
//    public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration) => UnityEngine.Debug.DrawLine(start, end, color, duration);
//    public static void DrawLine(Vector3 start, Vector3 end, Color color) => UnityEngine.Debug.DrawLine(start, end, color);
//    public static void DrawLine(Vector3 start, Vector3 end) => UnityEngine.Debug.DrawLine(start, end);
//    public static void DrawLine(Vector3 start, Vector3 end, Color color = default, float duration = 0, bool depthTest = true) => UnityEngine.Debug.DrawLine(start, end, color == default ? Color.white : color, duration, depthTest);
//    public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration) => UnityEngine.Debug.DrawRay(start, dir, color);
//    public static void DrawRay(Vector3 start, Vector3 dir, Color color) => UnityEngine.Debug.DrawRay(start, dir, color);
//    public static void DrawRay(Vector3 start, Vector3 dir, Color color = default, float duration = 0, bool depthTest = true) => UnityEngine.Debug.DrawRay(start, dir, color == default ? Color.white : color, duration, depthTest);
//    public static void DrawRay(Vector3 start, Vector3 dir) => UnityEngine.Debug.DrawRay(start, dir);
//    public static void Log(object message, [CallerLineNumber] int lineNumber = 0)
//    {
//        // UnityEngine.Debug.Log(message);
//        RuntimeInspector.HtmlDebug.Log(message.ToString(), "", LogType.Log, lineNumber);
//    }
//    public static void Log(object message, UnityEngine.Object context) => UnityEngine.Debug.Log(message, context);
//    public static void LogAssertion(object message, UnityEngine.Object context) => UnityEngine.Debug.LogAssertion(message, context);
//    public static void LogAssertion(object message) => UnityEngine.Debug.LogAssertion(message);
//    public static void LogAssertionFormat(UnityEngine.Object context, string format, params object[] args) => UnityEngine.Debug.LogAssertionFormat(context, format, args);
//    public static void LogAssertionFormat(string format, params object[] args) => UnityEngine.Debug.LogAssertionFormat(format, args);
//    public static void LogError(object message, UnityEngine.Object context) => UnityEngine.Debug.LogError(message, context);
//    public static void LogError(object message) => UnityEngine.Debug.LogError(message);
//    public static void LogErrorFormat(UnityEngine.Object context, string format, params object[] args) => UnityEngine.Debug.LogErrorFormat(context, format, args);
//    public static void LogErrorFormat(string format, params object[] args) => UnityEngine.Debug.LogErrorFormat(format, args);
//    public static void LogException(Exception exception) => UnityEngine.Debug.LogException(exception);
//    public static void LogException(Exception exception, UnityEngine.Object context) => UnityEngine.Debug.LogException(exception, context);
//    public static void LogFormat(string format, params object[] args) => UnityEngine.Debug.LogFormat(format, args);
//    public static void LogFormat(UnityEngine.Object context, string format, params object[] args) => UnityEngine.Debug.LogFormat(context, format, args);
//    public static void LogFormat(LogType logType, LogOption logOptions, UnityEngine.Object context, string format, params object[] args) => UnityEngine.Debug.LogFormat(logType, logOptions, context, format, args);
//    public static void LogWarning(object message, UnityEngine.Object context) => UnityEngine.Debug.LogWarning(message, context);
//    public static void LogWarning(object message) => UnityEngine.Debug.LogWarning(message);
//    public static void LogWarningFormat(UnityEngine.Object context, string format, params object[] args) => UnityEngine.Debug.LogWarningFormat(context, format, args);
//    public static void LogWarningFormat(string format, params object[] args) => UnityEngine.Debug.LogWarningFormat(format, args);
//}