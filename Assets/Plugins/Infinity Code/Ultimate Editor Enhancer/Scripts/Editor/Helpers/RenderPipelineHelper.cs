/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEngine;
using UnityEngine.Rendering;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static class RenderPipelineHelper
    {
        public static Shader GetDefaultShader()
        {
#if UNITY_6000_0_OR_NEWER
            RenderPipelineAsset rp = GraphicsSettings.defaultRenderPipeline;
#else
            RenderPipelineAsset rp = GraphicsSettings.renderPipelineAsset;
#endif
            if (rp != null)
            {
                if (rp.GetType().Name.Contains("HDRenderPipelineAsset"))
                {
                    return Shader.Find("HDRenderPipeline/Lit");
                }

                return Shader.Find("Universal Render Pipeline/Lit");
            }

            return Shader.Find("Standard");
        }
    }
}