// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using UnityEditor;

namespace MagicaCloth
{
    /// <summary>
    /// ??????????????
    /// </summary>
    [CustomEditor(typeof(MagicaAvatarParts))]
    public class MagicaAvatarPartsInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            MagicaAvatarParts scr = target as MagicaAvatarParts;

            // ?????
            //EditorInspectorUtility.DispVersionStatus(scr);
            EditorInspectorUtility.DispDataStatus(scr);
            //DrawDefaultInspector();

            serializedObject.Update();

            MainInspector();
        }

        //=========================================================================================
        private void MainInspector()
        {
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("mergeAvatarCollider"));
        }
    }
}