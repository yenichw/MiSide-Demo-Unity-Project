/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;

namespace InfinityCode.UltimateEditorEnhancer.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TitleAttribute : Attribute
    {
        public readonly string displayName;

        /// <summary>
        ///   <para>Specify a display name for an enum value.</para>
        /// </summary>
        /// <param name="displayName">The name to display.</param>
        public TitleAttribute(string displayName)
        {
            this.displayName = displayName;
        }
    }
}