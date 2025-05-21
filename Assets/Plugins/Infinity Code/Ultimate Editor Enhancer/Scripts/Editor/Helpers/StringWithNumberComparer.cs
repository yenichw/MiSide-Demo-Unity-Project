/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public class StringWithNumberComparer : IComparer<Transform>
    {
        public int Compare(Transform t1, Transform t2)
        {
            string n1 = t1.gameObject.name;
            string n2 = t2.gameObject.name;

            for (int i = 0, j = 0; i < n1.Length && j < n2.Length; i++, j++)
            {
                char c1 = n1[i];
                char c2 = n2[i];
                
                if (c1 == c2) continue;
                if (char.IsDigit(c1) && char.IsDigit(c2))
                {
                    int x = 0;
                    int y = 0;
                    while (i < n1.Length && char.IsDigit(n1[i])) x = x * 10 + n1[i++] - '0';
                    while (j < n2.Length && char.IsDigit(n2[j])) y = y * 10 + n2[j++] - '0';
                    if (x != y) return x - y;
                }
                return c1 - c2;
            }
            
            return n1.Length - n2.Length;
        }
    }
}