// Colorful FX - Unity Asset
// Copyright (c) 2015 - Thomas Hourdel
// http://www.thomashourdel.com

namespace Colorful
{
    using UnityEngine;

    [HelpURL("http://www.thomashourdel.com/colorful/doc/color-correction/gradient-ramp-dynamic.html")]
    [ExecuteInEditMode]
    [AddComponentMenu("Colorful FX/Color Correction/Gradient Ramp (Dynamic)")]
    public class GradientRampDynamic : BaseEffect
    {
        [Tooltip("Gradient used to remap the pixels luminosity.")]
        public Gradient Ramp;

        [Range(0f, 1f), Tooltip("Blending factor.")]
        public float Amount = 1f;

        protected Texture2D m_RampTexture;

        protected override void Start()
        {
            base.Start();

            if (Ramp != null)
                UpdateGradientCache();
        }

        protected virtual void Reset()
        {
            Ramp = new Gradient
            {
                colorKeys = new []
                {
                    new GradientColorKey(Color.black, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                alphaKeys = new []
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                }
            };

            UpdateGradientCache();
        }

        public void UpdateGradientCache()
        {
            if (m_RampTexture == null)
            {
                m_RampTexture = new Texture2D(256, 1, TextureFormat.RGB24, false)
                {
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.HideAndDontSave
                };
            }

            var pixels = new Color[256];

            for (int i = 0; i < 256; i++)
                pixels[i] = Ramp.Evaluate((float)i / 255f);

            m_RampTexture.SetPixels(pixels);
            m_RampTexture.Apply();
        }

        protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (Ramp == null || Amount <= 0f)
            {
                Graphics.Blit(source, destination);
                return;
            }

            Material.SetTexture("_RampTex", m_RampTexture);
            Material.SetFloat("_Amount", Amount);
            Graphics.Blit(source, destination, Material);
        }

        protected override string GetShaderName()
        {
            return "Hidden/Colorful/Gradient Ramp";
        }
    }
}
