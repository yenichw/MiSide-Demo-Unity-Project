/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.UltimateEditorEnhancer.Attributes;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.ComponentHeader
{
    public static class BoxColliderDetectSize
    {
        private static GUIContent content;
        private static GUIStyle style;
        private static bool inited;
        private static Vector3[] fourCorners;

        [ComponentHeaderButton (ComponentHeaderButtonOrder.BoxColliderDetectSize)]
        public static bool DrawHeaderButton(Rect rectangle, Object[] targets)
        {
            Object target = targets[0];
            if (!Validate(target)) return false;
            if (!inited) Init();

            if (GUI.Button(rectangle, content, style))
            {
                UpdateBounds(targets[0] as BoxCollider);
            }

            return true;
        }

        private static void Init()
        {
            if (content == null)
            {
                content = new GUIContent(EditorIconContents.rectTransformBlueprint.image, "Detect Bounds");
            }

            if (style == null)
            {
                style = new GUIStyle(Styles.iconButton)
                {
                    alignment = TextAnchor.MiddleCenter
                };
            }

            inited = true;
        }

        private static void UpdateBounds(BoxCollider collider)
        {
            GameObject gameObject = collider.gameObject;
            Bounds bounds = GameObjectUtils.GetOriginalBounds(gameObject);
            if (bounds.size == Vector3.zero) return;

            Undo.RecordObject(collider, "Update Collider Bounds");

            collider.center = bounds.center - gameObject.transform.position;
            collider.size = bounds.size;
        }

        private static bool Validate(Object target)
        {
            if (!Prefs.componentExtraHeaderButtons || !Prefs.boxColliderDetect) return false;
            if (!(target is BoxCollider)) return false;
            return true;
        }
    }
}