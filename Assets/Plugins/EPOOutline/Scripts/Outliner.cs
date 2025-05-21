using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace EPOOutline
{
    /// <summary>
    /// The component that is responsible for rendering the ouline and holding its parameters.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class Outliner : MonoBehaviour
    {
#if UNITY_EDITOR
        private static GameObject lastSelectedOutliner;

        private static List<Outliner> outliners = new List<Outliner>();
#endif

        private static List<Outlinable> temporaryOutlinables = new List<Outlinable>();

        private OutlineParameters parameters;
        private OutlineParameters Parameters => parameters ??= new OutlineParameters(new BasicCommandBufferWrapper(new CommandBuffer()));

#if UNITY_EDITOR
        private OutlineParameters editorPreviewParameters;
        private OutlineParameters EditorPreviewParameters => editorPreviewParameters ??= new OutlineParameters(new BasicCommandBufferWrapper(new CommandBuffer()));
#endif

        private Camera targetCamera;

        [SerializeField]
        private RenderStage stage = RenderStage.AfterTransparents;

        [SerializeField]
        private OutlineRenderingStrategy renderingStrategy = OutlineRenderingStrategy.Default;

        [SerializeField]
        private RenderingMode renderingMode;

        [SerializeField]
        private long outlineLayerMask = -1;

        [SerializeField]
        private BufferSizeMode primaryBufferSizeMode;

        [SerializeField]
        [Range(0.15f, 1.0f)]
        private float primaryRendererScale = 0.75f;

        [SerializeField]
        private int primarySizeReference = 800;

        [SerializeField]
        [Range(0.0f, 2.0f)]
        private float blurShift = 1.0f;

        [SerializeField]
        [Range(0.0f, 2.0f)]
        private float dilateShift = 1.0f;

        [SerializeField]
        private int dilateIterations = 1;

        [SerializeField]
        private DilateQuality dilateQuality;

        [SerializeField]
        private int blurIterations = 1;

        [SerializeField]
        private BlurType blurType = BlurType.Box;

        private RTHandle target;

        private RTHandle primaryBuffer;
        private RTHandle targetBuffer;
        
        private CameraEvent Event => stage == RenderStage.BeforeTransparents ? CameraEvent.AfterForwardOpaque : CameraEvent.BeforeImageEffects;

        /// <summary>
        /// Used to calculate the buffer size if <see cref="EPOOutline.BufferSizeMode.WidthControlsHeight"/> or <see cref="EPOOutline.BufferSizeMode.HeightControlsWidth"/> is set to <see cref="EPOOutline.Outliner.PrimaryBufferSizeMode"/>.
        /// <seealso cref="EPOOutline.BufferSizeMode.WidthControlsHeight"/>
        /// <seealso cref="EPOOutline.BufferSizeMode.HeightControlsWidth"/>
        /// </summary>
        public int PrimarySizeReference
        {
            get => primarySizeReference;
            set => primarySizeReference = value < 10 ? 50 : value;
        }

        /// <summary>
        /// <see cref="EPOOutline.BufferSizeMode"/> that will be used for this outliner.
        /// </summary>
        public BufferSizeMode PrimaryBufferSizeMode
        {
            get => primaryBufferSizeMode;
            set => primaryBufferSizeMode = value;
        }

        /// <summary>
        /// <see cref="EPOOutline.OutlineRenderingStrategy"/> that is going to be used by this outliner.
        /// </summary>
        public OutlineRenderingStrategy RenderingStrategy
        {
            get => renderingStrategy;
            set => renderingStrategy = value;
        }

        /// <summary>
        /// <see cref="EPOOutline.RenderStage"/> to use for this outliner.
        /// </summary>
        public RenderStage RenderStage
        {
            get => stage;
            set => stage = value;
        }

        /// <summary>
        /// <see cref="EPOOutline.DilateQuality"/> that is going to be used for this outliner.
        /// </summary>
        public DilateQuality DilateQuality
        {
            get => dilateQuality;
            set => dilateQuality = value;
        }

        /// <summary>
        /// <see cref="EPOOutline.RenderingMode"/> that is going to be used for this outliner. For HDRP it's always <see cref="EPOOutline.RenderingMode.HDR"/>.
        /// </summary>
        public RenderingMode RenderingMode
        {
            get => renderingMode;
            set => renderingMode = value;
        }

        /// <summary>
        /// Blur shift amount that is going to be applied to this outliner. The more, the higher the shift will be.
        /// </summary>
        public float BlurShift
        {
            get => blurShift;
            set => blurShift = Mathf.Clamp(value, 0, 2.0f);
        }

        /// <summary>
        /// Dilate shift amount that is going to be applied to this outliner. The more, the higher the dilate will be.
        /// </summary>
        public float DilateShift
        {
            get => dilateShift;
            set => dilateShift = Mathf.Clamp(value, 0, 2.0f);
        }

        /// <summary>
        /// The layer mask that will be used to filter the <see cref="EPOOutline.Outlinable"/> before rendering.
        /// <seealso cref="EPOOutline.Outlinable.OutlineLayer"/>
        /// </summary>
        public long OutlineLayerMask
        {
            get => outlineLayerMask;
            set => outlineLayerMask = value;
        }

        /// <summary>
        /// The value to scale the primary buffer size if <see cref="EPOOutline.Outliner.PrimaryBufferSizeMode"/> is set to <see cref="EPOOutline.BufferSizeMode.Scaled"/>.
        /// </summary>
        public float PrimaryRendererScale
        {
            get => primaryRendererScale;
            set => primaryRendererScale = Mathf.Clamp(value, 0.1f, 1.0f);
        }

        /// <summary>
        /// The count of blur iterations to apply.
        /// </summary>
        public int BlurIterations
        {
            get => blurIterations;
            set => blurIterations = value > 0 ? value : 0;
        }

        /// <summary>
        /// <see cref="EPOOutline.BlurType"/> to use for this outliner.
        /// </summary>
        public BlurType BlurType
        {
            get => blurType;
            set => blurType = value;
        }

        /// <summary>
        /// The count of dilate iterations to apply.
        /// </summary>
        public int DilateIterations
        {
            get => dilateIterations;
            set => dilateIterations = value > 0 ? value : 0;
        }

        private void OnValidate()
        {
            if (blurIterations < 0)
                blurIterations = 0;

            if (dilateIterations < 0)
                dilateIterations = 0;

            if (primarySizeReference < 10)
                primarySizeReference = 10;

            primaryRendererScale = Mathf.Clamp(primaryRendererScale, 0.1f, 1.0f);

            if (blurType < BlurType.Box)
                blurType = BlurType.Box;

            if (blurType > BlurType.Gaussian13x13)
                blurType = BlurType.Gaussian13x13;
        }

        private void OnEnable()
        {
            if (targetCamera == null)
                targetCamera = GetComponent<Camera>();

            targetCamera.forceIntoRenderTexture = targetCamera.stereoTargetEye == StereoTargetEyeMask.None || !UnityEngine.XR.XRSettings.enabled;

#if UNITY_EDITOR
            outliners.Add(this);
#endif
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            EditorPreviewParameters.Dispose();
#endif
            Parameters.Dispose();
        }

        private void OnDisable()
        {
            if (targetCamera != null)
                UpdateBuffer(targetCamera, Parameters.Buffer, true);

#if UNITY_EDITOR
            RemoveFromAllSceneViews();

            outliners.Remove(this);
#endif

#if UNITY_EDITOR
            if (RenderPipelineManager.currentPipeline != null)
                return;
            
            if (!(EditorPreviewParameters.Buffer is IUnderlyingBufferProvider bufferProvider))
                return;
            
            var underlyingBuffer = bufferProvider.UnderlyingBuffer;
            
            foreach (var view in UnityEditor.SceneView.sceneViews)
            {
                var viewToUpdate = (UnityEditor.SceneView)view;
                
                viewToUpdate.camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, underlyingBuffer);
                viewToUpdate.camera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, underlyingBuffer);
            }
#endif
        }

        private void UpdateBuffer(Camera cameraToUpdate, CommandBufferWrapper buffer, bool removeOnly)
        {
            if (RenderPipelineManager.currentPipeline != null)
                return;
            
            if (!(buffer is IUnderlyingBufferProvider bufferProvider))
                return;

            var underlyingBuffer = bufferProvider.UnderlyingBuffer;
            if (underlyingBuffer == null)
                return;

            cameraToUpdate.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, underlyingBuffer);
            cameraToUpdate.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, underlyingBuffer);
            if (removeOnly)
                return;

            cameraToUpdate.AddCommandBuffer(Event, underlyingBuffer);
        }

        private void OnPreRender()
        {
            if (PipelineFetcher.CurrentAsset != null)
                return;
            
            Parameters.OutlinablesToRender.Clear();
            SetupOutline(targetCamera, Parameters, false);
        }

        private void SetupOutline(Camera cameraToUse, OutlineParameters parametersToUse, bool isEditor)
        {
            UpdateBuffer(cameraToUse, parametersToUse.Buffer, false);
            PrepareParameters(parametersToUse, cameraToUse, isEditor);

            parametersToUse.Buffer.Clear();
            if (renderingStrategy == OutlineRenderingStrategy.Default)
            {
                OutlineEffect.SetupOutline(parametersToUse);
                parametersToUse.BlitMesh = null;
                parametersToUse.MeshPool.ReleaseAllMeshes();
            }
            else
            {
                temporaryOutlinables.Clear();
                temporaryOutlinables.AddRange(parametersToUse.OutlinablesToRender);

                parametersToUse.OutlinablesToRender.Clear();
                parametersToUse.OutlinablesToRender.Add(null);

                foreach (var outlinable in temporaryOutlinables)
                {
                    parametersToUse.OutlinablesToRender[0] = outlinable;
                    OutlineEffect.SetupOutline(parametersToUse);
                    parametersToUse.BlitMesh = null;
                }

                parametersToUse.MeshPool.ReleaseAllMeshes();
            }
        }

