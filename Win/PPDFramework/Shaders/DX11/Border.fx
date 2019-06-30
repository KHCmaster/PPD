float2 WidthHeight;
float4x4 Projection;
float4 Color;
float Thickness;
int Thickness2;
float ActualThickness;

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

#include "Blend.fx"

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	output.Position = mul(float4(input.Position, 1.0), Projection);
	output.TexCoord = input.TexCoord;
	output.Color = input.Color;
	return output;
}

float Outside(VertexShaderOutput input) {
	float maxAlpha = 0;
	float start = Thickness / 2.0;
	for (int i = 0; i < Thickness2; i++) {
		float x = -start + i % Thickness;
		float y = -start + i / Thickness;
		float alpha = 0;
		float distance = sqrt(x * x + y * y);
		if (distance <= ActualThickness) {
			alpha = FilterTexture.SampleLevel(LinearSampler, input.TexCoord + float2(x / WidthHeight.x, y / WidthHeight.y), 0).a;
		}
		else if (distance <= ActualThickness + 1) {
			alpha = (1 - (distance - ActualThickness))*FilterTexture.SampleLevel(LinearSampler, input.TexCoord + float2(x / WidthHeight.x, y / WidthHeight.y), 0).a;
		}
		maxAlpha = max(maxAlpha, alpha);
	}
	return maxAlpha;
}

float Inside(VertexShaderOutput input) {
	float maxAlpha = 0;
	float start = Thickness / 2.0;
	for (int i = 0; i < Thickness2; i++) {
		float x = -start + i % Thickness;
		float y = -start + i / Thickness;
		float alpha = 0;
		float distance = sqrt(x * x + y * y);
		if (distance <= ActualThickness) {
			alpha = 1 - FilterTexture.SampleLevel(LinearSampler, input.TexCoord + float2(x / WidthHeight.x, y / WidthHeight.y), 0).a;
		}
		else if (distance <= ActualThickness + 1) {
			alpha = (distance - ActualThickness)*(1 - FilterTexture.SampleLevel(LinearSampler, input.TexCoord + float2(x / WidthHeight.x, y / WidthHeight.y), 0).a);
		}
		maxAlpha = max(maxAlpha, alpha);
	}
	return maxAlpha;
}


float4 PixelShaderNormalOutsideFunction(VertexShaderOutput input) : SV_Target{
	float maxAlpha = Outside(input);
	return Normal(LastRenderTargetTexture.SampleLevel(LinearSampler, input.TexCoord, 0),
		float4(Color.rgb, Color.a * maxAlpha * (1 - FilterTexture.SampleLevel(LinearSampler, input.TexCoord, 0).a)));
}

float4 PixelShaderNormalInsideFunction(VertexShaderOutput input) : SV_Target{
	float maxAlpha = Inside(input);
	return Normal(LastRenderTargetTexture.SampleLevel(LinearSampler, input.TexCoord, 0),
		float4(Color.rgb, Color.a * maxAlpha *  FilterTexture.SampleLevel(LinearSampler, input.TexCoord, 0).a));
}

technique11 OutsideNormalBorder {
	pass p0 {
		SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_5_0, PixelShaderNormalOutsideFunction()));
	}
}

technique11 InsideNormalBorder {
	pass p0 {
		SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_5_0, PixelShaderNormalOutsideFunction()));
	}
}
float4 PixelShaderMultiplyOutsideFunction(VertexShaderOutput input) : SV_Target{
	float maxAlpha = Outside(input);
	return Multiply(LastRenderTargetTexture.SampleLevel(LinearSampler, input.TexCoord, 0),
		float4(Color.rgb, Color.a * maxAlpha * (1 - FilterTexture.SampleLevel(LinearSampler, input.TexCoord, 0).a)));
}

float4 PixelShaderMultiplyInsideFunction(VertexShaderOutput input) : SV_Target{
	float maxAlpha = Inside(input);
	return Multiply(LastRenderTargetTexture.SampleLevel(LinearSampler, input.TexCoord, 0),
		float4(Color.rgb, Color.a * maxAlpha *  FilterTexture.SampleLevel(LinearSampler, input.TexCoord, 0).a));
}

technique11 OutsideMultiplyBorder {
	pass p0 {
		SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_5_0, PixelShaderMultiplyOutsideFunction()));
	}
}

technique11 InsideMultiplyBorder {
	pass p0 {
		SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_5_0, PixelShaderMultiplyOutsideFunction()));
	}
}
float4 PixelShaderScreenOutsideFunction(VertexShaderOutput input) : SV_Target{
	float maxAlpha = Outside(input);
	return Screen(LastRenderTargetTexture.SampleLevel(LinearSampler, input.TexCoord, 0),
		float4(Color.rgb, Color.a * maxAlpha * (1 - FilterTexture.SampleLevel(LinearSampler, input.TexCoord, 0).a)));
}

float4 PixelShaderScreenInsideFunction(VertexShaderOutput input) : SV_Target{
	float maxAlpha = Inside(input);
	return Screen(LastRenderTargetTexture.SampleLevel(LinearSampler, input.TexCoord, 0),
		float4(Color.rgb, Color.a * maxAlpha *  FilterTexture.SampleLevel(LinearSampler, input.TexCoord, 0).a));
}

technique11 OutsideScreenBorder {
	pass p0 {
		SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_5_0, PixelShaderScreenOutsideFunction()));
	}
}

