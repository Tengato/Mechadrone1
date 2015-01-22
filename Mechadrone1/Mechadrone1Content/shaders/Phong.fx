#include "Constants.fxh"
#include "Structures.fxh"
#include "Common.fxh"

float3   gEyePosition;
float4x4 gWorld;
float4x4 gWorldViewProj;
float4x4 gWorldInvTranspose;
float    gFogStart;
float    gFogEnd;
float3   gFogColor;
float    gSpecExpFactor;
float    gNumSpecLevels;
float3   gAmbientLight;
float4   gMatSpecColor;
float    gBright;
float    gContrast;

texture2D Texture;
sampler2D gTextureSampler = sampler_state
{
    Texture = <Texture>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
};

texture2D gIrradianceMap;
sampler2D gIrradianceMapSampler = sampler_state
{
    Texture = <gIrradianceMap>;
    MinFilter = POINT;
    MagFilter = POINT;
    MipFilter = POINT;
    AddressU = WRAP;
    AddressV = CLAMP;
};

texture2D gSpecPrefilter;
sampler2D gSpecPrefilterSampler = sampler_state
{
    Texture = <gSpecPrefilter>;
    MinFilter = POINT;
    MagFilter = POINT;
    MipFilter = POINT;
    AddressU = WRAP;
    AddressV = CLAMP;
};

void VertexProc(float3   position        : POSITION,
                float3   normal          : NORMAL,
                float2   texCoord        : TEXCOORD0,
            out float4   oPosition       : POSITION,
            out float3   oNormal         : NORMAL,
            out float2   oTexCoord       : TEXCOORD0,
            out float3   oToEye          : TEXCOORD1)
{
    oTexCoord = texCoord;

    // Transform position from object space to world space:
    float4 wPosition = mul(float4(position, 1.0f), gWorld);
    oToEye = gEyePosition - wPosition.xyz;

    // Transform position from object space to clip space:
    oPosition = mul(float4(position, 1.0f), gWorldViewProj);
    oNormal = mul(normal, (float3x3)gWorldInvTranspose);
}

void PixelProc(float3   normal          : NORMAL,
               float2   texCoord        : TEXCOORD0,
               float3   toEye           : TEXCOORD1,
           out float4   oColor          : COLOR)
{
    normal = normalize(normal);

    Material surfaceMat;
    surfaceMat.Specular = gMatSpecColor;
    surfaceMat.Diffuse = tex2D(gTextureSampler, texCoord);

    float distToEye = length(toEye);
    toEye /= distToEye;

    float3 viewReflect = reflect(-toEye, normal);
    float lDotN = max(dot(toEye, normal), 0.0f);

    // Convert normal vector into spherical texture coordinates:
    float4 normalTex;
    normalTex.y = acos(normal.y) / PI;
    normalTex.x = (atan2(normal.x, -normal.z) + PI) / 2.0f / PI;
    normalTex.z = 0.0f;     // not used
    normalTex.w = 0.0f;     // mipmap level; irradiance map only has one level

    float4 viewReflectTex;
    viewReflectTex.y = acos(viewReflect.y) / PI;
    viewReflectTex.x = (atan2(viewReflect.x, -viewReflect.z) + PI) / 2.0f / PI;
    viewReflectTex.z = 0.0f;        // not used
    viewReflectTex.w = max(gNumSpecLevels - 1.0f - log(surfaceMat.Specular.w) / log(gSpecExpFactor), 0.0f);     // spec level

    float3 diffuse = tex2Dlod(gIrradianceMapSampler, normalTex).rgb * surfaceMat.Diffuse.rgb;
    float3 specF = surfaceMat.Specular.rgb + (float3(1.0f, 1.0f, 1.0f) - surfaceMat.Specular.rgb) * pow(1.0f - lDotN, 5.0f);
    float3 spec = tex2Dlod(gSpecPrefilterSampler, viewReflectTex).rgb * lDotN * 0.2f * specF;
    float3 ambient = surfaceMat.Diffuse.rgb * gAmbientLight;

    // Tone map:
    oColor.rgb = gBright * pow(diffuse + spec + ambient, gContrast);

    // Linear space to sRGB
    oColor.rgb = pow(oColor.rgb, 1.0f / 1.6f);

    // Common to take alpha from diffuse material.
    oColor.a = surfaceMat.Diffuse.a;

    float fogFactor = 1.0f - exp2(1.0f - (gFogEnd - gFogStart) / max(gFogEnd - max(distToEye, gFogStart), 0.000001f));
    oColor.rgb = lerp(oColor.rgb, gFogColor, fogFactor);
}

technique Phong
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexProc();
        PixelShader = compile ps_3_0 PixelProc();
    }
}
