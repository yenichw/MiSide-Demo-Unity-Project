using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;
using System;
using Object = UnityEngine.Object;
using System.Linq;
using UnityEditor.SceneManagement;

namespace Kamgam.GF
{
    public class GUIDUtils
    {
        /// <summary>
        /// A RegEx for all the possible GUID string formats.
        /// Used to FIND any guid in text based files.
        /// 
        /// Known formats are:
        ///  guid: b67d76c6ba21170418e5bee64213a95b                      - Found in most files.
        ///  GUID:b67d76c6ba21170418e5bee64213a95b                       - Found in .asmdef & .asmref files.
        ///  \"guid\":\"44bac777a5a2aa143b3de80ce8a6526f\"               - Found in .shadergraph & .shadersubgraph
        ///  \\\"guid\\\": \\\"089e4f22f2b769c419a503e243aefafe\\\"        same
        ///  "m_GuidSerialized": "62166bae-df7f-4843-8d56-19d211203f10"  - Found in .shadergraph & .shadersubgraph
        ///  
        /// </summary>
        //static Regex guidInTextRegex = new Regex(@"(guid: *)([a-f0-9]{32})", RegexOptions.IgnoreCase); // old simpler pattern
        static Regex guidInTextRegex = new Regex("(?:(\"?(?:guid)\\\\*\"?\\s*:\\s*\\\\*\"?\\s*)([a-f0-9]{32}))|(?:(m_GuidSerialized\": *\")([a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}))", RegexOptions.IgnoreCase);

        /// <summary>
        /// A RegEx for all the possible GUID string formats.
        /// Used to REPLACE the guid in a text.
        /// 
        /// The string {0}, and {1} will be replace with the actual guid before using the pattern.
        /// {0} will be the guid without dashes, like "b67d76c6ba21170418e5bee64213a95b".
        /// {1} will be the guid WITH dashes, like "62166bae-df7f-4843-8d56-19d211203f10".
        /// Be sure to add RegexOptions.IgnoreCase when using this as it should match
        /// the guidInTextRegex above.
        /// </summary>
        // static string guidReplacePatttern = @"(guid: ?)({0})"; // old simpler pattern
        static string guidReplacePatttern = "(?:(\"?(?:guid)\\\\*\"?\\s*:\\s*\\\\*\"?\\s*)({0}))|(?:(m_GuidSerialized\": *\")({1}))";

        static Regex validGUIDRegex = new Regex(@"[a-f0-9]{32}", RegexOptions.IgnoreCase);

        public const int GUID_LENGTH = 32;

        static readonly List<string> textAssetFileExtensions = new List<string>{
            ".mat",
            ".anim",
            ".prefab",
            ".unity",
            ".asset",
            ".asmref",
            ".asmdef",
            ".shadergraph",
            ".shadersubgraph"
        };

        public static List<string> SearchForGUIDInSelected(string[] guidsToSearchFor)
        {
            var selectedObjects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
            if (selectedObjects != null || selectedObjects.Length > 0)
            {
                var pathsToScan = new List<string>();
                foreach (var asset in selectedObjects)
                {
                    var path = AssetDatabase.GetAssetPath(asset);
                    if (!string.IsNullOrEmpty(path))
                    {
                        pathsToScan.Add(path);
                    }
                }

                return SearchForGUID(guidsToSearchFor, pathsToScan);
            }

            return new List<string>();
        }

