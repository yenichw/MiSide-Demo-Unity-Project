// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp

using System.Collections.Generic;

namespace MagicaCloth
{
    /// <summary>
    /// ?????????????????????????????????????????
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReferenceDataBuilder<T> where T : struct
    {
        private int indexCount;
        private List<T> dataList = new List<T>();
        private List<List<int>> indexToDataIndexList = new List<List<int>>();
        private List<List<int>> dataIndexToIndexList = new List<List<int>>();

        /// <summary>
        /// (1)???
        /// </summary>
        /// <param name="indexCount">??????????</param>
        public void Init(int indexCount)
        {
            this.indexCount = indexCount;
            for (int i = 0; i < indexCount; i++)
            {
                indexToDataIndexList.Add(new List<int>());
            }
        }

        /// <summary>
        /// (2)?????
        /// ?????????????????(???)?????
        /// </summary>
        /// <param name="data">???</param>
        /// <param name="indexes">?????????????????</param>
        public void AddData(T data, params int[] indexes)
        {
            int dataIndex = dataList.Count;
            dataList.Add(data);
            dataIndexToIndexList.Add(new List<int>());

            foreach (var index in indexes)
            {
                indexToDataIndexList[index].Add(dataIndex);

                // ???
                dataIndexToIndexList[dataIndex].Add(index);
            }
        }

        /// <summary>
        /// (3)???????
        /// ???????????????????????????????????????????????
        /// (???????1????????????????)
        /// </summary>
        /// <returns></returns>
        public (List<ReferenceDataIndex>, List<T>) GetDirectReferenceData()
        {
            var referenceDataList = new List<ReferenceDataIndex>();
            var sortDataList = new List<T>();

            // ????????????????????????
            int start = 0;
            for (int i = 0; i < indexToDataIndexList.Count; i++)
            {
                var work = indexToDataIndexList[i];

                var refdata = new ReferenceDataIndex();
                refdata.startIndex = start;
                refdata.count = work.Count;
                referenceDataList.Add(refdata);

                // ???????????????
                foreach (var dataIndex in work)
                {
                    sortDataList.Add(dataList[dataIndex]);
                }

                start += work.Count;
            }
            //int count = start;

            return (referenceDataList, sortDataList);
        }

        /// <summary>
        /// (3)???????
        /// ????????????????????????????????????????????????????????
        /// ???????????????????????????????????
        /// (????????????????????????)
        /// </summary>
        /// <returns></returns>
        public (List<ReferenceDataIndex>, List<int>, List<List<int>>) GetIndirectReferenceData()
        {
            var referenceDataList = new List<ReferenceDataIndex>();

            // ????????????????????????
            int start = 0;
            for (int i = 0; i < indexToDataIndexList.Count; i++)
            {
                var work = indexToDataIndexList[i];

                var refdata = new ReferenceDataIndex();
                refdata.startIndex = start;
                refdata.count = work.Count;
                referenceDataList.Add(refdata);

                start += work.Count;
            }
            //int count = start;

            // ?????????????????????????????
            List<int> dataIndexList = new List<int>();
            foreach (var work in indexToDataIndexList)
            {
                foreach (var dataIndex in work)
                {
                    dataIndexList.Add(dataIndex);
                }
            }

            // ????????????????????????????
            List<List<int>> dataToDataIndexList = new List<List<int>>();
            for (int dataIndex = 0; dataIndex < dataIndexToIndexList.Count; dataIndex++)
            {
                var indexList = dataIndexToIndexList[dataIndex];
                var dataIndexIndexList = new List<int>();

                foreach (var index in indexList)
                {
                    start = referenceDataList[index].startIndex;
                    int dataIndexIndex = indexToDataIndexList[index].IndexOf(dataIndex);

                    dataIndexIndexList.Add(start + dataIndexIndex);
                }

                dataToDataIndexList.Add(dataIndexIndexList);
            }

            return (referenceDataList, dataIndexList, dataToDataIndexList);
        }
    }
}
