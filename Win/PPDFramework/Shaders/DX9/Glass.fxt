float2 WidthHeight;
float4x4 Projection;

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

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	output.Position = mul(input.Position, Projection);
	output.TexCoord = input.TexCoord;
	output.Color = input.Color;
	return output;
}

float XNeighborFilterAlpha(float2 coord) {
	return (tex2D(FilterTextureSampler, coord - float2(1 / WidthHeight.x, 0)).a +
		tex2D(FilterTextureSampler, coord).a +
		tex2D(FilterTextureSampler, coord + float2(1 / WidthHeight.x, 0)).a) / 3;
}

float YNeighborFilterAlpha(float2 coord) {
	return (tex2D(FilterTextureSampler, coord - float2(0, 1 / WidthHeight.y)).a +
		tex2D(FilterTextureSampler, coord).a +
		tex2D(FilterTextureSampler, coord + float2(0, 1 / WidthHeight.y)).a) / 3;
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

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0{
	float top = XNeighborFilterAlpha(input.TexCoord + float2(0, -1 / WidthHeight.y));
	float bottom = XNeighborFilterAlpha(input.TexCoord + float2(0, 1 / WidthHeight.y));
	float left = YNeighborFilterAlpha(input.TexCoord + float2(-1 / WidthHeight.x, 0));
	float right = YNeighborFilterAlpha(input.TexCoord + float2(1 / WidthHeight.x, 0));
	float filterAlpha = tex2D(FilterTextureSampler, input.TexCoord);
	float4 baseColor = tex2D(LastRenderTargetTextureSampler, input.TexCoord +
		float2(CalcDiff(right - left, filterAlpha), CalcDiff(bottom - top, filterAlpha)));
	return baseColor * input.Color;
}

technique Glass {
	pass p0 {
		CullMode = none;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}