using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bewildered.SmartLibrary
{
    public static class LivePreviewer
    {
        public static IReadOnlyList<Type> SupportedTypes { get; } = new Type[] { typeof(GameObject) };
        
        public class PreviewInstance
        {
            private PreviewRenderer _renderer;
            private IPreviewGenerator _generator;
            private Object _target;
            
            public Object Target
            {
                get { return _target; }
            }

            public PreviewInstance(Object target)
            {
                _target = target;
                _renderer = new PreviewRenderer(PreviewResolution.x512);
                _generator = Previewer.CreateGeneratorForTarget(target, _renderer);
            }

            public Texture Generate()
            {
                return _generator.Generate(_target);
            }
            
            public Texture GenerateLive()
            {
                return _generator.OpenLiveGeneration(_target);
            }
            
            public void Dispose()
            {
                _generator.CloseLiveGeneration();
                
                _renderer.Cleanup();
                _generator.Cleanup();
            }
        }

        public static PreviewInstance GetPreviewInstanceFromGuid(string guid)
        {
            var asset = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid));
            return GetPreviewInstance(asset);
        }

        public static PreviewInstance GetPreviewInstance(Object target)
        {
            if (!Previewer.HasSupportedGenerator(target))
                return null;

            return new PreviewInstance(target);
        }
    }
}