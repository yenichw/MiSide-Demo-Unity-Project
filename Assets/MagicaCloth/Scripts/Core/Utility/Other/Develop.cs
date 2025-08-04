// based on the original game.Yen Chezky(yenichw)
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// ??????????
    /// ??????
    /// </summary>
    public static class Develop
    {
        /// <summary>
        /// ??????
        /// </summary>
        /// <param name="str"></param>
        [System.Diagnostics.Conditional("MAGICACLOTH_DEBUG")]
        public static void Log(string str)
        {
            Debug.Log(str);
        }
    }
}
