Shader "Tutorial/019_OutlinesPostprocessed" {
    Properties {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Vector) = (0,0,0,1)
        _NormalMult ("Normal Outline Multiplier", Range(0, 4)) = 1
        _NormalBias ("Normal Outline Bias", Range(1, 4)) = 1
        _DepthMult ("Depth Outline Multiplier", Range(0, 4)) = 1
        _DepthBias ("Depth Outline Bias", Range(1, 4)) = 1
        _BumpMap ("Normal Map", 2D) = "bump" {}
    }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Opaque" "IgnoreProjector"="True" }
        Pass {
            ZTest Always Cull Off ZWrite Off
            Fog { Mode off }
            Offset -1, -1

            CGPROGRAM
// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
#pragma exclude_renderers d3d11 gles
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
            };

            v2f vert (appdata_base v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            sampler2D _BumpMap;
            float4 _OutlineColor;
            float _NormalMult;
            float _NormalBias;
            float _DepthMult;
            float _DepthBias;

            fixed4 frag (v2f i) : SV_Target {
                // Sample the main texture
                fixed4 originalColor = tex2D(_MainTex, i.uv);

                // Sample depth at current pixel
                float centerDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                centerDepth = Linear01Depth(centerDepth);

                // Compute pixel size in texture coordinates
                float2 pixelSize = float2(1 / _ScreenParams.x, 1 / _ScreenParams.y);

                // Define offsets for neighboring pixels
                float2 offsets[8] = float2[](
                    float2(-pixelSize.x, 0),
                    float2(pixelSize.x, 0),
                    float2(0, -pixelSize.y),
                    float2(0, pixelSize.y),
                    float2(-pixelSize.x, -pixelSize.y),
                    float2(-pixelSize.x, pixelSize.y),
                    float2(pixelSize.x, -pixelSize.y),
                    float2(pixelSize.x, pixelSize.y)
                );

                // Accumulate depth differences
                float depthSum = 0;
                for (int j = 0; j < 8; j++) {
                    float neighborDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, clamp(i.uv + offsets[j], 0, 1));
                    neighborDepth = Linear01Depth(neighborDepth);
                    float depthDiff = abs(centerDepth - neighborDepth);
                    depthSum += depthDiff;
                }

                // Compute average depth difference
                float averageDepthDiff = depthSum / 8;

                // Weight the depth difference
                float weightedDepthDiff = averageDepthDiff * _DepthMult;

                // Accumulate normal differences
                float normalSum = 0;
                for (int j = 0; j < 8; j++) {
                    float2 offset = offsets[j];
                    float2 neighborUV = clamp(i.uv + offset, 0, 1);
                    float3 neighborNormal = UnpackNormal(tex2D(_BumpMap, neighborUV));
                    float normalDiff = 1 - dot(i.normal, neighborNormal);
                    normalSum += normalDiff;
                }

                // Compute average normal difference
                float averageNormalDiff = normalSum / 8;

                // Weight the normal difference
                float weightedNormalDiff = averageNormalDiff * _NormalMult;

                // Decide if it's an edge based on weighted depth and normal differences and their biases
                if (weightedDepthDiff > _DepthBias || weightedNormalDiff > _NormalBias) {
                    return _OutlineColor;
                } else {
                    return originalColor;
                }
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}