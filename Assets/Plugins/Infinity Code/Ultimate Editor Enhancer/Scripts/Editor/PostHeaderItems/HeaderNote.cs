/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using System.Linq;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.PostHeader
{
    [InitializeOnLoad]
    public class HeaderNote : PostHeaderItem
    {
        private static List<SavableNote> noteToSave = new List<SavableNote>();

        static HeaderNote()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        
        public override void OnBlockGUI(Object target)
        {
            if (!Prefs.inspectorNotes) return;

            GameObject go = target as GameObject;
            if (go == null) return;

            NoteItem note;
            NoteManager.TryGetValue(go, out note);
            if (!note.expanded) return;

            bool prevEmpty = note.isEmpty;
            
            GUIStyle style = EditorStyles.textArea;
            Vector2 size = style.CalcSize(TempContent.Get(note.text));

            EditorGUI.BeginChangeCheck();
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(Mathf.Min(size.y, note.maxHeight)));
            note.text = EditorGUIRef.ScrollableTextAreaInternal(rect, note.text, ref note.scrollPosition, style);
            if (EditorGUI.EndChangeCheck())
            {
                if (!EditorApplication.isPlaying)
                {
                    if (prevEmpty)
                    {
                        if (ReferenceManager.notes.FirstOrDefault(n => n.gid == note.gid) == null) ReferenceManager.notes.Add(note);
                    }
                    else if (note.isEmpty) ReferenceManager.notes.Remove(note); 
                    ReferenceManager.Save();
                }
                else
                {
                    RuntimeNote runtimeNote = go.GetComponent<RuntimeNote>();
                    if (runtimeNote == null)
                    {
                        runtimeNote = go.AddComponent<RuntimeNote>();
                        runtimeNote.hideFlags = HideFlags.HideInInspector;
                    }
                    runtimeNote.text = note.text;
                }
            }

            if (size.y > note.maxHeight)
            {
                if (GUILayout.Button("Expand", EditorStyles.toolbarButton)) note.maxHeight = size.y;
            }
            else if (size.y > 100)
            {
                if (GUILayout.Button("Collapse", EditorStyles.toolbarButton)) note.maxHeight = 100;
            }
        }
        
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                RuntimeNote[] runtimeNotes = ObjectHelper.FindObjectsOfType<RuntimeNote>();
                foreach (RuntimeNote runtimeNote in runtimeNotes)
                {
                    SavableNote note = new SavableNote
                    {
                        gameObject = runtimeNote.gameObject,
                        text = runtimeNote.text
                    };
                    
                    noteToSave.Add(note);
                }
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                if (noteToSave.Count == 0) return;
                
                NoteManager.ClearCache();
                
                foreach (SavableNote savableNote in noteToSave)
                {
                    if (savableNote.gameObject == null) continue;
                    
                    NoteItem note;
                    NoteManager.TryGetValue(savableNote.gameObject, out note);
                    note.text = savableNote.text;

                    bool contains = ReferenceManager.notes.Contains(note);

                    if (!note.isEmpty)
                    {
                        if (!contains) ReferenceManager.notes.Add(note);
                    }
                    else if (note.isEmpty)
                    {
                        if (contains) ReferenceManager.notes.Remove(note);
                    } 
                }
                
                NoteManager.RemoveEmptyNotes();
                
                ReferenceManager.Save();
                noteToSave.Clear();
            }
        }

        public override void OnRowGUI(Object target)
        {
            if (!Prefs.inspectorNotes) return;

            GameObject go = target as GameObject;
            if (go == null) return;

            NoteItem note;
            NoteManager.TryGetValue(go, out note);

            GUIContent content = note.isEmpty ? 
                TempContent.Get(Icons.noteEmpty, "Add Note\nRight click to open Note Manager") : 
                TempContent.Get(Icons.note, "Show/Hide Note\nRight click to open Note Manager");
            ButtonEvent buttonEvent = GUILayoutUtils.ToggleButton(content, EditorStyles.toolbarButton, note.expanded, GUILayout.Width(20), GUILayout.Height(20));
            if (buttonEvent == ButtonEvent.click)
            {
                Event e = Event.current;
                if (e.button == 0) note.expanded = !note.expanded;
                else if (e.button == 1) NoteManager.Open();
            }
        }
        
        internal class SavableNote
        {
            public GameObject gameObject;
            public string text;
        }
    }
}