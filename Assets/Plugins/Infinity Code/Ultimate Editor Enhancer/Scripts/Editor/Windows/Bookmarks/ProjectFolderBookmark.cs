/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    public class ProjectFolderBookmark
    {
        public ProjectBookmark bookmark;
        public Item[] items;

        public ProjectFolderBookmark(ProjectBookmark bookmark)
        {
            this.bookmark = bookmark;
            InitContent();
        }

        public void Dispose()
        {
            bookmark = null;
            for (int i = 0; i < items.Length; i++) items[i].Dispose();
            items = null;
        }

        private void InitContent()
        {
            string assetPath = bookmark.path;
            string[] entries = Directory.GetFiles(assetPath, "*.*", SearchOption.AllDirectories);

            List<Item> temp = new List<Item>();

            for (int i = 0; i < entries.Length; i++)
            {
                string entry = entries[i];
                if (entry.EndsWith(".meta")) continue;

                temp.Add(new Item(entry));
            }

            items = temp.ToArray();
        }

        public class Item: BookmarkItem
        {
            public readonly string path;
            public readonly string[] labels;
            public readonly string filename;
            public readonly string shortFilename;

            public override bool canBeRemoved
            {
                get => false;
            }

            public override bool isProjectItem
            {
                get => true;
            }

            public Item(string path)
            {
                this.path = path;
                target = AssetDatabase.LoadMainAssetAtPath(path);
                if (target != null)
                {
                    if (target != null) type = target.GetType().AssemblyQualifiedName;
                    labels = AssetDatabase.GetLabels(target);
                }

                FileInfo info = new FileInfo(path);
                filename = info.Name;
                if (info.Extension.Length > 0) shortFilename = filename.Substring(0, filename.Length - info.Extension.Length);
                else shortFilename = filename;

                title = shortFilename;
                tooltip = path;

            }

            protected override int GetSearchCount()
            {
                return 1;
            }

            protected override string GetSearchString(int index)
            {
                return shortFilename;
            }

            public override bool HasLabel(string label)
            {
                if (labels == null) return false;
                return labels.Contains(label);
            }
        }
    }
}