#if UNITY_EDITOR
        private void RemoveFromAllSceneViews()
        {
            if (RenderPipelineManager.currentPipeline != null)
                return;
            
            foreach (var view in UnityEditor.SceneView.sceneViews)
            {
                var viewToUpdate = (UnityEditor.SceneView)view;
                var eventTransferer = viewToUpdate.camera.GetComponent<OnPreRenderEventTransferer>();
                if (eventTransferer != null)
                    eventTransferer.OnPreRenderEvent -= UpdateEditorCamera;

                if (!(EditorPreviewParameters.Buffer is IUnderlyingBufferProvider bufferProvider))
                    return;

                var underlyingBuffer = bufferProvider.UnderlyingBuffer;
                
                viewToUpdate.camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, underlyingBuffer);
                viewToUpdate.camera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, underlyingBuffer);
            }
        }

        private void LateUpdate()
        {
            if (lastSelectedOutliner == null && outliners.Count > 0)
                lastSelectedOutliner = outliners[0].gameObject;

            var isSelected = Array.Find(UnityEditor.Selection.gameObjects, x => x == gameObject) ?? lastSelectedOutliner != null;
            if (isSelected)
                lastSelectedOutliner = gameObject;

            foreach (var view in UnityEditor.SceneView.sceneViews)
            {
                var viewToUpdate = (UnityEditor.SceneView)view;
                var eventTransferer = viewToUpdate.camera.GetComponent<OnPreRenderEventTransferer>();
                if (eventTransferer != null)
                    eventTransferer.OnPreRenderEvent -= UpdateEditorCamera;

                UpdateBuffer(viewToUpdate.camera, EditorPreviewParameters.Buffer, true);
            }

            if (!isSelected)
                return;

            foreach (var view in UnityEditor.SceneView.sceneViews)
            {
                var viewToUpdate = (UnityEditor.SceneView)view;
                if (!viewToUpdate.sceneViewState.showImageEffects)
                    continue;

                var eventTransferer = viewToUpdate.camera.GetComponent<OnPreRenderEventTransferer>();
                if (eventTransferer == null)
                    eventTransferer = viewToUpdate.camera.gameObject.AddComponent<OnPreRenderEventTransferer>();

                eventTransferer.OnPreRenderEvent += UpdateEditorCamera;
            }
        }

        private void UpdateEditorCamera(Camera cameraToUpdate)
        {
            SetupOutline(cameraToUpdate, EditorPreviewParameters, true);
        }
