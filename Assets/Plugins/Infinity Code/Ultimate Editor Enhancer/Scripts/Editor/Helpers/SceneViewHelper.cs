/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static class SceneViewHelper
    {
        public static void AlignViewToCamera(Camera camera, SceneView view = null)
        {
            if (camera == null) return;
            if (view == null) view = SceneView.lastActiveSceneView;
            Transform t = camera.transform;
            view.in2DMode = false;
            view.AlignViewToObject(t);
        }
    }
}