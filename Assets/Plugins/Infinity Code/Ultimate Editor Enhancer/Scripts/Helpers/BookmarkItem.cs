/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer
{
    [Serializable]
    public abstract class BookmarkItem : SearchableItem
    {
        public string title;
        public string type;
        public Object target;

        [NonSerialized]
        public string tooltip;

        [NonSerialized]
        public Texture preview;

        [NonSerialized]
        public bool previewLoaded;

        public GameObject gameObject
        {
            get
            {
                if (target == null) return null;
                if (target is Component) return (target as Component).gameObject;
                if (target is GameObject) return target as GameObject;
                return null;
            }
        }

        public abstract bool isProjectItem { get; }

        public virtual bool canBeRemoved
        {
            get => true;
        }

        public BookmarkItem()
        {

        }

        public BookmarkItem(Object obj)
        {
            target = obj;

            Type t = obj.GetType();
            type = t.AssemblyQualifiedName;

            if (obj is GameObject)
            {
                GameObject go = obj as GameObject;
                title = go.name;
                tooltip = GetTransformPath(go.transform).ToString();
            }
            else if (obj is Component)
            {
                GameObject go = (obj as Component).gameObject;
                title = go.name + " (" + t.Name + ")";
                tooltip = GetTransformPath(go.transform).ToString();
            }
            else
            {
                tooltip = title = obj.name;
            }
        }

        public static StringBuilder GetTransformPath(Transform t)
        {
            StringBuilder builder = StaticStringBuilder.Start();

            builder.Append(t.name);
            while ((t = t.parent) != null)
            {
                builder.Insert(0, '/');
                builder.Insert(0, t.name);
            }

            return builder;
        }

        public void Dispose()
        {
            target = null;
            preview = null;
        }

        protected override int GetSearchCount()
        {
            return 1;
        }

        protected override string GetSearchString(int index)
        {
            return title;
        }

        public abstract bool HasLabel(string label);

        public bool Update(string pattern, string valueType)
        {
            if (!string.IsNullOrEmpty(valueType) && !Contains(type, valueType)) return false;
            return Match(pattern);
        }
    }
}