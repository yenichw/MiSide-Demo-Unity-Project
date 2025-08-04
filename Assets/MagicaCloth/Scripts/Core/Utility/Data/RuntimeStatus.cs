// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;

namespace MagicaCloth
{
    /// <summary>
    /// ???????????????
    /// </summary>
    public class RuntimeStatus
    {
        // ???????????
        /// <summary>
        /// ??????????????
        /// </summary>
        bool initStart;

        /// <summary>
        /// ?????????true???(???????????)
        /// </summary>
        bool init;

        /// <summary>
        /// ????????????true???
        /// </summary>
        bool initError;

        /// <summary>
        /// ???????????????
        /// </summary>
        bool enable;

        /// <summary>
        /// ????????????????????????(v1.2)
        /// </summary>
        bool userEnable = true;

        /// <summary>
        /// ???????????????true???
        /// </summary>
        bool runtimeError;

        /// <summary>
        /// ??????????????true???
        /// </summary>
        bool dispose;

        /// <summary>
        /// ?????????????
        /// </summary>
        bool isActive;

        /// <summary>
        /// ????????????????
        /// </summary>
        bool isDirty;

        /// <summary>
        /// ??(?)?????
        /// ????????????????????????????????????
        /// </summary>
        internal HashSet<RuntimeStatus> parentStatusSet { get; private set; } = new HashSet<RuntimeStatus>();

        /// <summary>
        /// ??(?)?????
        /// ????????????????????????????UpdateStatus()?????
        /// </summary>
        internal HashSet<RuntimeStatus> childStatusSet { get; private set; } = new HashSet<RuntimeStatus>();

        //=========================================================================================
        /// <summary>
        /// ??????????????
        /// </summary>
        internal System.Action UpdateStatusAction;

        /// <summary>
        /// ???????????????????
        /// </summary>
        internal System.Action DisconnectedAction;

        /// <summary>
        /// ??????????????
        /// </summary>
        internal System.Func<System.Object> OwnerFunc;

        //=========================================================================================
        /// <summary>
        /// ??????????
        /// </summary>
        public bool IsActive
        {
            get
            {
                return isActive && !dispose;
            }
        }

        /// <summary>
        /// ??????????????
        /// </summary>
        /// <value></value>
        public bool IsInitStart
        {
            get
            {
                return initStart;
            }
        }

        /// <summary>
        /// ??????????(???????????)
        /// </summary>
        public bool IsInitComplete
        {
            get
            {
                return init;
            }
        }

        /// <summary>
        /// ???????????????
        /// </summary>
        public bool IsInitSuccess
        {
            get
            {
                return init && !initError;
            }
        }

        /// <summary>
        /// ???????????????
        /// </summary>
        public bool IsInitError
        {
            get
            {
                return init && initError;
            }
        }

        /// <summary>
        /// ?????????
        /// </summary>
        public bool IsDispose
        {
            get
            {
                return dispose;
            }
        }

        /// <summary>
        /// ???????????????
        /// </summary>
        public bool IsDirty => isDirty;

        /// <summary>
        /// ?????????????
        /// </summary>
        public void SetInitStart()
        {
            initStart = true;
        }

        /// <summary>
        /// ????????????
        /// </summary>
        public void SetInitComplete()
        {
            init = true;
        }

        /// <summary>
        /// ?????????????
        /// </summary>
        public void SetInitError()
        {
            initError = true;
        }

        /// <summary>
        /// ??????????
        /// </summary>
        /// <param name="sw"></param>
        /// <returns>?????????????true???</returns>
        public bool SetEnable(bool sw)
        {
            bool ret = enable != sw;
            enable = sw;
            return ret;
        }

        /// <summary>
        /// ???????????????????
        /// </summary>
        /// <param name="sw"></param>
        /// <returns>?????????????true???</returns>
        public bool SetUserEnable(bool sw)
        {
            bool ret = userEnable != sw;
            userEnable = sw;
            return ret;
        }

        /// <summary>
        /// ????????????????
        /// </summary>
        /// <param name="sw"></param>
        /// <returns>?????????????true???</returns>
        public bool SetRuntimeError(bool sw)
        {
            bool ret = runtimeError != sw;
            runtimeError = sw;
            return ret;
        }

        /// <summary>
        /// ?????????
        /// </summary>
        /// <returns></returns>
        public void SetDispose()
        {
            dispose = true;
        }

        /// <summary>
        /// ?????????
        /// </summary>
        public void SetDirty()
        {
            isDirty = true;
        }

        /// <summary>
        /// ???????????
        /// </summary>
        public void ClearDirty()
        {
            isDirty = false;
        }

        /// <summary>
        /// ???????????????
        /// </summary>
        /// <returns>?????????????????true???</returns>
        public bool UpdateStatus()
        {
            if (dispose)
                return false;

            // ???????????????????(?)????????????????????????????
            var active = init && !initError && enable && userEnable && !runtimeError && IsParentStatusActive();

            // ???????????????????
            if (MagicaPhysicsManager.IsInstance())
                active = active && MagicaPhysicsManager.Instance.IsActive;

            if (active != isActive)
            {
                isActive = active;

                // ??????
                UpdateStatusAction?.Invoke();

                // ??????(?)?????????????
                foreach (var status in childStatusSet)
                {
                    status?.UpdateStatus();
                }

                return true;
            }
            else
                return false;
        }

        //=========================================================================================
        /// <summary>
        /// ??(?)??????????
        /// </summary>
        /// <param name="status"></param>
        public void AddParentStatus(RuntimeStatus status)
        {
            parentStatusSet.Add(status);
        }

        /// <summary>
        /// ??(?)??????????
        /// </summary>
        /// <param name="status"></param>
        public void RemoveParentStatus(RuntimeStatus status)
        {
            parentStatusSet.Remove(status);
            parentStatusSet.Remove(null);

            // ?????????
            if (parentStatusSet.Count == 0 && childStatusSet.Count == 0)
                DisconnectedAction?.Invoke();
        }

        /// <summary>
        /// ??(?)??????????
        /// </summary>
        /// <param name="status"></param>
        public void AddChildStatus(RuntimeStatus status)
        {
            childStatusSet.Add(status);
        }

        /// <summary>
        /// ??(?)??????????
        /// </summary>
        /// <param name="status"></param>
        public void RemoveChildStatus(RuntimeStatus status)
        {
            childStatusSet.Remove(status);
            childStatusSet.Remove(null);

            // ?????????
            if (parentStatusSet.Count == 0 && childStatusSet.Count == 0)
                DisconnectedAction?.Invoke();
        }

        /// <summary>
        /// ???????????
        /// </summary>
        /// <param name="parent"></param>
        public void LinkParentStatus(RuntimeStatus parent)
        {
            AddParentStatus(parent);
            parent.AddChildStatus(this);
        }

        /// <summary>
        /// ???????????????
        /// </summary>
        /// <param name="parent"></param>
        public void UnlinkParentStatus(RuntimeStatus parent)
        {
            RemoveParentStatus(parent);
            parent.RemoveChildStatus(this);
        }

        /// <summary>
        /// ??(?)??????1???????????
        /// ??????????????????????
        /// </summary>
        /// <returns></returns>
        bool IsParentStatusActive()
        {
            if (parentStatusSet.Count == 0)
                return true;

            foreach (var status in parentStatusSet)
            {
                if (status != null && status.IsActive)
                    return true;
            }

            return false;
        }
    }
}
