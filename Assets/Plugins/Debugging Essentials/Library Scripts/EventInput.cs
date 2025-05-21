using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using UnityEngine;

namespace DebuggingEssentials
{
    [Serializable]
    public class AdvancedKey
    {
        public KeyCode keyCode;
        public int specialKeys;

        public AdvancedKey(KeyCode keyCode, bool shift = false, bool control = false, bool alt = false)
        {
            this.keyCode = keyCode;
            specialKeys = (shift ? SpecialKeyFlags.shift : 0) | (control ? SpecialKeyFlags.control : 0) | (alt ? SpecialKeyFlags.alt : 0);
        }

        public bool GetSpecialKeys()
        {
            if ((specialKeys & SpecialKeyFlags.shift) != 0 && !EventInput.isShiftKey) return false;
            if ((specialKeys & SpecialKeyFlags.control) != 0 && !EventInput.isControlKey) return false;
            if ((specialKeys & SpecialKeyFlags.alt) != 0 && !EventInput.isAltKey) return false;

            return true;
        }

        public bool GetKeyUp(Event currentEvent)
        {
            return (currentEvent.type == EventType.KeyUp && currentEvent.keyCode == keyCode && GetSpecialKeys());
        }

        public bool GetKey(Event currentEvent)
        {
            return (currentEvent.keyCode == keyCode && GetSpecialKeys());
        }

        public bool OnlyGetKey(Event currentEvent)
        {
            return (currentEvent.keyCode == keyCode);
        }
    }

    public static class SpecialKeyFlags
    {
        public const int shift = 1;
        public const int control = 2;
        public const int alt = 4;
    }

    public static class EventInput
    {
        public static bool isGamePauseKeyDown;
        public static bool isFocusCameraKey, isAlignWithViewKey, isMoveToViewKey;
        public static bool isMouseButtonDown0, isMouseButtonDown1, isMouseButtonDown2;
        public static bool isMouseButtonUp0;
        public static bool isKeyW, isKeyS, isKeyA, isKeyD, isKeyQ, isKeyE;
        public static bool isKeyDownD, isKeyDownDelete, isKeyDownFollowCamera;
        public static bool isKeyUpD = true, isKeyUpDelete = true, isKeyUpFollowCamera = true, isGamePauseKeyUp = true;
        public static bool isShiftKey, isControlKey, isAltKey;

        public static float mouseScrollDelta;
        public static Vector2 mousePos, mousePosInvY;

        static int oldFrame;
 
        public static void ResetInput()
        {
            int frame = Time.frameCount;

            if (frame != oldFrame)
            {
                isKeyDownD = false;
                isKeyDownDelete = false;
                isKeyDownFollowCamera = false;
                isGamePauseKeyDown = false;
            }

            oldFrame = Time.frameCount;
        }

