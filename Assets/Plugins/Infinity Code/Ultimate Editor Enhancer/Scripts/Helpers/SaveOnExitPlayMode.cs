/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    [AddComponentMenu("")]
    public class SaveOnExitPlayMode : MonoBehaviour
    {
        public List<Component> saveComponents = new List<Component>();
    }
}