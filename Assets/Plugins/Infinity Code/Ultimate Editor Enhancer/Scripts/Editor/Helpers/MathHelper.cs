/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static class MathHelper
    {
        public static bool ColorsEqualWithoutAlpha(Color clr1, Color clr2)
        {
            if (clr1.r != clr2.r) return false;
            if (clr1.g != clr2.g) return false;
            if (clr1.b != clr2.b) return false;
            return true;
        }

        public static Vector3 NormalToCubeSide(Vector3 n)
        {
            if (Mathf.Abs(n.x) > Mathf.Abs(n.y))
            {
                n.y = 0;
                if (Mathf.Abs(n.x) > Mathf.Abs(n.z))
                {
                    n.z = 0;
                    n.x = Mathf.Sign(n.x);
                }
                else
                {
                    n.x = 0;
                    n.z = Mathf.Sign(n.z);
                }
            }
            else
            {
                n.x = 0;
                if (Mathf.Abs(n.y) > Mathf.Abs(n.z))
                {
                    n.z = 0;
                    n.y = Mathf.Sign(n.y);
                }
                else
                {
                    n.y = 0;
                    n.z = Mathf.Sign(n.z);
                }
            }

            return n;
        }
    }
}