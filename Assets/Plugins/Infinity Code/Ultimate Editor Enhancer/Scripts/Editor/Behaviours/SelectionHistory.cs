/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Behaviors
{
    [InitializeOnLoad]
    public static class SelectionHistory
    {
        private const int MaxRecords = 30;

        private static List<SelectionRecord> _records;
        private static int index = -1;
        private static bool ignoreNextAdd = false;

        public static List<SelectionRecord> records
        {
            get { return _records; }
        }

        public static int activeIndex
        {
            get { return index; }
        }

        static SelectionHistory()
        {
            Selection.selectionChanged += SelectionChanged;

            KeyManager.KeyBinding prevBinding = KeyManager.AddBinding();
            prevBinding.OnValidate += ValidatePrev;
            prevBinding.OnPress += Prev;

            KeyManager.KeyBinding nextBinding = KeyManager.AddBinding();
            nextBinding.OnValidate += ValidateNext;
            nextBinding.OnPress += Next;

            _records = new List<SelectionRecord>();
            if (Selection.instanceIDs.Length > 0) Add(Selection.instanceIDs);
        }

        public static void Add(params int[] ids)
        {
            if (ignoreNextAdd)
            {
                ignoreNextAdd = false;
                return;
            }

            while (_records.Count > index + 1)
            {
                _records.RemoveAt(_records.Count - 1);
            }

            while (_records.Count > MaxRecords - 1)
            {
                _records.RemoveAt(_records.Count - 1);
            }

            SelectionRecord r = new SelectionRecord();
            r.ids = ids;
            r.names = ids.Select(id =>
            {
                Object obj = EditorUtility.InstanceIDToObject(id);
                return obj != null ? obj.name : null;
            }).ToArray();
            _records.Add(r);

            index = _records.Count - 1;
        }

        public static void Clear()
        {
            _records.Clear();
        }

        public static void Next()
        {
            if (_records.Count == 0 || index >= _records.Count - 1) return;

            index++;
            ignoreNextAdd = true;

            Selection.instanceIDs = _records[index].ids;

            Event.current.Use();
        }

        public static void Prev()
        {
            if (_records.Count == 0 || index <= 0) return;

            index--;
            ignoreNextAdd = true;

            Selection.instanceIDs = _records[index].ids;

            Event.current.Use();
        }

        private static void SelectionChanged()
        {
            if (Prefs.selectionHistory) Add(Selection.instanceIDs);
        }

        public static void SetIndex(int newIndex)
        {
            if (newIndex < 0 || newIndex >= records.Count) return;

            ignoreNextAdd = true;

            index = newIndex;
            Selection.instanceIDs = _records[index].ids;
        }

        private static bool ValidateNext()
        {
            if (!Prefs.selectionHistory) return false;
            if (Event.current.modifiers != Prefs.selectionHistoryModifiers) return false;
            return Event.current.keyCode == Prefs.selectionHistoryNextKeyCode;
        }

        private static bool ValidatePrev()
        {
            if (!Prefs.selectionHistory) return false;
            if (Event.current.modifiers != Prefs.selectionHistoryModifiers) return false;
            return Event.current.keyCode == Prefs.selectionHistoryPrevKeyCode;
        }

        public class SelectionRecord
        {
            public int[] ids;
            public string[] names;

            public string GetShortNames()
            {
                if (names == null || names.Length == 0) return string.Empty;
                if (names.Length == 1) return names[0];
                if (names.Length == 2) return names[0] + " + " + names[1];
                return names[0] + " + (" + (names.Length - 1) + " GameObjects)";
            }
        }
    }
}