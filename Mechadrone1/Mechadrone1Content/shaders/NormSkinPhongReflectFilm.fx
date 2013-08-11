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

float4x3 PosedBones[SKINNED_EFFECT_MAX_BONES];
int WeightsPerVert;

float MatSpecColor;
float FilmDepth;


#include "Common.fxh"


texture2D NormalMap;
sampler2D NormalMapSampler = sampler_state
{
    Texture = <NormalMap>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
};

texture2D Texture;
sampler2D DiffuseTextureSampler = sampler_state
{
    Texture = <Texture>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
};

textureCUBE EnviroMap;
sampler EnviroMapSampler = sampler_state {
    Texture = <EnviroMap>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
};

texture2D FringeMap;
sampler2D FringeMapSampler = sampler_state
{
    Texture = <FringeMap>;
    MinFilter = POINT;
    MagFilter = POINT;
    MipFilter = POINT;
};


void VertexProc(float3   position        : POSITION,
                float3   normal          : NORMAL,
                float3   tangent         : TANGENT,
                float3   binormal        : BINORMAL,
                float2   texCoord        : TEXCOORD0,
                int4     indices         : BLENDINDICES,
                float4   weights         : BLENDWEIGHT,
            out float4   oPosition       : POSITION,
            out float2   oTexCoord       : TEXCOORD0,
            out float3   eyeDisplacement : TEXCOORD1,
            out float3x3 tangentToWorld  : TEXCOORD2)    // Includes TEXCOORD3 and TEXCOORD4
{
    oTexCoord = texCoord;

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

    // Transform position from current pose (object space) to world space:
    float4 wPosition = mul(currPosePosition, World);
    eyeDisplacement = EyePosition - wPosition.xyz;

    // Transform position from current pose (object space) to clip space:
    oPosition = mul(currPosePosition, WorldViewProj);

    // Transform vertex vectors from bind pose to current pose:
    float3x3 skinningInvTranspose = transpose(invert3x3((float3x3)skinning));
    float3 currPoseNormal = mul(normal, skinningInvTranspose);
    float3 currPoseTangent = mul(tangent, skinningInvTranspose);
    float3 currPoseBinormal = mul(binormal, skinningInvTranspose);

    // Construct tangent-space-to-world-space 3x3 matrix
    tangentToWorld[0] = mul(currPoseTangent, (float3x3)WorldInvTranspose);
    tangentToWorld[1] = mul(currPoseBinormal, (float3x3)WorldInvTranspose);
    tangentToWorld[2] = mul(currPoseNormal, (float3x3)WorldInvTranspose);
}


void PixelProc(float2   texCoord        : TEXCOORD0,
               float3   eyeDisplacement : TEXCOORD1,
               float3x3 tangentToWorld  : TEXCOORD2,
           out float4   oColor          : COLOR)
{
    tangentToWorld[0] = normalize(tangentToWorld[0]);
    tangentToWorld[1] = normalize(tangentToWorld[1]);
    tangentToWorld[2] = normalize(tangentToWorld[2]);

    // Fetch and expand range-compressed normal
    float3 normalTex = tex2D(NormalMapSampler, texCoord).xyz;
    float3 normalTanSpc = expandNormalTex(normalTex);
    float3 normal = mul(normalTanSpc, tangentToWorld);

    Material surfaceMat;
    surfaceMat.Specular = MatSpecColor;
    surfaceMat.Diffuse = tex2D(DiffuseTextureSampler, texCoord);

    float eyeDistance = length(eyeDisplacement);
    float toEye = eyeDisplacement / eyeDistance;

    // Start with a sum of zero.
    oColor = float4(0.0f, 0.0f, 0.0f, 0.0f);

    // Sum the light contribution from each light source.
    float4 ambientPiece, diffusePiece, specularPiece, fringeColor;
    float filmDepthFromEye;

    [unroll]
    for (int i = 0; i < NumLights; i++)
    {
        ComputeDirectionalLight(surfaceMat, DirLights[i], normal, eyeDisplacement / eyeDistance, 
            ambientPiece, diffusePiece, specularPiece);

        filmDepthFromEye = FilmDepth / max(dot(normal, toEye), 0.000001f);
        fringeColor = tex2D(FringeMapSampler, float2(filmDepthFromEye, 0.5f));

        oColor += ambientPiece + diffusePiece + specularPiece * fringeColor;
    }

    float3 reflectRay = reflect(-eyeDisplacement, normal);

    // Fetch reflected environment color
    float4 reflectedColor = texCUBE(EnviroMapSampler, reflectRay);

    oColor = lerp(oColor, reflectedColor, surfaceMat.Specular);

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
