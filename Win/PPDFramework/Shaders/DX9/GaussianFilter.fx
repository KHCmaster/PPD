float2 WidthHeight;
float Weights[32];
float4x4 Projection;

texture Texture;
sampler TextureSampler = sampler_state
{
	Texture = < Texture >;
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

float4 PixelShaderFunctionPass1(VertexShaderOutput input) : COLOR0
{
	float4 result = Weights[0] * tex2D(TextureSampler, input.TexCoord);
	float onePixelWidth = 1 / WidthHeight.x;
	for (int i = 1; i < 32; i++) {
		float2 offset = float2(onePixelWidth * i, 0);
			result += Weights[i] * (tex2D(TextureSampler, input.TexCoord - offset) + tex2D(TextureSampler, input.TexCoord + offset));
	}
    result.a = 1;
	return result * input.Color;
}

float4 PixelShaderFunctionPass2(VertexShaderOutput input) : COLOR0
{
	float4 result = Weights[0] * tex2D(TextureSampler, input.TexCoord);
	float onePixelHeight = 1 / WidthHeight.y;
	for (int i = 1; i < 32; i++) {
		float2 offset = float2(0, onePixelHeight * i);
			result += Weights[i] * (tex2D(TextureSampler, input.TexCoord - offset) + tex2D(TextureSampler, input.TexCoord + offset));
	}
    result.a = 1;
	return result * input.Color;
}

technique Gaussian
{
	pass Pass1
	{
		CullMode = none;
		VertexShader = compile vs_3_0 VertexShaderFunctionPass1();
		PixelShader = compile ps_3_0 PixelShaderFunctionPass1();
	}
	pass Pass2
	{
		CullMode = none;
		VertexShader = compile vs_3_0 VertexShaderFunctionPass2();
		PixelShader = compile ps_3_0 PixelShaderFunctionPass2();
	}
}