// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// ?????(????????????????)
    /// </summary>
    [HelpURL("https://magicasoft.jp/directional-wind/")]
    [AddComponentMenu("MagicaCloth/MagicaDirectionalWind")]
    public partial class MagicaDirectionalWind : WindComponent
    {
        public override ComponentType GetComponentType()
        {
            return ComponentType.DirectionalWind;
        }

        /// <summary>
        /// ???????
        /// </summary>
        /// <returns></returns>
        public override PhysicsManagerWindData.WindType GetWindType()
        {
            return PhysicsManagerWindData.WindType.Direction;
        }

        /// <summary>
        /// ????????
        /// </summary>
        /// <returns></returns>
        public override PhysicsManagerWindData.ShapeType GetShapeType()
        {
            return PhysicsManagerWindData.ShapeType.Box;
        }

        /// <summary>
        /// ?????????
        /// </summary>
        /// <returns></returns>
        public override PhysicsManagerWindData.DirectionType GetDirectionType()
        {
            return PhysicsManagerWindData.DirectionType.OneDirection;
        }

        /// <summary>
        /// ??????????
        /// </summary>
        /// <returns></returns>
        public override bool IsAddition()
        {
            return false;
        }

        /// <summary>
        /// ?????????
        /// </summary>
        /// <returns></returns>
        public override Vector3 GetAreaSize()
        {
            return new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        }

        /// <summary>
        /// ?????????
        /// </summary>
        /// <returns></returns>
        //public override Vector3 GetAnchor()
        //{
        //    return Vector3.zero;
        //}

        /// <summary>
        /// ??????????
        /// </summary>
        /// <returns></returns>
        public override float GetAreaVolume()
        {
            return 100000000;
        }

        /// <summary>
        /// ????????????
        /// </summary>
        /// <returns></returns>
        public override float GetAreaLength()
        {
            return float.MaxValue;
        }

        //=========================================================================================
        /// <summary>
        /// ????????
        /// </summary>
        protected override void ResetParams()
        {
            main = 5;
            turbulence = 1;
            frequency = 1;
            areaSize = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            //anchor = Vector3.zero;
            directionAngleX = 0;
            directionAngleY = 0;
            directionType = PhysicsManagerWindData.DirectionType.OneDirection;
            attenuation.SetParam(1, 1, false, 0, false);
        }
    }
}
