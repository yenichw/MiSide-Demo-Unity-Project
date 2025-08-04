// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// ?????????????
    /// </summary>
    [CustomEditor(typeof(MagicaBoneCloth))]
    public class MagicaBoneClothInspector : ClothEditor
    {
        protected override void OnEnable()
        {
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            MagicaBoneCloth scr = target as MagicaBoneCloth;

            // ?????
            EditorInspectorUtility.DispVersionStatus(scr);
            EditorInspectorUtility.DispDataStatus(scr);

            serializedObject.Update();
            Undo.RecordObject(scr, "CreateBoneCloth");

            // ?????
            if (EditorApplication.isPlaying == false)
                VerifyData();

            // ???????
            EditorInspectorUtility.MonitorButtonInspector();

            // ???
            MainInspector();

            // ?????
            ColliderInspector();

            // ?????
            SkinningInspector();

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
                if (EditorInspectorUtility.MassInspector(cparam))
                    scr.Params.SetChangeParam(ClothParams.ParamType.Mass);
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

                if (EditorInspectorUtility.ClampDistanceInspector(cparam, scr.HasChangedParam(ClothParams.ParamType.ClampDistance)))
                    scr.Params.SetChangeParam(ClothParams.ParamType.ClampDistance);
                if (EditorInspectorUtility.ClampPositionInspector(cparam, true, scr.HasChangedParam(ClothParams.ParamType.ClampPosition)))
                    scr.Params.SetChangeParam(ClothParams.ParamType.ClampPosition);
                if (EditorInspectorUtility.ClampRotationInspector(cparam, scr.HasChangedParam(ClothParams.ParamType.ClampRotation), scr.ClothData))
                    scr.Params.SetChangeParam(ClothParams.ParamType.ClampRotation);

                if (EditorInspectorUtility.RestoreDistanceInspector(cparam, scr.HasChangedParam(ClothParams.ParamType.RestoreDistance)))
                    scr.Params.SetChangeParam(ClothParams.ParamType.RestoreDistance);
                if (EditorInspectorUtility.RestoreRotationInspector(cparam, scr.HasChangedParam(ClothParams.ParamType.RestoreRotation), scr.ClothData))
                    scr.Params.SetChangeParam(ClothParams.ParamType.RestoreRotation);
                if (scr.ClothTarget.IsMeshConnection)
                {
                    if (EditorInspectorUtility.TriangleBendInspector(cparam, scr.HasChangedParam(ClothParams.ParamType.TriangleBend), scr.ClothData))
                        scr.Params.SetChangeParam(ClothParams.ParamType.TriangleBend);
                }
                if (EditorInspectorUtility.CollisionInspector(cparam, scr.HasChangedParam(ClothParams.ParamType.ColliderCollision)))
                    scr.Params.SetChangeParam(ClothParams.ParamType.ColliderCollision);
                if (EditorInspectorUtility.PenetrationInspector(serializedObject, cparam, scr.HasChangedParam(ClothParams.ParamType.Penetration)))
                    scr.Params.SetChangeParam(ClothParams.ParamType.Penetration);
                //if (EditorInspectorUtility.BaseSkinningInspector(serializedObject, cparam))
                //    scr.Params.SetChangeParam(ClothParams.ParamType.BaseSkinning);
                if (EditorInspectorUtility.RotationInterpolationInspector(cparam, scr.HasChangedParam(ClothParams.ParamType.RotationInterpolation)))
                    scr.Params.SetChangeParam(ClothParams.ParamType.RotationInterpolation);
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
                    Undo.RecordObject(scr, "CreateBoneCloth");
                    // ????????????????????
                    if (scr.ClothSelection == null)
                        InitSelectorData();
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
            MagicaBoneCloth scr = target as MagicaBoneCloth;
            if (scr.VerifyData() != Define.Error.None)
            {
                // ?????
                serializedObject.ApplyModifiedProperties();
            }
        }

        //=========================================================================================
        void MainInspector()
        {
            MagicaBoneCloth scr = target as MagicaBoneCloth;

            EditorGUILayout.LabelField("Main Setup", EditorStyles.boldLabel);

            // ??????
            EditorInspectorUtility.DrawObjectList<Transform>(
                serializedObject.FindProperty("clothTarget.rootList"),
                scr.gameObject,
                false, true,
                () => scr.gameObject.transform.root.GetComponentsInChildren<Transform>()
                );
            //EditorGUILayout.Space();

            // ?????
            EditorGUILayout.PropertyField(serializedObject.FindProperty("clothTarget.connection"), new GUIContent("Connection Mode"));
            if (scr.ClothTarget.IsMeshConnection)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("clothTarget.sameSurfaceAngle"), new GUIContent("Same Surface Angle"));
            if (scr.ClothTarget.Connection == BoneClothTarget.ConnectionMode.MeshSequentialLoop || scr.ClothTarget.Connection == BoneClothTarget.ConnectionMode.MeshSequentialNoLoop)
            {
                // ??
                EditorGUILayout.HelpBox("RootList must be registered in the order of connection for sequential connection.", MessageType.Info);
            }

            // ?????
            TeamBasicInspector();

            // ?????????
            //scr.ClothTarget.IsAnimationBone = EditorGUILayout.Toggle("Is Animation Bones", scr.ClothTarget.IsAnimationBone);
            //scr.ClothTarget.IsAnimationPosition = EditorGUILayout.Toggle("Is Animation Position", scr.ClothTarget.IsAnimationPosition);
            //scr.ClothTarget.IsAnimationRotation = EditorGUILayout.Toggle("Is Animation Rotation", scr.ClothTarget.IsAnimationRotation);

            // ??????
            DrawInspectorGUI(scr);
            EditorGUILayout.Space();

            // ????
            CullingInspector();
        }

        //=========================================================================================
        /// <summary>
        /// ?????????
        /// ???????????????????????????
        /// </summary>
        /// <param name="selectorData"></param>
        protected override void OnResetSelector(List<int> selectorData)
        {
            // ?????????????????????
            MagicaBoneCloth scr = target as MagicaBoneCloth;

            // ??????????????
            // ??????????????????????????
            if (scr.ClothSelection != null)
            {
                // ??
                var sel = scr.ClothSelection.GetSelectionData(scr.MeshData, null);
                for (int i = 0; i < selectorData.Count; i++)
                {
                    if (i < sel.Count)
                        selectorData[i] = sel[i];
                }
            }
            else
            {
                // ??
                var tlist = scr.GetTransformList();
                for (int i = 0; i < tlist.Count; i++)
                {
                    var t = tlist[i];
                    int data = 0;
                    if (scr.ClothTarget.GetRootIndex(t) >= 0)
                    {
                        // ??
                        data = SelectionData.Fixed;
                    }
                    else
                    {
                        // ??
                        data = SelectionData.Move;
                    }
                    selectorData[i] = data;
                }
            }
        }

        /// <summary>
        /// ????????
        /// </summary>
        /// <param name="selectorData"></param>
        protected override void OnFinishSelector(List<int> selectorData)
        {
            MagicaBoneCloth scr = target as MagicaBoneCloth;

            // ????????????(?????????????)
            var sel = CreateSelection(scr, "clothSelection");

            // ????????
            sel.SetSelectionData(scr.MeshData, selectorData, null);

            // ?????????????????????
            if (scr.ClothSelection != null && scr.ClothSelection.Compare(sel))
                return;

            //if (scr.ClothSelection != null)
            //    Undo.RecordObject(scr.ClothSelection, "Set Selector");

            // ??
            var cdata = serializedObject.FindProperty("clothSelection");
            cdata.objectReferenceValue = sel;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
