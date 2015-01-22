//-----------------------------------------------------------------------------
// Common.fxh
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


float3 expandNormalTex(float3 v)
{
    return (v - 0.5f) * 2.0f;
}

float3x3 invert3x3(float3x3 a)
{
    float det = determinant(a);

    float3x3 inv;

    inv._11 = (a._22 * a._33 - a._23 * a._32) / det;
    inv._12 = (a._13 * a._32 - a._12 * a._33) / det;
    inv._13 = (a._12 * a._23 - a._13 * a._22) / det;
    inv._21 = (a._23 * a._31 - a._21 * a._33) / det;
    inv._22 = (a._11 * a._33 - a._13 * a._31) / det;
    inv._23 = (a._13 * a._21 - a._11 * a._23) / det;
    inv._31 = (a._21 * a._32 - a._22 * a._31) / det;
    inv._32 = (a._12 * a._31 - a._11 * a._32) / det;
    inv._33 = (a._11 * a._22 - a._12 * a._21) / det;

    return inv;
}


float CalculateShadowFactor(float4 smapTexCoord, float invShadowMapSize, sampler2D smapSampler)
{
    if (smapTexCoord.z >= 1.0)
        return 1.0f;

    const float2 offset[9] =
    {
        float2(-invShadowMapSize, invShadowMapSize),
        float2(0.0f, invShadowMapSize),
        float2(invShadowMapSize, invShadowMapSize),
        float2(-invShadowMapSize, 0.0f),
        float2(0.0f, 0.0f),
        float2(invShadowMapSize, 0.0f),
        float2(-invShadowMapSize, -invShadowMapSize),
        float2(0.0f, -invShadowMapSize),
        float2(invShadowMapSize, -invShadowMapSize)
    };

    smapTexCoord.xyz /= smapTexCoord.w;
    float percentLit = 0.0f;

    for (int i = 0; i < 9; ++i)
    {
        float smapDepth = tex2D(smapSampler, smapTexCoord.xy + offset[i]);

        if (smapTexCoord.z <= smapDepth)
            percentLit += 1.0f;
    }

    percentLit /= 9.0f;
    return percentLit;
}


//---------------------------------------------------------------------------------------
// Computes the ambient, diffuse, and specular terms in the lighting equation
// from a directional light.  We need to output the terms separately because
// later we may modify the individual terms.
//---------------------------------------------------------------------------------------
void ComputeDirectionalLight(Material mat,
                             DirectionalLight dirLight, 
                             float3 normal,
                             float3 toEye,
                             out float4 ambient,
                             out float4 diffuse,
                             out float4 spec)
{
    // The light vector aims opposite the direction the light rays travel.
    float3 toLight = -dirLight.Direction;

    // Add ambient term.
    ambient = mat.Diffuse * dirLight.Ambient;

    // Initialize outputs.
    diffuse = VECTOR4_ZERO;
    spec    = VECTOR4_ZERO;

    // Add diffuse and specular term, provided the surface is in 
    // the line of sight of the light.
    float diffuseFactor = dot(toLight, normal);

    if( diffuseFactor > 0.0f )
    {
        float3 specRay = reflect(-toLight, normal);
        float alignment = max(dot(specRay, toEye), 0.0f);
        float specFactor = alignment > 0.0f ? pow(alignment, mat.Specular.w) : 0.0f;

        diffuse = diffuseFactor * mat.Diffuse * dirLight.Diffuse * dirLight.Energy;
        spec = specFactor * mat.Specular * dirLight.Specular * dirLight.Energy;
    }
}


//---------------------------------------------------------------------------------------
// Computes the ambient, diffuse, and specular terms in the lighting equation
// from a directional light.  We need to output the terms separately because
// later we may modify the individual terms.
//---------------------------------------------------------------------------------------
void ComputeDirectionalLightBlinn(Material mat,
                                  DirectionalLight dirLight, 
                                  float3 normal,
                                  float3 toEye,
                                  out float4 ambient,
                                  out float4 diffuse,
                                  out float4 spec)
{
    // The light vector aims opposite the direction the light rays travel.
    float3 toLight = -dirLight.Direction;

    // Add ambient term.
    ambient = mat.Diffuse * dirLight.Ambient;

    // Initialize outputs.
    diffuse = VECTOR4_ZERO;
    spec    = VECTOR4_ZERO;

    // Add diffuse and specular term, provided the surface is in 
    // the line of sight of the light.
    float diffuseFactor = dot(toLight, normal);

    if( diffuseFactor > 0.0f )
    {
        float3 halfway = normalize(toLight + toEye);
        float alignment = max(dot(halfway, normal), 0.0f);
        float specFactor = alignment > 0.0f ? pow(alignment, 5.0f) : 0.0f;

        diffuse = diffuseFactor * mat.Diffuse * dirLight.Diffuse * dirLight.Energy;
        spec = specFactor * mat.Specular * dirLight.Specular * dirLight.Energy;
    }
}
