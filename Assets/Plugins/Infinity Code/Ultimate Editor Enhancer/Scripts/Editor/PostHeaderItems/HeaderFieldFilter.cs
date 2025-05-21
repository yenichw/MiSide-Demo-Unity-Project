/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.PostHeader
{
    public class HeaderFieldFilter : PostHeaderItem
    {
        private const string SearchBoxName = "UEEFieldFilter";
        
        private bool active = false;
        private string searchText;
        private VisualElement filterContent;
        private float contentHeight;
        private Component[] components;
        private List<SerializedProperty> properties = new List<SerializedProperty>();
        private bool focusOnTextField = false;

        public override void OnBlockGUI(Object target)
        {
            if (!active) return;
            
            GameObject go = target as GameObject;
            if (go == null) return;

            EditorGUI.BeginChangeCheck();
            GUI.SetNextControlName(SearchBoxName);
            searchText = GUILayoutUtils.ToolbarSearchField(searchText);
            
            if (focusOnTextField && Event.current.type == EventType.Repaint)
            {
                GUI.FocusControl(SearchBoxName);
                focusOnTextField = false;
            }
            
            if (!EditorGUI.EndChangeCheck()) return;
            UpdateFilter();
        }

        private void OnContentGUI()
        {
            bool wideMode = EditorGUIUtility.wideMode;
            EditorGUIUtility.wideMode = true;
            
            SerializedObject so = null;
            
            foreach (SerializedProperty prop in properties)
            {
                if (so != prop.serializedObject)
                {
                    if (so != null) so.ApplyModifiedProperties();
                    
                    so = prop.serializedObject;
                    so.Update();
                    EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(so.targetObject.GetType().Name), EditorStyles.toolbarButton);
                }
                
                EditorGUILayout.PropertyField(prop, true);
            }
            
            if (so != null) so.ApplyModifiedProperties();
            
            EditorGUIUtility.wideMode = wideMode;

            UpdateContentHeight();
        }

        public override void OnRowGUI(Object target)
        {
            GameObject go = target as GameObject;
            if (go == null) return;
            
            EditorGUI.BeginChangeCheck();
            active = Toggle(TempContent.Get(Icons.search, "Search By Serialized Fields"), active);
            if (!EditorGUI.EndChangeCheck()) return;
            
            if (active) StartFilter(go);
            else StopFilter();
        }

        private void OnSelectionChanged()
        {
            active = false;
            searchText = null;
            components = null;
            properties.Clear();
        }

        private void StartFilter(GameObject target)
        {
            EditorWindow wnd = EditorWindow.focusedWindow;
            if (wnd.GetType() != InspectorWindowRef.type) return;
            
            VisualElement mainContainer = VisualElementHelper.GetMainContainer(wnd);
            if (mainContainer == null) return;
            if (mainContainer.childCount < 2) return;
            VisualElement contentContainer = VisualElementHelper.GetVisualElementByClass(mainContainer, "unity-inspector-editors-list");
            if (contentContainer == null) return;
            
            focusOnTextField = true;

            filterContent = new IMGUIContainer(OnContentGUI);
            filterContent.name = "uee-filter-content";
            
            contentContainer.Add(filterContent);
            components = target.GetComponents<Component>();

            Selection.selectionChanged += OnSelectionChanged;
            
            UpdateFilter();
        }

        private void StopFilter()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            if (filterContent != null)
            {
                for (int i = 0; i < filterContent.parent.childCount; i++)
                {
                    VisualElement el = filterContent.parent[i];
                    el.style.display = DisplayStyle.Flex;
                }
                
                filterContent.parent.Remove(filterContent);
                filterContent = null;
            }

            components = null;
            properties.Clear();
        }

        private void UpdateContentHeight()
        {
            float b = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(0)).yMin;
            Event e = Event.current;
            if (e.type == EventType.Repaint)
            {
                float bottom = b + 5;

                if (Mathf.Abs(bottom - contentHeight) > 1)
                {
                    contentHeight = bottom;
                    filterContent.style.height = contentHeight;
                }
            }
        }

        private void UpdateFilter()
        {
            if (string.IsNullOrEmpty(searchText))
            {
                for (int i = 1; i < filterContent.parent.childCount; i++)
                {
                    VisualElement el = filterContent.parent[i];
                    el.style.display = el.name == "uee-filter-content"? DisplayStyle.None: DisplayStyle.Flex;
                }
                return;
            }

            for (int i = 1; i < filterContent.parent.childCount; i++)
            {
                VisualElement el = filterContent.parent[i];
                el.style.display = el.name == "uee-filter-content" ? DisplayStyle.Flex : DisplayStyle.None;
            }
            
            properties.Clear();
            
            string pattern = SearchableItem.GetPattern(searchText);

            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component == null) continue;
                SerializedObject so = new SerializedObject(component);
                SerializedProperty sp = so.GetIterator();

                if (!sp.NextVisible(true)) continue;
                
                do
                {
                    if (SearchableItem.Match(pattern, sp.displayName))
                    {
                        properties.Add(sp.Copy());
                    }
                } while (sp.NextVisible(false));
            }
        }
    }
}