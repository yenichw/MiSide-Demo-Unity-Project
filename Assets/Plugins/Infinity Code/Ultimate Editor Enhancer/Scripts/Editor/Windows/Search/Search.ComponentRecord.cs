/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    public partial class Search
    {
        internal class ComponentRecord : Record
        {
            private static Dictionary<int, int> typeHashes;
            private static List<string> typeNames;
            private static List<string> nicifyNames;

            public Component component;
            private string _type;

            public GameObject gameObject
            {
                get => component.gameObject;
            }

            public override Object target
            {
                get => component;
            }

            public override string tooltip
            {
                get
                {
                    if (_tooltip == null)
                    {
                        _tooltip = GameObjectUtils.GetTransformPath(component.transform).Append(" (")
                            .Append(ObjectNames.NicifyVariableName(component.GetType().Name)).Append(")").ToString();
                    }

                    return _tooltip;
                }
            }

            public override string type
            {
                get => _type;
            }

            public ComponentRecord(Component component)
            {
                this.component = component;

                if (typeHashes == null)
                {
                    typeHashes = new Dictionary<int, int>(64);
                    typeNames = new List<string>(64);
                    nicifyNames = new List<string>(64);
                }

                Type t = component.GetType();
                _type = t.Name.ToLowerInvariant();

                string componentName;
                string nicifyName;
                GetNames(t, out componentName, out nicifyName);

                string gameObjectName = component.gameObject.name;

                if (componentName != nicifyName)
                {
                    search = new[]
                    {
                        componentName,
                        nicifyName,
                        gameObjectName + " " + nicifyName
                    };
                }
                else
                {
                    search = new[]
                    {
                        componentName,
                        gameObjectName + " " + nicifyName
                    };
                }
            }

            private void GetNames(Type type, out string componentName, out string nicifyName)
            {
                int typeIndex;
                if (typeHashes.TryGetValue(type.GetHashCode(), out typeIndex))
                {
                    componentName = typeNames[typeIndex];
                    nicifyName = nicifyNames[typeIndex];
                }
                else
                {
                    componentName = type.Name;
                    nicifyName = ObjectNames.NicifyVariableName(componentName);

                    typeHashes.Add(type.GetHashCode(), typeNames.Count);
                    typeNames.Add(componentName);
                    nicifyNames.Add(nicifyName);
                }
            }

            public override void Dispose()
            {
                base.Dispose();
                component = null;
            }

            public override void Select(int state)
            {
                if (state == 2) ComponentWindow.Show(component);
                else if (state == 1)
                {
                    Selection.activeGameObject = gameObject;
                    EditorGUIUtility.PingObject(gameObject);
                    if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.Focus();
                }
            }

            protected override void ShowContextMenu(int index)
            {
                ComponentUtils.ShowContextMenu(component);
            }

            public override void UpdateGameObjectName(GameObject go)
            {
                if (go == gameObject) return;

                search[search.Length - 2] = go.name;
                search[search.Length - 1] = go.name + " " + search[search.Length - 3];
            }
        }
    }
}