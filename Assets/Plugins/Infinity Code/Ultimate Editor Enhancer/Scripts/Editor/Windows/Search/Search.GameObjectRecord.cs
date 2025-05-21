/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    public partial class Search
    {
        internal class GameObjectRecord : Record
        {
            private GameObject _gameObject;

            public GameObject gameObject
            {
                get => _gameObject;
            }

            public override Object target
            {
                get => _gameObject;
            }

            public override string tooltip
            {
                get
                {
                    if (_tooltip == null) _tooltip = GameObjectUtils.GetGameObjectPath(_gameObject).ToString();
                    return _tooltip;
                }
            }

            public override string type
            {
                get => "gameobject";
            }

            public GameObjectRecord(GameObject gameObject)
            {
                _gameObject = gameObject;

                search = new[]
                {
                    gameObject.name
                };
            }

            public override void Dispose()
            {
                base.Dispose();
                _gameObject = null;
            }

            public override void Select(int state)
            {
                if (state == 2)
                {
                    WindowsHelper.ShowInspector();
                }
                else if (state == 1)
                {
                    if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.Focus();
                }

                Selection.activeGameObject = gameObject;
                EditorGUIUtility.PingObject(gameObject);

                if (state == 3)
                {
                    SceneView.FrameLastActiveSceneView();
                }
            }

            protected override void ShowContextMenu(int index)
            {
                GameObjectUtils.ShowContextMenu(false, _gameObject);
            }

            public override void UpdateGameObjectName(GameObject go)
            {
                if (gameObject == go) return;

                search[0] = go.name;
            }
        }
    }
}