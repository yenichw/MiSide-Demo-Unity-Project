// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp

using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
    [System.Serializable]
    public abstract class ShareDataObject : ScriptableObject, IDataVerify, IDataHash
    {
        [SerializeField]
        protected int dataHash;
        [SerializeField]
        protected int dataVersion;

        //=========================================================================================
        /// <summary>
        /// ??????????????????????
        /// </summary>
        /// <returns></returns>
        public abstract int GetDataHash();

        public int SaveDataHash
        {
            set
            {
                dataHash = value;
            }
            get
            {
                return dataHash;
            }
        }

        public int SaveDataVersion
        {
            set
            {
                dataVersion = value;
            }
            get
            {
                return dataVersion;
            }
        }


        //=========================================================================================
        /// <summary>
        /// ?????????????
        /// </summary>
        /// <returns></returns>
        public abstract int GetVersion();

        /// <summary>
        /// ?????????(???????)???
        /// </summary>
        /// <returns></returns>
        public abstract Define.Error VerifyData();

        /// <summary>
        /// ???????????????
        /// </summary>
        /// <returns></returns>
        public virtual void CreateVerifyData()
        {
            dataHash = GetDataHash();
            dataVersion = GetVersion();
        }

        /// <summary>
        /// ?????????????????
        /// </summary>
        /// <returns></returns>
        public virtual string GetInformation()
        {
            return "No information.";
        }

        //=========================================================================================
        /// <summary>
        /// ??????????????(?????????)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="shareData"></param>
        /// <param name="rebuild"></param>
        /// <returns>????/?????????????????????</returns>
        public static T CreateShareData<T>(string dataName) where T : ShareDataObject
        {
            // ????
            T shareData = CreateInstance<T>();

            // ??
            shareData.name = dataName;

            return shareData;
        }

        /// <summary>
        /// ?????Null????????
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns>?????????????true</returns>
        public static bool RemoveNullAndDuplication<T>(List<T> data)
        {
            bool change = false;
            for (int i = 0; i < data.Count;)
            {
                var val = data[i];
                if (val == null)
                {
                    data.RemoveAt(i);
                    change = true;
                    continue;
                }
                int search = data.IndexOf(val);
                if (search < i)
                {
                    data.RemoveAt(i);
                    change = true;
                    continue;
                }
                i++;
            }

            return change;
        }

        /// <summary>
        /// ?????????????????(???????)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T Clone<T>(T source) where T : ShareDataObject
        {
            if (source == null)
                return null;

            var newdata = Instantiate(source);
            newdata.name = source.name;

            return newdata;
        }
    }
}
