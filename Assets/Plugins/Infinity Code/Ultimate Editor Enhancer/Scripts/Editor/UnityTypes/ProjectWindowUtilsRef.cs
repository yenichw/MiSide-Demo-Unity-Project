/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class ProjectWindowUtilsRef
    {
        private static MethodInfo _createFolderWithTemplatesMethod;

        private static MethodInfo createFolderWithTemplatesMethod
        {
            get
            {
                if (_createFolderWithTemplatesMethod == null) _createFolderWithTemplatesMethod = type.GetMethod("CreateFolderWithTemplates", Reflection.StaticLookup, null, new[] { typeof(string), typeof(string[]) }, null);
                return _createFolderWithTemplatesMethod;
            }
        }

        public static Type type
        {
            get
            {
                return typeof(ProjectWindowUtil);
            }
        }

        public static void CreateFolderWithTemplates(string defaultName, params string[] templates)
        {
            createFolderWithTemplatesMethod.Invoke(null, new object[] { defaultName, templates });
        }
    }
}