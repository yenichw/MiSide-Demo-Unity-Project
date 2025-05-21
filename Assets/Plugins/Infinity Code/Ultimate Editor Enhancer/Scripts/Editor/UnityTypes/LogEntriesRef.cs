/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class LogEntriesRef
    {
        private static MethodInfo _endGettingEntriesMethod;
        private static MethodInfo _getCountMethod;
        private static MethodInfo _getEntryMethod;
        private static MethodInfo _rowGotDoubleClickedMethod;
        private static MethodInfo _startGettingEntriesMethod;
        private static Type _type;

        private static MethodInfo endGettingEntriesMethod
        {
            get
            {
                if (_endGettingEntriesMethod == null) _endGettingEntriesMethod = Reflection.GetMethod(type, "EndGettingEntries", Reflection.StaticLookup);
                return _endGettingEntriesMethod;
            }
        }

        private static MethodInfo getCountMethod
        {
            get
            {
                if (_getCountMethod == null) _getCountMethod = type.GetMethod("GetCount", Reflection.StaticLookup);
                return _getCountMethod;
            }
        }

        private static MethodInfo getEntryMethod
        {
            get
            {
                if (_getEntryMethod == null) _getEntryMethod = type.GetMethod("GetEntryInternal", Reflection.StaticLookup);
                return _getEntryMethod;
            }
        }

        private static MethodInfo rowGotDoubleClickedMethod 
        { 
            get
            {
                if (_rowGotDoubleClickedMethod == null) _rowGotDoubleClickedMethod = type.GetMethod("RowGotDoubleClicked", Reflection.StaticLookup);
                return _rowGotDoubleClickedMethod;
            }
        }

        private static MethodInfo startGettingEntriesMethod
        {
            get
            {
                if (_startGettingEntriesMethod == null) _startGettingEntriesMethod = Reflection.GetMethod(type, "StartGettingEntries", Reflection.StaticLookup);
                return _startGettingEntriesMethod;
            }
        }

        public static Type type
        {
            get
            {
                if (_type == null)
                {
                    _type = Reflection.GetEditorType("LogEntries", "UnityEditorInternal");
                    if (_type == null) _type = Reflection.GetEditorType("LogEntries");
                }
                return _type;
            }
        }

        public static void EndGettingEntries()
        {
            endGettingEntriesMethod.Invoke(null, null);
        }

        public static int GetCount()
        {
            return (int) getCountMethod.Invoke(null, null);
        }

        public static bool GetEntryInternal(int row, object outputEntry)
        {
            return (bool) getEntryMethod.Invoke(null, new object[] {row, outputEntry});
        }

        public static void RowGotDoubleClicked(int index)
        {
            rowGotDoubleClickedMethod.Invoke(null, new object[] {index});
        }

        public static int StartGettingEntries()
        {
            return (int)startGettingEntriesMethod.Invoke(null, null);
        }
    }
}