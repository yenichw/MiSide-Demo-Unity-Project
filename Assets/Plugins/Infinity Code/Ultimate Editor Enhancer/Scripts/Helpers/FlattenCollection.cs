/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    [AddComponentMenu("")]
    [DefaultExecutionOrder(-30001)]
    public class FlattenCollection : MonoBehaviour
    {
        public bool flatten = false;
        public bool removeInPlayMode = false;

        private void Awake()
        {
            if (flatten)
            {
                Transform parent = transform.parent;
                int si = transform.GetSiblingIndex();

                while (transform.childCount > 0)
                {
                    Transform child = transform.GetChild(transform.childCount - 1);
                    child.SetParent(parent, true);
                    child.SetSiblingIndex(si);
                }
            }

            if (removeInPlayMode) Destroy(gameObject);
        }
    }
}