// based on the original game.Yen Chezky(yenichw)
/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;

namespace InfinityCode.UltimateEditorEnhancer.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ComponentHeaderButtonAttribute : Attribute
    {
        public float order = 0;
        
        public ComponentHeaderButtonAttribute(float order = 0)
        {
            this.order = order;
        }
    }
}