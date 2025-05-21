using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
#if UNITY_6000_0_OR_NEWER
using UnityEditor.Build;
#endif
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Rendering;

#if HDRP_OUTLINE
using UnityEngine.Rendering.HighDefinition;
#endif

namespace EPOOutline
{
    public class EPOSetuper : EditorWindow
    {
        private static readonly string SRPShownID = "EasyPerformantOutlineWasShownAndCanceled";
        private static bool UPRWasFound = false;
        private static bool HDRPWasFound = false;

        private static ListRequest request;
        private static AddRequest addRequest;
        
        private Texture2D logoImage;

        public enum SetupType
        {
            BuiltIn,
            URP,
            HDRP
        }

        [SerializeField]
        private Vector2 scroll;

        public static bool ShouldShow
        {
            get
            {
                return PlayerPrefs.GetInt(SRPShownID, 0) == 0;
            }

            set
            {
                PlayerPrefs.SetInt(SRPShownID, value ? 0 : 1);
            }
        }

        private static 
#if UNITY_6000_0_OR_NEWER
            List<NamedBuildTarget>
#else
            List<BuildTargetGroup> 
#endif
            GetApplicableGroups()
        {
            var groups = new List<BuildTargetGroup>();
            var type = typeof(BuildTargetGroup);

            var values = type.GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (var value in values)
            {
                if (value.GetCustomAttribute<ObsoleteAttribute>() != null)
                    continue;

                var targetValue = (BuildTargetGroup) value.GetValue(null);
                if (targetValue == BuildTargetGroup.Unknown)
                    continue;
                
                groups.Add(targetValue);
            }

#if UNITY_6000_0_OR_NEWER
            return groups.ConvertAll(NamedBuildTarget.FromBuildTargetGroup);
#else
            return groups;
#endif
        }

        private static bool CheckHasDefinition(string definition)
        {
            var targets = GetApplicableGroups();
            foreach (var buildTargetGroup in targets)
            {
#if UNITY_6000_0_OR_NEWER
                var definitions = PlayerSettings.GetScriptingDefineSymbols(buildTargetGroup);
#else
                var definitions = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
#endif
                
                var splited = definitions.Split(';');

                if (Array.Find(splited, x => x == definition) == null)
                    return false;
            }

            return true;
        }

#if URP_OUTLINE
        private static bool CheckShouldFixFeature()
        {
            var activeAssets = PipelineAssetUtility.ActiveAssets;

            foreach (var asset in activeAssets)
            {
                if (!PipelineAssetUtility.IsURP(asset))
                    continue;

                if (!PipelineAssetUtility.DoesAssetContainsSRPOutlineFeature(asset))
                    return true;
            }

            return false;
        }
#endif
        
#if URP_OUTLINE || HDRP_OUTLINE
        public enum ExpectedAssetType
        {
            URP,
            HDRP
        }
        
        private static bool CheckHasActiveRenderAssets(ExpectedAssetType type)
        {
            foreach (var asset in PipelineAssetUtility.ActiveAssets)
            {
                switch (type)
                {
                    case ExpectedAssetType.URP:
                        if (PipelineAssetUtility.IsURP(asset))
                            return true;
                        break;
                    case ExpectedAssetType.HDRP:
                        if (PipelineAssetUtility.IsHDRP(asset))
                            return true;
                        break;
                }
            }

            return false;
        }
#endif

        private void OnEnable()
        {
            EditorApplication.update += Check;
        }

        private void OnDisable()
        {
            EditorApplication.update -= Check;
        }

        private static void RemoveDefinition(string definition, Func<bool> check)
        {
            if (!check())
                return;

            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
#if UNITY_6000_0_OR_NEWER
            var definitions = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(group));
#else
            var definitions = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
#endif
            
            var splited = definitions.Split(';');

            var builder = new StringBuilder();

            var addedCount = 0;
            foreach (var item in splited)
            {
                if (item == definition)
                    continue;

                builder.Append(item);
                builder.Append(';');
                addedCount++;
            }

            if (addedCount != 0)
                builder.Remove(builder.Length - 1, 1);

#if UNITY_6000_0_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(group), builder.ToString());
#else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, builder.ToString());
#endif
        }

        private static void Check()
        {
            if (EditorApplication.isPlaying)
                return;

            if (request == null || request.Error != null)
            {
                request = Client.List();
                return;
            }

            if (!request.IsCompleted)
                return;

            UPRWasFound = HasURP(request.Result);
            HDRPWasFound = HasHDRP(request.Result);

            request = Client.List();
        }

