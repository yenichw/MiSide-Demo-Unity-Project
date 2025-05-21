using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bewildered.SmartLibrary
{
    public interface IPreviewGenerator
    {
        public PreviewRenderer Renderer
        {
            get;
        }

        public Texture2D Generate(Object target);
        
        public Texture OpenLiveGeneration(Object target);

        public void CloseLiveGeneration();

        public void Cleanup();
    }
    
    public abstract class PreviewGeneratorBase<T> : IPreviewGenerator where T : Object
    {
        private T _previousLiveTarget;
        private bool _didRenderPrevious;
        
        public PreviewRenderer Renderer { get; }

        public PreviewGeneratorBase(PreviewRenderer renderer)
        {
            Renderer = renderer;
        }

        Texture2D IPreviewGenerator.Generate(Object target)
        {
            if (target is T obj)
                return Generate(obj);

            return null;
        }

        public Texture2D Generate(T target)
        {
            Vector3 cachedCameraPos = Renderer.Camera.transform.position;
            Quaternion cachedCameraRot = Renderer.Camera.transform.rotation;
            
            Renderer.BeginRender();
            bool doRender = InitializeRenderTarget(target, false);
            
            if (doRender)
            {
                OnRender(target);
                Renderer.Render();
            }
            
            CleanupRenderTarget();

            Texture2D result = null;

            if (doRender)
                result = Renderer.EndStaticPreview();
            else
                Renderer.FinishFrame();

            Renderer.Camera.transform.position = cachedCameraPos;
            Renderer.Camera.transform.rotation = cachedCameraRot;

            _previousLiveTarget = target;
            
            return result;
        }

        Texture IPreviewGenerator.OpenLiveGeneration(Object target)
        {
            if (target is T obj)
                return GenerateLive(obj);

            return null;
        }
        
        public Texture GenerateLive(T target)
        {
            if (target == _previousLiveTarget && !_didRenderPrevious)
            {
                return null;
            }

            Renderer.BeginRender();

            bool doRender = true;
            
            if (target != _previousLiveTarget)
            {
                doRender = InitializeRenderTarget(target, true);
            }
            
            if (doRender)
            {
                OnRender(target);
                Renderer.Render();
            }
            
            if (target != _previousLiveTarget)
            {
                _didRenderPrevious = doRender;
                _previousLiveTarget = target;
            }

            Texture result = null;

            if (doRender)
                result = Renderer.EndLivePreview();
            else
                Renderer.FinishFrame();

            return result;
        }

        public void CloseLiveGeneration()
        {
            _previousLiveTarget = null;
            CleanupRenderTarget();
        }

        protected abstract bool InitializeRenderTarget(T target, bool isLive);

        protected virtual void OnRender(T target) { }

        protected virtual void CleanupRenderTarget() { }
        
        
        public virtual void Cleanup()
        {
            CleanupRenderTarget();
        }
    }
}
