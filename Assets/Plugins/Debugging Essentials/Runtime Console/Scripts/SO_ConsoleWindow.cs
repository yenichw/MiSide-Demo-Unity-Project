using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DebuggingEssentials
{
    // [CreateAssetMenu(fileName = "ConsoleWindow_Data", menuName = "ScriptableObjects/DebuggingEssentials/ConsoleWindow_Data", order = 1)]
    public class SO_ConsoleWindow : SO_BaseWindow
    {
        public WindowSettings consoleWindow;

        public GUISkin skin;
        public GUISkin skinAutoComplete;
        public Texture texDot;
        public Texture texArrow;
        public Texture texCornerScale;
        public Texture texStackOn;
        public Texture texStackOff;
        public CountIcon logIcon;
        public CountIcon warningIcon;
        public CountIcon errorIcon;
        public CountIcon exceptionIcon;
    }

    [Serializable]
    public class CountIcon
    {
        public Texture texOn;
        public Texture texOff;
        [NonSerialized] public int count;

        public GUIContent GetGUIContent() { return Helper.GetGUIContent(count.ToString(), (count > 0 ? texOn : texOff)); }
    }

}