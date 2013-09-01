#include "Constants.fxh"
#include "Structures.fxh"


float3 EyePosition;
float4x4 World;
float4x4 WorldViewProj;
float4x4 WorldInvTranspose;
float4x4 ShadowTransform;
float InvShadowMapSize;
float LargeTileLength;
float MedTileLength;
float SmallTileLength;
float2 LargeTexCoordOffset;

float4 MatSpecColor;

DirectionalLight DirLights[MAX_LIGHTS];
int NumLights;
int ShadowLightIndex;

float FogStart;
float FogEnd;
float4 FogColor;

#include "Common.fxh"

texture2D Texture1;
sampler2D Texture1Sampler = sampler_state
{
    Texture = <Texture1>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;
};

texture2D Texture2;
sampler2D Texture2Sampler = sampler_state
{
    Texture = <Texture2>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;
};

texture2D Texture3;
sampler2D Texture3Sampler = sampler_state
{
    Texture = <Texture3>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;
};

texture2D Texture3Normal;
sampler2D Texture3NormalSampler = sampler_state
{
    Texture = <Texture3Normal>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;
};

texture2D TextureBlend;
sampler2D TextureBlendSampler = sampler_state
{
    Texture = <TextureBlend>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = CLAMP;
    AddressV = CLAMP;
};


texture2D ShadowMap;
sampler2D ShadowMapSampler = sampler_state 
{
    Texture = <ShadowMap>;
    MinFilter = POINT;
    MagFilter = POINT;
    MipFilter = POINT;
    AddressU = CLAMP;
    AddressV = CLAMP;
};


float CalculateShadowFactor(float4 smapTexCoord)
{
    smapTexCoord.xyz /= smapTexCoord.w;

    float percentLit = 0.0f;

    const float2 offset[9] =
    {
        float2(-InvShadowMapSize, InvShadowMapSize),
        float2(0.0f, InvShadowMapSize),
        float2(InvShadowMapSize, InvShadowMapSize),
        float2(-InvShadowMapSize, 0.0f),
        float2(0.0f, 0.0f),
        float2(InvShadowMapSize, 0.0f),
        float2(-InvShadowMapSize, -InvShadowMapSize),
        float2(0.0f, -InvShadowMapSize),
        float2(InvShadowMapSize, -InvShadowMapSize)
    };

    float smapDepth;

    for (int i = 0; i < 9; i++)
    {
        smapDepth = tex2D(ShadowMapSampler, smapTexCoord.xy + offset[i]);
        percentLit += smapTexCoord.z <= smapDepth ? 1.0f : 0.0f;
    }

    float borderColor = smoothstep(0.0f, 0.1f, smapTexCoord.x) *
                      smoothstep(1.0f, 0.9f, smapTexCoord.x) *
                      smoothstep(0.0f, 0.1f, smapTexCoord.y) *
                      smoothstep(1.0f, 0.9f, smapTexCoord.y);

    return lerp(1.0f, percentLit / 9.0f, borderColor);
}


void VertexProc(float3   position        : POSITION,
                float3   normal          : NORMAL,
            out float4   oPosition       : POSITION,
            out float3   oNormal         : NORMAL,
            out float2   largeTexCoord   : TEXCOORD0,
            out float2   medTexCoord     : TEXCOORD1,
            out float2   smallTexCoord   : TEXCOORD2,
            out float3   eyeDisplacement : TEXCOORD3,
            out float4   shadowMapPos    : TEXCOORD4,
            out float3x3 tangentToWorld  : TEXCOORD5)    // Includes TEXCOORD6 and TEXCOORD7
{
    largeTexCoord = (position.xz  + LargeTexCoordOffset) / LargeTileLength;
    medTexCoord = position.xz / MedTileLength;
    smallTexCoord = position.xz / SmallTileLength;

    // Transform position from object space to world space:
    float4 wPosition = mul(float4(position, 1.0f), World);
    eyeDisplacement = EyePosition - wPosition.xyz;

    // Transform position from object space to clip space:
    oPosition = mul(float4(position, 1.0f), WorldViewProj);

    oNormal = mul(normal, (float3x3)WorldInvTranspose);

    float zPlaneTheta = atan2(normal.y, normal.x) - PI_OVER_2;
    float xPlaneTheta = atan2(normal.y, normal.z) - PI_OVER_2;

    float3 tangent = float3(cos(zPlaneTheta), sin(zPlaneTheta), 0.0f);
    float3 binormal = float3(0.0f, sin(xPlaneTheta), cos(xPlaneTheta));

    tangentToWorld[0] = mul(tangent, (float3x3)WorldInvTranspose);
    tangentToWorld[1] = mul(binormal, (float3x3)WorldInvTranspose);
    tangentToWorld[2] = mul(normal, (float3x3)WorldInvTranspose);

    shadowMapPos = mul(float4(position, 1.0f), ShadowTransform);
}


