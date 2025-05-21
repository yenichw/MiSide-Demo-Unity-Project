/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using InfinityCode.UltimateEditorEnhancer.ProjectTools;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool projectTools = true;
        public static bool projectCreateCustomEditor = true;
        public static bool projectCreateFolder = true;
        public static bool projectCreateFolderByShortcut = true;
        public static bool projectCreateMaterial = true;
        public static bool projectCreateScript = true;
        public static bool projectCreateShader = true;
        public static bool projectFileExtension = true;
        public static bool projectOddEven = true;
        public static bool projectPlayAudio = true;

        public class ProjectManager : StandalonePrefManager<ProjectManager>, IHasShortcutPref, IStateablePref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Create Custom Editor", 
                        "Create Folder By Shortcut",
                        "Create Material Button",
                        "Create Script Button",
                        "Create Shader Button",
                        "File Extension",
                        "Odd Even",
                        "Play Audio Button",
                    };
                }
            }

            public override void Draw()
            {
                EditorGUI.BeginChangeCheck();
                projectTools = EditorGUILayout.ToggleLeft("Project Tools", projectTools);
                if (EditorGUI.EndChangeCheck())
                {
                    if (projectTools) ProjectItemDrawer.Enable();
                    else ProjectItemDrawer.Disable();
                }
                
                EditorGUI.BeginDisabledGroup(!projectTools);
                
                projectCreateCustomEditor = EditorGUILayout.ToggleLeft("Create Custom Editor For MonoBehaviour", projectCreateCustomEditor);
                projectCreateFolder = EditorGUILayout.ToggleLeft("Create Folder Button", projectCreateFolder);
                projectCreateFolderByShortcut = EditorGUILayout.ToggleLeft("Create Folder By Shortcut (F7)", projectCreateFolderByShortcut);
                projectCreateMaterial = EditorGUILayout.ToggleLeft("Create Material Button", projectCreateMaterial);
                projectCreateScript = EditorGUILayout.ToggleLeft("Create Script Button", projectCreateScript);
                projectCreateShader = EditorGUILayout.ToggleLeft("Create Shader Button", projectCreateShader);
                projectFileExtension = EditorGUILayout.ToggleLeft("File Extension", projectFileExtension);
                projectOddEven = EditorGUILayout.ToggleLeft("Odd / Even Rows", projectOddEven);
                projectPlayAudio = EditorGUILayout.ToggleLeft("Play Audio Button", projectPlayAudio);
                
                EditorGUI.EndDisabledGroup();
            }

            public string GetMenuName()
            {
                return "Project";
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (projectCreateFolderByShortcut)
                {
                    return new[]
                    {
                        new Shortcut("Create Folder", "Project", KeyCode.F7),
                    };
                }

                return null;
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                projectTools = state;
                projectCreateCustomEditor = state;
                projectCreateFolder = state;
                projectCreateFolderByShortcut = state;
                projectCreateMaterial = state;
                projectCreateScript = state;
                projectCreateShader = state;
                projectFileExtension = state;
                projectOddEven = state;
                projectPlayAudio = state;
                
                GetManager<ProjectFolderIconManager>().SetState(state);
            }
        }
    }
}