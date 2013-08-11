#include "Constants.fxh"

float4x4 WorldViewProj;

void VertexShaderFunction(float3 position  : POSITION,
                      out float  depth     : TEXCOORD,
                      out float4 oPosition : POSITION)
{
    oPosition = mul(float4(position, 1.0f), WorldViewProj);
    depth = oPosition.z / oPosition.w;
}

void PixelShaderFunction(float  depth  : TEXCOORD,
                     out float4 oColor : COLOR)
{
    oColor = float4(depth + SHADOWMAP_BIAS, 0.0f, 0.0f, 0.0f);
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
