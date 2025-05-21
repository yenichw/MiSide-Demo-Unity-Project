/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Linq;
using System.Text;
using InfinityCode.UltimateEditorEnhancer.Tools;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    [InitializeOnLoad]
    public class TimescaleWindow : EditorWindow
    {
        private bool closeOnLossFocus;
        private bool isPopup;
        
        [NonSerialized]
        private Vector2 scrollPosition;
        
        [NonSerialized]
        private Mode mode = Mode.TimeScale;
        
        [NonSerialized]
        private bool stepperStarted;
        
        [NonSerialized]
        private double lastStepperTime;
        
        [NonSerialized]
        private float stepperRate = 2;

        static TimescaleWindow()
        {
            Timer.OnLeftClick += OnTimerClick;
        }

        private void DrawStepper()
        {
            EditorGUILayout.BeginHorizontal();
            
            EditorGUI.BeginChangeCheck();
            stepperStarted = GUILayout.Toggle(stepperStarted, stepperStarted? "Stop": "Start", GUI.skin.button, GUILayout.ExpandWidth(false));
            if (EditorGUI.EndChangeCheck())
            {
                if (stepperStarted)
                {
                    EditorApplication.update -= UpdateStepper;
                    EditorApplication.update += UpdateStepper;
                    lastStepperTime = EditorApplication.timeSinceStartup;
                }
                else EditorApplication.update -= UpdateStepper;
            }
            
            stepperRate = EditorGUILayout.Slider("Steps per second", stepperRate, 0, 100);

            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("0.1")) stepperRate = 0.1f;
            if (GUILayout.Button("0.25")) stepperRate = 0.25f;
            if (GUILayout.Button("0.5")) stepperRate = 0.5f;
            if (GUILayout.Button("1")) stepperRate = 1;
            if (GUILayout.Button("2")) stepperRate = 2;
            if (GUILayout.Button("5")) stepperRate = 5;
            if (GUILayout.Button("..."))
            {
                InputDialog.Show("Enter steps per second", stepperRate.ToString(), s =>
                {
                    float v;
                    if (float.TryParse(s, out v))
                    {
                        if (v < 0) EditorUtility.DisplayDialog("Error", "Steps per second is out of range. The value cannot be less than 0.0.", "OK");
                        else stepperRate = v;
                    }
                });
            }


            EditorGUILayout.EndHorizontal();
        }

        private void DrawTimer()
        {
            float time = Time.time;
            int totalSec = Mathf.FloorToInt(time);
            int hour = totalSec / 3600;
            int min = totalSec / 60 % 60;
            int sec = totalSec % 60;
            int ms = Mathf.RoundToInt((time - (int) time) * 1000);

            StringBuilder builder = StaticStringBuilder.Start();

            if (hour > 0) builder.Append(hour).Append(":");
            if (min < 10) builder.Append("0");
            builder.Append(min).Append(":");
            if (sec < 10) builder.Append("0");
            builder.Append(sec).Append(".");
            if (ms < 100) builder.Append("0");
            if (ms < 10) builder.Append("0");
            builder.Append(ms);

            EditorGUILayout.LabelField("Time From Start", builder.ToString(), EditorStyles.textArea);
        }

        private static void DrawTimeScale()
        {
            Time.timeScale = EditorGUILayout.Slider("Timescale", Time.timeScale, 0, 100);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("0.1")) Time.timeScale = 0.1f;
            if (GUILayout.Button("0.25")) Time.timeScale = 0.25f;
            if (GUILayout.Button("0.5")) Time.timeScale = 0.5f;
            if (GUILayout.Button("1")) Time.timeScale = 1f;
            if (GUILayout.Button("2")) Time.timeScale = 2f;
            if (GUILayout.Button("5")) Time.timeScale = 5f;
            if (GUILayout.Button("..."))
            {
                InputDialog.Show("Enter Timescale (0-100)", Time.timeScale.ToString(), s =>
                {
                    float v;
                    if (float.TryParse(s, out v))
                    {
                        if (v < 0) EditorUtility.DisplayDialog("Error", "TimeScale is out of range. The value cannot be less than 0.0.", "OK");
                        else if (v > 100) EditorUtility.DisplayDialog("Error", "TimeScale is out of range. When running in the editor this value needs to be less than or equal to 100.0", "OK");
                        else Time.timeScale = v;
                    }
                });
            }


            EditorGUILayout.EndHorizontal();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            EditorGUI.BeginChangeCheck();
            bool value = GUILayout.Toggle(mode == Mode.TimeScale, "TimeScale", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
            if (EditorGUI.EndChangeCheck())
            {
                if (value)
                {
                    mode = Mode.TimeScale;
                    EditorApplication.update -= UpdateStepper;
                }
            }

            if (Application.isPlaying)
            {
                EditorGUI.BeginChangeCheck();
                value = GUILayout.Toggle(mode == Mode.Stepper, "Stepper", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
                if (EditorGUI.EndChangeCheck())
                {
                    if (value)
                    {
                        mode = Mode.Stepper;
                        EditorApplication.isPaused = true;
                    }
                }
            }

            EditorGUILayout.Space();

            if (isPopup)
            {
                if (GUILayout.Button(EditorIconContents.sceneLoadIn.image, EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                {
                    Rect pos = position;
                    Close();
                    TimescaleWindow wnd = ShowWindow(true);
                    wnd.position = pos;
                }   
            }

            if (GUILayout.Button("?", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
            {
                Links.OpenDocumentation("timer");
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void OnDestroy()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.update -= Repaint;
            EditorApplication.update -= UpdateStepper;
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            if (EditorApplication.isPlaying)
            {
                EditorApplication.update -= Repaint;
                EditorApplication.update += Repaint;
            }
        }

        private void OnGUI()
        {
            if (closeOnLossFocus && focusedWindow != this && focusedWindow != this)
            {
                Close();
                return;
            }

            Event e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                e.Use();
                Close();
                return;
            }

            if (isPopup)
            {
                float sizeY = 65;
                if (Application.isPlaying) sizeY += 20;

                if (Math.Abs(position.height - sizeY) > 0.1f)
                {
                    Rect r = position;
                    r.height = sizeY;
                    minSize = r.size;
                    position = r;
                }
            }
            
            DrawToolbar();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            if (Application.isPlaying) DrawTimer();
            
            if (mode == Mode.TimeScale) DrawTimeScale();
            else if (mode == Mode.Stepper) DrawStepper();

            EditorGUILayout.EndScrollView();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            mode = Mode.TimeScale;
            stepperStarted = false;
            EditorApplication.update -= UpdateStepper;
            
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                EditorApplication.update -= Repaint;
                EditorApplication.update += Repaint;
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                EditorApplication.update -= Repaint;
            }
        }

        private static void OnTimerClick()
        {
            TimescaleWindow[] windows = UnityEngine.Resources.FindObjectsOfTypeAll<TimescaleWindow>();
            if (windows.Any(w => w.closeOnLossFocus))
            {
                foreach (TimescaleWindow window in windows)
                {
                    if (window.closeOnLossFocus) window.Close();
                }
            }
            else
            {
                Rect rect = GUILayoutUtils.lastRect;
                ShowPopupWindow(rect.position + new Vector2(0, rect.height + 5));
            }
        }

        public static EditorWindow ShowPopupWindow(Vector2 position)
        {
            TimescaleWindow wnd = CreateInstance<TimescaleWindow>();
            wnd.titleContent = new GUIContent("Timescale");
            position = GUIUtility.GUIToScreenPoint(position);
            Vector2 size = new Vector2(300, 44);
            Rect rect = new Rect(position, size);
            wnd.minSize = rect.size;
            wnd.position = rect;
            wnd.isPopup = true;
            wnd.closeOnLossFocus = true;
            wnd.ShowPopup();
            wnd.Focus();

            return wnd;
        }
        
        public static TimescaleWindow ShowWindow(bool forceNew)
        {
            TimescaleWindow wnd;
            if (forceNew)
            {
                wnd = CreateInstance<TimescaleWindow>();
                wnd.titleContent = new GUIContent("Timescale");
            }
            else wnd = GetWindow<TimescaleWindow>("Timescale");
            Rect rect = wnd.position;
            rect.size = new Vector2(300, 40);
            wnd.minSize = rect.size;
            wnd.position = rect;
            if (forceNew) wnd.Show();
            return wnd;
        }

        [MenuItem(WindowsHelper.MenuPath + "Timescale", false, MenuItemOrder.Timescale)]
        public static void ShowWindow()
        {
            ShowWindow(false);
        }

        private void UpdateStepper()
        {
            if (Application.isPlaying)
            {
                if (EditorApplication.isPaused)
                {
                    if (stepperRate <= 0) return;
                    
                    float stepInterval = 1 / stepperRate;
                    if (EditorApplication.timeSinceStartup - lastStepperTime > stepInterval)
                    {
                        lastStepperTime = EditorApplication.timeSinceStartup;
                        EditorApplication.Step();
                    }
                }
                else
                {
                    stepperStarted = false;
                    EditorApplication.update -= UpdateStepper;
                }
            }
            else
            {
                mode = Mode.TimeScale;
                EditorApplication.update -= UpdateStepper;
            }
        }

        private enum Mode
        {
            TimeScale,
            Stepper
        }
    }
}