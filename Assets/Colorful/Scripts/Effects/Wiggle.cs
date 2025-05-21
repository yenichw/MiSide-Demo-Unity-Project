// Colorful FX - Unity Asset
// Copyright (c) 2015 - Thomas Hourdel
// http://www.thomashourdel.com

namespace Colorful
{
    using UnityEngine;

    [HelpURL("http://www.thomashourdel.com/colorful/doc/camera-effects/wiggle.html")]
    [ExecuteInEditMode]
    [AddComponentMenu("Colorful FX/Camera Effects/Wiggle")]
    public class Wiggle : BaseEffect
    {
        public enum Algorithm
        {
            Simple,
            Complex
        }

        [Tooltip("Animation type. Complex is slower but looks more natural.")]
        public Algorithm Mode = Algorithm.Complex;

        public float Timer = 0f;

        [Tooltip("Wave animation speed.")]
        public float Speed = 1f;

        [Tooltip("Wave frequency (higher means more waves).")]
        public float Frequency = 12f;

        [Tooltip("Wave amplitude (higher means bigger waves).")]
        public float Amplitude = 0.01f;

        [Tooltip("Automatically animate this effect at runtime.")]
        public bool AutomaticTimer = true;

        protected virtual void Update()
        {
            if (AutomaticTimer)
            {
                // Reset the timer after a while to keep precision
                if (Timer > 100f)
                    Timer -= 100f;

                Timer += Speed * Time.deltaTime;
            }
        }

        protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (Mathf.Approximately(Amplitude, 0f))
            {
                Graphics.Blit(source, destination);
                return;
            }

            Material.SetVector("_Params", new Vector3(Frequency, Amplitude, Timer * (Mode == Algorithm.Complex ? 0.1f : 1f)));
            Graphics.Blit(source, destination, Material, (int)Mode);
        }

        protected override string GetShaderName()
        {
            return "Hidden/Colorful/Wiggle";
        }
    }
}
