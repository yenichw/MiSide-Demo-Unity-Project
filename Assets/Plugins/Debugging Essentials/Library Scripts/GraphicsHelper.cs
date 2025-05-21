using UnityEngine;

namespace DebuggingEssentials
{
    public static class GraphicsHelper
    {
        static public void Dispose(ref RenderTexture rt)
        {
            if (rt == null) return;
            rt.Release();
            if (Application.isPlaying) Object.Destroy(rt); else Object.DestroyImmediate(rt); 
            rt = null;
        }

        static public void Dispose(ref RenderTexture[] rts)
        {
            if (rts == null) return;

            for (int i = 0; i < rts.Length; i++) Dispose(ref rts[i]);
        }

        static public void Dispose(ref Texture2D tex)
        {
            if (tex == null) return;
            if (Application.isPlaying) Object.Destroy(tex); else Object.DestroyImmediate(tex);
            tex = null;
        }

        static public void Dispose(ref Texture3D tex)
        {
            if (tex == null) return;
            if (Application.isPlaying) Object.Destroy(tex); else Object.DestroyImmediate(tex);
            tex = null;
        }

        static public void Dispose(ref Texture2D[] textures)
        {
            if (textures == null) return;
            for (int i = 0; i < textures.Length; i++) Dispose(ref textures[i]);
        }

        static public void DisposeLightmaps()
        {
            //LightmapData[] lightmapDatas = LightmapSettings.lightmaps;

            //for (int i = 0; i < lightmapDatas.Length; i++)
            //{
            //    LightmapData lightmapData = lightmapDatas[i];

            //    DisposeTexture(lightmapData.lightmapColor);
            //    DisposeTexture(lightmapData.lightmapDir);
            //    DisposeTexture(lightmapData.shadowMask);
            //}

            LightmapSettings.lightmaps = null;
        }

        static public void Dispose(ref ComputeBuffer computeBuffer)
        {
            if (computeBuffer == null) return;
            computeBuffer.Dispose();
            computeBuffer = null;
        }

        static public void Dispose(ref Mesh mesh)
        {
            if (mesh == null) return;
            if (Application.isPlaying) Object.Destroy(mesh); else Object.DestroyImmediate(mesh);
            mesh = null;
        }
    }
}