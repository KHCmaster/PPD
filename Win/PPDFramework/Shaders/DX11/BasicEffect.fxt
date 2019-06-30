float4 OverlayColor;
float4x4 Projection;

Texture2D Texture;
SamplerState LinearSampler
{
    Filter = MIN_MAG_MIP_LINEAR;
	AddressU = Clamp;
	AddressV = Clamp;
};

struct VertexShaderInput
{
	float3 Position : POSITION;
	float4 Color    : COLOR;
	float2 TexCoord : TEXCOORD;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color    : COLOR;
	float2 TexCoord : TEXCOORD;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
    output.Position = mul(float4(input.Position, 1.0), Projection);
	output.TexCoord = input.TexCoord;
	output.Color = input.Color;
	return output;
}

float4 GetSrcColor(float2 texCoord) {
	float4 srcColor = Texture.SampleLevel(LinearSampler, texCoord, 0);
    srcColor.rgb = srcColor.rgb * (1 - OverlayColor.a) + OverlayColor.rgb * OverlayColor.a;
	return srcColor;
}

float4 PixelShaderFunction(VertexShaderOutput input) : SV_Target{
    return GetSrcColor(input.TexCoord) * input.Color;
}

technique BasicEffect{
	pass p0 {
		SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_5_0, PixelShaderFunction()));
	}
}