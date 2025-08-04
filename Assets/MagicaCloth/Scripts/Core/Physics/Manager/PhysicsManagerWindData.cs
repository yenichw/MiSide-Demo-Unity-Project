// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp

using Unity.Mathematics;
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// ????
    /// </summary>
    public class PhysicsManagerWindData : PhysicsManagerAccess
    {
        /// <summary>
        /// ????
        /// </summary>
        public enum WindType
        {
            None = 0,
            Direction,
            Area,
        }

        /// <summary>
        /// ?????
        /// </summary>
        public enum ShapeType
        {
            Box = 0,
            Sphere = 1,
        }

        /// <summary>
        /// ???
        /// </summary>
        public enum DirectionType
        {
            OneDirection = 0,   // ????
            Radial = 1,         // ???
        }


        /// <summary>
        /// ???????
        /// </summary>
        public const uint Flag_Enable = 0x00000001; // ?????
        public const uint Flag_Addition = 0x00000002; // ?????

        /// <summary>
        /// ????
        /// </summary>
        public struct WindData
        {
            /// <summary>
            /// ?????????
            /// </summary>
            public uint flag;

            /// <summary>
            /// ????
            /// </summary>
            public WindType windType;

            /// <summary>
            /// ??
            /// </summary>
            public ShapeType shapeType;

            /// <summary>
            /// ????????????????
            /// </summary>
            public int transformIndex;

            /// <summary>
            /// ????????(?????????????????)
            /// ??????x???
            /// </summary>
            public float3 areaSize;

            /// <summary>
            /// ??
            /// </summary>
            public float main;

            /// <summary>
            /// ???(0.0-1.0)
            /// </summary>
            public float turbulence;

            /// <summary>
            /// ?????(1.0???)
            /// </summary>
            public float frequency;

            /// <summary>
            /// ??????(-1.0 - +1.0)
            /// </summary>
            //public float3 anchor;

            /// <summary>
            /// ???????(????)
            /// </summary>
            public float3 direction;

            /// <summary>
            /// ???????
            /// </summary>
            public DirectionType directionType;

            /// <summary>
            /// ???????
            /// </summary>
            public float areaVolume;

            /// <summary>
            /// ?????????
            /// </summary>
            public float areaLength;

            /// <summary>
            /// ?????
            /// </summary>
            public CurveParam attenuation;

            /// <summary>
            /// ?????
            /// </summary>
            /// <param name="flag"></param>
            /// <returns></returns>
            public bool IsFlag(uint flag)
            {
                return (this.flag & flag) != 0;
            }

            /// <summary>
            /// ?????
            /// </summary>
            /// <param name="flag"></param>
            /// <param name="sw"></param>
            public void SetFlag(uint flag, bool sw)
            {
                if (sw)
                    this.flag |= flag;
                else
                    this.flag &= ~flag;
            }

            /// <summary>
            /// ????????
            /// </summary>
            /// <param name="sw"></param>
            public void SetEnable(bool sw)
            {
                if (sw)
                    flag |= Flag_Enable;
                else
                    flag &= ~Flag_Enable;
            }

            /// <summary>
            /// ???????????
            /// </summary>
            /// <returns></returns>
            public bool IsActive()
            {
                return (flag & Flag_Enable) != 0;
            }
        }

        //=========================================================================================
        /// <summary>
        /// ???????
        /// </summary>
        public FixedNativeList<WindData> windDataList;

        //=========================================================================================
        /// <summary>
        /// ????
        /// </summary>
        public override void Create()
        {
            windDataList = new FixedNativeList<WindData>();
        }

        /// <summary>
        /// ??
        /// </summary>
        public override void Dispose()
        {
            if (windDataList == null)
                return;

            windDataList.Dispose();
        }

        //=========================================================================================
        public int CreateWind(
            WindType windType, ShapeType shapeType, float3 areaSize, bool addition, float main, float turbulence, float frequency,
            float3 direction, DirectionType directinType, float areaVolume, float areaLength, BezierParam attenuation
            )
        {
            var data = new WindData();

            uint flag = Flag_Enable;
            flag |= addition ? Flag_Addition : 0;
            data.flag = flag;
            data.windType = windType;
            data.shapeType = shapeType;
            data.transformIndex = -1;
            data.areaSize = areaSize;
            data.main = main;
            data.turbulence = turbulence;
            data.frequency = frequency;
            //data.anchor = math.clamp(anchor, -1, 1);
            data.direction = direction; // local
            data.directionType = directinType;
            data.areaVolume = areaVolume;
            data.areaLength = areaLength;
            data.attenuation.Setup(attenuation);

            int windId = windDataList.Add(data);

            return windId;
        }

        public void RemoveWind(int windId)
        {
            if (windId >= 0)
            {
                windDataList.Remove(windId);
            }
        }

        /// <summary>
        /// ???????????
        /// </summary>
        /// <param name="windId"></param>
        /// <param name="sw"></param>
        public void SetEnable(int windId, bool sw, Transform target)
        {
            if (windId >= 0)
            {
                WindData data = windDataList[windId];
                data.SetEnable(sw);

                // ?????????????/??
                if (sw)
                {
                    if (data.transformIndex == -1)
                    {
                        data.transformIndex = Bone.AddBone(target);
                    }
                }
                else
                {
                    if (data.transformIndex >= 0)
                    {
                        Bone.RemoveBone(data.transformIndex);
                        data.transformIndex = -1;
                    }
                }

                windDataList[windId] = data;
            }
        }

        /// <summary>
        /// ???????????
        /// </summary>
        /// <param name="windId"></param>
        /// <returns></returns>
        public bool IsActive(int windId)
        {
            if (windId >= 0)
                return windDataList[windId].IsActive();
            else
                return false;
        }

        /// <summary>
        /// ?????????
        /// </summary>
        /// <param name="windId"></param>
        /// <param name="flag"></param>
        /// <param name="sw"></param>
        public void SetFlag(int windId, uint flag, bool sw)
        {
            if (windId < 0)
                return;
            WindData data = windDataList[windId];
            data.SetFlag(flag, sw);
            windDataList[windId] = data;
        }

        public void SetParameter(
            int windId, float3 areaSize, bool addition, float main, float turbulence, float frequency,
            float3 direction, float areaVolume, float areaLength, BezierParam attenuation
            )
        {
            if (windId < 0)
                return;
            WindData data = windDataList[windId];
            data.SetFlag(Flag_Addition, addition);
            data.areaSize = areaSize;
            data.main = main;
            data.turbulence = turbulence;
            data.frequency = frequency;
            //data.anchor = math.clamp(anchor, -1, 1);
            data.direction = direction; // local
            data.areaVolume = areaVolume;
            data.areaLength = areaLength;
            data.attenuation.Setup(attenuation);
            windDataList[windId] = data;
        }

        public int Count
        {
            get
            {
                if (windDataList == null)
                    return 0;
                return windDataList.Count;
            }
        }

        //=========================================================================================
        /// <summary>
        /// ????????????????
        /// </summary>
        /// <param name="time"></param>
        /// <param name="noiseBasePos"></param>
        /// <param name="mainDir"></param>
        /// <param name="main"></param>
        /// <param name="turbulence"></param>
        /// <param name="frequency"></param>
        /// <param name="randomScale"></param>
        /// <returns></returns>
        internal static float3 CalcWindForce(float time, float2 noiseBasePos, float3 mainDir, float main, float turbulence, float frequency, float randomScale)
        {
            // ?????????
            float ratio = main / 30.0f; // ??30???

            // ??????????(???????)
            float rang = 15.0f + 15.0f * ratio;

            // ??????
            float dirFreq = 1.0f + 2.0f * ratio; // 1.0 - 3.0
            dirFreq *= frequency;

            // ?????
            var noisePos1 = noiseBasePos.xy;
            var noisePos2 = noiseBasePos.yx;
            noisePos1.x += time * dirFreq; // ??(????????????????)2.0f?
            noisePos2.y += time * dirFreq; // ??(????????????????)2.0f?
            var nv1 = noise.snoise(noisePos1); // -1.0f~1.0f
            var nv2 = noise.snoise(noisePos2); // -1.0f~1.0f

            // ????????
            var ang1 = math.radians(nv1 * rang);
            var ang2 = math.radians(nv2 * rang);
            ang1 *= turbulence; // ???
            ang2 *= turbulence; // ???
            var rq = quaternion.Euler(ang1, ang2, 0.0f); // XY
            var dirq = MathUtility.AxisQuaternion(mainDir);
            float3 wdir = math.forward(math.mul(dirq, rq));

            // ?????
            var noisePos3 = noiseBasePos * 6.36913f;
            //noisePos3.x += time * frequency;
            noisePos3.x += time * (1.0f + 1.0f * ratio) * frequency;
            //float nv = noise.snoise(noisePos3); // -1.0f~1.0f
            float nv = noise.cnoise(noisePos3); // -1.0f~1.0f

            // ????????
            float scl = math.max(nv * randomScale, -1.0f); // scale
            main += main * scl;

            // ????
            float3 force = wdir * main;

            return force;
        }

        //=========================================================================================
#if false // ????????????????
        /// <summary>
        /// ????
        /// </summary>
        public void UpdateWind()
        {
            var job = new UpdateWindJob()
            {
                dtime = manager.UpdateTime.DeltaTime,
                elapsedTime = Time.time,

                bonePosList = Bone.bonePosList.ToJobArray(),
                boneRotList = Bone.boneRotList.ToJobArray(),

                windData = windDataList.ToJobArray(),
            };
            Compute.MasterJob = job.Schedule(windDataList.Length, 1, Compute.MasterJob);
        }

        [BurstCompile]
        struct UpdateWindJob : IJobParallelFor
        {
            public float dtime;
            public float elapsedTime;

            [Unity.Collections.ReadOnly]
            public NativeArray<float3> bonePosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> boneRotList;

            public NativeArray<WindData> windData;

            // ??????
            public void Execute(int index)
            {
                var wdata = windData[index];
                if (wdata.IsActive() == false || wdata.transformIndex < 0)
                    return;

                // ?????????
                var bpos = bonePosList[wdata.transformIndex];
                var brot = boneRotList[wdata.transformIndex];

                // ?????????
                float ratio = wdata.main / 30.0f; // ??30???

                // ??(?????????)
                float freq = 1.0f + 2.0f * ratio; // 1.0 - 3.0

                // ??????????
                float rang = 15.0f + 15.0f * ratio; // 15 - 30

                // ?????
                var noisePos1 = new float2(bpos.x, bpos.z) * 0.1f;
                var noisePos2 = new float2(bpos.x, bpos.z) * 0.1f;
                noisePos1.x += elapsedTime * freq; // ??(????????????????)2.0f?
                noisePos2.y += elapsedTime * freq; // ??(????????????????)2.0f?
                var nv1 = noise.snoise(noisePos1); // -1.0f~1.0f
                var nv2 = noise.snoise(noisePos2); // -1.0f~1.0f

                // ????????
                var ang1 = math.radians(nv1 * rang);
                var ang2 = math.radians(nv2 * rang);
                ang1 *= wdata.turbulence; // ???
                ang2 *= wdata.turbulence; // ???
                var rq = quaternion.Euler(ang1, ang2, 0.0f); // XY
                var dir = math.forward(math.mul(brot, rq)); // ???????????
                wdata.direction = dir;

                // ????
                windData[index] = wdata;
            }
        }
#endif
    }
}
