using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DebuggingEssentials
{
    [DefaultExecutionOrder(99000000)]
    public class NavigationCamera : MonoBehaviour
    {
        public static NavigationCamera instance;
        public static float fov;
        public static bool followTarget;
        public static Camera cam;

        public SO_NavigationCamera data;

        Transform t;
        GameObject go;

        Vector3 currentSpeed;
        Vector3 position, startPosition, navPosition, followPosition;
        Quaternion rotation, startRotation, navRotation, followRotation;
        Vector2 deltaMouse, mousePosOld;
        
        float tStamp, deltaTime;

        float oldFov;
        float scrollWheel;

        GameObject selectionIndicatorGO;
        Transform selectionIndicatorT;

        public static void ResetStatic()
        {
            followTarget = false;
        }

        void Awake()
        {
            instance = this;

            go = gameObject;
            t = transform;

            cam = GetComponent<Camera>();

            tStamp = Time.realtimeSinceStartup;

            startPosition = navPosition = t.position;
            startRotation = navRotation = t.rotation;

            navRotation = ResetRotZ(navRotation, false);

            fov = oldFov = cam.fieldOfView;

            selectionIndicatorGO = RuntimeInspector.selectionIndicatorGO;
            selectionIndicatorT = selectionIndicatorGO.transform;

            if (followTarget) ResetFollowPosRot();
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;

            RestoreCam();
        }

        void Update()
        {
            if (RuntimeInspector.instance == null)
            {
                Destroy(this);
                return;
            }

            if (EventInput.isKeyDownFollowCamera)
            {
                followTarget = !followTarget;

                if (followTarget) ResetFollowPosRot();
            }

            if (WindowManager.eventHandling.hasEntered) scrollWheel = 0;
            else scrollWheel = -EventInput.mouseScrollDelta * data.mouseScrollWheelMulti.value;
        }

        Quaternion ResetRotZ(Quaternion rot, bool roundTo180 = true)
        {
            Vector3 euler = rot.eulerAngles;
            if (roundTo180) euler.z = Mathf.Round(euler.z / 180) * 180;
            else euler.z = 0;
            return Quaternion.Euler(euler);
        }

        void LateUpdate()
        {
            if (RuntimeInspector.instance == null)
            {
                Destroy(this);
                return;
            }

            deltaMouse = EventInput.mousePos - mousePosOld;
            mousePosOld = EventInput.mousePos;

            float speedMulti = GetSpeedMulti();

            cam.fieldOfView = fov;
            deltaTime = Time.realtimeSinceStartup - tStamp;
            tStamp = Time.realtimeSinceStartup;

            Vector3 speed = Vector3.zero;

            if (followTarget)
            {
                RuntimeInspector.UpdateSelectedIndicatorTransform();
                FollowTarget();
            }

            if (EventInput.isMouseButtonDown1)
            {
                Quaternion oldRot = t.rotation;
                if (followTarget) t.rotation = rotation; else t.rotation = navRotation;

                t.Rotate(0, deltaMouse.x * data.mouseSensitity.value * 0.125f, 0, Space.World);
                t.Rotate(deltaMouse.y * data.mouseSensitity.value * 0.125f, 0, 0, Space.Self);

                if (followTarget) followRotation = Quaternion.Inverse(ResetRotZ(selectionIndicatorT.rotation, false)) * ResetRotZ(t.rotation);
                else navRotation = ResetRotZ(t.rotation);

                t.rotation = oldRot;

                if (EventInput.isKeyW) speed.z = 1;
                else if (EventInput.isKeyS) speed.z = -1;

                if (EventInput.isKeyD) speed.x = 1;
                else if (EventInput.isKeyA) speed.x = -1;

                if (EventInput.isKeyE) speed.y = 1;
                else if (EventInput.isKeyQ) speed.y = -1;

                speed *= speedMulti;
            }

            if (EventInput.isMouseButtonDown2)
            {
                speed.x = -(deltaMouse.x / deltaTime) / 60;
                speed.y = (deltaMouse.y / deltaTime) / 60;

                speed *= speedMulti * data.mouseStrafeMulti.value * 0.1f;
                currentSpeed = speed;
            }
            else Lerp2Way(ref currentSpeed, speed, data.accelMulti.value, data.decelMulti.value);

            GameObject selectionIndicatorGO = RuntimeInspector.selectionIndicatorGO;

            if (selectionIndicatorGO.activeInHierarchy)
            {
                if (EventInput.isFocusCameraKey && !EventInput.isAlignWithViewKey && !EventInput.isMoveToViewKey)
                {
                    ResetFollowPosRot();
                    FollowTarget();
                }
            }

            if (!followTarget)
            {
                t.rotation = rotation = navRotation;

                navPosition += t.TransformDirection(currentSpeed * deltaTime) + (t.forward * scrollWheel * GetSpeedMultiScrollWheel() * deltaTime);
                t.position = position = navPosition;
            }

            if (RuntimeInspector.selectedGO != null)
            {
                if (EventInput.isFocusCameraKey)
                {
                    if (EventInput.isAlignWithViewKey)
                    {
                        RuntimeInspector.selectedGO.transform.position = t.position;
                        RuntimeInspector.selectedGO.transform.forward = t.forward;
                    }
                    else if (EventInput.isMoveToViewKey)
                    {
                        RuntimeInspector.selectedGO.transform.position = t.position + (t.forward * 2);
                    }
                }
            }
        }

        void FollowTarget()
        {
            if (RuntimeInspector.selectedGO == go || RuntimeInspector.selectedGO == RuntimeInspector.selectionIndicatorGO)
            {
                followTarget = false;
                return;
            }

            t.rotation = rotation = navRotation = ResetRotZ(selectionIndicatorT.rotation, false) * followRotation;

            followPosition += selectionIndicatorT.InverseTransformDirection(t.TransformDirection(currentSpeed * deltaTime) + (t.forward * scrollWheel * deltaTime));
            t.position = position = navPosition = selectionIndicatorT.position + (selectionIndicatorT.rotation * followPosition);
        }

        public void ResetFollowPosRot()
        {
            Transform selectionIndicatorT = RuntimeInspector.selectionIndicatorGO.transform;

            t.rotation = selectionIndicatorT.rotation;
            t.position = selectionIndicatorT.position;

            followPosition = new Vector3(0, 0, -2);
            followRotation = Quaternion.identity;
        }

        public void SetCam()
        {
            t.rotation = rotation;
            t.position = position;
        }

        public void RestoreCam()
        {
            t.position = startPosition;
            t.rotation = startRotation;
            cam.fieldOfView = oldFov;
        }

        float GetSpeedMulti()
        {
            if (EventInput.isShiftKey) return data.speedFast.value;
            if (EventInput.isControlKey) return data.speedSlow.value;
            else return data.speedNormal.value;
        }

        float GetSpeedMultiScrollWheel()
        {
            if (EventInput.isShiftKey) return 2;
            if (EventInput.isControlKey) return 0.5f;
            else return 1;
        }

        void Lerp2Way(ref Vector3 v, Vector3 targetV, float upMulti, float downMulti)
        {
            Lerp2Way(ref v.x, targetV.x, upMulti, downMulti);
            Lerp2Way(ref v.y, targetV.y, upMulti, downMulti);
            Lerp2Way(ref v.z, targetV.z, upMulti, downMulti);
        }

        void Lerp2Way(ref float v, float targetV, float upMulti, float downMulti)
        {
            float multi;
            if (Mathf.Abs(v) < Mathf.Abs(targetV)) multi = upMulti; else multi = downMulti;
            v = Mathf.Lerp(v, targetV, multi * deltaTime);
        }
    }
}