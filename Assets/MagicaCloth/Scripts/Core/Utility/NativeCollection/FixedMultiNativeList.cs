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
    /// ?????????1??NativeArray?????????????
    /// NativeMultiHashMap????????????
    /// NativeMultiHashMap??????????????????????
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FixedMultiNativeList<T> : IDisposable where T : struct
    {
        /// <summary>
        /// ????????
        /// </summary>
        NativeArray<T> nativeArray;

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

        int initLength = 64;

        T emptyElement;

        int useLength;

        //=========================================================================================
        public FixedMultiNativeList()
        {
            nativeArray = new NativeArray<T>(initLength, Allocator.Persistent);
            nativeLength = nativeArray.Length;
            useLength = 0;
        }

        public void Dispose()
        {
            if (nativeArray.IsCreated)
            {
                nativeArray.Dispose();
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
                useLength = 0,
            };
            useChunkList.Add(data);
            useLength += length;

            if (nativeArray.Length < useLength)
            {
                // ??
                int len = nativeArray.Length;
                while (len < useLength)
                    len += Mathf.Min(len, 4096);
                var nativeArray2 = new NativeArray<T>(len, Allocator.Persistent);
                nativeArray2.CopyFromFast(nativeArray);
                nativeArray.Dispose();

                nativeArray = nativeArray2;
                nativeLength = nativeArray.Length;
            }

            return data;
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
                    nativeArray.SetValue(cdata.startIndex, cdata.dataLength, emptyElement);

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
            RemoveChunk(chunk.chunkNo);
        }

        /// <summary>
        /// ?????????????
        /// </summary>
        /// <param name="chunk"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ChunkData AddData(ChunkData chunk, T data)
        {
            if (chunk.useLength == chunk.dataLength)
            {
                // ??

                // ????????????
                int len = chunk.dataLength;
                len += Mathf.Min(len, 4096);
                var newChunk = AddChunk(len);

                // ????????????
                nativeArray.CopyBlock(chunk.startIndex, newChunk.startIndex, chunk.dataLength);
                newChunk.useLength = chunk.useLength;

                // ????????????
                RemoveChunk(chunk);
                chunk = newChunk;
            }

            nativeArray[chunk.startIndex + chunk.useLength] = data;
            chunk.useLength++;

            return chunk;
        }

        /// <summary>
        /// ??????????????
        /// </summary>
        /// <param name="chunk"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ChunkData RemoveData(ChunkData chunk, T data)
        {
            // ?????????????????
            int index = chunk.startIndex;
            for (int i = 0; i < chunk.useLength; i++, index++)
            {
                if (data.Equals(nativeArray[index]))
                {
                    // Swap Back???
                    if (i < (chunk.useLength - 1))
                    {
                        nativeArray[index] = nativeArray[chunk.startIndex + chunk.useLength - 1];
                        nativeArray[chunk.startIndex + chunk.useLength - 1] = emptyElement;
                    }
                    chunk.useLength--;
                }
            }

            return chunk;
        }

        /// <summary>
        /// ?????????????????
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public ChunkData ClearData(ChunkData chunk)
        {
            nativeArray.SetValue(chunk.startIndex, chunk.dataLength, emptyElement);
            chunk.useLength = 0;
            return chunk;
        }

        /// <summary>
        /// ????????????????????
        /// </summary>
        /// <param name="chunk"></param>
        /// <param name="act"></param>
        public void Process(ChunkData chunk, Action<T> act)
        {
            int index = chunk.startIndex;
            for (int i = 0; i < chunk.useLength; i++, index++)
            {
                act(nativeArray[index]);
            }
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
                return nativeArray[index];
            }
            //set
            //{
            //    nativeArray[index] = value;
            //}
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
            return nativeArray;
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
