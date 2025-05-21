#if URP_OUTLINE && UNITY_6000_0_OR_NEWER
using System.Collections.Generic;
using EPOOutline;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace EPOOutline
{
    public class UnsafeCommandBufferWrapper : CommandBufferWrapper
    {
        private Dictionary<RTHandle, TextureHandle> handleMap = new Dictionary<RTHandle, TextureHandle>();
        private UnsafeCommandBuffer cmb;

        public void SetHandleMap(Dictionary<RTHandle, TextureHandle> handles)
        {
            handleMap.Clear();
            foreach (var handle in handles)
                handleMap.Add(handle.Key, handle.Value);
        }

        public void SetCommandBuffer(UnsafeCommandBuffer buffer)
        {
            cmb = buffer;
        }
        
        public override void Clear()
        {
            cmb.Clear();
        }

        public override void SetGlobalInt(int hash, int value)
        {
            cmb.SetGlobalInt(hash, value);
        }

        public override void SetGlobalFloat(int hash, float value)
        {
            cmb.SetGlobalFloat(hash, value);
        }

        public override void SetGlobalVector(int hash, Vector4 value)
        {
            cmb.SetGlobalVector(hash, value);
        }

        public override void SetGlobalColor(int hash, Color color)
        {
            cmb.SetGlobalColor(hash, color);
        }

        public override void SetGlobalTexture(int hash, RTHandle texture)
        {
            cmb.SetGlobalTexture(hash, handleMap[texture]);
        }

        public override void SetRenderTarget(RTHandle color, int slice)
        {
            cmb.SetRenderTarget(color, 0, CubemapFace.Unknown, slice);
        }

        public override void SetRenderTarget(RTHandle color, RTHandle depth, int slice)
        {
            cmb.SetRenderTarget(color, depth, 0, CubemapFace.Unknown, slice);
        }

        public override void SetViewport(Rect rect)
        {
            cmb.SetViewport(rect);
        }

        public override void DisableShaderKeyword(string keyword)
        {
            cmb.DisableShaderKeyword(keyword);
        }

        public override void EnableShaderKeyword(string keyword)
        {
            cmb.EnableShaderKeyword(keyword);
        }

        public override void ClearRenderTarget(bool depth, bool color, Color clearColor)
        {
            cmb.ClearRenderTarget(depth, color, clearColor);
        }

        public override void DrawRenderer(Renderer target, Material material, int submesh)
        {
            cmb.DrawRenderer(target, material, submesh);
        }

        public override void DrawMeshInstanced(Mesh mesh, int submesh, Material material, int pass, Matrix4x4[] matrices, int countToDraw,
            MaterialPropertyBlock block)
        {
            cmb.DrawMeshInstanced(mesh, submesh, material, pass, matrices, countToDraw, block);
        }

        public override void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material, int submesh, int pass)
        {
            cmb.DrawMesh(mesh, matrix, material, submesh, pass);
        }
    }
}
#endif
