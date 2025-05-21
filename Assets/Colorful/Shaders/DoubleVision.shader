// Colorful FX - Unity Asset
// Copyright (c) 2015 - Thomas Hourdel
// http://www.thomashourdel.com

Shader "Hidden/Colorful/Double Vision"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Displace ("Displace", Vector) = (0.7, 0.0, 0.0, 0.0)
        _Amount ("Amount", Range(0.0, 1.0)) = 1.0
    }

    SubShader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Fog { Mode off }
            
            CGPROGRAM

                #pragma vertex vert_img
                #pragma fragment frag
                #pragma fragmentoption ARB_precision_hint_fastest 
                #include "UnityCG.cginc"
                #include "Colorful.cginc"

                sampler2D _MainTex;
                float4 _MainTex_ST;
                float4 _MainTex_TexelSize;
                half2 _Displace;
                half _Amount;

                half4 frag(v2f_img i) : SV_Target
                {
                    half2 displace = _Displace;
                    #if UNITY_UV_STARTS_AT_TOP
                    if (_MainTex_TexelSize.y < 0)
                        displace.y = -displace.y;
                    #endif

                    half4 c = tex2D(_MainTex, StereoScreenSpaceUVAdjust(i.uv, _MainTex_ST));
                    half4 n = c.rgba;

                    n += tex2D(_MainTex, StereoScreenSpaceUVAdjust(i.uv + half2(displace.x * 8.0, displace.y * 8.0), _MainTex_ST)) * 0.5;
                    n += tex2D(_MainTex, StereoScreenSpaceUVAdjust(i.uv + half2(displace.x * 16.0, displace.y * 16.0), _MainTex_ST)) * 0.3;
                    n += tex2D(_MainTex, StereoScreenSpaceUVAdjust(i.uv + half2(displace.x * 24.0, displace.y * 24.0), _MainTex_ST)) * 0.2;

                    n *= 0.5;

                    return lerp(c, n, _Amount);
                }

            ENDCG
        }
    }

    FallBack off
}
