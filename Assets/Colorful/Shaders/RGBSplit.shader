// Colorful FX - Unity Asset
// Copyright (c) 2015 - Thomas Hourdel
// http://www.thomashourdel.com

Shader "Hidden/Colorful/RGB Split"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Params ("Amount (X) Angle Sin (Y) Angle Cos (Z)", Vector) = (0, 0, 0, 0)
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
                float4 _MainTex_TexelSize;
                float4 _MainTex_ST;
                half3 _Params;

                half4 frag(v2f_img i) : SV_Target
                {
                    half paramsy = _Params.y;
                    #if UNITY_UV_STARTS_AT_TOP
                    if (_MainTex_TexelSize.y < 0)
                        paramsy = -paramsy;
                    #endif

                    half2 coords = i.uv;
                    half  d = distance(coords, half2(0.5, 0.5));
                    half  amount = _Params.x * d * 2;
                    half2 offset = amount * half2(_Params.z, paramsy);
                    half  cr  = tex2D(_MainTex, StereoScreenSpaceUVAdjust(coords + offset, _MainTex_ST)).r;
                    half2 cga = tex2D(_MainTex, StereoScreenSpaceUVAdjust(coords, _MainTex_ST)).ga;
                    half  cb  = tex2D(_MainTex, StereoScreenSpaceUVAdjust(coords - offset, _MainTex_ST)).b;

                    // Stupid hack to make it work with d3d9 (CG compiler bug ?)
                    return half4(cr + 0.0000001, cga.x + 0.0000002, cb + 0.0000003, cga.y);
                }

            ENDCG
        }
    }

    FallBack off
}
