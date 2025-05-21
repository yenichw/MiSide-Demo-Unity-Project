/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer.Editors
{
    [CustomEditor(typeof(ViewStateReadme))]
    public class ViewStateReadmeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            string message = "To open View Gallery select Window / Infinity Code / Ultimate Editor Enhancer / View Gallery or use shortcut CTRL + SHIFT + G (OSX: COMMAND + SHIFT + G).\n\nTo start Quick Preview hold CTRL + SHIFT + Q (OSX: COMMAND + SHIFT + Q) in Scene View.\n\nTo change the shortcuts, open the settings (Window / Infinity Code / Ultimate Editor Enhancer / Settings).";
            EditorGUILayout.HelpBox(message, MessageType.Info);
        }
    }
}