/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Attributes
{
    public class RequireComponentsAttribute : ValidateAttribute
    {
        private Type[] types;

        public RequireComponentsAttribute(params Type[] types)
        {
            this.types = types;
        }

        public override bool Validate()
        {
            GameObject go = Selection.activeGameObject;
            if (go == null) return false;

            foreach (Type type in types)
            {
                if (go.GetComponent(type) != null) return true;
            }

            return false;
        }
    }
}