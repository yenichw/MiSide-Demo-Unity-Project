using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DebuggingEssentials
{
    public partial class RuntimeInspector : MonoBehaviour 
    {
        public struct Bool3
        {
            public bool v1;
            public bool v2;
            public bool v3;

            public void Reset()
            {
                v1 = v2 = v3 = false;
            }
        }

        public class DrawInfo : CullItem
        {
            public static DrawInfo empty = new DrawInfo(false);

            public bool foldout;
            public int passSearch = 0;

            public DrawInfo(bool foldout) : base(0)
            {
                this.foldout = foldout;
            }
        }

        bool DrawSearch(WindowSettings window, bool changeText2)
        {
            List<Search> searchList = window.searchList;

            if (searchList.Count == 0) searchList.Add(new Search());

            bool hasChanged = false;

            for (int i = 0; i < searchList.Count; i++)
            {
                Search search = searchList[i];
                SearchMode searchMode = search.mode;

                if (!search.useSearch) GUI.color = Color.grey;
                else if (search.hasSearch) GUI.backgroundColor = GetColor(Color.magenta);

                GUILayout.BeginHorizontal();

                if (GUILayout.Button(Helper.GetGUIContent("Search", windowData.texSearch, "Toggle Search On/Off"), GUILayout.Width(75), GUILayout.Height(21)))
                {
                    search.useSearch = !search.useSearch;
                    hasChanged = true;
                }
                GUI.backgroundColor = GetColor(Color.white);

                if (i == 0)
                {
                    if (GUILayout.Button(Helper.GetGUIContent("+", "Add another Search Condition"), GUILayout.Width(20))) searchList.Add(new Search());
                }
                else
                {
                    if (GUILayout.Button(Helper.GetGUIContent("-", "Remove this Search Condition"), GUILayout.Width(20)))
                    {
                        if (search.useSearch && search.hasSearch) hasChanged = true;
                        searchList.RemoveAt(i--);
                        continue;
                    }
                }

                if (GUILayout.Button(Helper.GetGUIContent(searchMode.ToString(), "Toggle between searching for Names/Types"), GUILayout.Width(47)))
                {
                    if (searchMode == SearchMode.Name) search.mode = SearchMode.Type;
                    else search.mode = SearchMode.Name;

                    if (search.useSearch && search.hasSearch) hasChanged = true;
                }

                if (i == 0)
                {
                    // if (searchList.Count > 1) GUILayout.Space(24);
                    if (GUILayout.Button(Helper.GetGUIContent(window.showSearchNonFound ? windowData.texVisible : windowData.texInvisible, "Enabled: Shows all if parent object is unfolded.\n\nDisabled: Shows only found results."), GUILayout.Width(25)))
                    {
                        window.showSearchNonFound = !window.showSearchNonFound;
                    }
                }
                else
                {
                    skin.button.alignment = TextAnchor.MiddleCenter;
                    if (GUILayout.Button(Helper.GetGUIContent(search.condition == SearchCondition.Or ? "||" : "&", "|| = Or\n& = And"), GUILayout.Width(25)))
                    {
                        if (search.useSearch && search.hasSearch) hasChanged = true;
                        if (search.condition == SearchCondition.Or) search.condition = SearchCondition.And;
                        else search.condition = SearchCondition.Or;
                    }
                    skin.button.alignment = TextAnchor.MiddleLeft;
                }

                if (GUILayout.Button(Helper.GetGUIContent("X", "Delete search text"), GUILayout.Width(22)))
                {
                    search.text = string.Empty;
                    search.hasSearch = false;
                }
                
                if (search.hasSearch && search.mode == SearchMode.Type && search.types.Count == 0) GUI.color = Color.red;
                GUI.changed = false;
                search.text = GUILayout.TextField(search.text);
                
                if (search.useSearch || GUI.changed)
                {
                    if (GUI.changed) hasChanged = true;
                    search.hasSearch = (search.text != string.Empty);

                    if (search.mode == SearchMode.Type)
                    {
                        FastList<Type> typeList;

                        if (search.text.Contains("[]"))
                        {
                            if (typeNameLookup.TryGetValue(search.text.Replace("[]", ""), out typeList))
                            {
                                search.types.FastClear();
                                search.types.AddRange(typeList);
                                search.MakeArrayTypes();
                            }
                            // Debug.Log(search.type.FullName);

                        }
                        else if (typeNameLookup.TryGetValue(search.text, out typeList))
                        {
                            search.types.FastClear();
                            search.types.AddRange(typeList);
                        }
                        else search.types.FastClear();

                        if (search.types.Count > 0 && window == hierarchyWindow && !drawAssemblies.Value && !drawMemory.Value)
                        {
                            for (int j = 0; j < search.types.Count; j++)
                            {
                                if (!search.types.items[j].IsSubclassOf(typeof(Component))) search.types.RemoveAt(j--);
                            }
                        }
                    }
                }

                GUILayout.EndHorizontal();
                GUI.color = Color.white;
            }

            return hasChanged;
        }

        bool PassSearch(WindowType windowType, Transform t, string name, Type type)
        {
            int searchCount = 0;
            List<Search> searchList;

            if (windowType == WindowType.Hierarchy) searchList = hierarchyWindow.searchList;
            else searchList = inspectorWindow.searchList;

            bool totalPassed = false;
            bool first = true;

            for (int i = 0; i < searchList.Count; i++)
            {
                Search search = searchList[i];

                if (search.useSearch && search.hasSearch)
                {
                    bool passed = false;

                    if (search.mode == SearchMode.Name)
                    {
                        if (t != null) name = t.name;
                        passed = name.IndexOf(search.text, 0, StringComparison.CurrentCultureIgnoreCase) != -1;
                    }
                    else
                    {
                        if (t != null)
                        {
                            for (int j = 0; j < search.types.Count; j++)
                            {
                                Type searchType = search.types.items[j];
                                passed = (searchType != null && searchType.IsSubclassOf(typeof(Component)) && t.GetComponent(searchType) != null);
                                if (passed) break;
                            }
                        }
                        else
                        {
                            for (int j = 0; j < search.types.Count; j++)
                            {
                                Type searchType = search.types.items[j];
                                passed = searchType != null && searchType == type;
                                if (passed) break;
                            }
                        }
                    }

                    if (search.condition == SearchCondition.Or || first) totalPassed |= passed;
                    else totalPassed &= passed;

                    first = false;
                }
                else ++searchCount;
            }

            return totalPassed || searchCount == searchList.Count;
        }

        public DrawInfo GetObjectDrawInfo<T>(Dictionary<T, DrawInfo> lookup, T obj, bool defaultFoldout = false)
        {
            DrawInfo info;
            if (!lookup.TryGetValue(obj, out info))
            {
                info = new DrawInfo(defaultFoldout);
                lookup[obj] = info;
            }
            return info;
        }

        public void DrawDot(Color color, string tooltip, Texture tex, bool minusSpace = false)
        {
            GUI.color = color;
            GUILayout.Space(10);
            Rect rect = GUILayoutUtility.GetLastRect();
            rect.y += 7;
            rect.width = 16;
            rect.height = 16;
            GUI.Label(rect, Helper.GetGUIContent(string.Empty, tex, tooltip));
            // if (minusSpace) GUILayout.Space(-4); else GUILayout.Space(-2);
            
            GUI.color = GetColor(Color.white);
        }

        public Bool3 DrawElement(WindowSettings window, DrawInfo info, int childCount, string name, float width, Texture tex, string tooltip = "")
        {
            Bool3 clicked = new Bool3();

            if (childCount > 0)
            {
                GUILayout.Space(-1);
                // GUI.color = GUI.backgroundColor;
                clicked.v2 = GUILayout.Button(Helper.GetGUIContent(string.Empty, tooltip), GUILayout.Width(21), GUILayout.Height(21));
                Rect rect = GUILayoutUtility.GetLastRect();
                rect.x += (info.foldout ? 4 : 5);
                rect.y += (info.foldout ? 7 : 6);
                rect.width = rect.height = 8;
                GUI.DrawTexture(rect, info.foldout ? texArrowUnfolded : texArrowFolded);
                // GUI.color = GetColor(Color.white);
                GUILayout.Space(-4);
            }
            else
            {
                GUILayout.Space(5);
                DrawDot(GUI.backgroundColor, tooltip, tex);
                GUILayout.Space(5);
                // GUILayout.Toggle(true, Helper.MyGUIContent(string.Empty, tooltip), GUILayout.Width(25));
            }

            GUILayout.Space(2);

            if (width == -1) GUILayout.Label(name);
            else GUILayout.Label(name, GUILayout.Width(width));
           
            clicked.v3 = clicked.v1 || clicked.v2;

            return clicked;
        }

        public Bool3 DrawElement(WindowSettings window, DrawInfo info, int childCount, string text = "", Texture image = null, string tooltip = "", bool isActive = true, int prefix = 0)
        {
            Bool3 clicked = new Bool3();
            GUI.color = isActive ? Color.white : new Color(0.25f, 0.25f, 0.25f, 1);

            GUILayout.BeginHorizontal();
            if (prefix != 0) GUILayout.Space(prefix);

            if (childCount > 0 || childCount == -1)
            {
                if (windowData.showChildCount.Value && childCount > 0) clicked.v2 = GUILayout.Button((Helper.GetGUIContent(childCount.ToString(), info.foldout ? texArrowUnfolded : texArrowFolded)), GUILayout.Width(55), GUILayout.Height(21));
                else clicked.v2 = GUILayout.Button(Helper.GetGUIContent(string.Empty, info.foldout ? texArrowUnfolded : texArrowFolded), GUILayout.Width(25), GUILayout.Height(21));
            }
            else
            {
                GUILayout.Label(string.Empty, GUILayout.Width(windowData.showChildCount.Value ? 55 : 25), GUILayout.Height(21));
            }

            clicked.v1 = GUILayout.Button(Helper.GetGUIContent(text, image, tooltip), GUILayout.Height(21));

            GUILayout.EndHorizontal();

            GUI.color = Color.white;
            clicked.v3 = clicked.v1 || clicked.v2;

            return clicked;
        }

        Color GetColor(Color color)
        {
            color.a *= windowData.backgroundAlpha;
            return color;
        }

        Color GetColor(object obj, object selectedObj, bool needSearch, DrawInfo info)
        {
            if (obj == selectedObj && obj != null) return GetColor(Color.green);
            if (needSearch && info.passSearch == 2) return GetColor(Color.magenta); else return GetColor(Color.white);
        }

        public void DrawBoldLabel(GUISkin skin, string name, float space = -3)
        {
            if (space != 0) GUILayout.Space(space);
            skin.label.fontStyle = FontStyle.Bold;
            GUILayout.Label(name);
            skin.label.fontStyle = FontStyle.Normal;
            if (space != 0) GUILayout.Space(space);
        }

        public void DrawBoldLabel(GUISkin skin, string name)
        {
            skin.label.fontStyle = FontStyle.Bold;
            GUILayout.Label(name);
            skin.label.fontStyle = FontStyle.Normal;
        }

        void DrawPrefixLabel(string prefixName, string name, int prefix)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(prefixName, GUILayout.Width(prefix));
            GUILayout.Label(name);
            GUILayout.EndHorizontal();
        }

        void DrawInputField<T>(string prefixName, T inputField) where T : BaseInputField
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(prefixName, GUILayout.Width(prefix));
            GUI.changed = false;
            inputField.text = GUILayout.TextField(inputField.text);
            if (GUI.changed) inputField.TryParse(false);
            GUILayout.EndHorizontal();
        }

        void DrawToggleField(string prefixName, ref bool toggle)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(prefixName, GUILayout.Width(prefix));
            GUI.changed = false;
            toggle = GUILayout.Toggle(toggle, GUIContent.none);
            GUILayout.EndHorizontal();
        }

        void DrawLastRectOffset(string name, int offset)
        {
            Rect rect = GUILayoutUtility.GetLastRect();
            rect.xMin += offset;
            GUI.Label(rect, name);
        }

        Texture2D ReadBackTexture(Texture2D tex)
        {
            RenderTexture tmp = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            Graphics.Blit(tex, tmp);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = tmp;
            Texture2D texNew = new Texture2D(tex.width, tex.height);
            texNew.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            texNew.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tmp);

            return texNew;
        }

        Texture2D CopyAndRemoveAlphaFromTexture(Texture2D texRead, float colorMulti = 1)
        {
            Texture2D tex = ReadBackTexture(texRead);

            Color[] colors = tex.GetPixels();

            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i].a > 0f) colors[i].a = 1;
                colors[i].r *= colorMulti;
                colors[i].g *= colorMulti;
                colors[i].b *= colorMulti;
            }

            tex.SetPixels(colors);
            tex.Apply();

            return tex;
        }

        void DrawBox(GUIContent guiContent, Color color)
        {
            color.a *= windowData.boxAlpha;
            GUI.backgroundColor = color;
            GUILayout.Box(guiContent);
            color.a = 1;
            GUI.backgroundColor = Color.white;
        }

        void BeginVerticalBox(float alphaMulti = 1)
        {
            Color color = new Color(0, 0, 0, windowData.boxAlpha * alphaMulti);
            GUI.backgroundColor = color;
            GUILayout.BeginVertical("Box");
            GUI.backgroundColor = GetColor(Color.white);
        }

        void SelectGameObjectFromScreenRay()
        {
            if (WindowManager.DoWindowsContainMousePos()) return;

            navigationCamera.SetCam();
            Ray ray = mainCam.ScreenPointToRay(EventInput.mousePosInvY);
            navigationCamera.RestoreCam();
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, 10000, selectLayerMask))
            {
                SelectGameObject(hitInfo.transform.gameObject, true);
                selectionIndicatorLocalPos = hitInfo.transform.InverseTransformPoint(hitInfo.point);
                selectionIndicatorLocalRot = Quaternion.LookRotation(hitInfo.transform.InverseTransformDirection(-hitInfo.normal));

                UpdateSelectedIndicatorTransform();
                navigationCamera.ResetFollowPosRot();
            }
        }

        void SelectGameObject(GameObject go, bool unfold = false)
        {
            // R_Debug.Log("Select " + go.name);

            if (selectedGO == lastScreenRayedGO && unfold) RevertSelectionFoldout();
            else
            {
                selectList.Clear();
            }

            if (go)
            {
                selectionIndicatorLocalPos = Vector3.zero;
                selectionIndicatorLocalRot = Quaternion.LookRotation(Vector3.forward);
            }

            selectedGO = lastScreenRayedGO = go;
#if UNITY_EDITOR
            if (linkSelect.Value)
            {
                UnityEditor.Selection.activeGameObject = go;
            }
#endif
            if (unfold)
            {
                Transform t = go.transform;

                do
                {
                    t = t.parent;
                    if (t == null) break;

                    DrawInfo info = GetObjectDrawInfo(tLookup, t);
                    if (!info.foldout) selectList.Add(info);
                    info.foldout = true;
                }
                while (true);

                GetObjectDrawInfo(sceneLookup, go.scene).foldout = true;
                scrollToSelected = true;
            }
        }

        void RevertSelectionFoldout()
        {
            for (int i = 0; i < selectList.Count; i++)
            {
                selectList.items[i].foldout = false;
            }

            selectList.Clear();
        }

        void DrawWindowTitle(GUIContent guiContent, WindowSettings window)
        {
            GUILayout.BeginHorizontal();
            DrawBox(guiContent, Color.black);
            Helper.Drag(window, inspectorWindow, hierarchyWindow, true);

            skin.button.fontStyle = FontStyle.Bold;
            if (GUILayout.Button(window.isMinimized ? "[]" : "_", GUILayout.Width(21)))
            {
                window.isMinimized = !window.isMinimized;
            }
            if (GUILayout.Button("X", GUILayout.Width(21)))
            {
                SetActive(false);
            }
            skin.button.fontStyle = FontStyle.Normal;

            GUILayout.EndHorizontal();
        }

        void DrawHideFlagsIcons(object obj, HideFlags hideFlags, bool isPrefab, string name)
        {
            Rect rect = GUILayoutUtility.GetLastRect();
            rect.x = rect.xMax - 24;
            rect.width = 24;
            GUI.color = Color.red * 20;
            float iconSize = -24;

            if (hideFlags != HideFlags.None)
            {
                if ((hideFlags & HideFlags.DontUnloadUnusedAsset) != 0)
                {
                    DrawIcon(rect, Helper.GetGUIContent(windowData.texLoad, "DontUnloadUnusedAsset"), Color.white);
                    rect.x += iconSize;
                }
                if ((hideFlags & HideFlags.NotEditable) != 0)
                {
                    DrawIcon(rect, Helper.GetGUIContent(windowData.texEdit, "NotEditable"), Color.white);
                    rect.x += iconSize;
                }
                HideFlags saveHideFlags = (hideFlags & (HideFlags.HideAndDontSave | HideFlags.DontSave | HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor) & ~(HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable));

                if (saveHideFlags != 0)
                {
                    DrawIcon(rect, Helper.GetGUIContent(windowData.texSave, saveHideFlags.ToString()), Color.white);
                    rect.x += iconSize;
                }
                if ((hideFlags & HideFlags.HideInInspector) != 0)
                {
                    DrawIcon(rect, Helper.GetGUIContent(windowData.texInspector, "HideInInspector"), Color.white);
                    rect.x += iconSize;
                }
                if ((hideFlags & HideFlags.HideInHierarchy) != 0)
                {
                    DrawIcon(rect, Helper.GetGUIContent(windowData.texHierarchy, "HideInHierarchy"), Color.white);
                    rect.x += iconSize;
                }
            }
            if (isPrefab)
            {
                DrawIcon(rect, Helper.GetGUIContent(windowData.texPrefab, "Is Prefab"), Color.white);
                rect.x += iconSize;
            }
            if (obj == null)
            {
                DrawIcon(rect, Helper.GetGUIContent(windowData.texDestroyed, "Object is Destroyed"), Color.red);
                rect.x += iconSize;
            }
            else
            {
                var texture = obj as Texture;

                if (texture != null)
                {
                    rect.y++;
                    rect.height -= 2;
                    rect.width = (rect.height / texture.height) * texture.width;
                    if (rect.width > 64) rect.width = 64;
                    float delta = rect.width - rect.height;
                    rect.x -= delta;
                    DrawIcon(rect, Helper.GetGUIContent(texture), Color.white, false);
                    rect.x += iconSize;
                }
            }

            GUI.color = Color.white;
        }

        void DrawIcon(Rect rect, GUIContent guiContent, Color color, bool label = true)
        { 
            GUI.color = color;
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.85f);
            if (label) GUI.Label(rect, guiContent, skin.box);
            else GUI.DrawTexture(rect, guiContent.image);
        }
    }
}