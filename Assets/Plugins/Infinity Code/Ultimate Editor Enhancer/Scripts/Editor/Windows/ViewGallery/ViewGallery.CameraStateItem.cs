/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    public partial class ViewGallery
    {
        public class CameraStateItem : ViewItem
        {
            public Camera camera;
            private bool _useInPreview = true;

            public override bool useInPreview
            {
                get { return _useInPreview; }
                set
                {
                    if (_useInPreview == value) return;

                    _useInPreview = value;
                    if (!value)
                    {
                        if (camera.gameObject.GetComponent<HideInPreview>() == null) camera.gameObject.AddComponent<HideInPreview>();
                    }
                    else DestroyImmediate(camera.gameObject.GetComponent<HideInPreview>());
                }
            }

            public override bool allowPreview
            {
                get { return true; }
            }

            public override string name
            {
                get
                {
                    if (camera != null) return camera.name;
                    return string.Empty;
                }
            }

            public CameraStateItem(Camera camera)
            {
                this.camera = camera;
                _useInPreview = camera.gameObject.GetComponent<HideInPreview>() == null;
            }

            public override void PrepareMenu(GenericMenuEx menu)
            {
                menu.Add("Restore", Set);

                foreach (ViewStateItem view in views)
                {
                    menu.Add("Set From State/" + view.name, SetState, view);
                }

                menu.Add("Select", () => Selection.activeGameObject = camera.gameObject);
                menu.Add("Create View State", CreateViewState, this);
            }

            private void SetState(object userData)
            {
                ViewStateItem view = userData as ViewStateItem;
                if (view == null) return;

                view.SetView(camera);
            }

            public override void Set(SceneView view)
            {
                SceneViewHelper.AlignViewToCamera(camera);
                GetWindow<SceneView>();
                isDirty = true;
                GUI.changed = true;
            }
        }
    }
}