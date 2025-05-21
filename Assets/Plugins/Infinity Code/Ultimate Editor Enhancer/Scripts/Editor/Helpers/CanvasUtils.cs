/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using InfinityCode.UltimateEditorEnhancer.Attributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace InfinityCode.UltimateEditorEnhancer
{
    /// <summary>
    /// Methods for Canvas and Event System
    /// </summary>
    public static class CanvasUtils
    {
        /// <summary>
        /// Returns the width of the displayed text
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="font">Font</param>
        /// <param name="fontSize">Font size</param>
        /// <returns>Width of the displayed text</returns>
        public static int CalculateWidthOfMessage(string text, Font font, int fontSize)
        {
            int width = 0;

            char[] arr = text.ToCharArray();

            foreach (char c in arr)
            {
                CharacterInfo characterInfo;
                font.GetCharacterInfo(c, out characterInfo, fontSize);
                width += characterInfo.advance;
            }

            return width;
        }

        /// <summary>
        /// Returns the current canvas or creates a new one
        /// </summary>
        /// <returns></returns>
        public static Canvas GetCanvas()
        {
            Canvas canvas = ObjectHelper.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasGO = new GameObject("Canvas");
                canvasGO.layer = LayerMask.NameToLayer("UI");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();
            }

            GetEventSystem();

            return canvas;
        }

        /// <summary>
        /// Returns the current event system or creates a new one
        /// </summary>
        /// <returns></returns>
        public static EventSystem GetEventSystem()
        {
            EventSystem eventSystem = ObjectHelper.FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            return eventSystem;
        }
    }
}