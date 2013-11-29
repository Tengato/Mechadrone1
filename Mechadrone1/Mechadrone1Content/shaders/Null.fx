#include "Constants.fxh"
#include "Structures.fxh"


float3 EyePosition;
float4x4 World;
float4x4 WorldViewProj;
float4x4 WorldInvTranspose;

float4 MatSpecColor;

DirectionalLight DirLights[MAX_LIGHTS];
int NumLights;

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


void VertexProc(float3 position  : POSITION,
            out float4 oPosition : POSITION)
{
    oPosition = mul(float4(position, 1.0f), WorldViewProj);
}

void PixelProc(float  depth  : TEXCOORD,
           out float4 oColor : COLOR)
{
    oColor = VECTOR4_ZERO;
    clip(-1);
}


technique TheOnlyTechnique
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexProc();
        PixelShader = compile ps_3_0 PixelProc();
    }
}
