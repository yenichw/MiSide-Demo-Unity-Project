#if URP_OUTLINE && UNITY_6000_0_OR_NEWER
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace EPOOutline
{
    public class MainRenderFunctionParameter
    {
        public TextureHandle RenderTarget;
        public TextureHandle DepthTarget;
        
        public MainRenderFunctionParameter()
        {
            
        }
    }
    
    public class BlitRenderFunctionParameter
    {
        public TextureHandle Target;
        
        public BlitRenderFunctionParameter()
        {
            
        }
    }
}
#endif