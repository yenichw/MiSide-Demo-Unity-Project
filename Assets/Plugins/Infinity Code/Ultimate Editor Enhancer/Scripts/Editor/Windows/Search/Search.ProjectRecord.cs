/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    public partial class Search
    {
        internal class ProjectRecord : Record
        {
            internal string path;

            private Object _asset;
            private string _type;
            private bool isMissed;

            public Object asset
            {
                get
                {
                    if (_asset == null && !isMissed)
                    {
                        _asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                        isMissed = _asset == null;
                    }
                    return _asset;
                }
            }

            public override string label
            {
                get
                {
                    if (_label == null)
                    {
                        int len = path.Length - 7;
                        if (path[path.Length - 1] == '/') len--;
                        _label = path.Substring(7, len);

                        int lastDot = _label.LastIndexOf(".", StringComparison.InvariantCulture);
                        if (lastDot != -1) _label = _label.Substring(0, lastDot);

                        if (_label.Length > MaxLabelLength)
                        {
                            int start = _label.IndexOf("/", _label.Length - MaxLabelLength + 3, StringComparison.InvariantCulture);
                            if (start != -1) _label = "..." + _label.Substring(start);
                            else _label = "..." + _label.Substring(_label.Length - MaxLabelLength + 3);
                        }
                    }

                    return _label;
                }
            }

            public override string tooltip
            {
                get
                {
                    if (_tooltip == null && asset != null)
                    {
                        _tooltip = asset.GetType().Name + "\n" + path.Substring(7);
                    }
                    return _tooltip;
                }
            }

            public override Object target
            {
                get => asset;
            }

            public override string type
            {
                get
                {
                    if (_type == null)
                    {
                        Type assetType = AssetDatabase.GetMainAssetTypeAtPath(path);
                        if (assetType != null) _type = assetType.Name.ToLowerInvariant();
                        else _type = "missed";
                    }
                    return _type;
                }
            }

            public ProjectRecord(string path)
            {
                this.path = path;

                int lastDot = -1;
                int lastSlash = 6;

                for (int i = path.Length - 2; i >= 8; i--)
                {
                    char c = path[i];
                    if (c == '.')
                    {
                        if (lastDot == -1) lastDot = i;
                    }
                    else if (c == '/')
                    {
                        lastSlash = i;
                        break;
                    }
                }

                search = new[]
                {
                    path.Substring(lastSlash + 1, lastDot == -1? path.Length - lastSlash - 1: lastDot - lastSlash - 1)
                };
            }

            public override void Dispose()
            {
                base.Dispose();
                _asset = null;
            }

            protected override void PrepareContextMenuExtraItems(GenericMenuEx menu)
            {
                base.PrepareContextMenuExtraItems(menu);

                if (type == "monoscript" && Selection.activeGameObject != null)
                {
                    menu.Add("Add Component", () =>
                    {
                        Select(-1);
                        EventManager.BroadcastClosePopup();
                    });
                }
                else if (target is SceneAsset)
                {
                    menu.Add("Open Additive", () =>
                    {
                        string path = AssetDatabase.GetAssetPath(target);
                        EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                    });
                }

                menu.Add("Show In Explorer", () =>
                {
                    EditorUtility.RevealInFinder(path);
                    EventManager.BroadcastClosePopup();
                });
            }

            protected override int ProcessDoubleClick(Event e)
            {
                int state;
                if (e.modifiers == EventModifiers.Control) state = 2;
                else if (type == "monoscript" &&
#if UNITY_EDITOR_OSX
                    e.modifiers == (EventModifiers.Command | EventModifiers.Shift)
#else
                    e.modifiers == (EventModifiers.Control | EventModifiers.Shift)
#endif
                ) state = 3;
                else state = 1;
                return state;
            }

            public override void Select(int state)
            {
                if (state == 2) AssetDatabase.OpenAsset(target);
                else if (state == -1 && type == "monoscript" && Selection.activeGameObject != null)
                {
                    Selection.activeGameObject.AddComponent((target as MonoScript).GetClass());
                }
                else if (state == 1)
                {
                    EditorGUIUtility.PingObject(target);
                    Selection.activeObject = target;
                }
            }

            protected override void StartDrag(Event e)
            {
                isDragStarted = true;
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.paths = new[] { path };
                DragAndDrop.objectReferences = new[] { target };
                DragAndDrop.StartDrag("Dragging " + target.name);
                e.Use();
            }
        }
    }
}