#include "Common.fx"

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

Texture2D Texture;
Texture2D LastRenderTargetTexture;
Texture2D MaskTexture;

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
	output.LastRenderTargetTexCoord = float2(screen.x / WidthHeight.x, screen.y / WidthHeight.y);
	return output;
}

float GetMask(float2 texCoord) {
    return MaskTexture.SampleLevel(LinearSampler, texCoord, 0).a;
}

float4 GetSrcColor(float2 texCoord, float2 screenTexCoord, uniform bool paEnabled, uniform bool maskEnabled, uniform int filterType) {
	float4 srcColor = Texture.SampleLevel(LinearSampler, texCoord, 0);
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
  	float4 srcColor = Texture.SampleLevel(LinearSampler, texCoord, 0);
	srcColor.a *= DrawInfo.Alpha;
    if(maskEnabled){
        return srcColor.a * GetMask(screenTexCoord);
    }
    return srcColor.a;
}



float4 PixelShaderIncludeMaskFunction(VertexShaderOutput input, uniform bool maskEnabled) : SV_TARGET{
    return float4(0, 0, 0, max(LastRenderTargetTexture.SampleLevel(LinearSampler, input.LastRenderTargetTexCoord, 0).a,
        GetSrcAlpha(input.TexCoord, input.LastRenderTargetTexCoord, maskEnabled) * input.Color.a));
}
float4 PixelShaderExcludeMaskFunction(VertexShaderOutput input, uniform bool maskEnabled) : SV_TARGET{
    return float4(0, 0, 0, min(LastRenderTargetTexture.SampleLevel(LinearSampler, input.LastRenderTargetTexCoord, 0).a,
        1 - GetSrcAlpha(input.TexCoord, input.LastRenderTargetTexCoord, maskEnabled) * input.Color.a));
}
technique11 IncludeMaskMaskEnabled{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderIncludeMaskFunction(true)));
	}
}
technique11 ExcludeMaskMaskEnabled{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderExcludeMaskFunction(true)));
	}
}
technique11 IncludeMaskMaskDisabled{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderIncludeMaskFunction(false)));
	}
}
technique11 ExcludeMaskMaskDisabled{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderExcludeMaskFunction(false)));
	}
}




