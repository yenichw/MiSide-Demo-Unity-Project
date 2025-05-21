/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Linq;
using System.Text.RegularExpressions;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Behaviors
{
    [InitializeOnLoad]
    public static class Rename
    {
        public static GameObject[] gameObjects;
        private static int index;
        private static InputDialog dialog;

        static Rename()
        {
            KeyManager.KeyBinding binding = KeyManager.AddBinding();
            binding.OnValidate = OnValidate;
            binding.OnPress = OnInvoke;
        }

        private static void DrawExtra(InputDialog dialog)
        {
            index = int.MinValue;
            EditorGUILayout.LabelField("Preview: " + ReplaceTokens(gameObjects[0], dialog.text));

            EditorGUILayout.LabelField("Tokens:");
            EditorGUILayout.LabelField("{C} - counter");
            EditorGUILayout.LabelField("{C:N} - counter with initial number");
            EditorGUILayout.LabelField("{S} - sibling index");
            EditorGUILayout.LabelField("{START:LEN} - part of the original name");
        }

        public static void OnDialogClose(InputDialog dialog)
        {
            gameObjects = null;
        }

        private static void OnInvoke()
        {
            Event e = Event.current;
            if (e.keyCode != KeyCode.F2) return;
            if (e.modifiers != EventModifiers.FunctionKey) return;

            EditorWindow wnd = EditorWindow.focusedWindow;
            if (wnd.GetType() == typeof(ComponentWindow))
            {
                gameObjects = new[]
                {
                    (wnd as ComponentWindow).component.gameObject
                };
            }
            else if (wnd.GetType() == typeof(PinAndClose))
            {
                ComponentWindow cw = (wnd as PinAndClose).targetWindow as ComponentWindow;
                gameObjects = new[]
                {
                    cw.component.gameObject
                };
            }
            else
            {
                gameObjects = Selection.gameObjects.Where(g => g.scene.name != null).ToArray();
                if (Selection.gameObjects.Length == 0) return;
            }

            if (gameObjects.Length == 0) return;

            if (gameObjects.Length == 1)
            {
                dialog = InputDialog.Show("Enter a new GameObject name", gameObjects[0].name, OnRename);
                dialog.OnClose += OnDialogClose;
            }
            else ShowMassRename();
        }

        public static void OnRename(string name)
        {
            if (gameObjects == null || gameObjects.Length == 0) return;

            name = name.Trim();
            index = int.MinValue;

            Undo.RecordObjects(gameObjects, "Rename GameObjects");

            if (gameObjects.Length == 1)
            {
                foreach (GameObject go in gameObjects.Where(g => g.scene.name != null))
                {
                    go.name = name;
                }
            }
            else
            {
                foreach (GameObject go in gameObjects.Where(g => g.scene.name != null).OrderBy(g => g.transform.GetSiblingIndex()))
                {
                    go.name = ReplaceTokens(go, name);
                }
            }

            gameObjects = null;
        }

        private static bool OnValidate()
        {
            if (!Prefs.renameByShortcut) return false;

            EditorWindow wnd = EditorWindow.focusedWindow;
            if (wnd == null) return false;
            Type type = wnd.GetType();
            return type == typeof(SceneView) || 
                   type == InspectorWindowRef.type || 
                   type == typeof(ComponentWindow) || 
                   type == typeof(LayoutWindow) ||
                   type == ConsoleWindowRef.type ||
                   (type == SceneHierarchyWindowRef.type && Selection.gameObjects.Length > 1) || 
                   (type == typeof(PinAndClose) && (wnd as PinAndClose).targetWindow is ComponentWindow);
        }

        private static string ReplaceTokens(GameObject go, string name)
        {
            return Regex.Replace(name, @"{[\w\d:-]+}", delegate (Match match)
            {
                string v = match.Value.Trim('{', '}');
                if (char.ToUpperInvariant(v[0]) == 'C')
                {
                    if (index == int.MinValue)
                    {
                        if (v.Length > 2 && v[1] == ':')
                        {
                            int n;
                            if (int.TryParse(v.Substring(2), out n)) index = n;
                        }

                        if (index == int.MinValue) index = 1;
                    }
                    int i = index++;
                    return i.ToString();
                }

                if (char.ToUpperInvariant(v[0]) == 'S') return go.transform.GetSiblingIndex().ToString();

                string[] ss = v.Split(':');
                if (ss.Length >= 2)
                {
                    string original = go.name;
                    int start = 0, len = 0;

                    if (!string.IsNullOrEmpty(ss[0]) && !int.TryParse(ss[0], out start)) return "";

                    if (start < 0) start = original.Length + start;

                    if (!string.IsNullOrEmpty(ss[1]))
                    {
                        if (!int.TryParse(ss[1], out len)) return "";
                        if (len < 0) len = original.Length - start + len;
                    }
                    else len = original.Length - start;

                    if (original.Length <= start) return "";
                    if (original.Length < start + len) len = original.Length - start;
                    return original.Substring(start, len);
                }

                return "";
            });
        }

        public static void Show(params GameObject[] targets)
        {
            if (targets == null || targets.Length == 0) return;

            gameObjects = targets;
            
            if (targets.Length == 1)
            {
                dialog = InputDialog.Show("Enter a new GameObject name", gameObjects[0].name, OnRename);
                dialog.OnClose += OnDialogClose;
            }
            else ShowMassRename();
        }

        private static void ShowMassRename()
        {
            dialog = InputDialog.Show("Enter a new GameObjects name", gameObjects[0].name, OnRename);
            dialog.OnClose += OnDialogClose;
            dialog.OnDrawExtra += DrawExtra;
            dialog.minSize = new Vector2(dialog.minSize.x, 168);
            index = int.MinValue;
        }
    }
}