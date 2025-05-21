/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using InfinityCode.UltimateEditorEnhancer.Attributes;
using InfinityCode.UltimateEditorEnhancer.JSON;
using InfinityCode.UltimateEditorEnhancer.SceneTools;
using InfinityCode.UltimateEditorEnhancer.SceneTools.QuickAccessActions;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public partial class Prefs
    {
        public static bool quickAccessBar = true;
        public static bool quickAccessBarCloseViewGallery = true;
        public static float quickAccessBarIndentMin = 0;
        public static float quickAccessBarIndentMax = 0;

        public class QuickAccessBarManager : StandalonePrefManager<QuickAccessBarManager>
        {
            private const int HelpHeight = 40;
            private const int LineHeight = 20;
            private const int SelectWidth = 50;
            private static QuickAccessItem objectPickerTarget;
            private static QuickAccessItem objectPickerTextureTarget;
            private static QuickAccessItem windowPickerTarget;

            private GUIStyle contentStyle;
            private int objectPickerID;
            private ReorderableList reorderableList;
            private Vector2 scrollPosition;
            private bool waitWindowChanged;

            public static JsonArray json
            {
                get
                {
                    JsonArray jArr = new JsonArray();
                    for (int i = 0; i < ReferenceManager.quickAccessItems.Count; i++)
                    {
                        jArr.Add(ReferenceManager.quickAccessItems[i].json);
                    }

                    return jArr;
                }
                set
                {
                    if (ReferenceManager.quickAccessItems.Count > 0)
                    {
                        if (!EditorUtility.DisplayDialog("Import Quick Access Items", "Quick Access Bar already contain items", "Replace", "Ignore")) return;
                    }

                    List<QuickAccessItem> items = value.Deserialize<List<QuickAccessItem>>();

                    if (migrationReplace)
                    {
                        foreach (QuickAccessItem item in items)
                        {
                            if (item.settings == null) continue;

                            for (int i = 0; i < item.settings.Length; i++)
                            {
                                item.settings[i] = item.settings[i].Replace("InfinityCode.uContextPro", "InfinityCode.UltimateEditorEnhancer")
                                    .Replace("InfinityCode.uContext", "InfinityCode.UltimateEditorEnhancer")
                                    .Replace("uContext-Editor", "UltimateEditorEnhancer-Editor")
                                    .Replace("uContext-Pro-Editor", "UltimateEditorEnhancer-Editor");

                                item.iconSettings = item.iconSettings.Replace("\\", "/").Replace("Infinity Code/uContext Pro", "Infinity Code/Ultimate Editor Enhancer")
                                    .Replace("Infinity Code/uContext", "Infinity Code/Ultimate Editor Enhancer");
                            }
                        }
                    }

                    ReferenceManager.quickAccessItems.Clear();
                    ReferenceManager.quickAccessItems.AddRange(items);
                }
            }

            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Quick", "Access", "Bar"
                    };
                }
            }

            public override float order
            {
                get { return -40; }
            }

            private void AddItem(ReorderableList list)
            {
                GenericMenuEx menu = GenericMenuEx.Start();
                menu.Add("Add Window", () => AddItem(QuickAccessItemType.window));
                menu.Add("Add Static Method", () => AddItem(QuickAccessItemType.staticMethod));
                menu.Add("Add Menu Item", () => AddItem(QuickAccessItemType.menuItem));
                menu.Add("Add Scriptable Object", () => AddItem(QuickAccessItemType.scriptableObject));
                menu.Add("Add Settings", () => AddItem(QuickAccessItemType.settings));
                menu.Add("Add Action", () => AddItem(QuickAccessItemType.action));
                menu.Add("Add Space", () => AddItem(QuickAccessItemType.space));
                menu.Add("Add Flexible Space", () => AddItem(QuickAccessItemType.flexibleSpace));
                menu.Show();
            }

            private void AddItem(QuickAccessItemType type)
            {
                QuickAccessItem item = new QuickAccessItem(type);
                ReferenceManager.quickAccessItems.Add(item);
                if (type == QuickAccessItemType.scriptableObject)
                {
                    item.icon = QuickAccessItemIcon.texture;
                    item.iconSettings = Resources.iconsFolder + "ScriptableObject.png";
                }

                ReferenceManager.Save();
            }

            public override void Draw()
            {
                if (reorderableList == null)
                {
                    reorderableList = new ReorderableList(ReferenceManager.quickAccessItems, typeof(QuickAccessItem), true, true, true, true);
                    reorderableList.drawElementCallback += DrawItem;
                    reorderableList.drawHeaderCallback += DrawHeader;
                    reorderableList.elementHeightCallback += GetItemHeight;
                    reorderableList.onAddCallback += AddItem;
                    reorderableList.onRemoveCallback += RemoveItem;
                    reorderableList.onReorderCallback += Reorder;

                }

                ProcessEvents();

                float labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 80;

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                EditorGUI.BeginChangeCheck();
                quickAccessBar = EditorGUILayout.ToggleLeft("Quick Access Bar", quickAccessBar, EditorStyles.label);
                LocalSettings.collapseQuickAccessBar = EditorGUILayout.ToggleLeft("Collapse Bar", LocalSettings.collapseQuickAccessBar);
                if (EditorGUI.EndChangeCheck()) SceneView.RepaintAll();

                quickAccessBarCloseViewGallery = EditorGUILayout.ToggleLeft("Close View Gallery When Item Selected", quickAccessBarCloseViewGallery, EditorStyles.label);

                EditorGUILayout.BeginHorizontal();
                quickAccessBarIndentMin = EditorGUILayout.FloatField("Intend Min", quickAccessBarIndentMin);
                GUILayout.Space(10);
                quickAccessBarIndentMax = EditorGUILayout.FloatField("Max", quickAccessBarIndentMax);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Collapse All"))
                {
                    foreach (QuickAccessItem item in ReferenceManager.quickAccessItems) item.expanded = false;
                    ReorderableListRef.ClearCache(reorderableList);
                }

                if (GUILayout.Button("Expand All"))
                {
                    foreach (QuickAccessItem item in ReferenceManager.quickAccessItems) item.expanded = true;
                    ReorderableListRef.ClearCache(reorderableList);
                }

                if (GUILayout.Button("Restore Default Items"))
                {
                    ReferenceManager.quickAccessItems.Clear();
                    RecordUpgrader.InitDefaultQuickAccessItems();
                }

                EditorGUILayout.EndHorizontal();

                reorderableList.DoLayoutList();

                EditorGUILayout.EndScrollView();

                EditorGUIUtility.labelWidth = labelWidth;

                if (windowPickerTarget != null)
                {
                    EditorGUILayout.HelpBox("Set the focus on the window you want to select.", MessageType.Info);
                }
            }

            private static void DrawActionFields(QuickAccessItem item, ref Rect lineRect)
            {
                if (item.settings == null) item.settings = new string[1];
                else if (item.settings.Length != 1) Array.Resize(ref item.settings, 1);

                Rect r = new Rect(lineRect);
                r.width -= SelectWidth + 2;

                EditorGUI.LabelField(r, "Action", item.tooltip, EditorStyles.textField);

                r = new Rect(lineRect);
                r.xMin = r.xMax - SelectWidth;

                if (GUI.Button(r, "Select")) SelectActionItem(item);
            }

            private void DrawHeader(Rect rect)
            {
                GUI.Label(rect, "Items");
            }

            private void DrawIcon(QuickAccessItem item, ref Rect lineRect)
            {
                DrawIconPreview(item, lineRect);

                float labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 40;

                lineRect.xMin += 40;
                EditorGUI.BeginChangeCheck();
                item.icon = (QuickAccessItemIcon)EditorGUI.EnumPopup(lineRect, "Icon", item.icon);
                if (EditorGUI.EndChangeCheck())
                {
                    item.iconSettings = string.Empty;
                    item.ResetContent();
                }
                lineRect.y += LineHeight;

                if (item.icon == QuickAccessItemIcon.editorIconContent)
                {
                    Rect r = new Rect(lineRect);
                    r.width -= SelectWidth + 2;

                    item.iconSettings = EditorGUI.TextField(r, "ID", item.iconSettings);

                    r = new Rect(lineRect);
                    r.xMin = r.xMax - SelectWidth;

                    if (GUI.Button(r, "Select"))
                    {
                        EditorIconsBrowser browser = EditorIconsBrowser.OpenWindow();
                        browser.hint = "Click on the line to select ID.";
                        browser.OnSelect += s =>
                        {
                            item.iconSettings = s;
                            item.ResetContent();
                            ReferenceManager.Save();
                            settingsWindow.Repaint();
                        };
                    }
                }
                else if (item.icon == QuickAccessItemIcon.texture)
                {
                    Rect r = new Rect(lineRect);
                    r.width -= SelectWidth + 2;

                    item.iconSettings = EditorGUI.TextField(r, "Path", item.iconSettings);

                    r = new Rect(lineRect);
                    r.xMin = r.xMax - SelectWidth;

                    if (GUI.Button(r, "Select"))
                    {
                        objectPickerID = GUIUtility.GetControlID("Select_Button".GetHashCode(), FocusType.Keyboard) + 1000;
                        EditorGUIUtility.ShowObjectPicker<Texture>(null, false, "", objectPickerID);
                        objectPickerTextureTarget = item;
                    }

                    lineRect.y += LineHeight;
                    item.iconScale = EditorGUI.Slider(lineRect, "Scale", item.iconScale, 0, 1);
                }
                else if (item.icon == QuickAccessItemIcon.text)
                {
                    EditorGUIUtility.labelWidth = 110;
                    EditorGUI.BeginChangeCheck();
                    item.iconSettings = EditorGUI.TextField(lineRect, "Text (max 3 char)", item.iconSettings);
                    if (EditorGUI.EndChangeCheck()) item.ResetContent();
                }

                EditorGUIUtility.labelWidth = labelWidth;
            }

            private void DrawIconPreview(QuickAccessItem item, Rect lineRect)
            {
                Rect r = new Rect(lineRect.position.x, lineRect.position.y + 4, 32, 32);
                GUI.Box(r, GUIContent.none);
                GUIContent content = item.content;
                if (content != null)
                {
                    if (contentStyle == null)
                    {
                        contentStyle = new GUIStyle(Styles.centeredLabel);
                        contentStyle.fontSize = 8;
                    }

                    GUI.Box(r, content, contentStyle);
                }
            }

            private void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
            {
                if (index >= ReferenceManager.quickAccessItems.Count) return;
                EditorGUI.BeginChangeCheck();
                QuickAccessItem item = ReferenceManager.quickAccessItems[index];
                Rect lineRect = new Rect(rect)
                {
                    height = LineHeight - 2
                };

                if (item.type != QuickAccessItemType.flexibleSpace)
                {
                    string label = item.expanded || string.IsNullOrEmpty(item.tooltip) ? item.typeName : item.tooltip + " (" + item.typeName + ")";
                    item.expanded = EditorGUI.Foldout(lineRect, item.expanded, label);
                }
                else EditorGUI.LabelField(lineRect, item.typeName);
                lineRect.y += LineHeight;

                if (!item.expanded)
                {
                    if (EditorGUI.EndChangeCheck()) ReferenceManager.Save();
                    return;
                }

                DrawTypeFields(item, ref lineRect);
                lineRect.y += LineHeight;

                if (item.isButton)
                {
                    item.visibleRules = (SceneViewVisibleRules)EditorGUI.EnumPopup(lineRect, "Visible", item.visibleRules);
                    lineRect.y += LineHeight;

                    if (item.canHaveIcon)
                    {
                        EditorGUI.BeginChangeCheck();
                        item.tooltip = EditorGUI.TextField(lineRect, "Tooltip", item.tooltip);
                        if (EditorGUI.EndChangeCheck()) item.ResetContent();
                        lineRect.y += LineHeight;

                        DrawIcon(item, ref lineRect);
                    }
                }

                if (EditorGUI.EndChangeCheck()) ReferenceManager.Save();
            }

            private static void DrawMenuFields(QuickAccessItem item, ref Rect lineRect)
            {
                if (item.settings == null) item.settings = new string[1];
                else if (item.settings.Length != 1) Array.Resize(ref item.settings, 1);

                Rect r = new Rect(lineRect);
                r.width -= SelectWidth + 2;

                item.settings[0] = EditorGUI.TextField(r, "Item", item.settings[0]);

                r = new Rect(lineRect);
                r.xMin = r.xMax - SelectWidth;

                if (GUI.Button(r, "Select")) SelectMenuItem(item);
            }

            private void DrawMethodFields(QuickAccessItem item, ref Rect lineRect)
            {
                if (item.settings == null) item.settings = new string[2];
                else if (item.settings.Length != 2) Array.Resize(ref item.settings, 2);

                Rect r = new Rect(lineRect);
                r.width -= SelectWidth + 2;
                item.settings[0] = EditorGUI.TextField(r, "Class", item.settings[0]);
                r = new Rect(lineRect);
                r.xMin = r.xMax - SelectWidth;

                if (GUI.Button(r, "Select"))
                {
                    objectPickerID = GUIUtility.GetControlID("Select_Button".GetHashCode(), FocusType.Passive) + 1000;
                    EditorGUIUtility.ShowObjectPicker<MonoScript>(null, false, "", objectPickerID);
                    objectPickerTarget = item;
                }

                lineRect.y += LineHeight;
                r = new Rect(lineRect);
                r.width -= SelectWidth + 2;
                item.settings[1] = EditorGUI.TextField(r, "Method", item.settings[1]);
                Type methodType = item.methodType;
                EditorGUI.BeginDisabledGroup(methodType == null);
                r = new Rect(lineRect);
                r.xMin = r.xMax - SelectWidth;
                if (GUI.Button(r, "Select") && methodType != null)
                {
                    int count = 0;
                    GenericMenuEx menu = GenericMenuEx.Start();

                    MethodInfo[] methods = methodType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                    foreach (MethodInfo method in methods)
                    {
                        if (method.GetParameters().Length == 0 && !method.IsSpecialName)
                        {
                            string methodName = method.Name;
                            menu.Add(methodName, () =>
                            {
                                item.settings[1] = methodName;
                                item.tooltip = methodName;
                                item.icon = QuickAccessItemIcon.text;
                                item.iconSettings = GameObjectUtils.GetPsIconLabel(methodName, 3);
                                ReferenceManager.Save();
                            });
                            count++;
                        }
                    }
                    if (count > 0) menu.Show();
                }
                EditorGUI.EndDisabledGroup();
            }

            private void DrawScriptableObjectFields(QuickAccessItem item, ref Rect lineRect)
            {
                if (item.settings == null) item.settings = new string[1];
                else if (item.settings.Length != 1) Array.Resize(ref item.settings, 1);

                EditorGUI.BeginChangeCheck();
                try
                {
                    item.scriptableObject = EditorGUI.ObjectField(lineRect, "Object", item.scriptableObject, typeof(ScriptableObject), false) as ScriptableObject;
                }
                catch
                {

                }
                if (EditorGUI.EndChangeCheck())
                {
                    if (item.scriptableObject != null)
                    {
                        item.tooltip = item.scriptableObject.name;
                        item.icon = QuickAccessItemIcon.text;
                        item.iconSettings = GameObjectUtils.GetPsIconLabel(item.tooltip, 3);
                        item.ResetContent();
                    }
                }
            }

            private void DrawSettingsFields(QuickAccessItem item, ref Rect lineRect)
            {
                if (item.settings == null) item.settings = new string[1];
                else if (item.settings.Length != 1) Array.Resize(ref item.settings, 1);

                Rect r = new Rect(lineRect);
                r.width -= SelectWidth + 2;
                item.settings[0] = EditorGUI.TextField(r, "Path", item.settings[0]);

                r = new Rect(lineRect);
                r.xMin = r.xMax - SelectWidth;

                if (GUI.Button(r, "Select"))
                {
                    IEnumerable providers = Reflection.InvokeStaticMethod(typeof(SettingsService), "FetchSettingsProviders") as IEnumerable;
                    if (providers != null)
                    {
                        List<string> paths = new List<string>();

                        GenericMenuEx menu = GenericMenuEx.Start();
                        foreach (object p in providers)
                        {
                            SettingsProvider sp = p as SettingsProvider;
                            string path = sp.settingsPath;
                            paths.Add(path);
                        }

                        paths.Sort();

                        foreach (string path in paths)
                        {
                            string p = path;
                            menu.Add(p, () =>
                            {
                                item.settings[0] = p;
                                item.tooltip = p.Split('/').Last();
                                item.icon = QuickAccessItemIcon.text;
                                item.iconSettings = GameObjectUtils.GetPsIconLabel(item.tooltip, 3);
                                item.ResetContent();
                                GUI.changed = true;
                            });
                        }

                        menu.Show();
                    }
                }
            }

            private void DrawSpaceFields(QuickAccessItem item, ref Rect lineRect)
            {
                if (item.intSettings == null || item.intSettings.Length == 0)
                {
                    item.intSettings = new[]
                    {
                        10
                    };
                }

                item.intSettings[0] = EditorGUI.IntField(lineRect, "Size", item.intSettings[0]);
                if (item.intSettings[0] < 0) item.intSettings[0] = 0;
                lineRect.y += LineHeight;
            }

            private void DrawTypeFields(QuickAccessItem item, ref Rect lineRect)
            {
                if (item.type == QuickAccessItemType.window) DrawWindowFields(item, ref lineRect);
                else if (item.type == QuickAccessItemType.staticMethod) DrawMethodFields(item, ref lineRect);
                else if (item.type == QuickAccessItemType.menuItem) DrawMenuFields(item, ref lineRect);
                else if (item.type == QuickAccessItemType.action) DrawActionFields(item, ref lineRect);
                else if (item.type == QuickAccessItemType.space) DrawSpaceFields(item, ref lineRect);
                else if (item.type == QuickAccessItemType.settings) DrawSettingsFields(item, ref lineRect);
                else if (item.type == QuickAccessItemType.scriptableObject) DrawScriptableObjectFields(item, ref lineRect);
            }

            private void DrawWindowFields(QuickAccessItem item, ref Rect lineRect)
            {
                if (item.settings == null) item.settings = new string[1];
                else if (item.settings.Length != 1) Array.Resize(ref item.settings, 1);

                Rect r = new Rect(lineRect);
                r.width -= SelectWidth + 2;
                item.settings[0] = EditorGUI.TextField(r, "Class", item.settings[0]);
                r = new Rect(lineRect);
                r.xMin = r.xMax - SelectWidth;
                if (windowPickerTarget == item)
                {
                    if (GUI.Button(r, "Stop"))
                    {
                        windowPickerTarget = null;
                        EditorApplication.update -= WaitWindowChanged;
                    }
                }
                else
                {
                    if (GUI.Button(r, "Pick"))
                    {
                        windowPickerTarget = item;
                        EditorApplication.update -= WaitWindowChanged;
                        EditorApplication.update += WaitWindowChanged;
                    }
                }

                lineRect.y += LineHeight;
                item.windowMode = (QuickAccessWindowMode) EditorGUI.EnumPopup(lineRect, "Mode", item.windowMode);

                if (item.windowMode != QuickAccessWindowMode.popup)
                {
                    lineRect.y += LineHeight;
                    EditorGUIUtility.labelWidth += 50;
                    item.alignWindowToBar = EditorGUI.Toggle(lineRect, "Align To Bar", item.alignWindowToBar);
                    EditorGUIUtility.labelWidth -= 50;
                }
                else item.alignWindowToBar = true;

                if (item.alignWindowToBar)
                {
                    lineRect.y += LineHeight;

                    EditorGUIUtility.labelWidth += 50;
                    item.useCustomWindowSize = EditorGUI.Toggle(lineRect, "Custom Window Size", item.useCustomWindowSize);
                    EditorGUIUtility.labelWidth -= 50;

                    if (item.useCustomWindowSize)
                    {
                        lineRect.y += LineHeight;
                        EditorGUI.BeginChangeCheck();
                        item.customWindowSize = EditorGUI.Vector2Field(lineRect, "Window Size", item.customWindowSize);
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (item.customWindowSize.x < 50) item.customWindowSize.x = 50;
                            if (item.customWindowSize.y < 50) item.customWindowSize.y = 50;
                        }
                    }
                }
            }

            private float GetItemHeight(int index)
            {
                QuickAccessItem item = ReferenceManager.quickAccessItems[index];
                int rows = 6;
                bool addTextureRow = item.icon == QuickAccessItemIcon.texture && item.expanded;
                if (item.type == QuickAccessItemType.flexibleSpace || !item.expanded) rows = 1;
                else if (item.type == QuickAccessItemType.staticMethod) rows++;
                else if (item.type == QuickAccessItemType.action)
                {
                    rows = 3;
                    addTextureRow = false;
                }
                else if (item.type == QuickAccessItemType.space) rows = 2;
                else if (item.type == QuickAccessItemType.window)
                {
                    rows++;
                    if (item.windowMode != QuickAccessWindowMode.popup) rows++;
                    if (item.alignWindowToBar)
                    {
                        rows++;
                        if (item.useCustomWindowSize) rows++;
                    }
                }

                if (addTextureRow) rows++;

                int height = rows * LineHeight;

                return height;
            }

            private void ProcessEvents()
            {
                ProcessCommands();
            }

            private void ProcessCommands()
            {
                Event e = Event.current;
                if (e.type != EventType.ExecuteCommand) return;

                if (e.commandName == "ObjectSelectorUpdated")
                {
                    if (objectPickerTarget != null)
                    {
                        MonoScript script = EditorGUIUtility.GetObjectPickerObject() as MonoScript;
                        if (script != null)
                        {
                            objectPickerTarget.methodType = script.GetClass();
                            ReferenceManager.Save();
                            settingsWindow.Repaint();
                        }
                    }
                    else if (objectPickerTextureTarget != null)
                    {
                        Texture texture = EditorGUIUtility.GetObjectPickerObject() as Texture;
                        if (texture != null)
                        {
                            objectPickerTextureTarget.iconSettings = AssetDatabase.GetAssetPath(texture);
                            objectPickerTextureTarget.ResetContent();
                            ReferenceManager.Save();
                            settingsWindow.Repaint();
                        }
                    }
                }
                else if (e.commandName == "ObjectSelectorClosed")
                {
                    objectPickerTarget = null;
                    objectPickerTextureTarget = null;
                }
            }

            private void RemoveItem(ReorderableList list)
            {
                ReferenceManager.quickAccessItems.RemoveAt(list.index);
                ReferenceManager.Save();
            }

            private void Reorder(ReorderableList list)
            {
                ReferenceManager.Save();
            }

            private static void SelectActionItem(QuickAccessItem item)
            {
                TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<QuickAccessAction>();
                GenericMenuEx menu = GenericMenuEx.Start();
                foreach (Type type in types)
                {
                    Type t = type;
                    TitleAttribute titleAttribute = t.GetCustomAttribute<TitleAttribute>();
                    string label = titleAttribute != null ? titleAttribute.displayName : ObjectNames.NicifyVariableName(t.Name);
                    menu.Add(label, () =>
                    {
                        item.tooltip = label;
                        item.settings[0] = t.FullName;
                        item.ResetContent();
                        GUI.changed = true;
                    });
                }
                menu.Show();
            }

            private static void SelectMenuItem(QuickAccessItem item)
            {
                GenericMenuEx menu = GenericMenuEx.Start();

                string[] menuToString = EditorGUIUtility.SerializeMainMenuToString().Split('\n');
                string[] groups = new string[64];
                int prevLevel = -1;

                StringBuilder builder = StaticStringBuilder.Start();

                for (int i = 0; i < menuToString.Length; i++)
                {
                    string s = menuToString[i];
                    string s2 = s.Trim();
                    if (string.IsNullOrEmpty(s2)) continue;

                    int lDiff = s.Length - s2.Length;
                    int level = lDiff / 4;

                    if (prevLevel >= level)
                    {
                        string menuItem = builder.ToString();
                        string tooltip = groups[prevLevel];
                        menu.Add(menuItem, () =>
                        {
                            item.settings[0] = menuItem;
                            item.tooltip = tooltip;
                            item.icon = QuickAccessItemIcon.text;
                            item.iconSettings = GameObjectUtils.GetPsIconLabel(tooltip, 3);
                            item.ResetContent();
                            GUI.changed = true;
                        });
                    }

                    prevLevel = level;
                    groups[level] = s2;

                    if (groups[0] == "CONTEXT") continue;

                    builder.Clear();

                    for (int j = 0; j <= level; j++)
                    {
                        if (j > 0) builder.Append("/");
                        builder.Append(groups[j]);
                    }
                }

                menu.Show();
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                quickAccessBar = state;
            }

            private void WaitWindowChanged()
            {
                EditorWindow wnd = EditorWindow.focusedWindow;

                if (wnd == null) return;
                if (wnd.GetType() == ProjectSettingsWindowRef.type) return;

                EditorApplication.update -= WaitWindowChanged;
                windowPickerTarget.settings[0] = wnd.GetType().AssemblyQualifiedName;
                windowPickerTarget.tooltip = wnd.titleContent.text;
                windowPickerTarget.customWindowSize = wnd.position.size;
                if (wnd.titleContent.image != null)
                {
                    windowPickerTarget.icon = QuickAccessItemIcon.editorIconContent;
                    windowPickerTarget.iconSettings = wnd.titleContent.image.name;
                }
                else
                {
                    windowPickerTarget.icon = QuickAccessItemIcon.text;
                    windowPickerTarget.iconSettings = GameObjectUtils.GetPsIconLabel(windowPickerTarget.tooltip, 3);
                }
                windowPickerTarget.ResetContent();
                ReferenceManager.Save();

                windowPickerTarget = null;
                settingsWindow.Repaint();
            }
        }
    }
}