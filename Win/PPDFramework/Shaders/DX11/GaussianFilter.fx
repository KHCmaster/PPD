float2 WidthHeight;
float Weights[32];
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


VertexShaderOutput VertexShaderFunctionPass1(VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	output.Position = mul(float4(input.Position, 1.0), Projection);
	output.TexCoord = input.TexCoord;
	output.Color = input.Color;
	return output;
}

VertexShaderOutput VertexShaderFunctionPass2(VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	output.Position = mul(float4(input.Position, 1), Projection);
	output.TexCoord = input.TexCoord;
	output.Color = input.Color;
	return output;
}

float4 PixelShaderFunctionPass1(VertexShaderOutput input) : SV_Target
{
	float4 result = Weights[0] * Texture.SampleLevel(LinearSampler, input.TexCoord, 0);
	float onePixelWidth = 1 / WidthHeight.x;
	for (int i = 1; i < 32; i++) {
		float2 offset = float2(onePixelWidth * i, 0);
			result += Weights[i] * (Texture.SampleLevel(LinearSampler, input.TexCoord - offset, 0) + Texture.SampleLevel(LinearSampler, input.TexCoord + offset, 0));
	}
    result.a = 1;
	return result * input.Color;
}

float4 PixelShaderFunctionPass2(VertexShaderOutput input) : SV_Target
{
	float4 result = Weights[0] * Texture.SampleLevel(LinearSampler, input.TexCoord, 0);
	float onePixelHeight = 1 / WidthHeight.y;
	for (int i = 1; i < 32; i++) {
		float2 offset = float2(0, onePixelHeight * i);
			result += Weights[i] * (Texture.SampleLevel(LinearSampler, input.TexCoord - offset, 0) + Texture.SampleLevel(LinearSampler, input.TexCoord + offset, 0));
	}
    result.a = 1;
	return result * input.Color;
}

technique11 Gaussian
{
	pass Pass1
	{
		SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunctionPass1()));
		SetPixelShader(CompileShader(ps_5_0, PixelShaderFunctionPass1()));
	}
	pass Pass2
	{
		SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunctionPass2()));
		SetPixelShader(CompileShader(ps_5_0, PixelShaderFunctionPass2()));
	}
}