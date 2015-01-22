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

texture2D Texture;
sampler2D gDiffuseTextureSampler = sampler_state
{
    Texture = <Texture>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
};

void VertexProc(float3   position        : POSITION,
                float2   texCoord        : TEXCOORD0,
            out float4   oPosition       : POSITION,
            out float2   oTexCoord       : TEXCOORD0,
            out float3   oToEye          : TEXCOORD1)
{
    oTexCoord = texCoord;

    // Transform position from object space to world space:
    float4 wPosition = mul(float4(position, 1.0f), gWorld);
    oToEye = gEyePosition - wPosition.xyz;

    // Transform position from object space to clip space:
    oPosition = mul(float4(position, 1.0f), gWorldViewProj);
}

void PixelProc(float2   texCoord        : TEXCOORD0,
               float3   toEye           : TEXCOORD1,
           out float4   oColor          : COLOR)
{
    oColor = tex2D(gDiffuseTextureSampler, texCoord);
    float toEyeDist = length(toEye);
    float fogFactor = 1.0f - exp2(1.0f - (gFogEnd - gFogStart) / max(gFogEnd - max(toEyeDist, gFogStart), 0.000001f));
    oColor.rgb = lerp(oColor.rgb, gFogColor, fogFactor);
}

technique DirectTexture
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexProc();
        PixelShader = compile ps_3_0 PixelProc();
    }
}
