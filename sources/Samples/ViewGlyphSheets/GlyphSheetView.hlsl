struct psInput
{
    float4 Position : SV_Position;
    float2 TexCoord : TEXCOORD0;
};

SamplerState linearSampler : register(s0);
Texture2D<float> inputTexture : register(t0);

psInput VS(uint vertexID : SV_VertexID)
{
    psInput result;
    result.TexCoord = float2((vertexID << 1) & 2, vertexID & 2);
    result.Position = float4(result.TexCoord * float2(2.0f, -2.0f) + float2(-1.0f, 1.0f), 0.0f, 1.0f);
    return result;
}

float4 PS(psInput Input) : SV_Target
{
    float color = inputTexture.Sample(linearSampler, Input.TexCoord);
    return color;
}