technique11 InsideScreenBorder {
	pass p0 {
		SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_5_0, PixelShaderScreenOutsideFunction()));
	}
}
float4 PixelShaderOverlayOutsideFunction(VertexShaderOutput input) : SV_Target{
	float maxAlpha = Outside(input);
	return Overlay(LastRenderTargetTexture.SampleLevel(LinearSampler, input.TexCoord, 0),
		float4(Color.rgb, Color.a * maxAlpha * (1 - FilterTexture.SampleLevel(LinearSampler, input.TexCoord, 0).a)));
}

float4 PixelShaderOverlayInsideFunction(VertexShaderOutput input) : SV_Target{
	float maxAlpha = Inside(input);
	return Overlay(LastRenderTargetTexture.SampleLevel(LinearSampler, input.TexCoord, 0),
		float4(Color.rgb, Color.a * maxAlpha *  FilterTexture.SampleLevel(LinearSampler, input.TexCoord, 0).a));
}

technique11 OutsideOverlayBorder {
	pass p0 {
		SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_5_0, PixelShaderOverlayOutsideFunction()));
	}
}

technique11 InsideOverlayBorder {
	pass p0 {
		SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_5_0, PixelShaderOverlayOutsideFunction()));
	}
}
float4 PixelShaderHardLightOutsideFunction(VertexShaderOutput input) : SV_Target{
	float maxAlpha = Outside(input);
	return HardLight(LastRenderTargetTexture.SampleLevel(LinearSampler, input.TexCoord, 0),
		float4(Color.rgb, Color.a * maxAlpha * (1 - FilterTexture.SampleLevel(LinearSampler, input.TexCoord, 0).a)));
}

float4 PixelShaderHardLightInsideFunction(VertexShaderOutput input) : SV_Target{
	float maxAlpha = Inside(input);
	return HardLight(LastRenderTargetTexture.SampleLevel(LinearSampler, input.TexCoord, 0),
		float4(Color.rgb, Color.a * maxAlpha *  FilterTexture.SampleLevel(LinearSampler, input.TexCoord, 0).a));
}

technique11 OutsideHardLightBorder {
	pass p0 {
		SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_5_0, PixelShaderHardLightOutsideFunction()));
	}
}

technique11 InsideHardLightBorder {
	pass p0 {
		SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_5_0, PixelShaderHardLightOutsideFunction()));
	}
}
float4 PixelShaderColorDodgeOutsideFunction(VertexShaderOutput input) : SV_Target{
	float maxAlpha = Outside(input);
	return ColorDodge(LastRenderTargetTexture.SampleLevel(LinearSampler, input.TexCoord, 0),
		float4(Color.rgb, Color.a * maxAlpha * (1 - FilterTexture.SampleLevel(LinearSampler, input.TexCoord, 0).a)));
}

float4 PixelShaderColorDodgeInsideFunction(VertexShaderOutput input) : SV_Target{
	float maxAlpha = Inside(input);
	return ColorDodge(LastRenderTargetTexture.SampleLevel(LinearSampler, input.TexCoord, 0),
		float4(Color.rgb, Color.a * maxAlpha *  FilterTexture.SampleLevel(LinearSampler, input.TexCoord, 0).a));
}

technique11 OutsideColorDodgeBorder {
	pass p0 {
		SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_5_0, PixelShaderColorDodgeOutsideFunction()));
	}
}

technique11 InsideColorDodgeBorder {
	pass p0 {
		SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_5_0, PixelShaderColorDodgeOutsideFunction()));
	}
}
float4 PixelShaderLinearDodgeOutsideFunction(VertexShaderOutput input) : SV_Target{
	float maxAlpha = Outside(input);
	return LinearDodge(LastRenderTargetTexture.SampleLevel(LinearSampler, input.TexCoord, 0),
		float4(Color.rgb, Color.a * maxAlpha * (1 - FilterTexture.SampleLevel(LinearSampler, input.TexCoord, 0).a)));
}

float4 PixelShaderLinearDodgeInsideFunction(VertexShaderOutput input) : SV_Target{
	float maxAlpha = Inside(input);
	return LinearDodge(LastRenderTargetTexture.SampleLevel(LinearSampler, input.TexCoord, 0),
		float4(Color.rgb, Color.a * maxAlpha *  FilterTexture.SampleLevel(LinearSampler, input.TexCoord, 0).a));
}

technique11 OutsideLinearDodgeBorder {
	pass p0 {
		SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_5_0, PixelShaderLinearDodgeOutsideFunction()));
	}
}

technique11 InsideLinearDodgeBorder {
	pass p0 {
		SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_5_0, PixelShaderLinearDodgeOutsideFunction()));
	}
}
float4 PixelShaderDifferenceOutsideFunction(VertexShaderOutput input) : SV_Target{
	float maxAlpha = Outside(input);
	return Difference(LastRenderTargetTexture.SampleLevel(LinearSampler, input.TexCoord, 0),
		float4(Color.rgb, Color.a * maxAlpha * (1 - FilterTexture.SampleLevel(LinearSampler, input.TexCoord, 0).a)));
}

float4 PixelShaderDifferenceInsideFunction(VertexShaderOutput input) : SV_Target{
	float maxAlpha = Inside(input);
	return Difference(LastRenderTargetTexture.SampleLevel(LinearSampler, input.TexCoord, 0),
		float4(Color.rgb, Color.a * maxAlpha *  FilterTexture.SampleLevel(LinearSampler, input.TexCoord, 0).a));
}

technique11 OutsideDifferenceBorder {
	pass p0 {
		SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_5_0, PixelShaderDifferenceOutsideFunction()));
	}
}

technique11 InsideDifferenceBorder {
	pass p0 {
		SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_5_0, PixelShaderDifferenceOutsideFunction()));
	}
}