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

float4 Color;

#include "Common.fxh"


void VertexProc(float3   position        : POSITION,
            out float4   oPosition       : POSITION)
{
    // Transform position from object space to clip space:
    oPosition = mul(float4(position, 1.0f), WorldViewProj);
}


void PixelProc(out float4   oColor          : COLOR)
{
    oColor = Color;
}


technique FlatColor
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexProc();
        PixelShader = compile ps_3_0 PixelProc();
    }
}
