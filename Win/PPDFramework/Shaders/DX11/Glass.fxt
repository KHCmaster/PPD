float2 WidthHeight;
float4x4 Projection;

Texture2D LastRenderTargetTexture;
Texture2D FilterTexture;
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

float4 PixelShaderFunction(VertexShaderOutput input) : SV_Target{
	float top = XNeighborFilterAlpha(input.TexCoord + float2(0, -1 / WidthHeight.y));
	float bottom = XNeighborFilterAlpha(input.TexCoord + float2(0, 1 / WidthHeight.y));
	float left = YNeighborFilterAlpha(input.TexCoord + float2(-1 / WidthHeight.x, 0));
	float right = YNeighborFilterAlpha(input.TexCoord + float2(1 / WidthHeight.x, 0));
	float filterAlpha = FilterTexture.SampleLevel(LinearSampler, input.TexCoord, 0).a;
	float4 baseColor = LastRenderTargetTexture.SampleLevel(LinearSampler, input.TexCoord +
		float2(CalcDiff(right - left, filterAlpha), CalcDiff(bottom - top, filterAlpha)), 0);
	return baseColor * input.Color;
}

technique11 Glass {
	pass p0 {
		SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_5_0, PixelShaderFunction()));
	}
}