#endif

        public StereoTargetEyeMask GetTargetEyeMask(Camera cameraTarget)
        {
            return XRUtility.IsXRActive ? cameraTarget.stereoTargetEye : StereoTargetEyeMask.None;
        }

        public void UpdateSharedParameters(OutlineParameters parametersToUpdate, Camera cameraToUpdate, bool editorCamera, bool forceNative, bool forceHDR)
        {
            parametersToUpdate.DilateQuality = DilateQuality;
            parametersToUpdate.Camera = cameraToUpdate;
            parametersToUpdate.IsEditorCamera = editorCamera;
            parametersToUpdate.PrimaryBufferScale = forceNative ? 1.0f : primaryRendererScale;

            if (forceNative)
                parametersToUpdate.PrimaryBufferSizeMode = BufferSizeMode.Native;
            else
            {
                parametersToUpdate.PrimaryBufferSizeMode = primaryBufferSizeMode;
                parametersToUpdate.PrimaryBufferSizeReference = primarySizeReference;
            }

            parametersToUpdate.BlurIterations = blurIterations;
            parametersToUpdate.BlurType = blurType;
            parametersToUpdate.DilateIterations = dilateIterations;
            parametersToUpdate.BlurShift = blurShift;
            parametersToUpdate.DilateShift = dilateShift;
            parametersToUpdate.UseHDR = forceHDR || cameraToUpdate.allowHDR && (RenderingMode == RenderingMode.HDR);
            parametersToUpdate.EyeMask = GetTargetEyeMask(cameraToUpdate);

            parametersToUpdate.OutlineLayerMask = outlineLayerMask;
            
            parametersToUpdate.Prepare();
            
            parametersToUpdate.TextureHandleMap.Clear();
            foreach (var outlinable in parametersToUpdate.OutlinablesToRender)
            {
                for (var index = 0; index < outlinable.OutlineTargets.Count; index++)
                {
                    var outlineTarget = outlinable.OutlineTargets[index];
                    if (outlineTarget.IsValidForCutout)
                    {
                        var cutoutTexture = outlineTarget.CutoutTexture;
                        var rtHandle = parametersToUpdate.RTHandlePool.Allocate(cutoutTexture);
                        parametersToUpdate.TextureHandleMap[cutoutTexture] = rtHandle;
                    }

                    if (outlineTarget.Renderer is SpriteRenderer spriteRenderer)
                    {
                        var texture = spriteRenderer.sprite.texture;
                        var rtHandle = parametersToUpdate.RTHandlePool.Allocate(texture);
                        parametersToUpdate.TextureHandleMap[texture] = rtHandle;
                    }
                }
            }
        }

        public void ReplaceHandles(OutlineParameters parametersToUpdate)
        {
            Replace(ref parametersToUpdate.Handles.Target, parametersToUpdate.TargetWidth, parametersToUpdate.TargetHeight, parametersToUpdate,
                (width, height, outlineParameters) => RenderTargetUtility.GetRT(outlineParameters, width, height, "Target"));
            
            Replace(ref parametersToUpdate.Handles.InfoTarget, parametersToUpdate.TargetWidth, parametersToUpdate.TargetHeight, parametersToUpdate,
                (width, height, outlineParameters) => RenderTargetUtility.GetRT(outlineParameters, width, height, "Info target"));
            
            var (scaledWidth, scaledHeight) = parametersToUpdate.ScaledSize;
            
            Replace(ref parametersToUpdate.Handles.PrimaryTarget, scaledWidth, scaledHeight, parametersToUpdate,
                (width, height, outlineParameters) => RenderTargetUtility.GetRT(outlineParameters, width, height, "Primary target"));
            
            Replace(ref parametersToUpdate.Handles.SecondaryTarget, scaledWidth, scaledHeight, parametersToUpdate,
                (width, height, outlineParameters) => RenderTargetUtility.GetRT(outlineParameters, width, height, "Secondary target"));
            
            Replace(ref parametersToUpdate.Handles.PrimaryInfoBufferTarget, scaledWidth, scaledHeight, parametersToUpdate,
                (width, height, outlineParameters) => RenderTargetUtility.GetRT(outlineParameters, width, height, "Primary info target"));
            
            Replace(ref parametersToUpdate.Handles.SecondaryInfoBufferTarget, scaledWidth, scaledHeight, parametersToUpdate,
                (width, height, outlineParameters) => RenderTargetUtility.GetRT(outlineParameters, width, height, "Secondary info target"));
        }

        private static void Replace(ref RTHandle handle, int width, int height, OutlineParameters parameters, Func<int, int, OutlineParameters, RTHandle> newHandle)
        {
            if (handle != null)
            {
                if (width == handle.rtHandleProperties.currentRenderTargetSize.x &&
                    height == handle.rtHandleProperties.currentRenderTargetSize.y &&
                    handle.rt.descriptor.msaaSamples == parameters.Antialiasing)
                    return;
                
                handle.Release();
            }

            handle = newHandle(width, height, parameters);
            handle.SetCustomHandleProperties(new RTHandleProperties() { currentRenderTargetSize = new Vector2Int(width, height) });
        }

        private void PrepareParameters(OutlineParameters parametersToPrepare, Camera cameraToUse, bool editorCamera)
        {
            parametersToPrepare.RTHandlePool.ReleaseAll();
            
            parametersToPrepare.DepthTarget = parametersToPrepare.RTHandlePool.Allocate(RenderTargetUtility.ComposeTarget(parametersToPrepare, BuiltinRenderTextureType.CameraTarget));
            parametersToPrepare.Target = parametersToPrepare.RTHandlePool.Allocate(RenderTargetUtility.ComposeTarget(parametersToPrepare, BuiltinRenderTextureType.CameraTarget));

            var targetTexture = cameraToUse.targetTexture == null ? cameraToUse.activeTexture : cameraToUse.targetTexture;

            if (XRUtility.IsUsingVR(parametersToPrepare))
            {
                var descriptor = XRUtility.VRRenderTextureDescriptor;
                parametersToPrepare.TargetWidth = descriptor.width;
                parametersToPrepare.TargetHeight = descriptor.height;
            }
            else
            {
                parametersToPrepare.TargetWidth = targetTexture != null ? targetTexture.width : cameraToUse.scaledPixelWidth;
                parametersToPrepare.TargetHeight = targetTexture != null ? targetTexture.height : cameraToUse.scaledPixelHeight;
            }

            parametersToPrepare.Viewport =
                new Rect(0, 0, parametersToPrepare.TargetWidth, parametersToPrepare.TargetHeight);

            parametersToPrepare.Antialiasing = editorCamera ? (targetTexture == null ? 1 : targetTexture.antiAliasing) : CameraUtility.GetMSAA(targetCamera);

            parametersToPrepare.Camera = cameraToUse;

            var scaledSize = parametersToPrepare.ScaledSize;
            parametersToPrepare.ScaledBufferWidth = scaledSize.ScaledWidth;
            parametersToPrepare.ScaledBufferHeight = scaledSize.ScaledHeight;

            Outlinable.GetAllActiveOutlinables(parametersToPrepare.OutlinablesToRender);
            RendererFilteringUtility.Filter(parametersToPrepare.Camera, parametersToPrepare);
            UpdateSharedParameters(parametersToPrepare, cameraToUse, editorCamera, false, false);
            ReplaceHandles(parametersToPrepare);
        }
    }
}