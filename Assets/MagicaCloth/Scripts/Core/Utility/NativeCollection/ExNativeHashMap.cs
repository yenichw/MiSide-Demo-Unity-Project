// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System;
using System.Collections.Generic;
using Unity.Collections;

namespace MagicaCloth
{
    /// <summary>
    /// NativeHashMap??????
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class ExNativeHashMap<TKey, TValue>
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
#if MAGICACLOTH_USE_COLLECTIONS_130 && !MAGICACLOTH_USE_COLLECTIONS_200
        NativeParallelHashMap<TKey, TValue> nativeHashMap;
#else
        NativeHashMap<TKey, TValue> nativeHashMap;
#endif

        /// <summary>
        /// ????????????
        /// ????????????????????????
        /// </summary>
        int nativeLength;

        /// <summary>
        /// ???????
        /// </summary>
        HashSet<TKey> useKeySet = new HashSet<TKey>();

        //=========================================================================================
        public ExNativeHashMap()
        {
#if MAGICACLOTH_USE_COLLECTIONS_130 && !MAGICACLOTH_USE_COLLECTIONS_200
            nativeHashMap = new NativeParallelHashMap<TKey, TValue>(1, Allocator.Persistent);
#else
            nativeHashMap = new NativeHashMap<TKey, TValue>(1, Allocator.Persistent);
#endif
            nativeLength = NativeCount;
        }

        public void Dispose()
        {
            if (nativeHashMap.IsCreated)
            {
                nativeHashMap.Dispose();
            }
        }

        private int NativeCount
        {
            get
            {
#if MAGICACLOTH_USE_COLLECTIONS_200
                return nativeHashMap.Count;
#else
                return nativeHashMap.Count();
#endif
            }
        }

        //=========================================================================================
        /// <summary>
        /// ?????
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(TKey key, TValue value)
        {
            if (nativeHashMap.TryAdd(key, value) == false)
            {
                // ??????????????????????
                nativeHashMap.Remove(key);
                nativeHashMap.TryAdd(key, value);
            }
            useKeySet.Add(key);
            nativeLength = NativeCount;
        }

        /// <summary>
        /// ?????
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValue Get(TKey key)
        {
            TValue data;
            nativeHashMap.TryGetValue(key, out data);
            return data;
        }

        /// <summary>
        /// ??????
        /// </summary>
        /// <param name="func">true??????</param>
        public void Remove(Func<TKey, TValue, bool> func)
        {
            List<TKey> removeKey = new List<TKey>();
            foreach (TKey key in useKeySet)
            {
                TValue data;
                if (nativeHashMap.TryGetValue(key, out data))
                {
                    // ????
                    if (func(key, data))
                    {
                        // ??
                        nativeHashMap.Remove(key);
                        removeKey.Add(key);
                    }
                }
            }

            foreach (var key in removeKey)
                useKeySet.Remove(key);
            nativeLength = NativeCount;
        }

        /// <summary>
        /// ???????
        /// </summary>
        /// <param name="func">true??????</param>
        /// <param name="rdata">??????????????????????????</param>
        public void Replace(Func<TKey, TValue, bool> func, Func<TValue, TValue> datafunc)
        {
            foreach (var key in useKeySet)
            {
                TValue data;
                if (nativeHashMap.TryGetValue(key, out data))
                {
                    // ????
                    if (func(key, data))
                    {
                        // ????
                        var newdata = datafunc(data);
                        nativeHashMap.Remove(key); // ????????????????
                        nativeHashMap.TryAdd(key, newdata);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// ?????
        /// </summary>
        /// <param name="key"></param>
        public void Remove(TKey key)
        {
            nativeHashMap.Remove(key);
            nativeLength = 0;
            useKeySet.Remove(key);
        }


        /// <summary>
        /// ????????????????
        /// </summary>
        public int Count
        {
            get
            {
                return nativeLength;
            }
        }

        public void Clear()
        {
            nativeHashMap.Clear();
            nativeLength = 0;
            useKeySet.Clear();
        }

        /// <summary>
        /// ???NativeHashMap?????
        /// </summary>
        /// <returns></returns>
#if MAGICACLOTH_USE_COLLECTIONS_130 && !MAGICACLOTH_USE_COLLECTIONS_200
        public NativeParallelHashMap<TKey, TValue> Map
#else
        public NativeHashMap<TKey, TValue> Map
#endif
        {
            get
            {
                return nativeHashMap;
            }
        }

        /// <summary>
        /// ????????????
        /// </summary>
        public HashSet<TKey> UseKeySet
        {
            get
            {
                return useKeySet;
            }
        }
    }
}
