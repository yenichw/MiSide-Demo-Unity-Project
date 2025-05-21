/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    public partial class ViewGallery
    {
        internal class ViewStateItem: ViewItem
        {
            public Vector3 pivot;
            public float size;
            public Quaternion rotation;
            public string title;
            public ViewState viewState;
            public bool is2D;
            public SceneView view;
            public bool renderUI = false;

            public override bool useInPreview
            {
                get
                {
                    if (viewState != null) return viewState.useInPreview;
                    return false;
                }
                set
                {
                    if (viewState != null) viewState.useInPreview = value;
                }
            }

            public override bool allowPreview
            {
                get { return viewState != null; }
            }

            public override string name
            {
                get { return title; }
            }

            public ViewStateItem()
            {

            }

            public ViewStateItem(SceneView view)
            {
                pivot = view.pivot;
                size = view.size;
                rotation = view.rotation;
                is2D = view.in2DMode;
            }

            public ViewStateItem(ViewState viewState)
            {
                this.viewState = viewState;
                pivot = viewState.pivot;
                size = viewState.size;
                rotation = viewState.rotation;
                title = viewState.title;
                is2D = viewState.is2D;
            }

            public override void PrepareMenu(GenericMenuEx menu)
            {
                menu.Add("Restore", RestoreViewState, this);

                if (viewState != null)
                {
                    menu.Add("Rename", RenameViewState, this);
                    menu.Add("Delete", RemoveViewState, this);
                }
                else menu.AddDisabled("Delete");

                if (viewState == null) menu.Add("Create View State", CreateViewState, this);
            }

            public void SetView(Camera camera)
            {
                Transform t = camera.transform;

                if (!is2D)
                {
                    camera.orthographic = false;
                    camera.fieldOfView = 60;
                    t.position = pivot - rotation * Vector3.forward * ViewState.GetPerspectiveCameraDistance(size, 60);
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

            public void SetView(SceneView view)
            {
                SetView(view.camera);
            }

            public override void Set(SceneView view)
            {
                view.in2DMode = is2D;
                view.pivot = pivot;
                view.size = size;
                if (!is2D)
                {
                    view.rotation = rotation;
                    view.camera.fieldOfView = 60;
                }
            }
        }
    }
}