//-----------------------------------------------------------------------------
// Common.fxh
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


float3 expandNormalTex(float3 v)
{
    return (v - 0.5) * 2;
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
    diffuse = float4(0.0f, 0.0f, 0.0f, 0.0f);
    spec    = float4(0.0f, 0.0f, 0.0f, 0.0f);

    // Add diffuse and specular term, provided the surface is in 
    // the line of sight of the light.
    float diffuseFactor = dot(toLight, normal);

    // Flatten to avoid dynamic branching.
    [flatten]
    if( diffuseFactor > 0.0f )
    {
        float3 specRay = reflect(-toLight, normal);
        float alignment = max(dot(specRay, toEye), 0.0f);
        float specFactor = alignment > 0.0f ? pow(alignment, mat.Specular.w) : 0.0f;

        diffuse = diffuseFactor * mat.Diffuse * dirLight.Diffuse * dirLight.Energy;
        spec = specFactor * mat.Specular * dirLight.Specular * dirLight.Energy;
    }
}


//float ComputeFogFactor(float4 position)
//{
//    return saturate(dot(position, FogVector));
//}


void ApplyFog(inout float4 color, float fogFactor)
{
    color.rgb = lerp(color.rgb, FogColor * color.a, fogFactor);
}


void AddSpecular(inout float4 color, float3 specular)
{
    color.rgb += specular * color.a;
}


struct CommonVSOutput
{
    float4 Pos_ps;
    float4 Diffuse;
    float3 Specular;
    float  FogFactor;
};

/*
CommonVSOutput ComputeCommonVSOutput(float4 position)
{
    CommonVSOutput vout;
    
    vout.Pos_ps = mul(position, WorldViewProj);
    vout.Diffuse = DiffuseColor;
    vout.Specular = 0;
    vout.FogFactor = ComputeFogFactor(position);
    
    return vout;
}
*/

#define SetCommonVSOutputParams \
    vout.PositionPS = cout.Pos_ps; \
    vout.Diffuse = cout.Diffuse; \
    vout.Specular = float4(cout.Specular, cout.FogFactor);


#define SetCommonVSOutputParamsNoFog \
    vout.PositionPS = cout.Pos_ps; \
    vout.Diffuse = cout.Diffuse;

