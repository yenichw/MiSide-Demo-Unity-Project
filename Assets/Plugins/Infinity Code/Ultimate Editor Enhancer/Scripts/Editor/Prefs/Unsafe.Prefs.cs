/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.IO;
using InfinityCode.UltimateEditorEnhancer.Interceptors;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool _changeNumberFieldValueByArrow = true;
        public static bool _changeNumberFieldValueByMouseWheel = true;
        public static bool _expandLongTextFields = true;
        public static bool _hierarchyTypeFilter = true;
        public static bool _improveCurveEditor = true;
        public static bool _searchInEnumFields = true;
        public static bool _treeControllerCollapse = true;
        public static bool _unsafeFeatures = true;
        public static bool longTextFieldsInVisualScripting = false;
        public static int searchInEnumFieldsMinValues = 10;

#if !UNITY_EDITOR_OSX
        public static EventModifiers changeNumberFieldValueByWheelModifiers = EventModifiers.Control;
#else
        public static EventModifiers changeNumberFieldValueByWheelModifiers = EventModifiers.Command;
#endif
        
        private static int hasUnsafeBlock = -1; // -1 - unknown, 0 - no block, 1 - has block

        public static bool changeNumberFieldValueByArrow
        {
            get => _changeNumberFieldValueByArrow && unsafeFeatures;
        }
        
        public static bool changeNumberFieldValueByMouseWheel
        {
            get => _changeNumberFieldValueByMouseWheel && unsafeFeatures;
        }

        public static bool expandLongTextFields
        {
            get => _expandLongTextFields && unsafeFeatures;
        }

        public static bool hierarchyTypeFilter
        {
            get => _hierarchyTypeFilter && unsafeFeatures;
        }

        public static bool improveCurveEditor
        {
            get => _improveCurveEditor && unsafeFeatures;
        }

        public static bool searchInEnumFields
        {
            get => _searchInEnumFields && unsafeFeatures;
        }
        
        public static bool treeControllerCollapse
        {
            get => _treeControllerCollapse && unsafeFeatures;
        }

        public static bool unsafeFeatures
        {
            get
            {
                if (hasUnsafeBlock == -1)
                {
                    hasUnsafeBlock = File.Exists("UEENoUnsafe.txt") ? 1 : 0;
                }
                return _unsafeFeatures && hasUnsafeBlock == 0;
            }
        }

        public class UnsafeManager: StandalonePrefManager<UnsafeManager>, IStateablePref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Unsafe",
                        "Change Number Fields Value By Arrows",
                        "Hierarchy Type Filter",
                        "Improve Curve Editor",
                        "Search In Enum Fields",
                        "Tree Controller Collapse"
                    };
                }
            }

            public override void Draw()
            {
                EditorGUI.BeginChangeCheck();

                _unsafeFeatures = EditorGUILayout.ToggleLeft("Unsafe Features", _unsafeFeatures);

                if (EditorGUI.EndChangeCheck()) RefreshFeatures();

                EditorGUI.BeginDisabledGroup(!_unsafeFeatures);

                DrawToggleField("Change Number Fields Value By Arrows", ref _changeNumberFieldValueByArrow, NumberFieldInterceptor.Refresh);
                DrawToggleField("Change Number Fields Value By Mouse Wheel", ref _changeNumberFieldValueByMouseWheel, NumberFieldInterceptor.Refresh);
                EditorGUI.BeginDisabledGroup(!_changeNumberFieldValueByMouseWheel);
                changeNumberFieldValueByWheelModifiers = (EventModifiers)EditorGUILayout.EnumFlagsField("Modifiers", changeNumberFieldValueByWheelModifiers);
                EditorGUI.EndDisabledGroup();
                
                _expandLongTextFields = EditorGUILayout.ToggleLeft("Expand Long Text Fields", _expandLongTextFields);
                EditorGUI.indentLevel++;
                longTextFieldsInVisualScripting = EditorGUILayout.ToggleLeft("Long Text Fields In Visual Scripting", longTextFieldsInVisualScripting);
                EditorGUI.indentLevel--;


                DrawToggleField("Hierarchy Type Filter", ref _hierarchyTypeFilter, HierarchyToolbarInterceptor.Refresh);
                _improveCurveEditor = EditorGUILayout.ToggleLeft("Improve Curve Editor", _improveCurveEditor);
                DrawToggleField("Search In Enum Fields", ref _searchInEnumFields, EnumPopupInterceptor.Refresh);

                if (_searchInEnumFields)
                {
                    EditorGUI.indentLevel++;
                    searchInEnumFieldsMinValues = EditorGUILayout.IntField("Min Values", searchInEnumFieldsMinValues);
                    EditorGUI.indentLevel--;
                }

                EditorGUI.EndDisabledGroup();
                
                DrawToggleField("Tree Controller Collapse", ref _treeControllerCollapse, TreeViewControllerUserInputChangedExpandedState.Refresh);
            }

            public string GetMenuName()
            {
                return "Unsafe";
            }

            private static void RefreshFeatures()
            {
                EnumPopupInterceptor.Refresh();
                HierarchyToolbarInterceptor.Refresh();
                NumberFieldInterceptor.Refresh();
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                _unsafeFeatures = state;
                _changeNumberFieldValueByArrow = state;
                _changeNumberFieldValueByMouseWheel = state;
                _expandLongTextFields = state;
                _hierarchyTypeFilter = state;
                _improveCurveEditor = state;
                _searchInEnumFields = state;
                _treeControllerCollapse = state;
                
                RefreshFeatures();
            }
        }
    }
}