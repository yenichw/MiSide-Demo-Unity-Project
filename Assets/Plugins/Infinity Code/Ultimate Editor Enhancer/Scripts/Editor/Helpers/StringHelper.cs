/*           INFINITY CODE          */
/*     https://infinity-code.com    */

namespace InfinityCode.UltimateEditorEnhancer
{
    public static class StringHelper
    {
        public static bool Contains(string str, string value)
        {
            if (value.Length > str.Length) return false;
            for (int i = 0; i < str.Length - value.Length + 1; i++)
            {
                bool found = true;
                for (int j = 0; j < value.Length; j++)
                {
                    if (str[i + j] != value[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found) return true;
            }
            return false;
        }
        
        public static bool StartsWith(string str, string value)
        {
            if (value.Length > str.Length) return false;
            for (int i = 0; i < value.Length; i++) if (str[i] != value[i]) return false;
            return true;
        }
    }
}