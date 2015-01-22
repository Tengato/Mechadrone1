#include "Constants.fxh"

float4x4 gWorldViewProj;
int      gWeightsPerVert;
float4x3 gPosedBones[SKINNED_EFFECT_MAX_BONES];

void VertexShaderFunction(float3 position  : POSITION,
                          int4   indices   : BLENDINDICES,
                          float4 weights   : BLENDWEIGHT,
                      out float  oDepth     : TEXCOORD,
                      out float4 oPosition : POSITION)
{
    float4x3 skinning = {0.0f, 0.0f, 0.0f,
                         0.0f, 0.0f, 0.0f,
                         0.0f, 0.0f, 0.0f,
                         0.0f, 0.0f, 0.0f};

    for (int i = 0; i < gWeightsPerVert; i++)
    {
        skinning += gPosedBones[indices[i]] * weights[i];
    }

    // Transform position from bind pose to current pose:
    float4 currPosePosition;
    currPosePosition.xyz = mul(float4(position, 1.0f), skinning);
    currPosePosition.w = 1.0f;

    oPosition = mul(currPosePosition, gWorldViewProj);
    oDepth = oPosition.z / oPosition.w;
}


void PixelShaderFunction(float  depth     : TEXCOORD,
                     out float4 oColor    : COLOR)
{
    oColor = float4(depth + SHADOWMAP_BIAS, 0.0f, 0.0f, 0.0f);
}


technique DepthOnlySkin
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
