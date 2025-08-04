// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
namespace MagicaCloth
{
    /// <summary>
    /// ?????????????
    /// </summary>
    public interface IDataVerify
    {
        /// <summary>
        /// ?????????????
        /// </summary>
        /// <returns></returns>
        int GetVersion();

        /// <summary>
        /// ???????????????
        /// </summary>
        /// <returns></returns>
        void CreateVerifyData();

        /// <summary>
        /// ?????????(???????)???
        /// </summary>
        /// <returns></returns>
        Define.Error VerifyData();

        /// <summary>
        /// ?????????????????
        /// </summary>
        /// <returns></returns>
        string GetInformation();
    }
}
