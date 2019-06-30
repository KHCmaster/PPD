float2 WidthHeight;
float4x4 Projection;
int Size;

texture LastRenderTargetTexture;
sampler LastRenderTargetTextureSampler = sampler_state
{
	Texture = < LastRenderTargetTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = None;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture XTexture;
sampler XTextureSampler = sampler_state
{
	Texture = < XTexture >;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = None;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture FilterTexture;
sampler FilterTextureSampler = sampler_state
{
	Texture = < FilterTexture >;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = None;
	AddressU = Clamp;
	AddressV = Clamp;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Color    : COLOR0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
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

float4 PixelShaderMosaicXFunction(VertexShaderOutput input) : COLOR0{
	int start = input.TexCoord.x * WidthHeight.x / Size;
    start *= Size;
    float4 sum = 0;
    float4 count = 0;
	for (int i = 0; i < Size; i++) {
        float2 tex = float2((start + i) / WidthHeight.x, input.TexCoord.y);
        float alpha = tex2D(FilterTextureSampler, tex).a;
        sum += tex2D(LastRenderTargetTextureSampler, tex) * alpha;
        count += alpha;
	}
    return sum / count;
}

float4 PixelShaderMosaicYFunction(VertexShaderOutput input) : COLOR0{
	int start = input.TexCoord.y * WidthHeight.y / Size;
    start *= Size;
    float4 sum = 0;
    float4 count = 0;
	for (int i = 0; i < Size; i++) {
        float2 tex = float2(input.TexCoord.x, (start + i) / WidthHeight.y);
        float alpha = tex2D(FilterTextureSampler, tex).a;
        sum += tex2D(XTextureSampler, tex) * alpha;
        count += alpha;
	}
    float4 color = sum / count;
    color.a *= tex2D(FilterTextureSampler, input.TexCoord).a;
    return Normal(tex2D(LastRenderTargetTextureSampler, input.TexCoord), color, false);
}

technique Mosaic {
	pass p0 {
		CullMode = none;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMosaicXFunction();
	}

	pass p1 {
		CullMode = none;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMosaicYFunction();
	}
}