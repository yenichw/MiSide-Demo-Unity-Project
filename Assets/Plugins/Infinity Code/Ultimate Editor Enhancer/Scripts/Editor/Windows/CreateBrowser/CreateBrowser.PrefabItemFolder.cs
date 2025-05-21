/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    public partial class CreateBrowser
    {
        internal class PrefabItemFolder: FolderItem
        {
            private List<string> skippedLabels;

            public override string title
            {
                get
                {
                    if (skippedLabels == null) return label;
                    return skippedLabels.Last();
                }
            }

            public PrefabItemFolder(string[] parts, int index, string path)
            {
                children = new List<Item>();
                label = parts[index];

                if (parts.Length == index + 2)
                {
                    string part = parts[index + 1];
                    if (part.Length < 8) return;

                    PrefabItem child = new PrefabItem(part, path);
                    child.parent = this;
                    children.Add(child);
                }
                else
                {
                    PrefabItemFolder child = new PrefabItemFolder(parts, index + 1, path);
                    child.parent = this;
                    children.Add(child);
                }
            }

            public void Add(string[] parts, int index, string path)
            {
                string next = parts[index + 1];
                int nl = next.Length;

                int i;
                int count = children.Count;
                for (i = 0; i < count; i++)
                {
                    Item c = children[i];
                    string l = c.label;
                    if (nl != l.Length) continue;

                    int j;
                    for (j = 0; j < nl; j++)
                    {
                        if (l[j] != next[j]) break;
                    }

                    if (j != nl) continue;

                    PrefabItemFolder f = c as PrefabItemFolder;
                    if (f != null) f.Add(parts, index + 1, path);
                    break;
                }

                if (i == count)
                {
                    if (parts.Length == index + 2)
                    {
                        if (nl < 8) return;

                        PrefabItem child = new PrefabItem(next, path);
                        child.parent = this;
                        children.Add(child);
                    }
                    else
                    {
                        PrefabItemFolder child = new PrefabItemFolder(parts, index + 1, path);
                        child.parent = this;
                        children.Add(child);
                    }
                }
            }

            protected override void InitContent()
            {
                if (folderIconContent == null) folderIconContent = EditorIconContents.folder;
                _content = new GUIContent(folderIconContent);
                if (skippedLabels != null)
                {
                    if (skippedLabels.Count == 1) _content.text = label + "/" + skippedLabels[0];
                    else _content.text = label + "/.../" + skippedLabels.Last();

                    StringBuilder builder = StaticStringBuilder.Start();

                    builder.Append(label);
                    foreach (string s in skippedLabels) builder.Append("/").Append(s);
                    _content.tooltip = builder.ToString();
                }
                else
                {
                    _content.text = label;
                    _content.tooltip = label;
                }
            }

            public void Simplify()
            {
                if (children.Count == 1)
                {
                    PrefabItemFolder fi = children[0] as PrefabItemFolder;
                    if (fi != null)
                    {
                        children = fi.children;
                        foreach (Item child in children) child.parent = this;
                        if (skippedLabels == null) skippedLabels = new List<string>();
                        skippedLabels.Add(fi.label);
                        fi.children = null;
                        fi.Dispose();
                        Simplify();
                    }
                }
                else
                {
                    foreach (Item child in children)
                    {
                        PrefabItemFolder fi = child as PrefabItemFolder;
                        if (fi != null) fi.Simplify();
                    }
                }
            }
        }
    }
}