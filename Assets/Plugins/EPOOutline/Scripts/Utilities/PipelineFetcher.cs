using UnityEngine;
using UnityEngine.Rendering;

namespace EPOOutline
{
    public static class PipelineFetcher
    {
#if UNITY_2019_1_OR_NEWER
        public static RenderPipelineAsset CurrentAsset
        {
            get
            {
                var pipeline = QualitySettings.renderPipeline;
                if (pipeline == null)
                {
#if UNITY_6000_0_OR_NEWER
                    pipeline = GraphicsSettings.defaultRenderPipeline;
#else
                    pipeline = GraphicsSettings.renderPipelineAsset;
#endif
                }

                return pipeline;
            }
        }
#endif
    }
}