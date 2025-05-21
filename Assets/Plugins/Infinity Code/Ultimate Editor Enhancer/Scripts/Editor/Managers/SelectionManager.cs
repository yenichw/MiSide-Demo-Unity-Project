/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer
{
    [InitializeOnLoad]
    public static class SelectionManager
    {
        public static Action OnChanged;

        public static Object[] prevSelection { get; private set; }
        public static Object[] currentSelection { get; private set; }

        private static bool ignoreChanged = false;

        static SelectionManager()
        {
            Selection.selectionChanged += OnSelectionChanged;
        }

        private static void OnSelectionChanged()
        {
            if (ignoreChanged) return;

            prevSelection = currentSelection;
            currentSelection = Selection.objects;

            if (OnChanged != null) OnChanged();
        }

        public static void SetSelection(Object[] current, Object[] prev = null)
        {
            ignoreChanged = true;
            
            if (prev != null) prevSelection = prev;
            else prevSelection = currentSelection;

            Selection.objects = current;
            
            ignoreChanged = false;
        }
    }
}