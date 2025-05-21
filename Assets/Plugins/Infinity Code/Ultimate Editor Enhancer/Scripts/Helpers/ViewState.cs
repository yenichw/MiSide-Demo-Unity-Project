/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer 
{
    [AddComponentMenu("")]
    public class ViewState: MonoBehaviour
    {
        public Vector3 pivot;
        public float size;
        public Quaternion rotation;
        public string title;
        public bool is2D;
        public bool useInPreview = true;

        public Vector3 position
        {
            get
            {
                if (is2D)
                {
                    return pivot - Vector3.forward * size;
                }

                return pivot - rotation * Vector3.forward * GetPerspectiveCameraDistance(size, 60);
            }
        }

        public static float GetPerspectiveCameraDistance(float size, float fov)
        {
            return size / Mathf.Sin((float)(fov * 0.5 * (Math.PI / 180.0)));
        }

        public void SetView(Camera camera)
        {
            Transform t = camera.transform;

            if (!is2D)
            {
                camera.orthographic = false;
                camera.fieldOfView = 60;
                t.position = pivot - rotation * Vector3.forward * GetPerspectiveCameraDistance(size, 60);
                t.rotation = rotation;
            }
            else
            {
                camera.orthographic = true;
                camera.orthographicSize = size;
                t.position = pivot - Vector3.forward * size;
                t.rotation = Quaternion.identity;
            }
        }
    }
}