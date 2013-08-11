#include "Constants.fxh"

float4x4 WorldViewProj;
int WeightsPerVert;
float4x3 PosedBones[SKINNED_EFFECT_MAX_BONES];

void VertexShaderFunction(float3 position  : POSITION,
                          int4   indices   : BLENDINDICES,
                          float4 weights   : BLENDWEIGHT,
                      out float  depth     : TEXCOORD,
                      out float4 oPosition : POSITION)
{
    float4x3 skinning = {0.0f, 0.0f, 0.0f,
                         0.0f, 0.0f, 0.0f,
                         0.0f, 0.0f, 0.0f,
                         0.0f, 0.0f, 0.0f};

    [unroll]
    for (int i = 0; i < WeightsPerVert; i++)
    {
        skinning += PosedBones[indices[i]] * weights[i];
    }

    // Transform position from bind pose to current pose:
    float4 currPosePosition;
    currPosePosition.xyz = mul(float4(position, 1.0f), skinning);
    currPosePosition.w = 1.0f;

    oPosition = mul(currPosePosition, WorldViewProj);
    depth = oPosition.z / oPosition.w;
}


void PixelShaderFunction(float  depth     : TEXCOORD,
                     out float4 oColor    : COLOR)
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
