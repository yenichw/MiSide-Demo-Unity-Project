﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditorInternal;
#if HDRP_1_OR_NEWER
using UnityEngine.Rendering.HighDefinition;
#endif
using Bewildered.SmartLibrary.UI;

using Object = UnityEngine.Object;

namespace Bewildered.SmartLibrary
{
    public enum TextTruncationPosition { Start, Middle, End }

    internal static class LibraryUtility
    {
        private static Func<bool> _hasCurrentWindowKeyFocus;
        private static MethodInfo _openPropertyWindowInfo;

        private static readonly Material _previewGUIMaterial = (Material)AssetDatabase.LoadMainAssetAtPath(RootPath + "/UI/Materials/PreviewGUIMaterial.mat");
        private static readonly Material _previewGUIMaterialSRP = (Material)AssetDatabase.LoadMainAssetAtPath(RootPath + "/UI/Materials/PreviewGUIMaterialSRP.mat");

        /// <summary>
        /// The GUI <see cref="Material"/> used for rendering preview textures with transparent backgrounds.
        /// </summary>
        internal static Material PreviewGUIMaterial
        {
            get { return IsUsingSRP ? _previewGUIMaterialSRP : _previewGUIMaterial; }
        }

        /// <summary>
        /// Whether a Scriptable Render Pipeline us being used or the built-in pipeline.
        /// </summary>
        public static bool IsUsingSRP
        {
            get
            {
#if UNITY_6000_0_OR_NEWER
                return GraphicsSettings.defaultRenderPipeline != null;
#else
                return GraphicsSettings.renderPipelineAsset != null;
#endif
            }
        }
        
        /// <summary>
        /// Determines whether the High-definition renderpipeline is actually
        /// being used for rendering or if hte package is simply present.
        /// </summary>
        internal static bool IsRenderingWithHDRP
        {
            get
            {
#if HDRP_1_OR_NEWER
                return GraphicsSettings.currentRenderPipeline is HDRenderPipelineAsset;
#else
                return false;
#endif
            }
        }

#if HDRP_1_OR_NEWER
        
        internal static bool DoesHDRPSupportAlpha
        {
            get
            {
                if (!IsRenderingWithHDRP)
                    return true;
                
                var hdrpAsset = (HDRenderPipelineAsset) GraphicsSettings.currentRenderPipeline;
                
                // We return if the setting is already set to R16G16B16A16.
                return hdrpAsset.currentPlatformRenderPipelineSettings.colorBufferFormat == 
                       RenderPipelineSettings.ColorBufferFormat.R16G16B16A16;
            }
        }
#endif

        /// <summary>
        /// The path of the "SmartLibrary/Editor" folder reletive to the assets folder.
        /// </summary>
        public static string RootPath
        {
            get { return EditorAssetsFolderLocator.GetFolderPath(); }
        }

