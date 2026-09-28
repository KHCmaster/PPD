#include "Common.fx"

float2 WidthHeight;
float4x4 Projection;
int Size;

Texture2D LastRenderTargetTexture;
Texture2D XTexture;
Texture2D FilterTexture;

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

#include "Blend.fx"

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	output.Position = mul(input.Position, Projection);
	output.TexCoord = input.TexCoord;
	output.Color = input.Color;
	return output;
}

float4 PixelShaderMosaicXFunction(VertexShaderOutput input) : SV_TARGET{
	int start = input.TexCoord.x * WidthHeight.x / Size;
    start *= Size;
    float4 sum = 0;
    float4 count = 0;
	for (int i = 0; i < Size; i++) {
        float2 tex = float2((start + i) / WidthHeight.x, input.TexCoord.y);
        float alpha = FilterTexture.SampleLevel(LinearSampler, tex, 0).a;
        sum += LastRenderTargetTexture.SampleLevel(LinearSampler, tex, 0) * alpha;
        count += alpha;
	}
    return sum / count;
}

float4 PixelShaderMosaicYFunction(VertexShaderOutput input) : SV_TARGET{
	int start = input.TexCoord.y * WidthHeight.y / Size;
    start *= Size;
    float4 sum = 0;
    float4 count = 0;
	for (int i = 0; i < Size; i++) {
        float2 tex = float2(input.TexCoord.x, (start + i) / WidthHeight.y);
        float alpha = FilterTexture.SampleLevel(LinearSampler, tex, 0).a;
        sum += XTexture.SampleLevel(LinearSampler, tex, 0) * alpha;
        count += alpha;
	}
    float4 color = sum / count;
    color.a *= FilterTexture.SampleLevel(LinearSampler, input.TexCoord, 0).a;
    return Normal(LastRenderTargetTexture.SampleLevel(LinearSampler, input.TexCoord, 0), color);
}

technique11 Mosaic {
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMosaicXFunction()));
	}

	pass p1 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMosaicYFunction()));
	}
}