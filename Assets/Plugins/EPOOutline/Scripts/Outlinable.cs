using System;
using System.Collections.Generic;
using UnityEngine;

namespace EPOOutline
{
    /// <summary>
    /// Component which holds the outline settings for a specific object.
    /// </summary>
    [ExecuteAlways]
    public partial class Outlinable : MonoBehaviour
    {
        private static HashSet<Outlinable> outlinables = new HashSet<Outlinable>();

        [SerializeField]
        private ComplexMaskingMode complexMaskingMode;
        
        [SerializeField]
        private OutlinableDrawingMode drawingMode = OutlinableDrawingMode.Normal;

        [SerializeField]
        private int outlineLayer = 0;

        [SerializeField]
        private List<OutlineTarget> outlineTargets = new List<OutlineTarget>();

        [SerializeField]
        private RenderStyle renderStyle = RenderStyle.Single;

#pragma warning disable CS0649
        [SerializeField]
        private OutlineProperties outlineParameters = new OutlineProperties();

        [SerializeField]
        private OutlineProperties backParameters = new OutlineProperties();

        [SerializeField]
        private OutlineProperties frontParameters = new OutlineProperties();

        private bool shouldValidateTargets = false;
        
#pragma warning restore CS0649

        /// <summary>
        /// <see cref="EPOOutline.RenderStyle"/> to render the outlinable.
        /// </summary>
        public RenderStyle RenderStyle
        {
            get => renderStyle;
            set => renderStyle = value;
        }

        /// <summary>
        /// <see cref="EPOOutline.ComplexMaskingMode"/> to render the outlinable.
        /// </summary>
        public ComplexMaskingMode ComplexMaskingMode
        {
            get => complexMaskingMode;
            set => complexMaskingMode = value;
        }

        /// <summary>
        /// <see cref="EPOOutline.OutlinableDrawingMode"/> to render the outlinable.
        /// </summary>
        public OutlinableDrawingMode DrawingMode
        {
            get => drawingMode;
            set => drawingMode = value;
        }

        /// <summary>
        /// The layer of the outlinable.
        /// </summary>
        public int OutlineLayer
        {
            get => outlineLayer;
            set => outlineLayer = value;
        }

        /// <summary>
        /// The list of the outline targets of the outlinable.
        /// <br/>Note that it's immutable. To manage the targets, please use:
        /// <br/><see cref="EPOOutline.Outlinable.AddRenderer"/>
        /// <br/><see cref="EPOOutline.Outlinable.AddTarget"/>
        /// <br/><see cref="EPOOutline.Outlinable.RemoveTarget"/>
        /// </summary>
        public IReadOnlyList<OutlineTarget> OutlineTargets => outlineTargets;

        /// <summary>
        /// The parameters of the outline that are used when <see cref="EPOOutline.Outlinable.RenderStyle"/> is set to <see cref="EPOOutline.RenderStyle.Single"/>.
        /// </summary>
        public OutlineProperties OutlineParameters => outlineParameters;

        /// <summary>
        /// The parameters of the outline that are used for the front rendering when <see cref="EPOOutline.Outlinable.RenderStyle"/> is set to <see cref="EPOOutline.RenderStyle.FrontBack"/>.
        /// </summary>
        public OutlineProperties FrontParameters => frontParameters;

        /// <summary>
        /// The parameters of the outline that are used for the back rendering when <see cref="EPOOutline.Outlinable.RenderStyle"/> is set to <see cref="EPOOutline.RenderStyle.FrontBack"/>.
        /// </summary>
        public OutlineProperties BackParameters => backParameters;

        internal bool NeedsFillMask
        {
            get
            {
                if ((drawingMode & OutlinableDrawingMode.Normal) == 0)
                    return false;

                if (renderStyle != RenderStyle.FrontBack) 
                    return false;
                
                return (frontParameters.Enabled || backParameters.Enabled) && (frontParameters.FillPass.Material != null || backParameters.FillPass.Material != null);

            }
        }

        /// <summary>
        /// Adds renderer with all its sub-meshes to the targets list.
        /// </summary>
        /// <param name="rendererToAdd">The renderer to be added to the list.</param>
        /// <param name="targetProvider">The optional function that provides the <see cref="EPOOutline.OutlineTarget"/> in case if some configuration is needed</param>
        public void AddRenderer(Renderer rendererToAdd, OutlineTargetProvider targetProvider = null)
        {
            var submeshCount = RendererUtility.GetSubmeshCount(rendererToAdd);
            for (var index = 0; index < submeshCount; index++)
            {
                var target = targetProvider == null
                    ? new OutlineTarget(rendererToAdd, index)
                    : targetProvider(rendererToAdd, index);
                
                AddTarget(target);
            }
        }

        [Obsolete("It's obsolete and will be removed. Use AddTarget instead")]
        public void TryAddTarget(OutlineTarget target)
        {
            AddTarget(target);
        }
        
        /// <summary>
        /// Adds <see cref="EPOOutline.OutlineTarget"/> to the list of targets.
        /// </summary>
        /// <param name="target">The target to add to the list.</param>
        public void AddTarget(OutlineTarget target)
        {
            outlineTargets.Add(target);
            ValidateTargets();
        }

        /// <summary>
        /// Removes <see cref="EPOOutline.OutlineTarget"/> from the list of targets.
        /// </summary>
        /// <param name="target">The target to remove.</param>
        public void RemoveTarget(OutlineTarget target)
        {
            outlineTargets.Remove(target);
            if (target.renderer != null)
            {
                var listener = target.renderer.GetComponent<TargetStateListener>();
                if (listener == null)
                    return;
                
                listener.RemoveCallback(this, UpdateVisibility);
            }
        }
        
