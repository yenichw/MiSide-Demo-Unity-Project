using System;
using System.Collections.Generic;

namespace EPOOutline
{
    public static class KeywordsUtility
    {
        private static Dictionary<BlurType, string> BlurTypes = new Dictionary<BlurType, string>
                {
                    { BlurType.Box,             "BOX_BLUR" },
                    { BlurType.Gaussian5x5,     "GAUSSIAN5X5" },
                    { BlurType.Gaussian9x9,     "GAUSSIAN9X9" },
                    { BlurType.Gaussian13x13,   "GAUSSIAN13X13" }
                };

        private static Dictionary<DilateQuality, string> DilateQualityKeywords = new Dictionary<DilateQuality, string>
                {
                    { DilateQuality.Base,       "BASE_QUALITY_DILATE" },
                    { DilateQuality.High,       "HIGH_QUALITY_DILATE" },
                    { DilateQuality.Ultra,      "ULTRA_QUALITY_DILATE" }
                };
        
        public static string GetBackKeyword(ComplexMaskingMode mode)
        {
            switch (mode)
            {
                case ComplexMaskingMode.None:
                    return string.Empty;
                case ComplexMaskingMode.ObstaclesMode:
                    return "BACK_OBSTACLE_RENDERING";
                case ComplexMaskingMode.MaskingMode:
                    return "BACK_MASKING_RENDERING";
                default:
                    throw new ArgumentException("Unknown rendering mode");
            }
        }

        public static string GetTextureArrayCutoutKeyword()
        {
            return "TEXARRAY_CUTOUT";
        }

        public static string GetDilateQualityKeyword(DilateQuality quality)
        {
            switch (quality)
            {
                case DilateQuality.Base:
                    return "BASE_QUALITY_DILATE";
                case DilateQuality.High:
                    return "HIGH_QUALITY_DILATE";
                case DilateQuality.Ultra:
                    return "ULTRA_QUALITY_DILATE";
                default:
                    throw new System.Exception("Unknown dilate quality level");
            }
        }

        public static string GetEnabledInfoBufferKeyword()
        {
            return "USE_INFO_BUFFER";
        }

        public static string GetInfoBufferStageKeyword()
        {
            return "INFO_BUFFER_STAGE";
        }

        public static string GetBlurKeyword(BlurType type)
        {
            return BlurTypes[type];
        }

        public static string GetCutoutKeyword()
        {
            return "USE_CUTOUT";
        }

        public static void GetAllBlurKeywords(List<string> list)
        {
            list.Clear();
            foreach (var item in BlurTypes)
                list.Add(item.Value);
        }

        public static void GetAllDilateKeywords(List<string> list)
        {
            list.Clear();
            foreach (var item in DilateQualityKeywords)
                list.Add(item.Value);
        }
    }
}