        public static List<string> SearchForGUID(string[] guidsToSearchFor, IEnumerable<string> pathsToScan = null)
        {
            try
            {
                var results = new List<string>();

                if (guidsToSearchFor == null || guidsToSearchFor.Length == 0 || string.IsNullOrEmpty(guidsToSearchFor[0]))
                    return results;

                if (pathsToScan == null)
                    pathsToScan = AssetDatabase.GetAllAssetPaths();

                // Convert asset objects into paths to files which can contain guids.
                List<string> assetPaths = new List<string>();
                foreach (var filePath in pathsToScan)
                {
                    string extension = Path.GetExtension(filePath).ToLower();
                    bool isDirectory = Directory.Exists(filePath);

                    if (!filePath.StartsWith("Assets/"))
                        continue;

                    // Add all meta files
                    string metaFilePath = filePath + ".meta";
                    if (File.Exists(metaFilePath))
                    {
                        assetPaths.Add(metaFilePath);
                    }

                    // Skip if it's a directory or if it's not a file with a text-asset extension.
                    if (isDirectory || !textAssetFileExtensions.Contains(extension))
                        continue;

                    assetPaths.Add(filePath);
                }

                // Iterate over all files and remember the GUIDs to replace.
                int counter = 0;
                foreach (var path in assetPaths)
                {
                    EditorUtility.DisplayProgressBar("Scanning", path, counter / (float)assetPaths.Count);

                    if (!File.Exists(path))
                    {
                        counter++;
                        continue;
                    }

                    string text = File.ReadAllText(path);
                    foreach (var guidToSearchFor in guidsToSearchFor)
                    {
                        if (string.IsNullOrEmpty(guidToSearchFor))
                            continue;

                        string guidToSearchForWithoutDashes = RemoveDashesFromGUID(guidToSearchFor);
                        if (text.Contains(guidToSearchForWithoutDashes))
                        {
                            results.Add(path);
                            Logger.Message($"Found in {path}");
                            break;
                        }

                        string guidToSearchForWithDashes = AddDashesToGUID(guidToSearchFor);
                        if (text.Contains(guidToSearchForWithDashes))
                        {
                            results.Add(path);
                            Logger.Message($"Found in {path}");
                            break;
                        }
                    }
                    counter++;
                }

                return results;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        public static bool IsValidGUID(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                return false;

            if (guid.Length != GUID_LENGTH)
                return false;

            if (!validGUIDRegex.IsMatch(guid))
                return false;

            return true;
        }

        /// <summary>
        /// If the replacementGUID is set and assets is only one object then the
        /// replacementGUID will be used as the new GUID. Otherwise a new random
        /// GUID will be generated for every object in assets.
        /// </summary>
        /// <param name="assets"></param>
        /// <param name="replacementGUID"></param>
        /// <param name="replaceInAllFiles"></param>
        public static void Regenerate(Object[] assets, string replacementGUID = null, bool replaceInAllFiles = true)
        {
            if (assets == null || assets.Length == 0)
                return;

            // Do not allow to specify a single GUID if multiple objects should be regenerated.
            if (assets.Length > 1 && replacementGUID != null)
                replacementGUID = null;

            // validate new guid
            if (replacementGUID != null && !IsValidGUID(replacementGUID))
            {
                Logger.Error($"replacementGUID is invalid: {replacementGUID}. Aborting action.");
                return;
            }

            // Check if the selected assets are within the Assets/ folder. Abort if not.
            string firstPath = AssetDatabase.GetAssetPath(assets[0]);
            if (!firstPath.StartsWith("Assets/"))
            {
                Logger.Error($"Path must be inside the Assets folder\nFound path: {firstPath}. Aborting action.");
                return;
            }

            bool editingStopped = false;
            try
            {
                AssetDatabase.StartAssetEditing();

                // make sure the user does not forget to save changes.
                if (replaceInAllFiles)
                {
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                }

                // STEP 1
                // 1.1 Get the GUIDs of all the given files ("assets" parameter).
                //     We assume these are already expanded meaning no recursion is necessary.

                // Get a list of .meta files for all given "assets".
                List<string> assetPaths = new List<string>();
                foreach (var asset in assets)
                {
                    string filePath = AssetDatabase.GetAssetPath(asset);
                    if (string.IsNullOrEmpty(filePath))
                    {
                        Debug.LogWarning("Null file path " + asset.name);
                        continue;
                    }

                    // Add all meta files
                    string metaFilePath = filePath + ".meta";
                    if (File.Exists(metaFilePath))
                    {
                        assetPaths.Add(metaFilePath);
                    }
                }

                // 1.2 Go through the meta files and extract the very first GUID.
                // Old to new guid info storage
                Dictionary<string, string> oldToNew = new Dictionary<string, string>();

                // Iterate over the given set of files ("assets" parameter) and remember the GUIDs to replace.
                int counter = 0;
                foreach (var filePath in assetPaths)
                {
                    EditorUtility.DisplayProgressBar("Scanning", filePath, counter / (float)assetPaths.Count);
                    Logger.Message($"Scanning meta {filePath}");

                    if (!File.Exists(filePath))
                    {
                        counter++;
                        continue;
                    }

                    string text = File.ReadAllText(filePath);
                    IEnumerable<string> guidsInFile = GetGuidsInText(text);

                    bool isFirstGuid = true;
                    foreach (string oldGUID in guidsInFile)
                    {
                        // If the old GUID points to somewhere outside of /Assets then skip
                        var oldGuidPath = AssetDatabase.GUIDToAssetPath(oldGUID);
                        if (!oldGuidPath.StartsWith("Assets/"))
                            continue;

                        // The first GUID in a .meta file is the GUID of the corresponding asset.
                        // Only those are remembered for replacement.
                        if (isFirstGuid && filePath.EndsWith(".meta") && !oldToNew.ContainsKey(oldGUID))
                        {
                            string newGUID = replacementGUID != null ? replacementGUID : Guid.NewGuid().ToString("N");
                            oldToNew.Add(oldGUID, newGUID);

                            Logger.Message($"  The old guid {oldGUID} will become {newGUID}");
                        }
                    }
                    counter++;
                }

                // Step 2
                // 2.1 Get a list of ALL text assets since we have to replace the GUIDS in all of them.
                Dictionary<string, List<string>> filesWithGuids = new Dictionary<string, List<string>>();
                if (replaceInAllFiles)
                {
                    var allAssetPaths = AssetDatabase.GetAllAssetPaths();
                    assetPaths.Clear();
                    foreach (var filePath in allAssetPaths)
                    {
                        if (!filePath.StartsWith("Assets/"))
                            continue;

                        string extension = Path.GetExtension(filePath).ToLower();
                        bool isDirectory = Directory.Exists(filePath);

                        // Add all meta files
                        string metaFilePath = filePath + ".meta";
                        if (File.Exists(metaFilePath))
                        {
                            assetPaths.Add(metaFilePath);
                        }

                        // Skip if it's a directory or if it's not a file with a text-asset extension.
                        if (isDirectory || !textAssetFileExtensions.Contains(extension))
                            continue;

                        assetPaths.Add(filePath);
                    }
                }

                // 2.2 Iterate over ALL asset files and extract the GUIDS within them
                // We need these infos to replace the guids.
                counter = 0;
                foreach (var filePath in assetPaths)
                {
                    EditorUtility.DisplayProgressBar("Scanning", filePath, counter / (float)assetPaths.Count);

                    Logger.Message($"Scanning {filePath}");

                    if (!File.Exists(filePath))
                    {
                        counter++;
                        continue;
                    }

                    // Add all guids in each file to the filesWithGuids map.
                    string text = File.ReadAllText(filePath);
                    List<string> guidsInFile = GetGuidsInText(text);
                    bool containsOldGUIDs = false;
                    foreach (var old in oldToNew.Keys)
                    {
                        if (guidsInFile.Contains(old))
                        {
                            containsOldGUIDs = true;
                            break;
                        }
                    }

                    if (!containsOldGUIDs)
                        continue;

                    foreach (string oldGUID in guidsInFile)
                    {
                        if (!filesWithGuids.ContainsKey(filePath))
                            filesWithGuids.Add(filePath, new List<string> { });

                        if (!filesWithGuids[filePath].Contains(oldGUID))
                        {
                            filesWithGuids[filePath].Add(oldGUID);

                            // There may be guids in the form of 0000000000000000f000000000000000 in the logs.
                            // These are internal Unity GUIDS.
                            Logger.Message($"  Found guid: {oldGUID} in '{filePath}'");
                        }
                    }
                    counter++;
                }

                EditorUtility.ClearProgressBar();

                // Make a logable list of the files which are to be changed (limit to N files).
                int numOfFilesToList = 6;
                int fc = 0;
                var affectedFilePaths = "\n\nAffected files:\n";
                foreach (var kv in oldToNew)
                {
                    fc++;
                    if (fc > numOfFilesToList)
                        break;

                    var path = AssetDatabase.GUIDToAssetPath(kv.Key);
                    var fileName = System.IO.Path.GetFileName(path);
                    affectedFilePaths += $"* {fileName}\n";
                }
                fc = 0;
                foreach (var kv in filesWithGuids)
                {
                    fc++;
                    if (fc > numOfFilesToList)
                        break;

                    var path = kv.Key;
                    var fileName = System.IO.Path.GetFileName(path);
                    affectedFilePaths += $"* {fileName}\n";
                }
                if (oldToNew.Count > numOfFilesToList || filesWithGuids.Count > numOfFilesToList)
                {
                    affectedFilePaths += "...\n";
                }

                if (!EditorUtility.DisplayDialog("GUID Change",
                    $"You are about to change some guids." +
                    $"\n{oldToNew.Count} asset(s) will have their guids changed." +
                    $"\n{filesWithGuids.Count} will have their contents changed." +
                    affectedFilePaths +
                    $"\nDetails are in the logs." +
                    $"\n" +
                    $"\nThis may have unexpected results and can not be undone." +
                    $"\n" +
                    $"\nMAKE A BACKUP BEFORE YOU PROCEED!" +
                    $"\n(Be sure to make a copy if you have stuff ignored in your VCS.)",
                    "Change GUIDs", "Cancel"))
                    return;

                // Step 3:
                // Close any open scenes to avoid conflicts between unsaved scene state and our changes.
                // Memorize open scenes
                int sceneCount = EditorSceneManager.sceneCount;
                var openScenes = new string[sceneCount];
                if (replaceInAllFiles)
                {
                    for (int i = 0; i < sceneCount; i++)
                    {
                        var openScene = EditorSceneManager.GetSceneAt(i);
                        openScenes[i] = openScene.path;
                    }
                    // Create new tmp scene and open it
                    var tmpScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
                }

                // Step 4
                // Iterate over all files and replace the old GUIDs with new GUIDs.
                counter = 0;
                var guidsToChange = oldToNew.Keys.ToList();
                var filesToChange = filesWithGuids.Keys.ToList();
                int startIndexForRevert = 0;
                try
                {
                    for (int i = 0; i < filesToChange.Count; i++)
                    {
                        string filePath = filesToChange[i];
                        EditorUtility.DisplayProgressBar("Changing GUIDs", filePath, (i + 1) / (float)guidsToChange.Count);
                        counter++;

                        Logger.Message($"Editing {filePath}");

                        string text = File.ReadAllText(filePath);
                        bool modified = false;
                        foreach (string oldGUID in filesWithGuids[filePath])
                        {
                            if (!guidsToChange.Contains(oldGUID))
                                continue;

                            string newGUID = oldToNew[oldGUID];
                            if (string.IsNullOrEmpty(newGUID))
                                continue;

                            string newGUIDWithDashes = AddDashesToGUID(newGUID);
                            string oldGUIDWithDashes = AddDashesToGUID(oldGUID);
                            string replacePattern = string.Format(guidReplacePatttern, oldGUID, oldGUIDWithDashes);
                            text = Regex.Replace(text, replacePattern, m => {
                                if (string.IsNullOrWhiteSpace(m.Groups[1].Value))
                                {
                                    return m.Groups[3].Value + newGUIDWithDashes;
                                }
                                else
                                {
                                    return m.Groups[1].Value + newGUID;
                                }
                            }, RegexOptions.IgnoreCase);
                            modified = true;
                            Logger.Message($"  Changing {filePath} guid {oldGUID} to {newGUID}");
                        }

                        if (modified)
                        {
                            File.WriteAllText(filePath, text);
                            startIndexForRevert = i;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("Error while changing GUIDS (" + e.Message + ").");
                    Logger.Warning("Attempting to revert changes.");

                    try
                    {
                        for (int i = startIndexForRevert; i >= 0; i--)
                        {
                            string filePath = filesToChange[i];
                            EditorUtility.DisplayProgressBar("Reverting changing GUIDs", filePath, (startIndexForRevert - i) / (float)startIndexForRevert);

                            string text = File.ReadAllText(filePath);
                            bool modified = false;
                            foreach (string oldGUID in filesWithGuids[filePath])
                            {
                                if (!guidsToChange.Contains(oldGUID))
                                    continue;

                                string newGUID = oldToNew[oldGUID];
                                if (string.IsNullOrEmpty(newGUID))
                                    continue;

                                string newGUIDWithDashes = AddDashesToGUID(newGUID);
                                string oldGUIDWithDashes = AddDashesToGUID(oldGUID);
                                string replacePattern = string.Format(guidReplacePatttern, newGUID, newGUIDWithDashes);
                                text = Regex.Replace(text, replacePattern, m => {
                                    if (string.IsNullOrWhiteSpace(m.Groups[1].Value))
                                    {
                                        return m.Groups[3].Value + oldGUIDWithDashes;
                                    }
                                    else
                                    {
                                        return m.Groups[1].Value + oldGUID;
                                    }
                                }, RegexOptions.IgnoreCase);
                                modified = true;
                                Logger.Warning($"  Reverted {filePath} guid {newGUID} to {oldGUID}");
                            }

                            if (modified)
                            {
                                File.WriteAllText(filePath, text);
                            }
                        }

                        Logger.Message("Reverting seems to have worked. The native Exception will be shown below.");
                    }
                    catch
                    {
                        throw;
                    };

                    throw;
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                    EditorUtility.ClearProgressBar();
                    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                    editingStopped = true;

                    // Restore open scenes
                    if (replaceInAllFiles)
                    {
                        for (int i = 0; i < sceneCount; i++)
                        {
                            if (i == 0)
                            {
                                EditorSceneManager.OpenScene(openScenes[i], OpenSceneMode.Single);
                            }
                            else
                            {
                                EditorSceneManager.OpenScene(openScenes[i], OpenSceneMode.Additive);
                            }
                        }
                        openScenes = null;
                    }
                }
            }
            finally
            {
                if (!editingStopped)
                {
                    AssetDatabase.StopAssetEditing();
                    EditorUtility.ClearProgressBar();
                    AssetDatabase.Refresh();
                }
            }

            Logger.Message("Done.");
        }

        public static List<string> GetGuidsInFile(string filePath, bool unique = false)
        {
            string text = File.ReadAllText(filePath);
            return GetGuidsInText(text, unique);
        }

        public static List<string> GetGuidsInText(string text, bool unique = false)
        {
            List<string> guids = new List<string>();

            var matches = guidInTextRegex.Matches(text);
            foreach (Match match in matches)
            {
                // Due to the "()()|()()" grouping the the regexp the results will
                // have 5 groups with index 1, 2 for "guid"s and index 3, 4 for "m_GuidSerialized"s.
                string guid = string.IsNullOrWhiteSpace(match.Groups[2].Value) ? match.Groups[4].Value : match.Groups[2].Value;
                guid = RemoveDashesFromGUID(guid);
                if (unique)
                {
                    if (!guids.Contains(guid))
                        guids.Add(guid);
                }
                else
                {
                    guids.Add(guid);
                }
            }

            return guids;
        }

        /// <summary>
        /// Converts a GUID from
        ///   "37e4f7e1-5432-4b55-81b5-1219a537a28d"
        /// to
        ///   "37e4f7e154324b5581b51219a537a28d".
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static string RemoveDashesFromGUID(string guid)
        {
            return guid.Replace("-", "");
        }

        /// <summary>
        /// Converts a GUID from
        ///   "37e4f7e154324b5581b51219a537a28d"
        /// to
        ///   "37e4f7e1-5432-4b55-81b5-1219a537a28d".
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static string AddDashesToGUID(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                return guid;

            if (guid.Contains('-'))
                return guid;

            if (guid.Length < GUID_LENGTH)
                return guid;

            return guid.Substring(0, 8) + "-" +
                   guid.Substring(8, 4) + "-" +
                   guid.Substring(12, 4) + "-" +
                   guid.Substring(16, 4) + "-" +
                   guid.Substring(20);
        }

        public static List<string> GetMissingGUIDs(GameObject go)
        {
            string scenePath = null;
            var sceneGUIDs = AssetDatabase.FindAssets("t:scene");
            foreach (var guid in sceneGUIDs)
            {
                var tmpScenePath = AssetDatabase.GUIDToAssetPath(guid);
                if (go.scene.path == tmpScenePath)
                {
                    scenePath = tmpScenePath;
                    break;
                }
            }

            if (scenePath == null)
                return new List<string>();

            // Extract all game object names which contain any of the missing guids.
            // Then match them to the go.name and return only those which have
            // the same name.
            //
            // Results might be ambiguous if there are mutliple gameobject with the
            // same name.
            var guids = new List<string>();
            var missingGuids = GUIDUtils.GetMissingGUIDsInFile(scenePath);
            var missingObjectNames = GUIDUtils.GetGameObjectNamesContainingGUIDs(scenePath, missingGuids);
            var objectNamesWithMissingGUIDs = new List<string>();
            for (int i = 0; i < missingGuids.Count; i++)
            {
                var names = missingObjectNames[i];
                if (names.Count > 0)
                {
                    foreach (var name in names)
                    {
                        if (name != go.name)
                            continue;

                        // objectNamesWithMissingGUIDs only exists to detect amiguity.
                        if (!objectNamesWithMissingGUIDs.Contains(name))
                        {
                            objectNamesWithMissingGUIDs.Add(name);
                        }
                        else
                        {
                            Logger.Log("Ambiguous results. There are multiple GUIDs missing on a one or more game objects named '" + name + "'.");
                        }

                        // add to final results
                        if (!guids.Contains(missingGuids[i]))
                            guids.Add(missingGuids[i]);
                    }
                }
            }

            return guids;
        }

        public static List<string> GetMissingGUIDsInFile(string filePath)
        {
            return GetMissingGUIDsInFiles(new List<string>() { filePath });
        }

        /// <summary>
        /// It does not return guids which point to Unity internal files.
        /// </summary>
        /// <param name="filePaths"></param>
        /// <returns></returns>
        public static List<string> GetMissingGUIDsInFiles(IEnumerable<string> filePaths)
        {
            try
            {
                EditorUtility.DisplayProgressBar("Scanning files ...", "Scanning files for missing GUIDs.", 0.1f);

                var missingGuids = new List<string>();

                // Convert asset objects into paths to files which can contain guids.
                foreach (var filePath in filePaths)
                {
                    string extension = Path.GetExtension(filePath).ToLower();
                    bool isDirectory = Directory.Exists(filePath);

                    if (!filePath.StartsWith("Assets/"))
                        continue;

                    string text = File.ReadAllText(filePath);
                    if (string.IsNullOrEmpty(text))
                        continue;

                    var guids = GetGuidsInText(text, unique: true);
                    foreach (var guid in guids)
                    {
                        // Skip Unity internal GUIDs (0000000000000000f000000000000000)
                        if (IsInternalGUID(guid))
                            continue;

                        // Add only if it is missing
                        var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                        if (string.IsNullOrEmpty(assetPath) || !System.IO.File.Exists(assetPath))
                            missingGuids.Add(guid);
                    }
                }

                return missingGuids;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        /// <summary>
        /// For each given value in guids a result List is returned which contains the names of the 
        /// game objects which contain this GUID. If none is found the results will contains an empty
        /// list at that index. Indices of the reulsts match the indices of the given guids.
        /// </summary>
        /// <param name="sceneFilePath"></param>
        /// <param name="guids"></param>
        /// <returns></returns>
        public static List<List<string>> GetGameObjectNamesContainingGUIDs(string sceneFilePath, IList<string> guids)
        {
            var names = new List<List<string>>();

            string sceneYAML = File.ReadAllText(sceneFilePath);
            if (string.IsNullOrEmpty(sceneYAML))
                return names;

            foreach (var guid in guids)
            {
                // If more than one name is returned then the result was ambiguous.
                var foundNames = GetGameObjectNamesContainingGUID(sceneYAML, guid);
                names.Add(foundNames);
            }

            return names;
        }

        /// <summary>
        /// If more than one result is returned then the GUID is missing in multiple game objects.
        /// </summary>
        /// <param name="sceneYAML"></param>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static List<string> GetGameObjectNamesContainingGUID(string sceneYAML, string guid)
        {
            var results = new List<string>();

            if (string.IsNullOrEmpty(sceneYAML))
                return results;

            // Sample YAML data which we need to extract the name from.
            // GameObject:
            //   m_ObjectHideFlags: 0
            //   m_CorrespondingSourceObject: {fileID: 0}
            //   m_PrefabInstance: {fileID: 0}
            //   m_PrefabAsset: {fileID: 0}
            //   serializedVersion: 6
            //   m_Component:
            //   - component: {fileID: 1212576070}
            //   - component: {fileID: 1212576069}
            //   - component: {fileID: 1212576068}
            //   m_Layer: 0
            //   m_Name: ObjWithMissingScript (4)
            //   m_TagString: Untagged
            //   m_Icon: {fileID: 0}
            //   m_NavMeshLayer: 0
            //   m_StaticEditorFlags: 0
            //   m_IsActive: 1
            // --- !u!114 &1212576069
            // MonoBehaviour:
            //   m_ObjectHideFlags: 0
            //   m_CorrespondingSourceObject: {fileID: 0}
            //   m_PrefabInstance: {fileID: 0}
            //   m_PrefabAsset: {fileID: 0}
            //   m_GameObject: {fileID: 1212576068}
            //   m_Enabled: 1
            //   m_EditorHideFlags: 0
            //   m_Script: {fileID: 11500000, guid: 7fde22b9070274d498e956bbd69d310b, type: 3}
            //   m_Name: GameObject:
            //   m_EditorClassIdentifier: 
            // --- !u!114 &1212576068
            // MonoBehaviour:
            //   m_ObjectHideFlags: 0
            //   m_CorrespondingSourceObject: {fileID: 0}
            //   m_PrefabInstance: {fileID: 0}
            //   m_PrefabAsset: {fileID: 0}
            //   m_GameObject: {fileID: 1212576068}
            //   m_Enabled: 1
            //   m_EditorHideFlags: 0
            //   m_Script: {fileID: 11500000, guid: 7fde22b9070274d498e956bbd69d310b, type: 3}
            //   m_Name: 
            //   m_EditorClassIdentifier: 

            // Step 1: Extract a substrings which contain the starting "GameObject:" and
            // ends with the last ocurrence of the guid (non greedy).
            var gameObjectRegExp = new Regex(string.Format("^GameObject: *$.*?{0}", guid), RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.RightToLeft);
            var matches = gameObjectRegExp.Matches(sceneYAML);
            if (matches.Count == 0)
                return results;

            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                var gameObjectString = match.Value;
                if (string.IsNullOrEmpty(gameObjectString))
                    return results;

                // Step 2: Find the GameObject name and add it to the list
                var nameRegExp = new Regex(@"GameObject:.*?m_Name: *(.*?)[\r\n]", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline);
                var nameMatch = nameRegExp.Match(gameObjectString);
                if (!nameMatch.Success || nameMatch.Groups.Count < 2)
                    return results;

                results.Add(nameMatch.Groups[1].Value);
            }

            return results;
        }

        public static bool IsInternalGUID(string guid)
        {
            // Unity internal GUIDs look like this: 0000000000000000f000000000000000

            if (guid.StartsWith("00000000") && guid.EndsWith("00000000"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path) || !path.StartsWith("Asset"))
                    return true;
            }

            return false;
        }
    }

}