// based on the original game.Yen Chezky(yenichw)
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UnityEditor.Rendering.Universal
{
    [CustomEditor(typeof(ScriptableRendererFeature), true)]
    public class ScriptableRendererFeatureEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawPropertiesExcluding(serializedObject, "m_Script");
        }
    }
}
