float2 WidthHeight;
float4x4 Projection;

struct DrawInformation
{
    float4x4 Matrix;
    float4 OverlayColor;
    float Alpha;
};

DrawInformation DrawInfo;

struct FilterInformation
{
	float4 Arg1;
	float4 Arg2;
	float4 Arg3;
};

FilterInformation FilterInfo;

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

texture LastRenderTargetTexture;
sampler LastRenderTargetTextureSampler = sampler_state
{
	Texture = < LastRenderTargetTexture >;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = None;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture MaskTexture;
sampler MaskTextureSampler = sampler_state
{
	Texture = < MaskTexture >;
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
	float2 LastRenderTargetTexCoord : TEXCOORD1;
};

#include "Blend.fx"
#include "Filter.fx"

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
    float4 screen = mul(input.Position, DrawInfo.Matrix);
    output.Position  = mul(screen, Projection);
	output.TexCoord = input.TexCoord;
	output.Color = input.Color;
	output.LastRenderTargetTexCoord = float2((screen.x + 0.5f) / WidthHeight.x, (screen.y + 0.5f) / WidthHeight.y);
	return output;
}

float GetMask(float2 texCoord) {
    return tex2D(MaskTextureSampler, texCoord).a;
}

float4 GetSrcColor(float2 texCoord, float2 screenTexCoord, uniform bool paEnabled, uniform bool maskEnabled, uniform int filterType) {
	float4 srcColor = tex2D(TextureSampler, texCoord);
	srcColor.a *= DrawInfo.Alpha;
    if(maskEnabled){
        float maskAlpha = GetMask(screenTexCoord);
        srcColor.a *= maskAlpha;
        if(paEnabled){
            srcColor.rgb *= maskAlpha;
        }
    }
    if(paEnabled){
        srcColor.rgb = srcColor.rgb * (1 - DrawInfo.OverlayColor.a) + DrawInfo.OverlayColor.rgb * DrawInfo.OverlayColor.a * srcColor.a;
    }else{
        srcColor.rgb = srcColor.rgb * (1 - DrawInfo.OverlayColor.a) + DrawInfo.OverlayColor.rgb * DrawInfo.OverlayColor.a;
    }
    if(filterType == 0){
        return ColorFilter(srcColor, FilterInfo, paEnabled);
	}
	else if (filterType == 1) {
		return MaxGrayScaleFilter(srcColor, FilterInfo, paEnabled);
	}
	else if (filterType == 2) {
		return MiddleGrayScaleFilter(srcColor, FilterInfo, paEnabled);
	}
	else if (filterType == 3) {
		return NTSCGrayScaleFilter(srcColor, FilterInfo, paEnabled);
	}
	else if (filterType  == 4) {
		return HDTVGrayScaleFilter(srcColor, FilterInfo, paEnabled);
	}
	else if (filterType == 5) {
		return AverageGrayScaleFilter(srcColor, FilterInfo, paEnabled);
	}
	else if (filterType == 6) {
		return GreenGrayScaleFilter(srcColor, FilterInfo, paEnabled);
	}
	else if (filterType == 7) {
		return MedianGrayScaleFilter(srcColor, FilterInfo, paEnabled);
	}
	else if (filterType == 8) {
		return HueFilter(srcColor, FilterInfo, paEnabled);
	}
	else if (filterType == 9) {
		return SaturationFilter(srcColor, FilterInfo, paEnabled);
	}
	else if (filterType == 10) {
		return BrightnessFilter(srcColor, FilterInfo, paEnabled);
	}
	else if (filterType == 11) {
		return InvertFilter(srcColor, FilterInfo, paEnabled);
    }
	return srcColor;
}

float GetSrcAlpha(float2 texCoord, float2 screenTexCoord, uniform bool maskEnabled) {
  	float4 srcColor = tex2D(TextureSampler, texCoord);
	srcColor.a *= DrawInfo.Alpha;
    if(maskEnabled){
        return srcColor.a * GetMask(screenTexCoord);
    }
    return srcColor.a;
}



float4 PixelShaderIncludeMaskFunction(VertexShaderOutput input, uniform bool maskEnabled) : COLOR0{
    return float4(0, 0, 0, max(tex2D(LastRenderTargetTextureSampler, input.LastRenderTargetTexCoord).a,
        GetSrcAlpha(input.TexCoord, input.LastRenderTargetTexCoord, maskEnabled) * input.Color.a));
}
float4 PixelShaderExcludeMaskFunction(VertexShaderOutput input, uniform bool maskEnabled) : COLOR0{
    return float4(0, 0, 0, min(tex2D(LastRenderTargetTextureSampler, input.LastRenderTargetTexCoord).a,
        1 - GetSrcAlpha(input.TexCoord, input.LastRenderTargetTexCoord, maskEnabled) * input.Color.a));
}
technique IncludeMaskMaskEnabled{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderIncludeMaskFunction(true);
	}
}
technique ExcludeMaskMaskEnabled{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderExcludeMaskFunction(true);
	}
}
technique IncludeMaskMaskDisabled{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderIncludeMaskFunction(false);
	}
}
technique ExcludeMaskMaskDisabled{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderExcludeMaskFunction(false);
	}
}




