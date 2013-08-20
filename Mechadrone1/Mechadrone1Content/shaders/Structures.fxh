//-----------------------------------------------------------------------------
// Structures.fxh
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


struct DirectionalLight
{
    float4 Ambient;
    float4 Diffuse;
    float4 Specular;
    float3 Direction;
    float Energy;
};


struct PointLight
{ 
    float4 Ambient;
    float4 Diffuse;
    float4 Specular;

    float3 Position;
    float Range;

    float3 Att;
    float pad;
};


struct SpotLight
{
    float4 Ambient;
    float4 Diffuse;
    float4 Specular;

    float3 Position;
    float Range;

    float3 Direction;
    float Spot;

    float3 Att;
    float pad;
};


struct Material
{
    float4 Diffuse;
    float4 Specular; // w = SpecPower
    float4 Reflect;
};