        [MenuItem("Assets/Add to Last Collection", true)]
        private static bool AddToLastCollectionValidation()
        {
            if (Selection.assetGUIDs.Length > 0 && SmartLibraryWindow.LastActiveWindow != null)
            {
                if (SmartLibraryWindow.LastActiveWindow.SelectedCollection is ILibrarySet)
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }

        [MenuItem("Assets/Add to Last Collection")]
        private static void AddToLastCollection()
        {
            LibraryCollection collection = SmartLibraryWindow.LastActiveWindow.SelectedCollection;
            if (collection is ILibrarySet collectionSet)
            {
                var items = new List<LibraryItem>();
                foreach (var guid in Selection.assetGUIDs)
                {
                    items.Add(LibraryItem.GetItemInstance(guid));
                }
                collectionSet.UnionWith(items);
            }
        }

        internal static StyleSheet LoadStyleSheet(string name)
        {
            return AssetDatabase.LoadAssetAtPath<StyleSheet>($"{RootPath}/UI/{name}.uss");
        }

        internal static VisualTreeAsset LoadVisualTree(string name)
        {
            return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{RootPath}/UI/{name}.uxml");
        }

        internal static Texture2D LoadLibraryIcon(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>($"{RootPath}/UI/Icons/{(EditorGUIUtility.isProSkin ? "d_" : "")}{name}.png");
        }

        public static EditorWindow OpenPropertyEditor(Object obj)
        {
            if (_openPropertyWindowInfo == null)
            {
                Type propertyEditorType = typeof(EditorWindow).Assembly.GetType("UnityEditor.PropertyEditor");
                _openPropertyWindowInfo = TypeAccessor.GetMethod(propertyEditorType, "OpenPropertyEditor", typeof(Object), typeof(bool));
            }

            return (EditorWindow)_openPropertyWindowInfo.Invoke(null, new object[] { obj, true });
        }

        internal static void BuildCreateCollectionMenu(DropdownMenu menu, CollectionsTreeView collectionsTreeView, bool asChildOfSelected)
        {
            menu.AppendAction("Standard Collection", action => OnCreateMenuItemSelect(collectionsTreeView, asChildOfSelected, typeof(StandardCollection)));
            menu.AppendAction("Smart Collection", action => OnCreateMenuItemSelect(collectionsTreeView, asChildOfSelected, typeof(SmartCollection)));
            IEnumerable<Type> collectionTypes = TypeCache.GetTypesDerivedFrom<LibraryCollection>();
            foreach (Type collectionType in collectionTypes)
            {
                if (collectionType == typeof(RootLibraryCollection) ||
                    collectionType == typeof(StandardCollection) ||
                    collectionType == typeof(SmartCollection) || 
                    collectionType == typeof(CompoundCollection))
                    continue;

                string entryName = ObjectNames.NicifyVariableName(collectionType.Name);

                menu.AppendAction(entryName, action => OnCreateMenuItemSelect(collectionsTreeView, asChildOfSelected, collectionType));
            }
            menu.AppendSeparator();
            menu.AppendAction("Compound Collection", action => OnCreateMenuItemSelect(collectionsTreeView, asChildOfSelected, typeof(CompoundCollection)));
        }

        private static void OnCreateMenuItemSelect(CollectionsTreeView collectionsTreeView, bool asChildOfSelected, Type collectionType)
        {
            var newCollection = LibraryCollection.CreateCollection(collectionType);

            if (asChildOfSelected && collectionsTreeView.SelectedItem is CollectionTreeViewItem collectionTreeViewItem)
                collectionTreeViewItem.Collection.AddSubcollection(newCollection);
            else
                LibraryDatabase.AddBaseCollection(newCollection);
            
            collectionsTreeView.Rebuild();
            collectionsTreeView.ScrollToCollection(newCollection);
            collectionsTreeView.SetCollectionSelection(newCollection);
            collectionsTreeView.BeginRenamingCollection(newCollection);
        }

        /// <summary>
        /// Saves a <see cref="LibraryCollection"/> to disk.
        /// </summary>
        /// <param name="collection">The collection to save.</param>
        internal static void SaveCollection(LibraryCollection collection)
        {
            Directory.CreateDirectory(LibraryConstants.CollectionsPath);

            string path = GetCollectionPath(collection);
            
            InternalEditorUtility.SaveToSerializedFileAndForget(new Object[] {collection}, path, true);
        }

        internal static void EnqueueSaveCollection(LibraryCollection collection)
        {
            CollectionSaveSync.EnqueueCollection(collection);
        }

        internal static void DeleteCollectionFile(LibraryCollection collection)
        {
            string path = GetCollectionPath(collection);
            File.Delete(path);
        }

        internal static void RenameCollectionFile(LibraryCollection collection, string newName)
        {
            string path = GetCollectionPath(collection);
            
            if (!File.Exists(path))
                return;
            
            string newFilename = $"{newName}_{collection.ID}";
            newFilename = GetSafeFilename(newFilename);
            string newPath = $"{LibraryConstants.CollectionsPath}/{newFilename}{LibraryConstants.CollectionFileExtension}";
            
            File.Move(path, newPath);
        }

        /// <summary>
        /// Returns the project relative file path for the specified collection.
        /// </summary>
        /// <param name="collection">The collection to get the file path for.</param>
        /// <returns>The file path for the collection.</returns>
        internal static string GetCollectionPath(LibraryCollection collection)
        {
            string filename = $"{collection.CollectionName}_{collection.ID}";
            filename = GetSafeFilename(filename);
            return $"{LibraryConstants.CollectionsPath}/{filename}{LibraryConstants.CollectionFileExtension}";
        }

        internal static UniqueID GetCollectionIDFromPath(string collectionPath)
        {
            string filename = Path.GetFileNameWithoutExtension(collectionPath);
            string idString = filename.Substring(filename.LastIndexOf('_') + 1);
            
            return new UniqueID(idString);
        }

        internal static void LoadAllCollections()
        {
            var idCollectionPairs = new SerializableDictionary<UniqueID, LibraryCollection>();
            RootLibraryCollection rootCollection = null;

            Directory.CreateDirectory(LibraryConstants.CollectionsPath);
            foreach (string collectionPath in Directory.EnumerateFiles(LibraryConstants.CollectionsPath))
            {
                var collection = LoadCollection(collectionPath);

                if (collection == null)
                {
                    Debug.LogWarning($"SmartLibrary: Could not load collection at '{collectionPath}'");
                    continue;
                }
                
                // This is required since in 2.0.0 the HideAndDontSave flag was used. So this is used to update it.
                if (collection.hideFlags != HideFlags.DontSave)
                    collection.hideFlags = HideFlags.DontSave;   
                
                // References to other collections cannot be serialized so the list will
                // will only be null entries. We re-add the subcollections further down.
                //collection.SubcollectionsInternal.Clear();
                

                if (collection is RootLibraryCollection rootLibraryCollection)
                {
                    rootCollection = rootLibraryCollection;
                }

                idCollectionPairs[collection.ID] = collection;
            }
            
            if (rootCollection == null)
            {
                rootCollection = ScriptableObject.CreateInstance<RootLibraryCollection>();
                rootCollection.hideFlags = HideFlags.HideAndDontSave;
                rootCollection.CollectionName = "ROOT";
                SaveCollection(rootCollection);
            }

            LibraryDatabase.RootCollection = rootCollection;
            SessionData.instance.IDToCollectionMap = idCollectionPairs;

            // References to other collections are not saved to file, so we need to reassign them once we have
            // loaded them all back in.
            foreach (var collection in LibraryDatabase.EnumerateCollections())
            {
                if (collection is RootLibraryCollection)
                    continue;

                // We cache the parent id since setting the parent field to null would reset the id
                // and we use it for he warning log.
                UniqueID parentId = collection.ParentID;
                
                collection.Root = rootCollection;
                collection.Parent = LibraryDatabase.FindCollectionByID(parentId);
                if (collection.Parent == null)
                {
                    collection.Parent = rootCollection;
                    Debug.LogWarning($"Could not find parent with id {parentId}. Collection '{collection}' was added as a base collection.");
                }

                int index = collection.Parent.SubcollectionIDs.IndexOf(collection.ID);
                if (index > -1)
                {
                    collection.Parent.SubcollectionsInternal[index] = collection;
                }
                else // This should *never* happen...
                {
                    // For some reason a user had an issue where a collection's ID
                    // and the id stored in it's parent subcollectionIDs where different. This handles that.
                    LibraryCollection parent = collection.Parent; 
                    UniqueID mismatchedID = UniqueID.Empty;
                    for (int i = collection.Parent.SubcollectionIDs.Count - 1; i >= 0; i--)
                    {
                        var subcollection = LibraryDatabase.FindCollectionByID(collection.Parent.SubcollectionIDs[i]);
                        if (subcollection == null)
                        {
                            mismatchedID = parent.SubcollectionIDs[i];
                            parent.SubcollectionIDs.RemoveAt(i);
                            parent.SubcollectionsInternal.RemoveAt(i);
                            break;
                        }
                    }
                    
                    parent.SubcollectionIDs.Add(collection.ID);
                    parent.SubcollectionsInternal.Add(collection);
                    Debug.LogWarning(
                        $"SmartLibraryDataMismatch: The parent collection for '{collection.CollectionName}'({collection.ID}) " +
                        $"contained an invalid subcollection id ({mismatchedID}) for the collection and the collection was re-added. Please report this issue.");
                    SaveCollection(parent);
                }

                collection.OnAfterLoad();
                
                SessionData.CacheCollectionData(collection, false);
            }
        }

        /// <summary>
        /// Loads a <see cref="LibraryCollection"/> from a file path on disk.
        /// </summary>
        /// <param name="path">The project relative path to the <see cref="LibraryCollection"/> to load.</param>
        /// <returns>The <see cref="LibraryCollection"/> at the specified path if there is one; otherwise, <c>null</c>.</returns>
        internal static LibraryCollection LoadCollection(string path)
        {
            var loadedObjects = InternalEditorUtility.LoadSerializedFileAndForget(path);
            if (loadedObjects.Length == 0)
                return null;

            return loadedObjects[0] as LibraryCollection;
        }

        public static string GetSafeFilename(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }

        public static Bounds GetRenderableBoundsRecurse(Bounds bounds, GameObject go)
        {
            return GetRenderableBoundsRecurse(bounds, go, out _);
        }

        public static Bounds GetRenderableBoundsRecurse(Bounds bounds, GameObject go, out bool has2DRenderer)
        {
            has2DRenderer = false;
            
            // Do we have a mesh?
            var renderer = go.GetComponent<MeshRenderer>();
            var filter = go.GetComponent<MeshFilter>();
            if (renderer && filter && filter.sharedMesh)
            {
                // To prevent origin from always being included in bounds we initialize it
                // with renderer.bounds. This ensures correct bounds for meshes with origo outside the mesh.
                if (bounds.extents == Vector3.zero)
                    bounds = renderer.bounds;
                else
                    bounds.Encapsulate(renderer.bounds);
            }

            // Do we have a skinned mesh?
            var skin = go.GetComponent<SkinnedMeshRenderer>();
            if (skin && skin.sharedMesh)
            {
                if (bounds.extents == Vector3.zero)
                    bounds = skin.bounds;
                else
                    bounds.Encapsulate(skin.bounds);
            }

            // Do we have a Sprite?
            var sprite = go.GetComponent<SpriteRenderer>();
            if (sprite && sprite.sprite)
            {
                has2DRenderer = true;
                if (bounds.extents == Vector3.zero)
                    bounds = sprite.bounds;
                else
                    bounds.Encapsulate(sprite.bounds);
            }

            // Do we have a billboard?
            var billboard = go.GetComponent<BillboardRenderer>();
            if (billboard && billboard.billboard && billboard.sharedMaterial)
            {
                has2DRenderer = true;
                if (bounds.extents == Vector3.zero)
                    bounds = billboard.bounds;
                else
                    bounds.Encapsulate(billboard.bounds);
            }

            // Recurse into children
            foreach (Transform t in go.transform)
            {
                if (has2DRenderer)
                    bounds = GetRenderableBoundsRecurse(bounds, t.gameObject, out _);
                else
                    bounds = GetRenderableBoundsRecurse(bounds, t.gameObject, out has2DRenderer);
            }

            return bounds;
        }
        
        internal static string PrettyFileName(string name)
        {
            string result = "";
            for (int i = 0; i < name.Length; i++)
            {
                // Add a space before a capital letter if it is not the first character in the name,
                // and not already space before it.
                if (char.IsUpper(name[i]) && i > 0 && !char.IsWhiteSpace(name[i - 1]))
                {
                    if (i + 1 < name.Length)
                    {
                        if (char.IsLower(name[i + 1]) || char.IsLower(name[i - 1]))
                            result += ' ';
                    }
                    else if (char.IsLower(name[i - 1]))
                    {
                        result += ' ';
                    }
                }

                if (name[i] == '-' || name[i] == '_') // Replace '-' and '_' with a space.
                    result += ' ';
                else if (result.Length == 0) // Capitalize the first letter.
                    result += char.ToUpper(name[i]);
                else if (i > 0 && char.IsWhiteSpace(result[result.Length - 1])) // Capitalize the latter if the last added result was a space.
                    result += char.ToUpper(name[i]);
                else
                    result += name[i];
            }

            return result;
        }

        public static string TruncateTextWordWrap(GUIStyle style, string text, Rect area, TextTruncationPosition truncationPosition)
        {
            if (string.IsNullOrEmpty(text) || area.height < style.lineHeight)
                return text;

            Vector2 textSize = MessureTextSize(text);
            if (textSize.y <= area.height)
                return text;

            int lastTextIndex = text.Length;

            string truncatedText;
            int minIndex = truncationPosition == TextTruncationPosition.Start ? 1 : 0;
            int maxIndex = (truncationPosition == TextTruncationPosition.Start || truncationPosition == TextTruncationPosition.Middle) ? lastTextIndex : lastTextIndex - 1;
            int midIndex = (minIndex + maxIndex) / 2;
            int previusFitMidIndex = -1;

            int exceptionHandlerCount = 0;

            while (minIndex <= maxIndex)
            {
                truncatedText = Truncate(midIndex);
                textSize = MessureTextSize(truncatedText);

                if (textSize.y > area.height)
                {
                    if (midIndex - 1 == previusFitMidIndex)
                        return Truncate(previusFitMidIndex);

                    if (truncationPosition == TextTruncationPosition.Start)
                        minIndex = midIndex + 1;
                    else
                        maxIndex = midIndex - 1;
                }
                else
                {
                    if (truncationPosition == TextTruncationPosition.Start)
                        maxIndex = midIndex - 1;
                    else
                        minIndex = midIndex + 1;

                    previusFitMidIndex = midIndex;
                }

                midIndex = (minIndex + maxIndex) / 2;


                if (exceptionHandlerCount > 200)
                    throw new TimeoutException();
                else
                    exceptionHandlerCount++;
            }

            // Local Method.
            Vector2 MessureTextSize(string messureText)
            {
                return new Vector2(area.width, style.CalcHeight(new GUIContent(messureText), area.width));
            }

            // Local Method.
            // Actually truncates the text at the specified index.
            string Truncate(int middleIndex)
            {
                int lengthFromMiddle = (text.Length - 1) - (middleIndex - 1);
                switch (truncationPosition)
                {
                    case TextTruncationPosition.Start:
                        return LibraryConstants.Ellipsis + text.Substring(middleIndex, lengthFromMiddle);
                    case TextTruncationPosition.Middle: // Need to Mathf.Max(..) because some people get an argument out of range error here.
                        return text.Substring(0, Mathf.Max(middleIndex - 1, 0)) + LibraryConstants.Ellipsis + text.Substring(lengthFromMiddle);
                    case TextTruncationPosition.End:
                        return text.Substring(0, middleIndex) + LibraryConstants.Ellipsis;
                }

                return text;
            }

            return Truncate(previusFitMidIndex);
        }

        public static bool HasCurrentWindowKeyFocus()
        {
            if (_hasCurrentWindowKeyFocus == null)
            {
                var method = TypeAccessor.GetMethod<EditorGUIUtility>("HasCurrentWindowKeyFocus");
                _hasCurrentWindowKeyFocus = (Func<bool>)method.CreateDelegate(typeof(Func<bool>));
            }

            return _hasCurrentWindowKeyFocus();
        }
        
        internal static void HDRPPrompt()
        {
#if HDRP_1_OR_NEWER
            if (!IsRenderingWithHDRP || DoesHDRPSupportAlpha || LibraryPreferences.DidShowHDRPPrompt)
                return;
            
            var hdrpAsset = (HDRenderPipelineAsset) GraphicsSettings.currentRenderPipeline;

            bool changed = EditorUtility.DisplayDialog("Change HDRP render setting",
                "Would you like to change the 'Color Buffer Format' render setting of the HDRP asset to R16G16B16A16? " +
                "This is required to have previews in the library have transparent backgrounds.",
                "Change setting", "Do not change"
            );
            
            if (changed)
            {
                using (var hdrpSerializedObject = new SerializedObject(hdrpAsset))
                {
                    hdrpSerializedObject.FindProperty("m_RenderPipelineSettings.colorBufferFormat").intValue =
                        (int)RenderPipelineSettings.ColorBufferFormat.R16G16B16A16;
                    
                    hdrpSerializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
                    
                // It doesn't seem to update unless we import that asset.
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(hdrpAsset));
                AssetPreviewManager.DeleteAllPreviewTextures();
            }
            else
            {
                EditorUtility.DisplayDialog("Change later",
                    "If you change the setting later you can go to 'Preferences/Smart Library' and regenerate all previews so they will have transparent backgrounds.",
                    "Ok");
            }

            LibraryPreferences.DidShowHDRPPrompt = true;
#endif
        }
    }

}