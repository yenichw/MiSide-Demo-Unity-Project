// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace MagicaCloth
{
    public static class MathUtility
    {
        /// <summary>
        /// ???(-1.0f~1.0f)???????
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp1(float a)
        {
            return math.clamp(a, -1.0f, 1.0f);
        }

        /// <summary>
        /// ???(0.0f~1.0f)???????
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp01(float a)
        {
            return math.clamp(a, 0.0f, 1.0f);
        }

        /// <summary>
        /// ??????????
        /// </summary>
        /// <param name="v"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Project(float3 v, float3 n)
        {
            return math.dot(v, n) * n;
        }

        /// <summary>
        /// 2?????????????(????)
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns>????</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Angle(float3 v1, float3 v2)
        {
            float len1 = math.length(v1);
            float len2 = math.length(v2);

            float cos_sita = math.dot(v1, v2) / (len1 * len2);

            float sita = math.acos(Clamp1(cos_sita));

            //return degrees(sita);
            return sita;
        }

        /// <summary>
        /// ??????????????
        /// </summary>
        /// <param name="v"></param>
        /// <param name="minlength"></param>
        /// <param name="maxlength"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 ClampVector(float3 v, float minlength, float maxlength)
        {
            float len = math.length(v);
            if (len > 1e-06f)
            {
                if (len > maxlength)
                {
                    v *= (maxlength / len);
                }
                else if (len < minlength)
                {
                    v *= (minlength / len);
                }
            }

            return v;
        }

        /// <summary>
        /// ??????????????
        /// </summary>
        /// <param name="v"></param>
        /// <param name="minlength"></param>
        /// <param name="maxlength"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 ClampVector(float3 v, float maxlength)
        {
            float len = math.length(v);
            if (len > 1e-06f && len > maxlength)
            {
                v *= (maxlength / len);
            }

            return v;
        }

        /// <summary>
        /// frotm??to??????????????????
        /// </summary>
        /// <param name="from">????</param>
        /// <param name="to">????</param>
        /// <param name="maxlength">??????</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 ClampDistance(float3 from, float3 to, float maxlength)
        {
            float len = math.distance(from, to);
            if (len <= maxlength)
                return to;

            float t = maxlength / len;
            return math.lerp(from, to, t);
        }

        /// <summary>
        /// ????(dir)????????????
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="basedir"></param>
        /// <param name="maxAngle">????(????)</param>
        /// <returns></returns>
        public static bool ClampAngle(float3 dir, float3 basedir, float maxAngle, out float3 outdir)
        {
            float3 v1 = math.normalize(dir);
            float3 v2 = math.normalize(basedir);

            float c = Clamp1(math.dot(v1, v2));
            float angle = math.acos(c);

            //if (c > 0.9995f || angle <= maxAngle)
            if (angle <= maxAngle)
            {
                // ?????????
                outdir = dir;
                return false;
            }

            // ????
            float t = (angle - maxAngle) / angle;

            // dir?maxAngle??????????????????
            float3 axis = math.cross(v1, v2);
            if (math.abs(1.0f + c) < 1e-06f)
            {
                angle = (float)math.PI;

                if (v1.x > v1.y && v1.x > v1.z)
                {
                    axis = math.cross(v1, new float3(0, 1, 0));
                }
                else
                {
                    axis = math.cross(v1, new float3(1, 0, 0));
                }
            }
            else if (math.abs(1.0f - c) < 1e-06f)
            {
                //angle = 0.0f;
                //axis = new float3(1, 0, 0);
                outdir = dir;
                return false;
            }
            var q = quaternion.AxisAngle(math.normalize(axis), angle * t);

            outdir = math.mul(q, dir);
            return true;
        }

        /// <summary>
        /// from??to??????????????????
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="t">???(0.0-1.0)</param>
        /// <returns></returns>
        public static quaternion FromToRotation(float3 from, float3 to, float t = 1.0f)
        {
            float3 v1 = math.normalize(from);
            float3 v2 = math.normalize(to);

            float c = Clamp1(math.dot(v1, v2));
            float angle = math.acos(c);
            float3 axis = math.cross(v1, v2);

            if (math.abs(1.0f + c) < 1e-06f)
            {
                angle = (float)math.PI;

                if (v1.x > v1.y && v1.x > v1.z)
                {
                    axis = math.cross(v1, new float3(0, 1, 0));
                }
                else
                {
                    axis = math.cross(v1, new float3(1, 0, 0));
                }
            }
            else if (math.abs(1.0f - c) < 1e-06f)
            {
                //angle = 0.0f;
                //axis = new float3(1, 0, 0);
                return quaternion.identity;
            }

            return quaternion.AxisAngle(math.normalize(axis), angle * t);
        }

        /// <summary>
        /// from??to??????????????????
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion FromToRotation(quaternion from, quaternion to)
        {
            return math.mul(to, math.inverse(from));
        }

        /// <summary>
        /// ?????????????????(????)
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static float Angle(quaternion q)
        //{
        //    //float3 v1 = new float3(0, 0, 1);
        //    float3 v2 = math.forward(q);
        //    //float c = math.dot(v1, v2);
        //    float c = v2.z;
        //    float angle = math.acos(Clamp01(c));
        //    return angle;
        //}

        /// <summary>
        /// 2?????????????????(????)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Angle(quaternion a, quaternion b)
        {
            const float PI2 = math.PI * 2.0f;
            var ang = math.acos(Clamp1(math.dot(a, b))) * 2.0f; // x2.0???
            return ang > math.PI ? PI2 - ang : ang;
        }

        /// <summary>
        /// ????????????????????
        /// </summary>
        /// <param name="from">????</param>
        /// <param name="to">????</param>
        /// <param name="maxAngle">????(????)</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion ClampAngle(quaternion from, quaternion to, float maxAngle)
        {
            var ang = Angle(from, to);
            if (ang <= maxAngle)
                return to;

            float t = maxAngle / ang;

            return math.slerp(from, to, t);
        }

        /// <summary>
        /// ???????XY????(????)??????Z?????0???
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 AxisToEuler(float3 axis)
        {
            float angy = math.atan2(axis.x, axis.z);
            float angx = math.atan2(-axis.y, math.length(axis - new float3(0, axis.y, 0)));
            return new float3(angx, angy, 0);
        }

        /// <summary>
        /// ??????????????????????
        /// ????????????????????????????????????
        /// XY????????????
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion AxisQuaternion(float3 dir)
        {
            return quaternion.Euler(AxisToEuler(dir));
        }

        /// <summary>
        /// ???????ab????c?????ab??????t(0.0-1.0)???????
        /// </summary>
        /// <param name="c"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ClosestPtPointSegmentRatio(float3 c, float3 a, float3 b)
        {
            float3 ab = b - a;
            // ?????????????d(t) = a + t * (b - a) ??????ab?c???
            float t = math.dot(c - a, ab) / math.dot(ab, ab);
            // ???????????t(???d)???????????
            t = math.saturate(t);
            return t;
        }

        /// <summary>
        /// ???????ab????c?????ab??????t????????t?????????
        /// </summary>
        /// <param name="c"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ClosestPtPointSegmentRatioNoClamp(float3 c, float3 a, float3 b)
        {
            float3 ab = b - a;
            // ?????????????d(t) = a + t * (b - a) ??????ab?c???
            float t = math.dot(c - a, ab) / math.dot(ab, ab);
            return t;
        }

        /// <summary>
        /// ???????ab????c?????ab????????d???????
        /// </summary>
        /// <param name="c"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 ClosestPtPointSegment(float3 c, float3 a, float3 b)
        {
            float3 ab = b - a;
            // ?????????????d(t) = a + t * (b - a) ??????ab?c???
            float t = math.dot(c - a, ab) / math.dot(ab, ab);
            // ???????????t(???d)???????????
            t = math.saturate(t);
            // ?????????t???????????????
            return a + t * ab;
        }

        /// <summary>
        /// ???????ab????c?????ab????????d????????d?????????
        /// </summary>
        /// <param name="c"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 ClosestPtPointSegmentNoClamp(float3 c, float3 a, float3 b)
        {
            float3 ab = b - a;
            // ?????????????d(t) = a + t * (b - a) ??????ab?c???
            float t = math.dot(c - a, ab) / math.dot(ab, ab);
            // ?????????t???????????????
            return a + t * ab;
        }

        /// <summary>
        /// ????????
        /// ??????????????????????
        /// </summary>
        /// <param name="planePos"></param>
        /// <param name="planeDir"></param>
        /// <param name="pos"></param>
        /// <param name="outpos"></param>
        /// <returns>?????????true</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IntersectPointPlane(float3 planePos, float3 planeDir, float3 pos, out float3 outpos)
        {
            float3 v = pos - planePos;
            if (math.dot(planeDir, v) < 0.0f)
            {
                // ???????
                float3 gv = Project(v, planeDir);

                // ?????
                outpos = pos - gv;

                return true;
            }
            else
            {
                outpos = pos;

                return false;
            }
        }

        /// <summary>
        /// ????????
        /// ??????????????????????
        /// </summary>
        /// <param name="planePos"></param>
        /// <param name="planeDir"></param>
        /// <param name="pos"></param>
        /// <param name="outPos"></param>
        /// <returns>??????????????(???)???0.0??(????)</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float IntersectPointPlaneDist(float3 planePos, float3 planeDir, float3 pos, out float3 outPos)
        {
            float3 v = pos - planePos;

            // ???????
            float3 gv = Project(v, planeDir);
            var len = math.length(gv);

            if (math.dot(planeDir, v) < 0.0f)
            {
                // ?????
                outPos = pos - gv;

                // ??????????????
                return -len;
                //return 0.0f;
            }
            else
            {
                outPos = pos;

                // ?????????
                return len;
            }
        }

        /// <summary>
        /// ??(ab)??(p, pn)?????
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="p"></param>
        /// <param name="pn"></param>
        /// <param name="opos"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IntersectSegmentPlane(float3 a, float3 b, float3 p, float3 pn, out float3 opos)
        {
            // ???????ab???????????t?????
            var ab = b - a;
            float pd = math.dot(pn, p);
            float t = (pd - math.dot(pn, a)) / math.dot(pn, ab);
            // t?[0..1]??????
            if (t >= 0.0f && t <= 1.0f)
            {
                opos = a + t * ab;
                return true;
            }

            opos = 0;
            return false;
        }

        /// <summary>
        /// ????????
        /// ?????????????
        /// </summary>
        /// <param name="sc"></param>
        /// <param name="sr"></param>
        /// <param name="pos"></param>
        /// <param name="outPos"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IntersectPointSphere(float3 sc, float sr, float3 pos, out float3 outPos)
        {
            var v = pos - sc;
            var len = math.length(v);
            if (len < sr && len > 0.00001f)
            {
                outPos = pos + math.normalize(v) * (sr - len);
                return true;
            }
            else
            {
                outPos = pos;
                return false;
            }
        }

        /// <summary>
        /// ????????
        /// </summary>
        /// <param name="p"></param>
        /// <param name="sc"></param>
        /// <param name="sr"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IntersectPointSphere(float3 p, float3 sc, float sr)
        {
            var v = p - sc;
            var slen = math.lengthsq(v);
            return slen <= sr * sr;
            //return math.distance(p, sc) <= sr;
        }

        /// <summary>
        /// ?????????
        /// ?? r = p + td,|d|=1 ?? s ???????????????
        /// ???????????? q ???
        /// d?????????????????!
        /// </summary>
        /// <param name="p"></param>
        /// <param name="d"></param>
        /// <param name="sc"></param>
        /// <param name="sr"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        public static bool IntersectRaySphere(float3 p, float3 d, float3 sc, float sr, out float3 q, out float t)
        {
            q = 0;
            t = 0;
            float3 m = p - sc;
            float b = math.dot(m, d);
            float c = math.dot(m, m) - sr * sr;
            // r????s??????(c > 0),r?s?????????????????(b > 0)???
            if (c > 0.0f && b > 0.0f)
                return false;
            float discr = b * b - c;
            // ?????????????????????
            if (discr < 0.0f)
                return false;
            // ?????????????????????????????t???
            t = -b - math.sqrt(discr);
            // t?????????????????????????t????????
            if (t < 0)
                t = 0;
            q = p + t * d;
            return true;
        }

        /// <summary>
        /// ?????????
        /// ??(a->b)??(s)???????????????
        /// ???????????? q ???
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="sc"></param>
        /// <param name="sr"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        public static bool IntersectLineSphare(float3 a, float3 b, float3 sc, float sr, out float3 q)
        {
            var v = b - a;
            float vlen = math.length(v);

            // ????????????????????
            if (vlen == 0)
            {
                q = a;
                return IntersectPointSphere(a, sc, sr);
            }
            float3 d = math.normalize(v);

            // ??????????
            float t;
            if (IntersectRaySphere(a, d, sc, sr, out q, out t))
            {
                float len = math.distance(a, q);
                if (len < vlen)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// ??????????
        /// </summary>
        /// <param name="o">???????</param>
        /// <param name="d">?????</param>
        /// <param name="c">???????</param>
        /// <param name="v">?????</param>
        /// <param name="cost">?????????</param>
        /// <param name="t">???t</param>
        /// <param name="p">?????</param>
        /// <returns></returns>
        public static bool IntersectRayCone(float3 o, float3 d, float3 c, float3 v, float cost, out float t, out float3 p)
        {
            p = 0;
            t = 0;

            //float cost = math.cos(ang);
            float cos2 = cost * cost;

            // a
            float dot_d_v = math.dot(d, v);
            float _a = dot_d_v * dot_d_v - cos2;

            // b
            float3 co = c - o;
            float dot_co_v = math.dot(co, v);
            float _b = 2.0f * (dot_d_v * dot_co_v - math.dot(d, co * cos2));

            // c
            float _c = (dot_co_v * dot_co_v) - math.dot(co, co * cos2);

            // delta
            float delta = _b * _b - 4.0f * _a * _c;

            if (delta < 0.0f)
            {
                // ?????
                return false;
            }
            else if (delta == 0.0f)
            {
                // 1???????
                t = -_b / (2.0f * _a);
                // ???????
                t = -t;

                // ???????????
                p = o + d * t;
                float dot1 = math.dot(v, p - c); // ??????

                if (t < 0.0f || dot1 < 0.0f)
                    return false;
            }
            else
            {
                // 2???????
                float sq = math.sqrt(delta);
                float t1 = (-_b - sq) / (2.0f * _a);
                float t2 = (-_b + sq) / (2.0f * _a);
                // ???????
                t1 = -t1;
                t2 = -t2;

                // ???????????
                float3 p1 = o + d * t1;
                float3 p2 = o + d * t2;
                float dot1 = math.dot(v, p1 - c);
                float dot2 = math.dot(v, p2 - c);

                bool valid1 = t1 >= 0.0f && dot1 >= 0.0f; // ??????
                bool valid2 = t2 >= 0.0f && dot2 >= 0.0f; // ??????

                if (valid1 && valid2)
                {
                    // ??????
                    if (t1 < t2)
                    {
                        t = t1;
                        p = p1;
                    }
                    else
                    {
                        t = t2;
                        p = p2;
                    }
                }
                else if (valid1)
                {
                    t = t1;
                    p = p1;
                }
                else if (valid2)
                {
                    t = t2;
                    p = p2;
                }
                else
                    return false;
            }

            return true;
        }

        /// <summary>
        /// ?????????????????(???????)
        /// ???????????????????????????
        /// </summary>
        /// <param name="a">????</param>
        /// <param name="b">????</param>
        /// <param name="d">????(??????)</param>
        /// <param name="dlen">?????</param>
        /// <param name="c">???????</param>
        /// <param name="v">?????(??????)</param>
        /// <param name="cost">?????????</param>
        /// <param name="c1">?????</param>
        /// <param name="c2">?????</param>
        /// <param name="p">????</param>
        /// <returns></returns>
        public static bool IntersectLineConeSurface(float3 a, float3 b, float3 d, float dlen, float3 c, float3 v, float cost, float3 c1, float3 c2, out float3 p)
        {
            p = 0;

            // ?????????????
            float t;
            if (IntersectRayCone(a, d, c, v, cost, out t, out p) == false)
            {
                // ?????
                return false;
            }

            // ????????????????
            if (t > dlen)
            {
                // ?????
                return false;
            }

            // ??????????????????????
            // ClosestPtPointSegmentRatio()??
            float3 cv = c2 - c1;
            float ct = math.dot(p - c1, cv) / math.dot(cv, cv);
            if (ct < 0.0f || ct > 1.0f)
            {
                // ?????
                return false;
            }

            return true;
        }

        /// <summary>
        /// ????????????????(???????)
        /// ??????????????????????????
        /// </summary>
        /// <param name="sa">????</param>
        /// <param name="sb">????</param>
        /// <param name="p">??????</param>
        /// <param name="q">??????</param>
        /// <param name="r">??????</param>
        /// <param name="t">???t</param>
        /// <returns></returns>
        public static bool IntersectLineCylinderSurface(float3 sa, float3 sb, float3 p, float3 q, float r, out float t)
        {
            t = 0;

            float3 d = q - p;
            float3 m = sa - p;
            float3 n = sb - sa;
            float md = math.dot(m, d);
            float nd = math.dot(n, d);
            float dd = math.dot(d, d);

            // ???????????????????????????????
            if (md < 0.0f && md + nd < 0.0f)
            {
                // ?????p????????
                return false;
            }
            if (md > dd && md + nd > dd)
            {
                // ?????q????????
                return false;
            }

            float nn = math.dot(n, n);
            float mn = math.dot(m, n);
            float a = dd * nn - nd * nd;
            float k = math.dot(m, m) - r * r;
            float c = dd * k - md * md;

            if (math.abs(a) < 1e-6f)
            {
                // ???????????????????
                return false;
            }

            float b = dd * mn - nd * md;
            float discr = b * b - a * c;
            if (discr < 0.0f)
            {
                // ?????????????
                return false;
            }
            t = (-b - math.sqrt(discr)) / a;
            if (t < 0.0f || t > 1.0f)
            {
                // ???????????
                return false;
            }
            if (md + t * nd < 0.0f)
            {
                // ???p????????
                if (nd <= 0.0f)
                {
                    // ??????????????????
                    return false;
                }
                t = -md / nd;
                // Dot(S(t) - p, S(t) - p) <= r ^2 ??????????
                return k + 2 * t * (mn + t * nn) <= 0.0f;
            }
            else if (md + t * nd > dd)
            {
                // ???q????????
                if (nd >= 0.0f)
                {
                    // ??????????????????
                    return false;
                }
                t = (dd - md) / nd;
                // Dot(S(t) - q, S(t) - q) <= r ^ 2 ??????????
                return k + dd - 2 * md + t * (2 * (mn - nd) + t * nn) <= 0.0f;
            }

            return true;
        }

        /// <summary>
        /// ??????????????
        /// </summary>
        /// <param name="a">?????</param>
        /// <param name="b">?????</param>
        /// <param name="d">?????</param>
        /// <param name="c1">??????</param>
        /// <param name="c2">??????</param>
        /// <param name="r1">????????</param>
        /// <param name="r2">????????</param>
        /// <param name="p">????</param>
        public static bool IntersectLineCylinderSurface(float3 a, float3 b, float3 c1, float3 c2, float r1, float r2, out float3 p)
        {
            p = 0;

            float sa = math.abs(1.0f - r1 / r2); // ??????????
            if (sa > 0.001f)
            {
                // ???

                // ????????
                float3 d = b - a;
                float dlen = math.length(d);
                d /= dlen;

                // ???????????????????????
                // ????????????
                float3 c;
                float3 v;
                float vlen = math.distance(c1, c2);
                float f = 0;
                float g = 0;
                float cost = 0;
                float len1, len2;
                if (r1 < r2)
                {
                    v = c2 - c1;
                    //v = math.normalize(v);
                    v /= vlen;

                    f = r2 - r1;
                    g = r1 / (f / vlen);
                    c = c1 - v * g; // ??

                    len1 = vlen + g;
                    len2 = r2;

                }
                else
                {
                    v = c1 - c2;
                    //v = math.normalize(v);
                    v /= vlen;

                    f = r1 - r2;
                    g = r2 / (f / vlen);
                    c = c2 - v * g; // ??

                    len1 = vlen + g;
                    len2 = r1;
                }

                // ??????????
                float len3 = math.sqrt(len1 * len1 + len2 * len2);
                cost = len1 / len3;

                // c = ?????
                // v = ?????
                // cost = ????????

                return IntersectLineConeSurface(a, b, d, dlen, c, v, cost, c1, c2, out p);
            }
            else
            {
                // ???
                float t;
                bool ret = IntersectLineCylinderSurface(a, b, c1, c2, r1, out t);
                if (ret)
                {
                    p = math.lerp(a, b, t);
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// ????????????
        /// </summary>
        /// <param name="a">????</param>
        /// <param name="b">????</param>
        /// <param name="c1">??????</param>
        /// <param name="c2">??????</param>
        /// <param name="r1">????????</param>
        /// <param name="r2">????????</param>
        /// <param name="p">????</param>
        /// <returns></returns>
        public static bool IntersectLineCapsule(float3 a, float3 b, float3 c1, float3 c2, float r1, float r2, out float3 p)
        {
            p = a;

            // ????????????????????????
            float t = ClosestPtPointSegmentRatio(a, c1, c2);
            float dist = math.distance(a, math.lerp(c1, c2, t));
            float r = math.lerp(r1, r2, t);
            if (dist <= r)
            {
                return true;
            }

            // ??????????????
            float3 v = c2 - c1;
            if (IntersectLineSphare(a, b, c1, r1, out p))
            {
                // ????
                if (math.dot(v, p - c1) <= 0.0f)
                {
                    return true;
                }
            }
            if (IntersectLineSphare(a, b, c2, r2, out p))
            {
                // ????
                if (math.dot(v, p - c2) >= 0.0f)
                {
                    return true;
                }
            }

            // ??????(???/???)?????
            return IntersectLineCylinderSurface(a, b, c1, c2, r1, r2, out p);
        }

        /// <summary>
        /// ???????????????????????restDist????????????????????
        /// ????(??:p, ??:restDist)??????????????????????
        /// compressionStiffness?stretchStiffness??????????????????
        /// 1.0???1?????????????0.0???????
        /// </summary>
        /// <param name="p">????</param>
        /// <param name="p0">?????????0</param>
        /// <param name="p1">?????????1</param>
        /// <param name="p2">?????????2</param>
        /// <param name="restDist">????????????????</param>
        /// <param name="compressionStiffness">??1.0(0.0 - 1.0)</param>
        /// <param name="stretchStiffness">??1.0(0.0 - 1.0)</param>
        /// <param name="corr">p?????????</param>
        /// <param name="corr0">p0?????????</param>
        /// <param name="corr1">p1?????????</param>
        /// <param name="corr2">p2?????????</param>
        /// <returns></returns>
        public static bool IntersectTrianglePointDistance(
            float3 p, float3 p0, float3 p1, float3 p2,
            float restDist, float compressionStiffness, float stretchStiffness,
            out float3 corr, out float3 corr0, out float3 corr1, out float3 corr2
            )
        {
            corr = 0;
            corr0 = 0;
            corr1 = 0;
            corr2 = 0;

            // find barycentric coordinates of closest point on triangle

            float b0 = 1.0f / 3.0f;        // for singular case
            float b1 = b0;
            float b2 = b0;

            float3 d1 = p1 - p0;
            float3 d2 = p2 - p0;
            float3 pp0 = p - p0;
            float a = math.dot(d1, d1);
            float b = math.dot(d2, d1);
            float c = math.dot(pp0, d1);
            float d = b;
            float e = math.dot(d2, d2);
            float f = math.dot(pp0, d2);
            float det = a * e - b * d;

            //Debug.Log("det->" + det);

            if (det != 0.0f)
            {
                float s2 = (c * e - b * f) / det;
                float t = (a * f - c * d) / det;
                b0 = 1.0f - s2 - t;       // inside triangle
                b1 = s2;
                b2 = t;
                if (b0 < 0.0f)
                {
                    // on edge 1-2
                    float3 dv = p2 - p1;
                    float dv2 = math.dot(dv, dv);
                    t = (dv2 == 0.0f) ? 0.5f : math.dot(dv, p - p1) / dv2;
                    if (t < 0.0f) t = 0.0f;   // on point 1
                    if (t > 1.0f) t = 1.0f;   // on point 2
                    b0 = 0.0f;
                    b1 = (1.0f - t);
                    b2 = t;
                }
                else if (b1 < 0.0f)
                {
                    // on edge 2-0
                    float3 dv = p0 - p2;
                    float dv2 = math.dot(dv, dv);
                    t = (dv2 == 0.0f) ? 0.5f : math.dot(dv, p - p2) / dv2;
                    if (t < 0.0f) t = 0.0f;   // on point 2
                    if (t > 1.0f) t = 1.0f; // on point 0
                    b1 = 0.0f;
                    b2 = (1.0f - t);
                    b0 = t;
                }
                else if (b2 < 0.0f)
                {
                    // on edge 0-1
                    float3 dv = p1 - p0;
                    float dv2 = math.dot(dv, dv);
                    t = (dv2 == 0.0f) ? 0.5f : math.dot(dv, p - p0) / dv2;
                    if (t < 0.0f) t = 0.0f;   // on point 0
                    if (t > 1.0f) t = 1.0f;   // on point 1
                    b2 = 0.0f;
                    b0 = (1.0f - t);
                    b1 = t;
                }
            }
            float3 q = p0 * b0 + p1 * b1 + p2 * b2;
            float3 n = p - q;
            float dist = math.length(n);
            //Debug.Log("dist->" + dist);

            // ????????
            // dist???????????????????????????????????
            if (dist > restDist)
                return false;

            n = math.normalize(n);
            float C = dist - restDist;
            float3 grad = n;
            float3 grad0 = -n * b0;
            float3 grad1 = -n * b1;
            float3 grad2 = -n * b2;

            float s = 1 + b0 * b0 + b1 * b1 + b2 * b2;
            if (s == 0.0f)
                return false;

            s = C / s;
            if (C < 0.0f)

                s *= compressionStiffness;
            else
                s *= stretchStiffness;

            if (s == 0.0f)
                return false;

            corr = -s * grad;
            corr0 = -s * grad0;
            corr1 = -s * grad1;
            corr2 = -s * grad2;

            return true;
        }

        /// <summary>
        /// ???????????????????????restDist????????????????????
        /// ????(??:p, ??:restDist)??????????????????????
        /// compressionStiffness?stretchStiffness??????????????????
        /// 1.0???1?????????????0.0???????
        /// ???????????????????????????
        /// ???????????????? side ?????
        /// </summary>
        /// <param name="p">????</param>
        /// <param name="p0">?????????0</param>
        /// <param name="p1">?????????1</param>
        /// <param name="p2">?????????2</param>
        /// <param name="restDist">????????????????</param>
        /// <param name="compressionStiffness">??1.0(0.0 - 1.0)</param>
        /// <param name="stretchStiffness">??1.0(0.0 - 1.0)</param>
        /// <param name="side">???????????????(1.0 / -1.0)</param>
        /// <param name="corr">p?????????</param>
        /// <param name="corr0">p0?????????</param>
        /// <param name="corr1">p1?????????</param>
        /// <param name="corr2">p2?????????</param>
        /// <returns></returns>
        public static bool IntersectTrianglePointDistanceSide(
            float3 p, float3 p0, float3 p1, float3 p2,
            float restDist, float compressionStiffness, float stretchStiffness, float side,
            out float3 corr, out float3 corr0, out float3 corr1, out float3 corr2
            )
        {
            corr = 0;
            corr0 = 0;
            corr1 = 0;
            corr2 = 0;

            // find barycentric coordinates of closest point on triangle

            float b0 = 1.0f / 3.0f;        // for singular case
            float b1 = b0;
            float b2 = b0;

            float3 d1 = p1 - p0;
            float3 d2 = p2 - p0;
            float3 pp0 = p - p0;
            float a = math.dot(d1, d1);
            float b = math.dot(d2, d1);
            float c = math.dot(pp0, d1);
            float d = b;
            float e = math.dot(d2, d2);
            float f = math.dot(pp0, d2);
            float det = a * e - b * d;

            //Debug.Log("det->" + det);

            if (det != 0.0f)
            {
                float s2 = (c * e - b * f) / det;
                float t = (a * f - c * d) / det;
                b0 = 1.0f - s2 - t;       // inside triangle
                b1 = s2;
                b2 = t;
                if (b0 < 0.0f)
                {
                    // on edge 1-2
                    float3 dv = p2 - p1;
                    float dv2 = math.dot(dv, dv);
                    t = (dv2 == 0.0f) ? 0.5f : math.dot(dv, p - p1) / dv2;
                    if (t < 0.0f) t = 0.0f;   // on point 1
                    if (t > 1.0f) t = 1.0f;   // on point 2
                    b0 = 0.0f;
                    b1 = (1.0f - t);
                    b2 = t;
                }
                else if (b1 < 0.0f)
                {
                    // on edge 2-0
                    float3 dv = p0 - p2;
                    float dv2 = math.dot(dv, dv);
                    t = (dv2 == 0.0f) ? 0.5f : math.dot(dv, p - p2) / dv2;
                    if (t < 0.0f) t = 0.0f;   // on point 2
                    if (t > 1.0f) t = 1.0f; // on point 0
                    b1 = 0.0f;
                    b2 = (1.0f - t);
                    b0 = t;
                }
                else if (b2 < 0.0f)
                {
                    // on edge 0-1
                    float3 dv = p1 - p0;
                    float dv2 = math.dot(dv, dv);
                    t = (dv2 == 0.0f) ? 0.5f : math.dot(dv, p - p0) / dv2;
                    if (t < 0.0f) t = 0.0f;   // on point 0
                    if (t > 1.0f) t = 1.0f;   // on point 1
                    b2 = 0.0f;
                    b0 = (1.0f - t);
                    b1 = t;
                }
            }
            float3 q = p0 * b0 + p1 * b1 + p2 * b2;
            float3 n = p - q;
            float dist = math.length(n);
            //Debug.Log("dist->" + dist);

            // ????????
            // dist???????????????????????????????????
            if (dist > restDist)
                return false;

            // ????????? side ????????????????????
            float3 tnor = math.cross(d1, d2) * side;
            float C = dist - restDist;
            if (math.dot(tnor, n) < 0.0f)
            {
                n = -n;
                //C = -(restDist + dist);
            }

            n = math.normalize(n);
            float3 grad = n;
            float3 grad0 = -n * b0;
            float3 grad1 = -n * b1;
            float3 grad2 = -n * b2;

            float s = 1 + b0 * b0 + b1 * b1 + b2 * b2;
            if (s == 0.0f)
                return false;

            s = C / s;
            if (C < 0.0f)

                s *= compressionStiffness;
            else
                s *= stretchStiffness;

            if (s == 0.0f)
                return false;

            corr = -s * grad;
            corr0 = -s * grad0;
            corr1 = -s * grad1;
            corr2 = -s * grad2;

            return true;
        }

        public static bool IntersectTrianglePointDistanceSide2(
            float3 p, float3 p0, float3 p1, float3 p2,
            float radius, float restDist, float compressionStiffness, float stretchStiffness, float side,
            out float3 corr, out float3 corr0, out float3 corr1, out float3 corr2
            )
        {
            corr = 0;
            corr0 = 0;
            corr1 = 0;
            corr2 = 0;

            // find barycentric coordinates of closest point on triangle

            float b0 = 1.0f / 3.0f;        // for singular case
            float b1 = b0;
            float b2 = b0;

            float3 d1 = p1 - p0;
            float3 d2 = p2 - p0;
            float3 pp0 = p - p0;
            float a = math.dot(d1, d1);
            float b = math.dot(d2, d1);
            float c = math.dot(pp0, d1);
            float d = b;
            float e = math.dot(d2, d2);
            float f = math.dot(pp0, d2);
            float det = a * e - b * d;

            //Debug.Log("det->" + det);

            if (det != 0.0f)
            {
                float s2 = (c * e - b * f) / det;
                float t = (a * f - c * d) / det;
                b0 = 1.0f - s2 - t;       // inside triangle
                b1 = s2;
                b2 = t;
                if (b0 < 0.0f)
                {
                    // on edge 1-2
                    float3 dv = p2 - p1;
                    float dv2 = math.dot(dv, dv);
                    t = (dv2 == 0.0f) ? 0.5f : math.dot(dv, p - p1) / dv2;
                    if (t < 0.0f) t = 0.0f;   // on point 1
                    if (t > 1.0f) t = 1.0f;   // on point 2
                    b0 = 0.0f;
                    b1 = (1.0f - t);
                    b2 = t;
                }
                else if (b1 < 0.0f)
                {
                    // on edge 2-0
                    float3 dv = p0 - p2;
                    float dv2 = math.dot(dv, dv);
                    t = (dv2 == 0.0f) ? 0.5f : math.dot(dv, p - p2) / dv2;
                    if (t < 0.0f) t = 0.0f;   // on point 2
                    if (t > 1.0f) t = 1.0f; // on point 0
                    b1 = 0.0f;
                    b2 = (1.0f - t);
                    b0 = t;
                }
                else if (b2 < 0.0f)
                {
                    // on edge 0-1
                    float3 dv = p1 - p0;
                    float dv2 = math.dot(dv, dv);
                    t = (dv2 == 0.0f) ? 0.5f : math.dot(dv, p - p0) / dv2;
                    if (t < 0.0f) t = 0.0f;   // on point 0
                    if (t > 1.0f) t = 1.0f;   // on point 1
                    b2 = 0.0f;
                    b0 = (1.0f - t);
                    b1 = t;
                }
            }
            float3 q = p0 * b0 + p1 * b1 + p2 * b2;
            float3 n = p - q;
            float dist = math.length(n);
            //Debug.Log("dist->" + dist);

            // ????????
            // dist???????????????????????????????????
            if (dist > restDist)
                return false;

            // ????????? side ????????????????????
            float3 tnor = math.cross(d1, d2) * side;
            //float C = dist - radius;
            float C = dist - restDist;
            //if (math.dot(tnor, n) < 0.0f)
            //{
            //    //n = -n;
            //    //C = -(radius + dist);
            //}
            //else if (dist > radius)
            //{
            //    return false;
            //}

            n = math.normalize(n);
            float3 grad = n;
            float3 grad0 = -n * b0;
            float3 grad1 = -n * b1;
            float3 grad2 = -n * b2;

            float s = 1 + b0 * b0 + b1 * b1 + b2 * b2;
            if (s == 0.0f)
                return false;

            s = C / s;
            if (C < 0.0f)

                s *= compressionStiffness;
            else
                s *= stretchStiffness;

            if (s == 0.0f)
                return false;

            //if (math.dot(tnor, n) < 0.0f)
            //{
            //    grad = -grad;
            //    grad0 = -grad0;
            //    grad1 = -grad1;
            //    grad2 = -grad2;
            //}


            corr = -s * grad;
            corr0 = -s * grad0;
            corr1 = -s * grad1;
            corr2 = -s * grad2;

            return true;
        }

        //public static float IntersectTrianglePoint(float3 p, float3 p0, float3 p1, float3 p2, float radius)
        public static float DistanceTrianglePoint(float3 p, float3 p0, float3 p1, float3 p2)
        {
            // find barycentric coordinates of closest point on triangle

            float b0 = 1.0f / 3.0f;        // for singular case
            float b1 = b0;
            float b2 = b0;

            float3 d1 = p1 - p0;
            float3 d2 = p2 - p0;
            float3 pp0 = p - p0;
            float a = math.dot(d1, d1);
            float b = math.dot(d2, d1);
            float c = math.dot(pp0, d1);
            float d = b;
            float e = math.dot(d2, d2);
            float f = math.dot(pp0, d2);
            float det = a * e - b * d;

            if (det != 0.0f)
            {
                float s2 = (c * e - b * f) / det;
                float t = (a * f - c * d) / det;
                b0 = 1.0f - s2 - t;       // inside triangle
                b1 = s2;
                b2 = t;
                if (b0 < 0.0f)
                {
                    // on edge 1-2
                    float3 dv = p2 - p1;
                    float dv2 = math.dot(dv, dv);
                    t = (dv2 == 0.0f) ? 0.5f : math.dot(dv, p - p1) / dv2;
                    if (t < 0.0f) t = 0.0f;   // on point 1
                    if (t > 1.0f) t = 1.0f;   // on point 2
                    b0 = 0.0f;
                    b1 = (1.0f - t);
                    b2 = t;
                }
                else if (b1 < 0.0f)
                {
                    // on edge 2-0
                    float3 dv = p0 - p2;
                    float dv2 = math.dot(dv, dv);
                    t = (dv2 == 0.0f) ? 0.5f : math.dot(dv, p - p2) / dv2;
                    if (t < 0.0f) t = 0.0f;   // on point 2
                    if (t > 1.0f) t = 1.0f; // on point 0
                    b1 = 0.0f;
                    b2 = (1.0f - t);
                    b0 = t;
                }
                else if (b2 < 0.0f)
                {
                    // on edge 0-1
                    float3 dv = p1 - p0;
                    float dv2 = math.dot(dv, dv);
                    t = (dv2 == 0.0f) ? 0.5f : math.dot(dv, p - p0) / dv2;
                    if (t < 0.0f) t = 0.0f;   // on point 0
                    if (t > 1.0f) t = 1.0f;   // on point 1
                    b2 = 0.0f;
                    b0 = (1.0f - t);
                    b1 = t;
                }
            }
            float3 q = p0 * b0 + p1 * b1 + p2 * b2;
            float3 n = p - q;
            float dist = math.length(n);

            // ????????
            // dist???????????????????????????????????
            //return dist <= radius;
            return dist;
        }

        /// <summary>
        /// ?????????????
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 TriangleCenter(float3 p0, float3 p1, float3 p2)
        {
            return (p0 + p1 + p2) / 3.0f;
        }

        /// <summary>
        /// ?????????????
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 TriangleNormal(float3 p0, float3 p1, float3 p2)
        {
            return math.normalize(math.cross(p1 - p0, p2 - p0));
        }

        /// <summary>
        /// ???????????????
        /// ???(??-p0)?????????????
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion TriangleRotation(float3 p0, float3 p1, float3 p2)
        {
            var n = TriangleNormal(p0, p1, p2);
            var cen = TriangleCenter(p0, p1, p2);
            var tan = math.normalize(p0 - cen);
            return quaternion.LookRotation(tan, n);
        }

        /// <summary>
        /// ????2?????????????????
        /// ??????????????????????
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion TriangleCenterRotation(float3 p0, float3 p1, float3 p2, float3 p3)
        {
            var n0 = TriangleNormal(p0, p2, p3);
            var n1 = TriangleNormal(p1, p3, p2);
            var n = (n0 + n1) * 0.5f;
            var tan = math.normalize(p3 - p2);
            return quaternion.LookRotation(tan, n);
        }

        /// <summary>
        /// ?????????????????
        /// </summary>
        /// <param name="p"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceTriangleCenter(float3 p, float3 p0, float3 p1, float3 p2)
        {
            var cen = (p0 + p1 + p2) / 3.0f;
            return math.distance(p, cen);
        }

        /// <summary>
        /// ?p???????????????????????(-1/0/+1)
        /// </summary>
        /// <param name="p"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DirectionPointTriangle(float3 p, float3 a, float3 b, float3 c)
        {
            var ab = b - a;
            var ac = c - a;
            var ap = p - a;

            float3 n = math.cross(ab, ac);

            float d = math.dot(ap, n);
            return math.sign(d);
        }

        /// <summary>
        /// ???????pq??????abc?????????????????????
        /// ????????????hitpos?t???
        /// ·?????????????????
        /// ·??????0?????????
        /// </summary>
        /// <param name="p"></param>
        /// <param name="q"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="hitpos"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IntersectLineTriangle(float3 p, float3 q, float3 a, float3 b, float3 c, out float3 hitpos, out float t, out float3 n)
        {
            hitpos = 0;
            t = 0.0f;

            var ab = b - a;
            var ac = c - a;
            var qp = p - q;

            //float3 n = math.cross(ab, ac);
            n = math.cross(ab, ac);

            float d = math.dot(qp, n);
            if (d <= 0.0f)
                return false;

            var ap = p - a;
            t = math.dot(ap, n);
            if (t < 0.0f)
                return false;
            if (t > d)
                return false;

            float3 e = math.cross(qp, ap);
            float v = math.dot(ac, e);
            if (v < 0.0f || v > d)
                return false;
            float w = -math.dot(ab, e);
            if (w < 0.0f || (v + w) > d)
                return false;

            float ood = 1.0f / d;
            t *= ood;
            v *= ood;
            w *= ood;
            float u = 1.0f - v - w;
            //uvw = new float3(u, v, w);
            hitpos = a * u + b * v + c * w;

            return true;
        }

        /// <summary>
        /// 2????(p1-q1)(p2-q2)?????(c1, c2)?????
        /// ???????????????????
        /// </summary>
        /// <param name="p1">??1???</param>
        /// <param name="q1">??1???</param>
        /// <param name="p2">??2???</param>
        /// <param name="q2">??2???</param>
        /// <param name="c1">????1</param>
        /// <param name="c2">????2</param>
        /// <returns></returns>
        public static float ClosestPtSegmentSegment(float3 p1, float3 q1, float3 p2, float3 q2, out float s, out float t, out float3 c1, out float3 c2)
        {
            s = 0.0f;
            t = 0.0f;

            float3 d1 = q1 - p1; // ??s1???????
            float3 d2 = q2 - p2; // ??s2???????
            float3 r = p1 - p2;
            float a = math.dot(d1, d1); // ??s1??????????
            float e = math.dot(d2, d2); // ??s2??????????
            float f = math.dot(d2, r);
            // ????????????????????????????
            if (a <= 1e-6f && e <= 1e-6f)
            {
                // ??????????
                s = t = 0.0f;
                c1 = p1;
                c2 = p2;
                return math.dot(c1 - c2, c1 - c2);
            }
            if (a <= 1e-6f)
            {
                // ??????????
                s = 0.0f;
                t = math.saturate(f / e);
            }
            else
            {
                float c = math.dot(d1, r);
                if (e <= 1e-6f)
                {
                    // 2??????????
                    t = 0.0f;
                    s = math.saturate(-c / a);
                }
                else
                {
                    // ????????????????
                    float b = math.dot(d1, d2);
                    float denom = a * e - b * b; // ???
                    // ???????????L1??L2???????????????
                    // ??s1???????????????????s(????0)???
                    if (denom != 0.0f)
                    {
                        s = math.saturate((b * f - c * e) / denom);
                    }
                    else
                    {
                        s = 0.0f;
                    }
                    // L2??s1(s)?????????????????
                    // t = dot((p1 + d1 * s) - p2, d2) / dot(d2, d2) = (b * s + f) / e
                    t = (b * s + f) / e;
                    // t?[0,1]?????????
                    // ???????t??????s?t??????????????????
                    // s = dot((p2 + d2 * t) - p1, d1) / dot(d1, d1) = (t * b - c) / a
                    // ???s?[0,1]?????
                    if (t < 0.0f)
                    {
                        t = 0.0f;
                        s = math.saturate(-c / a);
                    }
                    else if (t > 1.0f)
                    {
                        t = 1.0f;
                        s = math.saturate((b - c) / a);
                    }
                }
            }

            c1 = p1 + d1 * s;
            c2 = p2 + d2 * t;

            return math.dot(c1 - c2, c1 - c2);
        }

        //=========================================================================================
        /// <summary>
        /// ???????????(t=0.0~1.0)?????????
        /// </summary>
        /// <param name="bparam">???????</param>
        /// <param name="t">????(0.0~1.0)</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetBezierValue(BezierParam bparam, float t)
        {
            return GetBezierValue(bparam.StartValue, bparam.EndValue, bparam.CurveValue, t);
        }

        /// <summary>
        /// 2????????????(t=0.0~1.0)?????????
        /// </summary>
        /// <param name="sval">???</param>
        /// <param name="eval">???</param>
        /// <param name="curve">????(-1.0~+1.0), 0.0?????</param>
        /// <param name="posx">????(0.0~1.0)</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetBezierValue(float sval, float eval, float curve, float t)
        {
            if (curve == 0.0f)
            {
                // ????
                return math.lerp(sval, eval, t);
            }
            else
            {
                // 2??????
                // ???
                float cval = math.lerp(eval, sval, curve * 0.5f + 0.5f);

                //float x = (1.0f - t) * (1.0f - t) * p[0].x + 2 * (1.0f - t) * t * p[1].x + t * t * p[2].x;
                //float y = (1.0f - t) * (1.0f - t) * p[0].y + 2 * (1.0f - t) * t * p[1].y + t * t * p[2].y;

                float w = 1.0f - t;
                float y = w * w * sval + 2 * w * t * cval + t * t * eval;

                return y;
            }
        }
    }
}
