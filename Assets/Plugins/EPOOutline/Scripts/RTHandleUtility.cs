using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

namespace EPOOutline.Utility
{
    public static class RTHandleUtility
    {
        private static MethodInfo setTextureInfo;
        private static object[] parameter = new object[1];

#if !UNITY_6000_0_OR_NEWER
        private static Dictionary<RTHandle, Action<RenderTargetIdentifier>> setTextureDelegates =
            new Dictionary<RTHandle, Action<RenderTargetIdentifier>>();

        internal static void RemoveDelegates(RTHandle handle)
        {
            setTextureDelegates.Remove(handle);
        }
#endif
        
        public static void SetTexture(this RTHandle handle, Texture texture)
        {
            if (setTextureInfo == null)
            {
                setTextureInfo = typeof(RTHandle).GetMethod("SetTexture",
                    BindingFlags.Default | BindingFlags.Instance | BindingFlags.NonPublic, null,
                    new Type[] { typeof(Texture) }, null);
            }

            parameter[0] = texture;
            setTextureInfo.Invoke(handle, parameter);
        }

        public static void SetRenderTargetIdentifier(this RTHandle handle, RenderTargetIdentifier identifier)
        {
#if UNITY_6000_0_OR_NEWER
            RTHandleStaticHelpers.SetRTHandleUserManagedWrapper(ref handle, identifier);
#else
            if (!setTextureDelegates.TryGetValue(handle, out Action<RenderTargetIdentifier> delegateToCall))
            {
                var function = typeof(RTHandle).GetMethod("SetTexture",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null, CallingConventions.Standard, new Type[] { typeof(RenderTargetIdentifier) }, null);

                delegateToCall =
                    (Action<RenderTargetIdentifier>)function.CreateDelegate(typeof(Action<RenderTargetIdentifier>),
                        handle);

                setTextureDelegates.Add(handle, delegateToCall);
            }

            delegateToCall(identifier);
#endif
        }
    }
}