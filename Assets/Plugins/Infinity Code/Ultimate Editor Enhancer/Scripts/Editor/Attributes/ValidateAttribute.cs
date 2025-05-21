/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;

namespace InfinityCode.UltimateEditorEnhancer.Attributes
{
    public abstract class ValidateAttribute : Attribute
    {
        public abstract bool Validate();
    }
}