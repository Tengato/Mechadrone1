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
float    gBright;
float    gContrast;

texture2D gNormalMap;
sampler2D gNormalMapSampler = sampler_state
{
    Texture = <gNormalMap>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
};

texture2D Texture;
sampler2D gDiffuseTextureSampler = sampler_state
{
    Texture = <Texture>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
};

texture2D gSpecMap;
sampler2D gSpecMapSampler = sampler_state
{
    Texture = <gSpecMap>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
};

texture2D gEmitMap;
sampler2D gEmitMapSampler = sampler_state
{
    Texture = <gEmitMap>;
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
                float3   tangent         : TANGENT,
                float3   binormal        : BINORMAL,
                float2   texCoord        : TEXCOORD0,
            out float4   oPosition       : POSITION,
            out float2   oTexCoord       : TEXCOORD0,
            out float3   oToEye          : TEXCOORD1,
            out float3x3 oTangentToWorld  : TEXCOORD2)    // Includes TEXCOORD3 and TEXCOORD4
{
    oTexCoord = texCoord;

    // Transform position from object space to world space:
    float4 wPosition = mul(float4(position, 1.0f), gWorld);
    oToEye = gEyePosition - wPosition.xyz;

    // Transform position from object space to clip space:
    oPosition = mul(float4(position, 1.0f), gWorldViewProj);

    // Construct tangent-space-to-world-space 3x3 matrix
    oTangentToWorld[0] = mul(tangent, (float3x3)gWorldInvTranspose);
    oTangentToWorld[1] = mul(binormal, (float3x3)gWorldInvTranspose);
    oTangentToWorld[2] = mul(normal, (float3x3)gWorldInvTranspose);
}

void PixelProc(float2   texCoord        : TEXCOORD0,
               float3   toEye           : TEXCOORD1,
               float3x3 tangentToWorld  : TEXCOORD2,
           out float4   oColor          : COLOR)
{
    tangentToWorld[0] = normalize(tangentToWorld[0]);
    tangentToWorld[1] = normalize(tangentToWorld[1]);
    tangentToWorld[2] = normalize(tangentToWorld[2]);

    // Fetch and expand range-compressed normal
    float3 normalTex = tex2D(gNormalMapSampler, texCoord).xyz;
    float3 normalTanSpc = expandNormalTex(normalTex);
    float3 normal = mul(normalTanSpc, tangentToWorld);

    Material surfaceMat;
    surfaceMat.Specular = tex2D(gSpecMapSampler, texCoord);
    surfaceMat.Specular.w = pow(1.03678, surfaceMat.Specular.w * 255.0f);
    surfaceMat.Diffuse = tex2D(gDiffuseTextureSampler, texCoord);

    float3 emitColor = tex2D(gEmitMapSampler, texCoord);

    float toEyeDist = length(toEye);
    toEye /= toEyeDist;

    float3 viewReflect = reflect(-toEye, normal);
    float lDotN = max(dot(toEye, normal), 0.0f);

    // Convert normal vector into spherical texture coordinates:
    float4 normalSphere;
    normalSphere.y = acos(normal.y) / PI;
    normalSphere.x = -atan2(normal.x, normal.z) / 2.0f / PI;
    normalSphere.z = 0.0f;     // not used
    normalSphere.w = 0.0f;     // mipmap level; irradiance map only has one level

    float4 viewReflectTex;
    viewReflectTex.y = acos(viewReflect.y) / PI;
    viewReflectTex.x = -atan2(viewReflect.x, viewReflect.z) / 2.0f / PI;
    viewReflectTex.z = 0.0f;        // not used
    viewReflectTex.w = max(gNumSpecLevels - 1.0f - log(surfaceMat.Specular.w) / log(gSpecExpFactor), 0.0f);     // spec level

    float3 diffuse = tex2Dlod(gIrradianceMapSampler, normalSphere).rgb * surfaceMat.Diffuse.rgb;
    float3 specF = surfaceMat.Specular.rgb + (float3(1.0f, 1.0f, 1.0f) - surfaceMat.Specular.rgb) * pow(1.0f - lDotN, 5.0f);
    float3 spec = tex2Dlod(gSpecPrefilterSampler, viewReflectTex).rgb * lDotN * specF;

    // Tone map:
    oColor.rgb = gBright * pow(diffuse + spec, gContrast);

    oColor.rgb = saturate(oColor.rgb + emitColor);

    // Linear space to sRGB
    oColor.rgb = pow(oColor.rgb, 1.0f / 2.2f);

    // Common to take alpha from diffuse material.
    oColor.a = surfaceMat.Diffuse.a;

    float fogFactor = 1.0f - exp2(1.0f - (gFogEnd - gFogStart) / max(gFogEnd - max(toEyeDist, gFogStart), 0.000001f));
    oColor.rgb = lerp(oColor.rgb, gFogColor, fogFactor);
}


technique NormSpecPhong
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexProc();
        PixelShader = compile ps_3_0 PixelProc();
    }
}
