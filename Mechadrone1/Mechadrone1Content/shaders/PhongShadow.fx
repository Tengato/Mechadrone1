#include "Constants.fxh"
#include "Structures.fxh"


float3 EyePosition;
float4x4 World;
float4x4 WorldViewProj;
float4x4 WorldInvTranspose;
float4x4 ShadowTransform;
float InvShadowMapSize;

float4 MatSpecColor;

DirectionalLight DirLights[MAX_LIGHTS];
int NumLights;
int ShadowLightIndex;

float FogStart;
float FogEnd;
float4 FogColor;

#include "Common.fxh"

texture2D Texture;
sampler2D DiffuseTextureSampler = sampler_state
{
    Texture = <Texture>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
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

    smapTexCoord.xyz /= smapTexCoord.w;

    float smapInfluence = smoothstep(0.0f, 0.1f, smapTexCoord.x) *
                      smoothstep(1.0f, 0.9f, smapTexCoord.x) *
                      smoothstep(0.0f, 0.1f, smapTexCoord.y) *
                      smoothstep(1.0f, 0.9f, smapTexCoord.y);

    float shadowFactor = 1.0;

    if (smapInfluence >= 0.005f)
    {
        float percentLit = 0.0f;

        for (int i = 0; i < 9; i++)
        {
            float smapDepth = tex2D(ShadowMapSampler, smapTexCoord.xy + offset[i]);

            if (smapTexCoord.z <= smapDepth)
                percentLit += 1.0f;
        }

        percentLit /= 9.0f;

        shadowFactor = lerp(1.0f, percentLit, smapInfluence);
    }

    return shadowFactor;
}


void VertexProc(float3   position        : POSITION,
                float3   normal          : NORMAL,
                float2   texCoord        : TEXCOORD0,
            out float4   oPosition       : POSITION,
            out float3   oNormal         : NORMAL,
            out float2   oTexCoord       : TEXCOORD0,
            out float3   eyeDisplacement : TEXCOORD1,
            out float4   shadowMapPos    : TEXCOORD2)
{
    oTexCoord = texCoord;

    // Transform position from object space to world space:
    float4 wPosition = mul(float4(position, 1.0f), World);
    eyeDisplacement = EyePosition - wPosition.xyz;

    // Transform position from object space to clip space:
    oPosition = mul(float4(position, 1.0f), WorldViewProj);

    oNormal = mul(normal, (float3x3)WorldInvTranspose);

    shadowMapPos = mul(float4(position, 1.0f), ShadowTransform);
}


void PixelProc(float3   normal          : NORMAL,
               float2   texCoord        : TEXCOORD0,
               float3   eyeDisplacement : TEXCOORD1,
               float4   shadowMapPos    : TEXCOORD2,
           out float4   oColor          : COLOR)
{
    Material surfaceMat;
    surfaceMat.Specular = MatSpecColor;
    surfaceMat.Diffuse = tex2D(DiffuseTextureSampler, texCoord);

    float eyeDistance = length(eyeDisplacement);

    // Start with a sum of zero.
    oColor = VECTOR4_ZERO;

    float shadowFactor = CalculateShadowFactor(shadowMapPos);

    // Sum the light contribution from each light source.
    float4 ambientPiece, diffusePiece, specularPiece;

    for (int i = 0; i < NumLights; i++)
    {
        ComputeDirectionalLight(surfaceMat, DirLights[i], normalize(normal), eyeDisplacement / eyeDistance, 
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
