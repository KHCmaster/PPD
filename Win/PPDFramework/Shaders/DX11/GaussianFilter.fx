#include "Common.fx"

cbuffer Parameters : register(b0)
{
    float2 WidthHeight;
    float Weights[32];
    float4x4 Projection;
};

Texture2D Texture : register(t0);

struct VertexShaderInput
{
	float4 Position : POSITION;
	float4 Color    : COLOR;
	float2 TexCoord : TEXCOORD;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color    : COLOR0;
	float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunctionPass1(VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	output.Position = mul(input.Position, Projection);
	output.TexCoord = input.TexCoord;
	output.Color = input.Color;
	return output;
}

VertexShaderOutput VertexShaderFunctionPass2(VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	output.Position = mul(input.Position, Projection);
	output.TexCoord = input.TexCoord;
	output.Color = input.Color;
	return output;
}

float4 PixelShaderFunctionPass1(VertexShaderOutput input) : SV_TARGET
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

float4 PixelShaderFunctionPass2(VertexShaderOutput input) : SV_TARGET
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
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunctionPass1()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderFunctionPass1()));
	}
	pass Pass2
	{
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunctionPass2()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderFunctionPass2()));
	}
}