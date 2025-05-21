using System;
using System.Collections.Generic;
using EPOOutline.Utility;
using UnityEngine;
using UnityEngine.Rendering;

namespace EPOOutline
{
    public class RTHandlePool : IDisposable
    {
        private class PoolSegment : IDisposable
        {
            private List<RTHandle> allocated = new List<RTHandle>();
            private Queue<RTHandle> free = new Queue<RTHandle>();
            
            public RTHandle GetFree()
            {
                if (free.Count != 0)
                    return free.Dequeue();

                var newCreated = OutlineEffect.HandleSystem.Alloc(new RenderTargetIdentifier());
                free.Enqueue(newCreated);
                allocated.Add(newCreated);

                return newCreated;
            }
            
            public void ReleaseAll()
            {
                free.Clear();
                foreach (var entry in allocated)
                    free.Enqueue(entry);
            }

            public void Dispose()
            {
                foreach (var entry in allocated)
                {
#if !UNITY_6000_0_OR_NEWER
                    RTHandleUtility.RemoveDelegates(entry);
#endif
                    entry.Release();
                }

                allocated.Clear();
                free.Clear();
            }
        }

        private readonly PoolSegment textureSegment = new PoolSegment();
        private readonly PoolSegment rtiSegment = new PoolSegment();
        
        public RTHandle Allocate(Texture target)
        {
            var entry = textureSegment.GetFree();
            entry.SetTexture(target);

            return entry;
        }

        public RTHandle Allocate(RenderTargetIdentifier target)
        {
            var entry = rtiSegment.GetFree();
            entry.SetRenderTargetIdentifier(target);
            return entry;
        }

        public void ReleaseAll()
        {
            textureSegment.ReleaseAll();
            rtiSegment.ReleaseAll();
        }

        public void Dispose()
        {
            textureSegment.Dispose();
            rtiSegment.Dispose();
        }
    }
}