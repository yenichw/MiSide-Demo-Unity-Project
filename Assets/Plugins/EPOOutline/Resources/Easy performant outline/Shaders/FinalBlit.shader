Shader "Hidden/FinalBlit"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Blend One OneMinusSrcAlpha

        Pass
        {
            Stencil 
            {
                Ref [_OutlineRef]
                Comp [_Comparison]
                Pass [_Operation]
                ZFail Keep
                Fail Keep
                ReadMask [_ReadMask]
                WriteMask 255
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile __ USE_INFO_BUFFER
			#pragma fragmentoption ARB_precision_hint_fastest
            
            #include "UnityCG.cginc"
            #include "MiskCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
				DefineTransform
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;

                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            half _EffectSize;

            UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
            half4 _MainTex_ST;
            half4 _MainTex_TexelSize;

            UNITY_DECLARE_SCREENSPACE_TEXTURE(_InitialTex);
            half4 _InitialTex_ST;
            half4 _InitialTex_TexelSize;

			DefineCoords
            
            UNITY_INSTANCING_BUFFER_START (Properties)
            UNITY_DEFINE_INSTANCED_PROP (float4x4, _NormalMatrices)
            UNITY_INSTANCING_BUFFER_END(Properties)

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				
                o.vertex = UnityObjectToClipPos(v.vertex);
				
                ComputeScreenShift

                o.uv = ComputeScreenPos(o.vertex);
				o.uv.xy *= _Scale;

                return o;
            }
            
            half4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                return FetchTexel(i.uv.xy / i.uv.w);
            }
            ENDCG
        }
    }
}
