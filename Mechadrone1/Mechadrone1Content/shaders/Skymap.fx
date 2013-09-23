float4x4 worldViewProj;
textureCUBE skybox;

sampler envMap = sampler_state {
    Texture = <skybox>;
};

void VertexShaderFunction(float3 position  : POSITION,
                       out float4 oPosition : POSITION,
                       out float3 mapLookup : TEXCOORD)
{
    oPosition = mul(float4(position, 1.0f), worldViewProj).xyww;
    mapLookup = position;
}

float4 PixelShaderFunction(float3 mapLookup : TEXCOORD) : COLOR
{
    return texCUBE(envMap, mapLookup);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