        public static void GetInput()
        {
            Event currentEvent = Event.current;

            KeyCode eventKeyCode = currentEvent.keyCode;
            EventType eventType = currentEvent.type;

            mousePos = mousePosInvY = currentEvent.mousePosition;
            mousePosInvY.y = Screen.height - mousePos.y;

            isShiftKey = currentEvent.shift;
            isControlKey = currentEvent.control;
            isAltKey = currentEvent.alt;

            int eventButton = currentEvent.button;

            if (eventType == EventType.ScrollWheel) mouseScrollDelta = currentEvent.delta.y;
            else mouseScrollDelta = 0;

            GetKeyDown(ref isFocusCameraKey, RuntimeInspector.instance.focusCameraKey, eventKeyCode, eventType);
            GetKeyDown(ref isAlignWithViewKey, RuntimeInspector.instance.alignWithViewKey, eventKeyCode, eventType);
            GetKeyDown(ref isMoveToViewKey, RuntimeInspector.instance.moveToViewKey, eventKeyCode, eventType);
            
            GetKeyDown(ref isKeyW, KeyCode.W, eventKeyCode, eventType);
            GetKeyDown(ref isKeyS, KeyCode.S, eventKeyCode, eventType);
            GetKeyDown(ref isKeyA, KeyCode.A, eventKeyCode, eventType);
            GetKeyDown(ref isKeyD, KeyCode.D, eventKeyCode, eventType);
            GetKeyDown(ref isKeyQ, KeyCode.Q, eventKeyCode, eventType);
            GetKeyDown(ref isKeyE, KeyCode.E, eventKeyCode, eventType);

            if (isKeyUpD) GetKeyDown(ref isKeyDownD, KeyCode.D, eventKeyCode, eventType);
            if (isKeyUpDelete) GetKeyDown(ref isKeyDownDelete, KeyCode.Delete, eventKeyCode, eventType);
            if (isKeyUpFollowCamera) GetKeyDown(ref isKeyDownFollowCamera, RuntimeInspector.instance.followCameraKey, eventKeyCode, eventType);
            if (isGamePauseKeyUp) GetKeyDown(ref isGamePauseKeyDown, RuntimeInspector.instance.gamePauseKey, eventKeyCode, eventType);

            GetKeyUp(ref isGamePauseKeyUp, RuntimeInspector.instance.gamePauseKey, eventKeyCode, eventType);
            GetKeyUp(ref isKeyUpD, KeyCode.D, eventKeyCode, eventType);
            GetKeyUp(ref isKeyUpDelete, KeyCode.Delete, eventKeyCode, eventType);
            GetKeyUp(ref isKeyUpFollowCamera, RuntimeInspector.instance.followCameraKey, eventKeyCode, eventType);

            GetMouseButtonDown(ref isMouseButtonDown0, 0, eventButton, eventType);
            GetMouseButtonDown(ref isMouseButtonDown1, 1, eventButton, eventType);
            GetMouseButtonDown(ref isMouseButtonDown2, 2, eventButton, eventType);

            GetMouseButtonUp(ref isMouseButtonUp0, 0, eventButton, eventType);
        }

        static void GetKeyDown(ref bool isKey, KeyCode keyCode, KeyCode eventKeyCode, EventType eventType)
        {
            if (eventType == EventType.KeyDown)
            {
                if (eventKeyCode == keyCode) isKey = true;
            }
            if (eventType == EventType.KeyUp)
            {
                if (eventKeyCode == keyCode) isKey = false;
            }
        }

        static void GetKeyDown(ref bool isKey, AdvancedKey advancedKey, KeyCode eventKeyCode, EventType eventType)
        {
            if (eventType == EventType.KeyDown)
            {
                if (eventKeyCode == advancedKey.keyCode && advancedKey.GetSpecialKeys()) isKey = true;
            }
            if (eventType == EventType.KeyUp)
            {
                if (eventKeyCode == advancedKey.keyCode) isKey = false;
            }
        }

        static void GetKeyUp(ref bool isKey, KeyCode keyCode, KeyCode eventKeyCode, EventType eventType)
        {
            if (eventType == EventType.KeyDown)
            {
                if (eventKeyCode == keyCode) isKey = false;
            }
            if (eventType == EventType.KeyUp)
            {
                if (eventKeyCode == keyCode) isKey = true;
            }
        }

        static void GetKeyUp(ref bool isKey, AdvancedKey advancedKey, KeyCode eventKeyCode, EventType eventType)
        {
            if (eventType == EventType.KeyDown)
            {
                if (eventKeyCode == advancedKey.keyCode && advancedKey.GetSpecialKeys()) isKey = false;
            }
            if (eventType == EventType.KeyUp)
            {
                if (eventKeyCode == advancedKey.keyCode) isKey = true;
            }
        }

        static void GetMouseButtonDown(ref bool isButton, int button, int eventButton, EventType eventType)
        {
            if (eventType == EventType.MouseDown)
            {
                if (eventButton == button) isButton = true;
            }
            if (eventType == EventType.MouseUp)
            {
                if (eventButton == button) isButton = false;
            }
        }

        static void GetMouseButtonUp(ref bool isButton, int button, int eventButton, EventType eventType)
        {
            if (eventType == EventType.MouseDown)
            {
                if (eventButton == button) isButton = false;
            }
            if (eventType == EventType.MouseUp)
            {
                if (eventButton == button) isButton = true;
            }
        }
    }
}