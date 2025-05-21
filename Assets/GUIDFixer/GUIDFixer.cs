using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Kamgam.GF
{
    public class GUIDFixer
    {
        // Timestamp used to avoid executing the context menu multiple times.
        static double lastClickActionTime;

        static bool abortClickAction()
        {
            // Prevent executing multiple times when right-clicking.
            if (EditorApplication.timeSinceStartup - lastClickActionTime < 0.5)
                return true;

            lastClickActionTime = EditorApplication.timeSinceStartup;
            return false;
        }

        [MenuItem("Tools/GUID Fixer/Manual", priority = 1200)]
        public static void OpenManual()
        {
            EditorUtility.OpenWithDefaultApp("Assets/GUIDFixer/GUIDFixerManual.pdf");
        }

        [MenuItem("Tools/GUID Fixer/Settings", priority = 1201)]
        public static void OpenSettings()
        {
            GUIDFixerSettings.SelectSettings();
        }

        [MenuItem("Tools/GUID Fixer/Please leave a review  :)", priority = 1300)]
        public static void OpenAssetStore()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/slug/230484");
        }

        [MenuItem("Tools/GUID Fixer/Version " + GUIDFixerSettings.Version, priority = 1301)]
        public static void LogVersion()
        {
            Debug.Log("GUID Fixer v" + GUIDFixerSettings.Version + ", Unity: " + Application.unityVersion);
        }

        [MenuItem("Assets/GUID/Regenerate", priority = 1100)]
        public static void RegenerateSelected(MenuCommand menuCommand)
        {
            // Prevent executing multiple times when right-clicking.
            if (abortClickAction())
                return;

            RegenerateSelected();
        }

        [MenuItem("Tools/GUID Fixer/Regenerate GUIDs for selected")]
        public static void RegenerateSelected()
        {
            var selectedObjects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);

            if (selectedObjects == null || selectedObjects.Length == 0)
            {
                Debug.LogWarning($"No assets selected. Remember: the path must be inside the Assets folder.\n Aborting action.");
                return;
            }

            GUIDUtils.Regenerate(selectedObjects);
        }

        [MenuItem("Assets/GUID/Regenerate (but don't replace)", priority = 1100)]
        public static void RegenerateSelectedButDontReplace(MenuCommand menuCommand)
        {
            // Prevent executing multiple times when right-clicking.
            if (abortClickAction())
                return;

            RegenerateSelectedButDontReplace();
        }

        [MenuItem("Tools/GUID Fixer/Regenerate GUIDs (but don't replace)")]
        public static void RegenerateSelectedButDontReplace()
        {
            var selectedObjects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);

            if (selectedObjects == null || selectedObjects.Length == 0)
            {
                Debug.LogWarning($"No assets selected. Remember: the path must be inside the Assets folder.\n Aborting action.");
                return;
            }

            GUIDUtils.Regenerate(selectedObjects, null, replaceInAllFiles: false);
        }

        [MenuItem("Assets/GUID/Replace", priority = 1101)]
        public static void ReplaceSelected(MenuCommand menuCommand)
        {
            // Prevent executing multiple times when right-clicking.
            if (abortClickAction())
                return;

            ReplaceSelected();
        }

        [MenuItem("Tools/GUID Fixer/Replace GUID for selected")]
        public static void ReplaceSelected()
        {
            var selectedObjects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
            if (selectedObjects == null || selectedObjects.Length != 1)
            {
                Debug.LogWarning($"You can only replace the GUID for one object at a time.\n Aborting action.");
                return;
            }


            string path = AssetDatabase.GetAssetPath(selectedObjects[0]);
            string guid = GUIDFromAssetPath(path).ToString();
            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogWarning($"Could not find GUID the selected object.\n Aborting action.");
                return;
            }

            var window = GUIDFixerWindow.GetOrOpen();
            window.SetData(Command.Replace, guid);
        }

        [MenuItem("Tools/GUID Fixer/Replace GUID (global)")]
        public static void ReplaceGlobal()
        {
            var window = GUIDFixerWindow.GetOrOpen();
            window.SetData(Command.Replace, null);
        }

        [MenuItem("Assets/GUID/Copy", priority = 1102)]
        public static void CopytoClipboard(MenuCommand menuCommand)
        {
            // Prevent executing multiple times when right-clicking.
            if (abortClickAction())
                return;

            CopyToClipboard();
        }

        public static void CopyToClipboard()
        {
            var selectedObjects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);

            if (selectedObjects == null || selectedObjects.Length == 0)
            {
                Debug.LogWarning($"No assets selected. Remember: the path must be inside the Assets folder.\n Aborting action.");
                return;
            }

            string guids = "";
            foreach (var asset in selectedObjects)
            {
                string path = AssetDatabase.GetAssetPath(asset);
                if (path != null)
                {
                    string guid = GUIDFromAssetPath(path).ToString();
                    if (guids.Length == 0)
                        guids = guid;
                    else
                        guids += "," + guid;
                }
            }
            Logger.Message("Copying to clipboard: " + guids);
            GUIUtility.systemCopyBuffer = guids;
        }

        [MenuItem("Tools/GUID Fixer/Search for GUID")]
        [MenuItem("Assets/GUID/Search", priority = 1101)]
        public static void Search(MenuCommand menuCommand)
        {
            // Prevent executing multiple times when right-clicking.
            if (abortClickAction())
                return;

            var selectedObjects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
            if (selectedObjects != null && selectedObjects.Length > 0)
            {
                string guids = "";
                foreach (var asset in selectedObjects)
                {
                    string path = AssetDatabase.GetAssetPath(asset);
                    string guid = GUIDFromAssetPath(path).ToString();
                    if (guids.Length > 0)
                        guids += "," + guid;
                    else
                        guids = guid;
                }

                SearchForGUID(guids);
            }
            else
            {
                SearchForGUID(null);
            }
        }

        public static string GUIDFromAssetPath(string path)
        {
#if UNITY_2020_2_OR_NEWER
            return AssetDatabase.GUIDFromAssetPath(path).ToString();
#else
            var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (asset != null)
            {
                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset.GetInstanceID(), out string guid, out long localId))
                {
                    return guid;
                }
            }
            return null;
