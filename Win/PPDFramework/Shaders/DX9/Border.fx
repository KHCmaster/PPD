float2 WidthHeight;
float4x4 Projection;
float4 Color;
float Thickness;
int Thickness2;
float ActualThickness;

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

#include "Blend.fx"

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	output.Position = mul(input.Position, Projection);
	output.TexCoord = input.TexCoord;
	output.Color = input.Color;
	return output;
}

float Outside(VertexShaderOutput input) {
	float maxAlpha = 0;
	float start = Thickness / 2;
	for (int i = 0; i < Thickness2; i++) {
		float x = -start + i % Thickness;
		float y = -start + i / Thickness;
		float alpha = 0;
		float distance = sqrt(x * x + y * y);
		if (distance <= ActualThickness) {
			alpha = tex2D(FilterTextureSampler, input.TexCoord + float2(x / WidthHeight.x, y / WidthHeight.y)).a;
		}
		else if (distance <= ActualThickness + 1) {
			alpha = (1 - (distance - ActualThickness))*tex2D(FilterTextureSampler, input.TexCoord + float2(x / WidthHeight.x, y / WidthHeight.y)).a;
		}
		maxAlpha = max(maxAlpha, alpha);
	}
	return maxAlpha;
}

float Inside(VertexShaderOutput input) {
	float maxAlpha = 0;
	float start = Thickness / 2;
	for (int i = 0; i < Thickness2; i++) {
		float x = -start + i % Thickness;
		float y = -start + i / Thickness;
		float alpha = 0;
		float distance = sqrt(x * x + y * y);
		if (distance <= ActualThickness) {
			alpha = 1 - tex2D(FilterTextureSampler, input.TexCoord + float2(x / WidthHeight.x, y / WidthHeight.y)).a;
		}
		else if (distance <= ActualThickness + 1) {
			alpha = (distance - ActualThickness)*(1 - tex2D(FilterTextureSampler, input.TexCoord + float2(x / WidthHeight.x, y / WidthHeight.y)).a);
		}
		maxAlpha = max(maxAlpha, alpha);
	}
	return maxAlpha;
}


float4 PixelShaderNormalOutsideFunction(VertexShaderOutput input) : COLOR0{
	float maxAlpha = Outside(input);
	return Normal(tex2D(LastRenderTargetTextureSampler, input.TexCoord),
		float4(Color.rgb, Color.a * maxAlpha * (1 - tex2D(FilterTextureSampler, input.TexCoord).a)), false);
}

float4 PixelShaderNormalInsideFunction(VertexShaderOutput input) : COLOR0{
	float maxAlpha = Inside(input);
	return Normal(tex2D(LastRenderTargetTextureSampler, input.TexCoord),
		float4(Color.rgb, Color.a * maxAlpha *  tex2D(FilterTextureSampler, input.TexCoord).a), false);
}

technique OutsideNormalBorder {
	pass p0 {
		CullMode = none;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalOutsideFunction();
	}
}

technique InsideNormalBorder {
	pass p0 {
		CullMode = none;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalInsideFunction();
	}
}
float4 PixelShaderMultiplyOutsideFunction(VertexShaderOutput input) : COLOR0{
	float maxAlpha = Outside(input);
	return Multiply(tex2D(LastRenderTargetTextureSampler, input.TexCoord),
		float4(Color.rgb, Color.a * maxAlpha * (1 - tex2D(FilterTextureSampler, input.TexCoord).a)), false);
}

float4 PixelShaderMultiplyInsideFunction(VertexShaderOutput input) : COLOR0{
	float maxAlpha = Inside(input);
	return Multiply(tex2D(LastRenderTargetTextureSampler, input.TexCoord),
		float4(Color.rgb, Color.a * maxAlpha *  tex2D(FilterTextureSampler, input.TexCoord).a), false);
}

technique OutsideMultiplyBorder {
	pass p0 {
		CullMode = none;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyOutsideFunction();
	}
}

technique InsideMultiplyBorder {
	pass p0 {
		CullMode = none;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyInsideFunction();
	}
}
float4 PixelShaderScreenOutsideFunction(VertexShaderOutput input) : COLOR0{
	float maxAlpha = Outside(input);
	return Screen(tex2D(LastRenderTargetTextureSampler, input.TexCoord),
		float4(Color.rgb, Color.a * maxAlpha * (1 - tex2D(FilterTextureSampler, input.TexCoord).a)), false);
}

float4 PixelShaderScreenInsideFunction(VertexShaderOutput input) : COLOR0{
	float maxAlpha = Inside(input);
	return Screen(tex2D(LastRenderTargetTextureSampler, input.TexCoord),
		float4(Color.rgb, Color.a * maxAlpha *  tex2D(FilterTextureSampler, input.TexCoord).a), false);
}

technique OutsideScreenBorder {
	pass p0 {
		CullMode = none;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenOutsideFunction();
	}
}