float4 PixelShaderNormalFunction(VertexShaderOutput input, uniform bool paEnabled, uniform bool maskEnabled, uniform int filterType) : SV_TARGET{
	float4 destColor = LastRenderTargetTexture.SampleLevel(LinearSampler, input.LastRenderTargetTexCoord, 0);
	float4 srcColor = GetSrcColor(input.TexCoord, input.LastRenderTargetTexCoord, paEnabled, maskEnabled, filterType);
    srcColor *= input.Color;
	return Normal(destColor, srcColor);
}
technique11 NormalBlendPAEnabledMaskEnabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(true, true, -1)));
	}
}
technique11 NormalBlendPAEnabledMaskEnabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(true, true, 0)));
	}
}
technique11 NormalBlendPAEnabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(true, true, 1)));
	}
}
technique11 NormalBlendPAEnabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(true, true, 2)));
	}
}
technique11 NormalBlendPAEnabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(true, true, 3)));
	}
}
technique11 NormalBlendPAEnabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(true, true, 4)));
	}
}
technique11 NormalBlendPAEnabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(true, true, 5)));
	}
}
technique11 NormalBlendPAEnabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(true, true, 6)));
	}
}
technique11 NormalBlendPAEnabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(true, true, 7)));
	}
}
technique11 NormalBlendPAEnabledMaskEnabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(true, true, 8)));
	}
}
technique11 NormalBlendPAEnabledMaskEnabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(true, true, 9)));
	}
}
technique11 NormalBlendPAEnabledMaskEnabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(true, true, 10)));
	}
}
technique11 NormalBlendPAEnabledMaskEnabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(true, true, 11)));
	}
}
technique11 NormalBlendPAEnabledMaskDisabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(true, false, -1)));
	}
}
technique11 NormalBlendPAEnabledMaskDisabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(true, false, 0)));
	}
}
technique11 NormalBlendPAEnabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(true, false, 1)));
	}
}
technique11 NormalBlendPAEnabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(true, false, 2)));
	}
}
technique11 NormalBlendPAEnabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(true, false, 3)));
	}
}
technique11 NormalBlendPAEnabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(true, false, 4)));
	}
}
technique11 NormalBlendPAEnabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(true, false, 5)));
	}
}
technique11 NormalBlendPAEnabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(true, false, 6)));
	}
}
technique11 NormalBlendPAEnabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(true, false, 7)));
	}
}
technique11 NormalBlendPAEnabledMaskDisabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(true, false, 8)));
	}
}
technique11 NormalBlendPAEnabledMaskDisabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(true, false, 9)));
	}
}
technique11 NormalBlendPAEnabledMaskDisabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(true, false, 10)));
	}
}
technique11 NormalBlendPAEnabledMaskDisabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(true, false, 11)));
	}
}
technique11 NormalBlendPADisabledMaskEnabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(false, true, -1)));
	}
}
technique11 NormalBlendPADisabledMaskEnabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(false, true, 0)));
	}
}
technique11 NormalBlendPADisabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(false, true, 1)));
	}
}
technique11 NormalBlendPADisabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(false, true, 2)));
	}
}
technique11 NormalBlendPADisabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(false, true, 3)));
	}
}
technique11 NormalBlendPADisabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(false, true, 4)));
	}
}
technique11 NormalBlendPADisabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(false, true, 5)));
	}
}
technique11 NormalBlendPADisabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(false, true, 6)));
	}
}
technique11 NormalBlendPADisabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(false, true, 7)));
	}
}
technique11 NormalBlendPADisabledMaskEnabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(false, true, 8)));
	}
}
technique11 NormalBlendPADisabledMaskEnabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(false, true, 9)));
	}
}
technique11 NormalBlendPADisabledMaskEnabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(false, true, 10)));
	}
}
technique11 NormalBlendPADisabledMaskEnabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(false, true, 11)));
	}
}
technique11 NormalBlendPADisabledMaskDisabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(false, false, -1)));
	}
}
technique11 NormalBlendPADisabledMaskDisabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(false, false, 0)));
	}
}
technique11 NormalBlendPADisabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(false, false, 1)));
	}
}
technique11 NormalBlendPADisabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(false, false, 2)));
	}
}
technique11 NormalBlendPADisabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(false, false, 3)));
	}
}
technique11 NormalBlendPADisabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(false, false, 4)));
	}
}
technique11 NormalBlendPADisabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(false, false, 5)));
	}
}
technique11 NormalBlendPADisabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(false, false, 6)));
	}
}
technique11 NormalBlendPADisabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(false, false, 7)));
	}
}
technique11 NormalBlendPADisabledMaskDisabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(false, false, 8)));
	}
}
technique11 NormalBlendPADisabledMaskDisabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(false, false, 9)));
	}
}
technique11 NormalBlendPADisabledMaskDisabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(false, false, 10)));
	}
}
technique11 NormalBlendPADisabledMaskDisabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderNormalFunction(false, false, 11)));
	}
}
float4 PixelShaderMultiplyFunction(VertexShaderOutput input, uniform bool paEnabled, uniform bool maskEnabled, uniform int filterType) : SV_TARGET{
	float4 destColor = LastRenderTargetTexture.SampleLevel(LinearSampler, input.LastRenderTargetTexCoord, 0);
	float4 srcColor = GetSrcColor(input.TexCoord, input.LastRenderTargetTexCoord, paEnabled, maskEnabled, filterType);
    srcColor *= input.Color;
	return Multiply(destColor, srcColor);
}
technique11 MultiplyBlendPAEnabledMaskEnabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(true, true, -1)));
	}
}
technique11 MultiplyBlendPAEnabledMaskEnabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(true, true, 0)));
	}
}
technique11 MultiplyBlendPAEnabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(true, true, 1)));
	}
}
technique11 MultiplyBlendPAEnabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(true, true, 2)));
	}
}
technique11 MultiplyBlendPAEnabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(true, true, 3)));
	}
}
technique11 MultiplyBlendPAEnabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(true, true, 4)));
	}
}
technique11 MultiplyBlendPAEnabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(true, true, 5)));
	}
}
technique11 MultiplyBlendPAEnabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(true, true, 6)));
	}
}
technique11 MultiplyBlendPAEnabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(true, true, 7)));
	}
}
technique11 MultiplyBlendPAEnabledMaskEnabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(true, true, 8)));
	}
}
technique11 MultiplyBlendPAEnabledMaskEnabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(true, true, 9)));
	}
}
technique11 MultiplyBlendPAEnabledMaskEnabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(true, true, 10)));
	}
}
technique11 MultiplyBlendPAEnabledMaskEnabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(true, true, 11)));
	}
}
technique11 MultiplyBlendPAEnabledMaskDisabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(true, false, -1)));
	}
}
technique11 MultiplyBlendPAEnabledMaskDisabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(true, false, 0)));
	}
}
technique11 MultiplyBlendPAEnabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(true, false, 1)));
	}
}
technique11 MultiplyBlendPAEnabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(true, false, 2)));
	}
}
technique11 MultiplyBlendPAEnabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(true, false, 3)));
	}
}
technique11 MultiplyBlendPAEnabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(true, false, 4)));
	}
}
technique11 MultiplyBlendPAEnabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(true, false, 5)));
	}
}
technique11 MultiplyBlendPAEnabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(true, false, 6)));
	}
}
technique11 MultiplyBlendPAEnabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(true, false, 7)));
	}
}
technique11 MultiplyBlendPAEnabledMaskDisabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(true, false, 8)));
	}
}
technique11 MultiplyBlendPAEnabledMaskDisabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(true, false, 9)));
	}
}
technique11 MultiplyBlendPAEnabledMaskDisabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(true, false, 10)));
	}
}
technique11 MultiplyBlendPAEnabledMaskDisabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(true, false, 11)));
	}
}
technique11 MultiplyBlendPADisabledMaskEnabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(false, true, -1)));
	}
}
technique11 MultiplyBlendPADisabledMaskEnabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(false, true, 0)));
	}
}
technique11 MultiplyBlendPADisabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(false, true, 1)));
	}
}
technique11 MultiplyBlendPADisabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(false, true, 2)));
	}
}
technique11 MultiplyBlendPADisabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(false, true, 3)));
	}
}
technique11 MultiplyBlendPADisabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(false, true, 4)));
	}
}
technique11 MultiplyBlendPADisabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(false, true, 5)));
	}
}
technique11 MultiplyBlendPADisabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(false, true, 6)));
	}
}
technique11 MultiplyBlendPADisabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(false, true, 7)));
	}
}
technique11 MultiplyBlendPADisabledMaskEnabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(false, true, 8)));
	}
}
technique11 MultiplyBlendPADisabledMaskEnabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(false, true, 9)));
	}
}
technique11 MultiplyBlendPADisabledMaskEnabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(false, true, 10)));
	}
}
technique11 MultiplyBlendPADisabledMaskEnabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(false, true, 11)));
	}
}
technique11 MultiplyBlendPADisabledMaskDisabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(false, false, -1)));
	}
}
technique11 MultiplyBlendPADisabledMaskDisabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(false, false, 0)));
	}
}
technique11 MultiplyBlendPADisabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(false, false, 1)));
	}
}
technique11 MultiplyBlendPADisabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(false, false, 2)));
	}
}
technique11 MultiplyBlendPADisabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(false, false, 3)));
	}
}
technique11 MultiplyBlendPADisabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(false, false, 4)));
	}
}
technique11 MultiplyBlendPADisabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(false, false, 5)));
	}
}
technique11 MultiplyBlendPADisabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(false, false, 6)));
	}
}
technique11 MultiplyBlendPADisabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(false, false, 7)));
	}
}
technique11 MultiplyBlendPADisabledMaskDisabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(false, false, 8)));
	}
}
technique11 MultiplyBlendPADisabledMaskDisabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(false, false, 9)));
	}
}
technique11 MultiplyBlendPADisabledMaskDisabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(false, false, 10)));
	}
}
technique11 MultiplyBlendPADisabledMaskDisabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderMultiplyFunction(false, false, 11)));
	}
}
float4 PixelShaderScreenFunction(VertexShaderOutput input, uniform bool paEnabled, uniform bool maskEnabled, uniform int filterType) : SV_TARGET{
	float4 destColor = LastRenderTargetTexture.SampleLevel(LinearSampler, input.LastRenderTargetTexCoord, 0);
	float4 srcColor = GetSrcColor(input.TexCoord, input.LastRenderTargetTexCoord, paEnabled, maskEnabled, filterType);
    srcColor *= input.Color;
	return Screen(destColor, srcColor);
}
technique11 ScreenBlendPAEnabledMaskEnabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(true, true, -1)));
	}
}
technique11 ScreenBlendPAEnabledMaskEnabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(true, true, 0)));
	}
}
technique11 ScreenBlendPAEnabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(true, true, 1)));
	}
}
technique11 ScreenBlendPAEnabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(true, true, 2)));
	}
}
technique11 ScreenBlendPAEnabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(true, true, 3)));
	}
}
technique11 ScreenBlendPAEnabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(true, true, 4)));
	}
}
technique11 ScreenBlendPAEnabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(true, true, 5)));
	}
}
technique11 ScreenBlendPAEnabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(true, true, 6)));
	}
}
technique11 ScreenBlendPAEnabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(true, true, 7)));
	}
}
technique11 ScreenBlendPAEnabledMaskEnabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(true, true, 8)));
	}
}
technique11 ScreenBlendPAEnabledMaskEnabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(true, true, 9)));
	}
}
technique11 ScreenBlendPAEnabledMaskEnabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(true, true, 10)));
	}
}
technique11 ScreenBlendPAEnabledMaskEnabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(true, true, 11)));
	}
}
technique11 ScreenBlendPAEnabledMaskDisabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(true, false, -1)));
	}
}
technique11 ScreenBlendPAEnabledMaskDisabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(true, false, 0)));
	}
}
technique11 ScreenBlendPAEnabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(true, false, 1)));
	}
}
technique11 ScreenBlendPAEnabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(true, false, 2)));
	}
}
technique11 ScreenBlendPAEnabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(true, false, 3)));
	}
}
technique11 ScreenBlendPAEnabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(true, false, 4)));
	}
}
technique11 ScreenBlendPAEnabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(true, false, 5)));
	}
}
technique11 ScreenBlendPAEnabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(true, false, 6)));
	}
}
technique11 ScreenBlendPAEnabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(true, false, 7)));
	}
}
technique11 ScreenBlendPAEnabledMaskDisabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(true, false, 8)));
	}
}
technique11 ScreenBlendPAEnabledMaskDisabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(true, false, 9)));
	}
}
technique11 ScreenBlendPAEnabledMaskDisabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(true, false, 10)));
	}
}
technique11 ScreenBlendPAEnabledMaskDisabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(true, false, 11)));
	}
}
technique11 ScreenBlendPADisabledMaskEnabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(false, true, -1)));
	}
}
technique11 ScreenBlendPADisabledMaskEnabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(false, true, 0)));
	}
}
technique11 ScreenBlendPADisabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(false, true, 1)));
	}
}
technique11 ScreenBlendPADisabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(false, true, 2)));
	}
}
technique11 ScreenBlendPADisabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(false, true, 3)));
	}
}
technique11 ScreenBlendPADisabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(false, true, 4)));
	}
}
technique11 ScreenBlendPADisabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(false, true, 5)));
	}
}
technique11 ScreenBlendPADisabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(false, true, 6)));
	}
}
technique11 ScreenBlendPADisabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(false, true, 7)));
	}
}
technique11 ScreenBlendPADisabledMaskEnabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(false, true, 8)));
	}
}
technique11 ScreenBlendPADisabledMaskEnabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(false, true, 9)));
	}
}
technique11 ScreenBlendPADisabledMaskEnabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(false, true, 10)));
	}
}
technique11 ScreenBlendPADisabledMaskEnabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(false, true, 11)));
	}
}
technique11 ScreenBlendPADisabledMaskDisabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(false, false, -1)));
	}
}
technique11 ScreenBlendPADisabledMaskDisabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(false, false, 0)));
	}
}
technique11 ScreenBlendPADisabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(false, false, 1)));
	}
}
technique11 ScreenBlendPADisabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(false, false, 2)));
	}
}
technique11 ScreenBlendPADisabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(false, false, 3)));
	}
}
technique11 ScreenBlendPADisabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(false, false, 4)));
	}
}
technique11 ScreenBlendPADisabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(false, false, 5)));
	}
}
technique11 ScreenBlendPADisabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(false, false, 6)));
	}
}
technique11 ScreenBlendPADisabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(false, false, 7)));
	}
}
technique11 ScreenBlendPADisabledMaskDisabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(false, false, 8)));
	}
}
technique11 ScreenBlendPADisabledMaskDisabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(false, false, 9)));
	}
}
technique11 ScreenBlendPADisabledMaskDisabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(false, false, 10)));
	}
}
technique11 ScreenBlendPADisabledMaskDisabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderScreenFunction(false, false, 11)));
	}
}
float4 PixelShaderOverlayFunction(VertexShaderOutput input, uniform bool paEnabled, uniform bool maskEnabled, uniform int filterType) : SV_TARGET{
	float4 destColor = LastRenderTargetTexture.SampleLevel(LinearSampler, input.LastRenderTargetTexCoord, 0);
	float4 srcColor = GetSrcColor(input.TexCoord, input.LastRenderTargetTexCoord, paEnabled, maskEnabled, filterType);
    srcColor *= input.Color;
	return Overlay(destColor, srcColor);
}
technique11 OverlayBlendPAEnabledMaskEnabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(true, true, -1)));
	}
}
technique11 OverlayBlendPAEnabledMaskEnabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(true, true, 0)));
	}
}
technique11 OverlayBlendPAEnabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(true, true, 1)));
	}
}
technique11 OverlayBlendPAEnabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(true, true, 2)));
	}
}
technique11 OverlayBlendPAEnabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(true, true, 3)));
	}
}
technique11 OverlayBlendPAEnabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(true, true, 4)));
	}
}
technique11 OverlayBlendPAEnabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(true, true, 5)));
	}
}
technique11 OverlayBlendPAEnabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(true, true, 6)));
	}
}
technique11 OverlayBlendPAEnabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(true, true, 7)));
	}
}
technique11 OverlayBlendPAEnabledMaskEnabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(true, true, 8)));
	}
}
technique11 OverlayBlendPAEnabledMaskEnabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(true, true, 9)));
	}
}
technique11 OverlayBlendPAEnabledMaskEnabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(true, true, 10)));
	}
}
technique11 OverlayBlendPAEnabledMaskEnabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(true, true, 11)));
	}
}
technique11 OverlayBlendPAEnabledMaskDisabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(true, false, -1)));
	}
}
technique11 OverlayBlendPAEnabledMaskDisabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(true, false, 0)));
	}
}
technique11 OverlayBlendPAEnabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(true, false, 1)));
	}
}
technique11 OverlayBlendPAEnabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(true, false, 2)));
	}
}
technique11 OverlayBlendPAEnabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(true, false, 3)));
	}
}
technique11 OverlayBlendPAEnabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(true, false, 4)));
	}
}
technique11 OverlayBlendPAEnabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(true, false, 5)));
	}
}
technique11 OverlayBlendPAEnabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(true, false, 6)));
	}
}
technique11 OverlayBlendPAEnabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(true, false, 7)));
	}
}
technique11 OverlayBlendPAEnabledMaskDisabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(true, false, 8)));
	}
}
technique11 OverlayBlendPAEnabledMaskDisabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(true, false, 9)));
	}
}
technique11 OverlayBlendPAEnabledMaskDisabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(true, false, 10)));
	}
}
technique11 OverlayBlendPAEnabledMaskDisabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(true, false, 11)));
	}
}
technique11 OverlayBlendPADisabledMaskEnabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(false, true, -1)));
	}
}
technique11 OverlayBlendPADisabledMaskEnabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(false, true, 0)));
	}
}
technique11 OverlayBlendPADisabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(false, true, 1)));
	}
}
technique11 OverlayBlendPADisabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(false, true, 2)));
	}
}
technique11 OverlayBlendPADisabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(false, true, 3)));
	}
}
technique11 OverlayBlendPADisabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(false, true, 4)));
	}
}
technique11 OverlayBlendPADisabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(false, true, 5)));
	}
}
technique11 OverlayBlendPADisabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(false, true, 6)));
	}
}
technique11 OverlayBlendPADisabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(false, true, 7)));
	}
}
technique11 OverlayBlendPADisabledMaskEnabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(false, true, 8)));
	}
}
technique11 OverlayBlendPADisabledMaskEnabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(false, true, 9)));
	}
}
technique11 OverlayBlendPADisabledMaskEnabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(false, true, 10)));
	}
}
technique11 OverlayBlendPADisabledMaskEnabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(false, true, 11)));
	}
}
technique11 OverlayBlendPADisabledMaskDisabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(false, false, -1)));
	}
}
technique11 OverlayBlendPADisabledMaskDisabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(false, false, 0)));
	}
}
technique11 OverlayBlendPADisabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(false, false, 1)));
	}
}
technique11 OverlayBlendPADisabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(false, false, 2)));
	}
}
technique11 OverlayBlendPADisabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(false, false, 3)));
	}
}
technique11 OverlayBlendPADisabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(false, false, 4)));
	}
}
technique11 OverlayBlendPADisabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(false, false, 5)));
	}
}
technique11 OverlayBlendPADisabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(false, false, 6)));
	}
}
technique11 OverlayBlendPADisabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(false, false, 7)));
	}
}
technique11 OverlayBlendPADisabledMaskDisabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(false, false, 8)));
	}
}
technique11 OverlayBlendPADisabledMaskDisabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(false, false, 9)));
	}
}
technique11 OverlayBlendPADisabledMaskDisabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(false, false, 10)));
	}
}
technique11 OverlayBlendPADisabledMaskDisabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderOverlayFunction(false, false, 11)));
	}
}
float4 PixelShaderHardLightFunction(VertexShaderOutput input, uniform bool paEnabled, uniform bool maskEnabled, uniform int filterType) : SV_TARGET{
	float4 destColor = LastRenderTargetTexture.SampleLevel(LinearSampler, input.LastRenderTargetTexCoord, 0);
	float4 srcColor = GetSrcColor(input.TexCoord, input.LastRenderTargetTexCoord, paEnabled, maskEnabled, filterType);
    srcColor *= input.Color;
	return HardLight(destColor, srcColor);
}
technique11 HardLightBlendPAEnabledMaskEnabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(true, true, -1)));
	}
}
technique11 HardLightBlendPAEnabledMaskEnabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(true, true, 0)));
	}
}
technique11 HardLightBlendPAEnabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(true, true, 1)));
	}
}
technique11 HardLightBlendPAEnabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(true, true, 2)));
	}
}
technique11 HardLightBlendPAEnabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(true, true, 3)));
	}
}
technique11 HardLightBlendPAEnabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(true, true, 4)));
	}
}
technique11 HardLightBlendPAEnabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(true, true, 5)));
	}
}
technique11 HardLightBlendPAEnabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(true, true, 6)));
	}
}
technique11 HardLightBlendPAEnabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(true, true, 7)));
	}
}
technique11 HardLightBlendPAEnabledMaskEnabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(true, true, 8)));
	}
}
technique11 HardLightBlendPAEnabledMaskEnabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(true, true, 9)));
	}
}
technique11 HardLightBlendPAEnabledMaskEnabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(true, true, 10)));
	}
}
technique11 HardLightBlendPAEnabledMaskEnabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(true, true, 11)));
	}
}
technique11 HardLightBlendPAEnabledMaskDisabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(true, false, -1)));
	}
}
technique11 HardLightBlendPAEnabledMaskDisabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(true, false, 0)));
	}
}
technique11 HardLightBlendPAEnabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(true, false, 1)));
	}
}
technique11 HardLightBlendPAEnabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(true, false, 2)));
	}
}
technique11 HardLightBlendPAEnabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(true, false, 3)));
	}
}
technique11 HardLightBlendPAEnabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(true, false, 4)));
	}
}
technique11 HardLightBlendPAEnabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(true, false, 5)));
	}
}
technique11 HardLightBlendPAEnabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(true, false, 6)));
	}
}
technique11 HardLightBlendPAEnabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(true, false, 7)));
	}
}
technique11 HardLightBlendPAEnabledMaskDisabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(true, false, 8)));
	}
}
technique11 HardLightBlendPAEnabledMaskDisabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(true, false, 9)));
	}
}
technique11 HardLightBlendPAEnabledMaskDisabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(true, false, 10)));
	}
}
technique11 HardLightBlendPAEnabledMaskDisabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(true, false, 11)));
	}
}
technique11 HardLightBlendPADisabledMaskEnabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(false, true, -1)));
	}
}
technique11 HardLightBlendPADisabledMaskEnabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(false, true, 0)));
	}
}
technique11 HardLightBlendPADisabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(false, true, 1)));
	}
}
technique11 HardLightBlendPADisabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(false, true, 2)));
	}
}
technique11 HardLightBlendPADisabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(false, true, 3)));
	}
}
technique11 HardLightBlendPADisabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(false, true, 4)));
	}
}
technique11 HardLightBlendPADisabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(false, true, 5)));
	}
}
technique11 HardLightBlendPADisabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(false, true, 6)));
	}
}
technique11 HardLightBlendPADisabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(false, true, 7)));
	}
}
technique11 HardLightBlendPADisabledMaskEnabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(false, true, 8)));
	}
}
technique11 HardLightBlendPADisabledMaskEnabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(false, true, 9)));
	}
}
technique11 HardLightBlendPADisabledMaskEnabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(false, true, 10)));
	}
}
technique11 HardLightBlendPADisabledMaskEnabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(false, true, 11)));
	}
}
technique11 HardLightBlendPADisabledMaskDisabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(false, false, -1)));
	}
}
technique11 HardLightBlendPADisabledMaskDisabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(false, false, 0)));
	}
}
technique11 HardLightBlendPADisabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(false, false, 1)));
	}
}
technique11 HardLightBlendPADisabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(false, false, 2)));
	}
}
technique11 HardLightBlendPADisabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(false, false, 3)));
	}
}
technique11 HardLightBlendPADisabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(false, false, 4)));
	}
}
technique11 HardLightBlendPADisabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(false, false, 5)));
	}
}
technique11 HardLightBlendPADisabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(false, false, 6)));
	}
}
technique11 HardLightBlendPADisabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(false, false, 7)));
	}
}
technique11 HardLightBlendPADisabledMaskDisabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(false, false, 8)));
	}
}
technique11 HardLightBlendPADisabledMaskDisabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(false, false, 9)));
	}
}
technique11 HardLightBlendPADisabledMaskDisabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(false, false, 10)));
	}
}
technique11 HardLightBlendPADisabledMaskDisabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderHardLightFunction(false, false, 11)));
	}
}
float4 PixelShaderColorDodgeFunction(VertexShaderOutput input, uniform bool paEnabled, uniform bool maskEnabled, uniform int filterType) : SV_TARGET{
	float4 destColor = LastRenderTargetTexture.SampleLevel(LinearSampler, input.LastRenderTargetTexCoord, 0);
	float4 srcColor = GetSrcColor(input.TexCoord, input.LastRenderTargetTexCoord, paEnabled, maskEnabled, filterType);
    srcColor *= input.Color;
	return ColorDodge(destColor, srcColor);
}
technique11 ColorDodgeBlendPAEnabledMaskEnabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(true, true, -1)));
	}
}
technique11 ColorDodgeBlendPAEnabledMaskEnabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(true, true, 0)));
	}
}
technique11 ColorDodgeBlendPAEnabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(true, true, 1)));
	}
}
technique11 ColorDodgeBlendPAEnabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(true, true, 2)));
	}
}
technique11 ColorDodgeBlendPAEnabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(true, true, 3)));
	}
}
technique11 ColorDodgeBlendPAEnabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(true, true, 4)));
	}
}
technique11 ColorDodgeBlendPAEnabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(true, true, 5)));
	}
}
technique11 ColorDodgeBlendPAEnabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(true, true, 6)));
	}
}
technique11 ColorDodgeBlendPAEnabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(true, true, 7)));
	}
}
technique11 ColorDodgeBlendPAEnabledMaskEnabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(true, true, 8)));
	}
}
technique11 ColorDodgeBlendPAEnabledMaskEnabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(true, true, 9)));
	}
}
technique11 ColorDodgeBlendPAEnabledMaskEnabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(true, true, 10)));
	}
}
technique11 ColorDodgeBlendPAEnabledMaskEnabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(true, true, 11)));
	}
}
technique11 ColorDodgeBlendPAEnabledMaskDisabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(true, false, -1)));
	}
}
technique11 ColorDodgeBlendPAEnabledMaskDisabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(true, false, 0)));
	}
}
technique11 ColorDodgeBlendPAEnabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(true, false, 1)));
	}
}
technique11 ColorDodgeBlendPAEnabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(true, false, 2)));
	}
}
technique11 ColorDodgeBlendPAEnabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(true, false, 3)));
	}
}
technique11 ColorDodgeBlendPAEnabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(true, false, 4)));
	}
}
technique11 ColorDodgeBlendPAEnabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(true, false, 5)));
	}
}
technique11 ColorDodgeBlendPAEnabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(true, false, 6)));
	}
}
technique11 ColorDodgeBlendPAEnabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(true, false, 7)));
	}
}
technique11 ColorDodgeBlendPAEnabledMaskDisabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(true, false, 8)));
	}
}
technique11 ColorDodgeBlendPAEnabledMaskDisabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(true, false, 9)));
	}
}
technique11 ColorDodgeBlendPAEnabledMaskDisabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(true, false, 10)));
	}
}
technique11 ColorDodgeBlendPAEnabledMaskDisabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(true, false, 11)));
	}
}
technique11 ColorDodgeBlendPADisabledMaskEnabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(false, true, -1)));
	}
}
technique11 ColorDodgeBlendPADisabledMaskEnabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(false, true, 0)));
	}
}
technique11 ColorDodgeBlendPADisabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(false, true, 1)));
	}
}
technique11 ColorDodgeBlendPADisabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(false, true, 2)));
	}
}
technique11 ColorDodgeBlendPADisabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(false, true, 3)));
	}
}
technique11 ColorDodgeBlendPADisabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(false, true, 4)));
	}
}
technique11 ColorDodgeBlendPADisabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(false, true, 5)));
	}
}
technique11 ColorDodgeBlendPADisabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(false, true, 6)));
	}
}
technique11 ColorDodgeBlendPADisabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(false, true, 7)));
	}
}
technique11 ColorDodgeBlendPADisabledMaskEnabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(false, true, 8)));
	}
}
technique11 ColorDodgeBlendPADisabledMaskEnabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(false, true, 9)));
	}
}
technique11 ColorDodgeBlendPADisabledMaskEnabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(false, true, 10)));
	}
}
technique11 ColorDodgeBlendPADisabledMaskEnabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(false, true, 11)));
	}
}
technique11 ColorDodgeBlendPADisabledMaskDisabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(false, false, -1)));
	}
}
technique11 ColorDodgeBlendPADisabledMaskDisabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(false, false, 0)));
	}
}
technique11 ColorDodgeBlendPADisabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(false, false, 1)));
	}
}
technique11 ColorDodgeBlendPADisabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(false, false, 2)));
	}
}
technique11 ColorDodgeBlendPADisabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(false, false, 3)));
	}
}
technique11 ColorDodgeBlendPADisabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(false, false, 4)));
	}
}
technique11 ColorDodgeBlendPADisabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(false, false, 5)));
	}
}
technique11 ColorDodgeBlendPADisabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(false, false, 6)));
	}
}
technique11 ColorDodgeBlendPADisabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(false, false, 7)));
	}
}
technique11 ColorDodgeBlendPADisabledMaskDisabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(false, false, 8)));
	}
}
technique11 ColorDodgeBlendPADisabledMaskDisabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(false, false, 9)));
	}
}
technique11 ColorDodgeBlendPADisabledMaskDisabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(false, false, 10)));
	}
}
technique11 ColorDodgeBlendPADisabledMaskDisabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderColorDodgeFunction(false, false, 11)));
	}
}
float4 PixelShaderLinearDodgeFunction(VertexShaderOutput input, uniform bool paEnabled, uniform bool maskEnabled, uniform int filterType) : SV_TARGET{
	float4 destColor = LastRenderTargetTexture.SampleLevel(LinearSampler, input.LastRenderTargetTexCoord, 0);
	float4 srcColor = GetSrcColor(input.TexCoord, input.LastRenderTargetTexCoord, paEnabled, maskEnabled, filterType);
    srcColor *= input.Color;
	return LinearDodge(destColor, srcColor);
}
technique11 LinearDodgeBlendPAEnabledMaskEnabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(true, true, -1)));
	}
}
technique11 LinearDodgeBlendPAEnabledMaskEnabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(true, true, 0)));
	}
}
technique11 LinearDodgeBlendPAEnabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(true, true, 1)));
	}
}
technique11 LinearDodgeBlendPAEnabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(true, true, 2)));
	}
}
technique11 LinearDodgeBlendPAEnabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(true, true, 3)));
	}
}
technique11 LinearDodgeBlendPAEnabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(true, true, 4)));
	}
}
technique11 LinearDodgeBlendPAEnabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(true, true, 5)));
	}
}
technique11 LinearDodgeBlendPAEnabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(true, true, 6)));
	}
}
technique11 LinearDodgeBlendPAEnabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(true, true, 7)));
	}
}
technique11 LinearDodgeBlendPAEnabledMaskEnabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(true, true, 8)));
	}
}
technique11 LinearDodgeBlendPAEnabledMaskEnabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(true, true, 9)));
	}
}
technique11 LinearDodgeBlendPAEnabledMaskEnabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(true, true, 10)));
	}
}
technique11 LinearDodgeBlendPAEnabledMaskEnabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(true, true, 11)));
	}
}
technique11 LinearDodgeBlendPAEnabledMaskDisabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(true, false, -1)));
	}
}
technique11 LinearDodgeBlendPAEnabledMaskDisabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(true, false, 0)));
	}
}
technique11 LinearDodgeBlendPAEnabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(true, false, 1)));
	}
}
technique11 LinearDodgeBlendPAEnabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(true, false, 2)));
	}
}
technique11 LinearDodgeBlendPAEnabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(true, false, 3)));
	}
}
technique11 LinearDodgeBlendPAEnabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(true, false, 4)));
	}
}
technique11 LinearDodgeBlendPAEnabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(true, false, 5)));
	}
}
technique11 LinearDodgeBlendPAEnabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(true, false, 6)));
	}
}
technique11 LinearDodgeBlendPAEnabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(true, false, 7)));
	}
}
technique11 LinearDodgeBlendPAEnabledMaskDisabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(true, false, 8)));
	}
}
technique11 LinearDodgeBlendPAEnabledMaskDisabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(true, false, 9)));
	}
}
technique11 LinearDodgeBlendPAEnabledMaskDisabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(true, false, 10)));
	}
}
technique11 LinearDodgeBlendPAEnabledMaskDisabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(true, false, 11)));
	}
}
technique11 LinearDodgeBlendPADisabledMaskEnabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(false, true, -1)));
	}
}
technique11 LinearDodgeBlendPADisabledMaskEnabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(false, true, 0)));
	}
}
technique11 LinearDodgeBlendPADisabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(false, true, 1)));
	}
}
technique11 LinearDodgeBlendPADisabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(false, true, 2)));
	}
}
technique11 LinearDodgeBlendPADisabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(false, true, 3)));
	}
}
technique11 LinearDodgeBlendPADisabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(false, true, 4)));
	}
}
technique11 LinearDodgeBlendPADisabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(false, true, 5)));
	}
}
technique11 LinearDodgeBlendPADisabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(false, true, 6)));
	}
}
technique11 LinearDodgeBlendPADisabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(false, true, 7)));
	}
}
technique11 LinearDodgeBlendPADisabledMaskEnabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(false, true, 8)));
	}
}
technique11 LinearDodgeBlendPADisabledMaskEnabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(false, true, 9)));
	}
}
technique11 LinearDodgeBlendPADisabledMaskEnabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(false, true, 10)));
	}
}
technique11 LinearDodgeBlendPADisabledMaskEnabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(false, true, 11)));
	}
}
technique11 LinearDodgeBlendPADisabledMaskDisabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(false, false, -1)));
	}
}
technique11 LinearDodgeBlendPADisabledMaskDisabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(false, false, 0)));
	}
}
technique11 LinearDodgeBlendPADisabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(false, false, 1)));
	}
}
technique11 LinearDodgeBlendPADisabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(false, false, 2)));
	}
}
technique11 LinearDodgeBlendPADisabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(false, false, 3)));
	}
}
technique11 LinearDodgeBlendPADisabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(false, false, 4)));
	}
}
technique11 LinearDodgeBlendPADisabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(false, false, 5)));
	}
}
technique11 LinearDodgeBlendPADisabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(false, false, 6)));
	}
}
technique11 LinearDodgeBlendPADisabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(false, false, 7)));
	}
}
technique11 LinearDodgeBlendPADisabledMaskDisabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(false, false, 8)));
	}
}
technique11 LinearDodgeBlendPADisabledMaskDisabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(false, false, 9)));
	}
}
technique11 LinearDodgeBlendPADisabledMaskDisabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(false, false, 10)));
	}
}
technique11 LinearDodgeBlendPADisabledMaskDisabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderLinearDodgeFunction(false, false, 11)));
	}
}
float4 PixelShaderDifferenceFunction(VertexShaderOutput input, uniform bool paEnabled, uniform bool maskEnabled, uniform int filterType) : SV_TARGET{
	float4 destColor = LastRenderTargetTexture.SampleLevel(LinearSampler, input.LastRenderTargetTexCoord, 0);
	float4 srcColor = GetSrcColor(input.TexCoord, input.LastRenderTargetTexCoord, paEnabled, maskEnabled, filterType);
    srcColor *= input.Color;
	return Difference(destColor, srcColor);
}
technique11 DifferenceBlendPAEnabledMaskEnabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(true, true, -1)));
	}
}
technique11 DifferenceBlendPAEnabledMaskEnabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(true, true, 0)));
	}
}
technique11 DifferenceBlendPAEnabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(true, true, 1)));
	}
}
technique11 DifferenceBlendPAEnabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(true, true, 2)));
	}
}
technique11 DifferenceBlendPAEnabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(true, true, 3)));
	}
}
technique11 DifferenceBlendPAEnabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(true, true, 4)));
	}
}
technique11 DifferenceBlendPAEnabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(true, true, 5)));
	}
}
technique11 DifferenceBlendPAEnabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(true, true, 6)));
	}
}
technique11 DifferenceBlendPAEnabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(true, true, 7)));
	}
}
technique11 DifferenceBlendPAEnabledMaskEnabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(true, true, 8)));
	}
}
technique11 DifferenceBlendPAEnabledMaskEnabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(true, true, 9)));
	}
}
technique11 DifferenceBlendPAEnabledMaskEnabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(true, true, 10)));
	}
}
technique11 DifferenceBlendPAEnabledMaskEnabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(true, true, 11)));
	}
}
technique11 DifferenceBlendPAEnabledMaskDisabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(true, false, -1)));
	}
}
technique11 DifferenceBlendPAEnabledMaskDisabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(true, false, 0)));
	}
}
technique11 DifferenceBlendPAEnabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(true, false, 1)));
	}
}
technique11 DifferenceBlendPAEnabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(true, false, 2)));
	}
}
technique11 DifferenceBlendPAEnabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(true, false, 3)));
	}
}
technique11 DifferenceBlendPAEnabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(true, false, 4)));
	}
}
technique11 DifferenceBlendPAEnabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(true, false, 5)));
	}
}
technique11 DifferenceBlendPAEnabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(true, false, 6)));
	}
}
technique11 DifferenceBlendPAEnabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(true, false, 7)));
	}
}
technique11 DifferenceBlendPAEnabledMaskDisabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(true, false, 8)));
	}
}
technique11 DifferenceBlendPAEnabledMaskDisabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(true, false, 9)));
	}
}
technique11 DifferenceBlendPAEnabledMaskDisabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(true, false, 10)));
	}
}
technique11 DifferenceBlendPAEnabledMaskDisabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(true, false, 11)));
	}
}
technique11 DifferenceBlendPADisabledMaskEnabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(false, true, -1)));
	}
}
technique11 DifferenceBlendPADisabledMaskEnabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(false, true, 0)));
	}
}
technique11 DifferenceBlendPADisabledMaskEnabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(false, true, 1)));
	}
}
technique11 DifferenceBlendPADisabledMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(false, true, 2)));
	}
}
technique11 DifferenceBlendPADisabledMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(false, true, 3)));
	}
}
technique11 DifferenceBlendPADisabledMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(false, true, 4)));
	}
}
technique11 DifferenceBlendPADisabledMaskEnabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(false, true, 5)));
	}
}
technique11 DifferenceBlendPADisabledMaskEnabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(false, true, 6)));
	}
}
technique11 DifferenceBlendPADisabledMaskEnabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(false, true, 7)));
	}
}
technique11 DifferenceBlendPADisabledMaskEnabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(false, true, 8)));
	}
}
technique11 DifferenceBlendPADisabledMaskEnabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(false, true, 9)));
	}
}
technique11 DifferenceBlendPADisabledMaskEnabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(false, true, 10)));
	}
}
technique11 DifferenceBlendPADisabledMaskEnabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(false, true, 11)));
	}
}
technique11 DifferenceBlendPADisabledMaskDisabledNoneFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(false, false, -1)));
	}
}
technique11 DifferenceBlendPADisabledMaskDisabledColorFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(false, false, 0)));
	}
}
technique11 DifferenceBlendPADisabledMaskDisabledMaxGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(false, false, 1)));
	}
}
technique11 DifferenceBlendPADisabledMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(false, false, 2)));
	}
}
technique11 DifferenceBlendPADisabledMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(false, false, 3)));
	}
}
technique11 DifferenceBlendPADisabledMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(false, false, 4)));
	}
}
technique11 DifferenceBlendPADisabledMaskDisabledAverageGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(false, false, 5)));
	}
}
technique11 DifferenceBlendPADisabledMaskDisabledGreenGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(false, false, 6)));
	}
}
technique11 DifferenceBlendPADisabledMaskDisabledMedianGrayScaleFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(false, false, 7)));
	}
}
technique11 DifferenceBlendPADisabledMaskDisabledHueFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(false, false, 8)));
	}
}
technique11 DifferenceBlendPADisabledMaskDisabledSaturationFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(false, false, 9)));
	}
}
technique11 DifferenceBlendPADisabledMaskDisabledBrightnessFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(false, false, 10)));
	}
}
technique11 DifferenceBlendPADisabledMaskDisabledInvertFilter{
	pass p0 {
		SetBlendState(NoBlend, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderDifferenceFunction(false, false, 11)));
	}
}