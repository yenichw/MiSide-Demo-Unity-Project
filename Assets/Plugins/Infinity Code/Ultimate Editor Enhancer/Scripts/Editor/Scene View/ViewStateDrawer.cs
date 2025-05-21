/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Linq;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.SceneTools
{
    [InitializeOnLoad]
    public static class ViewStateDrawer
    {
        private static Camera[] cameras;
        private static Texture2D emptyTexture;
        private static Texture2D eyeTexture;
        private static double lastUpdateTime = double.MinValue;
        private static ViewStateWrapper[] states;
        private static GUIContent viewContent;
        private static ViewState[] viewStates;
        private static GUIStyle viewStyle;

        static ViewStateDrawer()
        {
            SceneViewManager.AddListener(DrawViewStates, SceneViewOrder.Normal, true);
        }

        private static void DrawViewStates(SceneView sceneView)
        {
            if (!Prefs.showViewStateInScene) return;

            Event e = Event.current;
            if (!e.alt)
            {
                cameras = null;
                viewStates = null;
                return;
            }

            if (e.type == EventType.Layout && 
                (viewStates == null || EditorApplication.timeSinceStartup - lastUpdateTime > 10))
            {
                viewStates = ObjectHelper.FindObjectsOfType<ViewState>();
                cameras = ObjectHelper.FindObjectsOfType<Camera>();
                lastUpdateTime = EditorApplication.timeSinceStartup;

                int countStates = viewStates.Length + cameras.Length;

                if (states == null) states = new ViewStateWrapper[countStates];
                else if (states.Length < countStates) states = new ViewStateWrapper[countStates];
            }

            if (viewStates == null || cameras == null || viewStates.Length + cameras.Length == 0) return;

            Camera camera = sceneView.camera;
            Vector3 cameraPosition = camera.transform.position;

            Handles.BeginGUI();

            for (int i = 0; i < viewStates.Length; i++)
            {
                ViewState state = viewStates[i];
                Vector3 position = state.position;
                float magnitude = (position - cameraPosition).magnitude;

                Vector2 point = HandleUtility.WorldToGUIPoint(position);
                states[i] = new ViewStateWrapper
                {
                    state = state,
                    screenPoint = point,
                    position = position,
                    distance = magnitude
                };
            }

            for (int i = 0, j = viewStates.Length; i < cameras.Length; i++, j++)
            {
                Camera cam = cameras[i];
                if (cam == null) continue;
                
                Vector3 position = cam.transform.position;
                float magnitude = (position - cameraPosition).magnitude;

                Vector2 point = HandleUtility.WorldToGUIPoint(position);
                states[j] = new ViewStateWrapper
                {
                    camera = cam,
                    screenPoint = point,
                    position = position,
                    distance = magnitude
                };
            }

            if (viewContent == null)
            {
                eyeTexture = Resources.LoadIcon("Eye");
                emptyTexture = Resources.CreateTexture(64, 64, new Color(0, 0, 0, 0));
                viewContent = new GUIContent();
            }

            if (viewStyle == null)
            {
                viewStyle = new GUIStyle
                {
                    imagePosition = ImagePosition.ImageAbove, 
                    alignment = TextAnchor.MiddleCenter, 
                    normal =
                    {
                        textColor = Color.white
                    }
                };
            }

            Vector2 mousePosition = e.mousePosition;
            bool used = false;

            foreach (ViewStateWrapper w in states.OrderByDescending(s => s.distance))
            {
                Vector3 vp = camera.WorldToViewportPoint(w.position);
                if (vp.x < 0 || vp.y < 0 || vp.x > 1 || vp.y > 1 || vp.z < 0) continue;

                viewContent.text = w.title + "\nDistance: " + w.distance.ToString("F0") + " meters";
                Rect rect = new Rect(w.screenPoint.x - 24, w.screenPoint.y - 12, 48, 48);

                if (rect.Contains(mousePosition)) GUI.color = new Color32(211, 211, 211, 255);

                if (w.state != null)
                {
                    viewContent.image = eyeTexture;
                }
                else
                {
                    viewContent.image = emptyTexture;
                }

                if (GUI.Button(rect, viewContent, viewStyle) && !used)
                {
                    w.SetTo(sceneView);
                    used = true;
                }

                GUI.color = Color.white;

                w.Dispose();
            }

            Handles.EndGUI();
        }

        internal class ViewStateWrapper
        {
            public ViewState state;
            public Camera camera;
            public Vector2 screenPoint;
            public float distance;
            public Vector3 position;

            public string title
            {
                get
                {
                    if (state != null) return state.title;
                    if (camera != null) return camera.gameObject.name;
                    return "";
                }
            }

            public void Dispose()
            {
                camera = null;
                state = null;
            }

            public void SetTo(SceneView view)
            {
                if (state != null)
                {
                    view.orthographic = state.is2D;
                    view.pivot = state.pivot;
                    view.size = state.size;
                    view.rotation = state.rotation;
                }
                else if (camera != null)
                {
                    SceneViewHelper.AlignViewToCamera(camera, view);
                }
            }
        }
    }
}