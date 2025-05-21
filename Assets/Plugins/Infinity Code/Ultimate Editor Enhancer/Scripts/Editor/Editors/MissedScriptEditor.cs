/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Editors
{
    [CustomEditor(typeof(MonoBehaviour), isFallback = true)]
    public class MissedScriptEditor : Editor
    {
        private static BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        private static MonoScript[] _scripts;

        [NonSerialized]
        private List<ReplaceCandidate> candidates;

        [NonSerialized]
        private ReplaceCandidate bestCandidate;

        [NonSerialized]
        private MonoScript newScript;

        public static MonoScript[] scripts
        {
            get
            {
                if (_scripts == null) _scripts = MonoImporter.GetAllRuntimeMonoScripts();
                return _scripts;
            }
        }

        private static IEnumerable<FieldInfo> GetFields(Type type)
        {
            if (type == null) return new FieldInfo[0];
            return type.GetFields(flags).Concat(GetFields(type.BaseType)).Where(f => !f.IsDefined(typeof(HideInInspector), true));
        }

        private void InitCandidates()
        {
            candidates = new List<ReplaceCandidate>();

            foreach (MonoScript script in scripts)
            {
                Type type = script.GetClass();
                if (type == null) continue;
                if (!type.IsSubclassOf(typeof(Component))) continue;
                
                FieldInfo[] fields = GetFields(type).ToArray();
                int match = 0;
                int total = 0;

                SerializedProperty iterator = serializedObject.GetIterator();
                iterator.Next(true);
                while (iterator.NextVisible(false))
                {
                    string propName = iterator.name;
                    if (propName == "m_Script") continue;

                    total++;
                    FieldInfo field = null;

                    for (int j = 0; j < fields.Length; j++)
                    {
                        if (fields[j].Name == propName)
                        {
                            field = fields[j];
                            break;
                        }
                    }

                    if (field == null) continue;

                    switch (iterator.propertyType)
                    {
                        case SerializedPropertyType.ArraySize:
                            if (field.FieldType.HasElementType) match += 1;
                            else if (typeof(IEnumerable).IsAssignableFrom(field.FieldType)) match += 1;
                            break;
                        case SerializedPropertyType.AnimationCurve:
                            if (field.FieldType == typeof(AnimationCurve)) match += 1;
                            break;
                        case SerializedPropertyType.Boolean:
                            if (field.FieldType == typeof(bool)) match += 1;
                            break;
                        case SerializedPropertyType.Bounds:
                            if (field.FieldType == typeof(Bounds)) match += 1;
                            break;
                        case SerializedPropertyType.Color:
                            if (field.FieldType == typeof(Color32) || field.FieldType == typeof(Color)) match += 1;
                            break;
                        case SerializedPropertyType.Enum:
                            if (field.FieldType.IsEnum) match += 1;
                            break;
                        case SerializedPropertyType.Float:
                            if (typeof(float).IsAssignableFrom(field.FieldType)) match += 1;
                            break;
                        case SerializedPropertyType.Integer:
                            if (typeof(int).IsAssignableFrom(field.FieldType) || typeof(uint).IsAssignableFrom(field.FieldType) || field.FieldType.IsEnum) match += 1;
                            break;
                        case SerializedPropertyType.ObjectReference:
                            if (!field.FieldType.IsValueType) match += 1;
                            break;
                        case SerializedPropertyType.Rect:
                            if (field.FieldType == typeof(Rect)) match += 1;
                            break;
                        case SerializedPropertyType.String:
                            if (field.FieldType == typeof(string)) match += 1;
                            break;
                        case SerializedPropertyType.Vector2:
                            if (field.FieldType == typeof(Vector2)) match += 1;
                            break;
                        case SerializedPropertyType.Vector3:
                            if (field.FieldType == typeof(Vector3)) match += 1;
                            break;
                        default:
                            match += 1;
                            break;
                    }
                }

                if (match > 0)
                {
                    candidates.Add(new ReplaceCandidate(script, 100f * match / total));
                }
            }

            if (candidates.Count > 0)
            {
                candidates = candidates.OrderByDescending(c => c.similarity).ToList();
                bestCandidate = candidates[0];
            }
        }

        public override void OnInspectorGUI()
        {
            if (target != null)
            {
                base.OnInspectorGUI();
                return;
            }

            if (candidates == null)
            {
                InitCandidates();
            }

            GUI.enabled = false;

            SerializedProperty iterator = serializedObject.GetIterator();
            iterator.Next(true);
            while (iterator.NextVisible(false))
            {
                try
                {
                    EditorGUILayout.PropertyField(iterator);
                }
                catch
                {
                }
            }
            GUI.enabled = true;

            EditorGUILayout.BeginHorizontal();
            newScript = EditorGUILayout.ObjectField("Replace To", newScript, typeof(MonoScript), false) as MonoScript;
            if (GUILayout.Button("Set", GUILayout.ExpandWidth(false)))
            {
                SetScript(newScript);
                EditorUtility.SetDirty(target);
            }
            EditorGUILayout.EndHorizontal();

            if (bestCandidate != null)
            {
                EditorGUILayout.HelpBox("Ultimate Editor Enhancer has determined that the best replacement candidate for this missing script is " + bestCandidate.script.name + ". Replace it?", MessageType.Warning);
                if (GUILayout.Button("Use " + bestCandidate.script.name))
                {
                    SetScript(bestCandidate.script);
                }

                if (GUILayout.Button("Use Other"))
                {
                    GenericMenu menu = new GenericMenu();

                    for (int i = 0; i < Mathf.Min(candidates.Count, 10); i++)
                    {
                        ReplaceCandidate candidate = candidates[i];
                        menu.AddItem(new GUIContent(candidate.script.name + " [" + candidate.similarity + "%]"), false, () => SetScript(candidate.script));
                    }

                    menu.ShowAsContext();
                }

                if (GUILayout.Button("Remove Component"))
                {
                    GameObject go = (target as Component).gameObject;
                    Undo.DestroyObjectImmediate(target);
                    EditorUtility.SetDirty(go);
                }
            }
            else
            {
                if (GUILayout.Button("Remove Component"))
                {
                    GameObject go = (target as Component).gameObject;
                    Undo.DestroyObjectImmediate(target);
                    EditorUtility.SetDirty(go);
                }
            }
        }

        private void SetScript(MonoScript script)
        {
            SerializedProperty prop = serializedObject.FindProperty("m_Script");
            prop.objectReferenceValue = script;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }

        private class ReplaceCandidate
        {
            public MonoScript script;
            public float similarity;

            public ReplaceCandidate(MonoScript script, float similarity)
            {
                this.script = script;
                this.similarity = similarity;
            }
        }
    }
}