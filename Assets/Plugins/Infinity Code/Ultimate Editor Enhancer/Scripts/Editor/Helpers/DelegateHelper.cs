/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static class DelegateHelper
    {
        public static void InvokeGUI(Delegate target, params object[] args)
        {
            if (target == null) return;

            Delegate[] hooks = target.GetInvocationList();

            for (int i = 0; i < hooks.Length; i++)
            {
                try
                {
                    hooks[i].DynamicInvoke(args);
                }
                catch (ExitGUIException e)
                {
                    throw e;
                }
                catch (Exception e)
                {
                    Log.Add(e);
                }
            }
        }
    }
}