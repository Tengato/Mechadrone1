#include "Constants.fxh"
#include "Structures.fxh"


float3 EyePosition;
float4x4 World;
float4x4 WorldViewProj;
float4x4 WorldInvTranspose;

DirectionalLight DirLights[MAX_LIGHTS];
int NumLights;

float FogStart;
float FogEnd;
float4 FogColor;

texture2D Texture;
sampler2D DiffuseTextureSampler = sampler_state
{
    Texture = <Texture>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
};

#include "Common.fxh"


void VertexProc(float3   position        : POSITION,
                float2   texCoord        : TEXCOORD0,
            out float4   oPosition       : POSITION,
            out float2   oTexCoord       : TEXCOORD0,
            out float3   eyeDisplacement : TEXCOORD1)
{
    oTexCoord = texCoord;

    // Transform position from object space to world space:
    float4 wPosition = mul(float4(position, 1.0f), World);
    eyeDisplacement = EyePosition - wPosition.xyz;

    // Transform position from object space to clip space:
    oPosition = mul(float4(position, 1.0f), WorldViewProj);
}


void PixelProc(float2   texCoord        : TEXCOORD0,
               float3   eyeDisplacement : TEXCOORD1,
           out float4   oColor          : COLOR)
{
    oColor = tex2D(DiffuseTextureSampler, texCoord);
    float eyeDistance = length(eyeDisplacement);
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
