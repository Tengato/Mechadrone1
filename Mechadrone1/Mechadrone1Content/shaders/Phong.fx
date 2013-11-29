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


void VertexProc(float3   position        : POSITION,
                float3   normal          : NORMAL,
                float2   texCoord        : TEXCOORD0,
            out float4   oPosition       : POSITION,
            out float2   oTexCoord       : TEXCOORD0,
            out float3   eyeDisplacement : TEXCOORD1,
            out float3   oNormal         : TEXCOORD2)
{
    oTexCoord = texCoord;

    // Transform position from object space to world space:
    float4 wPosition = mul(float4(position, 1.0f), World);
    eyeDisplacement = EyePosition - wPosition.xyz;

    // Transform position from object space to clip space:
    oPosition = mul(float4(position, 1.0f), WorldViewProj);
    oNormal = mul(normal, (float3x3)WorldInvTranspose);
}


void PixelProc(float2   texCoord        : TEXCOORD0,
               float3   eyeDisplacement : TEXCOORD1,
               float3   normal          : TEXCOORD2,
           out float4   oColor          : COLOR)
{
    Material surfaceMat;
    surfaceMat.Specular = MatSpecColor;
    surfaceMat.Diffuse = tex2D(DiffuseTextureSampler, texCoord);

    float eyeDistance = length(eyeDisplacement);

    normal = normalize(normal);

    // Start with a sum of zero.
    oColor = VECTOR4_ZERO;

    // Sum the light contribution from each light source.
    float4 ambientPiece, diffusePiece, specularPiece;

    for (int i = 0; i < NumLights; i++)
    {
        ComputeDirectionalLight(surfaceMat, DirLights[i], normal, eyeDisplacement / eyeDistance, 
            ambientPiece, diffusePiece, specularPiece);

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
