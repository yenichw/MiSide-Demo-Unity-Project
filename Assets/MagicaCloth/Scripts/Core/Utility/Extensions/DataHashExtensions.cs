// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// ???????????????????
    /// ????????GetHashCode()???????????????????????????????
    /// ·???????GetHashCode()?????????????
    /// ·??????????????/???? IDataHash ?????????????int GetDataHash() ????????????
    /// ·??????????????????????????????GetDataHash()??????????
    /// </summary>
    public static class DataHashExtensions
    {
        public const int NullHash = 397610387;
        public const int NumberHash = 932781045;

        /// <summary>
        /// ????Object?GetDataHash()???
        /// ???????GetHashCode()???
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int GetDataHash(this System.Object data)
        {
            var unityObj = data as UnityEngine.Object;
            if (!object.ReferenceEquals(unityObj, null))
            {
                if (unityObj != null)
                {
                    if (unityObj is Transform)
                        return (unityObj as Transform).name.GetHashCode();
                    else if (unityObj is GameObject)
                        return (unityObj as GameObject).name.GetHashCode();
                    else if (unityObj is Mesh)
                    {
                        // ????????????????????
                        var mesh = unityObj as Mesh;
                        int hash = 0;
                        hash += mesh.vertexCount.GetDataHash(); // ????????
                        hash += mesh.triangles.Length.GetDataHash(); // ?????????????
                        hash += mesh.subMeshCount.GetDataHash();
                        hash += mesh.isReadable.GetDataHash();
                        return hash;
                    }
                    else
                        return NumberHash + data.GetHashCode();
                }
                else
                    return NullHash;
            }
            else
            {
                if (data != null)
                    return NumberHash + data.GetHashCode();
                else
                    return NullHash;
            }
        }

        public static int GetDataHash(this IDataHash data)
        {
            return data.GetDataHash();
        }

        //=========================================================================================
        /// <summary>
        /// ???????????????????????
        /// ??????????/???? IDataHash ?????????????int GetDataHash() ????????????
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int GetDataHash<T>(this T[] data)
        {
            int hash = 0;
            if (data != null)
                foreach (var d in data)
                {
                    hash = hash * 31;

                    IDataHash dh = d as IDataHash;
                    if (dh != null)
                        hash += dh.GetDataHash();
                    else
                        hash += d.GetDataHash();
                }

            return hash;
        }

        /// <summary>
        /// ???????????????????
        /// ??????????/???? IDataHash ?????????????int GetDataHash() ????????????
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int GetDataHash<T>(this List<T> data)
        {
            int hash = 0;
            if (data != null)
                foreach (var d in data)
                {
                    hash = hash * 31;

                    IDataHash dh = d as IDataHash;
                    if (dh != null)
                        hash += dh.GetDataHash();
                    else
                        hash += d.GetDataHash();
                }

            return hash;
        }

        /// <summary>
        /// ??????????????????????????
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int GetDataCountHash<T>(this T[] data)
        {
            return data != null ? data.Length.GetDataHash() : NullHash;
        }

        /// <summary>
        /// ???????????????????????
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int GetDataCountHash<T>(this List<T> data)
        {
            return data != null ? data.Count.GetDataHash() : NullHash;
        }

        /// <summary>
        /// Vector3??????????????
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static ulong GetVectorDataHash(Vector3 v)
        {
            var u = math.asuint(v);
            ulong x = u.x;
            ulong y = u.y;
            ulong z = u.z;
            x += 0x68DF0763u;
            y += 0x5A394F9Fu;
            z += 0xE094B323u;
            x = x ^ (x << 13);
            y = y ^ (y >> 17);
            z = z ^ (z << 15);
            x *= 0x9B13B92Du;
            y *= 0x4ABF0813u;
            z *= 0x86068063u;
            return x + y + z + 0xD75513F9u;
        }
    }
}
