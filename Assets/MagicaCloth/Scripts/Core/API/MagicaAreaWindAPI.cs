// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp

namespace MagicaCloth
{
    /// <summary>
    /// MagicaAreaWind API
    /// </summary>
    public partial class MagicaAreaWind : WindComponent
    {
        /// <summary>
        /// ???????
        /// Wind area shape.
        /// </summary>
        public PhysicsManagerWindData.ShapeType ShapeType
        {
            get => shapeType;
            set
            {
                shapeType = value;
                status.SetDirty();
            }
        }

        /// <summary>
        /// ?????
        /// Addition mode.
        /// </summary>
        public bool Addition
        {
            get => isAddition;
            set
            {
                isAddition = value;
                status.SetDirty();
            }
        }
    }
}
