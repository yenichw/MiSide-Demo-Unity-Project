/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Interceptors
{
    public abstract class Interceptor
    {
        protected static Harmony harmony;
        private static int _isAppleM;
        protected MethodInfo patch;

        public static bool isAppleM
        {
            get
            {
                if (_isAppleM == 0) _isAppleM = SystemInfo.processorType.Contains("Apple M") ? 1 : -1;
                return _isAppleM == 1;
            }
        }

        protected virtual InitType initType
        {
            get => InitType.immediately;
        }

        protected abstract MethodInfo originalMethod { get; }

        protected virtual string prefixMethodName
        {
            get => null;
        }

        protected virtual string postfixMethodName
        {
            get => null;
        }

        protected virtual void Init()
        {

        }

        [InitializeOnLoadMethod]
        private static async void InitInterceptors()
        {
            TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<Interceptor>();
            List<Interceptor> phaseOne = new List<Interceptor>();
            List<Interceptor> phaseTwo = new List<Interceptor>();

            harmony = new Harmony("InfinityCode.UltimateEditorEnhancer");

            foreach (Type type in types)
            {
                if (type.IsAbstract) continue;
                Interceptor interceptor = Activator.CreateInstance(type) as Interceptor;
                interceptor.Init();
                if (interceptor.initType == InitType.immediately) phaseOne.Add(interceptor);
                else phaseTwo.Add(interceptor);
            }

            foreach (Interceptor interceptor in phaseOne)
            {
                try
                {
                    interceptor.Patch();
                }
                catch
                {
                    
                }
            }

            while (GUISkinRef.GetCurrent() == null) await Task.Delay(1);

            Debug.unityLogger.logEnabled = false;
            foreach (Interceptor interceptor in phaseTwo)
            {
                try
                {
                    interceptor.Patch();
                }
                catch
                {

                }
            }
            Debug.unityLogger.logEnabled = true;
        }

        protected virtual void Patch()
        {
            if (!Prefs.unsafeFeatures) return;
            if (isAppleM) return;

            if (patch != null) return;

            MethodInfo original = originalMethod;
            if (original == null) return;

            try
            {
                HarmonyMethod prefix = null;
                HarmonyMethod postfix = null;

                string _prefixName = prefixMethodName;
                if (!string.IsNullOrEmpty(_prefixName))
                {
                    MethodInfo prefixMethod = AccessTools.Method(GetType(), _prefixName);
                    if (prefixMethod != null) prefix = new HarmonyMethod(prefixMethod);
                }

                string _postfixName = postfixMethodName;
                if (!string.IsNullOrEmpty(_postfixName))
                {
                    MethodInfo postfixMethod = AccessTools.Method(GetType(), _postfixName);
                    if (postfixMethod != null) postfix = new HarmonyMethod(postfixMethod);
                }

                patch = harmony.Patch(original, prefix, postfix);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        protected virtual void Unpatch()
        {
            if (harmony == null || originalMethod == null || patch == null) return;

            if (!string.IsNullOrEmpty(prefixMethodName)) harmony.Unpatch(originalMethod, AccessTools.Method(GetType(), prefixMethodName));
            if (!string.IsNullOrEmpty(postfixMethodName)) harmony.Unpatch(originalMethod, AccessTools.Method(GetType(), postfixMethodName));
            patch = null;
        }

        protected enum InitType
        {
            immediately,
            gui
        }
    }
}