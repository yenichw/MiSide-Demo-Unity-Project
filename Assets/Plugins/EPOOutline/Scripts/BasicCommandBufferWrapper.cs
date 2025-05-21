using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace EPOOutline
{
    public class BasicCommandBufferWrapper : CommandBufferWrapper, IUnderlyingBufferProvider, IDisposable
    {
        private CommandBuffer buffer;

        private static RenderTargetIdentifier ConvertToRTI(RTHandle handle)
        { 
            return handle.rt != null ? handle.rt : handle.nameID;
        }

        public BasicCommandBufferWrapper(CommandBuffer buffer)
        {
            this.buffer = buffer;
        }

        public CommandBuffer UnderlyingBuffer => buffer;

        public void SetCommandBuffer(CommandBuffer buffer)
        {
            this.buffer = buffer;
        }

        public override void Clear()
        {
            buffer.Clear();
        }

        public override void SetGlobalInt(int hash, int value)
        {
            buffer.SetGlobalInt(hash, value);
        }

        public override void SetGlobalFloat(int hash, float value)
        {
            buffer.SetGlobalFloat(hash, value);
        }

        public override void SetGlobalVector(int hash, Vector4 value)
        {
            buffer.SetGlobalVector(hash, value);
        }

        public override void SetGlobalColor(int hash, Color color)
        {
            buffer.SetGlobalColor(hash, color);
        }

        public override void SetGlobalTexture(int hash, RTHandle texture)
        {
            buffer.SetGlobalTexture(hash, ConvertToRTI(texture));
        }

        public override void SetRenderTarget(RTHandle color, int slice)
        {
            var target = new RenderTargetIdentifier(ConvertToRTI(color), 0, CubemapFace.Unknown, slice);
            buffer.SetRenderTarget(target);
        }

        public override void SetRenderTarget(RTHandle color, RTHandle depth, int slice)
        {
            var colorRTI = new RenderTargetIdentifier(ConvertToRTI(color), 0, CubemapFace.Unknown, slice);
            var depthRTI = new RenderTargetIdentifier(ConvertToRTI(depth), 0, CubemapFace.Unknown, slice);
            
            buffer.SetRenderTarget(colorRTI, depthRTI);
        }

        public override void SetViewport(Rect rect)
        {
            buffer.SetViewport(rect);
        }

        public override void DisableShaderKeyword(string keyword)
        {
            buffer.DisableKeyword(GlobalKeyword.Create(keyword));
        }

        public override void EnableShaderKeyword(string keyword)
        {
            buffer.EnableKeyword(GlobalKeyword.Create(keyword));
        }

        public override void ClearRenderTarget(bool depth, bool clr, Color clearColor)
        {
            buffer.ClearRenderTarget(depth, clr, clearColor);
        }

        public override void DrawRenderer(Renderer target, Material material, int submesh)
        {
            buffer.DrawRenderer(target, material, submesh);
        }

        public override void DrawMeshInstanced(Mesh mesh, int submesh, Material material, int pass, Matrix4x4[] matrices, int countToDraw,
            MaterialPropertyBlock block)
        {
            buffer.DrawMeshInstanced(mesh, submesh, material, pass, matrices, countToDraw, block);
        }

        public override void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material, int submesh, int pass)
        {
            buffer.DrawMesh(mesh, matrix, material, submesh, pass);
        }

        public void Dispose()
        {
            buffer?.Dispose();
            buffer = null;
        }
    }
}