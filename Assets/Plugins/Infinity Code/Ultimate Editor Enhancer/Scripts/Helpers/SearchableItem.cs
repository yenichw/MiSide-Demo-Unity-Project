/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public abstract class SearchableItem
    {
        protected static bool Contains(string str1, string str2)
        {
            if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2) || str2.Length > str1.Length) return false;

            int i, j;

            TextInfo textInfo = Culture.textInfo;

            int l2 = str2.Length;

            for (i = 0; i < str1.Length - l2 + 1; i++)
            {
                for (j = 0; j < l2; j++)
                {
                    char c1 = textInfo.ToUpper(str1[i + j]);
                    char c2 = textInfo.ToUpper(str2[j]);
                    if (c1 != c2) break;
                }

                if (j == l2) return true;
            }

            return false;
        }

        public static string GetPattern(string str)
        {
            string search = str;

            TextInfo textInfo = Culture.textInfo;
            StringBuilder builder = StaticStringBuilder.Start();

            bool lastWhite = false;

            for (int i = 0; i < search.Length; i++)
            {
                char c = search[i];
                if (c == ' ' || c == '\t' || c == '\n')
                {
                    if (!lastWhite && builder.Length > 0)
                    {
                        builder.Append(' ');
                        lastWhite = true;
                    }
                }
                else
                {
                    builder.Append(textInfo.ToUpper(c));
                    lastWhite = false;
                }
            }

            if (lastWhite) builder.Length -= 1;

            return builder.ToString();
        }

        public static string GetPattern(string str, out string assetType)
        {
            assetType = string.Empty;
            string search = str;

            TextInfo textInfo = Culture.textInfo;

            Match match = Regex.Match(search, @"(:)(\w*)");
            if (match.Success)
            {
                assetType = textInfo.ToUpper(match.Groups[2].Value);
                if (assetType == "PREFAB") assetType = "GAMEOBJECT";
                search = Regex.Replace(search, @"(:)(\w*)", "");
            }

            StringBuilder builder = StaticStringBuilder.Start();

            bool lastWhite = false;

            for (int i = 0; i < search.Length; i++)
            {
                char c = search[i];
                if (c == ' ' || c == '\t' || c == '\n')
                {
                    if (!lastWhite && builder.Length > 0)
                    {
                        builder.Append(' ');
                        lastWhite = true;
                    }
                }
                else
                {
                    builder.Append(textInfo.ToUpper(c));
                    lastWhite = false;
                }
            }

            if (lastWhite) builder.Length -= 1;

            return builder.ToString();
        }

        protected abstract int GetSearchCount();

        protected abstract string GetSearchString(int index);

        public virtual bool Match(string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return true;

            for (int i = 0; i < GetSearchCount(); i++)
            {
                if (Match(pattern, GetSearchString(i))) return true;
            }

            return false;
        }

        public static bool Match(string pattern, params string[] values)
        {
            if (values == null || values.Length == 0) return false;
            if (string.IsNullOrEmpty(pattern)) return true;

            for (int i = 0; i < values.Length; i++)
            {
                if (MatchInternal(pattern, values[i])) return true;
            }

            return false;
        }

        protected static bool MatchInternal(string pattern, string str)
        {
            int l1 = str.Length;
            int l2 = pattern.Length;

            TextInfo textInfo = Culture.textInfo;

            for (int i = 0; i < l1 - l2 + 1; i++)
            {
                bool success = true;
                int jOffset = 0;
                
                /*
                 * 0 - search any
                 * 1 - lower no skip or upper with skip
                 * 2 - search lower no skip
                 * 3 - search upper with skip
                 */
                int searchRule = 0;
                
                for (int j = 0; j < l2; j++)
                {
                    char pc = pattern[j];

                    int i1 = i + j + jOffset;
                    if (i1 >= l1)
                    {
                        success = false;
                        break;
                    }

                    char sc = str[i1];
                    
                    bool isUpperCase = char.IsUpper(sc) || char.IsDigit(sc);

                    if (isUpperCase)
                    {
                        if (sc == pc)
                        {
                            searchRule = 1;
                            continue;
                        }
                    }
                    else if (textInfo.ToUpper(sc) == pc)
                    {
                        if (searchRule == 0)
                        {
                            if (j == 0) searchRule = 2;
                            continue;
                        }
                        if (searchRule == 1)
                        {
                            continue;
                        }
                        if (searchRule == 2)
                        {
                            continue;
                        }
                    }

                    if (searchRule == 1) searchRule = 3;

                    if (j == 0 || searchRule == 2)
                    {
                        success = false;
                        break;
                    }

                    jOffset++;
                    j--;
                }
                
                if (success) return true;
            }

            return false;
        }
    }
}