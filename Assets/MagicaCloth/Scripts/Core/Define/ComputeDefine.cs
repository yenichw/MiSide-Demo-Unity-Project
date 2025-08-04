// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp

namespace MagicaCloth
{
    public static partial class Define
    {
        /// <summary>
        /// ????????
        /// </summary>
        public static class Compute
        {
            /// <summary>
            /// ????????????????
            /// </summary>
            public const float Epsilon = 1e-6f;

            /// <summary>
            /// ??????????????????
            /// </summary>
            public const float CollisionFrictionRange = 0.03f; // 0.05(v1.6.1) 0.01(v1.7.0) 0.03(v1.7.5)

            /// <summary>
            /// ??????
            /// </summary>
            public const float FrictionDampingRate = 0.6f; // 0.6(v1.6.1) 0.6(v1.7.0)

            /// <summary>
            /// ??????????
            /// 0.9f = ??????10%?????
            /// </summary>
            public const float FrictionMoveRatio = 0.5f; // 0.5(v1.6.1) 0.9(v1.7.0) 1.0(v1.8.0)

            /// <summary>
            /// ?????????????
            /// </summary>
            public const float FrictionPower = 4.0f; // 4.0(v1.7.0)

            /// <summary>
            /// ClampPosition??????????????(m/s)
            /// </summary>
            public const float ClampPositionMaxVelocity = 1.0f;

            /// <summary>
            /// ???????????1???????????
            /// </summary>
            public const float GlobalColliderMaxMoveDistance = 0.2f;

            /// <summary>
            /// ???????????1???????????(deg)
            /// </summary>
            public const float GlobalColliderMaxRotationAngle = 10.0f;

            /// <summary>
            /// ?????????????????
            /// ?????????????0.8??????
            /// </summary>
            public const float ColliderExtrusionMaxPower = 0.4f; // 1.0(v1.8.4)

            /// <summary>
            /// ???????????????????????
            /// </summary>
            public const float ColliderExtrusionDirectionPower = 0.3f; // 0.5(v1.8.0)

            /// <summary>
            /// ????????????????????????????
            /// </summary>
            public const float ColliderExtrusionDistPower = 2.0f;

            /// <summary>
            /// ???????????????
            /// </summary>
            public const float ColliderExtrusionVelocityInfluence = 0.25f; // 0.5(v1.8.0) 0.25(v1.8.1)

            /// <summary>
            /// ????
            /// </summary>
            public const float MaxWindMain = 100;

            //=================================================================
            // Algorithm 1
            //=================================================================
            /// <summary>
            /// ClampRotation??????????????(m/s)
            /// </summary>
            public const float ClampRotationMaxVelocity = 1.0f;


            //=================================================================
            // Algorithm 2
            //=================================================================
            /// <summary>
            /// ClampRotation2????????????(m/s)
            /// </summary>
            public const float ClampRotationMaxVelocity2 = 2.0f;

            /// <summary>
            /// ClampRotation2????????????(0.0-1.0)
            /// (0.4???)(0.3?????)
            /// </summary>
            //public const float ClampRotationPivotRatio = 0.3f;

            /// <summary>
            /// TriangleBend?????(0.0-1.0)
            /// </summary>
            public const float TriangleBendVelocityInfluence = 0.5f;
        }
    }
}
