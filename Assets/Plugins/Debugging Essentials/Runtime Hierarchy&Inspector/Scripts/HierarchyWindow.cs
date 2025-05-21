using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DebuggingEssentials
{
    public partial class RuntimeInspector : MonoBehaviour
    {
        [NonSerialized] public bool needHierarchySearch;
        public bool enableCameraOnStart;

        GUIChangeBool pauseGame = new GUIChangeBool();
        GUIChangeBool useNavigationCamera = new GUIChangeBool();

        GUIChangeBool drawScenes = new GUIChangeBool(true);
        GUIChangeBool drawAssemblies = new GUIChangeBool();
        GUIChangeBool drawMemory = new GUIChangeBool();
        GUIChangeBool drawUnityAssemblies = new GUIChangeBool();
        GUIChangeBool drawSystemAssemblies = new GUIChangeBool();
        GUIChangeBool drawMonoAssemblies = new GUIChangeBool();
        GUIChangeBool drawOtherAssemblies = new GUIChangeBool(true);

        GUIChangeBool drawMonoBehaviourMemory = new GUIChangeBool(true);
        GUIChangeBool drawScriptableObjectMemory = new GUIChangeBool();
        GUIChangeBool drawComponentMemory = new GUIChangeBool();
        GUIChangeBool drawOtherMemory = new GUIChangeBool();

        GUIChangeBool drawCompareFilter = new GUIChangeBool();
        GUIChangeBool drawHideFlags = new GUIChangeBool(true);

        GUIChangeBool showHelp = new GUIChangeBool();
        GUIChangeBool showSettings = new GUIChangeBool();
        GUIChangeBool linkSelect = new GUIChangeBool(true);

        FastList<DrawInfo> selectList = new FastList<DrawInfo>(32);
        GameObject lastScreenRayedGO;

        static Dictionary<NamespaceTypes, DrawInfo> namespaceTypesLookup = new Dictionary<NamespaceTypes, DrawInfo>();
        public static FastList<CustomAssembly> customAssemblies = new FastList<CustomAssembly>();
        static bool hasInitAssemblies;

        public Dictionary<string, DrawInfo> memoryLookup = new Dictionary<string, DrawInfo>();
        public Dictionary<MemoryObject, DrawInfo> memoryObjectLookup = new Dictionary<MemoryObject, DrawInfo>();

        SortedFastList<MemorySnapshot> memorySnapshots = new SortedFastList<MemorySnapshot>();
        MemorySnapshot selectedMemorySnapshot;
        MemorySnapshot difSnapshot = new MemorySnapshot();
        CompareMode memoryCompareMode = CompareMode.Key;
        bool doMemorySnapshotCompare = false;

        FastList<Transform> transformList = new FastList<Transform>(4096);
        FastList<Transform> searchPassedList = new FastList<Transform>(4096);
        FastList<DrawInfo> searchFailedList = new FastList<DrawInfo>(4096);
        List<GameObject> searchRootList = new List<GameObject>(1024);
        bool isSearchingScenes;

        FastList<CustomType> searchTypePassedList = new FastList<CustomType>(4096);
        FastList<CustomType> searchTypeFailedList = new FastList<CustomType>(4096);
        int totalCount;
        bool isSearchingAssemblies;

        HashSet<DrawInfo> searchMemoryObjectPassedList = new HashSet<DrawInfo>();
        HashSet<DrawInfo> searchMemoryObjectFailedList = new HashSet<DrawInfo>();
        bool isSearchingMemory;

        int searchGameObjectsPerFrame;

        Vector2 settingsScrollView;

        float refreshHierarchySearch;
        bool scrollToSelected;

        const int prefix = 150;

        void Pause()
        {
            if (pauseGame.RealValue)
            {
                oldTimeScale = Time.timeScale;
                Time.timeScale = float.Epsilon;
            }
            else
            {
                if (oldTimeScale == float.Epsilon) oldTimeScale = 1;
                Time.timeScale = oldTimeScale;
            }

            if (onTimeScaleChanged != null) onTimeScaleChanged.Invoke(Time.timeScale);
        }

        void DrawHierarchy(int windowId)
        {
            WindowManager.BeginRenderTextureGUI();

            GUI.backgroundColor = new Color(1, 1, 1, 1);// windowData.color; 
            GUI.color = Color.white;

            DrawWindowTitle(Helper.GetGUIContent("Hierarchy", windowData.texHierarchy), hierarchyWindow);

            if (hierarchyWindow.isMinimized)
            {
                GUILayout.Space(-2);
                HierarchyDragAndTooltip(windowId);
                return;
            }

            BeginVerticalBox();

            GUILayout.BeginHorizontal(); 
            skin.button.fontSize = 10;
            GUILayout.Label("Time Scale", GUILayout.Width(75));
            if (Helper.DrawShowButton(windowData, Helper.GetGUIContent("Pause", windowData.texPause, "Toggles Time-Scale to 0 and before"), pauseGame, Color.green, 65))
            {
                Pause();
            }
            float oldTimeScale = Time.timeScale;
            GUILayout.Label(Time.timeScale.ToString("F2"), GUILayout.Width(30));
            Time.timeScale = GUILayout.HorizontalSlider(Time.timeScale, float.Epsilon, 3);
            if (Time.timeScale == float.Epsilon) pauseGame.Value = true; else pauseGame.Value = false;
            if (GUILayout.Button(Helper.GetGUIContent("R", "Reset Time-Scale to 1"), GUILayout.Width(20)))
            {
                Time.timeScale = 1;
            }
            if (Time.timeScale != oldTimeScale)
            {
                if (onTimeScaleChanged != null) onTimeScaleChanged.Invoke(Time.timeScale);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (useNavigationCamera.Value)
            {
                GUILayout.Label("Fov " + NavigationCamera.fov.ToString("F0"), GUILayout.Width(55));
                NavigationCamera.fov = GUILayout.HorizontalSlider(NavigationCamera.fov, 2, 90);
                if (GUILayout.Button(Helper.GetGUIContent("R", "Reset fov to 60"), GUILayout.Width(20)))
                {
                    NavigationCamera.fov = 60;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Helper.DrawShowButton(windowData, Helper.GetGUIContent("Help", windowData.texHelp, "Show Controls"), showHelp, Color.green);
            Helper.DrawShowButton(windowData, Helper.GetGUIContent("Settings", windowData.texSettings, "Change tooltip, search and Camera settings"), showSettings, Color.green);

            Texture camIcon = (NavigationCamera.followTarget ? windowData.texCameraFollow : windowData.texCamera);
            if (Helper.DrawShowButton(windowData, Helper.GetGUIContent("Camera", camIcon, "Toggle navigation camera\n\nWhen enabled use:\n" + focusCameraKey + " to focus on the selected object\n" + followCameraKey + " to follow the selected object"), useNavigationCamera, Color.green))
            {
                if (useNavigationCamera.RealValue) EnableCamControl(); else DisableCamControl();
            }

            if (navigationCamera == null || NavigationCamera.cam == null)
            {
                navigationCamera = null;
                useNavigationCamera.Value = false;
            }

            if (Application.isEditor)
            {
                Helper.DrawShowButton(windowData, Helper.GetGUIContent("Link Select", "Link selected to Unity's hierarchy window"), linkSelect, Color.green);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Helper.DrawShowButton(windowData, Helper.GetGUIContent("Child Count", "Shows the child count for each GameObject"), windowData.showChildCount, Color.green);
            
            if (Helper.DrawShowButton(windowData, Helper.GetGUIContent("Scenes", windowData.texScene, "Scene Mode"), drawScenes, Color.green, -1, true))
            {
                drawAssemblies.Value = drawMemory.Value = false;
                if (selectedGO != null) scrollViewCull.recalc = true;
            }
            if (Helper.DrawShowButton(windowData, Helper.GetGUIContent("Assemblies", windowData.texAssembly, "Assembly Mode"), drawAssemblies, Color.green, -1, true))
            {
                drawMemory.Value = drawScenes.Value = false;
                if (needHierarchySearch) refreshHierarchySearch = 1;
                if (selectedStaticType != null) scrollViewCull.recalc = true;
            }
            if (Helper.DrawShowButton(windowData, Helper.GetGUIContent("Memory", windowData.texMemory, "Memory Mode"), drawMemory, Color.green, -1, true))
            {
                drawScenes.Value = false; drawAssemblies.Value = false;
                if (selectedObject != null) scrollViewCull.recalc = true;
            }
            GUILayout.EndHorizontal();

            if (drawAssemblies.Value)
            {
                GUILayout.BeginHorizontal();
                if (Helper.DrawShowButton(windowData, Helper.GetGUIContent("Unity", "Show/Hide Unity Assemblies\nShift click to multi select"), drawUnityAssemblies, Color.green, -1))
                {
                    if (!currentEvent.shift) drawSystemAssemblies.Value = drawMonoAssemblies.Value = drawOtherAssemblies.Value = false;
                    refreshHierarchySearch = 1;
                }
                if (Helper.DrawShowButton(windowData, Helper.GetGUIContent("System", "Show/Hide System Assemblies\nShift click to multi select"), drawSystemAssemblies, Color.green, -1))
                {
                    if (!currentEvent.shift) drawUnityAssemblies.Value = drawMonoAssemblies.Value = drawOtherAssemblies.Value = false;
                    refreshHierarchySearch = 1;
                }
                if (Helper.DrawShowButton(windowData, Helper.GetGUIContent("Mono", "Show/Hide Mono Assemblies\nShift click to multi select"), drawMonoAssemblies, Color.green, -1))
                {
                    if (!currentEvent.shift) drawUnityAssemblies.Value = drawSystemAssemblies.Value = drawOtherAssemblies.Value = false;
                    refreshHierarchySearch = 1;
                }
                if (Helper.DrawShowButton(windowData, Helper.GetGUIContent("Other", "Show/Hide All Other Assemblies\nShift click to multi select"), drawOtherAssemblies, Color.green, -1))
                {
                    if (!currentEvent.shift) drawUnityAssemblies.Value = drawSystemAssemblies.Value = drawMonoAssemblies.Value = false;
                    refreshHierarchySearch = 1;
                }
                GUILayout.EndHorizontal();
            }
            else if (drawMemory.Value)
            {
                bool isComparingSnapshots = (selectedMemorySnapshot == difSnapshot);

                if (currentEvent.type == EventType.Layout)
                {
                    if (doMemorySnapshotCompare)
                    {
                        doMemorySnapshotCompare = false;
                        DoMemorySnapshotCompare();
                        isComparingSnapshots = (selectedMemorySnapshot == difSnapshot);
                    }
                    
                    if (isComparingSnapshots)
                    {
                        if (drawCompareFilter.Value)
                        {
                            if (!difSnapshot.hasCleanedDifCompare) difSnapshot.CleanDifSnapshot(memoryCompareMode);
                        }
                        else if (difSnapshot.hasCleanedDifCompare) doMemorySnapshotCompare = true;
                    }
                }

                GUILayout.BeginHorizontal();
                if (Helper.DrawShowButton(windowData, Helper.GetGUIContent("MonoBehaviour", "Show/Hide Memory Objects that are MonoBehaviours\nShift click to multi select"), drawMonoBehaviourMemory, Color.green, -1, !currentEvent.shift))
                {
                    if (!currentEvent.shift) drawComponentMemory.Value = drawScriptableObjectMemory.Value = drawOtherMemory.Value = false;
                    refreshHierarchySearch = 1;
                }
                if (Helper.DrawShowButton(windowData, Helper.GetGUIContent("ScriptableObject", "Show/Hide Memory Objects that are ScriptableObjects\nShift click to multi select"), drawScriptableObjectMemory, Color.green, -1, !currentEvent.shift))
                {
                    if (!currentEvent.shift) drawMonoBehaviourMemory.Value = drawComponentMemory.Value = drawOtherMemory.Value = false;
                    refreshHierarchySearch = 1;
                }
                if (Helper.DrawShowButton(windowData, Helper.GetGUIContent("Component", "Show/Hide Memory Objects that are Components\nShift click to multi select"), drawComponentMemory, Color.green, -1, !currentEvent.shift))
                {
                    if (!currentEvent.shift) drawMonoBehaviourMemory.Value = drawScriptableObjectMemory.Value = drawOtherMemory.Value = false;
                    refreshHierarchySearch = 1;
                }
                if (Helper.DrawShowButton(windowData, Helper.GetGUIContent("Other", "Show/Hide other Memory Objects\nShift click to multi select"), drawOtherMemory, Color.green, -1, !currentEvent.shift))
                {
                    if (!currentEvent.shift) drawMonoBehaviourMemory.Value = drawScriptableObjectMemory.Value = drawComponentMemory.Value = false;
                    refreshHierarchySearch = 1;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button(Helper.GetGUIContent("Take Snapshot", "Take Memory Snapshot"), GUILayout.Width(85), GUILayout.Height(20)))
                {
                    ScanMemory();
                }
                
                if (GUILayout.Button(Helper.GetGUIContent("Sort", (memoryCompareMode == CompareMode.Key ? windowData.texAlphabeticSort : windowData.texCountSort), "Toggle sorting between alphabetic and child count"), GUILayout.Width(50), GUILayout.Height(20)))
                {
                    if (memoryCompareMode == CompareMode.Key) memoryCompareMode = CompareMode.Value; else memoryCompareMode = CompareMode.Key;
                    if (isComparingSnapshots) difSnapshot.memoryTypesLookup.Sort(memoryCompareMode);
                    else if (selectedMemorySnapshot != null) selectedMemorySnapshot.memoryTypesLookup.Sort(memoryCompareMode);
                }

                Helper.DrawShowButton(windowData, Helper.GetGUIContent("Filter Compare", "Toggle Filter Compare\nRemoves 'added' and 'removed' Objects from a compare, when they have the same name"), drawCompareFilter, Color.green, 85);
                Helper.DrawShowButton(windowData, Helper.GetGUIContent("HideFlags", "Show HideFlags icons"), drawHideFlags, Color.green, 60);

                GUILayout.EndHorizontal();

                int activeMemorySnapshotCount = ActiveMemorySnapshotCount;

                if (memorySnapshots.Count > 0)
                {
                    GUILayout.BeginHorizontal();
                    int count = 0;
                    for (int i = 0; i < memorySnapshots.Count; i++)
                    {
                        MemorySnapshot memorySnapshot = memorySnapshots.items[i];
                        if (Helper.DrawShowButton(windowData, Helper.GetGUIContent(memorySnapshot.tStamp.ToString("F1"), "Shift Click => Compare\nControl Click => Delete"), memorySnapshot.selected, Color.green, 54.65f, true))
                        {
                            refreshHierarchySearch = 1;
                            if (currentEvent.shift)
                            {
                                if (activeMemorySnapshotCount == 1) doMemorySnapshotCompare = true;
                                else if (memorySnapshot.selected.Value) memorySnapshot.selected.Value = false;
                                else SelectMemorySnapshot(i);
                            }
                            else if (currentEvent.control)
                            {
                                memorySnapshots.RemoveAt(i--);
                                if (memorySnapshot.selected.Value) SelectFirstSelectedMemorySnapshot();
                            }
                            else SelectMemorySnapshot(i);
                        }
                        if (++count >= 5)
                        {
                            count = 0;
                            GUILayout.EndHorizontal();
                            if (i < memorySnapshots.Count - 1) GUILayout.BeginHorizontal();
                        }
                    }

                    if (count > 0) GUILayout.EndHorizontal();
                }
            }

            skin.button.fontSize = 0;
            bool useSettingsScrollView = (showHelp.Value || showSettings.Value);

            if (useSettingsScrollView)
            {
                float settingsHeight = (showHelp.Value ? 252 : 0) + (showSettings.Value ? 372 : 0);
                if (settingsHeight > windowData.hierarchyWindow.rect.height - 250) settingsHeight = windowData.hierarchyWindow.rect.height - 250;
                settingsScrollView = GUILayout.BeginScrollView(settingsScrollView, GUILayout.Height(settingsHeight));
            }

            if (showHelp.Value) DrawHelp();
            if (showSettings.Value) DrawSettings();

            if (useSettingsScrollView)
            {
                GUILayout.EndScrollView();
            }

            if (DrawSearch(hierarchyWindow, false))
            {
                refreshHierarchySearch = 1;
            }

            skin.button.alignment = TextAnchor.MiddleLeft;

            if (currentEvent.type == EventType.Layout) hierarchyWindow.UpdateScrollView();

            hierarchyWindow.rectStartScroll = GUILayoutUtility.GetLastRect();

            hierarchyWindow.scrollView = GUILayout.BeginScrollView(hierarchyWindow.scrollView);

            hierarchyCull.Start(hierarchyWindow);

            if (drawScenes.Value) DrawScenes();
            else if (drawAssemblies.Value) DrawAssemblies();
            else if (drawMemory.Value) DrawMemory(selectedMemorySnapshot);

            hierarchyCull.End();

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            HierarchyDragAndTooltip(windowId);

            WindowManager.EndRenderTextureGUI();
        }

        void HierarchyDragAndTooltip(int windowId)
        {
            if (Helper.Drag(hierarchyWindow, inspectorWindow, hierarchyWindow, false))
            {
                if (inspectorWindow.isDocked.Value) scrollViewCull.recalc = true;
            }
            WindowManager.SetToolTip(windowId);
        }

        void DrawHelp()
        {
            BeginVerticalBox();
            DrawPrefixLabel("Shift Click '>' button", "Foldout one level", prefix);
            DrawPrefixLabel("Control Click '>' button", "Fold one level", prefix);
            DrawPrefixLabel("Alt Click '>' button", "Foldout all up/down", prefix);
            DrawPrefixLabel("Shift Click GameObject", "Toggle Active", prefix);
            DrawPrefixLabel("Shift Click Inspector", "Dock", prefix);
            DrawPrefixLabel("Click Component in Inspector", "Search inside it", prefix);
            DrawPrefixLabel("Right Click Scene", "Set Active Scene", prefix);
            DrawPrefixLabel("Control Click 'D'", "Duplicate GameObject", prefix);
            DrawPrefixLabel("'Del'", "Delete GameObject", prefix);
            GUILayout.EndVertical();
        }

        void DrawSettings()
        {
            WindowManager windowManager = WindowManager.instance;

            BeginVerticalBox();

            DrawToggleField("Show Tooltips", ref windowData.showTooltip);
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Label(Helper.GetGUIContent("Selection Sphere " + windowData.selectionSphereRadius.ToString("F2"), "The radius of the Selection Sphere"), GUILayout.Width(prefix));
            GUI.changed = false;
            windowData.selectionSphereRadius = GUILayout.HorizontalSlider(windowData.selectionSphereRadius, 0.02f, 1f);
            if (GUI.changed)
            {
                SetSelectionSphereRadius();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            DrawInputField("GUI Scale", windowManager.windowData.guiScale);
            if (windowManager.windowData.guiScale.value < 0.55f) windowManager.windowData.guiScale.value = 0.55f;
            if (windowManager.windowData.guiScale.value > 10) windowManager.windowData.guiScale.value = 10;

            GUILayout.Space(5);

            DrawBoldLabel(skin, "Search", 0);
            DrawInputField("GameObjects Per Frame", windowData.searchGameObjectsPerFrame);
            DrawInputField("Assembly Per Frame", windowData.searchAssemblyPerFrame);
            DrawInputField("Inspector Per Frame", windowData.searchInspectorPerFrame);

            GUILayout.Space(5);
            DrawBoldLabel(skin, "Camera", 0);
            DrawInputField("Mouse Sensivity", navigationCameraData.mouseSensitity);
            DrawInputField("Acceleration Multi", navigationCameraData.accelMulti);
            DrawInputField("Deceleration Multi", navigationCameraData.decelMulti);

            DrawInputField("Speed Slow", navigationCameraData.speedSlow);
            DrawInputField("Speed Normal", navigationCameraData.speedNormal);
            DrawInputField("Speed Fast", navigationCameraData.speedFast);

            DrawInputField("Mouse ScrollWheel Multi", navigationCameraData.mouseScrollWheelMulti);
            DrawInputField("Mouse Strafe Multi", navigationCameraData.mouseStrafeMulti);

            GUILayout.EndVertical();
        }

        public static void InitAssemblies()
        {
            if (!hasInitAssemblies) CustomAssembly.InitAssemblies(ref hasInitAssemblies, customAssemblies, namespaceTypesLookup, typeNameLookup);
        }

        void DrawAssemblies()
        {
            for (int i = 0; i < customAssemblies.Count; i++)
            {
                CustomAssembly customAssembly = customAssemblies.items[i];

                string name = customAssembly.name;

                AssemblyType assemblyType = customAssembly.type;

                if (assemblyType == AssemblyType.Unity && !drawUnityAssemblies.Value) continue;
                if (assemblyType == AssemblyType.System && !drawSystemAssemblies.Value) continue;
                if (assemblyType == AssemblyType.Mono && !drawMonoAssemblies.Value) continue;
                if (assemblyType == AssemblyType.Other && !drawOtherAssemblies.Value) continue;

                DrawInfo info = GetObjectDrawInfo(assemblyLookup, customAssembly, false);

                if (needHierarchySearch && info.passSearch == 0) continue;

                bool isCulled = Helper.IsCulled(ref hierarchyCull);

                if (!isCulled)
                {
                    GUI.backgroundColor = new Color(0.4f, 0.5f, 1f, 1f);
                    skin.button.fontStyle = FontStyle.Bold;
                    Bool3 clicked = DrawElement(hierarchyWindow, info, customAssembly.namespaceLookup.list.Count + customAssembly.types.Count, name);
                    skin.button.fontStyle = FontStyle.Normal;

                    if (clicked.v3)
                    {
                        info.foldout = !info.foldout;
                    }
                }

                if (PassedFoldout(info, needHierarchySearch, true, hierarchyWindow.showSearchNonFound, false))
                {
                    DrawAssembly(customAssembly, info.foldout);
                }
                
            }
        }

        void DrawAssembly(CustomAssembly assembly, bool hasSearchPass)
        {
            int indent = 15;
            var namespaceList = assembly.namespaceLookup.list;

            for (int i = 0; i < namespaceList.Count; i++)
            {
                NamespaceTypes namespaceTypes = namespaceList.items[i].value;
                DrawInfo info = GetObjectDrawInfo(namespaceTypesLookup, namespaceTypes);

                if (!PassedFoldout(info, needHierarchySearch, hasSearchPass, hierarchyWindow.showSearchNonFound, true)) continue;

                bool isCulled = Helper.IsCulled(ref hierarchyCull);
                
                if (!isCulled)
                {
                    GUI.backgroundColor = new Color(1f, 1f, 0.5f, 1f);
                    skin.button.fontStyle = FontStyle.Bold;
                    Bool3 clicked = DrawElement(hierarchyWindow, info, namespaceTypes.types.Count, namespaceTypes.name, null, string.Empty, true, indent);
                    skin.button.fontStyle = FontStyle.Normal;

                    if (clicked.v3)
                    {
                        info.foldout = !info.foldout;
                    }
                }

                if (PassedFoldout(info, needHierarchySearch, hasSearchPass, hierarchyWindow.showSearchNonFound, false))
                {
                    DrawAssemblyTypes(namespaceTypes.types, indent + 15, info.foldout);
                }
            }

            if (assembly.types.Count > 0) DrawAssemblyTypes(assembly.types, indent, hasSearchPass);
        }

        void DrawAssemblyTypes(FastList<CustomType> customTypes, int indent, bool hasSearchPass)
        {
            for (int i = 0; i < customTypes.Count; i++)
            {
                CustomType customType = customTypes.items[i];

                DrawInfo info = GetObjectDrawInfo(typeLookup, customType);

                if (!PassedFoldout(info, needHierarchySearch, hasSearchPass, hierarchyWindow.showSearchNonFound, true)) continue;

                bool isCulled = Helper.IsCulled(ref hierarchyCull);

                if (!isCulled)
                {
                    GUI.backgroundColor = GetColor(customType.type, selectedStaticType, needHierarchySearch, info);
                    Bool3 clicked = DrawElement(hierarchyWindow, info, 0, customType.type.Name, null, string.Empty, true, indent);
                    GUI.backgroundColor = GetColor(Color.white);

                    if (clicked.v3)
                    {
                        if (selectedStaticType == customType.type) selectedStaticType = null;
                        else selectedStaticType = customType.type;
                    }
                }
            }
        }
        
        void SelectMemorySnapshot(int snapshotIndex)
        {
            for (int i = 0; i < memorySnapshots.Count; i++)
            {
                memorySnapshots.items[i].selected.Value = false;
            }

            selectedMemorySnapshot = memorySnapshots.items[snapshotIndex];
            selectedMemorySnapshot.selected.Value = true;
        }

        void SelectFirstSelectedMemorySnapshot()
        {
            bool found = false;

            for (int i = 0; i < memorySnapshots.Count; i++)
            {
                MemorySnapshot memorySnapshot = memorySnapshots.items[i];
                if (memorySnapshot.selected.Value)
                {
                    selectedMemorySnapshot = memorySnapshot;
                    found = true;
                    break;
                }
            }

            if (!found) selectedMemorySnapshot = null;
        }

        int ActiveMemorySnapshotCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < memorySnapshots.Count; i++)
                {
                    if (memorySnapshots.items[i].selected.Value) ++count;
                }
                return count;
            }
        }

        void DoMemorySnapshotCompare()
        {
            MemorySnapshot oldSnapshot = null;
            MemorySnapshot newSnapshot = null;

            for (int i = 0; i < memorySnapshots.Count; i++)
            {
                if (memorySnapshots.items[i].selected.Value)
                {
                    if (oldSnapshot == null) oldSnapshot = memorySnapshots.items[i];
                    else
                    {
                        newSnapshot = memorySnapshots.items[i]; ;
                        break;
                    }
                }
            }

            if (oldSnapshot == null || newSnapshot == null) return;

            // Debug.Log("Compare " + oldSnapshot.tStamp + " " + newSnapshot.tStamp);
            MemorySnapshot.CompareMemorySnapshots(oldSnapshot, newSnapshot, difSnapshot, memoryCompareMode);
            selectedMemorySnapshot = difSnapshot;
        }

        void ScanMemory()
        {
            refreshHierarchySearch = 1;

            MemorySnapshot memorySnapshot;

            memorySnapshot = new MemorySnapshot();
            memorySnapshots.Add(memorySnapshot);
            SelectMemorySnapshot(memorySnapshots.Count - 1);

            memorySnapshot.ScanMemory(memoryCompareMode);
            selectedMemorySnapshot = memorySnapshot;
        }

        void DrawMemory(MemorySnapshot memorySnapshot)
        {
            if (memorySnapshot == null) return;

            var memoryTypesLookup = memorySnapshot.memoryTypesLookup;

            for (int i = 0; i < memoryTypesLookup.list.Count; i++)
            {
                var instanceType = memoryTypesLookup.list.items[i];
                string typeName = instanceType.key;
                MemoryInstanceType memoryInstanceType = instanceType.value;

                if (memoryInstanceType.memoryType == MemoryType.MonoBehaviour && !drawMonoBehaviourMemory.Value) continue;
                if (memoryInstanceType.memoryType == MemoryType.Component && !drawComponentMemory.Value) continue;
                if (memoryInstanceType.memoryType == MemoryType.ScriptableObject && !drawScriptableObjectMemory.Value) continue;
                if (memoryInstanceType.memoryType == MemoryType.Other && !drawOtherMemory.Value) continue;

                DrawInfo info = GetObjectDrawInfo(memoryLookup, typeName, false);

                if (needHierarchySearch && info.passSearch == 0) continue;

                bool isCulled = Helper.IsCulled(ref hierarchyCull);

                Texture icon = windowData.componentIcons.GetIcon(memoryInstanceType.type);

                if (!isCulled)
                {
                    GUI.backgroundColor = new Color(0.4f, 0.5f, 1f, 1f);
                    skin.button.fontStyle = FontStyle.Bold;

                    Bool3 clicked = DrawElement(hierarchyWindow, info, memoryInstanceType.instances.list.Count, typeName, icon);
                    skin.button.fontStyle = FontStyle.Normal;
                    GUI.backgroundColor = Color.white;

                    if (clicked.v3)
                    {
                        info.foldout = !info.foldout;
                    }
                }

                if (PassedFoldout(info, needHierarchySearch, true, hierarchyWindow.showSearchNonFound, false))
                {
                    DrawMemoryInstances(memorySnapshot, memoryInstanceType, info.foldout, icon);
                }
            }
        }

        void DrawMemoryInstances(MemorySnapshot memorySnapshot, MemoryInstanceType memoryInstanceType, bool hasSearchPass, Texture icon)
        {
            int indent = 15;
            FastList<MemoryObject> instanceList = memoryInstanceType.instances.list;

            if (!memoryInstanceType.isSorted) memoryInstanceType.Sort();
            bool isSnapshotCompare = (memorySnapshot == difSnapshot);
            if (isSnapshotCompare) indent += 15;

            // var memoryObjectLookup = memorySnapshot.memoryObjectLookup;
            bool showHideFlags = drawHideFlags.Value;

            for (int i = 0; i < instanceList.Count; i++)
            {
                MemoryObject memoryObject = instanceList.items[i];

                UnityEngine.Object obj = Helper.FindObjectFromInstanceID(memoryObject.instanceId);
                
                DrawInfo info = GetObjectDrawInfo(memoryObjectLookup, memoryObject);

                if (!PassedFoldout(info, needHierarchySearch, hasSearchPass, hierarchyWindow.showSearchNonFound, true)) continue;
                // if (needHierarchySearch && info.passSearch == 0) continue;

                bool isCulled = Helper.IsCulled(ref hierarchyCull);

                string tooltip = string.Empty;

                if (!isCulled)
                {
                    if (obj == null)
                    {
                        tooltip = "Object is Destroyed";
                    }
                    
                    GUI.backgroundColor = GetColor(obj, selectedObject, needHierarchySearch, info);
                    Bool3 clicked = DrawElement(hierarchyWindow, info, 0, memoryObject.name, icon, tooltip, true, indent);
                    if (memoryObject.hideFlags != HideFlags.None || memoryObject.isPrefab || obj == null) DrawHideFlagsIcons(obj, showHideFlags? memoryObject.hideFlags : HideFlags.None, memoryObject.isPrefab, memoryObject.name);

                    GUI.backgroundColor = Color.white;
                    GUI.color = Color.white;
                    if (isSnapshotCompare)
                    {
                        Rect rect = GUILayoutUtility.GetLastRect();
                        rect.x = 37 + (windowData.showChildCount.Value ? 29 : 0);
                        rect.width = 20;
                        if (memoryObject.compareResult == CompareResult.Removed)
                        {
                            GUI.backgroundColor = Color.red;
                            tooltip = "Compare Result => Object is removed";
                        }
                        else
                        {
                            GUI.backgroundColor = Color.green;
                            tooltip = "Compare Result => Object is new";
                        }
                        GUI.Button(rect, Helper.GetGUIContent(string.Empty, null, tooltip));
                        GUI.backgroundColor = Color.white;
                    }

                    if (clicked.v3 && obj != null)
                    {
                        if (selectedObject == obj) selectedObject = null;
                        else
                        {
                            selectedObject = obj;
#if UNITY_EDITOR
                            if (linkSelect.Value) UnityEditor.Selection.activeObject = obj;
#endif
                        }
                    }
                }
            }
        }

        void DrawScenes()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            int sceneCount = SceneManager.sceneCount;
            Scene scene;

            for (int i = 0; i < sceneCount; i++)
            {
                scene = SceneManager.GetSceneAt(i);

                DrawScene(scene, activeScene, sceneCount);
            }

            if (WindowManager.instance.useDontDestroyOnLoadScene)
            {
                scene = WindowManager.instance.gameObject.scene;
                DrawScene(scene, activeScene, sceneCount);
            }
        }

        void DrawScene(Scene scene, Scene activeScene, int sceneCount)
        {
            DrawInfo info = GetObjectDrawInfo(sceneLookup, scene, unfoldMultipleScenesOnStart || sceneCount == 1);

            scene.GetRootGameObjects(rootList);
            bool isCulled = Helper.IsCulled(ref hierarchyCull);

            if (!isCulled)
            {
                GUI.backgroundColor = (scene == activeScene ? GetColor(new Color(0.25f, 1f, 0.25f)) : GetColor(Color.yellow));
                GUI.skin.button.fontStyle = FontStyle.Bold;

                string sceneName = scene.name;
                if (sceneName == string.Empty) sceneName = "Untitled";

                Bool3 clicked = DrawElement(hierarchyWindow, info, rootList.Count, sceneName, windowData.texScene);

                if (clicked.v3)
                {
                    if (currentEvent.button == 1)
                    {
                        SceneManager.SetActiveScene(scene);
                    }
                    else
                    {
                        if (currentEvent.shift) FoldOutScene(scene, rootList);
                        else if (currentEvent.control) FoldInScene(scene, rootList);
                        else if (currentEvent.alt) FoldScene(scene, rootList, !info.foldout);
                        else info.foldout = !info.foldout;
                    }
                }

                GUI.skin.button.fontStyle = FontStyle.Normal;
                GUI.backgroundColor = GetColor(Color.white);
            }

            if (PassedFoldout(info, needHierarchySearch, false, hierarchyWindow.showSearchNonFound, false))
            {
                DrawSceneGameObjects(rootList, info.foldout);
            }
        }

        bool PassedFoldout(DrawInfo info, bool needSearch, bool hasSearchPass, bool showSearchNonFound, bool isChild)
        {
            if (!needSearch) return (info.foldout || isChild);
            return (info.passSearch > 0 || (hasSearchPass && showSearchNonFound) || (info.foldout && info.passSearch > 0));
        }

        void DrawSceneGameObjects(List<GameObject> goList, bool hasSearchPass)
        {
            int indent = 20;

            for (int i = 0; i < goList.Count; i++)
            {
                DrawGameObject(goList[i], ref indent, hasSearchPass);
            }
        }

        void DrawGameObjects(Transform t, int indent, bool hasSearchPass)
        {
            indent += 15;

            int childCount = t.childCount;

            for (int i = 0; i < childCount; i++)
            {
                DrawGameObject(t.GetChild(i).gameObject, ref indent, hasSearchPass);
            }
        }

        void DrawGameObject(GameObject go, ref int indent, bool hasSearchPass)
        {
            Transform t = go.transform;

            DrawInfo info = GetObjectDrawInfo(tLookup, t);

            if (!PassedFoldout(info, needHierarchySearch, hasSearchPass, hierarchyWindow.showSearchNonFound, true)) return;

            int childCount = t.childCount;
            bool hasChildren = childCount > 0;

            bool isCulled = Helper.IsCulled(ref hierarchyCull);

            if (scrollToSelected && go == selectedGO)
            {
                scrollToSelected = false;
                hierarchyWindow.newScrollViewY = Mathf.Max(hierarchyCull.scrollWindowPosY - ((hierarchyWindow.rect.height / 2) - (hierarchyWindow.rectStartScroll.y + 42)), 0);
            }

            if (!isCulled || hasChildren)
            {
                if (!isCulled)
                {
                    GUI.backgroundColor = GetColor(go, selectedGO, needHierarchySearch, info);
                    Bool3 clicked = DrawElement(hierarchyWindow, info, childCount, t.name, windowData.componentIcons.gameObjectIcon, string.Empty, go.activeInHierarchy, indent);
                    GUI.backgroundColor = GetColor(Color.white);

                    if (clicked.v1)
                    {
                        if (currentEvent.shift) go.SetActive(!go.activeInHierarchy);
                        else
                        {
                            if (selectedGO == go) SelectGameObject(null);
                            else SelectGameObject(go);
                        }
                    }
                    else if (clicked.v2)
                    {
                        if (currentEvent.shift) FoldOutGameObjects(t);
                        else if (currentEvent.control) FoldInGameObjects(t);
                        else if (currentEvent.alt) FoldGameObject(t, !info.foldout);
                        else info.foldout = !info.foldout;
                    }
                }

                if (hasChildren && PassedFoldout(info, needHierarchySearch, hasSearchPass, hierarchyWindow.showSearchNonFound, false))
                {
                    DrawGameObjects(t, indent, info.foldout);
                }
            }
        }

        void FoldScene(Scene scene, List<GameObject> rootList, bool foldout)
        {
            int childCount = rootList.Count;
            if (childCount == 0) return;

            DrawInfo info = GetObjectDrawInfo(sceneLookup, scene);
            info.foldout = foldout;

            for (int i = 0; i < childCount; i++)
            {
                FoldGameObject(rootList[i].transform, foldout);
            }
        }

        bool FoldOutScene(Scene scene, List<GameObject> rootList)
        {
            int childCount = rootList.Count;
            if (childCount == 0) return false;

            DrawInfo info = GetObjectDrawInfo(sceneLookup, scene);

            if (info.foldout)
            {
                for (int i = 0; i < childCount; i++)
                {
                    FoldOutGameObjects(rootList[i].transform);
                }
            }
            else info.foldout = true;

            return true;
        }

        bool FoldInScene(Scene scene, List<GameObject> rootList)
        {
            int childCount = rootList.Count;
            if (childCount == 0) return false;

            DrawInfo info = GetObjectDrawInfo(sceneLookup, scene);

            if (info.foldout)
            {
                bool executed = false;
                for (int i = 0; i < childCount; i++)
                {
                    if (FoldInGameObjects(rootList[i].transform)) executed = true;
                }

                if (!executed) info.foldout = false;
                return true;
            }

            return false;
        }

        void FoldGameObject(Transform t, bool foldout)
        {
            int childCount = t.childCount;
            if (childCount == 0) return;

            DrawInfo info = GetObjectDrawInfo(tLookup, t);
            info.foldout = foldout;

            for (int i = 0; i < childCount; i++)
            {
                FoldGameObject(t.GetChild(i), foldout);
            }
        }

        bool FoldOutGameObjects(Transform t)
        {
            int childCount = t.childCount;
            if (childCount == 0) return false;

            DrawInfo info = GetObjectDrawInfo(tLookup, t);

            if (info.foldout)
            {
                for (int i = 0; i < childCount; i++)
                {
                    FoldOutGameObjects(t.GetChild(i));
                }
            }
            else info.foldout = true;

            return true;
        }

        bool FoldInGameObjects(Transform t)
        {
            int childCount = t.childCount;
            if (childCount == 0) return false;


            DrawInfo info = GetObjectDrawInfo(tLookup, t);

            if (info.foldout)
            {
                bool executed = false;
                for (int i = 0; i < childCount; i++)
                {
                    if (FoldInGameObjects(t.GetChild(i))) executed = true;
                }

                if (!executed) info.foldout = false;
                return true;
            }

            return false;
        }

        void SearchMemory()
        {
            if (refreshHierarchySearch == 0) return;
            if (!isSearchingMemory && selectedMemorySnapshot != null) StartCoroutine(SearchMemoryCR());
        }

        IEnumerator SearchMemoryCR()
        {
            // RuntimeConsole.Log("Search Memory");
            yield return null;
            refreshHierarchySearch = 0;
            isSearchingMemory = true;
            searchMemoryObjectPassedList.Clear();
            searchMemoryObjectFailedList.Clear();

            int count = 0;
            totalCount = 0;

            FastList<ItemHolder<string, MemoryInstanceType>> list = selectedMemorySnapshot.memoryTypesLookup.list;

            for (int i = 0; i < list.Count; i++)
            {
                MemoryInstanceType mit = list.items[i].value;

                if (mit.memoryType == MemoryType.Component && !drawComponentMemory.RealValue) continue;
                if (mit.memoryType == MemoryType.MonoBehaviour && !drawMonoBehaviourMemory.RealValue) continue;
                if (mit.memoryType == MemoryType.ScriptableObject && !drawScriptableObjectMemory.RealValue) continue;
                if (mit.memoryType == MemoryType.Other && !drawOtherMemory.RealValue) continue;

                DrawInfo mitInfo = GetObjectDrawInfo(memoryLookup, mit.type.Name);

                FastList<MemoryObject> instances = mit.instances.list;

                for (int j = 0; j < instances.Count; j++)
                {
                    if (!needHierarchySearch) goto End;

                    MemoryObject m = instances.items[j];

                    DrawInfo info = GetObjectDrawInfo(memoryObjectLookup, m);
                    if (PassSearch(WindowType.Hierarchy, null, m.name, mit.type))
                    {
                        info.passSearch = 2;
                        searchMemoryObjectPassedList.Add(mitInfo);
                    }
                    else
                    {
                        info.passSearch = 0;
                        searchMemoryObjectFailedList.Add(mitInfo);
                    }

                    if (count++ > windowData.searchGameObjectsPerFrame.value)
                    {
                        count = 0;
                        yield return null;
                    }
                }
            }

            // Debug.Log("Foud " + searchTypePassedList.Count + " total Count " + totalCount);
            foreach (DrawInfo info in searchMemoryObjectFailedList) info.passSearch = 0;
            foreach (DrawInfo info in searchMemoryObjectPassedList) info.passSearch = 1;

            End:;
            isSearchingMemory = false;
        }

        void SearchAssemblies()
        {
            if (!hasInitAssemblies || refreshHierarchySearch == 0) return;
            if (!isSearchingAssemblies) StartCoroutine(SearchAssembliesCR());
        }

        IEnumerator SearchAssembliesCR()
        {
            // RuntimeConsole.Log("Search Assemblies");
            refreshHierarchySearch = 0;
            isSearchingAssemblies = true;
            searchTypePassedList.Clear();
            searchTypeFailedList.Clear();

            int count = 0;
            totalCount = 0;

            for (int i = 0; i < customAssemblies.Count; i++)
            {
                CustomAssembly customAssembly = customAssemblies.items[i];

                if (customAssembly.type == AssemblyType.Unity && !drawUnityAssemblies.RealValue) continue;
                if (customAssembly.type == AssemblyType.System && !drawSystemAssemblies.RealValue) continue;
                if (customAssembly.type == AssemblyType.Mono && !drawMonoAssemblies.RealValue) continue;
                if (customAssembly.type == AssemblyType.Other && !drawOtherAssemblies.RealValue) continue;

                int startIndex = 0;

                while (startIndex >= 0)
                {
                    startIndex = SearchThroughTypes(customAssembly.types, startIndex, ref count);
                    if (startIndex >= 0) yield return null;
                }

                if (startIndex == -2) goto End;

                var namespaceList = customAssembly.namespaceLookup.list;
                for (int j = 0; j < namespaceList.Count; j++)
                {
                    NamespaceTypes namespaceTypes = namespaceList.items[j].value;

                    startIndex = 0;
                    while (startIndex >= 0)
                    {
                        startIndex = SearchThroughTypes(namespaceTypes.types, startIndex, ref count);
                        if (startIndex >= 0) yield return null;
                    }

                    if (startIndex == -2) goto End;
                }
            }

            // Debug.Log("Foud " + searchTypePassedList.Count + " total Count " + totalCount);
            for (int i = 0; i < searchTypeFailedList.Count; i++)
            {
                SetFound(searchTypeFailedList.items[i], false);
            }

            for (int i = 0; i < searchTypePassedList.Count; i++)
            {
                SetFound(searchTypePassedList.items[i], true);
            }

            End:;
            isSearchingAssemblies = false;
        }

        void SetFound(CustomType customType, bool found)
        {
            DrawInfo info;

            NamespaceTypes namespaceTypes = customType.parent as NamespaceTypes;
            CustomAssembly customAssembly;

            if (namespaceTypes != null)
            {
                info = GetObjectDrawInfo(namespaceTypesLookup, namespaceTypes);
                if (found)
                {
                    if (info.passSearch == 0) info.passSearch = 1;
                }
                else info.passSearch = 0;

                customAssembly = (CustomAssembly)namespaceTypes.parent;
            }
            else
            {
                customAssembly = (CustomAssembly)customType.parent;
            }

            info = GetObjectDrawInfo(assemblyLookup, customAssembly); // Can be optimized like inspectorSearch

            if (found)
            {
                if (info.passSearch == 0) info.passSearch = 1;
            }
            else info.passSearch = 0;
        }

        int SearchThroughTypes(FastList<CustomType> types, int startIndex, ref int count)
        {
            for (int i = startIndex; i < types.Count; i++)
            {
                if (!needHierarchySearch) return -2;
                CustomType customType = types.items[i];

                ++totalCount;

                DrawInfo info = GetObjectDrawInfo(typeLookup, customType);
                if (PassSearch(WindowType.Hierarchy, null, customType.type.Name, customType.type))
                {
                    info.passSearch = 2;
                    searchTypePassedList.Add(customType);
                }
                else
                {
                    info.passSearch = 0;
                    searchTypeFailedList.Add(customType);
                }

                if (count++ > windowData.searchAssemblyPerFrame.value)
                {
                    count = 0;
                    return i + 1;
                }
            }

            return -1;
        }

        void SearchScenes()
        {
            if (!isSearchingScenes) StartCoroutine(SearchScenesCR());
        }

        void SetSearchObjectsPerFrame()
        {
            searchGameObjectsPerFrame = Mathf.Max(Mathf.CeilToInt(windowData.searchGameObjectsPerFrame.value * refreshHierarchySearch), 1);
        }

        IEnumerator SearchScenesCR()
        {
            // RuntimeConsole.Log("Search Scenes " + refreshHierarchySearch, true);

            isSearchingScenes = true;
            searchPassedList.Clear();
            searchFailedList.Clear();
            transformList.Clear();

            int count = 0;

            SetSearchObjectsPerFrame();
            
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                scene.GetRootGameObjects(searchRootList);
                for (int j = 0; j < searchRootList.Count; j++)
                {
                    transformList.Add(searchRootList[j].transform);
                }

                yield return null;

                for (int j = 0; j < transformList.Count; j++)
                {
                    if (!needHierarchySearch) goto End;
                    Transform t = transformList.items[j];
                    if (t == null) continue;

                    DrawInfo info = GetObjectDrawInfo(tLookup, t);
                    if (PassSearch(WindowType.Hierarchy, t, null, null))
                    {
                        info.passSearch = 2;
                        searchPassedList.Add(t);
                    }
                    else searchFailedList.Add(info);

                    if (t.childCount > 0)
                    {
                        for (int k = 0; k < t.childCount; k++)
                        {
                            transformList.Add(t.GetChild(k));

                            if (count++ > searchGameObjectsPerFrame)
                            {
                                count = 0;
                                yield return null;
                                SetSearchObjectsPerFrame();
                                if (t == null) break;
                            }
                        }
                    }

                    if (count++ > searchGameObjectsPerFrame)
                    {
                        count = 0;
                        yield return null;
                        SetSearchObjectsPerFrame();
                    }
                }

                transformList.Clear();
            }

            //Debug.Log("Foud " + searchPassedList.Count);
            for (int i = 0; i < searchFailedList.Count; i++)
            {
                searchFailedList.items[i].passSearch = 0;
            }

            for (int i = 0; i < searchPassedList.Count; i++)
            {
                Transform t = searchPassedList.items[i];
                if (t == null) continue;
                DrawInfo info;

                while (t.parent != null)
                {
                    t = t.parent;
                    info = GetObjectDrawInfo(tLookup, t); // Can be optimized like inspectorSearch
                    if (info.passSearch == 0) info.passSearch = 1;
                }

                info = GetObjectDrawInfo(sceneLookup, t.gameObject.scene);
                info.passSearch = 1;
            }

            End:;
            isSearchingScenes = false;
            refreshHierarchySearch = 0.5f;
        }
    }
}