        private static bool HasHDRP(PackageCollection result)
        {
            return HasPackage(result, "com.unity.render-pipelines.high-definition");
        }

        private static bool HasURP(PackageCollection result)
        {
            return HasPackage(result, "com.unity.render-pipelines.universal");
        }

        private static bool HasPackage(PackageCollection result, string packageName)
        {
            if (result == null)
                return false;

            var found = false;
            var name = packageName;
            foreach (var item in result)
            {
                if (item.name == name)
                {
                    found = true;
                    break;
                }
            }

            return found;
        }

        [MenuItem("Tools/Easy performant outline/Setup")]
        private static void ForceShowWindow()
        {
            ShouldShow = true;
            ShowWindow();
        }

        [MenuItem("Tools/Easy performant outline/Online docs")]
        private static void ShowDocs()
        {
            Application.OpenURL("https://docs.google.com/document/d/17GvzvXNEjpEQ8DShRrVHKQ4I6s2tTVtwX6NzCZZ28AQ");
        }

        private static void ShowWindow()
        {
            if (!ShouldShow)
                return;

            var window = GetWindow<EPOSetuper>(true, "EPO Setuper", false);
            window.maxSize = new Vector2(500, 500);
            window.minSize = new Vector2(500, 500);
        }

        public void OnGUI()
        {
            if (logoImage == null)
                logoImage = Resources.Load<Texture2D>("Easy performant outline/EP Outline logo");

            var height = 180;
            GUILayout.Space(height);

            var imagePosition = new Rect(Vector2.zero, new Vector2(position.width, height));

            GUI.DrawTexture(imagePosition, logoImage, ScaleMode.ScaleAndCrop, true);

            GUILayout.Space(10);

            scroll = GUILayout.BeginScrollView(scroll);

            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Please stop running the app to start setup process", MessageType.Info);

                if (GUILayout.Button("Stop"))
                    EditorApplication.isPlaying = false;

                GUILayout.EndScrollView();
                return;
            }

            if (EditorApplication.isCompiling)
            {
                EditorGUILayout.HelpBox(new GUIContent("Compiling... please wait."));
                GUILayout.EndScrollView();
                return;
            }

            EditorGUILayout.HelpBox("In order to make DOTween work, please enable it on the DOTween setup panel", MessageType.Warning);

            EditorGUI.indentLevel = 0;

            EditorPrefs.SetInt("SetupType", (int)(SetupType)EditorGUILayout.EnumPopup("Setup type", (SetupType)EditorPrefs.GetInt("SetupType")));

            switch ((SetupType)EditorPrefs.GetInt("SetupType"))
            {
                case SetupType.BuiltIn:
                    DrawBuiltInSetup();
                    break;
                case SetupType.URP:
                    DrawURPSetup();
                    break;
                case SetupType.HDRP:
                    DrawHDRPSetup();
                    break;
            }

