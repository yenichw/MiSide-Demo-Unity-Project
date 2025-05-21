/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.PostHeader
{
    [InitializeOnLoad]
    public static class PostHeaderManager
    {
        private static PostHeaderItem[] rowItems;
        private static PostHeaderItem[] blockItems;

        static PostHeaderManager()
        {
            Editor.finishedDefaultHeaderGUI += OnFinishedDefaultHeaderGUI;
            IEnumerable<Type> types = TypeCache.GetTypesDerivedFrom<PostHeaderItem>().Where(i => !i.IsAbstract);
            List<PostHeaderItem> tempItems = new List<PostHeaderItem>();
            foreach (Type type in types)
            {
                try
                {
                    PostHeaderItem item = Activator.CreateInstance(type) as PostHeaderItem;
                    tempItems.Add(item);
                }
                catch (Exception e)
                {
                    Log.Add(e);
                }
            }

            rowItems = tempItems.OrderBy(i => i.rowOrder).ToArray();
            blockItems = tempItems.OrderBy(i => i.blockOrder).ToArray();
        }

        private static void OnFinishedDefaultHeaderGUI(Editor editor)
        {
            if (editor.targets.Length > 1) return;

            Object target = editor.target;

            EditorGUILayout.BeginHorizontal();

            for (int i = 0; i < rowItems.Length; i++)
            {
                try
                {
                    rowItems[i].OnRowGUI(target);
                }
                catch (Exception e)
                {
                    Log.Add(e);
                }
            }

            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < blockItems.Length; i++)
            {
                try
                {
                    blockItems[i].OnBlockGUI(target);
                }
                catch (Exception e)
                {
                    Log.Add(e);
                }
            }
        }
    }
}