// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;

namespace MagicaCloth
{
    public static class DataUtility
    {
        /// <summary>
        /// 2?????????1??Uint?????????
        /// ??16??????16????v0/v1???????
        /// ?????????????????????
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static uint PackPair(int v0, int v1)
        {
            if (v0 > v1)
            {
                return (uint)v1 << 16 | (uint)v0 & 0xffff;
            }
            else
            {
                return (uint)v0 << 16 | (uint)v1 & 0xffff;
            }
        }

        /// <summary>
        /// ???????2????(v0/v1)?????
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        public static void UnpackPair(uint pack, out int v0, out int v1)
        {
            // ??????
            v0 = (int)((pack >> 16) & 0xffff);
            v1 = (int)(pack & 0xffff);
        }

        /// <summary>
        /// 2??int?1??uint????????
        /// </summary>
        /// <param name="hi"></param>
        /// <param name="low"></param>
        /// <returns></returns>
        public static uint Pack16(int hi, int low)
        {
            return (uint)hi << 16 | (uint)low & 0xffff;
        }

        /// <summary>
        /// uint??????????16bit?int?????
        /// </summary>
        /// <param name="pack"></param>
        /// <returns></returns>
        public static int Unpack16Hi(uint pack)
        {
            return (int)((pack >> 16) & 0xffff);
        }

        /// <summary>
        /// uint??????????16bit?int?????
        /// </summary>
        /// <param name="pack"></param>
        /// <returns></returns>
        public static int Unpack16Low(uint pack)
        {
            return (int)(pack & 0xffff);
        }

        /// <summary>
        /// 2??int?1??uint????????(4bit/28bit)
        /// </summary>
        /// <param name="hi"></param>
        /// <param name="low"></param>
        /// <returns></returns>
        public static uint Pack4_28(int hi, int low)
        {
            return (uint)hi << 28 | (uint)low & 0xfffffff;
        }

        /// <summary>
        /// uint??????????4bit?int?????
        /// </summary>
        /// <param name="pack"></param>
        /// <returns></returns>
        public static int Unpack4_28Hi(uint pack)
        {
            return (int)((pack >> 28) & 0xf);
        }

        /// <summary>
        /// uint??????????28bit?int?????
        /// </summary>
        /// <param name="pack"></param>
        /// <returns></returns>
        public static int Unpack4_28Low(uint pack)
        {
            return (int)(pack & 0xfffffff);
        }

        /// <summary>
        /// 2??int?1??uint????????(8bit/24bit)
        /// </summary>
        /// <param name="hi"></param>
        /// <param name="low"></param>
        /// <returns></returns>
        public static uint Pack8_24(int hi, int low)
        {
            return (uint)hi << 24 | (uint)low & 0xffffff;
        }

        /// <summary>
        /// uint??????????8bit?int?????
        /// </summary>
        /// <param name="pack"></param>
        /// <returns></returns>
        public static int Unpack8_24Hi(uint pack)
        {
            return (int)((pack >> 24) & 0xf);
        }

        /// <summary>
        /// uint??????????24bit?int?????
        /// </summary>
        /// <param name="pack"></param>
        /// <returns></returns>
        public static int Unpack8_24Low(uint pack)
        {
            return (int)(pack & 0xffffff);
        }

        /// <summary>
        /// 2??int?1??ulong????????
        /// </summary>
        /// <param name="hi"></param>
        /// <param name="low"></param>
        /// <returns></returns>
        public static ulong Pack32(int hi, int low)
        {
            return (ulong)hi << 32 | (ulong)low & 0xffffffff;
        }

        /// <summary>
        /// ulong????????????????
        /// </summary>
        /// <param name="pack"></param>
        /// <returns></returns>
        public static int Unpack32Hi(ulong pack)
        {
            return (int)((pack >> 32) & 0xffffffff);
        }

        /// <summary>
        /// ulong????????????????
        /// </summary>
        /// <param name="pack"></param>
        /// <returns></returns>
        public static int Unpack32Low(ulong pack)
        {
            return (int)(pack & 0xffffffff);
        }

        /// <summary>
        /// 3?????????1??ulong?????????
        /// ??????????????????
        /// ??????????????????????
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static ulong PackTriple(int v0, int v1, int v2)
        {
            List<ulong> indexList = new List<ulong>();
            indexList.Add((ulong)v0);
            indexList.Add((ulong)v1);
            indexList.Add((ulong)v2);
            indexList.Sort();
            ulong hash = (indexList[0] << 32) | (indexList[1] << 16) | (indexList[2]);
            return hash;
        }

        /// <summary>
        /// ???????3?????????
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        public static void UnpackTriple(ulong pack, out int v0, out int v1, out int v2)
        {
            v0 = (int)((pack >> 32) & 0xffff);
            v1 = (int)((pack >> 16) & 0xffff);
            v2 = (int)(pack & 0xffff);
        }

        /// <summary>
        /// 4?????????1??ulong?????????
        /// ??????????????????
        /// ??????????????????????
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static ulong PackQuater(int v0, int v1, int v2, int v3)
        {
            List<ulong> indexList = new List<ulong>();
            indexList.Add((ulong)v0);
            indexList.Add((ulong)v1);
            indexList.Add((ulong)v2);
            indexList.Add((ulong)v3);
            indexList.Sort();

            ulong hash = (indexList[0] << 48) | (indexList[1] << 32) | (indexList[2] << 16) | (indexList[3]);
            return hash;
        }

        /// <summary>
        /// ???????4?????????
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        public static void UnpackQuater(ulong pack, out int v0, out int v1, out int v2, out int v3)
        {
            v0 = (int)((pack >> 48) & 0xffff);
            v1 = (int)((pack >> 32) & 0xffff);
            v2 = (int)((pack >> 16) & 0xffff);
            v3 = (int)(pack & 0xffff);
        }
    }
}
