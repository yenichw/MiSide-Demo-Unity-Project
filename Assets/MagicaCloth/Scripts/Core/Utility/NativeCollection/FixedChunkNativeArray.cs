// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// ????(????)???????????????NativeList
    /// ?????????????????(????)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FixedChunkNativeArray<T> : IDisposable where T : struct
    {
        /// <summary>
        /// ????????
        /// </summary>
        NativeArray<T> nativeArray0;

        NativeArray<T> nativeArray1;

        /// <summary>
        /// ????????????
        /// ????????????????????????
        /// </summary>
        int nativeLength;

        /// <summary>
        /// ???????????
        /// </summary>
        List<ChunkData> emptyChunkList = new List<ChunkData>();

        /// <summary>
        /// ???????????
        /// </summary>
        List<ChunkData> useChunkList = new List<ChunkData>();

        int chunkSeed;

        //int initLength = 256;
        int initLength = 64;

        T emptyElement;

        int useLength;

        //=========================================================================================
        public FixedChunkNativeArray()
        {
            nativeArray0 = new NativeArray<T>(initLength, Allocator.Persistent);
            nativeLength = nativeArray0.Length;
            useLength = 0;
        }

        //public FixedChunkNativeArray(int length)
        //{
        //    initLength = length;
        //    nativeArray0 = new NativeArray<T>(initLength, Allocator.Persistent);
        //    nativeLength = nativeArray0.Length;
        //    useLength = 0;
        //}

        public void Dispose()
        {
            if (nativeArray0.IsCreated)
            {
                nativeArray0.Dispose();
            }
            if (nativeArray1.IsCreated)
            {
                nativeArray1.Dispose();
            }
            nativeLength = 0;
            useLength = 0;
            emptyChunkList.Clear();
            useChunkList.Clear();
        }

        public void SetEmptyElement(T empty)
        {
            emptyElement = empty;
        }

        //=========================================================================================
        /// <summary>
        /// ??????????
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public ChunkData AddChunk(int length)
        {
            // ???????
            for (int i = 0; i < emptyChunkList.Count; i++)
            {
                var cdata = emptyChunkList[i];
                if (cdata.dataLength >= length)
                {
                    // ????????????
                    int remainder = cdata.dataLength - length;
                    if (remainder > 0)
                    {
                        // ??
                        var rchunk = new ChunkData()
                        {
                            chunkNo = ++chunkSeed,
                            startIndex = cdata.startIndex + length,
                            dataLength = remainder,
                        };
                        emptyChunkList[i] = rchunk;
                    }
                    else
                    {
                        emptyChunkList.RemoveAt(i);
                    }
                    cdata.dataLength = length;

                    // ????????
                    useChunkList.Add(cdata);

                    return cdata;
                }
            }

            // ????
            var data = new ChunkData()
            {
                chunkNo = ++chunkSeed,
                startIndex = useLength,
                dataLength = length,
            };
            useChunkList.Add(data);
            useLength += length;

            if (nativeArray0.Length < useLength)
            {
                // ??
                int len = nativeArray0.Length;
                while (len < useLength)
                    len += Mathf.Min(len, 4096);
                //len += len;
                var nativeArray2 = new NativeArray<T>(len, Allocator.Persistent);
                nativeArray2.CopyFromFast(nativeArray0);
                nativeArray0.Dispose();

                nativeArray0 = nativeArray2;
                nativeLength = nativeArray0.Length;
            }

            return data;
        }

        /// <summary>
        /// ???1?????????????????
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ChunkData Add(T data)
        {
            var c = AddChunk(1);
            nativeArray0[c.startIndex] = data;
            return c;
        }

        /// <summary>
        /// ??????????
        /// </summary>
        /// <param name="chunkNo"></param>
        public void RemoveChunk(int chunkNo)
        {
            for (int i = 0; i < useChunkList.Count; i++)
            {
                if (useChunkList[i].chunkNo == chunkNo)
                {
                    // ???????????
                    var cdata = useChunkList[i];
                    useChunkList.RemoveAt(i);

                    // ??????
                    nativeArray0.SetValue(cdata.startIndex, cdata.dataLength, emptyElement);

                    // ??????????????????????
                    for (int j = 0; j < emptyChunkList.Count;)
                    {
                        var edata = emptyChunkList[j];
                        if ((edata.startIndex + edata.dataLength) == cdata.startIndex)
                        {
                            // ??
                            edata.dataLength += cdata.dataLength;
                            cdata = edata;
                            emptyChunkList.RemoveAt(j);
                            continue;
                        }
                        else if (edata.startIndex == (cdata.startIndex + cdata.dataLength))
                        {
                            // ??
                            cdata.dataLength += edata.dataLength;
                            emptyChunkList.RemoveAt(j);
                            continue;
                        }

                        j++;
                    }

                    // ????????????
                    emptyChunkList.Add(cdata);

                    return;
                }
            }
        }

        /// <summary>
        /// ??????????
        /// </summary>
        /// <param name="chunk"></param>
        public void RemoveChunk(ChunkData chunk)
        {
            if (chunk.IsValid())
                RemoveChunk(chunk.chunkNo);
        }

        /// <summary>
        /// ?????????
        /// </summary>
        /// <param name="chunk"></param>
        /// <param name="data"></param>
        public void Fill(ChunkData chunk, T data)
        {
            //int end = chunk.startIndex + chunk.dataLength;
            //for (int i = chunk.startIndex; i < end; i++)
            //{
            //    nativeArray[i] = data;
            //}
            nativeArray0.SetValue(chunk.startIndex, chunk.dataLength, data);
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
        /// ??????????????????
        /// </summary>
        public int ChunkCount
        {
            get
            {
                return useChunkList.Count;
            }
        }

        /// <summary>
        /// ????????????????
        /// </summary>
        public int Count
        {
            get
            {
                int cnt = 0;
                foreach (var c in useChunkList)
                    cnt += c.dataLength;
                return cnt;
            }
        }

        public T this[int index]
        {
            get
            {
                return nativeArray0[index];
            }
            set
            {
                nativeArray0[index] = value;
            }
        }

        //public void Clear()
        //{
        //    if (nativeArray0.IsCreated)
        //        nativeArray0.Dispose();
        //    nativeArray0 = new NativeArray<T>(initLength, Allocator.Persistent);
        //    nativeLength = initLength;
        //    useLength = 0;
        //    emptyChunkList.Clear();
        //    useChunkList.Clear();
        //}

        //public T[] ToArray()
        //{
        //    return nativeArray.ToArray();
        //}

        /// <summary>
        /// Job?????????????NativeArray?????????
        /// </summary>
        /// <returns></returns>
        public NativeArray<T> ToJobArray()
        {
            return nativeArray0;
        }

        public NativeArray<T> ToJobArray(int bufferIndex)
        {
            return bufferIndex == 0 ? nativeArray0 : nativeArray1;
        }

        //public NativeArray<T> BackJobArray()
        //{
        //    return nativeArray1;
        //}

        public void SwapBuffer()
        {
            var back = nativeArray1;
            nativeArray1 = nativeArray0;

            // ????????
            if (back.IsCreated == false || back.Length != nativeArray0.Length)
            {
                if (back.IsCreated)
                    back.Dispose();
                //back = new NativeArray<T>(nativeArray0.Length, Allocator.Persistent, NativeArrayOptions.ClearMemory);
                back = new NativeArray<T>(nativeArray0.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                back.CopyFromFast(nativeArray0);

                //Debug.Log("??????!");
            }

            nativeArray0 = back;
        }

        //=========================================================================================
        public override string ToString()
        {
            string str = string.Empty;

            str += "nativeList length=" + Length + "\n";
            str += "use chunk count=" + ChunkCount + "\n";
            str += "empty chunk count=" + emptyChunkList.Count + "\n";

            str += "<< use chunks >>\n";
            foreach (var cdata in useChunkList)
            {
                str += cdata;
            }

            str += "<< empty chunks >>\n";
            foreach (var cdata in emptyChunkList)
            {
                str += cdata;
            }

            return str;
        }
    }
}