void PixelProc(float3   normal          : NORMAL,
               float2   largeTexCoord   : TEXCOORD0,
               float2   medTexCoord     : TEXCOORD1,
               float2   smallTexCoord   : TEXCOORD2,
               float3   eyeDisplacement : TEXCOORD3,
               float4   shadowMapPos    : TEXCOORD4,
               float3x3 tangentToWorld  : TEXCOORD5,
           out float4   oColor          : COLOR)
{
    Material surfaceMat;
    surfaceMat.Specular = MatSpecColor;

    tangentToWorld[0] = normalize(tangentToWorld[0]);
    tangentToWorld[1] = normalize(tangentToWorld[1]);
    tangentToWorld[2] = normalize(tangentToWorld[2]);

    // Fetch and expand range-compressed normal
    float3 normal3Tex = tex2D(Texture3NormalSampler, medTexCoord).xyz;
    float3 normal3TanSpc = expandNormalTex(normal3Tex);
    float3 normal3 = mul(normal3TanSpc, tangentToWorld);

    float4 tex1Diffuse = tex2D(Texture1Sampler, medTexCoord);
    float4 tex2Diffuse = tex2D(Texture2Sampler, medTexCoord);
    float4 tex3Diffuse = tex2D(Texture3Sampler, medTexCoord);
    float4 texBlend = tex2D(TextureBlendSampler, largeTexCoord);

    texBlend.b = length(normal.xz);
    texBlend.g = 0.0f;

    float4 overlayDiffuse = lerp(tex1Diffuse, tex3Diffuse, texBlend.b / (texBlend.b + texBlend.g));
    float3 overlayNormal = normalize(lerp(float3(0.0f, 1.0f, 0.0f), normal3, texBlend.b / (texBlend.b + texBlend.g)));
    surfaceMat.Diffuse = lerp(tex2Diffuse, overlayDiffuse, clamp(texBlend.b + texBlend.g, 0.0f, 1.0f));
    float3 surfaceNormal = normalize(lerp(float3(0.0f, 1.0f, 0.0f), overlayNormal, clamp(texBlend.b + texBlend.g, 0.0f, 1.0f)));

    float eyeDistance = length(eyeDisplacement);

    // Start with a sum of zero.
    oColor = float4(0.0f, 0.0f, 0.0f, 0.0f);

    float shadowFactor = CalculateShadowFactor(shadowMapPos);

    // Sum the light contribution from each light source.
    float4 ambientPiece, diffusePiece, specularPiece;

    for (int i = 0; i < NumLights; i++)
    {
        ComputeDirectionalLight(surfaceMat, DirLights[i], surfaceNormal, eyeDisplacement / eyeDistance, 
            ambientPiece, diffusePiece, specularPiece);

        // Add shade for the shadow mapped light.
        if (i == ShadowLightIndex)
        {
            diffusePiece *= shadowFactor;
            specularPiece *= shadowFactor;
        }
        oColor += ambientPiece + diffusePiece + specularPiece;
    }

    // Common to take alpha from diffuse material.
    oColor.a = surfaceMat.Diffuse.a;

    float fogFactor = 1.0f - exp2(1.0f - (FogEnd - FogStart) / max(FogEnd - max(eyeDistance, FogStart), 0.000001f));
    ApplyFog(oColor, fogFactor);
}


technique TheOnlyTechnique
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexProc();
        PixelShader = compile ps_3_0 PixelProc();
    }
}
