Shader "Hidden/ZPrepass"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
	}

    SubShader
    {
        Pass
        {
			Cull [_Cull]
			ZWrite On
			ZTest LEqual
			ColorMask 0

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile __ USE_CUTOUT
            #pragma multi_compile __ USE_TEXTURE
			#pragma multi_compile __ TEXARRAY_CUTOUT
			#pragma multi_compile __ EPO_HDRP
			#pragma fragmentoption ARB_precision_hint_fastest

            #include "UnityCG.cginc"
            #include "MiskCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
#if USE_CUTOUT
                float2 uv : TEXCOORD0;
#endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
#if USE_CUTOUT
                float2 uv : TEXCOORD0;
#endif
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
			DEFINE_CUTOUT
			DefineCoords

            v2f vert (appdata v)
            {
                v2f o;
                
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);

#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
#endif

                FixDepth
				TRANSFORM_CUTOUT

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				CHECK_CUTOUT

                return float4(1, 0, 1, 0);
            }
            ENDCG
        }
    }
}
