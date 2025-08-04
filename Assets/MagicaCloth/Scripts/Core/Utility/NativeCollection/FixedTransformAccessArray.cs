// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;

namespace MagicaCloth
{
    /// <summary>
    /// ????????TransformAccessArray
    /// ?????????????????(????)
    /// ??????????????????????????(TransformAccessArray??????????)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FixedTransformAccessArray : IDisposable
    {
        /// <summary>
        /// ????????
        /// </summary>
        TransformAccessArray transformArray;

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
        /// ??????????
        /// </summary>
        Dictionary<int, int> useIndexDict = new Dictionary<int, int>();

        /// <summary>
        /// ????????????????
        /// </summary>
        Dictionary<int, int> indexDict = new Dictionary<int, int>();

        /// <summary>
        /// ????????????????
        /// </summary>
        Dictionary<int, int> referenceDict = new Dictionary<int, int>();

        //=========================================================================================
        public FixedTransformAccessArray(int desiredJobCount = -1)
        {
            transformArray = new TransformAccessArray(0, desiredJobCount);
            nativeLength = transformArray.length;
        }

        public FixedTransformAccessArray(int capacity, int desiredJobCount)
        {
            transformArray = new TransformAccessArray(capacity, desiredJobCount);
            nativeLength = transformArray.length;
        }

        /// ?????
        /// ?????????????
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public int Add(Transform element)
        {
            int index = 0;

            int id = element.GetInstanceID();

            if (referenceDict.ContainsKey(id))
            {
                // ??????+
                referenceDict[id] = referenceDict[id] + 1;
                return indexDict[id];
            }

            if (emptyStack.Count > 0)
            {
                // ???
                index = emptyStack.Dequeue();
                transformArray[index] = element;
            }
            else
            {
                // ??
                index = transformArray.length;
                transformArray.Add(element);
            }
            useIndexDict.Add(index, id);
            indexDict.Add(id, index);
            referenceDict.Add(id, 1);
            nativeLength = transformArray.length;

            return index;
        }

        /// <summary>
        /// ?????
        /// ??????????????????
        /// </summary>
        /// <param name="index"></param>
        public void Remove(int index)
        {
            if (useIndexDict.ContainsKey(index))
            {
                int id = useIndexDict[index];
                int cnt = referenceDict[id] - 1;
                if (cnt > 0)
                {
                    // ??????-
                    referenceDict[id] = cnt;
                    return;
                }

                // ??
                transformArray[index] = null;
                emptyStack.Enqueue(index);
                useIndexDict.Remove(index);
                indexDict.Remove(id);
                referenceDict.Remove(id);
                nativeLength = transformArray.length;
            }
        }


        public bool Exist(int index)
        {
            return useIndexDict.ContainsKey(index);
        }

        public bool Exist(Transform element)
        {
            if (element == null)
                return false;
            return indexDict.ContainsKey(element.GetInstanceID());
        }

        /// <summary>
        /// ??????
        /// </summary>
        public int Count
        {
            get
            {
                return useIndexDict.Count;
            }
        }

        /// <summary>
        /// ??????
        /// </summary>
        public int Length
        {
            get
            {
                return nativeLength;
            }
        }

        public Transform this[int index]
        {
            get
            {
                return transformArray[index];
            }
        }

        public int GetIndex(Transform element)
        {
            if (element == null)
                return -1;
            int id = element.GetInstanceID();
            if (indexDict.ContainsKey(id))
                return indexDict[id];
            else
                return -1;
        }

        public void Clear()
        {
            // ??????????????
            foreach (var index in useIndexDict.Keys)
                emptyStack.Enqueue(index);
            useIndexDict.Clear();
            for (int i = 0, cnt = Length; i < cnt; i++)
                transformArray[i] = null;
            indexDict.Clear();
            referenceDict.Clear();
            nativeLength = 0;
        }

        public void Dispose()
        {
            if (transformArray.isCreated)
                transformArray.Dispose();
            emptyStack.Clear();
            useIndexDict.Clear();
            indexDict.Clear();
            referenceDict.Clear();
            nativeLength = 0;
        }

        /// <summary>
        /// TransformAccessArray?????
        /// </summary>
        /// <returns></returns>
        public TransformAccessArray GetTransformAccessArray()
        {
            return transformArray;
        }
    }
}