float4 PixelShaderNormalFunction(VertexShaderOutput input, uniform bool paEnabled, uniform bool maskEnabled, uniform int filterType) : COLOR0{
	float4 destColor = tex2D(LastRenderTargetTextureSampler, input.LastRenderTargetTexCoord);
	float4 srcColor = GetSrcColor(input.TexCoord, input.LastRenderTargetTexCoord, paEnabled, maskEnabled, filterType);
    srcColor *= input.Color;
	return Normal(destColor, srcColor);
}
technique NormalBlendPAEnabledMaskEnabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(true, true, -1);
	}
}
technique NormalBlendPAEnabledMaskEnabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(true, true, 0);
	}
}
technique NormalBlendPAEnabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(true, true, 1);
	}
}
technique NormalBlendPAEnabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(true, true, 2);
	}
}
technique NormalBlendPAEnabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(true, true, 3);
	}
}
technique NormalBlendPAEnabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(true, true, 4);
	}
}
technique NormalBlendPAEnabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(true, true, 5);
	}
}
technique NormalBlendPAEnabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(true, true, 6);
	}
}
technique NormalBlendPAEnabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(true, true, 7);
	}
}
technique NormalBlendPAEnabledMaskEnabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(true, true, 8);
	}
}
technique NormalBlendPAEnabledMaskEnabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(true, true, 9);
	}
}
technique NormalBlendPAEnabledMaskEnabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(true, true, 10);
	}
}
technique NormalBlendPAEnabledMaskEnabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(true, true, 11);
	}
}
technique NormalBlendPAEnabledMaskDisabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(true, false, -1);
	}
}
technique NormalBlendPAEnabledMaskDisabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(true, false, 0);
	}
}
technique NormalBlendPAEnabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(true, false, 1);
	}
}
technique NormalBlendPAEnabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(true, false, 2);
	}
}
technique NormalBlendPAEnabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(true, false, 3);
	}
}
technique NormalBlendPAEnabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(true, false, 4);
	}
}
technique NormalBlendPAEnabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(true, false, 5);
	}
}
technique NormalBlendPAEnabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(true, false, 6);
	}
}
technique NormalBlendPAEnabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(true, false, 7);
	}
}
technique NormalBlendPAEnabledMaskDisabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(true, false, 8);
	}
}
technique NormalBlendPAEnabledMaskDisabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(true, false, 9);
	}
}
technique NormalBlendPAEnabledMaskDisabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(true, false, 10);
	}
}
technique NormalBlendPAEnabledMaskDisabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(true, false, 11);
	}
}
technique NormalBlendPADisabledMaskEnabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(false, true, -1);
	}
}
technique NormalBlendPADisabledMaskEnabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(false, true, 0);
	}
}
technique NormalBlendPADisabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(false, true, 1);
	}
}
technique NormalBlendPADisabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(false, true, 2);
	}
}
technique NormalBlendPADisabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(false, true, 3);
	}
}
technique NormalBlendPADisabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(false, true, 4);
	}
}
technique NormalBlendPADisabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(false, true, 5);
	}
}
technique NormalBlendPADisabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(false, true, 6);
	}
}
technique NormalBlendPADisabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(false, true, 7);
	}
}
technique NormalBlendPADisabledMaskEnabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(false, true, 8);
	}
}
technique NormalBlendPADisabledMaskEnabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(false, true, 9);
	}
}
technique NormalBlendPADisabledMaskEnabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(false, true, 10);
	}
}
technique NormalBlendPADisabledMaskEnabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(false, true, 11);
	}
}
technique NormalBlendPADisabledMaskDisabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(false, false, -1);
	}
}
technique NormalBlendPADisabledMaskDisabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(false, false, 0);
	}
}
technique NormalBlendPADisabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(false, false, 1);
	}
}
technique NormalBlendPADisabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(false, false, 2);
	}
}
technique NormalBlendPADisabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(false, false, 3);
	}
}
technique NormalBlendPADisabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(false, false, 4);
	}
}
technique NormalBlendPADisabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(false, false, 5);
	}
}
technique NormalBlendPADisabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(false, false, 6);
	}
}
technique NormalBlendPADisabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(false, false, 7);
	}
}
technique NormalBlendPADisabledMaskDisabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(false, false, 8);
	}
}
technique NormalBlendPADisabledMaskDisabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(false, false, 9);
	}
}
technique NormalBlendPADisabledMaskDisabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(false, false, 10);
	}
}
technique NormalBlendPADisabledMaskDisabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderNormalFunction(false, false, 11);
	}
}
float4 PixelShaderMultiplyFunction(VertexShaderOutput input, uniform bool paEnabled, uniform bool maskEnabled, uniform int filterType) : COLOR0{
	float4 destColor = tex2D(LastRenderTargetTextureSampler, input.LastRenderTargetTexCoord);
	float4 srcColor = GetSrcColor(input.TexCoord, input.LastRenderTargetTexCoord, paEnabled, maskEnabled, filterType);
    srcColor *= input.Color;
	return Multiply(destColor, srcColor);
}
technique MultiplyBlendPAEnabledMaskEnabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(true, true, -1);
	}
}
technique MultiplyBlendPAEnabledMaskEnabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(true, true, 0);
	}
}
technique MultiplyBlendPAEnabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(true, true, 1);
	}
}
technique MultiplyBlendPAEnabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(true, true, 2);
	}
}
technique MultiplyBlendPAEnabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(true, true, 3);
	}
}
technique MultiplyBlendPAEnabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(true, true, 4);
	}
}
technique MultiplyBlendPAEnabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(true, true, 5);
	}
}
technique MultiplyBlendPAEnabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(true, true, 6);
	}
}
technique MultiplyBlendPAEnabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(true, true, 7);
	}
}
technique MultiplyBlendPAEnabledMaskEnabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(true, true, 8);
	}
}
technique MultiplyBlendPAEnabledMaskEnabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(true, true, 9);
	}
}
technique MultiplyBlendPAEnabledMaskEnabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(true, true, 10);
	}
}
technique MultiplyBlendPAEnabledMaskEnabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(true, true, 11);
	}
}
technique MultiplyBlendPAEnabledMaskDisabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(true, false, -1);
	}
}
technique MultiplyBlendPAEnabledMaskDisabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(true, false, 0);
	}
}
technique MultiplyBlendPAEnabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(true, false, 1);
	}
}
technique MultiplyBlendPAEnabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(true, false, 2);
	}
}
technique MultiplyBlendPAEnabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(true, false, 3);
	}
}
technique MultiplyBlendPAEnabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(true, false, 4);
	}
}
technique MultiplyBlendPAEnabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(true, false, 5);
	}
}
technique MultiplyBlendPAEnabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(true, false, 6);
	}
}
technique MultiplyBlendPAEnabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(true, false, 7);
	}
}
technique MultiplyBlendPAEnabledMaskDisabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(true, false, 8);
	}
}
technique MultiplyBlendPAEnabledMaskDisabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(true, false, 9);
	}
}
technique MultiplyBlendPAEnabledMaskDisabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(true, false, 10);
	}
}
technique MultiplyBlendPAEnabledMaskDisabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(true, false, 11);
	}
}
technique MultiplyBlendPADisabledMaskEnabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(false, true, -1);
	}
}
technique MultiplyBlendPADisabledMaskEnabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(false, true, 0);
	}
}
technique MultiplyBlendPADisabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(false, true, 1);
	}
}
technique MultiplyBlendPADisabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(false, true, 2);
	}
}
technique MultiplyBlendPADisabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(false, true, 3);
	}
}
technique MultiplyBlendPADisabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(false, true, 4);
	}
}
technique MultiplyBlendPADisabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(false, true, 5);
	}
}
technique MultiplyBlendPADisabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(false, true, 6);
	}
}
technique MultiplyBlendPADisabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(false, true, 7);
	}
}
technique MultiplyBlendPADisabledMaskEnabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(false, true, 8);
	}
}
technique MultiplyBlendPADisabledMaskEnabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(false, true, 9);
	}
}
technique MultiplyBlendPADisabledMaskEnabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(false, true, 10);
	}
}
technique MultiplyBlendPADisabledMaskEnabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(false, true, 11);
	}
}
technique MultiplyBlendPADisabledMaskDisabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(false, false, -1);
	}
}
technique MultiplyBlendPADisabledMaskDisabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(false, false, 0);
	}
}
technique MultiplyBlendPADisabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(false, false, 1);
	}
}
technique MultiplyBlendPADisabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(false, false, 2);
	}
}
technique MultiplyBlendPADisabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(false, false, 3);
	}
}
technique MultiplyBlendPADisabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(false, false, 4);
	}
}
technique MultiplyBlendPADisabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(false, false, 5);
	}
}
technique MultiplyBlendPADisabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(false, false, 6);
	}
}
technique MultiplyBlendPADisabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(false, false, 7);
	}
}
technique MultiplyBlendPADisabledMaskDisabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(false, false, 8);
	}
}
technique MultiplyBlendPADisabledMaskDisabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(false, false, 9);
	}
}
technique MultiplyBlendPADisabledMaskDisabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(false, false, 10);
	}
}
technique MultiplyBlendPADisabledMaskDisabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiplyFunction(false, false, 11);
	}
}
float4 PixelShaderScreenFunction(VertexShaderOutput input, uniform bool paEnabled, uniform bool maskEnabled, uniform int filterType) : COLOR0{
	float4 destColor = tex2D(LastRenderTargetTextureSampler, input.LastRenderTargetTexCoord);
	float4 srcColor = GetSrcColor(input.TexCoord, input.LastRenderTargetTexCoord, paEnabled, maskEnabled, filterType);
    srcColor *= input.Color;
	return Screen(destColor, srcColor);
}
technique ScreenBlendPAEnabledMaskEnabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(true, true, -1);
	}
}
technique ScreenBlendPAEnabledMaskEnabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(true, true, 0);
	}
}
technique ScreenBlendPAEnabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(true, true, 1);
	}
}
technique ScreenBlendPAEnabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(true, true, 2);
	}
}
technique ScreenBlendPAEnabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(true, true, 3);
	}
}
technique ScreenBlendPAEnabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(true, true, 4);
	}
}
technique ScreenBlendPAEnabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(true, true, 5);
	}
}
technique ScreenBlendPAEnabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(true, true, 6);
	}
}
technique ScreenBlendPAEnabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(true, true, 7);
	}
}
technique ScreenBlendPAEnabledMaskEnabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(true, true, 8);
	}
}
technique ScreenBlendPAEnabledMaskEnabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(true, true, 9);
	}
}
technique ScreenBlendPAEnabledMaskEnabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(true, true, 10);
	}
}
technique ScreenBlendPAEnabledMaskEnabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(true, true, 11);
	}
}
technique ScreenBlendPAEnabledMaskDisabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(true, false, -1);
	}
}
technique ScreenBlendPAEnabledMaskDisabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(true, false, 0);
	}
}
technique ScreenBlendPAEnabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(true, false, 1);
	}
}
technique ScreenBlendPAEnabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(true, false, 2);
	}
}
technique ScreenBlendPAEnabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(true, false, 3);
	}
}
technique ScreenBlendPAEnabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(true, false, 4);
	}
}
technique ScreenBlendPAEnabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(true, false, 5);
	}
}
technique ScreenBlendPAEnabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(true, false, 6);
	}
}
technique ScreenBlendPAEnabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(true, false, 7);
	}
}
technique ScreenBlendPAEnabledMaskDisabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(true, false, 8);
	}
}
technique ScreenBlendPAEnabledMaskDisabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(true, false, 9);
	}
}
technique ScreenBlendPAEnabledMaskDisabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(true, false, 10);
	}
}
technique ScreenBlendPAEnabledMaskDisabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(true, false, 11);
	}
}
technique ScreenBlendPADisabledMaskEnabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(false, true, -1);
	}
}
technique ScreenBlendPADisabledMaskEnabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(false, true, 0);
	}
}
technique ScreenBlendPADisabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(false, true, 1);
	}
}
technique ScreenBlendPADisabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(false, true, 2);
	}
}
technique ScreenBlendPADisabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(false, true, 3);
	}
}
technique ScreenBlendPADisabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(false, true, 4);
	}
}
technique ScreenBlendPADisabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(false, true, 5);
	}
}
technique ScreenBlendPADisabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(false, true, 6);
	}
}
technique ScreenBlendPADisabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(false, true, 7);
	}
}
technique ScreenBlendPADisabledMaskEnabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(false, true, 8);
	}
}
technique ScreenBlendPADisabledMaskEnabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(false, true, 9);
	}
}
technique ScreenBlendPADisabledMaskEnabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(false, true, 10);
	}
}
technique ScreenBlendPADisabledMaskEnabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(false, true, 11);
	}
}
technique ScreenBlendPADisabledMaskDisabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(false, false, -1);
	}
}
technique ScreenBlendPADisabledMaskDisabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(false, false, 0);
	}
}
technique ScreenBlendPADisabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(false, false, 1);
	}
}
technique ScreenBlendPADisabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(false, false, 2);
	}
}
technique ScreenBlendPADisabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(false, false, 3);
	}
}
technique ScreenBlendPADisabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(false, false, 4);
	}
}
technique ScreenBlendPADisabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(false, false, 5);
	}
}
technique ScreenBlendPADisabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(false, false, 6);
	}
}
technique ScreenBlendPADisabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(false, false, 7);
	}
}
technique ScreenBlendPADisabledMaskDisabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(false, false, 8);
	}
}
technique ScreenBlendPADisabledMaskDisabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(false, false, 9);
	}
}
technique ScreenBlendPADisabledMaskDisabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(false, false, 10);
	}
}
technique ScreenBlendPADisabledMaskDisabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderScreenFunction(false, false, 11);
	}
}
float4 PixelShaderOverlayFunction(VertexShaderOutput input, uniform bool paEnabled, uniform bool maskEnabled, uniform int filterType) : COLOR0{
	float4 destColor = tex2D(LastRenderTargetTextureSampler, input.LastRenderTargetTexCoord);
	float4 srcColor = GetSrcColor(input.TexCoord, input.LastRenderTargetTexCoord, paEnabled, maskEnabled, filterType);
    srcColor *= input.Color;
	return Overlay(destColor, srcColor);
}
technique OverlayBlendPAEnabledMaskEnabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(true, true, -1);
	}
}
technique OverlayBlendPAEnabledMaskEnabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(true, true, 0);
	}
}
technique OverlayBlendPAEnabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(true, true, 1);
	}
}
technique OverlayBlendPAEnabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(true, true, 2);
	}
}
technique OverlayBlendPAEnabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(true, true, 3);
	}
}
technique OverlayBlendPAEnabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(true, true, 4);
	}
}
technique OverlayBlendPAEnabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(true, true, 5);
	}
}
technique OverlayBlendPAEnabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(true, true, 6);
	}
}
technique OverlayBlendPAEnabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(true, true, 7);
	}
}
technique OverlayBlendPAEnabledMaskEnabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(true, true, 8);
	}
}
technique OverlayBlendPAEnabledMaskEnabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(true, true, 9);
	}
}
technique OverlayBlendPAEnabledMaskEnabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(true, true, 10);
	}
}
technique OverlayBlendPAEnabledMaskEnabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(true, true, 11);
	}
}
technique OverlayBlendPAEnabledMaskDisabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(true, false, -1);
	}
}
technique OverlayBlendPAEnabledMaskDisabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(true, false, 0);
	}
}
technique OverlayBlendPAEnabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(true, false, 1);
	}
}
technique OverlayBlendPAEnabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(true, false, 2);
	}
}
technique OverlayBlendPAEnabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(true, false, 3);
	}
}
technique OverlayBlendPAEnabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(true, false, 4);
	}
}
technique OverlayBlendPAEnabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(true, false, 5);
	}
}
technique OverlayBlendPAEnabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(true, false, 6);
	}
}
technique OverlayBlendPAEnabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(true, false, 7);
	}
}
technique OverlayBlendPAEnabledMaskDisabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(true, false, 8);
	}
}
technique OverlayBlendPAEnabledMaskDisabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(true, false, 9);
	}
}
technique OverlayBlendPAEnabledMaskDisabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(true, false, 10);
	}
}
technique OverlayBlendPAEnabledMaskDisabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(true, false, 11);
	}
}
technique OverlayBlendPADisabledMaskEnabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(false, true, -1);
	}
}
technique OverlayBlendPADisabledMaskEnabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(false, true, 0);
	}
}
technique OverlayBlendPADisabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(false, true, 1);
	}
}
technique OverlayBlendPADisabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(false, true, 2);
	}
}
technique OverlayBlendPADisabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(false, true, 3);
	}
}
technique OverlayBlendPADisabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(false, true, 4);
	}
}
technique OverlayBlendPADisabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(false, true, 5);
	}
}
technique OverlayBlendPADisabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(false, true, 6);
	}
}
technique OverlayBlendPADisabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(false, true, 7);
	}
}
technique OverlayBlendPADisabledMaskEnabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(false, true, 8);
	}
}
technique OverlayBlendPADisabledMaskEnabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(false, true, 9);
	}
}
technique OverlayBlendPADisabledMaskEnabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(false, true, 10);
	}
}
technique OverlayBlendPADisabledMaskEnabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(false, true, 11);
	}
}
technique OverlayBlendPADisabledMaskDisabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(false, false, -1);
	}
}
technique OverlayBlendPADisabledMaskDisabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(false, false, 0);
	}
}
technique OverlayBlendPADisabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(false, false, 1);
	}
}
technique OverlayBlendPADisabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(false, false, 2);
	}
}
technique OverlayBlendPADisabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(false, false, 3);
	}
}
technique OverlayBlendPADisabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(false, false, 4);
	}
}
technique OverlayBlendPADisabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(false, false, 5);
	}
}
technique OverlayBlendPADisabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(false, false, 6);
	}
}
technique OverlayBlendPADisabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(false, false, 7);
	}
}
technique OverlayBlendPADisabledMaskDisabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(false, false, 8);
	}
}
technique OverlayBlendPADisabledMaskDisabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(false, false, 9);
	}
}
technique OverlayBlendPADisabledMaskDisabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(false, false, 10);
	}
}
technique OverlayBlendPADisabledMaskDisabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderOverlayFunction(false, false, 11);
	}
}
float4 PixelShaderHardLightFunction(VertexShaderOutput input, uniform bool paEnabled, uniform bool maskEnabled, uniform int filterType) : COLOR0{
	float4 destColor = tex2D(LastRenderTargetTextureSampler, input.LastRenderTargetTexCoord);
	float4 srcColor = GetSrcColor(input.TexCoord, input.LastRenderTargetTexCoord, paEnabled, maskEnabled, filterType);
    srcColor *= input.Color;
	return HardLight(destColor, srcColor);
}
technique HardLightBlendPAEnabledMaskEnabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(true, true, -1);
	}
}
technique HardLightBlendPAEnabledMaskEnabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(true, true, 0);
	}
}
technique HardLightBlendPAEnabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(true, true, 1);
	}
}
technique HardLightBlendPAEnabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(true, true, 2);
	}
}
technique HardLightBlendPAEnabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(true, true, 3);
	}
}
technique HardLightBlendPAEnabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(true, true, 4);
	}
}
technique HardLightBlendPAEnabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(true, true, 5);
	}
}
technique HardLightBlendPAEnabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(true, true, 6);
	}
}
technique HardLightBlendPAEnabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(true, true, 7);
	}
}
technique HardLightBlendPAEnabledMaskEnabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(true, true, 8);
	}
}
technique HardLightBlendPAEnabledMaskEnabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(true, true, 9);
	}
}
technique HardLightBlendPAEnabledMaskEnabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(true, true, 10);
	}
}
technique HardLightBlendPAEnabledMaskEnabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(true, true, 11);
	}
}
technique HardLightBlendPAEnabledMaskDisabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(true, false, -1);
	}
}
technique HardLightBlendPAEnabledMaskDisabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(true, false, 0);
	}
}
technique HardLightBlendPAEnabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(true, false, 1);
	}
}
technique HardLightBlendPAEnabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(true, false, 2);
	}
}
technique HardLightBlendPAEnabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(true, false, 3);
	}
}
technique HardLightBlendPAEnabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(true, false, 4);
	}
}
technique HardLightBlendPAEnabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(true, false, 5);
	}
}
technique HardLightBlendPAEnabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(true, false, 6);
	}
}
technique HardLightBlendPAEnabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(true, false, 7);
	}
}
technique HardLightBlendPAEnabledMaskDisabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(true, false, 8);
	}
}
technique HardLightBlendPAEnabledMaskDisabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(true, false, 9);
	}
}
technique HardLightBlendPAEnabledMaskDisabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(true, false, 10);
	}
}
technique HardLightBlendPAEnabledMaskDisabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(true, false, 11);
	}
}
technique HardLightBlendPADisabledMaskEnabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(false, true, -1);
	}
}
technique HardLightBlendPADisabledMaskEnabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(false, true, 0);
	}
}
technique HardLightBlendPADisabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(false, true, 1);
	}
}
technique HardLightBlendPADisabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(false, true, 2);
	}
}
technique HardLightBlendPADisabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(false, true, 3);
	}
}
technique HardLightBlendPADisabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(false, true, 4);
	}
}
technique HardLightBlendPADisabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(false, true, 5);
	}
}
technique HardLightBlendPADisabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(false, true, 6);
	}
}
technique HardLightBlendPADisabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(false, true, 7);
	}
}
technique HardLightBlendPADisabledMaskEnabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(false, true, 8);
	}
}
technique HardLightBlendPADisabledMaskEnabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(false, true, 9);
	}
}
technique HardLightBlendPADisabledMaskEnabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(false, true, 10);
	}
}
technique HardLightBlendPADisabledMaskEnabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(false, true, 11);
	}
}
technique HardLightBlendPADisabledMaskDisabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(false, false, -1);
	}
}
technique HardLightBlendPADisabledMaskDisabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(false, false, 0);
	}
}
technique HardLightBlendPADisabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(false, false, 1);
	}
}
technique HardLightBlendPADisabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(false, false, 2);
	}
}
technique HardLightBlendPADisabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(false, false, 3);
	}
}
technique HardLightBlendPADisabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(false, false, 4);
	}
}
technique HardLightBlendPADisabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(false, false, 5);
	}
}
technique HardLightBlendPADisabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(false, false, 6);
	}
}
technique HardLightBlendPADisabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(false, false, 7);
	}
}
technique HardLightBlendPADisabledMaskDisabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(false, false, 8);
	}
}
technique HardLightBlendPADisabledMaskDisabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(false, false, 9);
	}
}
technique HardLightBlendPADisabledMaskDisabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(false, false, 10);
	}
}
technique HardLightBlendPADisabledMaskDisabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderHardLightFunction(false, false, 11);
	}
}
float4 PixelShaderColorDodgeFunction(VertexShaderOutput input, uniform bool paEnabled, uniform bool maskEnabled, uniform int filterType) : COLOR0{
	float4 destColor = tex2D(LastRenderTargetTextureSampler, input.LastRenderTargetTexCoord);
	float4 srcColor = GetSrcColor(input.TexCoord, input.LastRenderTargetTexCoord, paEnabled, maskEnabled, filterType);
    srcColor *= input.Color;
	return ColorDodge(destColor, srcColor);
}
technique ColorDodgeBlendPAEnabledMaskEnabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(true, true, -1);
	}
}
technique ColorDodgeBlendPAEnabledMaskEnabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(true, true, 0);
	}
}
technique ColorDodgeBlendPAEnabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(true, true, 1);
	}
}
technique ColorDodgeBlendPAEnabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(true, true, 2);
	}
}
technique ColorDodgeBlendPAEnabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(true, true, 3);
	}
}
technique ColorDodgeBlendPAEnabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(true, true, 4);
	}
}
technique ColorDodgeBlendPAEnabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(true, true, 5);
	}
}
technique ColorDodgeBlendPAEnabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(true, true, 6);
	}
}
technique ColorDodgeBlendPAEnabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(true, true, 7);
	}
}
technique ColorDodgeBlendPAEnabledMaskEnabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(true, true, 8);
	}
}
technique ColorDodgeBlendPAEnabledMaskEnabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(true, true, 9);
	}
}
technique ColorDodgeBlendPAEnabledMaskEnabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(true, true, 10);
	}
}
technique ColorDodgeBlendPAEnabledMaskEnabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(true, true, 11);
	}
}
technique ColorDodgeBlendPAEnabledMaskDisabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(true, false, -1);
	}
}
technique ColorDodgeBlendPAEnabledMaskDisabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(true, false, 0);
	}
}
technique ColorDodgeBlendPAEnabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(true, false, 1);
	}
}
technique ColorDodgeBlendPAEnabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(true, false, 2);
	}
}
technique ColorDodgeBlendPAEnabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(true, false, 3);
	}
}
technique ColorDodgeBlendPAEnabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(true, false, 4);
	}
}
technique ColorDodgeBlendPAEnabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(true, false, 5);
	}
}
technique ColorDodgeBlendPAEnabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(true, false, 6);
	}
}
technique ColorDodgeBlendPAEnabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(true, false, 7);
	}
}
technique ColorDodgeBlendPAEnabledMaskDisabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(true, false, 8);
	}
}
technique ColorDodgeBlendPAEnabledMaskDisabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(true, false, 9);
	}
}
technique ColorDodgeBlendPAEnabledMaskDisabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(true, false, 10);
	}
}
technique ColorDodgeBlendPAEnabledMaskDisabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(true, false, 11);
	}
}
technique ColorDodgeBlendPADisabledMaskEnabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(false, true, -1);
	}
}
technique ColorDodgeBlendPADisabledMaskEnabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(false, true, 0);
	}
}
technique ColorDodgeBlendPADisabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(false, true, 1);
	}
}
technique ColorDodgeBlendPADisabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(false, true, 2);
	}
}
technique ColorDodgeBlendPADisabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(false, true, 3);
	}
}
technique ColorDodgeBlendPADisabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(false, true, 4);
	}
}
technique ColorDodgeBlendPADisabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(false, true, 5);
	}
}
technique ColorDodgeBlendPADisabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(false, true, 6);
	}
}
technique ColorDodgeBlendPADisabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(false, true, 7);
	}
}
technique ColorDodgeBlendPADisabledMaskEnabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(false, true, 8);
	}
}
technique ColorDodgeBlendPADisabledMaskEnabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(false, true, 9);
	}
}
technique ColorDodgeBlendPADisabledMaskEnabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(false, true, 10);
	}
}
technique ColorDodgeBlendPADisabledMaskEnabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(false, true, 11);
	}
}
technique ColorDodgeBlendPADisabledMaskDisabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(false, false, -1);
	}
}
technique ColorDodgeBlendPADisabledMaskDisabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(false, false, 0);
	}
}
technique ColorDodgeBlendPADisabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(false, false, 1);
	}
}
technique ColorDodgeBlendPADisabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(false, false, 2);
	}
}
technique ColorDodgeBlendPADisabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(false, false, 3);
	}
}
technique ColorDodgeBlendPADisabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(false, false, 4);
	}
}
technique ColorDodgeBlendPADisabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(false, false, 5);
	}
}
technique ColorDodgeBlendPADisabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(false, false, 6);
	}
}
technique ColorDodgeBlendPADisabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(false, false, 7);
	}
}
technique ColorDodgeBlendPADisabledMaskDisabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(false, false, 8);
	}
}
technique ColorDodgeBlendPADisabledMaskDisabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(false, false, 9);
	}
}
technique ColorDodgeBlendPADisabledMaskDisabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(false, false, 10);
	}
}
technique ColorDodgeBlendPADisabledMaskDisabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderColorDodgeFunction(false, false, 11);
	}
}
float4 PixelShaderLinearDodgeFunction(VertexShaderOutput input, uniform bool paEnabled, uniform bool maskEnabled, uniform int filterType) : COLOR0{
	float4 destColor = tex2D(LastRenderTargetTextureSampler, input.LastRenderTargetTexCoord);
	float4 srcColor = GetSrcColor(input.TexCoord, input.LastRenderTargetTexCoord, paEnabled, maskEnabled, filterType);
    srcColor *= input.Color;
	return LinearDodge(destColor, srcColor);
}
technique LinearDodgeBlendPAEnabledMaskEnabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(true, true, -1);
	}
}
technique LinearDodgeBlendPAEnabledMaskEnabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(true, true, 0);
	}
}
technique LinearDodgeBlendPAEnabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(true, true, 1);
	}
}
technique LinearDodgeBlendPAEnabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(true, true, 2);
	}
}
technique LinearDodgeBlendPAEnabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(true, true, 3);
	}
}
technique LinearDodgeBlendPAEnabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(true, true, 4);
	}
}
technique LinearDodgeBlendPAEnabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(true, true, 5);
	}
}
technique LinearDodgeBlendPAEnabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(true, true, 6);
	}
}
technique LinearDodgeBlendPAEnabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(true, true, 7);
	}
}
technique LinearDodgeBlendPAEnabledMaskEnabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(true, true, 8);
	}
}
technique LinearDodgeBlendPAEnabledMaskEnabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(true, true, 9);
	}
}
technique LinearDodgeBlendPAEnabledMaskEnabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(true, true, 10);
	}
}
technique LinearDodgeBlendPAEnabledMaskEnabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(true, true, 11);
	}
}
technique LinearDodgeBlendPAEnabledMaskDisabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(true, false, -1);
	}
}
technique LinearDodgeBlendPAEnabledMaskDisabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(true, false, 0);
	}
}
technique LinearDodgeBlendPAEnabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(true, false, 1);
	}
}
technique LinearDodgeBlendPAEnabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(true, false, 2);
	}
}
technique LinearDodgeBlendPAEnabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(true, false, 3);
	}
}
technique LinearDodgeBlendPAEnabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(true, false, 4);
	}
}
technique LinearDodgeBlendPAEnabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(true, false, 5);
	}
}
technique LinearDodgeBlendPAEnabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(true, false, 6);
	}
}
technique LinearDodgeBlendPAEnabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(true, false, 7);
	}
}
technique LinearDodgeBlendPAEnabledMaskDisabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(true, false, 8);
	}
}
technique LinearDodgeBlendPAEnabledMaskDisabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(true, false, 9);
	}
}
technique LinearDodgeBlendPAEnabledMaskDisabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(true, false, 10);
	}
}
technique LinearDodgeBlendPAEnabledMaskDisabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(true, false, 11);
	}
}
technique LinearDodgeBlendPADisabledMaskEnabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(false, true, -1);
	}
}
technique LinearDodgeBlendPADisabledMaskEnabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(false, true, 0);
	}
}
technique LinearDodgeBlendPADisabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(false, true, 1);
	}
}
technique LinearDodgeBlendPADisabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(false, true, 2);
	}
}
technique LinearDodgeBlendPADisabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(false, true, 3);
	}
}
technique LinearDodgeBlendPADisabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(false, true, 4);
	}
}
technique LinearDodgeBlendPADisabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(false, true, 5);
	}
}
technique LinearDodgeBlendPADisabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(false, true, 6);
	}
}
technique LinearDodgeBlendPADisabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(false, true, 7);
	}
}
technique LinearDodgeBlendPADisabledMaskEnabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(false, true, 8);
	}
}
technique LinearDodgeBlendPADisabledMaskEnabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(false, true, 9);
	}
}
technique LinearDodgeBlendPADisabledMaskEnabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(false, true, 10);
	}
}
technique LinearDodgeBlendPADisabledMaskEnabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(false, true, 11);
	}
}
technique LinearDodgeBlendPADisabledMaskDisabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(false, false, -1);
	}
}
technique LinearDodgeBlendPADisabledMaskDisabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(false, false, 0);
	}
}
technique LinearDodgeBlendPADisabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(false, false, 1);
	}
}
technique LinearDodgeBlendPADisabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(false, false, 2);
	}
}
technique LinearDodgeBlendPADisabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(false, false, 3);
	}
}
technique LinearDodgeBlendPADisabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(false, false, 4);
	}
}
technique LinearDodgeBlendPADisabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(false, false, 5);
	}
}
technique LinearDodgeBlendPADisabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(false, false, 6);
	}
}
technique LinearDodgeBlendPADisabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(false, false, 7);
	}
}
technique LinearDodgeBlendPADisabledMaskDisabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(false, false, 8);
	}
}
technique LinearDodgeBlendPADisabledMaskDisabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(false, false, 9);
	}
}
technique LinearDodgeBlendPADisabledMaskDisabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(false, false, 10);
	}
}
technique LinearDodgeBlendPADisabledMaskDisabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderLinearDodgeFunction(false, false, 11);
	}
}
float4 PixelShaderDifferenceFunction(VertexShaderOutput input, uniform bool paEnabled, uniform bool maskEnabled, uniform int filterType) : COLOR0{
	float4 destColor = tex2D(LastRenderTargetTextureSampler, input.LastRenderTargetTexCoord);
	float4 srcColor = GetSrcColor(input.TexCoord, input.LastRenderTargetTexCoord, paEnabled, maskEnabled, filterType);
    srcColor *= input.Color;
	return Difference(destColor, srcColor);
}
technique DifferenceBlendPAEnabledMaskEnabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(true, true, -1);
	}
}
technique DifferenceBlendPAEnabledMaskEnabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(true, true, 0);
	}
}
technique DifferenceBlendPAEnabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(true, true, 1);
	}
}
technique DifferenceBlendPAEnabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(true, true, 2);
	}
}
technique DifferenceBlendPAEnabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(true, true, 3);
	}
}
technique DifferenceBlendPAEnabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(true, true, 4);
	}
}
technique DifferenceBlendPAEnabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(true, true, 5);
	}
}
technique DifferenceBlendPAEnabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(true, true, 6);
	}
}
technique DifferenceBlendPAEnabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(true, true, 7);
	}
}
technique DifferenceBlendPAEnabledMaskEnabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(true, true, 8);
	}
}
technique DifferenceBlendPAEnabledMaskEnabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(true, true, 9);
	}
}
technique DifferenceBlendPAEnabledMaskEnabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(true, true, 10);
	}
}
technique DifferenceBlendPAEnabledMaskEnabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(true, true, 11);
	}
}
technique DifferenceBlendPAEnabledMaskDisabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(true, false, -1);
	}
}
technique DifferenceBlendPAEnabledMaskDisabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(true, false, 0);
	}
}
technique DifferenceBlendPAEnabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(true, false, 1);
	}
}
technique DifferenceBlendPAEnabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(true, false, 2);
	}
}
technique DifferenceBlendPAEnabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(true, false, 3);
	}
}
technique DifferenceBlendPAEnabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(true, false, 4);
	}
}
technique DifferenceBlendPAEnabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(true, false, 5);
	}
}
technique DifferenceBlendPAEnabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(true, false, 6);
	}
}
technique DifferenceBlendPAEnabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(true, false, 7);
	}
}
technique DifferenceBlendPAEnabledMaskDisabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(true, false, 8);
	}
}
technique DifferenceBlendPAEnabledMaskDisabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(true, false, 9);
	}
}
technique DifferenceBlendPAEnabledMaskDisabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(true, false, 10);
	}
}
technique DifferenceBlendPAEnabledMaskDisabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(true, false, 11);
	}
}
technique DifferenceBlendPADisabledMaskEnabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(false, true, -1);
	}
}
technique DifferenceBlendPADisabledMaskEnabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(false, true, 0);
	}
}
technique DifferenceBlendPADisabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(false, true, 1);
	}
}
technique DifferenceBlendPADisabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(false, true, 2);
	}
}
technique DifferenceBlendPADisabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(false, true, 3);
	}
}
technique DifferenceBlendPADisabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(false, true, 4);
	}
}
technique DifferenceBlendPADisabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(false, true, 5);
	}
}
technique DifferenceBlendPADisabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(false, true, 6);
	}
}
technique DifferenceBlendPADisabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(false, true, 7);
	}
}
technique DifferenceBlendPADisabledMaskEnabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(false, true, 8);
	}
}
technique DifferenceBlendPADisabledMaskEnabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(false, true, 9);
	}
}
technique DifferenceBlendPADisabledMaskEnabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(false, true, 10);
	}
}
technique DifferenceBlendPADisabledMaskEnabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(false, true, 11);
	}
}
technique DifferenceBlendPADisabledMaskDisabledNoneFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(false, false, -1);
	}
}
technique DifferenceBlendPADisabledMaskDisabledColorFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(false, false, 0);
	}
}
technique DifferenceBlendPADisabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(false, false, 1);
	}
}
technique DifferenceBlendPADisabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(false, false, 2);
	}
}
technique DifferenceBlendPADisabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(false, false, 3);
	}
}
technique DifferenceBlendPADisabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(false, false, 4);
	}
}
technique DifferenceBlendPADisabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(false, false, 5);
	}
}
technique DifferenceBlendPADisabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(false, false, 6);
	}
}
technique DifferenceBlendPADisabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(false, false, 7);
	}
}
technique DifferenceBlendPADisabledMaskDisabledHueFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(false, false, 8);
	}
}
technique DifferenceBlendPADisabledMaskDisabledSaturationFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(false, false, 9);
	}
}
technique DifferenceBlendPADisabledMaskDisabledBrightnessFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(false, false, 10);
	}
}
technique DifferenceBlendPADisabledMaskDisabledInvertFilter{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = false;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderDifferenceFunction(false, false, 11);
	}
}