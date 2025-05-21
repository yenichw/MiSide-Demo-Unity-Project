/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class AudioUtilsRef
    {
        private static MethodInfo _isClipPlayingMethod;
        private static MethodInfo _playClipMethod;
        private static MethodInfo _stopAllClipsMethod;
        private static MethodInfo _stopClipMethod;
        private static Type _type;

        private static MethodInfo isClipPlayingMethod
        {
            get
            {
                if (_isClipPlayingMethod == null)
                {
#if UNITY_2020_2_OR_NEWER
                    _isClipPlayingMethod = type.GetMethod("IsPreviewClipPlaying", Reflection.StaticLookup);
#else
                    _isClipPlayingMethod = type.GetMethod("IsClipPlaying", Reflection.StaticLookup, null, new[] { typeof(AudioClip) }, null);
#endif
                }
                return _isClipPlayingMethod;
            }
        }

        private static MethodInfo playClipMethod
        {
            get
            {
                if (_playClipMethod == null)
                {
#if UNITY_2020_2_OR_NEWER
                    _playClipMethod = type.GetMethod("PlayPreviewClip", Reflection.StaticLookup, null, new[] { typeof(AudioClip), typeof(int), typeof(bool) }, null);
#else
                    _playClipMethod = type.GetMethod("PlayClip", Reflection.StaticLookup, null, new[] { typeof(AudioClip), typeof(int), typeof(bool) }, null);
#endif
                }
                return _playClipMethod;
            }
        }

        private static MethodInfo stopAllClipsMethod
        {
            get
            {
                if (_stopAllClipsMethod == null)
                {
#if UNITY_2020_2_OR_NEWER
                    _stopAllClipsMethod = type.GetMethod("StopAllPreviewClips", Reflection.StaticLookup);
#else
                    _stopAllClipsMethod = type.GetMethod("StopAllClips", Reflection.StaticLookup);
#endif
                }
                return _stopAllClipsMethod;
            }
        }

        private static MethodInfo stopClipMethod
        {
            get
            {
                if (_stopClipMethod == null)
                {
#if UNITY_2020_2_OR_NEWER
                    _stopClipMethod = type.GetMethod("PausePreviewClip", Reflection.StaticLookup);
#else
                    _stopClipMethod = type.GetMethod("StopClip", Reflection.StaticLookup, null, new[] { typeof(AudioClip) }, null);
#endif
                }
                return _stopClipMethod;
            }
        }

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("AudioUtil");
                return _type;
            }
        }

        public static bool IsClipPlaying(AudioClip clip)
        {
#if UNITY_2020_2_OR_NEWER
            return (bool)isClipPlayingMethod.Invoke(null, Array.Empty<object>());
#else
            return (bool)isClipPlayingMethod.Invoke(null, new object[] {clip});
#endif
        }

        public static void PlayClip(AudioClip clip, int startSample = 0, bool loop = false)
        {
            playClipMethod.Invoke(null, new object[] { clip, startSample, loop});
        }

        public static void StopAllClips()
        {
            stopAllClipsMethod.Invoke(null, null);
        }

        public static void StopClip(AudioClip clip)
        {
#if UNITY_2020_2_OR_NEWER
            stopClipMethod.Invoke(null, Array.Empty<object>());
#else 
            stopClipMethod.Invoke(null, new object[] {clip});
#endif
        }
    }
}