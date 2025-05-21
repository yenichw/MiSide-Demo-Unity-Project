// Colorful FX - Unity Asset
// Copyright (c) 2015 - Thomas Hourdel
// http://www.thomashourdel.com

namespace Colorful.Editors
{
    using UnityEditor;

    [CustomEditor(typeof(Noise))]
    public class NoiseEditor : BaseEffectEditor
    {
        SerializedProperty p_Mode;
        SerializedProperty p_Animate;
        SerializedProperty p_Seed;
        SerializedProperty p_Strength;
        SerializedProperty p_LumContribution;

        void OnEnable()
        {
            p_Mode = serializedObject.FindProperty("Mode");
            p_Animate = serializedObject.FindProperty("Animate");
            p_Seed = serializedObject.FindProperty("Seed");
            p_Strength = serializedObject.FindProperty("Strength");
            p_LumContribution = serializedObject.FindProperty("LumContribution");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(p_Mode);
            EditorGUILayout.PropertyField(p_Animate);

            using (new EditorGUI.DisabledGroupScope(p_Animate.boolValue))
            {
                EditorGUILayout.PropertyField(p_Seed);
            }

            EditorGUILayout.PropertyField(p_Strength);
            EditorGUILayout.PropertyField(p_LumContribution, GetContent("Luminance Contribution"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
