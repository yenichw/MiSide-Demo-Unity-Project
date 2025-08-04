// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MagicaCloth
{
    /// <summary>
    /// ???????
    /// ·?????????????
    /// ·PC????????????????????
    /// </summary>
    public class SimpleInputManager : CreateSingleton<SimpleInputManager>
    {
        // ??????
        private const int MaxFinger = 3;

        /// <summary>
        /// ???????(cm)
        /// </summary>
        public float tapRadiusCm = 0.5f;

        /// <summary>
        /// ????????(cm)
        /// </summary>
        public float flickRangeCm = 0.01f;

        /// <summary>
        /// ????????(cm/s)
        /// </summary>
        public float flickCheckSpeed = 1.0f;

        /// <summary>
        /// ?????????????·??????????
        /// </summary>
        public float mouseWheelSpeed = 5.0f;

        // ??????
        private int mainFingerId = -1;
        private int subFingerId = -1;
        private Vector2[] downPos;              // ??????(?????)
        private Vector2[] lastPos;
        private Vector2[] flickDownPos;         // ??????(?????)
        private float[] flickDownTime;
        private float lastTime = 0;             // ??????????????

        // ????????
        private bool mobilePlatform = false;

        // ???????????????
        private bool[] mouseDown;
        private Vector2[] mouseOldMovePos;

        // ?????
        private float screenDpi;                // ?????DPI?
        private float screenDpc;                // ?????Dots per cm?(1cm?????????)

        //------------------------------ ??????????/??????????? ------------------
        // ???????
        // ??????????????ID?????(?????)???????
        public static UnityAction<int, Vector2> OnTouchDown;

        // ????
        // ??????????????????????ID?????(?????)???(???????/s)???(cm/s)???????
        public static UnityAction<int, Vector2, Vector2, Vector2> OnTouchMove;

        // ?????????????????????????ID?????(?????)???(???????/s)???(cm/s)???????
        public static UnityAction<int, Vector2, Vector2, Vector2> OnDoubleTouchMove;

        // ???????
        // ?????????????ID???(?????)???????
        public static UnityAction<int, Vector2> OnTouchUp;

        // ??????????
        // ???????????(????????)????????????ID???????(?????)???????
        public static UnityAction<int, Vector2> OnTouchMoveCancel;

        // ?????
        // ??????????????ID?????(?????)???????
        public static UnityAction<int, Vector2> OnTouchTap;

        // ??????
        // ??????????????????ID?????(?????)???????(???????/s)???(cm/s)???????
        public static UnityAction<int, Vector2, Vector2, Vector2> OnTouchFlick;

        // ?????/?????
        // ?????/??????(???????/s)???(cm/s)???????
        public static UnityAction<float, float> OnTouchPinch;

        // ????????(Androidde????????PC?? BackSpace ???)
        public static UnityAction OnBackButton;

        //=========================================================================================
        /// <summary>
        /// Reload Domain ??
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            InitMember();
        }

        //=========================================================================================
        protected override void InitSingleton()
        {
            // ???????
            CalcScreenDpi();

            // ?????
            downPos = new Vector2[MaxFinger];
            lastPos = new Vector2[MaxFinger];
            flickDownPos = new Vector2[MaxFinger];
            flickDownTime = new float[MaxFinger];

            // ????
            mouseDown = new bool[3];
            mouseOldMovePos = new Vector2[3];

            AllResetTouchInfo();

            // ?????????????? 
            mobilePlatform = Application.isMobilePlatform;
        }

        void Update()
        {
            // ??????????
            if (mobilePlatform)
            {
                // ?????????? 
                UpdateMobile();
            }
            else
            {
                // ??????????? 
                UpdateMouse();
            }
        }

        //=========================================================================================
        /// <summary>
        /// ??????DPI?(Dots per inchi)1?????????????????
        /// </summary>
        public static float ScreenDpi
        {
            get
            {
                return Instance.screenDpi;
            }
        }

        /// <summary>
        /// ??????DPC?(Dots per cm)1cm??????????????
        /// </summary>
        public static float ScreenDpc
        {
            get
            {
                return Instance.screenDpc;
            }
        }

        /// <summary>
        /// ?????Dpi/Dpc????
        /// </summary>
        private void CalcScreenDpi()
        {
            screenDpi = Screen.dpi;
            if (screenDpi == 0.0f)
            {
                screenDpi = 96; // ???
            }
            screenDpc = screenDpi / 2.54f; // ????cm???
        }

        // ???????????
        private void AllResetTouchInfo()
        {
            mainFingerId = -1;
            subFingerId = -1;
            for (int i = 0; i < 3; i++)
            {
                mouseDown[i] = false;
            }
        }

        public int GetTouchCount()
        {
            return Input.touchCount;
        }

        public bool IsUI()
        {
            if (mobilePlatform)
            {
                // ?????????? 
                return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
            }
            else
            {
                // ??????????? 
                return EventSystem.current.IsPointerOverGameObject();
            }
        }

        //=========================================================================================
        /// <summary>
        /// ?????????
        /// </summary>
        private void UpdateMobile()
        {
            int count = Input.touchCount;

            if (count == 0)
            {
                AllResetTouchInfo();

                // ??????
                if (Application.platform == RuntimePlatform.Android)
                {
                    if (Input.GetKey(KeyCode.Escape) && lastTime + 0.2f < Time.time)
                    {
                        lastTime = Time.time;
                        if (OnBackButton != null)
                        {
                            OnBackButton();
                        }
                        return;
                    }
                }
            }
            else
            {
                // ??? 
                for (int i = 0; i < count; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    int fid = touch.fingerId;

                    // ?????ID?0?1??????? 
                    if (fid >= 2)
                    {
                        continue;
                    }

                    if (touch.phase == TouchPhase.Began)
                    {
                        if (IsUI())
                            continue;
                        // down pos
                        downPos[fid] = touch.position;
                        lastPos[fid] = touch.position;
                        flickDownPos[fid] = touch.position;

                        if (fid == 0)
                        {
                            mainFingerId = fid;
                        }
                        else
                        {
                            subFingerId = fid;
                        }

                        // Down??????????? 
                        if (fid == 0)
                        {
                            flickDownTime[fid] = Time.time;
                            if (OnTouchDown != null)
                            {
                                OnTouchDown(fid, touch.position);
                            }
                        }
                    }
                    else if (touch.phase == TouchPhase.Moved)
                    {
                        // ?????/????? 
                        if (mainFingerId >= 0 && subFingerId >= 0)
                        {
                            Vector2 t1pos = Vector2.zero;
                            Vector2 t2pos = Vector2.zero;
                            Vector2 t1delta = Vector2.zero;
                            Vector2 t2delta = Vector2.zero;

                            int setcnt = 0;
                            for (int j = 0; j < count; j++)
                            {
                                Touch t = Input.GetTouch(j);
                                if (mainFingerId == t.fingerId)
                                {
                                    t1pos = t.position;
                                    t1delta = t.deltaPosition;
                                    setcnt++;
                                }
                                else if (subFingerId == t.fingerId)
                                {
                                    t2pos = t.position;
                                    t2delta = t.deltaPosition;
                                    setcnt++;
                                }
                            }

                            if (setcnt == 2)
                            {
                                float nowdist = Vector2.Distance(t1pos, t2pos);
                                float olddist = Vector2.Distance(t1pos - t1delta, t2pos - t2delta);
                                float dist = nowdist - olddist;

                                // cm/s???
                                float distcm = dist / screenDpc; // ???(cm)
                                float speedcm = distcm / Time.deltaTime; // ??(cm/s)

                                // ??????????
                                float speedscr = (dist / (Screen.width + Screen.height) * 0.5f) / Time.deltaTime;

                                // ?????(???(cm), ??(cm/s))
                                if (OnTouchPinch != null)
                                {
                                    OnTouchPinch(speedscr, speedcm);
                                }
                            }

                            if (fid == 0)
                            {
                                Vector2 distVec2 = touch.position - lastPos[fid];
                                Vector2 distcm = distVec2 / screenDpc; // ???(cm)
                                Vector2 speedcm = distcm / Time.deltaTime; // ??(cm/s)

                                // ??(???????)
                                Vector2 speedscr = CalcScreenRatioVector(distVec2) / Time.deltaTime;

                                // ????(????????????(???????), ??(cm/s))
                                if (OnDoubleTouchMove != null)
                                {
                                    OnDoubleTouchMove(fid, touch.position, speedscr, speedcm);
                                }

                                lastPos[fid] = touch.position;
                            }
                        }
                        else
                        {
                            // Move??????????? 
                            if (fid == 0 && mainFingerId >= 0)
                            {
                                Vector2 distVec2 = touch.position - lastPos[fid];
                                Vector2 distcm = distVec2 / screenDpc; // ???(cm)
                                Vector2 speedcm = distcm / Time.deltaTime; // ??(cm/s)

                                // ??(???????)
                                Vector2 speedscr = CalcScreenRatioVector(distVec2) / Time.deltaTime;

                                // ????(????????????(???????), ??(cm/s))
                                if (OnTouchMove != null)
                                {
                                    OnTouchMove(fid, touch.position, speedscr, speedcm);
                                }

                                // ???????????
                                flickDownPos[fid] = (flickDownPos[fid] + touch.position) * 0.5f;
                                flickDownTime[fid] = Time.time;
                            }

                            lastPos[fid] = touch.position;
                        }
                    }
                    else if (touch.phase == TouchPhase.Ended)
                    {
                        // ?????ID????? 
                        if (fid == 0)
                        {
                            mainFingerId = -1;
                            subFingerId = -1;
                        }
                        else
                        {
                            subFingerId = -1;
                        }

                        // End, Tap ??????????? 
                        if (fid == 0)
                        {
                            // ?????
                            float dist = Vector2.Distance(downPos[fid], touch.position);
                            float distcm = dist / screenDpc;

                            if (distcm <= tapRadiusCm)
                            {
                                // ?????
                                if (OnTouchTap != null)
                                {
                                    OnTouchTap(fid, touch.position);
                                }
                            }
                            // ??????
                            else
                            {
                                CheckFlic(fid, downPos[fid], touch.position, flickDownPos[fid], flickDownTime[fid]);
                            }

                            // ????????
                            if (OnTouchUp != null)
                            {
                                OnTouchUp(fid, touch.position);
                            }
                        }
                    }
                    else if (touch.phase == TouchPhase.Canceled)
                    {
                        // ?????ID????? 
                        if (fid == 0)
                        {
                            mainFingerId = -1;
                            subFingerId = -1;
                        }
                        else
                        {
                            subFingerId = -1;
                        }

                        // Cancel??????????? 
                        if (fid == 0)
                        {
                            if (OnTouchMoveCancel != null)
                            {
                                OnTouchMoveCancel(fid, touch.position);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ????????????????????
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        private Vector2 CalcScreenRatioVector(Vector2 vec)
        {
            return new Vector2(vec.x / Screen.width, vec.y / Screen.height);
        }

        /// <summary>
        /// ??????
        /// </summary>
        /// <param name="oldpos"></param>
        /// <param name="nowpos"></param>
        /// <param name="downpos"></param>
        /// <param name="flicktime"></param>
        /// <returns></returns>
        private bool CheckFlic(int fid, Vector2 oldpos, Vector2 nowpos, Vector2 downpos, float flicktime)
        {
            // ??????
            float dist = Vector2.Distance(nowpos, downpos);
            float distcm = dist / screenDpc;
            if (distcm > flickRangeCm)
            {
                {
                    // ???????cm??????cm/s?????
                    Vector2 distVec = (nowpos - downpos);
                    Vector2 distVec2 = distVec / screenDpc; // cm???(???(cm))
                    float timeInterval = Time.time - flicktime;
                    float speedX = distVec2.x / timeInterval; // ??(cm/s)
                    float speedY = distVec2.y / timeInterval; // ??(cm/s)

                    //Develop.Log("distVec", distVec * 100);
                    //Develop.Log("sppedX:", speedX, " speedY:", speedY);

                    if (Mathf.Abs(speedX) >= flickCheckSpeed || Mathf.Abs(speedY) >= flickCheckSpeed)
                    {
                        // ??????(???????,??(???????/s),??(cm/s))
                        if (OnTouchFlick != null)
                        {
                            OnTouchFlick(fid, nowpos, CalcScreenRatioVector(distVec) / timeInterval, new Vector2(speedX, speedY));
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        //=========================================================================================
        /// <summary>
        /// ??????(PC?)
        /// ???????????
        /// ·???????????
        /// ·?????/????????????
        /// </summary>
        private void UpdateMouse()
        {
            // BackSpace ? Android ???????????????
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                if (OnBackButton != null)
                    OnBackButton();
                return;
            }

            for (int i = 0; i < 3; i++)
            {
                // ?????????
                if (Input.GetMouseButtonDown(i))
                {
                    if (IsUI())
                        continue;

                    if (mouseDown[i] == false && i == 0)
                    {
                        flickDownTime[i] = Time.time;
                    }
                    mouseDown[i] = true;

                    // ???????
                    downPos[i] = Input.mousePosition;
                    mouseOldMovePos[i] = Input.mousePosition;
                    if (i == 0)
                        flickDownPos[i] = Input.mousePosition;

                    // ????????????
                    if (OnTouchDown != null)
                        OnTouchDown(i, Input.mousePosition);
                }

                // ?????????
                if (Input.GetMouseButtonUp(i) && mouseDown[i])
                {
                    mouseDown[i] = false;

                    // ??????
                    if (i == 0)
                    {
                        CheckFlic(i, mouseOldMovePos[i], Input.mousePosition, flickDownPos[i], flickDownTime[i]);
                    }

                    mouseOldMovePos[i] = Vector2.zero;

                    // ??????????
                    if (OnTouchUp != null)
                        OnTouchUp(i, Input.mousePosition);

                    // ?????
                    float distcm = Vector2.Distance(downPos[0], Input.mousePosition) / screenDpc;
                    if (distcm <= tapRadiusCm)
                    {
                        if (OnTouchTap != null)
                            OnTouchTap(i, Input.mousePosition);
                    }
                }

                // ??
                if (mouseDown[i])
                {
                    Vector2 spos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                    Vector2 delta = spos - mouseOldMovePos[i];

                    if (spos != mouseOldMovePos[i])
                    {
                        // ??
                        Vector3 deltacm = delta / screenDpc; // ???(cm)
                        Vector2 speedcm = deltacm / Time.deltaTime; // ??(cm/s)

                        // ????(????????????(???????/s)???(cm/s))
                        if (OnTouchMove != null)
                            OnTouchMove(i, Input.mousePosition, CalcScreenRatioVector(delta) / Time.deltaTime, speedcm);
                    }

                    mouseOldMovePos[i] = Input.mousePosition;

                    // ???????????
                    flickDownPos[i] = (flickDownPos[i] + spos) * 0.5f;
                    flickDownTime[i] = Time.time;
                }

            }

            // ?????/??? 
            float w = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(w) > 0.01f)
            {
                // ?????????????????????????? 
                w *= mouseWheelSpeed;

                float speedcm = w / Time.deltaTime;
                float speedscr = (w / (Screen.width + Screen.height) * 0.5f) / Time.deltaTime;

                // ??(??(???????/s)???(cm/s)
                if (OnTouchPinch != null)
                    OnTouchPinch(speedscr, speedcm);
            }
        }
    }
}
