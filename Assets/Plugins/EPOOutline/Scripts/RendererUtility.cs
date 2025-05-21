using UnityEngine;

namespace EPOOutline
{
    public static class RendererUtility
    {
        public static int GetSubmeshCount(Renderer renderer)
        {
            if (renderer is MeshRenderer)
                return renderer.GetComponent<MeshFilter>().sharedMesh.subMeshCount;
                
            if (renderer is SkinnedMeshRenderer meshRenderer)
                return meshRenderer.sharedMesh.subMeshCount;
                
            return 1;
        }
    }
}