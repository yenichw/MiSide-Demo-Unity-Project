using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DebuggingEssentials
{
    public partial class RuntimeInspector : MonoBehaviour
    {
        public enum DrawMode { Object, Assembly }
        public const BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Default | BindingFlags.FlattenHierarchy;

        Dictionary<Type, TypeData> typeDataLookup = new Dictionary<Type, TypeData>();
        Dictionary<MethodInfo, string> methodArgumentsLookup = new Dictionary<MethodInfo, string>();
        Dictionary<MemberInfo, MemberData> memberDataLookup = new Dictionary<MemberInfo, MemberData>();

        FastQueue<string> methodArgs = new FastQueue<string>(4);

        ScrollViewCullDrawOnce scrollViewCull = new ScrollViewCullDrawOnce();

        GUIChangeBool showFields = new GUIChangeBool(true);
        GUIChangeBool showProperties = new GUIChangeBool(true);
        GUIChangeBool showMethods = new GUIChangeBool(true);

        GUIChangeBool showInherited = new GUIChangeBool(true);
        GUIChangeBool showDeclared = new GUIChangeBool(true);
        GUIChangeBool showStatic = new GUIChangeBool(true);
        GUIChangeBool showInstance = new GUIChangeBool(true);
        GUIChangeBool showPublic = new GUIChangeBool(true);
        GUIChangeBool showPrivate = new GUIChangeBool(true);
        GUIChangeBool showProtected = new GUIChangeBool(true);

        
        FastList<object> searchObjectList = new FastList<object>(1024);
        List<Component> searchComponentList = new List<Component>(1024);
        FastList<SearchMember> searchMemberList = new FastList<SearchMember>(4096);
        FastList<SearchMember> searchMemberPassedList = new FastList<SearchMember>(4096);
        FastList<SearchMember> searchMemberFailedList = new FastList<SearchMember>(4096);
        HashSet<object> searchDoubles = new HashSet<object>();
        HashSet<object> skip = new HashSet<object>();
        HashSet<Type> searchTypes = new HashSet<Type>();
        bool isSearchingInspector;
        bool needInspectorSearch;
        float refreshInspectorSearch;
        int inspectorSearchLevel = 1;
        int localTotalSearched;
        int searchInspectorPerFrame;

        EditField editField = new EditField();
        string editText;
        int selectAllEditText;

        GameObject oldSelectedGO;
        object oldDrawObject;

        DrawMode drawMode;
        float fieldWidth;

        Type selectedStaticType;
        UnityEngine.Object selectedObject;

        float memberSeparationHeight = 15;

        void DrawInspector(int windowId)
        {
            WindowManager.BeginRenderTextureGUI();

            DrawWindowTitle(Helper.GetGUIContent("Deep Inspector", windowData.texInspector), inspectorWindow); 

            if (inspectorWindow.drag == 1) inspectorWindow.isDocked.Value = false;

            Rect lastRect = GUILayoutUtility.GetLastRect();
            lastRect.position += inspectorWindow.rect.position;

            if ((currentEvent.type == EventType.MouseDown && currentEvent.shift && lastRect.Contains(mousePos)))
            {
                inspectorWindow.isDocked.Value = true;
                inspectorWindow.position = new Vector2((hierarchyWindow.rect.xMax + 0) / Screen.width, hierarchyWindow.position.y);
            }

            if (inspectorWindow.isMinimized)
            {
                GUILayout.Space(-2);
                InspectorDragAndTooltip(windowId);
                return;
            }

            fieldWidth = (inspectorWindow.rect.width - 150) / 3.0f;

            BeginVerticalBox();

            GUILayout.BeginHorizontal();
            if (Helper.DrawShowButton(windowData, Helper.GetGUIContent("Fields", "Show/Hide Fields"), showFields, Color.green)) scrollViewCull.recalc = true;
            if (Helper.DrawShowButton(windowData, Helper.GetGUIContent("Properties", "Show/Hide Properties"), showProperties, Color.green)) scrollViewCull.recalc = true;
            if (Helper.DrawShowButton(windowData, Helper.GetGUIContent("Methods", "Show/Hide Methods"), showMethods, Color.green)) scrollViewCull.recalc = true;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (Helper.DrawShowButton(windowData, Helper.GetGUIContent("Inherited", "Show/Hide Inherited Members"), showInherited, colorInherited)) scrollViewCull.recalc = true;
            if (Helper.DrawShowButton(windowData, Helper.GetGUIContent("Declared", "Show/Hide Declared Members"), showDeclared, Color.green)) scrollViewCull.recalc = true;
            if (Helper.DrawShowButton(windowData, Helper.GetGUIContent("Static", "Show/Hide Static Members"), showStatic, colorStatic)) scrollViewCull.recalc = true;
            if (Helper.DrawShowButton(windowData, Helper.GetGUIContent("Instance", "Show/Hide Instance Members"), showInstance, Color.green)) scrollViewCull.recalc = true;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (Helper.DrawShowButton(windowData, Helper.GetGUIContent("Public", "Show/Hide Public Members"), showPublic, colorPublic)) scrollViewCull.recalc = true;
            if (Helper.DrawShowButton(windowData, Helper.GetGUIContent("Protected", "Show/Hide Protected Members"), showProtected, colorProtected)) scrollViewCull.recalc = true;
            if (Helper.DrawShowButton(windowData, Helper.GetGUIContent("Private", "Sow/Hide Private Members"), showPrivate, colorPrivate)) scrollViewCull.recalc = true;
            GUILayout.EndHorizontal();

            if (DrawSearch(inspectorWindow, true))
            {
                scrollViewCull.recalc = true;
                refreshInspectorSearch = 1;
            }

            if (needInspectorSearch)
            {
                GUILayout.Space(-2);
                GUILayout.BeginHorizontal();
                GUILayout.Space(6);
                GUILayout.Label("Search Level " + inspectorSearchLevel, GUILayout.Width(90));
                GUI.changed = false;
                int oldInspectorSearchLevel = inspectorSearchLevel;
                inspectorSearchLevel = (int)GUILayout.HorizontalSlider(inspectorSearchLevel, 1, 7, GUILayout.Width(82));
                if (GUI.changed)
                {
                    if (inspectorSearchLevel != oldInspectorSearchLevel)
                    {
                        if (inspectorSearchLevel > oldInspectorSearchLevel) refreshInspectorSearch = 1;
                        ClearMemberLookups(inspectorSearchLevel + 1);
                    }
                }

                if (searchTypes.Count == 0 && drawScenes.Value)
                {
                    GUI.color = GetColor(Helper.AnimateColor(Color.magenta, 2));
                    GUILayout.Label("Click on a Component to search inside it");
                    GUI.color = GetColor(Color.white);
                }
                else GUILayout.Label("Found " + totalFound + " / " + totalSearched);

                GUILayout.EndHorizontal();
            }
            else totalFound = totalSearched = 0;

            GUILayout.Space(3);

            if (selectedStaticType != null && drawAssemblies.Value) DrawSelected(selectedStaticType, DrawMode.Assembly);
            else if (selectedObject != null && drawMemory.Value) DrawSelected(selectedObject, DrawMode.Object);
            else if (selectedGO) DrawGameObject(selectedGO);

            GUILayout.EndVertical();

            InspectorDragAndTooltip(windowId);

            WindowManager.EndRenderTextureGUI();
        }

        void InspectorDragAndTooltip(int windowId)
        {
            if (Helper.Drag(inspectorWindow, inspectorWindow, hierarchyWindow, false))
            {
                scrollViewCull.recalc = true;
            }
            if (!DrawEnum.draw) WindowManager.SetToolTip(windowId);
        }

        void ClearMemberLookups(int startLevel)
        {
            for (int i = startLevel; i < memberLookups.Count; i++)
            {
                memberLookups.items[i].Clear();
            }
        }

        void DrawSelected(object obj, DrawMode drawMode)
        {
            if (obj != oldDrawObject)
            {
                oldDrawObject = obj;
                scrollViewCull.ResetAndRecalc();
            }

            this.drawMode = drawMode;

            GameObject go = obj as GameObject;
            if (go)
            {
                DrawGameObject(go);
                return;
            }
            
            DrawObject(obj, drawMode == DrawMode.Assembly ? windowData.componentIcons.csScriptIcon : null);

            BeginVerticalBox(0.75f);
            scrollViewCull.BeginScrollView();
            
            DrawObjectMembers(null, obj, 1, needInspectorSearch, false, drawMode == DrawMode.Assembly, 15);

            scrollViewCull.EndScrollView();
            GUILayout.EndVertical();
        }

        //void DrawSearchTypesButtons()
        //{
        //    GUI.backgroundColor = GetColor(Color.magenta);

        //    int count = 0;

        //    for (int i = 0; i < searchTypes.Count; i++)
        //    {
        //        Type type = searchTypes.items[i];

        //        if (i == 0) GUILayout.BeginHorizontal();

        //        if (GUILayout.Button(Helper.GetGUIContent(type.Name, "Click to stop searching\nControl click to stop searching in all")))
        //        {
        //            if (currentEvent.control) { searchTypes.Clear(); break; }
        //            else searchTypes.RemoveAt(i--);
        //        }
        //        if (++count == 3)
        //        {
        //            count = 0;
        //            GUILayout.EndHorizontal();
        //            if (i < searchTypes.Count - 1) GUILayout.BeginHorizontal();
        //        }
        //    }
        //    if (count != 0) GUILayout.EndHorizontal();

        //    GUI.backgroundColor = GetColor(Color.white);
        //}

        void ClearEdit()
        {
            editField.Reset();
        }

        bool SubmitEdit()
        {
            if (currentEvent.type == EventType.Layout) return false;
            if (currentEvent.keyCode == KeyCode.Escape) ClearEdit();
            return (currentEvent.keyCode == KeyCode.Return || currentEvent.keyCode == KeyCode.KeypadEnter);
        }

        void DrawObject(object obj, Texture icon = null)
        {
            float height = 0, width = 0;
            string name;

            Texture tex = obj as Texture;
            if (tex != null)
            {
                name = obj.ToString();
                icon = tex;

                height = Mathf.Clamp(icon.height, 23, 150);
                width = ((icon.width / icon.height) * height) - 8;
                if (width > 256) width = 256;
            }
            else
            {
                bool isMonoScript = false; ;
#if UNITY_EDITOR
                isMonoScript = (obj is UnityEditor.MonoScript);
#endif
                GameObject go = obj as GameObject;

                if (go) name = go.name;
                else if (isMonoScript) name = obj.GetType().Name;
                else name = obj.ToString();

                height = 23;
                width = 16;
            }

            if (icon == null)
            {
                icon = windowData.componentIcons.GetIcon(obj);
                height = 23;
                width = 16;
            }

            GUI.backgroundColor = new Color(0.25f, 0.25f, 0.25f, windowData.boxAlpha);
            GUILayout.BeginVertical("Box");
            GUILayout.Space(-4);
            GUI.backgroundColor = Color.white;

            GUILayout.BeginHorizontal();

            if (icon != null)
            {
                GUILayout.Label(Helper.GetGUIContent(icon), GUILayout.Width(width), GUILayout.Height(height));
            }
            skin.label.fontStyle = FontStyle.Bold;
            float sizeY = skin.label.CalcSize(Helper.GetGUIContent(name)).y;
            GUILayout.Label(name, GUILayout.Height(Mathf.Max(height, sizeY)));
            skin.label.fontStyle = FontStyle.Normal;

            GUILayout.EndHorizontal();
            GUILayout.Space(-4);
            GUILayout.EndVertical();
        }

        void DrawGameObject(GameObject go)
        {
            if (go != oldSelectedGO)
            {
                scrollViewCull.recalc = true;
                oldSelectedGO = go;
            }

            DrawObject(go);
            GUILayout.Space(-26);
            GUILayout.BeginHorizontal();
            GUILayout.Space(6);
            GUILayout.Label(string.Empty);
            
            string layerName = LayerMask.LayerToName(go.layer);
            skin.button.alignment = TextAnchor.MiddleCenter;
            skin.button.fontStyle = FontStyle.Bold;
            if (GUILayout.Button(layerName, GUILayout.Width(150)))
            {
                drawEnum.InitEnumWindow(go, mousePos);
            }
            skin.button.fontStyle = FontStyle.Normal;
            skin.button.alignment = TextAnchor.MiddleLeft;
            GUILayout.Space(6);
            skin.label.fontStyle = FontStyle.Normal;
            GUILayout.EndHorizontal();
            skin.box.alignment = TextAnchor.MiddleCenter;

            BeginVerticalBox(0.75f);
            scrollViewCull.BeginScrollView();
            
            go.GetComponents(components);

            for (int i = 0; i < components.Count; i++)
            {
                Component c = components[i];
                if (c) DrawComponent(c, 0);
            }
            
            scrollViewCull.EndScrollView();
            GUILayout.EndVertical();
        }

        void DrawComponent(Component component, int level)
        {
            if (memberLookups.Count == 0)
            {
                memberLookups.Add(new Dictionary<object, DrawInfo>(memberLookupsCapacity));
            }

            int enabledValue = -1;
            Behaviour b = null;
            Renderer r = null;
            Collider c = null;

            b = component as Behaviour;
            if (b != null) { enabledValue = (b.enabled ? 1 : 0); goto End; }

            r = component as Renderer;
            if (r != null) { enabledValue = (r.enabled ? 1 : 0); goto End; }

            c = component as Collider;
            if (c != null) { enabledValue = (c.enabled ? 1 : 0); goto End; }

            End:;

            float isEnabledMulti = enabledValue != 0 ? 1 : 0.5f;

            Type type = component.GetType();
            DrawInfo info = GetObjectDrawInfo(memberLookups.items[level], type);
            bool needSearch = searchTypes.Contains(type);
            
            if (scrollViewCull.StartCull())
            {
                GUI.backgroundColor = (component is MonoBehaviour ? GetColor(new Color(0.5f, 0.75f, 1f) * isEnabledMulti) : GetColor(new Color(1f, 1f, 1f) * isEnabledMulti));

                skin.button.fontStyle = FontStyle.Bold;
                GUILayout.BeginHorizontal();

                if (needSearch) GUI.backgroundColor = GetColor(Color.magenta);

                string tooltip = needSearch ? "Click to stop searching inside\nControl click to stop searching in all\nShift click to enable/disable" : "Click to search inside\nShift click to enable/disable";

                Bool3 clicked = DrawElement(inspectorWindow, info, -1, component.GetType().Name, windowData.componentIcons.GetIcon(component), tooltip, enabledValue != 0);

                if (clicked.v2)
                {
                    info.foldout = !info.foldout;
                    scrollViewCull.recalc = true;
                    return;
                }
                else if (clicked.v1)
                {
                    if (currentEvent.control)
                    {
                        searchTypes.Clear();
                    }
                    else if (currentEvent.shift)
                    {
                        if (b != null) b.enabled = !b.enabled;
                        else if (r != null) r.enabled = !r.enabled;
                        else if (c != null) c.enabled = !c.enabled;
                    }
                    else
                    {
                        if (!searchTypes.Contains(type))
                        {
                            refreshInspectorSearch = 1;
                            searchTypes.Add(type);
                        }
                        else searchTypes.Remove(type);
                    }
                }

                GUI.backgroundColor = GetColor(Color.white);

                GUILayout.EndHorizontal();

                skin.button.fontStyle = FontStyle.Normal;
                GUI.backgroundColor = GetColor(Color.white);
            }

            scrollViewCull.EndCull();
            
            if (!needInspectorSearch) needSearch = false;

            if (PassedFoldout(info, needSearch, false, inspectorWindow.showSearchNonFound, false))
            {
                DrawObjectMembers(null, component, level + 1, needSearch, info.foldout, false, 15);
            }
        }

        bool DrawObjectMembers(MemberInfo parentMember, object obj, int level, bool needSearch, bool hasSearchPass, bool isType, float indent)
        {
            if (level >= memberLookups.Count)
            {
                memberLookups.Add(new Dictionary<object, DrawInfo>(memberLookupsCapacity));
            }

            Type type;
            if (isType) type = (Type)obj;
            else type = obj.GetType();

            TypeData typeData = GetTypeData(type);
            FastList<SubTypeData> subTypeDatas = typeData.subTypeDatas;

            bool result = false;

            // FieldInfo[] fields = typeData.fields;
            // PropertyInfo[] properties = typeData.properties;
            // MethodInfo[] methods = typeData.methods;

            bool didDraw = false;
            bool didDrawFields = false;
            bool didDrawProperties = false;

            if (showFields.Value)
            {
                for (int i = 0; i < subTypeDatas.Count; i++)
                {
                    if (!showInherited.Value && i >= 1) break;

                    SubTypeData subTypeData = subTypeDatas.items[i];
                    FieldInfo[] fields = subTypeData.fields;
                    if (fields.Length > 0)
                    {
                        result |= DrawMembers(parentMember, obj, typeData, subTypeData, fields, MemberType.Field, level, needSearch, hasSearchPass, indent, out didDraw);
                        didDrawFields = true;
                    }
                }
            }

            bool didDrawSpace = false;

            if (showProperties.Value)
            {
                for (int i = 0; i < subTypeDatas.Count; i++)
                {
                    if (!showInherited.Value && i >= 1) break;

                    SubTypeData subTypeData = subTypeDatas.items[i];
                    PropertyInfo[] properties = subTypeData.properties;

                    if (properties.Length > 0)
                    {
                        if (!didDrawSpace && showFields.Value && didDrawFields && !needSearch && didDraw)
                        {
                            didDrawSpace = true;
                            if (scrollViewCull.StartCull()) GUILayout.Space(memberSeparationHeight);
                            scrollViewCull.EndCull();
                        }
                        result |= DrawMembers(parentMember, obj, typeData, subTypeData, properties, MemberType.Property, level, needSearch, hasSearchPass, indent, out didDraw);
                        didDrawProperties = true;
                    }
                }
            }

            if (showMethods.Value)
            {
                didDrawSpace = false;
                for (int i = 0; i < subTypeDatas.Count; i++)
                {
                    if (!showInherited.Value && i >= 1) break;

                    SubTypeData subTypeData = subTypeDatas.items[i];
                    MethodInfo[] methods = subTypeData.methods;

                    if (methods.Length > 0)
                    {
                        if (!didDrawSpace && ((showFields.Value && didDrawFields || showProperties.Value && didDrawProperties) && !needSearch && didDraw))
                        {
                            didDrawSpace = true;
                            if (scrollViewCull.StartCull()) GUILayout.Space(memberSeparationHeight);
                            scrollViewCull.EndCull();
                        }
                        result |= DrawMembers(parentMember, obj, typeData, subTypeData, methods, MemberType.Method, level, needSearch, hasSearchPass, indent, out didDraw);
                    }
                }
            }

            return result;
        }

        bool DrawMembers(MemberInfo parentMember, object obj, TypeData typeData, SubTypeData subTypeData, MemberInfo[] members, MemberType memberType, int level, bool needSearch, bool hasSearchPass, float indent, out bool didDraw)
        {
            if (!needSearch) 
            {
                string name;
                if (memberType == MemberType.Field) name = "Fields ";
                else if (memberType == MemberType.Property) name = "Properties ";
                else if (memberType == MemberType.Method) name = "Methods ";
                else name = "";

                if (scrollViewCull.StartCull())
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(indent);
                    GUI.backgroundColor = GetColor(new Color(0.2f, 0.2f, 0.2f, 0.5f));
                    GUI.skin.box.alignment = TextAnchor.MiddleLeft;
                    GUILayout.Box(name + members.Length + " " + subTypeData.typeName);
                    GUI.backgroundColor = GetColor(Color.white);
                    GUI.skin.box.alignment = TextAnchor.MiddleCenter;
                    GUILayout.EndHorizontal();
                }
                scrollViewCull.EndCull();
            }

            didDraw = false;
            bool result = false;

            for (int i = 0; i < members.Length; i++)
            {
                MemberInfo member = members[i];
                if (member != null) result |= DrawMember(parentMember, obj, typeData, subTypeData, member, memberType, level, needSearch, hasSearchPass, ref didDraw, i == members.Length - 1, indent);
            }
            
            return result;
        }

        TypeData GetTypeData(Type objType)
        {
            TypeData typeData;
            if (!typeDataLookup.TryGetValue(objType, out typeData))
            {
                typeData = new TypeData(objType, bindingFlags);
                typeDataLookup.Add(objType, typeData);
            }

            return typeData;
        }

        MemberData GetMemberData(Type objType, MemberInfo member, MemberType memberType)
        {
            MemberData memberData;
            if (!memberDataLookup.TryGetValue(member, out memberData))
            {
                memberData = new MemberData(objType, member, memberType, colorPublic, colorProtected, colorPrivate);
                memberDataLookup.Add(member, memberData);
            }
            return memberData;
        }

        bool DrawMember(MemberInfo parentMember, object obj, TypeData typeData, SubTypeData subTypeData, MemberInfo member, MemberType memberType, int level, bool needSearch, bool hasSearchPass, ref bool didDraw, bool isLastMember, float indent, Array array = null, int arrayIndex = -1)
        {
            while (level >= memberLookups.Count)
            {
                memberLookups.Add(new Dictionary<object, DrawInfo>(memberLookupsCapacity));
            }

            object lookupObj;
            
            if (member == null)
            {
                if (arrayIndex != -1) lookupObj = arrayIndex;
                else lookupObj = obj;
            }
            else lookupObj = member;

            DrawInfo info = GetObjectDrawInfo(memberLookups.items[level], lookupObj);

            if (!PassedFoldout(info, needSearch, hasSearchPass, inspectorWindow.showSearchNonFound, true)) return false;

            MemberData memberData;
            Type objType = subTypeData.type;

            if (arrayIndex == -1)
            {
                memberData = GetMemberData(objType, member, memberType);
            }
            else memberData = null;

            FieldInfo field;
            PropertyInfo prop;
            MethodInfo method;
            Type type;

            Scope scope;

            bool isStatic;
            bool isInherited;
            bool isConstant;

            if (arrayIndex == -1)
            {
                field = memberData.field;
                prop = memberData.prop;
                method = memberData.method;
                type = memberData.type;
                isStatic = memberData.isStatic;
                isInherited = subTypeData.index > 0;
                isConstant = memberData.isConstant;
                scope = memberData.scope;
                typeData = null;
            }
            else
            {
                field = null;
                prop = null;
                method = null;
                isConstant = false;
                isStatic = false;
                isInherited = false;
                type = objType;
                scope = Scope.Public;
            } 

            bool result = false;

            if (arrayIndex == -1)
            {
                if ((!isInherited && !showDeclared.Value) || (!isStatic && !showInstance.Value)) return result;
                if ((isInherited && !showInherited.Value) || (isStatic && !showStatic.Value)) return result;
                if ((scope == Scope.Public && !showPublic.Value) || (scope == Scope.Protected && !showProtected.Value) || (scope == Scope.Private && !showPrivate.Value)) return result;
            }
            // ==================================================================================================================================

            bool isString;
            bool isClass; 
            bool isStruct;
            bool isArray; 
            bool isValueThis;
            bool isInterface;

            string name;
            string typeName;

            if (arrayIndex == -1)
            {
                isString = memberData.isString;
                isClass = memberData.isClass;
                isStruct = memberData.isStruct;
                isArray = memberData.isArray;
                isInterface = memberData.isInterface;
                name = memberData.name;
                typeName = memberData.typeName;
            }
            else
            {
                isString = typeData.isString;
                isClass = typeData.isClass;
                isStruct = typeData.isStruct;
                isArray = typeData.isArray;
                isInterface = typeData.isInterface;
                name = string.Empty;
                typeName = subTypeData.typeName;
            }

            object value = null;
            
            if (memberType == MemberType.Field)
            {
                if (drawMode == DrawMode.Object || isStatic || level >= 1)
                {
                    try
                    {
                        value = field.GetValue(obj);
                    }
                    catch (Exception)
                    {
                        skip.Add(field);
                    }
                }
            }
            else if (memberType == MemberType.Property)
            {
                if (prop.IsDefined(typeof(ObsoleteAttribute), true)) return false;
                
                if (prop.GetIndexParameters().Length == 0) 
                {
                    var property = (PropertyInfo)member;

                    if (!skip.Contains(property))
                    {
                        try
                        {
                            bool tryGetValue = true;
                            if (objType == typeof(MeshFilter) && name == "mesh") tryGetValue = false;
                            else if (objType == typeof(MeshRenderer) && (name == "material" || name == "materials")) tryGetValue = false;

                            if (tryGetValue) value = property.GetValue(obj, null);
                        }
                        catch (Exception)
                        {
                            skip.Add(property);
                        }
                    }
                }
            }
            else if (arrayIndex != -1)
            {
                name = "Element " + arrayIndex;
                value = obj;
            }

            if (isArray && value != null && !value.Equals(null))
            {
                array = (Array)value;
                name += " [" + array.Length + "]";
            }

            isValueThis = (arrayIndex == -1 && value == obj);

            // name += " " + needSearch + " " + info.passSearch + " " + hasSearchPass + " " + level;

            bool canFoldout = memberType != MemberType.Method && ((isClass || isArray || isStruct || isInterface) && value != null && !isValueThis); // !value.Equals(null)

#if UNITY_EDITOR
            bool isMonoScript = (obj is UnityEditor.MonoScript && name == "text");
            if (isMonoScript) canFoldout = false;
#else
            bool isMonoScript = false;
#endif

            didDraw = true;

            Color scopeColor;
            string scopeTooltip;

            if (arrayIndex != -1)
            {
                scopeColor = Color.white;
                scopeTooltip = string.Empty;
            }
            else
            {
                scopeColor = memberData.scopeColor;
                scopeTooltip = memberData.scopeToolTip;
            }

            if (scrollViewCull.StartCull())
            {
                if (needSearch && info.passSearch == 2)
                {
                    GUI.skin.box.overflow.left -= (int)indent - 8;
                    GUI.color = Color.magenta * windowData.boxAlpha * 0.85f;
                    GUILayout.BeginVertical("Box");
                    GUI.skin.box.overflow.left += (int)indent - 8;
                    GUI.color = Color.white;
                }

                GUILayout.BeginHorizontal();
                if (needSearch && info.passSearch == 2) indent -= 8;
                GUILayout.Space(indent);
                if (needSearch && info.passSearch == 2) indent += 8;
                GUI.backgroundColor = scopeColor;

                // name += " " + info.passSearch + " " + hasSearchPass;

                if (memberType == MemberType.Method)
                {
                    if (isInherited) DrawDot(colorInherited, "Is Inherited", windowData.texDot2, true); else GUILayout.Space(10);
                    if (isStatic) DrawDot(colorStatic, "Is Static", windowData.texDot2, true); else GUILayout.Space(10);
                    GUILayout.Space(-3);
                    DrawElement(inspectorWindow, info, canFoldout ? 1 : 0, name, -1, windowData.texDot2, scopeTooltip);
                    ParameterInfo[] parameters = memberData.parameters;

                    if (memberData.validInvokeParameters && RuntimeConsole.instance)
                    {
                        string inputArguments = string.Empty;

                        if (parameters.Length > 0)
                        {
                            if (!methodArgumentsLookup.TryGetValue(method, out inputArguments))
                            {
                                inputArguments = string.Empty;
                            }

                            float size = 15;
                            if (inputArguments.Length > 0)
                            {
                                size += skin.textField.CalcSize(Helper.GetGUIContent(inputArguments)).x;
                                if (size > 200) size = 200;
                            }

                            inputArguments = GUILayout.TextField(inputArguments, GUILayout.Width(size));
                            methodArgumentsLookup[method] = inputArguments;
                        }

                        if (GUILayout.Button("Invoke", GUILayout.Width(55)))
                        {
                            string executeArguments = inputArguments.Replace(',', ' ');
                            if (parameters.Length > 0)
                            {
                                RuntimeConsole.GetArguments(executeArguments, methodArgs);
                            }

                            string command = obj.ToString() + " => " + name + " " + inputArguments;

                            if (RuntimeConsole.instance.showConsoleWhenInvokingMethod) RuntimeConsole.SetActive(true);

                            RuntimeConsole.Log(new LogEntry(command, null, LogType.Log, EntryType.Command, Color.green, RuntimeConsole.instance.logFontSize, FontStyle.Bold));
                            if (HtmlDebug.instance) HtmlDebug.instance.UnityDebugLog(command, null, LogType.Log, true, -1, null, EntryType2.Command, false);
                            RuntimeConsole.CommandData commandData = new RuntimeConsole.CommandData(null, obj, "", RuntimeConsole.MemberType.Method, member, parameters, true);
                            commandData.Execute(methodArgs, executeArguments);
                        }
                    }
                }
                else
                {
                    if (DrawElement(inspectorWindow, info, canFoldout ? 1 : 0, name, fieldWidth, windowData.texDot, scopeTooltip).v2)
                    {
                        info.foldout = !info.foldout;
                        scrollViewCull.recalc = true;
                        return false;
                    }

                    GUI.backgroundColor = GetColor(Color.white);

                    // DrawDot(color, scopeText);
                    if (isInherited) DrawDot(colorInherited, "Is Inherited", windowData.texDot2, true); else GUILayout.Space(10);
                    if (isStatic) DrawDot(colorStatic, "Is Static", windowData.texDot2, true); else GUILayout.Space(10);
                    GUILayout.Space(3);

                    GUILayout.Label(typeName, GUILayout.Width(fieldWidth));
                }

                GUI.backgroundColor = GetColor(Color.white);

                if (memberType == MemberType.Property)
                {
                    if (prop.CanRead)
                    {
                        GUI.color = Color.green;
                        GUILayout.Toggle(true, Helper.GetGUIContent(string.Empty, "Has a Getter"), GUILayout.Width(10));
                    }
                    else GUILayout.Space(14);

                    if (prop.CanWrite)
                    {
                        GUI.color = Color.red;
                        GUILayout.Toggle(true, Helper.GetGUIContent(string.Empty, "Has a Setter"), GUILayout.Width(10));
                    }
                    else GUILayout.Space(14);

                    GUI.color = Color.white;
                }

                string valueName = string.Empty;

                if (value != null)
                {
                    bool isEnum = type.IsEnum;

                    if ((type.IsPrimitive || isString || isEnum) && (memberType != MemberType.Property || prop.CanWrite) && !isConstant)
                    {
                        if (editField.IsThisEdit(parentMember, member, arrayIndex))
                        {
                            GUI.SetNextControlName("InspectorEditText");

                            editText = GUILayout.TextField(editText, wrapTextField);

                            if (selectAllEditText > 0)
                            {
                                GUI.FocusControl("InspectorEditText");
                                var editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                                editor.SelectAll();
                                selectAllEditText = 0;
                            }

                            if (SubmitEdit())
                            {
                                ClearEdit();

                                object newValue;

                                if (isEnum)
                                {
                                    int intValue;
                                    int.TryParse(editText, out intValue);
                                    newValue = Enum.ToObject(type, intValue);
                                }
                                else newValue = Convert.ChangeType(editText, type);

                                if (arrayIndex != -1) array.SetValue(newValue, arrayIndex);
                                else if (memberType == MemberType.Field) field.SetValue(obj, newValue);
                                else prop.SetValue(obj, newValue, null);

                                result = true;
                            }
                        }
                        else
                        {
                            if (isEnum) GUI.backgroundColor = GetColor(new Color(1, 1, 0.55f, 1));
                            GUILayout.Space(-1);
                            if (GUILayout.Button(value.ToString(), wrapButton))
                            {
                                if (isEnum)
                                {
                                    drawEnum.InitEnumWindow(obj, member, (Enum)value, mousePos);
                                }
                                else if (type == typeof(bool))
                                {
                                    if (arrayIndex != -1) array.SetValue(true, arrayIndex);
                                    else if (memberType == MemberType.Field) field.SetValue(obj, !(bool)value);
                                    else prop.SetValue(obj, !(bool)value, null);
                                }
                                else
                                {
                                    editField.Set(parentMember, member, arrayIndex);
                                    editText = value.ToString();
                                    selectAllEditText = 200;
                                }
                            }
                            GUI.backgroundColor = GetColor(Color.white);
                        }
                    }
                    else
                    {
                        var valueObj = value as UnityEngine.Object;
                        if (valueObj != null) { valueName = valueObj.name; goto End; }

                        if (memberType == MemberType.Field)
                        {
                            FieldInfo fieldInfo = value.GetType().GetField("name", bindingFlags);
                            if (fieldInfo != null)
                            {
                                object newValue = fieldInfo.GetValue(value);
                                if (newValue != null) { valueName = newValue.ToString(); goto End; }
                            }
                        }

                        if (type.IsGenericType)
                        {
                            Type[] genericTypes = type.GetGenericArguments();
                            valueName += "<";
                            for (int i = 0; i < genericTypes.Length; i++)
                            {
                                if (i < genericTypes.Length - 1) valueName += type.GetGenericArguments()[i].Name + ", ";
                                else valueName += type.GetGenericArguments()[i].Name;
                            }
                            valueName += ">";
                        }
                        else if (!isClass) valueName = value.ToString();

                        End:;

                        if (isValueThis)
                        {
                            GUI.color = Color.green;
                            GUILayout.Label(Helper.GetGUIContent("this", null, "value references to itself"));
                            GUI.color = Color.white;
                        }

                        else if (valueObj != null && (valueObj is Component || valueObj is GameObject))
                        {
                            GUI.backgroundColor = GetColor(new Color(0.25f, 0.75f, 1, 1));

                            GUILayout.Space(-1);
                            if (Helper.DrawButton(Helper.GetGUIContent(valueName, windowData.componentIcons.GetIcon(valueObj)), wrapButton))
                            {
                                if (valueObj is GameObject) SelectGameObject((GameObject)valueObj, true);
                                else if (valueObj is Component) SelectGameObject(((Component)valueObj).gameObject, true);
                            }
                            GUI.backgroundColor = GetColor(Color.white);
                        }
                        else
                        {
                            if (isMonoScript && valueName.Length > 0)
                            {
                                GUILayout.EndHorizontal();
                                if (valueName.Length > 8192) valueName = valueName.Substring(0, 8192);
                                GUILayout.Label(valueName);
                            }
                            else
                            {
                                if (isConstant)
                                {
                                    GUI.color = Color.yellow;
                                    GUILayout.Label(Helper.GetGUIContent(valueName, null, "is Constant"));
                                    GUI.color = Color.white;
                                }
                                else GUILayout.Label(valueName);
                            }
                        }
                    }
                }
                else if (memberType != MemberType.Method)
                {
                    GUI.color = Color.red;
                    GUILayout.Label("null");
                    GUI.color = Color.white;
                }

                // GUI.color = Color.white; 
                GUILayout.EndHorizontal();
                if (needSearch && info.passSearch == 2) GUILayout.EndVertical();

                if (!isLastMember)
                {
                    Rect rect = GUILayoutUtility.GetLastRect();
                    rect.x += indent;
                    rect.y += rect.height + 1;
                    rect.height = 2;

                    float c = 0.1f;
                    GUI.color = new Color(c, c, c, 1);
                    GUI.DrawTexture(rect, Texture2D.whiteTexture);
                    GUI.color = Color.white;
                }
            }
            scrollViewCull.EndCull();

            if (canFoldout && value != null)
            {
                hasSearchPass = info.foldout && (hasSearchPass || info.passSearch == 2);

                if (PassedFoldout(info, needSearch, hasSearchPass, inspectorWindow.showSearchNonFound, false))
                {
                    if (isArray) DrawArray(member, array, level + 1, needSearch, hasSearchPass, indent + 15);
                    else
                    {
                        result = DrawObjectMembers(member, value, level + 1, needSearch, info.foldout, false, indent + 15);

                        if (result)
                        {
                            if (arrayIndex != -1) array.SetValue(value, arrayIndex);
                            else if (memberType == MemberType.Field) field.SetValue(obj, value);
                            else prop.SetValue(obj, value, null);
                        }
                    }

                    return false;
                }
            }

            return result;
        }

        void DrawArray(MemberInfo parentMember, Array array, int level, bool needSearch, bool hasSearchPass, float indent)
        {
            int nullStart = 0;
            int nullEnd = 0;
            bool didDraw = false;

            int arrayDimensions = array.GetType().GetArrayRank();

            if (arrayDimensions == 1)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    object value = array.GetValue(i);

                    if (value != null)
                    {
                        if (nullEnd > nullStart)
                        {
                            if (scrollViewCull.StartCull())
                            {
                                DrawLabelIndent(nullStart + " .. " + nullEnd + " = null", indent);
                            }
                            scrollViewCull.EndCull();
                        }

                        Type valueType = value.GetType();

                        // FieldInfo field = valueType.GetField("m_value", bindingFlags);
                        // MemberType memberType;
                        // if (field != null) memberType = MemberType.Field;
                        // else memberType = MemberType.ArrayElement; 

                        TypeData typeData = GetTypeData(valueType);
                        FastList<SubTypeData> subTypeDatas = typeData.subTypeDatas;
                        SubTypeData subTypeData = subTypeDatas.items[0];
                        
                        DrawMember(parentMember, value, typeData, subTypeData, null, MemberType.ArrayElement, level + 1, needSearch, hasSearchPass, ref didDraw, false, indent, array, i);

                        nullEnd = ++nullStart;
                    }
                    else nullEnd++;
                }

                if (nullEnd > nullStart)
                {
                    didDraw = true;

                    if (scrollViewCull.StartCull())
                    {
                        DrawLabelIndent(nullStart + " .. " + nullEnd + " = null", indent);
                    }
                    scrollViewCull.EndCull();
                }
            }
            else
            {
                didDraw = true;

                if (scrollViewCull.StartCull())
                {
                    DrawLabelIndent("Multi dimensional arrays will be supported soon", indent);
                }
                scrollViewCull.EndCull();
            }

            if (didDraw)
            {
                if (scrollViewCull.StartCull()) GUILayout.Space(memberSeparationHeight);
                scrollViewCull.EndCull();
            }

            DrawObjectMembers(parentMember, array, level++, needSearch, hasSearchPass, false, indent);
        }

        void DrawLabelIndent(string text, float indent)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(indent);
            GUILayout.Label(text);
            GUILayout.EndHorizontal();
        }

        void SearchInsideGameObject(GameObject go)
        {
            if (refreshInspectorSearch == 0) return;

            if (!isSearchingInspector)
            {
                go.GetComponents(searchComponentList);
                searchObjectList.Clear();
                for (int i = 0; i < searchComponentList.Count; i++)
                {
                    searchObjectList.Add(searchComponentList[i]);
                }
                StartCoroutine(SearchInspectorCR(true));
            }
        }

        void SearchInspector(object obj)
        {
            if (refreshInspectorSearch == 0) return;

            if (!isSearchingInspector)
            {
                searchObjectList.Clear();
                searchObjectList.Add(obj);
                StartCoroutine(SearchInspectorCR(false));
            }
        }

        void AddSearchMembers(object obj, Type objType, SearchMember searchMember, MemberInfo[] members, MemberType memberType, int level) 
        {
            for (int j = 0; j < members.Length; j++)
            {
                searchMemberList.Add(new SearchMember(searchMember, obj, objType, members[j], memberType, null, level));
            }
        }

        void AddAllSearchMembers(object searchObj, Type objType, SearchMember searchMember, int level)
        {
            TypeData typeData = GetTypeData(objType);
            FastList<SubTypeData> subTypeDatas = typeData.subTypeDatas;

            for (int i = 0; i < subTypeDatas.Count; i++)
            {
                SubTypeData subTypeData = subTypeDatas.items[i];
                if (showFields.Value) AddSearchMembers(searchObj, objType, searchMember, subTypeData.fields, MemberType.Field, level);
                if (showProperties.Value) AddSearchMembers(searchObj, objType, searchMember, subTypeData.properties, MemberType.Property, level);
                if (showMethods.Value) AddSearchMembers(searchObj, objType, searchMember, subTypeData.methods, MemberType.Method, level);
            }
        }

        void SetSearchInspectorPerFrame()
        {
            searchInspectorPerFrame = Mathf.CeilToInt(windowData.searchInspectorPerFrame.value * refreshInspectorSearch);
        }

        IEnumerator SearchInspectorCR(bool checkSearchType)
        {
            // RuntimeConsole.Log("Refresh Inspector Search " + refreshInspectorSearch.ToString(), true);
            isSearchingInspector = true;
           
            searchMemberList.Clear();
            searchMemberPassedList.Clear();
            searchMemberFailedList.Clear();
            searchDoubles.Clear();

            int oldInspectorSearchLevel = inspectorSearchLevel;

            if (memberLookups.Count == 0)
            {
                memberLookups.Add(new Dictionary<object, DrawInfo>(memberLookupsCapacity));
            }

            localTotalSearched = 0;
            int count = 0;

            SetSearchInspectorPerFrame();

            for (int i = 0; i < searchObjectList.Count; i++)
            {
                object searchObj = searchObjectList.items[i];

                if (!searchDoubles.Add(searchObj)) continue;

                bool isType = searchObj is Type;

                Type objType;
                if (isType) objType = (Type)searchObj;
                else objType = searchObj.GetType();

                DrawInfo info = GetObjectDrawInfo(memberLookups.items[0], objType);

                if (checkSearchType && !searchTypes.Contains(objType)) continue;

                SearchMember searchMember = new SearchMember(null, searchObj, objType, null, (searchObj is Behaviour ? MemberType.Field : MemberType.Property), info, 0);
                searchMemberFailedList.Add(searchMember);

                AddAllSearchMembers(searchObj, objType, searchMember, 1);
                
                if (count++ > windowData.searchInspectorPerFrame.value)
                {
                    count = 0;
                    yield return null;
                    SetSearchInspectorPerFrame();
                }
                if (oldInspectorSearchLevel != inspectorSearchLevel) goto End;

                for (int j = 0; j < searchMemberList.Count; j++)
                {
                    searchMember = searchMemberList.items[j];

                    object obj = searchMember.obj;

                    if (skip.Contains(searchMember.member)) continue;

                    MemberInfo member = searchMember.member;

                    int level = searchMember.level;

                    if (level >= memberLookups.Count)
                    {
                        memberLookups.Add(new Dictionary<object, DrawInfo>(memberLookupsCapacity));
                    }

                    MemberType memberType = searchMember.memberType;

                    MemberData memberData = GetMemberData(searchMember.objType, member, memberType);

                    FieldInfo field = null;
                    PropertyInfo prop = null;
                    Type type;
                    string name;

                    if (searchMember.memberType == MemberType.Field)
                    {
                        field = memberData.field;
                    }
                    else if (searchMember.memberType == MemberType.Property)
                    {
                        prop = memberData.prop;
                    }

                    type = memberData.type;
                    name = memberData.name;

                    localTotalSearched++;
                    if (count++ > searchInspectorPerFrame)
                    {
                        count = 0;
                        yield return null;
                        SetSearchInspectorPerFrame();
                    }
                    if (oldInspectorSearchLevel != inspectorSearchLevel) goto End;

                    object value = null;
                    if (memberType == MemberType.Field)
                    {
                        try
                        {
                            value = field.GetValue(obj);
                        }
                        catch (Exception)
                        {
                            skip.Add(searchMember.member);
                        }
                    }
                    else if (memberType == MemberType.Property)
                    {
                        if (prop.IsDefined(typeof(ObsoleteAttribute), true)) continue;

                        if (prop.GetIndexParameters().Length == 0)
                        {
                            try
                            {
                                if (prop.GetGetMethod(true) != null)
                                {
                                    value = prop.GetValue(obj, null);
                                }
                            }
                            catch (Exception)
                            {
                                skip.Add(searchMember.member);
                                // Debug.LogException(e);
                            }
                        }
                    }

                    bool isClass = memberData.isClass;
                    bool isStruct = memberData.isStruct;
                    bool isArray = memberData.isArray;

                    bool hasChildren = (isClass || isStruct || isArray) && value != null && !value.Equals(null);

                    info = GetObjectDrawInfo(memberLookups.items[level], member);
                    searchMember.info = info;
                                        
                    if (!needInspectorSearch) goto End;
                    if (PassSearch(WindowType.Inspector, null, name, type))
                    {
                        searchMemberPassedList.Add(searchMember);
                    }
                    else searchMemberFailedList.Add(searchMember);

                    if (level >= inspectorSearchLevel) continue;
                    if (hasChildren && isClass && !searchDoubles.Add(value)) continue;

                    if (hasChildren)
                    {
                        Type valueType = value.GetType();
                        AddAllSearchMembers(value, valueType, searchMember, level + 1);
                    }
                }
            }

            // Debug.Log(searchMemberFailedList.Count + " " + searchMemberPassedList.Count);

            for (int i = 0; i < searchMemberFailedList.Count; i++)
            {
                searchMemberFailedList.items[i].info.passSearch = 0;
            }

            for (int i = 0; i < searchMemberPassedList.Count; i++)
            {
                SearchMember searchMember = searchMemberPassedList.items[i];

                searchMember.info.passSearch = 2;

                while (searchMember.parent != null)
                {
                    searchMember = (SearchMember)searchMember.parent;
                    if (searchMember.info.passSearch == 0) searchMember.info.passSearch = 1;
                }
                // R_Debug.Log(searchMemberPassedList.items[i].member.Name + " " + searchMemberPassedList.items[i].value.ToString() + " " + searchMemberPassedList.items[i].member.MemberType.ToString());
            }

            scrollViewCull.recalc = true;

            totalFound = searchMemberPassedList.Count;
            totalSearched = localTotalSearched;

            End:;
            isSearchingInspector = false;

            if (inspectorSearchLevel > 1) refreshInspectorSearch = 0.25f;
            else refreshInspectorSearch = 0;
        }

        int totalSearched;
        int totalFound;
    }
}