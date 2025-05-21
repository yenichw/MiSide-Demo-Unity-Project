/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class HandleUtilityRef
    {
        private static MethodInfo _applyWireMaterialMethod;
        private static MethodInfo _intersectRayMeshMethod;

        private static MethodInfo applyWireMaterialMethod
        {
            get
            {
                if (_applyWireMaterialMethod == null)
                {
                    _applyWireMaterialMethod = Reflection.GetMethod(type, "ApplyWireMaterial", new[] { typeof(CompareFunction)}, Reflection.StaticLookup);
                }

                return _applyWireMaterialMethod;
            }
        }

        private static MethodInfo intersectRayMeshMethod
        {
            get
            {
                if (_intersectRayMeshMethod == null)
                {
                    _intersectRayMeshMethod = Reflection.GetMethod(type, "IntersectRayMesh", new []{typeof(Ray), typeof(Mesh), typeof(Matrix4x4), typeof(RaycastHit).MakeByRefType()}, Reflection.StaticLookup);
                }

                return _intersectRayMeshMethod;
            }
        }

#if !UNITY_2020_3_OR_NEWER
        private static MethodInfo _findNearestVertexMethod;

        private static MethodInfo findNearestVertexMethod
        {
            get
            {
                if (_findNearestVertexMethod == null)
                {
                    _findNearestVertexMethod = Reflection.GetMethod(type, "FindNearestVertex", new[] { typeof(Vector2), typeof(Transform[]), typeof(Vector3).MakeByRefType() }, Reflection.StaticLookup);
                }

                return _findNearestVertexMethod;
            }
        }
#endif

        public static Type type
        {
            get => typeof(HandleUtility);
        }

        public static void ApplyWireMaterial(CompareFunction compareFunction)
        {
            applyWireMaterialMethod.Invoke(null, new object[] {compareFunction});
        }

        public static void FindNearestVertex(Vector2 screenPosition, out Vector3 position)
        {
#if UNITY_2020_3_OR_NEWER
            HandleUtility.FindNearestVertex(screenPosition, out position);
#else
            object[] p = {screenPosition, null, null};
            findNearestVertexMethod.Invoke(null, p);
            position = (Vector3) p[2];
#endif

        }

        public static bool IntersectRayMesh(Ray ray, Mesh mesh, Matrix4x4 matrix, out RaycastHit hit)
        {
            object[] obj = new object[]
            {
                ray,
                mesh,
                matrix,
                null
            };
            bool ret = (bool)intersectRayMeshMethod.Invoke(null, obj);
            hit = (RaycastHit) obj[3];
            return ret;
        }
    }
}