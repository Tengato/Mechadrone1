float4x4    gWorldViewProj;

textureCUBE gEnvironmentMap;
samplerCUBE gEnvironmentMapSampler = sampler_state
{
    Texture = <gEnvironmentMap>;
};

void VertexShaderFunction(float3 position   : POSITION,
                      out float4 oPosition  : POSITION,
                      out float3 oMapLookup : TEXCOORD)
{
    oPosition = mul(float4(position, 1.0f), gWorldViewProj).xyww;
    oMapLookup = position;
}

float4 PixelShaderFunction(float3 mapLookup : TEXCOORD) : COLOR
{
    return texCUBE(gEnvironmentMapSampler, mapLookup);
}

technique Skymap
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
