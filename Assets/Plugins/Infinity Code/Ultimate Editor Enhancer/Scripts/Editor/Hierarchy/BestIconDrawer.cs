/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.HierarchyTools
{
    [InitializeOnLoad]
    public static class BestIconDrawer
    {
        private const double CacheLifeTimeSec = 5;
        
        private static Texture _prefabIcon;
        private static Texture _prefabVariantIcon;
        private static Texture _unityLogoTexture;
        private static HashSet<int> hierarchyWindows;
        private static bool inited = false;
        private static Dictionary<int, CachedTexture> cachedTextures = new Dictionary<int, CachedTexture>();
        private static double lastUpdateTime;

        private static Texture prefabIcon
        {
            get
            {
                if (_prefabIcon == null) _prefabIcon = EditorIconContents.prefab.image;
                return _prefabIcon;
            }
        }

        private static Texture prefabVariantIcon
        {
            get
            {
                if (_prefabVariantIcon == null) _prefabVariantIcon = EditorIconContents.prefabVariant.image;
                return _prefabVariantIcon;
            }
        }

        private static Texture unityLogoTexture
        {
            get
            {
                if (_unityLogoTexture == null) _unityLogoTexture = EditorIconContents.unityLogo.image;
                return _unityLogoTexture;
            }
        }

        static BestIconDrawer()
        {
            hierarchyWindows = new HashSet<int>();
            HierarchyItemDrawer.Register("BestIconDrawer", DrawItem, HierarchyToolOrder.BestIcon);

            lastUpdateTime = EditorApplication.timeSinceStartup;
        }
        
        private static void DrawItem(HierarchyItem item)
        {
            if (!Prefs.hierarchyOverrideMainIcon) return;
            if (!inited) Init();

            Event e = Event.current;

            if (e.type == EventType.Layout)
            {
                EditorWindow lastHierarchyWindow = SceneHierarchyWindowRef.GetLastInteractedHierarchy();
                int wid = lastHierarchyWindow.GetInstanceID();
                if (!hierarchyWindows.Contains(wid)) InitWindow(lastHierarchyWindow, wid);
                return;
            }

            if (e.type != EventType.Repaint) return;

            Texture texture;
            if (!GetTexture(item, out texture)) return;
            if (texture == null) return;

            const int iconSize = 16;

            Rect rect = item.rect;
            Rect iconRect = new Rect(rect) {width = iconSize, height = iconSize};
            iconRect.y += (rect.height - iconSize) / 2;
            GUI.DrawTexture(iconRect, texture, ScaleMode.ScaleToFit);
        }

        private static void FirstInit(int id, Rect rect)
        {
            EditorApplication.hierarchyWindowItemOnGUI -= FirstInit;
            Init();
        }

        private static Component GetBestComponent(GameObject go)
        {
            Component[] components = go.GetComponents<Component>();
            if (components.Length == 1) return components[0];
            
            Component best = components[1];
            if (components.Length == 2) return best;

            if (!(best is CanvasRenderer)) return best;
            
            best = components[2];
            if (components.Length == 3 || !(best is UnityEngine.UI.Image)) return best;
            
            Component c = components[3];
            Texture texture = AssetPreview.GetMiniThumbnail(c);
            if (texture == null) return best;
            
            string textureName = texture.name;
            if (textureName == "cs Script Icon" || textureName == "d_cs Script Icon") return best;
            
            return c;
        }

        public static Texture GetGameObjectIcon(GameObject go)
        {
            if (go.tag == "Collection") return Icons.collection;

            Texture texture = AssetPreview.GetMiniThumbnail(go);
            string textureName = texture.name;

            if (textureName == "d_Prefab Icon" || textureName == "Prefab Icon")
            {
                if (PrefabUtility.IsAnyPrefabInstanceRoot(go)) return prefabIcon;
            }
            else if (textureName == "PrefabVariant Icon" || textureName == "d_PrefabVariant Icon")
            {
                if (PrefabUtility.IsAnyPrefabInstanceRoot(go)) return prefabVariantIcon;
            }
            else if (textureName != "d_GameObject Icon" && textureName != "GameObject Icon")
            {
                return texture;
            }

            Component best = GetBestComponent(go);
            if (best && best.GetType().FullName == "Unity.Scenes.SubScene") return EditorIconContents.unityLogo.image;
            texture = AssetPreview.GetMiniThumbnail(best);

            if (texture == null) return EditorIconContents.gameObject.image;
            return texture;
        }

        private static bool GetTexture(HierarchyItem item, out Texture texture)
        {
            texture = null;
            CachedTexture cachedTexture;
            if (cachedTextures.TryGetValue(item.id, out cachedTexture))
            {
                if (EditorApplication.timeSinceStartup - cachedTexture.time < CacheLifeTimeSec)
                {
                    texture = cachedTexture.texture;
                    return true; 
                }
                cachedTextures.Remove(item.id);
            }

            if (item.gameObject != null)
            {
                texture = GetGameObjectIcon(item.gameObject);
                cachedTextures.Add(item.id, new CachedTexture
                {
                    texture = texture,
                    time = EditorApplication.timeSinceStartup
                });
            }
            else if (item.target == null) texture = unityLogoTexture;
            else return false;

            return true;
        }

        private static void Init()
        {
            inited = true;
            Object[] windows = UnityEngine.Resources.FindObjectsOfTypeAll(SceneHierarchyWindowRef.type);
            foreach (EditorWindow window in windows)
            {
                int wid = window.GetInstanceID();
                if (!hierarchyWindows.Contains(wid))
                {
                    InitWindow(window, wid);
                }
            }
        }

        private static void InitWindow(EditorWindow window, int wid)
        {
            try
            {
                IMGUIContainer container = window.rootVisualElement.parent.Query<IMGUIContainer>().First();
                container.onGUIHandler = (() => OnGUIBefore(wid)) + container.onGUIHandler;
                HierarchyHelper.SetDefaultIconsSize(window);
                hierarchyWindows.Add(wid);
            }
            catch
            {
                
            }
        }

        private static void OnGUIBefore(int wid)
        {
            if (!Prefs.hierarchyOverrideMainIcon) return;
            if (Event.current.type != EventType.Layout) return;

            EditorWindow w = EditorUtility.InstanceIDToObject(wid) as EditorWindow;
            if (w != null) HierarchyHelper.SetDefaultIconsSize(w);
        }
        
        internal class CachedTexture
        {
            public Texture texture;
            public double time;
        }
    }
}