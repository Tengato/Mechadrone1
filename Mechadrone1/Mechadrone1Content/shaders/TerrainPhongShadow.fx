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
float4x4 gShadowTransform;
float    gInvShadowMapSize;
float    gSpecExpFactor;
float    gNumSpecLevels;
float3   gAmbientLight;
float    gLargeTileLength;
float    gMedTileLength;
float    gSmallTileLength;
float2   gLargeTexCoordOffset;
float4   gMatSpecColor;
float    gBright;
float    gContrast;

texture2D gTextureFlatBase;
sampler2D gTextureFlatBaseSampler = sampler_state
{
    Texture = <gTextureFlatBase>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;
};

texture2D gTextureFlatBlend;
sampler2D gTextureFlatBlendSampler = sampler_state
{
    Texture = <gTextureFlatBlend>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;
};

texture2D gTextureSteep;
sampler2D gTextureSteepSampler = sampler_state
{
    Texture = <gTextureSteep>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;
};

texture2D gTextureSteepNormal;
sampler2D gTextureSteepNormalSampler = sampler_state
{
    Texture = <gTextureSteepNormal>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;
};

texture2D gTextureLowFreq;
sampler2D gTextureLowFreqSampler = sampler_state
{
    Texture = <gTextureLowFreq>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

texture2D gShadowMap;
sampler2D gShadowMapSampler = sampler_state 
{
    Texture = <gShadowMap>;
    MinFilter = POINT;
    MagFilter = POINT;
    MipFilter = POINT;
    AddressU = CLAMP;
    AddressV = CLAMP;
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
            out float4   oPosition       : POSITION,
            out float2   oLargeTexCoord   : TEXCOORD0,
            out float2   oMedTexCoord     : TEXCOORD1,
            out float2   oSmallTexCoord   : TEXCOORD2,
            out float3   oToEye           : TEXCOORD3,
            out float4   oShadowMapPos    : TEXCOORD4,
            out float3x3 oTangentToWorld  : TEXCOORD5)    // Includes TEXCOORD6 and TEXCOORD7
{
    // Transform position from object space to world space:
    float4 wPosition = mul(float4(position, 1.0f), gWorld);
    oToEye = gEyePosition - wPosition.xyz;

    oLargeTexCoord = (wPosition.xz + gLargeTexCoordOffset) / gLargeTileLength;
    oMedTexCoord = wPosition.xz / gMedTileLength;
    oSmallTexCoord = wPosition.xz / gSmallTileLength;

    // Transform position from object space to clip space:
    oPosition = mul(float4(position, 1.0f), gWorldViewProj);

    float zPlaneTheta = atan2(normal.y, normal.x) - PI_OVER_2;
    float xPlaneTheta = atan2(normal.y, normal.z) - PI_OVER_2;

    float3 tangent = float3(cos(zPlaneTheta), sin(zPlaneTheta), 0.0f);
    float3 binormal = float3(0.0f, sin(xPlaneTheta), cos(xPlaneTheta));

    oTangentToWorld[0] = mul(tangent, (float3x3)gWorldInvTranspose);
    oTangentToWorld[1] = mul(binormal, (float3x3)gWorldInvTranspose);
    oTangentToWorld[2] = mul(normal, (float3x3)gWorldInvTranspose);

    oShadowMapPos = mul(float4(position, 1.0f), gShadowTransform);
}

void PixelProc(float2   largeTexCoord   : TEXCOORD0,
               float2   medTexCoord     : TEXCOORD1,
               float2   smallTexCoord   : TEXCOORD2,
               float3   toEye           : TEXCOORD3,
               float4   shadowMapPos    : TEXCOORD4,
               float3x3 tangentToWorld  : TEXCOORD5,
           out float4   oColor          : COLOR)
{
    Material surfaceMat;
    surfaceMat.Specular = gMatSpecColor;

    tangentToWorld[0] = normalize(tangentToWorld[0]);
    tangentToWorld[1] = normalize(tangentToWorld[1]);
    tangentToWorld[2] = normalize(tangentToWorld[2]);

    // Fetch and expand range-compressed normal
    float3 normalSteepTex = tex2D(gTextureSteepNormalSampler, medTexCoord).xyz;
    float3 normalSteepTanSpc = expandNormalTex(normalSteepTex);
    float3 normalSteep = mul(normalSteepTanSpc, tangentToWorld);

    float3 texFlatBaseDiffuse = tex2D(gTextureFlatBaseSampler, smallTexCoord);
    float3 texFlatBlendDiffuse = tex2D(gTextureFlatBlendSampler, smallTexCoord);
    float3 texSteepDiffuse = tex2D(gTextureSteepSampler, medTexCoord);
    float4 texLowFreq = tex2D(gTextureLowFreqSampler, largeTexCoord);

    float3 flatDiffuse = lerp(texFlatBaseDiffuse, texFlatBlendDiffuse, texLowFreq.r);
    float steepness = length(tangentToWorld[2].xz);
    float steepAmt = smoothstep(0.4f, 0.7f, steepness);
    surfaceMat.Diffuse = float4(lerp(flatDiffuse, texSteepDiffuse, steepAmt) * texLowFreq.g, 1.0f);
    float3 surfaceNormal = normalize(lerp(tangentToWorld[2], normalSteep, steepAmt));

    float distToEye = length(toEye);
    toEye /= distToEye;

    float shadowFactor = CalculateShadowFactor(shadowMapPos, gInvShadowMapSize, gShadowMapSampler);

    float3 viewReflect = reflect(-toEye, surfaceNormal);
    float lDotN = max(dot(toEye, surfaceNormal), 0.0f);

    // Convert normal vector into spherical texture coordinates:
    float4 normalTex;
    normalTex.y = acos(surfaceNormal.y) / PI;
    normalTex.x = (atan2(surfaceNormal.x, -surfaceNormal.z) + PI) / 2.0f / PI;
    normalTex.z = 0.0f;     // not used
    normalTex.w = 0.0f;     // mipmap level; irradiance map only has one level

    float4 viewReflectTex;
    viewReflectTex.y = acos(viewReflect.y) / PI;
    viewReflectTex.x = (atan2(viewReflect.x, -viewReflect.z) + PI) / 2.0f / PI;
    viewReflectTex.z = 0.0f;        // not used
    viewReflectTex.w = max(gNumSpecLevels - 1.0f - log(surfaceMat.Specular.w) / log(gSpecExpFactor), 0.0f);     // spec level

    float3 diffuse = tex2Dlod(gIrradianceMapSampler, normalTex).rgb * surfaceMat.Diffuse.rgb * shadowFactor;
    float3 specF = surfaceMat.Specular.rgb + (float3(1.0f, 1.0f, 1.0f) - surfaceMat.Specular.rgb) * pow(1.0f - lDotN, 5.0f);
    float3 spec = tex2Dlod(gSpecPrefilterSampler, viewReflectTex).rgb * lDotN * 0.2f * specF * shadowFactor;
    float3 ambient = surfaceMat.Diffuse.rgb * gAmbientLight * (1.0f - shadowFactor);

    // Tone map:
    oColor.rgb = gBright * pow(diffuse + spec + ambient, gContrast);

    // Linear space to sRGB
    oColor.rgb = pow(oColor.rgb, 1.0f / 1.6f);

    // Common to take alpha from diffuse material.
    oColor.a = surfaceMat.Diffuse.a;

    float fogFactor = 1.0f - exp2(1.0f - (gFogEnd - gFogStart) / max(gFogEnd - max(distToEye, gFogStart), 0.000001f));
    oColor.rgb = lerp(oColor.rgb, gFogColor, fogFactor);
}

technique TerrainPhongShadow
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexProc();
        PixelShader = compile ps_3_0 PixelProc();
    }
}
