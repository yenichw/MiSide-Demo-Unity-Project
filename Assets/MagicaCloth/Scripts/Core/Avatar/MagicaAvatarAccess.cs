// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System;

namespace MagicaCloth
{
    /// <summary>
    /// ??????????
    /// </summary>
    public abstract class MagicaAvatarAccess : IDisposable
    {
        protected MagicaAvatar owner;

        protected MagicaAvatarRuntime Runtime
        {
            get
            {
                return owner.Runtime;
            }
        }

        //=========================================================================================
        /// <summary>
        /// ?????
        /// </summary>
        /// <param name="manager"></param>
        public void SetParent(MagicaAvatar avatar)
        {
            this.owner = avatar;
        }

        /// <summary>
        /// ????
        /// </summary>
        public abstract void Create();

        /// <summary>
        /// ??
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// ???
        /// </summary>
        public abstract void Active();

        /// <summary>
        /// ???
        /// </summary>
        public abstract void Inactive();
    }
}
