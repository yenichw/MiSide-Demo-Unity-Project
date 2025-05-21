using UnityEngine;

namespace EPOOutline
{
    public partial class Outlinable
    {
        /// <summary>
        /// Outline parameters and settings for the <see cref="EPOOutline.Outlinable"/> rendering.
        /// </summary>
        [System.Serializable]
        public class OutlineProperties
        {
#pragma warning disable CS0649
            [SerializeField]
            private bool enabled = true;

            /// <summary>
            /// Is enabled and should be rendered.
            /// </summary>
            public bool Enabled
            {
                get => enabled;
                set => enabled = value;
            }

            [SerializeField]
            private Color color = Color.yellow;

            /// <summary>
            /// Color of the outline.
            /// </summary>
            public Color Color
            {
                get => color;
                set => color = value;
            }

            [SerializeField]
            [Range(0.0f, 1.0f)]
            private float dilateShift = 1.0f;

            /// <summary>
            /// Dilate shift. The larger, the more shifted dilate will be.
            /// </summary>
            public float DilateShift
            {
                get => dilateShift;
                set => dilateShift = value;
            }

            [SerializeField]
            [Range(0.0f, 1.0f)]
            private float blurShift = 1.0f;

            /// <summary>
            /// Blur shift. The larger, the more shifted blur will be.
            /// </summary>
            public float BlurShift
            {
                get => blurShift;
                set => blurShift = value;
            }

            [SerializeField, SerializedPassInfo("Fill style", "Hidden/EPO/Fill/")]
            private SerializedPass fillPass = new SerializedPass();

            /// <summary>
            /// The <see cref="EPOOutline.SerializedPass"/> used for rendering.
            /// </summary>
            public SerializedPass FillPass => fillPass;
#pragma warning restore CS0649
        }
    }
}