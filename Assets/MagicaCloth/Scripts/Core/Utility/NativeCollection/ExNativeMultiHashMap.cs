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
    /// NativeMultiHashMap??????
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class ExNativeMultiHashMap<TKey, TValue>
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        /// <summary>
        /// ????????????
        /// </summary>
#if MAGICACLOTH_USE_COLLECTIONS_130 && !MAGICACLOTH_USE_COLLECTIONS_200
        NativeParallelMultiHashMap<TKey, TValue> nativeMultiHashMap;
#else
        NativeMultiHashMap<TKey, TValue> nativeMultiHashMap;
#endif

        /// <summary>
        /// ????????????
        /// ????????????????????????
        /// </summary>
        int nativeLength;

        /// <summary>
        /// ???????
        /// </summary>
        Dictionary<TKey, int> useKeyDict = new Dictionary<TKey, int>();

        //=========================================================================================
        public ExNativeMultiHashMap()
        {
#if MAGICACLOTH_USE_COLLECTIONS_130 && !MAGICACLOTH_USE_COLLECTIONS_200
            nativeMultiHashMap = new NativeParallelMultiHashMap<TKey, TValue>(1, Allocator.Persistent);
#else
            nativeMultiHashMap = new NativeMultiHashMap<TKey, TValue>(1, Allocator.Persistent);
#endif
            nativeLength = NativeCount;
        }

        public void Dispose()
        {
            if (nativeMultiHashMap.IsCreated)
            {
                nativeMultiHashMap.Dispose();
            }
            nativeLength = 0;
        }

        private int NativeCount
        {
            get
            {
                return nativeMultiHashMap.Count();
            }
        }

        //=========================================================================================
        public bool IsCreated
        {
            get
            {
                return nativeMultiHashMap.IsCreated;
            }
        }

        /// <summary>
        /// ?????
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(TKey key, TValue value)
        {
            nativeMultiHashMap.Add(key, value);

            if (useKeyDict.ContainsKey(key))
                useKeyDict[key] = useKeyDict[key] + 1;
            else
                useKeyDict[key] = 1;

            nativeLength = NativeCount;
        }

        /// <summary>
        /// ?????
        /// ??????????????????!
        /// ??????????????????????????????(???)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Remove(TKey key, TValue value)
        {
            TValue data;
#if MAGICACLOTH_USE_COLLECTIONS_130 && !MAGICACLOTH_USE_COLLECTIONS_200
            NativeParallelMultiHashMapIterator<TKey> iterator;
#else
            NativeMultiHashMapIterator<TKey> iterator;
#endif
            if (nativeMultiHashMap.TryGetFirstValue(key, out data, out iterator))
            {
                do
                {
                    if (data.Equals(value))
                    {
                        // ??
                        nativeMultiHashMap.Remove(iterator);

                        var cnt = useKeyDict[key] - 1;
                        if (cnt == 0)
                            useKeyDict.Remove(key);

                        break;
                    }
                }
                while (nativeMultiHashMap.TryGetNextValue(out data, ref iterator));
            }

            nativeLength = NativeCount;
        }

        /// <summary>
        /// ??????
        /// ?????????????????????!
        /// </summary>
        /// <param name="func">true??????</param>
        public void Remove(Func<TKey, TValue, bool> func)
        {
            List<TKey> removeKey = new List<TKey>();
            foreach (TKey key in useKeyDict.Keys)
            {
                TValue data;
#if MAGICACLOTH_USE_COLLECTIONS_130 && !MAGICACLOTH_USE_COLLECTIONS_200
                NativeParallelMultiHashMapIterator<TKey> iterator;
#else
                NativeMultiHashMapIterator<TKey> iterator;
#endif
                if (nativeMultiHashMap.TryGetFirstValue(key, out data, out iterator))
                {
                    do
                    {
                        // ????
                        if (func(key, data))
                        {
                            // ??
                            nativeMultiHashMap.Remove(iterator);

                            var cnt = useKeyDict[key] - 1;
                            if (cnt == 0)
                                removeKey.Add(key);
                        }
                    }
                    while (nativeMultiHashMap.TryGetNextValue(out data, ref iterator));
                }
            }

            foreach (var key in removeKey)
                useKeyDict.Remove(key);

            nativeLength = NativeCount;
        }

        /// <summary>
        /// ???????
        /// </summary>
        /// <param name="func">true??????</param>
        /// <param name="rdata">??????????????????????????</param>
        public void Replace(Func<TKey, TValue, bool> func, Func<TValue, TValue> datafunc)
        {
            foreach (var key in useKeyDict.Keys)
            {
                TValue data;
#if MAGICACLOTH_USE_COLLECTIONS_130 && !MAGICACLOTH_USE_COLLECTIONS_200
                NativeParallelMultiHashMapIterator<TKey> iterator;
#else
                NativeMultiHashMapIterator<TKey> iterator;
#endif
                if (nativeMultiHashMap.TryGetFirstValue(key, out data, out iterator))
                {
                    do
                    {
                        // ????
                        if (func(key, data))
                        {
                            // ????
                            nativeMultiHashMap.SetValue(datafunc(data), iterator);
                            return;
                        }
                    }
                    while (nativeMultiHashMap.TryGetNextValue(out data, ref iterator));
                }
            }
            nativeLength = NativeCount;
        }

        /// <summary>
        /// ???????????????
        /// </summary>
        /// <param name="act"></param>
        public void Process(Action<TKey, TValue> act)
        {
            foreach (var key in useKeyDict.Keys)
            {
                TValue data;
#if MAGICACLOTH_USE_COLLECTIONS_130 && !MAGICACLOTH_USE_COLLECTIONS_200
                NativeParallelMultiHashMapIterator<TKey> iterator;
#else
                NativeMultiHashMapIterator<TKey> iterator;
#endif
                if (nativeMultiHashMap.TryGetFirstValue(key, out data, out iterator))
                {
                    do
                    {
                        act(key, data);
                    }
                    while (nativeMultiHashMap.TryGetNextValue(out data, ref iterator));
                }
            }
        }

        /// <summary>
        /// ??????????????????
        /// </summary>
        /// <param name="key"></param>
        /// <param name="act"></param>
        public void Process(TKey key, Action<TValue> act)
        {
            TValue data;
#if MAGICACLOTH_USE_COLLECTIONS_130 && !MAGICACLOTH_USE_COLLECTIONS_200
            NativeParallelMultiHashMapIterator<TKey> iterator;
#else
            NativeMultiHashMapIterator<TKey> iterator;
#endif
            if (nativeMultiHashMap.TryGetFirstValue(key, out data, out iterator))
            {
                do
                {
                    act(data);
                }
                while (nativeMultiHashMap.TryGetNextValue(out data, ref iterator));
            }
        }

        /// <summary>
        /// ?????????????
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(TKey key, TValue value)
        {
            TValue data;
#if MAGICACLOTH_USE_COLLECTIONS_130 && !MAGICACLOTH_USE_COLLECTIONS_200
            NativeParallelMultiHashMapIterator<TKey> iterator;
#else
            NativeMultiHashMapIterator<TKey> iterator;
#endif
            if (nativeMultiHashMap.TryGetFirstValue(key, out data, out iterator))
            {
                do
                {
                    if (data.Equals(value))
                    {
                        return true;
                    }
                }
                while (nativeMultiHashMap.TryGetNextValue(out data, ref iterator));
            }

            return false;
        }

        /// <summary>
        /// ????????????
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(TKey key)
        {
            return useKeyDict.ContainsKey(key);
        }

        /// <summary>
        /// ?????
        /// </summary>
        /// <param name="key"></param>
        public void Remove(TKey key)
        {
            nativeMultiHashMap.Remove(key);
            useKeyDict.Remove(key);
            nativeLength = NativeCount;
        }


        /// <summary>
        /// ????????????????
        /// </summary>
        public int Count
        {
            get
            {
                //return nativeMultiHashMap.Length;
                return nativeLength;
            }
        }

        /// <summary>
        /// ?????
        /// </summary>
        public void Clear()
        {
            nativeMultiHashMap.Clear();
            nativeLength = 0;
            useKeyDict.Clear();
        }

        /// <summary>
        /// ???NativeMultiHashMap?????
        /// </summary>
        /// <returns></returns>
#if MAGICACLOTH_USE_COLLECTIONS_130 && !MAGICACLOTH_USE_COLLECTIONS_200
        public NativeParallelMultiHashMap<TKey, TValue> Map
#else
        public NativeMultiHashMap<TKey, TValue> Map
#endif
        {
            get
            {
                return nativeMultiHashMap;
            }
        }
    }
}
