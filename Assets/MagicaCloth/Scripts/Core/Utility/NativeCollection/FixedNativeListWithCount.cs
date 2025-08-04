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
    /// ????????????????NativeList
    /// ?????????????????????
    /// ?????????????????(????)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FixedNativeListWithCount<T> : IDisposable where T : unmanaged
    {
        /// <summary>
        /// ????????
        /// </summary>
        NativeList<T> nativeList;

        /// <summary>
        /// ????????????
        /// ????????????????????????
        /// </summary>
        int nativeLength;

        /// <summary>
        /// ???????????
        /// </summary>
        Queue<int> emptyStack = new Queue<int>();

        /// <summary>
        /// ???????????
        /// </summary>
        HashSet<int> useIndexSet = new HashSet<int>();

        Dictionary<T, int> indexDict = new Dictionary<T, int>();

        Dictionary<T, int> countDict = new Dictionary<T, int>();

        T emptyElement;

        //=========================================================================================
        public FixedNativeListWithCount()
        {
            nativeList = new NativeList<T>(Allocator.Persistent);
            nativeLength = nativeList.Length;
            emptyElement = new T();
        }

        public FixedNativeListWithCount(int capacity)
        {
            nativeList = new NativeList<T>(capacity, Allocator.Persistent);
            nativeLength = nativeList.Length;
        }

        public void Dispose()
        {
            if (nativeList.IsCreated)
            {
                nativeList.Dispose();
            }
            nativeLength = 0;
            emptyStack.Clear();
            useIndexSet.Clear();
            indexDict.Clear();
            countDict.Clear();
        }

        public void SetEmptyElement(T empty)
        {
            emptyElement = empty;
        }

        //=========================================================================================
        /// <summary>
        /// ?????
        /// ?????????????
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public int Add(T element)
        {
            int index = 0;

            // ??????
            if (indexDict.ContainsKey(element))
            {
                // ????+
                index = indexDict[element];
                countDict[element] = countDict[element] + 1;
            }
            else
            {
                // ??
                if (emptyStack.Count > 0)
                {
                    // ???
                    index = emptyStack.Dequeue();
                    nativeList[index] = element;
                }
                else
                {
                    // ??
                    index = nativeList.Length;
                    nativeList.Add(element);
                    nativeLength = nativeList.Length;
                }
                useIndexSet.Add(index);
                indexDict[element] = index;
                countDict[element] = 1;
            }

            return index;
        }

        /// <summary>
        /// ?????
        /// ??????????????????
        /// </summary>
        /// <param name="element"></param>
        public void Remove(T element)
        {
            if (indexDict.ContainsKey(element))
            {
                int cnt = countDict[element];
                if (cnt <= 1)
                {
                    // ??
                    int index = indexDict[element];

                    // ????????????????
                    nativeList[index] = emptyElement;

                    emptyStack.Enqueue(index);
                    useIndexSet.Remove(index);
                    indexDict.Remove(element);
                    countDict.Remove(element);
                }
                else
                {
                    // ??????-
                    countDict[element] = cnt - 1;
                }
            }
        }

        public bool Exist(T element)
        {
            return indexDict.ContainsKey(element);
        }

        public int GetUseCount(T element)
        {
            if (countDict.ContainsKey(element))
                return countDict[element];

            return 0;
        }

        /// <summary>
        /// ?????????????????????
        /// </summary>
        public int Length
        {
            get
            {
                return nativeLength;
            }
        }

        /// <summary>
        /// ????????????????
        /// </summary>
        public int Count
        {
            get
            {
                return useIndexSet.Count;
            }
        }

        public T this[int index]
        {
            get
            {
                return nativeList[index];
            }
            set
            {
                nativeList[index] = value;
            }
        }

        public void Clear()
        {
            nativeList.Clear();
            nativeLength = 0;
            emptyStack.Clear();
            useIndexSet.Clear();
            indexDict.Clear();
            countDict.Clear();
        }

        //public T[] ToArray()
        //{
        //    return nativeList.ToArray();
        //}

        /// <summary>
        /// Job?????????????NativeArray?????????
        /// </summary>
        /// <returns></returns>
        public NativeArray<T> ToJobArray()
        {
            return nativeList.AsArray();
        }
    }
}
