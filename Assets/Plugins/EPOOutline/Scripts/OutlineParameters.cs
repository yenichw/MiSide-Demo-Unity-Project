using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace EPOOutline
{
    public class OutlineParameters : IDisposable
    {
        public OutlineParameters(CommandBufferWrapper wrapper)
        {
            Buffer = wrapper;
        }

        public readonly RTHandlePool RTHandlePool = new RTHandlePool();
        
        public readonly MeshPool MeshPool = new MeshPool();

        public readonly Handles Handles = new Handles();
        
        public Camera Camera;
        public RTHandle Target;
        public RTHandle DepthTarget;
        public CommandBufferWrapper Buffer;
        public DilateQuality DilateQuality = DilateQuality.Base;
        public int DilateIterations = 2;
        public int BlurIterations = 5;

        public Vector2 Scale = Vector2.one;

        public Rect Viewport;

        public long OutlineLayerMask = -1;

        public int TargetWidth;
        public int TargetHeight;

        public int ScaledBufferWidth;
        public int ScaledBufferHeight;
        
        public float BlurShift = 1.0f;

        public float DilateShift = 1.0f;

        public bool UseHDR;

        public bool UseInfoBuffer;

        public bool IsEditorCamera;

        public BufferSizeMode PrimaryBufferSizeMode;
        public int PrimaryBufferSizeReference;

        public float PrimaryBufferScale = 0.1f;

        public StereoTargetEyeMask EyeMask;

        public int Antialiasing = 1;

        public BlurType BlurType = BlurType.Gaussian13x13;

        public LayerMask Mask = -1;

        public Mesh BlitMesh;

        public List<Outlinable> OutlinablesToRender = new List<Outlinable>();

        public readonly Dictionary<Texture, RTHandle> TextureHandleMap = new Dictionary<Texture, RTHandle>();

        public (int ScaledWidth, int ScaledHeight) ScaledSize
        {
            get
            {
                var scaledWidth = TargetWidth;
                var scaledHeight = TargetHeight;

                switch (PrimaryBufferSizeMode)
                {
                    case BufferSizeMode.WidthControlsHeight:
                        scaledWidth = PrimaryBufferSizeReference;
                        scaledHeight = (int)((float)PrimaryBufferSizeReference /
                                             ((float)TargetWidth / (float)TargetHeight));
                        break;
                    case BufferSizeMode.HeightControlsWidth:
                        scaledWidth = (int)((float)PrimaryBufferSizeReference /
                                            ((float)TargetHeight / (float)TargetWidth));
                        scaledHeight = PrimaryBufferSizeReference;
                        break;
                    case BufferSizeMode.Scaled:
                        scaledWidth = (int)(TargetWidth * PrimaryBufferScale);
                        scaledHeight = (int)(TargetHeight * PrimaryBufferScale);
                        break;
                }

                if (EyeMask == StereoTargetEyeMask.None)
                    return (scaledWidth, scaledHeight);

                if (scaledWidth % 2 != 0)
                    scaledWidth++;

                if (scaledHeight % 2 != 0)
                    scaledHeight++;

                return (scaledWidth, scaledHeight);
            }
        }

        public void Prepare()
        {
            if (OutlinablesToRender.Count == 0)
                return;
            
            UseInfoBuffer = OutlinablesToRender.Find(x => x != null && ((x.DrawingMode & (OutlinableDrawingMode.Obstacle | OutlinableDrawingMode.Mask)) != 0 || x.ComplexMaskingMode != ComplexMaskingMode.None)) != null;
            if (UseInfoBuffer)
                return;

            foreach (var target in OutlinablesToRender)
            {
                if ((target.DrawingMode & OutlinableDrawingMode.Normal) == 0)
                    continue;

                if (!CheckDiffers(target))
                    continue;

                UseInfoBuffer = true;
                break;
            }
        }

        private static bool CheckDiffers(Outlinable outlinable)
        {
            if (outlinable.RenderStyle == RenderStyle.Single)
                return CheckIfNotUnit(outlinable.OutlineParameters);
            
            return CheckIfNotUnit(outlinable.FrontParameters) || CheckIfNotUnit(outlinable.BackParameters);
        }

        private static bool CheckIfNotUnit(Outlinable.OutlineProperties parameters)
        {
            return !Mathf.Approximately(parameters.BlurShift, 1.0f) ||
                   !Mathf.Approximately(parameters.DilateShift, 1.0f);
        }

        public void Dispose()
        {
            if (Buffer is IDisposable disposable)
                disposable.Dispose();
            
            Object.DestroyImmediate(BlitMesh);
            MeshPool?.Dispose();
            RTHandlePool?.Dispose();
        }
    }
}