#include "Constants.fxh"

float4x4 gWorldViewProj;

texture2D Texture;
sampler2D gDiffuseTextureSampler = sampler_state
{
    Texture = <Texture>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
};

void VertexShaderFunction(float3 position  : POSITION,
                          float2 texCoord  : TEXCOORD,
                      out float4 oPosition : POSITION,
                      out float2 oTexCoord : TEXCOORD)
{
    oPosition = mul(float4(position, 1.0f), gWorldViewProj);
    oTexCoord = texCoord;
}


void PixelShaderFunction(out float4 oColor : COLOR)
{
    oColor = float4(1.0f, 1.0f, 1.0f, 1.0f);
}


technique DepthOnlyAlpha
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
