using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace EPOOutline
{
    /// <summary>
    /// The target to render outline for
    /// </summary>
    [System.Serializable]
    public class OutlineTarget
    {
        private static List<Material> TempSharedMaterials = new List<Material>();
        
        internal bool IsVisible = false;

        /// <summary>
        /// <see cref="EPOOutline.ColorMask"/> to use during the cutout process.
        /// </summary>
        [SerializeField]
        public ColorMask CutoutMask = ColorMask.A;

        [SerializeField]
        internal Renderer renderer;

        /// <summary>
        /// Sub-mesh index of the renderer.
        /// </summary>
        [SerializeField]
        public int SubmeshIndex;

        /// <summary>
        /// <see cref="EPOOutline.BoundsMode"/> to use for this target.
        /// </summary>
        [SerializeField]
        public BoundsMode BoundsMode = BoundsMode.Default;

        /// <summary>
        /// Bounds of this target that will be used for rendering.
        /// <br/>Only applicable if <see cref="EPOOutline.OutlineTarget.BoundsMode"/> is set to <see cref="EPOOutline.BoundsMode.Manual"/>
        /// </summary>
        [SerializeField]
        public Bounds Bounds = new Bounds(Vector3.zero, Vector3.one);

        /// <summary>
        /// The threshold for the cutout to be used.
        /// </summary>
        [SerializeField]
        [Range(0.0f, 1.0f)]
        public float CutoutThreshold = 0.5f;

        /// <summary>
        /// The <see cref="UnityEngine.Rendering.CullMode"/> to be used for rendering of this target.
        /// </summary>
        [SerializeField]
        public CullMode CullMode;

        [SerializeField]
        private string cutoutTextureName;

        [SerializeField]
        private int cutoutTextureIndex;
        
        private int? cutoutTextureId;
        
        /// <summary>
        /// The renderer of this outline target.
        /// </summary>
        public Renderer Renderer => renderer;

        internal bool UsesCutout => !string.IsNullOrEmpty(cutoutTextureName);

        internal Material SharedMaterial
        {
            get
            {
                if (renderer == null)
                    return null;
                
                TempSharedMaterials.Clear();
                renderer.GetSharedMaterials(TempSharedMaterials);

                return TempSharedMaterials.Count == 0 ? 
                    null :
                    TempSharedMaterials[ShiftedSubmeshIndex % TempSharedMaterials.Count];
            }
        }

        internal Texture CutoutTexture
        {
            get
            {
                var sharedMaterial = SharedMaterial;
                return sharedMaterial == null ? 
                    null : 
                    sharedMaterial.GetTexture(CutoutTextureId);
            }
        }

        internal bool IsValidForCutout
        {
            get
            {
                var materialToGetTextureFrom = SharedMaterial;
                return UsesCutout &&
                       materialToGetTextureFrom != null &&
                       materialToGetTextureFrom.HasProperty(CutoutTextureId) &&
                       CutoutTexture != null;
            }
        }
        
        /// <summary>
        /// The cutout texture index. Only applicable if the texture is TexArray.
        /// </summary>
        public int CutoutTextureIndex
        {
            get => cutoutTextureIndex;

            set
            {
                cutoutTextureIndex = value;
                if (cutoutTextureIndex >= 0) 
                    return;
                
                Debug.LogError("Trying to set cutout texture index less than zero");
                cutoutTextureIndex = 0;
            }
        }
        
        internal int ShiftedSubmeshIndex => SubmeshIndex;

        internal int CutoutTextureId
        {
            get
            {
                if (!cutoutTextureId.HasValue)
                    cutoutTextureId = Shader.PropertyToID(cutoutTextureName);

                return cutoutTextureId.Value;
            }
        }

        /// <summary>
        /// The name of the texture that is going to be used while rendering as a cutout source.
        /// </summary>
        public string CutoutTextureName
        {
            get => cutoutTextureName;

            set
            {
                cutoutTextureName = value;
                cutoutTextureId = null;
            }
        }

        public OutlineTarget()
        {

        }

        /// <summary>
        /// The constructor of the target.
        /// </summary>
        /// <param name="renderer">The renderer of the target.</param>
        /// <param name="submesh">The sub-mesh of the target.</param>
        public OutlineTarget(Renderer renderer, int submesh = 0)
        {
            SubmeshIndex = submesh;
            this.renderer = renderer;

            CutoutThreshold = 0.5f;
            cutoutTextureId = null;
            cutoutTextureName = string.Empty;
            CullMode = renderer is SpriteRenderer ? CullMode.Off : CullMode.Back;
        }

        /// <summary>
        /// The constructor of the target.
        /// </summary>
        /// <param name="renderer">The renderer of the target.</param>
        /// <param name="cutoutTextureName">The name of the texture to be used to render the cutout.</param>
        /// <param name="cutoutThreshold">The cutout threshold.</param>
        public OutlineTarget(Renderer renderer, string cutoutTextureName, float cutoutThreshold = 0.5f)
        {
            SubmeshIndex = 0;
            this.renderer = renderer;

            cutoutTextureId = Shader.PropertyToID(cutoutTextureName);
            CutoutThreshold = cutoutThreshold;
            this.cutoutTextureName = cutoutTextureName;
            CullMode = renderer is SpriteRenderer ? CullMode.Off : CullMode.Back;
        }
        
        /// <summary>
        /// The constructor of the target.
        /// </summary>
        /// <param name="renderer">The renderer of the target.</param>
        /// <param name="submeshIndex">The sub-mesh of the target.</param>
        /// <param name="cutoutTextureName">The name of the texture to be used to render the cutout.</param>
        /// <param name="cutoutThreshold">The cutout threshold.</param>
        public OutlineTarget(Renderer renderer, int submeshIndex, string cutoutTextureName, float cutoutThreshold = 0.5f)
        {
            SubmeshIndex = submeshIndex;
            this.renderer = renderer;

            cutoutTextureId = Shader.PropertyToID(cutoutTextureName);
            CutoutThreshold = cutoutThreshold;
            this.cutoutTextureName = cutoutTextureName;
            CullMode = renderer is SpriteRenderer ? CullMode.Off : CullMode.Back;
        }
    }
}