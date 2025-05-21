/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer
{
    [InitializeOnLoad]
    public static class LogManager
    {
        private const int MaxEntries = 999;
        
        private const double Frequency = 1;
        private const int ErrorMode = 16640;
        private const int ErrorMode2 = 8405248;
        private const int ExceptionMode = 4325632;
        private const int ExceptionMode2 = 12714240;

        private static int countEntries;
        private static Entry[] entriesArr = new Entry[MaxEntries];
        private static Entry[] newEntries = new Entry[MaxEntries];
        private static Dictionary<int, List<Entry>> entriesDict;
        private static double lastUpdatedTime;
        private static bool isDirty;
        private static int lastCount;
        private static object lastEntry;

        static LogManager()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
            Application.logMessageReceived += OnLogMessageReceived;

            Application.logMessageReceivedThreaded -= OnLogMessageReceived;
            Application.logMessageReceivedThreaded += OnLogMessageReceived;

            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;

            entriesDict = new Dictionary<int, List<Entry>>();

            UpdateEntries();
        }

        public static List<Entry> GetEntries(int id)
        {
            List<Entry> _entries;
            return entriesDict.TryGetValue(id, out _entries) ? _entries : null;
        }

        private static void InsertNewEntries(int count)
        {
            for (int i = Math.Max(countEntries, MaxEntries - count) - 1; i >= 0; i--)
            {
                entriesArr[i + count] = entriesArr[i];
            }

            for (int i = 0; i < count; i++) entriesArr[i] = newEntries[i];
        }

        private static void OnLogMessageReceived(string condition, string stacktrace, LogType type)
        {
            isDirty = true;
        }

        private static void OnUpdate()
        {
            if (!Prefs.hierarchyErrorIcons) return;

            if (isDirty)
            {
                UpdateEntries();
            }
            else if (EditorApplication.timeSinceStartup - lastUpdatedTime > Frequency)
            {
                int currentCount = LogEntriesRef.GetCount();
                if (lastCount != currentCount) UpdateEntries();
                lastCount = currentCount;
            }
        }

        private static void UpdateEntries()
        {
            try
            {
                int countNewEntries = UpdateNewEntries();
                if (countNewEntries != 0)
                {
                    InsertNewEntries(countNewEntries);
                    UpdateDictionary();

                    EditorApplication.RepaintHierarchyWindow();
                }
            }
            catch (Exception ex)
            {
                Log.Add(ex);
            }

            LogEntriesRef.EndGettingEntries();
            lastUpdatedTime = EditorApplication.timeSinceStartup;
            isDirty = false;
        }

        private static void UpdateDictionary()
        {
            entriesDict.Clear();

            for (int i = 0; i < countEntries; i++)
            {
                Entry entry = entriesArr[i];
                
                List<Entry> entries;
                if (!entriesDict.TryGetValue(entry.instanceId, out entries))
                {
                    entries = new List<Entry>();
                    entriesDict[entry.instanceId] = entries;
                }
                
                entries.Add(entry);
            }
        }

        private static int UpdateNewEntries()
        {
            int count = LogEntriesRef.StartGettingEntries();
            object nativeEntry = Activator.CreateInstance(LogEntryRef.type);

            int maxRecords = Mathf.Min(count, 999);
            int countNewEntries = 0;

            for (int i = 0; i < maxRecords; i++)
            {
                LogEntriesRef.GetEntryInternal(i, nativeEntry);
                if (nativeEntry == lastEntry) break;
                if (i == 0) lastEntry = nativeEntry;

                int mode = LogEntryRef.GetMode(nativeEntry);
                if (mode != ErrorMode &&
                    mode != ErrorMode2 &&
                    mode != ExceptionMode &&
                    mode != ExceptionMode2)
                {
                    continue;
                }
                
                int instanceID = LogEntryRef.GetInstanceID(nativeEntry);
                if (instanceID == 0) continue;

                Entry entry = new Entry(nativeEntry, i);
                Object reference = EditorUtility.InstanceIDToObject(instanceID);
                if (!reference) continue;

                GameObject target = reference as GameObject;

                if (!target)
                {
                    Component component = reference as Component;
                    if (!component) continue;
                    target = component.gameObject;
                }
                
                entry.instanceId = target.GetInstanceID();
                newEntries[countNewEntries++] = entry;
            }

            return countNewEntries;
        }

        public class Entry
        {
            public string message;
            public int instanceId;
            private int index;

            public Entry(object nativeEntry, int index)
            {
                this.index = index;
                message = LogEntryRef.GetMessage(nativeEntry);
            }

            public void Open()
            {
                LogEntriesRef.RowGotDoubleClicked(index);
            }
        }
    }
}