technique InsideScreenBorder {
	pass p0 {
		CullMode = none;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenInsideFunction();
	}
}
float4 PixelShaderOverlayOutsideFunction(VertexShaderOutput input) : COLOR0{
	float maxAlpha = Outside(input);
	return Overlay(tex2D(LastRenderTargetTextureSampler, input.TexCoord),
		float4(Color.rgb, Color.a * maxAlpha * (1 - tex2D(FilterTextureSampler, input.TexCoord).a)), false);
}

float4 PixelShaderOverlayInsideFunction(VertexShaderOutput input) : COLOR0{
	float maxAlpha = Inside(input);
	return Overlay(tex2D(LastRenderTargetTextureSampler, input.TexCoord),
		float4(Color.rgb, Color.a * maxAlpha *  tex2D(FilterTextureSampler, input.TexCoord).a), false);
}

technique OutsideOverlayBorder {
	pass p0 {
		CullMode = none;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayOutsideFunction();
	}
}

technique InsideOverlayBorder {
	pass p0 {
		CullMode = none;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayInsideFunction();
	}
}
float4 PixelShaderHardLightOutsideFunction(VertexShaderOutput input) : COLOR0{
	float maxAlpha = Outside(input);
	return HardLight(tex2D(LastRenderTargetTextureSampler, input.TexCoord),
		float4(Color.rgb, Color.a * maxAlpha * (1 - tex2D(FilterTextureSampler, input.TexCoord).a)), false);
}

float4 PixelShaderHardLightInsideFunction(VertexShaderOutput input) : COLOR0{
	float maxAlpha = Inside(input);
	return HardLight(tex2D(LastRenderTargetTextureSampler, input.TexCoord),
		float4(Color.rgb, Color.a * maxAlpha *  tex2D(FilterTextureSampler, input.TexCoord).a), false);
}

technique OutsideHardLightBorder {
	pass p0 {
		CullMode = none;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightOutsideFunction();
	}
}

technique InsideHardLightBorder {
	pass p0 {
		CullMode = none;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightInsideFunction();
	}
}
float4 PixelShaderColorDodgeOutsideFunction(VertexShaderOutput input) : COLOR0{
	float maxAlpha = Outside(input);
	return ColorDodge(tex2D(LastRenderTargetTextureSampler, input.TexCoord),
		float4(Color.rgb, Color.a * maxAlpha * (1 - tex2D(FilterTextureSampler, input.TexCoord).a)), false);
}

float4 PixelShaderColorDodgeInsideFunction(VertexShaderOutput input) : COLOR0{
	float maxAlpha = Inside(input);
	return ColorDodge(tex2D(LastRenderTargetTextureSampler, input.TexCoord),
		float4(Color.rgb, Color.a * maxAlpha *  tex2D(FilterTextureSampler, input.TexCoord).a), false);
}

technique OutsideColorDodgeBorder {
	pass p0 {
		CullMode = none;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeOutsideFunction();
	}
}

technique InsideColorDodgeBorder {
	pass p0 {
		CullMode = none;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeInsideFunction();
	}
}
float4 PixelShaderLinearDodgeOutsideFunction(VertexShaderOutput input) : COLOR0{
	float maxAlpha = Outside(input);
	return LinearDodge(tex2D(LastRenderTargetTextureSampler, input.TexCoord),
		float4(Color.rgb, Color.a * maxAlpha * (1 - tex2D(FilterTextureSampler, input.TexCoord).a)), false);
}

float4 PixelShaderLinearDodgeInsideFunction(VertexShaderOutput input) : COLOR0{
	float maxAlpha = Inside(input);
	return LinearDodge(tex2D(LastRenderTargetTextureSampler, input.TexCoord),
		float4(Color.rgb, Color.a * maxAlpha *  tex2D(FilterTextureSampler, input.TexCoord).a), false);
}

technique OutsideLinearDodgeBorder {
	pass p0 {
		CullMode = none;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeOutsideFunction();
	}
}

technique InsideLinearDodgeBorder {
	pass p0 {
		CullMode = none;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeInsideFunction();
	}
}
float4 PixelShaderDifferenceOutsideFunction(VertexShaderOutput input) : COLOR0{
	float maxAlpha = Outside(input);
	return Difference(tex2D(LastRenderTargetTextureSampler, input.TexCoord),
		float4(Color.rgb, Color.a * maxAlpha * (1 - tex2D(FilterTextureSampler, input.TexCoord).a)), false);
}

float4 PixelShaderDifferenceInsideFunction(VertexShaderOutput input) : COLOR0{
	float maxAlpha = Inside(input);
	return Difference(tex2D(LastRenderTargetTextureSampler, input.TexCoord),
		float4(Color.rgb, Color.a * maxAlpha *  tex2D(FilterTextureSampler, input.TexCoord).a), false);
}

technique OutsideDifferenceBorder {
	pass p0 {
		CullMode = none;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceOutsideFunction();
	}
}

technique InsideDifferenceBorder {
	pass p0 {
		CullMode = none;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceInsideFunction();
	}
}