//Those are provided by the runtime directly
cbuffer ShaderConstants : register(b0) 
{
    float4 TextColor : packoffset(c4);
};

SamplerState sampler0 : register(s0);
Texture2D<float> tex0 : register(t0);

//Our own custom cbuffer
cbuffer ShaderTimeParams : register(b1)
{
    float time;
    float3 padding;
};




struct PSIn 
{
    float4 Position : SV_Position;
    float4 GlyphColor : COLOR;
    float2 TexCoord : TEXCOORD;
};


float4 PS(PSIn Input) : SV_Target 
{
    float a = tex0.Sample(sampler0, Input.TexCoord);

    if(a == 0.0f)
        discard;

    float4 col = sin(Input.TexCoord.x * 50.0f + time);
    return a * col;
}

