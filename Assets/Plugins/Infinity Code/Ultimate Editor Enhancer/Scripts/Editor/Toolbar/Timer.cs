/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Tools
{
    [InitializeOnLoad]
    public static class Timer
    {
        public static Action OnLeftClick;
        private static GUIContent content;

        static Timer()
        {
            ToolbarManager.AddLeftToolbar("Timer", OnGUI);
        }

        private static void DrawIcon()
        {
            if (GUILayoutUtils.Button(new GUIContent(Icons.timer, "Timescale"), Styles.appToolbarButtonLeft, GUILayout.Width(32), GUILayout.Height(18)) == ButtonEvent.click)
            {
                Event e = Event.current;

                if (e.button == 0)
                {
                    if (OnLeftClick != null) OnLeftClick();
                }
                else if (e.button == 1) ShowContextMenu();
            }
        }

        private static void DrawTimer()
        {
            float time = Time.time;
            int totalSec = Mathf.FloorToInt(time);
            int hour = totalSec / 3600;
            int min = totalSec / 60 % 60;
            int sec = totalSec % 60;
            int ms = Mathf.RoundToInt((time - (int) time) * 1000);

            float width = 68;

            StringBuilder builder = StaticStringBuilder.Start();
            if (hour > 0)
            {
                builder.Append(hour).Append(":");
                width += EditorStyles.textField.CalcSize(TempContent.Get(hour.ToString())).x;
            }

            if (min < 10) builder.Append("0");
            builder.Append(min).Append(":");
            if (sec < 10) builder.Append("0");
            builder.Append(sec).Append(".");
            if (ms < 100) builder.Append("0");
            if (ms < 10) builder.Append("0");
            builder.Append(ms);

            if (content == null) content = new GUIContent(builder.ToString(), "Time since the start of the game.");
            else content.text = builder.ToString();

            Rect rect = GUILayoutUtility.GetRect(content, EditorStyles.textField, GUILayout.Width(width));
            GUILayoutUtils.lastRect = rect;

            Event e = Event.current;
            if (e.type == EventType.Repaint)
            {
                EditorStyles.textField.Draw(rect, content, false, false, false, false);
                GUI.changed = true;
            }
            else if (e.type == EventType.MouseDown)
            {
                if (rect.Contains(e.mousePosition))
                {
                    if (e.button == 0)
                    {
                        if (OnLeftClick != null) OnLeftClick();
                    }
                    else if (e.button == 1) ShowContextMenu();
                }
            }
        }

        private static void OnGUI()
        {
            if (Prefs.timerMode == TimerMode.icon) DrawIcon();
            else if (Prefs.timerMode == TimerMode.timer)
            {
                if (EditorApplication.isPlaying) DrawTimer();
                else DrawIcon();
            }
        }

        private static void SetMode(TimerMode mode)
        {
            Prefs.timerMode = mode;
            Prefs.Save();
        }

        private static void ShowContextMenu()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Timer"), Prefs.timerMode == TimerMode.timer, () => SetMode(TimerMode.timer));
            menu.AddItem(new GUIContent("Icon"), Prefs.timerMode == TimerMode.icon, () => SetMode(TimerMode.icon));
            menu.AddItem(new GUIContent("Hide"), Prefs.timerMode == TimerMode.hide, () => SetMode(TimerMode.hide));
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Config"), false, Settings.OpenToolbarSettings);
            menu.ShowAsContext();
        }
    }
}