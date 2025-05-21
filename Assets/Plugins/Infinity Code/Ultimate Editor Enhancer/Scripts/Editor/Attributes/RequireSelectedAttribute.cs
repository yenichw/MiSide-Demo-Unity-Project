/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer.Attributes
{
    public class RequireSelectedAttribute : ValidateAttribute
    {
        public RequireSelectedAttribute()
        {

        }

        public override bool Validate()
        {
            return Selection.gameObjects.Length > 0;
        }
    }
}