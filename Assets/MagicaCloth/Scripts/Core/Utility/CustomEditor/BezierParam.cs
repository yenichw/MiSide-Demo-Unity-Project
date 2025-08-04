// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// ????????????????
    /// </summary>
    [System.Serializable]
    public class BezierParam : IDataHash
    {
        /// <summary>
        /// ???
        /// </summary>
        [SerializeField]
        private float startValue;

        /// <summary>
        /// ???
        /// </summary>
        [SerializeField]
        private float endValue;

        /// <summary>
        /// ??????
        /// false????????startValue?????
        /// </summary>
        [SerializeField]
        private bool useEndValue;

        /// <summary>
        /// ?????(-1.0~+1.0)
        /// </summary>
        [SerializeField]
        private float curveValue;

        /// <summary>
        /// ?????
        /// false?????????????
        /// </summary>
        [SerializeField]
        private bool useCurveValue;

        public BezierParam() { }

        public BezierParam(float val)
        {
            this.startValue = val;
            this.endValue = val;
            this.useEndValue = false;
            this.curveValue = 0.0f;
            this.useCurveValue = false;
        }

        public BezierParam(float sval, float eval)
        {
            this.startValue = sval;
            this.endValue = eval;
            this.useEndValue = true;
            this.curveValue = 0.0f;
            this.useCurveValue = false;
        }

        public BezierParam(float sval, float eval, bool useEval, float cval, bool useCval)
        {
            this.startValue = sval;
            this.endValue = eval;
            this.useEndValue = useEval;
            this.curveValue = cval;
            this.useCurveValue = useCval;
        }

        public void SetParam(float sval, float eval, bool useEval = true, float cval = 0.0f, bool useCval = false)
        {
            this.startValue = sval;
            this.endValue = eval;
            this.useEndValue = useEval;
            this.curveValue = cval;
            this.useCurveValue = useCval;
        }

        public float StartValue
        {
            get
            {
                return startValue;
            }
        }

        public float EndValue
        {
            get
            {
                return useEndValue ? endValue : startValue;
            }
        }

        public float CurveValue
        {
            get
            {
                return useCurveValue && useEndValue ? curveValue : 0.0f;
            }
        }

        public bool UseCurve
        {
            get
            {
                return useEndValue && useCurveValue;
            }
        }

        /// <summary>
        /// ???????????(x=0.0~1.0)?????????
        /// </summary>
        /// <param name="x">0.0~1.0</param>
        /// <returns></returns>
        public float Evaluate(float x)
        {
            return MathUtility.GetBezierValue(StartValue, EndValue, CurveValue, Mathf.Clamp01(x));
        }

        /// <summary>
        /// ????????????????????
        /// </summary>
        /// <param name="startVal"></param>
        /// <param name="endVal"></param>
        /// <param name="curveVal"></param>
        public BezierParam AutoSetup(float startVal, float endVal, float curveVal = 0)
        {
            if (startVal == endVal)
                SetParam(startVal, endVal, false);
            else if (curveVal == 0)
                SetParam(startVal, endVal, true);
            else
                SetParam(startVal, endVal, true, Mathf.Clamp(curveVal, -1, 1), true);

            return this;
        }

        /// <summary>
        /// ???????
        /// </summary>
        /// <returns></returns>
        public int GetDataHash()
        {
            int hash = 0;
            hash += startValue.GetDataHash();
            hash += endValue.GetDataHash();
            hash += useEndValue.GetDataHash();
            hash += curveValue.GetDataHash();
            hash += useCurveValue.GetDataHash();
            return hash;
        }

        /// <summary>
        /// ???????
        /// </summary>
        /// <returns></returns>
        public BezierParam Clone()
        {
            var bz = new BezierParam()
            {
                startValue = this.startValue,
                endValue = this.endValue,
                useEndValue = this.useEndValue,
                curveValue = this.curveValue,
                useCurveValue = this.useCurveValue,
            };
            return bz;
        }
    }
}