            GUILayout.EndScrollView();
        }

        private void DrawBuiltInSetup()
        {
            EditorGUILayout.HelpBox(new GUIContent("There is no need in making any changes to work with Built-In renderer. Just add Outliner to camera and Outlinable to the object you wish to highlight"));
        }

        private void DrawHDRPSetup()
        {
            if (addRequest != null && !addRequest.IsCompleted)
            {
                EditorGUILayout.HelpBox(new GUIContent("Adding package..."));
                return;
            }

            var packageName = "com.unity.render-pipelines.high-definition";

            if (!HDRPWasFound)
            {
                EditorGUILayout.HelpBox(new GUIContent("There is no package added. Chick 'Add' to add the pipeline package."));

                if (GUILayout.Button("Add"))
                    addRequest = Client.Add(packageName);

                return;
            }
            else
                EditorGUILayout.HelpBox(new GUIContent("Pipeline asset has been found in packages"));

#if HDRP_OUTLINE
            if (!CheckHasActiveRenderAssets(ExpectedAssetType.HDRP))
            {
                EditorGUILayout.HelpBox(new GUIContent("There is no renderer asset set up. Create one?"));

                if (GUILayout.Button("Create"))
                {
                    var path = EditorUtility.SaveFilePanelInProject("Asset location", "Rendering asset", "asset", "Select the folder to save rendering asset");
                    if (string.IsNullOrEmpty(path))
                    {
                        GUILayout.EndScrollView();
                        return;
                    }

                    var pathNoExt = Path.ChangeExtension(path, string.Empty);
                    pathNoExt = pathNoExt.Substring(0, pathNoExt.Length - 1);

                    var asset = PipelineAssetUtility.CreateHDRPAsset();

#if UNITY_6000_0_OR_NEWER
                    GraphicsSettings.defaultRenderPipeline = asset;
#else
                    GraphicsSettings.renderPipelineAsset = asset;
#endif
                    AssetDatabase.CreateAsset(asset, path);
                }
            }
            else
                EditorGUILayout.HelpBox(new GUIContent("At least one renderer asset is set up"));

#if UNITY_6000_0_OR_NEWER
            var volume = FindAnyObjectByType<CustomPassVolume>();
#else
            var volume = FindObjectOfType<CustomPassVolume>();
#endif
            
            if (volume == null)
            {
                EditorGUILayout.HelpBox(new GUIContent("There is no custom pass volume in the scene. Click Add to fix it."));

                if (GUILayout.Button("Add"))
                {
                    var go = new GameObject("Custom volume");
                    go.AddComponent<CustomPassVolume>();

                    EditorUtility.SetDirty(go);
                }
            }
            else
            {
                EditorGUILayout.HelpBox(new GUIContent("The scene has custom pass volume."));

                if (volume.customPasses.Find(x => x is EPOOutline.OutlineCustomPass) == null)
                {
                    EditorGUILayout.HelpBox(new GUIContent("The volume doesn't have custom pass. Click Add to fix it."));
                    if (GUILayout.Button("Add"))
                    {
                        volume.AddPassOfType(typeof(EPOOutline.OutlineCustomPass));

                        EditorUtility.SetDirty(volume);
                    }
                }
                else
                    EditorGUILayout.HelpBox(new GUIContent("The custom volume is set up"));
            }
#endif
        }

        private void DrawURPSetup()
        {
            if (addRequest != null && !addRequest.IsCompleted)
            {
                EditorGUILayout.HelpBox(new GUIContent("Adding package..."));
                return;
            }

            var packageName = "com.unity.render-pipelines.universal";
            if (!UPRWasFound)
            {
                EditorGUILayout.HelpBox(new GUIContent("There is no package added. Chick 'Add' to add the pipeline package."));

                if (GUILayout.Button("Add"))
                    addRequest = Client.Add(packageName);

                return;
            }
            else
                EditorGUILayout.HelpBox(new GUIContent("Pipeline asset has been found in packages"));

#if URP_OUTLINE
            if (!CheckHasActiveRenderAssets(ExpectedAssetType.URP))
            {
                EditorGUILayout.HelpBox(new GUIContent("There is no renderer asset set up. Create one?"));

                if (GUILayout.Button("Create"))
                {
                    var path = EditorUtility.SaveFilePanelInProject("Asset location", "Rendering asset", "asset", "Select the folder to save rendering asset");
                    if (string.IsNullOrEmpty(path))
                    {
                        GUILayout.EndScrollView();
                        return;
                    }

                    var pathNoExt = Path.ChangeExtension(path, string.Empty);
                    pathNoExt = pathNoExt.Substring(0, pathNoExt.Length - 1);

                    var asset = PipelineAssetUtility.CreateAsset(pathNoExt);
#if UNITY_6000_0_OR_NEWER
                    GraphicsSettings.defaultRenderPipeline = asset;
#else
                    GraphicsSettings.renderPipelineAsset = asset;
#endif
                }
            }
            else
                EditorGUILayout.HelpBox(new GUIContent("At least one renderer asset is set up"));

            if (CheckShouldFixFeature())
            {
                var assets = PipelineAssetUtility.ActiveAssets;
                foreach (var asset in assets)
                {
                    if (PipelineAssetUtility.DoesAssetContainsSRPOutlineFeature(asset))
                        continue;

                    EditorGUI.indentLevel = 0;

                    var text = string.Format("There is no outline feature added to the pipeline asset called '{0}'. Please add the feature:", asset.name);
                    EditorGUILayout.HelpBox(new GUIContent(text));

                    Editor previous = null;
                    Editor.CreateCachedEditor(PipelineAssetUtility.GetRenderer(asset), null, ref previous);

                    previous.OnInspectorGUI();
                }

                for (var index = 0; index < 10; index++)
                    EditorGUILayout.Space();

                return;
            }
            else
                EditorGUILayout.HelpBox(new GUIContent("Feature is added for all renderers in use"));
#endif

        }

        public void OnDestroy()
        {
            ShouldShow = false;
        }
    }
}