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
    /// ???????????
    /// </summary>
    public abstract class ClothEditor : Editor
    {
        /// <summary>
        /// ????????????
        /// </summary>
        PointSelector pointSelector = new PointSelector();

        /// <summary>
        /// ???????????
        /// </summary>
        List<int> selectorData = new List<int>();

        /// <summary>
        /// ??????????????????????
        /// </summary>
        IEditorMesh editorMesh;

        //=========================================================================================
        protected virtual void OnEnable()
        {
            pointSelector.EnableEdit();
        }

        protected virtual void OnDisable()
        {
            pointSelector.DisableEdit(this);
        }

        /// <summary>
        /// ?????????
        /// ???????????????????????????
        /// ?????????????????selectorData???????????????
        /// </summary>
        /// <param name="selectorData"></param>
        protected virtual void OnResetSelector(List<int> selectorData) { }

        /// <summary>
        /// ????????
        /// </summary>
        /// <param name="selectorData"></param>
        protected virtual void OnFinishSelector(List<int> selectorData) { }

        /// <summary>
        /// ??????GUI??????
        /// </summary>
        /// <param name="clothData"></param>
        /// <param name="editorMesh"></param>
        protected void DrawInspectorGUI(IEditorMesh editorMesh)
        {
            this.editorMesh = editorMesh;

            if (editorMesh == null)
                return;

            pointSelector.DrawInspectorGUI(this, StartEdit, EndEdit);
        }

        /// <summary>
        /// ?????????????
        /// </summary>
        protected void InitSelectorData()
        {
            // ???????
            List<Vector3> wposList;
            List<Vector3> wnorList;
            List<Vector3> wtanList;
            int meshVertexCount = editorMesh.GetEditorPositionNormalTangent(out wposList, out wnorList, out wtanList);

            // ????????
            selectorData.Clear();
            for (int i = 0; i < meshVertexCount; i++)
                selectorData.Add(0); // Invalid

            // ????
            OnResetSelector(selectorData);

            // ???????
            OnFinishSelector(selectorData);
        }


        //=============================================================================================
        /// <summary>
        /// ?????????????
        /// </summary>
        /// <returns></returns>
        protected abstract bool CheckCreate();

        //=============================================================================================
        /// <summary>
        /// ????????
        /// </summary>
        /// <param name="pointSelector"></param>
        void StartEdit(PointSelector pointSelector)
        {
            // ???????
            // ??????????????
            pointSelector.AddPointType("Move Point", Color.green, SelectionData.Move);
            pointSelector.AddPointType("Fixed Point", Color.red, SelectionData.Fixed);
            pointSelector.AddPointType("Invalid Point", Color.gray, SelectionData.Invalid);

            // ???????
            List<Vector3> wposList;
            List<Vector3> wnorList;
            List<Vector3> wtanList;
            int meshVertexCount = editorMesh.GetEditorPositionNormalTangent(out wposList, out wnorList, out wtanList);

            // ????????
            selectorData.Clear();
            for (int i = 0; i < meshVertexCount; i++)
                selectorData.Add(0); // Invalid
            OnResetSelector(selectorData);

            if (meshVertexCount == 0)
                return;

            // ??????????????????????
            for (int i = 0; i < meshVertexCount; i++)
            {
                pointSelector.AddPoint(wposList[i], i, selectorData[i]);
            }
        }

        /// <summary>
        /// ????????
        /// </summary>
        /// <param name="pointSelector"></param>
        void EndEdit(PointSelector pointSelector)
        {
            // ????????????????
            var pointList = pointSelector.GetPointList();
            foreach (var p in pointList)
            {
                selectorData[p.index] = p.value;
            }

            // ??
            OnFinishSelector(selectorData);
        }

        /// <summary>
        /// ??????????????
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        protected SelectionData CreateSelection(MonoBehaviour obj, string property)
        {
            string dataname = "SelectionData_" + obj.name;
            var selection = ShareDataObject.CreateShareData<SelectionData>(dataname);
            return selection;
        }

        /// <summary>
        /// ????????????????
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="property"></param>
        /// <param name="selectionData"></param>
        protected void ApplySelection(MonoBehaviour obj, string property, SelectionData selectionData)
        {
            var so = new SerializedObject(obj);
            var sel = so.FindProperty(property);
            sel.objectReferenceValue = selectionData;
            so.ApplyModifiedProperties();
        }

        //=========================================================================================
        /// <summary>
        /// ???????????
        /// </summary>
        protected void TeamBasicInspector()
        {
            BaseCloth scr = target as BaseCloth;

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("updateMode"));
            EditorGUILayout.Slider(serializedObject.FindProperty("userBlendWeight"), 0.0f, 1.0f, "Blend Weight");
        }

        protected void CullingInspector()
        {
            BaseCloth scr = target as BaseCloth;

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Culling", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cullingMode"));
            if (scr is MagicaBoneCloth || scr is MagicaBoneSpring)
            {
                if (scr.CullingMode != PhysicsTeam.TeamCullingMode.Off)
                {
                    // warning
                    if (scr.GetCullRenderListCount() == 0)
                    {
                        EditorGUILayout.HelpBox("If you want to cull, you need to register the renderer that makes the display decision here.\nIf not registered, culling will not be performed.", MessageType.Warning);
                    }

                    //EditorGUILayout.PropertyField(serializedObject.FindProperty("cullRendererList"));
                    EditorInspectorUtility.DrawObjectList<Renderer>(
                        serializedObject.FindProperty("cullRendererList"),
                        scr.gameObject,
                        true, true,
                        SearchBoneClothRenderer,
                        "Auto Select"
                        );
                }
            }
        }

        /// <summary>
        /// BoneCloth/Spring???????????????????????
        /// </summary>
        /// <returns></returns>
        private Renderer[] SearchBoneClothRenderer()
        {
            BaseCloth scr = target as BaseCloth;
            if (scr is MagicaBoneCloth || scr is MagicaBoneSpring)
            {
                var rendererList = new List<Renderer>();
                var skinRendererSet = new HashSet<SkinnedMeshRenderer>();

                // search all bone
                var boneSet = new HashSet<Transform>();
                var property = serializedObject.FindProperty("clothTarget.rootList");
                for (int i = 0; i < property.arraySize; i++)
                {
                    var boneRoot = property.GetArrayElementAtIndex(i).objectReferenceValue as Transform;
                    if (boneRoot)
                    {
                        // all transform
                        var tlist = boneRoot.GetComponentsInChildren<Transform>();
                        if (tlist != null)
                        {
                            foreach (var t in tlist)
                                boneSet.Add(t);
                        }

                        // mesh renderer
                        var rlist = boneRoot.GetComponentsInChildren<MeshRenderer>();
                        if (rlist != null)
                            rendererList.AddRange(rlist);

                        // skin renderer
                        Transform root = boneRoot;
                        while (root.parent)
                        {
                            if (root.GetComponent<Animator>() != null || root.GetComponent<Animation>() != null)
                                break;
                            root = root.parent;
                        }
                        var srlist = root.GetComponentsInChildren<SkinnedMeshRenderer>();
                        if (srlist != null)
                        {
                            foreach (var skin in srlist)
                                skinRendererSet.Add(skin);
                        }
                    }
                }
                //foreach (var t in boneSet)
                //    Debug.Log(t);

                // skinrenderer
                foreach (var skin in skinRendererSet)
                {
                    //Debug.Log(skin);
                    var useBoneList = MeshUtility.GetUseBoneTransformList(skin.bones, skin.sharedMesh);
                    foreach (var bone in useBoneList)
                    {
                        if (boneSet.Contains(bone))
                        {
                            rendererList.Add(skin);
                            break;
                        }
                    }
                }

                return rendererList.ToArray();
            }
            else
                return null;
        }

        /// <summary>
        /// ?????????????
        /// </summary>
        protected void ColliderInspector()
        {
            PhysicsTeam scr = target as PhysicsTeam;

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Collider", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("teamData.mergeAvatarCollider"));
            EditorInspectorUtility.DrawObjectList<ColliderComponent>(
                serializedObject.FindProperty("teamData.colliderList"),
                scr.gameObject,
                true, true,
                () => scr.gameObject.transform.root.GetComponentsInChildren<ColliderComponent>()
                );
        }

        /// <summary>
        /// ?????????????
        /// </summary>
        protected void SkinningInspector()
        {
            PhysicsTeam scr = target as PhysicsTeam;
            var mode = serializedObject.FindProperty("skinningMode");
            //var boneList = serializedObject.FindProperty("teamData.skinningBoneList");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Skinning", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(mode, new GUIContent("Skinning Mode"), true);
            //if (scr.SkinningMode == PhysicsTeam.TeamSkinningMode.GenerateFromBones)
            //{
            //    var updateFixed = serializedObject.FindProperty("skinningUpdateFixed");
            //    EditorGUILayout.PropertyField(updateFixed, new GUIContent("Update Fixed"), true);
            //}
            //EditorGUILayout.PropertyField(boneList, new GUIContent("Skinning Bone List"), true);
        }

        /// <summary>
        /// ?????????????????????????
        /// </summary>
        protected void ConvertToLatestAlgorithmParameters()
        {
            BaseCloth cloth = target as BaseCloth;

            Debug.Log($"[{cloth.name}] Convert Parameters.");

            Undo.RecordObject(cloth, "Convert Parameters");
            cloth.Params.ConvertToLatestAlgorithmParameter();

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(cloth);
        }
    }
}
