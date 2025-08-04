// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// ????????????????
    /// ·?????????????
    /// ·???????????
    /// ·DontDestroyOnLoad??
    /// ·?????Instance??????
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class CreateSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;

        /// <summary>
        /// ??????
        /// </summary>
        private static T initInstance;

        private static bool isDestroy;


        /// <summary>
        /// Reload Domain ??
        /// ?????????????????[RuntimeInitializeOnLoadMethod]??????????
        /// ????????????[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        /// ?????????????????
        /// </summary>
        protected static void InitMember()
        {
            instance = null;
            initInstance = null;
            isDestroy = false;
        }

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    // FindObjectOfType????????????????!
                    // ????????????????????????!
                    instance = FindObjectOfType<T>();

                    if (instance == null && Application.isPlaying)
                    {
                        var obj = new GameObject(typeof(T).Name);
                        instance = obj.AddComponent<T>();
                    }
                }

                // ???
                InitInstance();

                return instance;
            }
        }

        private static void InitInstance()
        {
            if (initInstance == null && instance != null && Application.isPlaying)
            {
                // ?????????????????????????
                DontDestroyOnLoad(instance.gameObject);

                // ???????
                var s = instance as CreateSingleton<T>;
                s.InitSingleton();

                initInstance = instance;
            }
        }

        /// <summary>
        /// ??????????????True?????
        /// </summary>
        /// <returns></returns>
        public static bool IsInstance()
        {
            return instance != null && isDestroy == false;
        }

        /// <summary>
        /// Awake()??????????
        /// </summary>
        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                InitInstance();
            }
            else if(instance != this)
            {
                // 2?????????????
                var s = instance as CreateSingleton<T>;
                s.DuplicateDetection(this as T);

                // 2???????????????
                Destroy(this.gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            // ??????????????????????
            if (instance == this)
            {
                isDestroy = true;
            }
        }

        /// <summary>
        /// 2??????????????????
        /// </summary>
        /// <param name="duplicate"></param>
        protected virtual void DuplicateDetection(T duplicate) { }

        /// <summary>
        /// ?????
        /// </summary>
        protected abstract void InitSingleton();
    }
}
