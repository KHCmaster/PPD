#include "Common.fx"

cbuffer Parameters : register(b0)
{
    float2 WidthHeight;
    float4x4 Projection;
};

Texture2D LastRenderTargetTexture : register(t0);
Texture2D FilterTexture : register(t1);

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

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	output.Position = mul(input.Position, Projection);
	output.TexCoord = input.TexCoord;
	output.Color = input.Color;
	return output;
}

float XNeighborFilterAlpha(float2 coord) {
	return (FilterTexture.SampleLevel(LinearSampler, coord - float2(1 / WidthHeight.x, 0), 0).a +
		FilterTexture.SampleLevel(LinearSampler, coord, 0).a +
		FilterTexture.SampleLevel(LinearSampler, coord + float2(1 / WidthHeight.x, 0), 0).a) / 3;
}

float YNeighborFilterAlpha(float2 coord) {
	return (FilterTexture.SampleLevel(LinearSampler, coord - float2(0, 1 / WidthHeight.y), 0).a +
		FilterTexture.SampleLevel(LinearSampler, coord, 0).a +
		FilterTexture.SampleLevel(LinearSampler, coord + float2(0, 1 / WidthHeight.y), 0).a) / 3;
}

float CalcDiff(float diff, float alpha) {
	float theta = atan(diff / 2);
	float theta2 = asin(sin(theta) / 2.2);
	if (theta - theta2 == 0) {
		return 0;
	}
	float tanTheta = tan(theta);
	float tanTheta2 = tan(theta2);
	return alpha / ((1 + tanTheta * tanTheta2) / (tanTheta - tanTheta2));
}

float4 PixelShaderFunction(VertexShaderOutput input) : SV_TARGET{
	float top = XNeighborFilterAlpha(input.TexCoord + float2(0, -1 / WidthHeight.y));
	float bottom = XNeighborFilterAlpha(input.TexCoord + float2(0, 1 / WidthHeight.y));
	float left = YNeighborFilterAlpha(input.TexCoord + float2(-1 / WidthHeight.x, 0));
	float right = YNeighborFilterAlpha(input.TexCoord + float2(1 / WidthHeight.x, 0));
	float filterAlpha = FilterTexture.SampleLevel(LinearSampler, input.TexCoord, 0).x;
	float4 baseColor = LastRenderTargetTexture.SampleLevel(LinearSampler, input.TexCoord +
		float2(CalcDiff(right - left, filterAlpha), CalcDiff(bottom - top, filterAlpha)), 0);
	return baseColor * input.Color;
}

technique11 Glass {
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderFunction()));
	}
}