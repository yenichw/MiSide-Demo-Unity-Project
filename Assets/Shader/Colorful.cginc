// Colorful FX - Unity Asset
// Copyright (c) 2015 - Thomas Hourdel
// http://www.thomashourdel.com

/* Constants */
#define PI 3.14159265

/* Single-pass rendering compatibility */
float2 StereoTransformScreenSpaceTex(float2 uv)
{
    #if defined(UNITY_SINGLE_PASS_STEREO)
        return UnityStereoTransformScreenSpaceTex(uv);
    #else
        return uv;
    #endif
}

float2 StereoScreenSpaceUVAdjust(float2 uv, float4 st)
{
    #if defined(UNITY_SINGLE_PASS_STEREO)
        return UnityStereoScreenSpaceUVAdjust(uv, st);
    #else
        return uv;
    #endif
}

/* Maths */
half luminance(half3 color)
{
    return dot(color, half3(0.222, 0.707, 0.071));
}

half3 mod(half3 x, half3 y)
{
    return x - y * floor(x / y);
}

half2 mod(half2 x, half2 y)
{
    return x - y * floor(x / y);
}

half mod(half x, half y)
{
    return x - y * floor(x / y);
}

half rot(half value, half low, half hi)
{
    return (value < low) ? value + hi : (value > hi) ? value - hi : value;
}

half rot10(half value)
{
    return rot(value, 0.0, 1.0);
}

float simpleNoise(float2 uv)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
}

float simpleNoise_fracLess(float2 uv)
{
    return sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453;
}

half invlerp(half from, half to, half value)
{
    return (value - from) / (to - from);
}

/* Distortions */
half4 pixelate(sampler2D tex, float2 uv, float scale, float ratio)
{
    float ds = 1.0 / scale;
    float2 coord = float2(ds * ceil(uv.x / ds), (ds * ratio) * ceil(uv.y / ds / ratio));
    return half4(tex2D(tex, coord).xyzw);
}

half4 pixelate(sampler2D tex, float2 uv, float2 scale)
{
    float2 ds = 1.0 / scale;
    float2 coord = ds * ceil(uv / ds);
    return half4(tex2D(tex, coord).xyzw);
}

float2 barrelDistortion(float2 coord, float spherical, float barrel, float scale) 
{
    // Inspired by SynthEyes lens distortion algorithm
    // See http://www.ssontech.com/content/lensalg.htm
    float2 h = coord.xy - float2(0.5, 0.5);
    float r2 = dot(h, h);
    float f = 1.0 + r2 * (spherical + barrel * sqrt(r2));
    return f * scale * h + 0.5;
}

/* Color conversion */
half3 HSVtoRGB(half3 c)
{
    half4 K = half4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    half3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
}

half3 RGBtoHSV(half3 c)
{
    half4 K = half4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    half4 p = lerp(half4(c.bg, K.wz), half4(c.gb, K.xy), step(c.b, c.g));
    half4 q = lerp(half4(p.xyw, c.r), half4(c.r, p.yzx), step(p.x, c.r));
    half d = q.x - min(q.w, q.y);
    half e = 1.0e-4;
    return half3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

half RGBtoHUE(half3 c)
{
    half4 K = half4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    half4 p = lerp(half4(c.bg, K.wz), half4(c.gb, K.xy), step(c.b, c.g));
    half4 q = lerp(half4(p.xyw, c.r), half4(c.r, p.yzx), step(p.x, c.r));
    half d = q.x - min(q.w, q.y);
    return abs(q.z + (q.w - q.y) / (6.0 * d + 1.0e-10));
}

half3 HUEtoRGB(half h)
{
    half h6 = h * 6.0;
    half r = abs(h6 - 3.0) - 1.0;
    half g = 2.0 - abs(h6 - 2.0);
    half b = 2.0 - abs(h6 - 4.0);
    return saturate(half3(r, g, b));
}

half3 RGBtoYUV(half3 c)
{
    half3 yuv;
    yuv.x = dot(c, half3(0.299, 0.587, 0.114));
    yuv.y = dot(c, half3(-0.14713, -0.28886, 0.436));
    yuv.z = dot(c, half3(0.615, -0.51499, -0.10001));
    return yuv;
}

half3 YUVtoRGB(half3 c)
{
    half3 rgb;
    rgb.r = c.x + c.z * 1.13983;
    rgb.g = c.x + dot(half2(-0.39465, -0.58060), c.yz);
    rgb.b = c.x + c.y * 2.03211;
    return rgb;
}

half4 RGBtoCMYK(half3 c)
{
    half k = max(max(c.r, c.g), c.b);
    return min(half4(c.rgb / k, k), 1.0);
}

half3 CMYKtoRGB(half4 c)
{
    return c.rgb * c.a;
}

/*
 * sRGB <-> Linear
 */
half3 sRGB(half3 c)
{
#if defined(SHADER_API_MOBILE)
    return max(1.055 * pow(c, 0.416666667) - 0.055, 0.0);
#else
    half3 sRGBLo = c * 12.92;
    half3 sRGBHi = (pow(c, half3(1.0 / 2.4, 1.0 / 2.4, 1.0 / 2.4)) * 1.055) - 0.055;
    half3 sRGB = (c <= 0.0031308) ? sRGBLo : sRGBHi;
    return sRGB;
#endif
}

half4 sRGB(half4 c)
{
    return half4(sRGB(c.rgb), c.a);
}

half3 Linear(half3 c)
{
#if defined(SHADER_API_MOBILE)
    return c * (c * (c * 0.305306011 + 0.682171111) + 0.012522878);
#else
    half3 linearRGBLo = c / 12.92;
    half3 linearRGBHi = pow((c + 0.055) / 1.055, half3(2.4, 2.4, 2.4));
    half3 linearRGB = (c <= 0.04045) ? linearRGBLo : linearRGBHi;
    return linearRGB;
#endif
}

half4 Linear(half4 c)
{
    return half4(Linear(c.rgb), c.a);
}
