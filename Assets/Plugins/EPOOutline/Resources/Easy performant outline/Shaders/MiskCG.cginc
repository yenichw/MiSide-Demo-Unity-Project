#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
#define FetchTexel(uv) UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex,(uv))
#define FetchTexelAt(uv) FetchTexelAtFrom(_MainTex,(uv),_MainTex_ST)
#define FetchTexelAtWithShift(uv,shift) FetchTexelAtFrom(_MainTex,(uv)+(shift),_MainTex_ST)
#define FetchTexelAtFrom(tex,uv,texST) UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,(uv))
#else
#define FetchTexel(uv) UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, UnityStereoScreenSpaceUVAdjust((uv),_MainTex_ST))
#define FetchTexelAt(uv) FetchTexelAtFrom(_MainTex,(uv),_MainTex_ST)
#define FetchTexelAtWithShift(uv,shift) UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex,UnityStereoScreenSpaceUVAdjust((uv),(_MainTex_ST))+(shift))
#define FetchTexelAtFrom(tex,uv,texST) UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex,UnityStereoScreenSpaceUVAdjust((uv),(texST)))
#endif

#define DEFINE_VERTEX_CUTOUT_DATA(i) 

#if USE_INFO_BUFFER
#define GetMaskingScaler(inUV)

#else

#endif

inline float GetScaler(float4 inUV, half4 info)
{
#if USE_INFO_BUFFER
	float2 uv = inUV.xy / inUV.w;

	float scaler = 1.0f;

#if BACK_OBSTACLE_RENDERING
	scaler = abs(info.b - 0.25f) < 0.05f;
#endif

#if BACK_MASKING_RENDERING
	scaler = abs(info.b - 0.6f) > 0.05f;
#endif

#else
	float scaler = 1.0f;
#endif

	return scaler;
}

#define DefineEdgeDilateParameters float3 normal : TEXCOORD6;

#define ComputeScreenShift o.vertex.xy += EPOComputeShift(v.normal, 1.41f * _MainTex_TexelSize.xy * 2.0f * (_EffectSize + v.additionalScale.x) * o.vertex.w, UNITY_ACCESS_INSTANCED_PROP(Properties, _NormalMatrices));

#define ComputeSmoothScreenShift float2 clipNormal = (mul((float3x3) UNITY_MATRIX_MVP, mul((float3x3) UNITY_MATRIX_M, v.normal))).xy; o.vertex.xy += (normalize(clipNormal) / _ScreenParams.xy) * 2.0f * _DilateShift * o.vertex.w;

#define DefineTransform float2 additionalScale : TEXCOORD0;

#define DefineCoords float2 _Scale;

inline float2 EPOComputeShift(float3 normal, float2 shiftAmount, float4x4 normalMatrix)
{
	float2 transformedNormal = mul(mul((float3x3)unity_MatrixVP, (float3x3) normalMatrix), normal).xy;
	transformedNormal = normalize(transformedNormal);

	return transformedNormal.xy * shiftAmount;
}

#if UNITY_UV_STARTS_AT_TOP
#define CheckY o.vertex.y *= -_ProjectionParams.x;
#else
#define CheckY;
#endif

#if defined(UNITY_REVERSED_Z) 
#define ChangeDepth o.vertex.z += 0.0001f;
#else
#define ChangeDepth o.vertex.z -= 0.0001f;
#endif

#if EPO_HDRP
#define FixDepth ChangeDepth
#else
#define FixDepth
#endif

#if USE_CUTOUT
	#if TEXARRAY_CUTOUT
	#define DEFINE_CUTOUT UNITY_DECLARE_TEX2DARRAY(_CutoutTexture); half4 _CutoutTexture_ST; half _CutoutThreshold; float _TextureIndex; float4 _CutoutMask;
	#else
	#define DEFINE_CUTOUT sampler2D _CutoutTexture; half4 _CutoutTexture_ST; half _CutoutThreshold; float4 _CutoutMask;
	#endif
#else
#define DEFINE_CUTOUT
#endif

#if USE_CUTOUT
#if TEXARRAY_CUTOUT
#define READ_CUTOUT UNITY_SAMPLE_TEX2DARRAY(_CutoutTexture, float3(i.uv, _TextureIndex))
#else
#define READ_CUTOUT tex2D(_CutoutTexture, i.uv)
#endif
#endif

#if USE_CUTOUT
	#define CHECK_CUTOUT clip(dot(_CutoutMask, READ_CUTOUT) - _CutoutThreshold);
#else
#define CHECK_CUTOUT
#endif

#if USE_CUTOUT
	#define TRANSFORM_CUTOUT o.uv = TRANSFORM_TEX(v.uv, _CutoutTexture);
#else
#define TRANSFORM_CUTOUT
#endif