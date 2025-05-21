/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Integration
{
    [InitializeOnLoad]
    public static class Cinemachine
    {
        public static bool isPresent { get; }

        static Cinemachine()
        {
            Assembly assembly = Reflection.GetAssembly("Cinemachine");
            if (assembly != null) isPresent = true;
        }

        public static bool ContainBrain(GameObject gameObject)
        {
            return isPresent && gameObject.GetComponent("Cinemachine.CinemachineBrain") != null;
        }
    }
}