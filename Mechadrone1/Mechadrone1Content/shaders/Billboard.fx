#include "Constants.fxh"

float3   gEyePosition;
float4x4 gWorld;
float4x4 gWorldViewProj;
float4x4 gWorldInvTranspose;
float    gFogStart;
float    gFogEnd;
float3   gFogColor;
float    gBright = 1.0f;
float    gContrast = 1.0f;

float3 WindDirection = float3(1, 0, 0);
float WindWaveSize = 0.1;
float WindRandomness = 1;
float WindSpeed = 4;
float WindAmount;
float CurrentTime;

texture2D gIrradianceMap;
sampler2D gIrradianceMapSampler = sampler_state
{
    Texture = <gIrradianceMap>;
    MinFilter = POINT;
    MagFilter = POINT;
    MipFilter = POINT;
    AddressU = WRAP;
    AddressV = CLAMP;
};

// 1 means we should only accept opaque pixels.
// -1 means only accept transparent pixels.
float AlphaTestDirection = 1;
float AlphaTestThreshold = 0.95;


// Parameters describing the billboard itself.
float BillboardWidth;
float BillboardHeight;

texture Texture;
sampler TextureSampler = sampler_state
{
    Texture = (Texture);
};

struct VS_INPUT
{
    float3 Position : POSITION0;
    float3 Normal   : NORMAL0;
    float2 TexCoord : TEXCOORD0;
    float  Random   : TEXCOORD1;
};

struct VS_OUTPUT
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float3 ToEye    : TEXCOORD1;
    float4 Color    : COLOR0;
};

void GetOrientation(float3 toEye,
                out float3 up,
                out float3 forward,
                out float3 right)
{
    // Work out what direction we are viewing the billboard from.
    up = mul(float3(0.0f, 1.0f, 0.0f), (float3x3)gWorldInvTranspose);

    right = cross(toEye, up);
    if (any(right))   // if any components are nonzero...
    {
        right = normalize(right);
    }
    else
    {
        right.x = 1.0f;
    }

    forward = cross(up, right);
}

void GetWorldPosition(VS_INPUT input,
                      float3   right,
                      float3   up,
                  out float3   wPosition)
{
    // Apply a scaling factor to make some of the billboards
    // shorter and fatter while others are taller and thinner.
    float squishFactor = 0.75 + abs(input.Random) / 2.0f;

    float width = BillboardWidth * squishFactor;
    float height = BillboardHeight / squishFactor;

    wPosition = (mul(float4(input.Position, 1.0f), gWorld)).xyz;// float4(0.0f, 0.0f, 0.0f, 1.0f); 

    // Calculate the position of this billboard vertex.

    // Offset to the left or right.
    wPosition += right * (0.5f - input.TexCoord.x) * width;

    // Offset upward if we are one of the top two vertices.
    wPosition += up * (1.0f - input.TexCoord.y) * height;

    // Work out how this vertex should be affected by the wind effect.
    float waveOffset = dot(wPosition, WindDirection) * WindWaveSize;

    waveOffset += input.Random * WindRandomness;

    // Wind makes things wave back and forth in a sine wave pattern.
    float wind = sin(CurrentTime * WindSpeed + waveOffset) * WindAmount;

    // But it should only affect the top two vertices of the billboard!
    wind *= (1 - input.TexCoord.y);

    wPosition += WindDirection * wind;
}

VS_OUTPUT VertexShaderFunction(VS_INPUT input)
{
    VS_OUTPUT output;

    float3 billboardCenter = mul(float4(input.Position, 1.0f), gWorld).xyz;
    output.ToEye = gEyePosition - billboardCenter;

    float3 wUp;
    float3 wForward;
    float3 wRight;
    GetOrientation(output.ToEye, wUp, wForward, wRight);

    float3 wPosition;
    GetWorldPosition(input, wRight, wUp, wPosition);
    output.Position.xyz = wPosition;
    output.Position.w = 1.0f;

    // Apply the camera transform.
    output.Position = mul(output.Position, mul(transpose(gWorldInvTranspose), gWorldViewProj));

    // Flip half of the billboards from left to right. This gives visual variety
    // even though we are actually just repeating the same texture over and over.
    if (input.Random < 0)
        output.TexCoord.x = 1.0f - input.TexCoord.x;

    output.TexCoord.y = input.TexCoord.y;

    // Compute lighting.
    // Convert forward vector into spherical texture coordinates:
    wForward.y = acos(wForward.y) / PI;
    wForward.x = -atan2(wForward.x, wForward.z) / 2.0f / PI;
    wForward.z = 0.0f;     // not used

    output.Color.rgb = tex2Dlod(gIrradianceMapSampler, float4(wForward, 0.0f)).rgb;    // w = mipmap level; irradiance map only has one level
    output.Color.a = 1.0f;

    return output;
}

float4 PixelShaderFunction(float2 texCoord : TEXCOORD0,
                           float3 toEye    : TEXCOORD1,
                           float4 color    : COLOR0) : COLOR0
{
    color *= tex2D(TextureSampler, texCoord);

    // Apply the alpha test.
    clip((color.a - AlphaTestThreshold) * AlphaTestDirection);

    // Tone map:
    color.rgb = gBright * pow(color.rgb, gContrast);

    // Linear space to sRGB
    color.rgb = pow(color.rgb, 1.0f / 2.2f);

    float fogFactor = 1.0f - exp2(1.0f - (gFogEnd - gFogStart) / max(gFogEnd - max(length(toEye), gFogStart), 0.000001f));
    color.rgb = lerp(color.rgb, gFogColor, fogFactor);

    return color;
}

void DepthOnlyVertexShaderFunction(VS_INPUT input,
                               out float2   uv        : TEXCOORD0,
                               out float    depth     : TEXCOORD1,
                               out float4   oPosition : POSITION)
{
    float3 billboardCenter = mul(float4(input.Position, 1.0f), gWorld).xyz;
    float3 toEye = gEyePosition - billboardCenter;

    float3 wUp;
    float3 wForward;
    float3 wRight;
    GetOrientation(toEye, wUp, wForward, wRight);

    float3 wPosition;
    GetWorldPosition(input, wRight, wUp, wPosition);

    // Apply the camera transform.
    oPosition = mul(float4(wPosition, 1.0f), mul(transpose(gWorldInvTranspose), gWorldViewProj));

    depth = oPosition.z / oPosition.w;
    uv = input.TexCoord;
}

void DepthOnlyPixelShaderFunction(float2 uv     : TEXCOORD0,
                                  float  depth  : TEXCOORD1,
                              out float4 oColor : COLOR)
{
    float4 texValue = tex2D(TextureSampler, uv);

    // Apply the alpha test.
    clip((texValue.a - AlphaTestThreshold) * AlphaTestDirection);

    oColor = float4(depth + SHADOWMAP_BIAS, 0.0f, 0.0f, 0.0f);
}


technique Billboard
{
    pass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}

technique DepthOnlyBillboard
{
    pass
    {
        VertexShader = compile vs_3_0 DepthOnlyVertexShaderFunction();
        PixelShader = compile ps_3_0 DepthOnlyPixelShaderFunction();
    }
}
