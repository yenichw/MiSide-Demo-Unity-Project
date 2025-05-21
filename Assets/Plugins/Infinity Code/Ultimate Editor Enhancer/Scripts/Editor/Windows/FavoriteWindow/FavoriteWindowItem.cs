/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using InfinityCode.UltimateEditorEnhancer.JSON;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    [Serializable]
    public class FavoriteWindowItem
    {
        public string title;
        public string className;

        public JsonObject json
        {
            get
            {
                JsonObject obj = new JsonObject();
                obj.Add("title", title);
                obj.Add("className", className);
                return obj;
            }
            set
            {
                className = value.V<string>("class");
                title = value.V<string>("title");
            }
        }

        public FavoriteWindowItem(EditorWindow window)
        {
            Type type = window.GetType();
            className = type.AssemblyQualifiedName;
            title = window.titleContent.text;
        }

        public FavoriteWindowItem(JsonItem item)
        {
            json = item as JsonObject;
        }

        public void Open()
        {
            Type type = Type.GetType(className);
            if (type == null) EditorUtility.DisplayDialog("Error", "Can't find the window class. Please delete the entry and add again.", "OK");
            else EditorWindow.GetWindow(type, false, title);
        }
    }
}