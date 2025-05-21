using UnityEngine;
using UnityEngine.Rendering;

namespace EPOOutline
{
    public abstract class CommandBufferWrapper
    {
        public abstract void Clear();
        public abstract void SetGlobalInt(int hash, int value);
        public abstract void SetGlobalFloat(int hash, float value);
        public abstract void SetGlobalVector(int hash, Vector4 value);
        public abstract void SetGlobalColor(int hash, Color color);
        public abstract void SetGlobalTexture(int hash, RTHandle texture);
        public abstract void SetRenderTarget(RTHandle color, int slice);
        public abstract void SetRenderTarget(RTHandle color, RTHandle depth, int slice);
        public abstract void SetViewport(Rect rect);
        public abstract void DisableShaderKeyword(string keyword);
        public abstract void EnableShaderKeyword(string keyword);
        public abstract void ClearRenderTarget(bool depth, bool color, Color clearColor);
        public abstract void DrawRenderer(Renderer target, Material material, int submesh);

        public abstract void DrawMeshInstanced(Mesh mesh, int submesh, Material material, int pass,
            Matrix4x4[] matrices, int countToDraw, MaterialPropertyBlock block);
        
        public abstract void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material, int submesh, int pass);
    }
}