        /// <summary>
        /// Gets or sets the <see cref="EPOOutline.OutlineTarget"/> at the specific index.
        /// </summary>
        /// <param name="index">The index to get or set the target to/from.</param>
        public OutlineTarget this[int index]
        {
            get => outlineTargets[index];

            set
            {
                outlineTargets[index] = value;
                ValidateTargets();
            }
        }

        /// <summary>
        /// Provides the outline targets count.
        /// </summary>
        public int OutlineTargetsCount => outlineTargets.Count;

        private void Reset()
        {
            AddAllChildRenderersToRenderingList(RenderersAddingMode.SkinnedMeshRenderer | RenderersAddingMode.MeshRenderer | RenderersAddingMode.SpriteRenderer);
        }

        private void OnValidate()
        {
            outlineLayer = Mathf.Clamp(outlineLayer, 0, 63);
            shouldValidateTargets = true;
        }

        private void SubscribeToVisibilityChange(GameObject go)
        {
            var listener = go.GetComponent<TargetStateListener>();
            if (listener == null)
            {
                listener = go.AddComponent<TargetStateListener>();
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(listener);
                UnityEditor.EditorUtility.SetDirty(go);
#endif
            }

            listener.RemoveCallback(this, UpdateVisibility);
            listener.AddCallback(this, UpdateVisibility);

            listener.ForceUpdate();
        }

        private void UpdateVisibility()
        {
            if (!enabled)
            {
                outlinables.Remove(this);
                return;
            }

            outlineTargets.RemoveAll(x => x.renderer == null);
            for (var index = 0; index < OutlineTargets.Count; index++)
            {
                var target = OutlineTargets[index];
                target.IsVisible = target.renderer.isVisible;
            }

            outlineTargets.RemoveAll(x => x.renderer == null);

            foreach (var target in outlineTargets)
            {
                if (target.IsVisible)
                {
                    outlinables.Add(this);
                    return;
                }
            }

            outlinables.Remove(this);
        }

        private void OnEnable()
        {
            UpdateVisibility();
        }

        private void OnDisable()
        {
            outlinables.Remove(this);
        }

        private void Awake()
        {
            ValidateTargets();
        }

        private void ValidateTargets()
        {
            outlineTargets.RemoveAll(x => x.renderer == null);
            foreach (var target in outlineTargets)
                SubscribeToVisibilityChange(target.renderer.gameObject);
        }

        private void OnDestroy()
        {
            outlinables.Remove(this);
        }
        
        public static void GetAllActiveOutlinables(List<Outlinable> outlinablesList)
        {
            outlinablesList.Clear();
            foreach (var outlinable in outlinables)
                outlinablesList.Add(outlinable);
        }

        /// <summary>
        /// Adds all child renderers to the rendering list of the outlinable.
        /// </summary>
        /// <param name="renderersAddingMode"><see cref="EPOOutline.RenderersAddingMode"/> that will be used during the adding process.</param>
        public void AddAllChildRenderersToRenderingList(RenderersAddingMode renderersAddingMode = RenderersAddingMode.All)
        {
            outlineTargets.Clear();
            var renderers = GetComponentsInChildren<Renderer>(true);
            foreach (var rendererToAdd in renderers)
            {
                if (!MatchingMode(rendererToAdd, renderersAddingMode))
                    continue;

                var submeshesCount = RendererUtility.GetSubmeshCount(rendererToAdd);
                for (var index = 0; index < submeshesCount; index++)
                    AddTarget(new OutlineTarget(rendererToAdd, index));
            }
        }

        private void Update()
        {
            if (!shouldValidateTargets)
                return;

            shouldValidateTargets = false;
            ValidateTargets();
        }

        private bool MatchingMode(Renderer rendererToMatch, RenderersAddingMode mode)
        {
            return 
                (!(rendererToMatch is MeshRenderer) && !(rendererToMatch is SkinnedMeshRenderer) && !(rendererToMatch is SpriteRenderer) && (mode & RenderersAddingMode.Others) != RenderersAddingMode.None) ||
                (rendererToMatch is MeshRenderer && (mode & RenderersAddingMode.MeshRenderer) != RenderersAddingMode.None) ||
                (rendererToMatch is SpriteRenderer && (mode & RenderersAddingMode.SpriteRenderer) != RenderersAddingMode.None) ||
                (rendererToMatch is SkinnedMeshRenderer && (mode & RenderersAddingMode.SkinnedMeshRenderer) != RenderersAddingMode.None);
        }

#if UNITY_EDITOR
        public void OnDrawGizmosSelected()
        {
            foreach (var target in outlineTargets)
            {
                if (target.Renderer == null)
                    continue;
                
                if (target.BoundsMode != BoundsMode.Manual)
                    continue;

                var bounds = target.BoundsMode != BoundsMode.Manual ? target.Renderer.bounds : target.Bounds;
                
                Gizmos.matrix = target.Renderer.transform.localToWorldMatrix;

                Gizmos.color = new Color(1.0f, 0.5f, 0.0f, 0.2f);
                var size = bounds.size;

                if (target.BoundsMode == BoundsMode.Manual)
                {
                    var scale = target.Renderer.transform.localScale;
                    size.x /= scale.x;
                    size.y /= scale.y;
                    size.z /= scale.z;
                }

                Gizmos.DrawCube(bounds.center, size);
                Gizmos.DrawWireCube(bounds.center, size);
            }
        }
#endif
    }
}