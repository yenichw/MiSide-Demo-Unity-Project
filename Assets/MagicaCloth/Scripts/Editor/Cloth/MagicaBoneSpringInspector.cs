// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using UnityEditor;
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// ???????????????
    /// </summary>
    [CustomEditor(typeof(MagicaBoneSpring))]
    public class MagicaBoneSpringInspector : ClothEditor
    {
        protected override void OnEnable()
        {
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            MagicaBoneSpring scr = target as MagicaBoneSpring;

            // ?????
            EditorInspectorUtility.DispVersionStatus(scr);
            EditorInspectorUtility.DispDataStatus(scr);

            serializedObject.Update();
            Undo.RecordObject(scr, "CreateBoneSpring");

            // ?????
            if (EditorApplication.isPlaying == false)
                VerifyData();

            // ???????
            EditorInspectorUtility.MonitorButtonInspector();

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
                if (EditorInspectorUtility.RadiusInspector(cparam))
                    scr.Params.SetChangeParam(ClothParams.ParamType.Radius);
                //if (EditorInspectorUtility.MassInspector(cparam))
                //    scr.Params.SetChangeParam(ClothParams.ParamType.Mass);
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
                if (EditorInspectorUtility.SimpleSpringInspector(cparam, scr.HasChangedParam(ClothParams.ParamType.Spring)))
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
                    Undo.RecordObject(scr, "CreateBoneSpring");
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
        }

        //=========================================================================================
        /// <summary>
        /// ?????????????
        /// </summary>
        /// <returns></returns>
        protected override bool CheckCreate()
        {
            if (PointSelector.EditEnable)
                return false;

            return true;
        }

        /// <summary>
        /// ?????
        /// </summary>
        private void VerifyData()
        {
            MagicaBoneSpring scr = target as MagicaBoneSpring;
            if (scr.VerifyData() != Define.Error.None)
            {
                // ?????
                serializedObject.ApplyModifiedProperties();
            }
        }

        //=========================================================================================
        void MainInspector()
        {
            MagicaBoneSpring scr = target as MagicaBoneSpring;

            EditorGUILayout.LabelField("Main Setup", EditorStyles.boldLabel);

            // ??????
            EditorInspectorUtility.DrawObjectList<Transform>(
                serializedObject.FindProperty("clothTarget.rootList"),
                scr.gameObject,
                false, true,
                () => scr.gameObject.transform.root.GetComponentsInChildren<Transform>()
                );

            // ?????
            TeamBasicInspector();

            // ?????????
            //scr.ClothTarget.IsAnimationBone = EditorGUILayout.Toggle("Is Animation Bones", scr.ClothTarget.IsAnimationBone);
            //scr.ClothTarget.IsAnimationPosition = EditorGUILayout.Toggle("Is Animation Position", scr.ClothTarget.IsAnimationPosition);
            //scr.ClothTarget.IsAnimationRotation = EditorGUILayout.Toggle("Is Animation Rotation", scr.ClothTarget.IsAnimationRotation);

            // ??????
            //DrawInspectorGUI(scr);

            EditorGUILayout.Space();

            // ????
            CullingInspector();
        }
    }
}
