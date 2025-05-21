/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public partial class Prefs
    {
        public static Vector2Int defaultWindowSize = new Vector2Int(400, 300);

        public class WindowManager : PrefManager
        {
            public override float order
            {
                get { return Order.Window; }
            }

            public override void Draw()
            {
                defaultWindowSize = EditorGUILayout.Vector2IntField("Window Size", defaultWindowSize);
            }
        }
    }
}