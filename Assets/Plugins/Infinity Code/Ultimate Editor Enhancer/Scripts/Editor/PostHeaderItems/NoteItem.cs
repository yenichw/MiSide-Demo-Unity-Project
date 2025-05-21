/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.PostHeader
{
    [Serializable]
    public class NoteItem
    {
        public string text;
        public string gid;
        public bool expanded;
        
        [NonSerialized]
        public Vector2 scrollPosition;
        
        [NonSerialized]
        public float maxHeight = 100;

        public bool isEmpty
        {
            get => string.IsNullOrEmpty(text);
        }
    }
}