using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#if HDRP_OUTLINE
using UnityEngine.Rendering.HighDefinition;

namespace EPOOutline
{
    [VolumeComponentMenu("EPO/Outline")]
    public class OutlineCustomPass : CustomPass
    {
        private List<Outlinable> tempOutlinables = new List<Outlinable>();

        private static Queue<OutlineParameters> pool = new Queue<OutlineParameters>();
        private static Queue<OutlineParameters> parametersInUse = new Queue<OutlineParameters>();

        private List<Outliner> outliners = new List<Outliner>();

        [SerializeField]
        [HideInInspector]
        private Camera lastSelectedCamera = null;

        protected override void Execute(CustomPassContext ctx)
        {
            var camera = ctx.hdCamera.camera;

            outliners ??= new List<Outliner>();
            outliners.Clear();
            
    #if UNITY_EDITOR
            if (camera.cameraType == CameraType.SceneView)
            {
                if (lastSelectedCamera == null)
                {
                    foreach (var obj in UnityEditor.Selection.gameObjects)
                    {
                        lastSelectedCamera = obj.GetComponent<Camera>();
                        if (lastSelectedCamera != null)
                            lastSelectedCamera.GetComponents(outliners);

                        if (outliners.Count != 0)
                            break;
                        
                        lastSelectedCamera = null;
                    }
                }

                outliners.Clear();
                if (lastSelectedCamera != null)
                    lastSelectedCamera.GetComponents(outliners);
            }
            else
    #endif
                camera.GetComponents(outliners);
            
            var hdCameraData = ctx.hdCamera;
            var commandBuffer = ctx.cmd;
            var context = ctx.renderContext;
            
            RTHandle colorTarget;
            RTHandle depthTarget;
            
            colorTarget = ctx.cameraColorBuffer;
            depthTarget = ctx.cameraDepthBuffer;
            
            foreach (var outliner in outliners)
                UpdateOutliner(context, commandBuffer, camera, outliner, hdCameraData, colorTarget, depthTarget);

            if (parametersInUse != null)
            {
                foreach (var param in parametersInUse)
                    pool.Enqueue(param);

                parametersInUse.Clear();
            }
        }

        private void UpdateOutliner(ScriptableRenderContext renderContext, CommandBuffer cmd, Camera camera, Outliner outlineEffect, HDCamera hdCamera, RTHandle colorTarget, RTHandle depthTarget)
        {
            if (outlineEffect == null || !outlineEffect.enabled)
                return;

            pool ??= new Queue<OutlineParameters>();
            parametersInUse ??= new Queue<OutlineParameters>();

            if (pool.Count == 0)
                pool.Enqueue(new OutlineParameters(new BasicCommandBufferWrapper(cmd)));

            var parameters = pool.Dequeue();
            parametersInUse.Enqueue(parameters);

            (parameters.Buffer as BasicCommandBufferWrapper).SetCommandBuffer(cmd);

            outlineEffect.UpdateSharedParameters(parameters, camera, camera.cameraType == CameraType.SceneView, true, true);
            
            Outlinable.GetAllActiveOutlinables(parameters.OutlinablesToRender);
            RendererFilteringUtility.Filter(parameters.Camera, parameters);

            parameters.Buffer.EnableShaderKeyword("EPO_HDRP");

            parameters.Target = colorTarget;
            parameters.DepthTarget = depthTarget;

            parameters.TargetWidth = colorTarget.rt.width;
            parameters.TargetHeight = colorTarget.rt.height;
            parameters.Antialiasing = colorTarget.rt.antiAliasing;
            parameters.Scale = new Vector2(colorTarget.rtHandleProperties.rtHandleScale.x, colorTarget.rtHandleProperties.rtHandleScale.y);

            var viewportSize = colorTarget.useScaling
                ? colorTarget.GetScaledSize(colorTarget.rtHandleProperties.currentViewportSize)
                : colorTarget.rtHandleProperties.currentViewportSize;

            parameters.Viewport = new Rect(Vector2.zero, viewportSize);

            parameters.ScaledBufferWidth = viewportSize.x;
            parameters.ScaledBufferHeight = viewportSize.y;

            if (outlineEffect.RenderingStrategy == OutlineRenderingStrategy.Default)
            {
                outlineEffect.UpdateSharedParameters(parameters, camera, camera.cameraType == CameraType.SceneView, true, true);

                outlineEffect.ReplaceHandles(parameters);

                parameters.Prepare();

                OutlineEffect.SetupOutline(parameters);

                renderContext.ExecuteCommandBuffer((parameters.Buffer as BasicCommandBufferWrapper).UnderlyingBuffer);

                parameters.Buffer.Clear();
            }
            else
            {
                tempOutlinables ??= new List<Outlinable>();
                tempOutlinables.Clear();
                tempOutlinables.AddRange(parameters.OutlinablesToRender);

                outlineEffect.UpdateSharedParameters(parameters, camera, camera.cameraType == CameraType.SceneView, true, true);
                outlineEffect.ReplaceHandles(parameters);

                foreach (var outlinable in tempOutlinables)
                {
                    parameters.OutlinablesToRender.Clear();
                    parameters.OutlinablesToRender.Add(outlinable);

                    parameters.BlitMesh = null;

                    parameters.Prepare();

                    OutlineEffect.SetupOutline(parameters);

                    renderContext.ExecuteCommandBuffer((parameters.Buffer as BasicCommandBufferWrapper).UnderlyingBuffer);

                    parameters.Buffer.Clear();
                }

                if (tempOutlinables.Count == 0)
                {
                    parameters.OutlinablesToRender.Clear();
                    parameters.Prepare();
                    parameters.BlitMesh = null;
                    
                    OutlineEffect.SetupOutline(parameters);
                    
                    renderContext.ExecuteCommandBuffer((parameters.Buffer as BasicCommandBufferWrapper).UnderlyingBuffer);

                    parameters.Buffer.Clear();
                }

                parameters.MeshPool.ReleaseAllMeshes();
            }
            
            parameters.Buffer.Clear();
        }
    }
}
#endif