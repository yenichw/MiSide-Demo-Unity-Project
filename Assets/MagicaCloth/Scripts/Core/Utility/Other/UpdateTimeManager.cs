// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// ???????
    /// </summary>
    [System.Serializable]
    public class UpdateTimeManager
    {
        // ????????
        public enum UpdateCount
        {
            _60 = 60,
            _90_Default = 90,
            _120 = 120,
            _150 = 150,
            _180 = 180,
        }

        // 1???????
        [SerializeField]
        private UpdateCount updatePerSeccond = UpdateCount._90_Default;

        // ?????
        public enum UpdateMode
        {
            UnscaledTime = 0,   // ?????
            OncePerFrame = 1,   // ????(??1?????1?)

            DelayUnscaledTime = 10, // ?????(????)
        }
        [SerializeField]
        private UpdateMode updateMode = UpdateMode.UnscaledTime;

        // ????(UnscaledTime/OncePerFrame??)
        public enum UpdateLocation
        {
            AfterLateUpdate = 0,
            BeforeLateUpdate = 1,
        }
        [SerializeField]
        private UpdateLocation updateLocation = UpdateLocation.AfterLateUpdate;

        // ????????????
        private float timeScale = 1.0f;

        /// <summary>
        /// ???????????
        /// </summary>
        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float futurePredictionRate = 1.0f;

        /// <summary>
        /// ??????????(Unity2019.2.13??)
        /// </summary>
        [SerializeField]
        private bool updateBoneScale = false;


        private int fixedUpdateCount = 0;

        //=========================================================================================
        public void ResetFixedUpdateCount()
        {
            fixedUpdateCount = 0;
        }

        public void AddFixedUpdateCount()
        {
            fixedUpdateCount++;
        }

        public int FixedUpdateCount
        {
            get
            {
                return fixedUpdateCount;
            }
        }

        /// <summary>
        /// ???????????
        /// </summary>
        /// <returns></returns>
        public UpdateMode GetUpdateMode()
        {
            return updateMode;
        }

        public void SetUpdateMode(UpdateMode mode)
        {
            updateMode = mode;
        }

        /// <summary>
        /// ???????
        /// </summary>
        /// <returns></returns>
        public UpdateLocation GetUpdateLocation()
        {
            return updateLocation;
        }

        public void SetUpdateLocation(UpdateLocation location)
        {
            updateLocation = location;
        }

        /// <summary>
        /// 1???????
        /// </summary>
        public int UpdatePerSecond
        {
            get
            {
                return (int)updatePerSeccond;
            }
        }

        public void SetUpdatePerSecond(UpdateCount ucount)
        {
            updatePerSeccond = ucount;
        }

        /// <summary>
        /// ??????
        /// </summary>
        public float UpdateIntervalTime
        {
            get
            {
                return 1.0f / UpdatePerSecond;
            }
        }

        /// <summary>
        /// ???(90ups????????)
        /// 60fps = 1.5 / 90ups = 1.0f / 120fps = 0.75
        /// </summary>
        public float UpdatePower
        {
            get
            {
                float power = 90.0f / (float)UpdatePerSecond;
                //power = Mathf.Pow(power, 0.3f); // ??
                return power;
            }
        }

        /// <summary>
        /// ???????
        /// 1.0?????????????????????
        /// ?????????????????
        /// </summary>
        public float TimeScale
        {
            get
            {
                return timeScale;
            }
            set
            {
                timeScale = Mathf.Clamp01(value);
            }
        }

        /// <summary>
        /// ???????????
        /// </summary>
        public float DeltaTime
        {
            get
            {
                return Time.deltaTime;
            }
        }

        public float PhysicsDeltaTime
        {
            get
            {
                return Time.fixedDeltaTime * fixedUpdateCount;
            }
        }

        /// <summary>
        /// ?????????????(=??FPS)
        /// </summary>
        public float AverageDeltaTime
        {
            get
            {
                return Time.smoothDeltaTime;
            }
        }

        /// <summary>
        /// ?????????
        /// </summary>
        public bool IsUnscaledUpdate
        {
            get
            {
                return updateMode == UpdateMode.UnscaledTime || updateMode == UpdateMode.DelayUnscaledTime;
            }
        }

        /// <summary>
        /// ??????
        /// </summary>
        public bool IsDelay
        {
            get
            {
                return updateMode == UpdateMode.DelayUnscaledTime;
            }
        }

        /// <summary>
        /// ???????????(0.0-1.0)
        /// </summary>
        public float FuturePredictionRate
        {
            get
            {
                return futurePredictionRate;
            }
            set
            {
                futurePredictionRate = Mathf.Clamp01(value);
            }
        }

        /// <summary>
        /// ???????????(Unity2019.2.13??)
        /// </summary>
        public bool UpdateBoneScale
        {
            get
            {
                return updateBoneScale;
            }
            set
            {
                updateBoneScale = value;
            }
        }

        /// <summary>
        /// ???????????????????
        /// </summary>
        /// <returns></returns>
        public int WorkerMaximumCount
        {
            get
            {
                return Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerMaximumCount;
            }
        }
    }
}
