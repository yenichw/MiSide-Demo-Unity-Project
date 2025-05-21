// Colorful FX - Unity Asset
// Copyright (c) 2015 - Thomas Hourdel
// http://www.thomashourdel.com

Shader "Hidden/Colorful/Negative"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Amount ("Amount (Float)", Range(0.0, 1.0)) = 1.0
    }

    CGINCLUDE

        #include "UnityCG.cginc"
        #include "Colorful.cginc"

        sampler2D _MainTex;
        float4 _MainTex_ST;
        half _Amount;

        half4 frag(v2f_img i) : SV_Target
        {
            half4 oc = tex2D(_MainTex, StereoScreenSpaceUVAdjust(i.uv, _MainTex_ST));
            half4 nc = 1.0 - oc;
            return lerp(oc, nc, _Amount);
        }

        half4 frag_linear(v2f_img i) : SV_Target
        {
            half4 oc = tex2D(_MainTex, StereoScreenSpaceUVAdjust(i.uv, _MainTex_ST));
            oc = pow(oc, 0.454545);
            half4 nc = 1.0 - oc;
            nc = pow(nc, 2.2);
            return lerp(oc, nc, _Amount);
        }

    ENDCG

    SubShader
    {
        ZTest Always Cull Off ZWrite Off
        Fog { Mode off }

        // (0) Gamma
        Pass
        {
            CGPROGRAM

                #pragma vertex vert_img
                #pragma fragment frag
                #pragma fragmentoption ARB_precision_hint_fastest 

            ENDCG
        }

        // (1) Linear
        Pass
        {
            CGPROGRAM

                #pragma vertex vert_img
                #pragma fragment frag_linear
                #pragma fragmentoption ARB_precision_hint_fastest 

            ENDCG
        }
    }

    FallBack off
}
