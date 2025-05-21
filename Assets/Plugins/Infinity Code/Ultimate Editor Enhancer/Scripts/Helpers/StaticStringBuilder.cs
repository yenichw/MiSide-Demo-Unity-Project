/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Text;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static class StaticStringBuilder
    {
        private static StringBuilder builder = new StringBuilder();

        public static StringBuilder AppendEscaped(StringBuilder b, string value, string[] escapeChars)
        {
            if (escapeChars == null || escapeChars.Length % 2 != 0) throw new Exception("Length of escapeChars must be N * 2");
            for (int i = 0; i < value.Length; i++)
            {
                bool escaped = false;
                for (int j = 0; j < escapeChars.Length; j += 2)
                {
                    string s = escapeChars[j];
                    if (value[i] == s[0])
                    {
                        b.Append(escapeChars[j + 1]);
                        escaped = true;
                        break;
                    }
                }

                if (!escaped) b.Append(value[i]);
            }

            return b;
        }

        public static StringBuilder Start(bool clear = true)
        {
            if (clear) builder.Length = 0;
            return builder;
        }
    }
}