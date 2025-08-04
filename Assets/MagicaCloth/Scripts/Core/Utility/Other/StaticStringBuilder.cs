// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Text;

namespace MagicaCloth
{
    /// <summary>
    /// ?????StringBuilder???
    /// </summary>
    public class StaticStringBuilder
    {
        private static StringBuilder stringBuilder = new StringBuilder(1024);

        /// <summary>
        /// StringBuilder????????????
        /// </summary>
        public static StringBuilder Instance
        {
            get
            {
                return stringBuilder;
            }
        }

        /// <summary>
        /// StringBuilfer?????????
        /// </summary>
        public static void Clear()
        {
            stringBuilder.Length = 0;
        }

        /// <summary>
        /// ?????StringBuilder?????????????
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static StringBuilder Append(params object[] args)
        {
            //stringBuilder.Length = 0;
            for (int i = 0; i < args.Length; i++)
            {
                stringBuilder.Append(args[i]);
            }
            return stringBuilder;
            //return stringBuilder.ToString();
        }

        /// <summary>
        /// ?????StringBuilder???????????????????????????
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static StringBuilder AppendLine(params object[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                stringBuilder.Append(args[i]);
            }
            stringBuilder.Append("\n");
            return stringBuilder;
        }

        /// <summary>
        /// ?????StringBuilder????????
        /// </summary>
        /// <returns></returns>
        public static StringBuilder AppendLine()
        {
            stringBuilder.Append("\n");
            return stringBuilder;
        }

        /// <summary>
        /// ?????????????????????
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string AppendToString(params object[] args)
        {
            stringBuilder.Length = 0;
            for (int i = 0; i < args.Length; i++)
            {
                stringBuilder.Append(args[i]);
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// ?????StringBuilder???????
        /// </summary>
        /// <returns></returns>
        public static new string ToString()
        {
            return stringBuilder.ToString();
        }
    }
}