#endif
        }

        public static void SearchForGUID(string guid)
        {
            var window = GUIDFixerWindow.GetOrOpen();
            window.SetData(Command.Search, guid);
        }

        public static void Regenerate(Object[] objects)
        {
            if (objects == null || objects.Length == 0)
                return;

            // Check if the selected assets are within the Assets/ folder. Abort if not.
            string firstPath = AssetDatabase.GetAssetPath(objects[0]);
            if (!firstPath.StartsWith("Assets/"))
            {
                Debug.LogError($"Path must be inside the Assets folder\nFound path: {firstPath}. Aborting action.");
                return;
            }

            GUIDUtils.Regenerate(objects);
        }

        [MenuItem("GameObject/Copy Missing GUID", isValidateFunction: false, priority = 10)]
        public static void CopyMissingGUIDFromGameObject(MenuCommand menuCommand)
        {
            var go = menuCommand.context as GameObject;

            if (go != null && GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go) == 0)
            {
                Logger.Message("No missing GUIDs found in " + go.name);
                return;
            }

            var missingGuids = GUIDUtils.GetMissingGUIDs(go);
            var guidsString = "";
            foreach (var guid in missingGuids)
            {
                if (string.IsNullOrEmpty(guidsString))
                    guidsString += guid;
                else
                    guidsString += "," + guid;
            }

            if (!string.IsNullOrEmpty(guidsString))
            {
                Logger.Message("Copying to clipboard: " + guidsString);
                GUIUtility.systemCopyBuffer = guidsString;
            }
            else
            {
                Logger.Message("Sorry, the GUID info is not there anymore. You'll have to fix it manually");
            }
        }

    }
}