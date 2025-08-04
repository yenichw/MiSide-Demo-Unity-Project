// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using UnityEditor;
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// ????????????????
    /// </summary>
    [CustomEditor(typeof(MagicaMeshSpring))]
    public class MagicaMeshSpringInspector : ClothEditor
    {
        protected override void OnEnable()
        {
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            MagicaMeshSpring scr = target as MagicaMeshSpring;

            // ?????
            EditorInspectorUtility.DispVersionStatus(scr);
            EditorInspectorUtility.DispDataStatus(scr);

            serializedObject.Update();
            Undo.RecordObject(scr, "CreateMeshSpring");

            // ?????
            if (EditorApplication.isPlaying == false)
                VerifyData();

            // ???????
            EditorInspectorUtility.MonitorButtonInspector();

            EditorGUI.BeginChangeCheck();

            // ???
            MainInspector();

            // ?????
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorPresetUtility.DrawPresetButton(scr, scr.Params);
            {
                var cparam = serializedObject.FindProperty("clothParams");
                if (EditorInspectorUtility.AlgorithmInspector(cparam, scr.HasChangedParam(ClothParams.ParamType.Algorithm), ConvertToLatestAlgorithmParameters))
                    scr.Params.SetChangeParam(ClothParams.ParamType.Algorithm);
                if (EditorInspectorUtility.GravityInspector(cparam))
                    scr.Params.SetChangeParam(ClothParams.ParamType.Gravity);
                if (EditorInspectorUtility.ExternalForceInspector(cparam))
                    scr.Params.SetChangeParam(ClothParams.ParamType.ExternalForce);
                if (EditorInspectorUtility.DragInspector(cparam))
                    scr.Params.SetChangeParam(ClothParams.ParamType.Drag);
                if (EditorInspectorUtility.MaxVelocityInspector(cparam))
                    scr.Params.SetChangeParam(ClothParams.ParamType.MaxVelocity);
                if (EditorInspectorUtility.WorldInfluenceInspector(cparam, scr.HasChangedParam(ClothParams.ParamType.WorldInfluence)))
                    scr.Params.SetChangeParam(ClothParams.ParamType.WorldInfluence);
                if (EditorInspectorUtility.DistanceDisableInspector(cparam, scr.HasChangedParam(ClothParams.ParamType.DistanceDisable)))
                    scr.Params.SetChangeParam(ClothParams.ParamType.DistanceDisable);
                if (EditorInspectorUtility.ClampPositionInspector(cparam, true, scr.HasChangedParam(ClothParams.ParamType.ClampPosition)))
                    scr.Params.SetChangeParam(ClothParams.ParamType.ClampPosition);
                if (EditorInspectorUtility.FullSpringInspector(cparam, scr.HasChangedParam(ClothParams.ParamType.Spring)))
                    scr.Params.SetChangeParam(ClothParams.ParamType.Spring);
                if (EditorInspectorUtility.AdjustRotationInspector(cparam, scr.HasChangedParam(ClothParams.ParamType.AdjustRotation)))
                    scr.Params.SetChangeParam(ClothParams.ParamType.AdjustRotation);
            }
            serializedObject.ApplyModifiedProperties();

            // ?????
            if (EditorApplication.isPlaying == false)
            {
                EditorGUI.BeginDisabledGroup(CheckCreate() == false);

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Create"))
                {
                    Undo.RecordObject(scr, "CreateMeshSpringData");
                    BuildManager.CreateComponent(scr);
                }
                GUI.backgroundColor = Color.white;

                EditorGUI.EndDisabledGroup();
            }
            else
            {
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                GUI.backgroundColor = Color.blue;
                if (GUILayout.Button("Reset Position"))
                {
                    scr.ResetCloth();
                }
                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.Space();

            if (EditorGUI.EndChangeCheck())
            {
                // Scene?????
                SceneView.RepaintAll();
            }
        }

        //=========================================================================================
        /// <summary>
        /// ?????????????
        /// </summary>
        /// <returns></returns>
        protected override bool CheckCreate()
        {
            MagicaMeshSpring scr = target as MagicaMeshSpring;

            if (scr.Deformer == null)
                return false;

            if (scr.Deformer.VerifyData() != Define.Error.None)
                return false;

            return true;
        }

        /// <summary>
        /// ?????
        /// </summary>
        private void VerifyData()
        {
            MagicaMeshSpring scr = target as MagicaMeshSpring;
            if (scr.VerifyData() != Define.Error.None)
            {
                // ?????
                serializedObject.ApplyModifiedProperties();
            }
        }

        //=========================================================================================
        void MainInspector()
        {
            MagicaMeshSpring scr = target as MagicaMeshSpring;

            EditorGUILayout.LabelField("Main Setup", EditorStyles.boldLabel);

            // ?????????????
            EditorGUILayout.PropertyField(serializedObject.FindProperty("virtualDeformer"));

            EditorGUILayout.Space();

            // ????????????
            scr.CenterTransform = EditorGUILayout.ObjectField(
                "Center Transform", scr.CenterTransform, typeof(Transform), true
                ) as Transform;
            scr.DirectionAxis = (MagicaMeshSpring.Axis)EditorGUILayout.EnumPopup("Direction Axis", scr.DirectionAxis);

            EditorGUILayout.Space();

            // ?????
            TeamBasicInspector();

            // ????
            CullingInspector();
        }
    }
}
