/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    public partial class CreateBrowser
    {
        public class PrefabProvider: Provider
        {
            public override float order
            {
                get { return 1; }
            }

            public override string title
            {
                get { return instance.prefabsLabel; }
            }

            public override void Cache()
            {
                items = new List<Item>();

                string[] blacklist = Prefs.createBrowserBlacklist.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                CacheItems(blacklist, "t:prefab");
                CacheItems(blacklist, "t:model");

                foreach (Item item in items)
                {
                    PrefabItemFolder fi = item as PrefabItemFolder;
                    if (fi == null) continue;
                    fi.Simplify();
                }

                items = items.OrderBy(o =>
                {
                    if (o is FolderItem) return 0;
                    return -1;
                }).ThenBy(o => o.label).ToList();
            }

            private void CacheItems(string[] blacklist, string filter)
            {
                bool hasBlacklist = blacklist.Length > 0;

                string[] assets = AssetDatabase.FindAssets(filter, new[] {"Assets"});
                foreach (string guid in assets)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (hasBlacklist)
                    {
                        bool inBlackList = false;
                        for (int i = 0; i < blacklist.Length; i++)
                        {
                            string s = blacklist[i];
                            if (s.Length > assetPath.Length) continue;

                            int j;
                            for (j = 0; j < s.Length; j++)
                            {
                                if (s[i] != assetPath[i]) break;
                            }

                            if (j == assetPath.Length)
                            {
                                inBlackList = true;
                                break;
                            }
                        }

                        if (inBlackList) continue;
                    }

                    string shortPath = assetPath.Substring(7);
                    string[] parts = shortPath.Split('/');
                    if (parts.Length == 1)
                    {
                        if (shortPath.Length < 8) continue;
                        items.Add(new PrefabItem(shortPath, assetPath));
                    }
                    else
                    {
                        string label = parts[0];
                        int i;
                        for (i = 0; i < items.Count; i++)
                        {
                            Item item = items[i];
                            string l = item.label;
                            if (l.Length != label.Length) continue;

                            int j;
                            for (j = 0; j < l.Length; j++)
                            {
                                if (l[j] != label[j]) break;
                            }

                            if (j != l.Length) continue;

                            PrefabItemFolder f = item as PrefabItemFolder;
                            if (f != null) f.Add(parts, 0, assetPath);
                            break;
                        }

                        if (i == items.Count)
                        {
                            items.Add(new PrefabItemFolder(parts, 0, assetPath));
                        }
                    }
                }
            }
        }
    }
}