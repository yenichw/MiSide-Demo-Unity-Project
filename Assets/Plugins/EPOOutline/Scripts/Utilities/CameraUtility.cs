using UnityEngine;

#if URP_OUTLINE
using UnityEngine.Rendering.Universal;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

#if HDRP_OUTLINE
using UnityEngine.Rendering.HighDefinition;
#endif

namespace EPOOutline
{
    public static class CameraUtility
    {
        public static int GetMSAA(Camera camera)
        {
            if (camera.targetTexture != null)
                return camera.targetTexture.antiAliasing;

            var antialiasing = GetRenderPipelineMSAA();

            var msaa = Mathf.Max(antialiasing, 1);
            if (!camera.allowMSAA)
                msaa = 1;

            if (camera.actualRenderingPath != RenderingPath.Forward &&
                camera.actualRenderingPath != RenderingPath.VertexLit)
                msaa = 1;

            return msaa;
        }
        
        private static int GetRenderPipelineMSAA()
        {
#if URP_OUTLINE
            if (PipelineFetcher.CurrentAsset is
                UniversalRenderPipelineAsset universalRenderPipelineAsset)
                return universalRenderPipelineAsset.msaaSampleCount;
#endif

#if HDRP_OUTLINE
            if (PipelineFetcher.CurrentAsset is HDRenderPipelineAsset)
                return 1;
#endif

            return QualitySettings.antiAliasing;
        }
    }
}