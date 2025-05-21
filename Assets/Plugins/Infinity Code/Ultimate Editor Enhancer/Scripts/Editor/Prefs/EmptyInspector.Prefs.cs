/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using InfinityCode.UltimateEditorEnhancer.InspectorTools;
using InfinityCode.UltimateEditorEnhancer.JSON;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool emptyInspector = true;
        public static bool emptyInspectorShowUpdates = true;

        public class EmptyInspectorManager : StandalonePrefManager<EmptyInspectorManager>
        {
            private SerializedObject serializedObject;
            private SerializedProperty elementsProperty;

            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Empty Inspector",
                    };
                }
            }

            public static JsonArray json
            {
                get
                {
                    JsonArray jArr = new JsonArray();
                    for (int i = 0; i < ReferenceManager.emptyInspectorItems.Count; i++)
                    {
                        jArr.Add(ReferenceManager.emptyInspectorItems[i].json);
                    }

                    return jArr;
                }
                set
                {
                    if (ReferenceManager.emptyInspectorItems.Count > 0)
                    {
                        if (!EditorUtility.DisplayDialog("Import Empty Inspector", "Empty Inspector already contain items", "Replace", "Ignore")) return;
                    }

                    List<EmptyInspector.Group> items = value.Deserialize<List<EmptyInspector.Group>>();
                    ReferenceManager.emptyInspectorItems.Clear();
                    ReferenceManager.emptyInspectorItems.AddRange(items);
                }
            }

            public override void Draw()
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                if (serializedObject == null)
                {
                    serializedObject = new SerializedObject(ReferenceManager.instance);
                    if (serializedObject != null)
                    {
                        elementsProperty = serializedObject.FindProperty("_emptyInspectorItems");
                        if (elementsProperty != null) elementsProperty.isExpanded = true; 
                    }
                }

                emptyInspector = EditorGUILayout.ToggleLeft("Empty Inspector", emptyInspector);
                emptyInspectorShowUpdates = EditorGUILayout.ToggleLeft("Show Updates", emptyInspectorShowUpdates);
                
                if (GUILayout.Button("Reimport Items From Window Menu", GUILayout.ExpandWidth(false)))
                {
                    RecordUpgrader.InitDefaultEmptyInspectorItems();
                    EmptyInspector.ResetCachedItems();
                    ReferenceManager.Save();
                }

                if (elementsProperty != null)
                {
                    serializedObject.Update();

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(elementsProperty);
                    bool isDirty = EditorGUI.EndChangeCheck();
                    
                    serializedObject.ApplyModifiedProperties();

                    if (isDirty) EmptyInspector.ResetCachedItems();
                }


                EditorGUILayout.EndScrollView();
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                emptyInspector = state;
            }
        }
    }
}