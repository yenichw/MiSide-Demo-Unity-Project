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
        internal class CreateItemFolder : FolderItem
        {
            private string tooltip;

            public CreateItemFolder(string[] parts, int index, string submenu)
            {
                children = new List<Item>();
                label = parts[index];

                StringBuilder builder = StaticStringBuilder.Start();
                builder.Append("GameObject");
                for (int i = 0; i <= index; i++)
                {
                    builder.Append("/").Append(parts[i]);
                }

                tooltip = builder.ToString();

                if (parts.Length == index + 2)
                {
                    CreateItem child = new CreateItem(parts[index + 1], submenu);
                    child.parent = this;
                    children.Add(child);
                }
                else
                {
                    CreateItemFolder child = new CreateItemFolder(parts, index + 1, submenu);
                    child.parent = this;
                    children.Add(child);
                }
            }

            public void Add(string[] parts, int index, string submenu)
            {
                string next = parts[index + 1];
                CreateItemFolder folder = children.FirstOrDefault(c => c.label == next) as CreateItemFolder;
                if (folder != null) folder.Add(parts, index + 1, submenu);
                else
                {
                    if (parts.Length == index + 2)
                    {
                        CreateItem child = new CreateItem(parts[index + 1], submenu);
                        child.parent = this;
                        children.Add(child);
                    }
                    else
                    {
                        CreateItemFolder child = new CreateItemFolder(parts, index + 1, submenu);
                        child.parent = this;
                        children.Add(child);
                    }
                }
            }

            protected override void InitContent()
            {
                if (folderIconContent == null) folderIconContent = EditorIconContents.folder;
                _content = new GUIContent(folderIconContent);
                _content.text = label;
                _content.tooltip = tooltip;
            }
        }
    }
}