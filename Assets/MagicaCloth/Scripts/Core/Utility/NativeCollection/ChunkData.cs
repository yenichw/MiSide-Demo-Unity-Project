// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
namespace MagicaCloth
{
    /// <summary>
    /// FixedChunkNativeList / FixedMultiNativeList???????????
    /// </summary>
    public struct ChunkData
    {
        public int chunkNo;

        /// <summary>
        /// ?????????????????????
        /// </summary>
        public int startIndex;

        /// <summary>
        /// ????
        /// </summary>
        public int dataLength;

        /// <summary>
        /// ???????????????????????
        /// (FixedMultiNativeList???)
        /// </summary>
        public int useLength;

        public void Clear()
        {
            chunkNo = 0;
            startIndex = 0;
            dataLength = 0;
            useLength = 0;
        }

        public bool IsValid()
        {
            return dataLength > 0;
        }

        public override string ToString()
        {
            string str = string.Empty;
            str += "[chunkNo=" + chunkNo + ",startIndex=" + startIndex + ",dataLength=" + dataLength + ",useLength=" + useLength + "\n";
            return str;
        }
    }
}
