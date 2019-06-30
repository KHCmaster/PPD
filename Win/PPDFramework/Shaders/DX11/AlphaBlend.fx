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
	float4 Color    : COLOR0;
	float2 TexCoord : TEXCOORD0;
	float2 LastRenderTargetTexCoord : TEXCOORD1;
};

#include "Blend.fx"
#include "Filter.fx"

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	float4 screen = float4(input.Position, 1.0);
	output.Position = mul(screen, Projection);
	output.TexCoord = input.TexCoord;
	output.Color = input.Color;
	output.LastRenderTargetTexCoord = float2((screen.x + 0.5) / WidthHeight.x, (screen.y + 0.5) / WidthHeight.y);
	return output;
}

float GetMask(float2 texCoord) {
    return MaskTexture.SampleLevel(LinearSampler, texCoord, 0).a;
}

float4 GetSrcColor(float2 texCoord, float2 screenTexCoord, uniform bool maskEnabled, uniform int filterType) {
	float4 srcColor = Texture.SampleLevel(LinearSampler, texCoord, 0);
	srcColor.a *= DrawInfo.Alpha;
    if(maskEnabled){
        srcColor.a *= GetMask(screenTexCoord);
    }
    srcColor.rgb = srcColor.rgb * (1 - DrawInfo.OverlayColor.a) + DrawInfo.OverlayColor.rgb * DrawInfo.OverlayColor.a;
    if(filterType == 0){
        return ColorFilter(srcColor, FilterInfo);
	}
	else if (filterType == 1) {
		return MaxGrayScaleFilter(srcColor, FilterInfo);
	}
	else if (filterType == 2) {
		return MiddleGrayScaleFilter(srcColor, FilterInfo);
	}
	else if (filterType == 3) {
		return NTSCGrayScaleFilter(srcColor, FilterInfo);
	}
	else if (filterType  == 4) {
		return HDTVGrayScaleFilter(srcColor, FilterInfo);
	}
	else if (filterType == 5) {
		return AverageGrayScaleFilter(srcColor, FilterInfo);
	}
	else if (filterType == 6) {
		return GreenGrayScaleFilter(srcColor, FilterInfo);
	}
	else if (filterType == 7) {
		return MedianGrayScaleFilter(srcColor, FilterInfo);
	}
	else if (filterType == 8) {
		return HueFilter(srcColor, FilterInfo);
	}
	else if (filterType == 9) {
		return SaturationFilter(srcColor, FilterInfo);
	}
	else if (filterType == 10) {
		return BrightnessFilter(srcColor, FilterInfo);
	}
	else if (filterType == 11) {
		return InvertFilter(srcColor, FilterInfo);
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
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_5_0, PixelShaderIncludeMaskFunction(true)));
	}
}
technique ExcludeMaskMaskEnabled{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_5_0, PixelShaderExcludeMaskFunction(true)));
	}
}
technique11 IncludeMaskMaskDisabled{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_5_0, PixelShaderIncludeMaskFunction(false)));
	}
}
technique ExcludeMaskMaskDisabled{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_5_0, PixelShaderExcludeMaskFunction(false)));
	}
}



float4 PixelShaderNormalFunction(VertexShaderOutput input, uniform bool maskEnabled, uniform int filterType) : SV_TARGET{
	float4 destColor = LastRenderTargetTexture.SampleLevel(LinearSampler, input.LastRenderTargetTexCoord, 0);
	float4 srcColor = GetSrcColor(input.TexCoord, input.LastRenderTargetTexCoord, maskEnabled, filterType) * input.Color;
	return Normal(destColor, srcColor);
}
technique NormalBlendMaskEnabledNoneFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderNormalFunction(true, -1)));
	}
}
technique NormalBlendMaskEnabledColorFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderNormalFunction(true, 0)));
	}
}
technique NormalBlendMaskEnabledMaxGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderNormalFunction(true, 1)));
	}
}
technique NormalBlendMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderNormalFunction(true, 2)));
	}
}
technique NormalBlendMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderNormalFunction(true, 3)));
	}
}
technique NormalBlendMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderNormalFunction(true, 4)));
	}
}
technique NormalBlendMaskEnabledAverageGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderNormalFunction(true, 5)));
	}
}
technique NormalBlendMaskEnabledGreenGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderNormalFunction(true, 6)));
	}
}
technique NormalBlendMaskEnabledMedianGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderNormalFunction(true, 7)));
	}
}
technique NormalBlendMaskEnabledHueFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderNormalFunction(true, 8)));
	}
}
technique NormalBlendMaskEnabledSaturationFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderNormalFunction(true, 9)));
	}
}
technique NormalBlendMaskEnabledBrightnessFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderNormalFunction(true, 10)));
	}
}
technique NormalBlendMaskEnabledInvertFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderNormalFunction(true, 11)));
	}
}
technique NormalBlendMaskDisabledNoneFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderNormalFunction(false, -1)));
	}
}
technique NormalBlendMaskDisabledColorFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderNormalFunction(false, 0)));
	}
}
technique NormalBlendMaskDisabledMaxGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderNormalFunction(false, 1)));
	}
}
technique NormalBlendMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderNormalFunction(false, 2)));
	}
}
technique NormalBlendMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderNormalFunction(false, 3)));
	}
}
technique NormalBlendMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderNormalFunction(false, 4)));
	}
}
technique NormalBlendMaskDisabledAverageGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderNormalFunction(false, 5)));
	}
}
technique NormalBlendMaskDisabledGreenGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderNormalFunction(false, 6)));
	}
}
technique NormalBlendMaskDisabledMedianGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderNormalFunction(false, 7)));
	}
}
technique NormalBlendMaskDisabledHueFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderNormalFunction(false, 8)));
	}
}
technique NormalBlendMaskDisabledSaturationFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderNormalFunction(false, 9)));
	}
}
technique NormalBlendMaskDisabledBrightnessFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderNormalFunction(false, 10)));
	}
}
technique NormalBlendMaskDisabledInvertFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderNormalFunction(false, 11)));
	}
}
float4 PixelShaderMultiplyFunction(VertexShaderOutput input, uniform bool maskEnabled, uniform int filterType) : SV_TARGET{
	float4 destColor = LastRenderTargetTexture.SampleLevel(LinearSampler, input.LastRenderTargetTexCoord, 0);
	float4 srcColor = GetSrcColor(input.TexCoord, input.LastRenderTargetTexCoord, maskEnabled, filterType) * input.Color;
	return Multiply(destColor, srcColor);
}
technique MultiplyBlendMaskEnabledNoneFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderMultiplyFunction(true, -1)));
	}
}
technique MultiplyBlendMaskEnabledColorFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderMultiplyFunction(true, 0)));
	}
}
technique MultiplyBlendMaskEnabledMaxGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderMultiplyFunction(true, 1)));
	}
}
technique MultiplyBlendMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderMultiplyFunction(true, 2)));
	}
}
technique MultiplyBlendMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderMultiplyFunction(true, 3)));
	}
}
technique MultiplyBlendMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderMultiplyFunction(true, 4)));
	}
}
technique MultiplyBlendMaskEnabledAverageGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderMultiplyFunction(true, 5)));
	}
}
technique MultiplyBlendMaskEnabledGreenGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderMultiplyFunction(true, 6)));
	}
}
technique MultiplyBlendMaskEnabledMedianGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderMultiplyFunction(true, 7)));
	}
}
technique MultiplyBlendMaskEnabledHueFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderMultiplyFunction(true, 8)));
	}
}
technique MultiplyBlendMaskEnabledSaturationFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderMultiplyFunction(true, 9)));
	}
}
technique MultiplyBlendMaskEnabledBrightnessFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderMultiplyFunction(true, 10)));
	}
}
technique MultiplyBlendMaskEnabledInvertFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderMultiplyFunction(true, 11)));
	}
}
technique MultiplyBlendMaskDisabledNoneFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderMultiplyFunction(false, -1)));
	}
}
technique MultiplyBlendMaskDisabledColorFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderMultiplyFunction(false, 0)));
	}
}
technique MultiplyBlendMaskDisabledMaxGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderMultiplyFunction(false, 1)));
	}
}
technique MultiplyBlendMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderMultiplyFunction(false, 2)));
	}
}
technique MultiplyBlendMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderMultiplyFunction(false, 3)));
	}
}
technique MultiplyBlendMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderMultiplyFunction(false, 4)));
	}
}
technique MultiplyBlendMaskDisabledAverageGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderMultiplyFunction(false, 5)));
	}
}
technique MultiplyBlendMaskDisabledGreenGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderMultiplyFunction(false, 6)));
	}
}
technique MultiplyBlendMaskDisabledMedianGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderMultiplyFunction(false, 7)));
	}
}
technique MultiplyBlendMaskDisabledHueFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderMultiplyFunction(false, 8)));
	}
}
technique MultiplyBlendMaskDisabledSaturationFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderMultiplyFunction(false, 9)));
	}
}
technique MultiplyBlendMaskDisabledBrightnessFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderMultiplyFunction(false, 10)));
	}
}
technique MultiplyBlendMaskDisabledInvertFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderMultiplyFunction(false, 11)));
	}
}
float4 PixelShaderScreenFunction(VertexShaderOutput input, uniform bool maskEnabled, uniform int filterType) : SV_TARGET{
	float4 destColor = LastRenderTargetTexture.SampleLevel(LinearSampler, input.LastRenderTargetTexCoord, 0);
	float4 srcColor = GetSrcColor(input.TexCoord, input.LastRenderTargetTexCoord, maskEnabled, filterType) * input.Color;
	return Screen(destColor, srcColor);
}
technique ScreenBlendMaskEnabledNoneFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderScreenFunction(true, -1)));
	}
}
technique ScreenBlendMaskEnabledColorFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderScreenFunction(true, 0)));
	}
}
technique ScreenBlendMaskEnabledMaxGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderScreenFunction(true, 1)));
	}
}
technique ScreenBlendMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderScreenFunction(true, 2)));
	}
}
technique ScreenBlendMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderScreenFunction(true, 3)));
	}
}
technique ScreenBlendMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderScreenFunction(true, 4)));
	}
}
technique ScreenBlendMaskEnabledAverageGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderScreenFunction(true, 5)));
	}
}
technique ScreenBlendMaskEnabledGreenGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderScreenFunction(true, 6)));
	}
}
technique ScreenBlendMaskEnabledMedianGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderScreenFunction(true, 7)));
	}
}
technique ScreenBlendMaskEnabledHueFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderScreenFunction(true, 8)));
	}
}
technique ScreenBlendMaskEnabledSaturationFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderScreenFunction(true, 9)));
	}
}
technique ScreenBlendMaskEnabledBrightnessFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderScreenFunction(true, 10)));
	}
}
technique ScreenBlendMaskEnabledInvertFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderScreenFunction(true, 11)));
	}
}
technique ScreenBlendMaskDisabledNoneFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderScreenFunction(false, -1)));
	}
}
technique ScreenBlendMaskDisabledColorFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderScreenFunction(false, 0)));
	}
}
technique ScreenBlendMaskDisabledMaxGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderScreenFunction(false, 1)));
	}
}
technique ScreenBlendMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderScreenFunction(false, 2)));
	}
}
technique ScreenBlendMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderScreenFunction(false, 3)));
	}
}
technique ScreenBlendMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderScreenFunction(false, 4)));
	}
}
technique ScreenBlendMaskDisabledAverageGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderScreenFunction(false, 5)));
	}
}
technique ScreenBlendMaskDisabledGreenGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderScreenFunction(false, 6)));
	}
}
technique ScreenBlendMaskDisabledMedianGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderScreenFunction(false, 7)));
	}
}
technique ScreenBlendMaskDisabledHueFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderScreenFunction(false, 8)));
	}
}
technique ScreenBlendMaskDisabledSaturationFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderScreenFunction(false, 9)));
	}
}
technique ScreenBlendMaskDisabledBrightnessFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderScreenFunction(false, 10)));
	}
}
technique ScreenBlendMaskDisabledInvertFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderScreenFunction(false, 11)));
	}
}
float4 PixelShaderOverlayFunction(VertexShaderOutput input, uniform bool maskEnabled, uniform int filterType) : SV_TARGET{
	float4 destColor = LastRenderTargetTexture.SampleLevel(LinearSampler, input.LastRenderTargetTexCoord, 0);
	float4 srcColor = GetSrcColor(input.TexCoord, input.LastRenderTargetTexCoord, maskEnabled, filterType) * input.Color;
	return Overlay(destColor, srcColor);
}
technique OverlayBlendMaskEnabledNoneFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderOverlayFunction(true, -1)));
	}
}
technique OverlayBlendMaskEnabledColorFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderOverlayFunction(true, 0)));
	}
}
technique OverlayBlendMaskEnabledMaxGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderOverlayFunction(true, 1)));
	}
}
technique OverlayBlendMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderOverlayFunction(true, 2)));
	}
}
technique OverlayBlendMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderOverlayFunction(true, 3)));
	}
}
technique OverlayBlendMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderOverlayFunction(true, 4)));
	}
}
technique OverlayBlendMaskEnabledAverageGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderOverlayFunction(true, 5)));
	}
}
technique OverlayBlendMaskEnabledGreenGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderOverlayFunction(true, 6)));
	}
}
technique OverlayBlendMaskEnabledMedianGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderOverlayFunction(true, 7)));
	}
}
technique OverlayBlendMaskEnabledHueFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderOverlayFunction(true, 8)));
	}
}
technique OverlayBlendMaskEnabledSaturationFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderOverlayFunction(true, 9)));
	}
}
technique OverlayBlendMaskEnabledBrightnessFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderOverlayFunction(true, 10)));
	}
}
technique OverlayBlendMaskEnabledInvertFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderOverlayFunction(true, 11)));
	}
}
technique OverlayBlendMaskDisabledNoneFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderOverlayFunction(false, -1)));
	}
}
technique OverlayBlendMaskDisabledColorFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderOverlayFunction(false, 0)));
	}
}
technique OverlayBlendMaskDisabledMaxGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderOverlayFunction(false, 1)));
	}
}
technique OverlayBlendMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderOverlayFunction(false, 2)));
	}
}
technique OverlayBlendMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderOverlayFunction(false, 3)));
	}
}
technique OverlayBlendMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderOverlayFunction(false, 4)));
	}
}
technique OverlayBlendMaskDisabledAverageGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderOverlayFunction(false, 5)));
	}
}
technique OverlayBlendMaskDisabledGreenGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderOverlayFunction(false, 6)));
	}
}
technique OverlayBlendMaskDisabledMedianGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderOverlayFunction(false, 7)));
	}
}
technique OverlayBlendMaskDisabledHueFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderOverlayFunction(false, 8)));
	}
}
technique OverlayBlendMaskDisabledSaturationFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderOverlayFunction(false, 9)));
	}
}
technique OverlayBlendMaskDisabledBrightnessFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderOverlayFunction(false, 10)));
	}
}
technique OverlayBlendMaskDisabledInvertFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderOverlayFunction(false, 11)));
	}
}
float4 PixelShaderHardLightFunction(VertexShaderOutput input, uniform bool maskEnabled, uniform int filterType) : SV_TARGET{
	float4 destColor = LastRenderTargetTexture.SampleLevel(LinearSampler, input.LastRenderTargetTexCoord, 0);
	float4 srcColor = GetSrcColor(input.TexCoord, input.LastRenderTargetTexCoord, maskEnabled, filterType) * input.Color;
	return HardLight(destColor, srcColor);
}
technique HardLightBlendMaskEnabledNoneFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderHardLightFunction(true, -1)));
	}
}
technique HardLightBlendMaskEnabledColorFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderHardLightFunction(true, 0)));
	}
}
technique HardLightBlendMaskEnabledMaxGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderHardLightFunction(true, 1)));
	}
}
technique HardLightBlendMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderHardLightFunction(true, 2)));
	}
}
technique HardLightBlendMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderHardLightFunction(true, 3)));
	}
}
technique HardLightBlendMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderHardLightFunction(true, 4)));
	}
}
technique HardLightBlendMaskEnabledAverageGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderHardLightFunction(true, 5)));
	}
}
technique HardLightBlendMaskEnabledGreenGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderHardLightFunction(true, 6)));
	}
}
technique HardLightBlendMaskEnabledMedianGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderHardLightFunction(true, 7)));
	}
}
technique HardLightBlendMaskEnabledHueFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderHardLightFunction(true, 8)));
	}
}
technique HardLightBlendMaskEnabledSaturationFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderHardLightFunction(true, 9)));
	}
}
technique HardLightBlendMaskEnabledBrightnessFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderHardLightFunction(true, 10)));
	}
}
technique HardLightBlendMaskEnabledInvertFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderHardLightFunction(true, 11)));
	}
}
technique HardLightBlendMaskDisabledNoneFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderHardLightFunction(false, -1)));
	}
}
technique HardLightBlendMaskDisabledColorFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderHardLightFunction(false, 0)));
	}
}
technique HardLightBlendMaskDisabledMaxGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderHardLightFunction(false, 1)));
	}
}
technique HardLightBlendMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderHardLightFunction(false, 2)));
	}
}
technique HardLightBlendMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderHardLightFunction(false, 3)));
	}
}
technique HardLightBlendMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderHardLightFunction(false, 4)));
	}
}
technique HardLightBlendMaskDisabledAverageGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderHardLightFunction(false, 5)));
	}
}
technique HardLightBlendMaskDisabledGreenGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderHardLightFunction(false, 6)));
	}
}
technique HardLightBlendMaskDisabledMedianGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderHardLightFunction(false, 7)));
	}
}
technique HardLightBlendMaskDisabledHueFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderHardLightFunction(false, 8)));
	}
}
technique HardLightBlendMaskDisabledSaturationFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderHardLightFunction(false, 9)));
	}
}
technique HardLightBlendMaskDisabledBrightnessFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderHardLightFunction(false, 10)));
	}
}
technique HardLightBlendMaskDisabledInvertFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderHardLightFunction(false, 11)));
	}
}
float4 PixelShaderColorDodgeFunction(VertexShaderOutput input, uniform bool maskEnabled, uniform int filterType) : SV_TARGET{
	float4 destColor = LastRenderTargetTexture.SampleLevel(LinearSampler, input.LastRenderTargetTexCoord, 0);
	float4 srcColor = GetSrcColor(input.TexCoord, input.LastRenderTargetTexCoord, maskEnabled, filterType) * input.Color;
	return ColorDodge(destColor, srcColor);
}
technique ColorDodgeBlendMaskEnabledNoneFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderColorDodgeFunction(true, -1)));
	}
}
technique ColorDodgeBlendMaskEnabledColorFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderColorDodgeFunction(true, 0)));
	}
}
technique ColorDodgeBlendMaskEnabledMaxGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderColorDodgeFunction(true, 1)));
	}
}
technique ColorDodgeBlendMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderColorDodgeFunction(true, 2)));
	}
}
technique ColorDodgeBlendMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderColorDodgeFunction(true, 3)));
	}
}
technique ColorDodgeBlendMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderColorDodgeFunction(true, 4)));
	}
}
technique ColorDodgeBlendMaskEnabledAverageGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderColorDodgeFunction(true, 5)));
	}
}
technique ColorDodgeBlendMaskEnabledGreenGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderColorDodgeFunction(true, 6)));
	}
}
technique ColorDodgeBlendMaskEnabledMedianGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderColorDodgeFunction(true, 7)));
	}
}
technique ColorDodgeBlendMaskEnabledHueFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderColorDodgeFunction(true, 8)));
	}
}
technique ColorDodgeBlendMaskEnabledSaturationFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderColorDodgeFunction(true, 9)));
	}
}
technique ColorDodgeBlendMaskEnabledBrightnessFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderColorDodgeFunction(true, 10)));
	}
}
technique ColorDodgeBlendMaskEnabledInvertFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderColorDodgeFunction(true, 11)));
	}
}
technique ColorDodgeBlendMaskDisabledNoneFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderColorDodgeFunction(false, -1)));
	}
}
technique ColorDodgeBlendMaskDisabledColorFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderColorDodgeFunction(false, 0)));
	}
}
technique ColorDodgeBlendMaskDisabledMaxGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderColorDodgeFunction(false, 1)));
	}
}
technique ColorDodgeBlendMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderColorDodgeFunction(false, 2)));
	}
}
technique ColorDodgeBlendMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderColorDodgeFunction(false, 3)));
	}
}
technique ColorDodgeBlendMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderColorDodgeFunction(false, 4)));
	}
}
technique ColorDodgeBlendMaskDisabledAverageGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderColorDodgeFunction(false, 5)));
	}
}
technique ColorDodgeBlendMaskDisabledGreenGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderColorDodgeFunction(false, 6)));
	}
}
technique ColorDodgeBlendMaskDisabledMedianGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderColorDodgeFunction(false, 7)));
	}
}
technique ColorDodgeBlendMaskDisabledHueFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderColorDodgeFunction(false, 8)));
	}
}
technique ColorDodgeBlendMaskDisabledSaturationFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderColorDodgeFunction(false, 9)));
	}
}
technique ColorDodgeBlendMaskDisabledBrightnessFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderColorDodgeFunction(false, 10)));
	}
}
technique ColorDodgeBlendMaskDisabledInvertFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderColorDodgeFunction(false, 11)));
	}
}
float4 PixelShaderLinearDodgeFunction(VertexShaderOutput input, uniform bool maskEnabled, uniform int filterType) : SV_TARGET{
	float4 destColor = LastRenderTargetTexture.SampleLevel(LinearSampler, input.LastRenderTargetTexCoord, 0);
	float4 srcColor = GetSrcColor(input.TexCoord, input.LastRenderTargetTexCoord, maskEnabled, filterType) * input.Color;
	return LinearDodge(destColor, srcColor);
}
technique LinearDodgeBlendMaskEnabledNoneFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderLinearDodgeFunction(true, -1)));
	}
}
technique LinearDodgeBlendMaskEnabledColorFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderLinearDodgeFunction(true, 0)));
	}
}
technique LinearDodgeBlendMaskEnabledMaxGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderLinearDodgeFunction(true, 1)));
	}
}
technique LinearDodgeBlendMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderLinearDodgeFunction(true, 2)));
	}
}
technique LinearDodgeBlendMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderLinearDodgeFunction(true, 3)));
	}
}
technique LinearDodgeBlendMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderLinearDodgeFunction(true, 4)));
	}
}
technique LinearDodgeBlendMaskEnabledAverageGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderLinearDodgeFunction(true, 5)));
	}
}
technique LinearDodgeBlendMaskEnabledGreenGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderLinearDodgeFunction(true, 6)));
	}
}
technique LinearDodgeBlendMaskEnabledMedianGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderLinearDodgeFunction(true, 7)));
	}
}
technique LinearDodgeBlendMaskEnabledHueFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderLinearDodgeFunction(true, 8)));
	}
}
technique LinearDodgeBlendMaskEnabledSaturationFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderLinearDodgeFunction(true, 9)));
	}
}
technique LinearDodgeBlendMaskEnabledBrightnessFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderLinearDodgeFunction(true, 10)));
	}
}
technique LinearDodgeBlendMaskEnabledInvertFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderLinearDodgeFunction(true, 11)));
	}
}
technique LinearDodgeBlendMaskDisabledNoneFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderLinearDodgeFunction(false, -1)));
	}
}
technique LinearDodgeBlendMaskDisabledColorFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderLinearDodgeFunction(false, 0)));
	}
}
technique LinearDodgeBlendMaskDisabledMaxGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderLinearDodgeFunction(false, 1)));
	}
}
technique LinearDodgeBlendMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderLinearDodgeFunction(false, 2)));
	}
}
technique LinearDodgeBlendMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderLinearDodgeFunction(false, 3)));
	}
}
technique LinearDodgeBlendMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderLinearDodgeFunction(false, 4)));
	}
}
technique LinearDodgeBlendMaskDisabledAverageGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderLinearDodgeFunction(false, 5)));
	}
}
technique LinearDodgeBlendMaskDisabledGreenGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderLinearDodgeFunction(false, 6)));
	}
}
technique LinearDodgeBlendMaskDisabledMedianGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderLinearDodgeFunction(false, 7)));
	}
}
technique LinearDodgeBlendMaskDisabledHueFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderLinearDodgeFunction(false, 8)));
	}
}
technique LinearDodgeBlendMaskDisabledSaturationFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderLinearDodgeFunction(false, 9)));
	}
}
technique LinearDodgeBlendMaskDisabledBrightnessFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderLinearDodgeFunction(false, 10)));
	}
}
technique LinearDodgeBlendMaskDisabledInvertFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderLinearDodgeFunction(false, 11)));
	}
}
float4 PixelShaderDifferenceFunction(VertexShaderOutput input, uniform bool maskEnabled, uniform int filterType) : SV_TARGET{
	float4 destColor = LastRenderTargetTexture.SampleLevel(LinearSampler, input.LastRenderTargetTexCoord, 0);
	float4 srcColor = GetSrcColor(input.TexCoord, input.LastRenderTargetTexCoord, maskEnabled, filterType) * input.Color;
	return Difference(destColor, srcColor);
}
technique DifferenceBlendMaskEnabledNoneFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderDifferenceFunction(true, -1)));
	}
}
technique DifferenceBlendMaskEnabledColorFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderDifferenceFunction(true, 0)));
	}
}
technique DifferenceBlendMaskEnabledMaxGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderDifferenceFunction(true, 1)));
	}
}
technique DifferenceBlendMaskEnabledMiddleGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderDifferenceFunction(true, 2)));
	}
}
technique DifferenceBlendMaskEnabledNTSCGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderDifferenceFunction(true, 3)));
	}
}
technique DifferenceBlendMaskEnabledHDTVGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderDifferenceFunction(true, 4)));
	}
}
technique DifferenceBlendMaskEnabledAverageGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderDifferenceFunction(true, 5)));
	}
}
technique DifferenceBlendMaskEnabledGreenGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderDifferenceFunction(true, 6)));
	}
}
technique DifferenceBlendMaskEnabledMedianGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderDifferenceFunction(true, 7)));
	}
}
technique DifferenceBlendMaskEnabledHueFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderDifferenceFunction(true, 8)));
	}
}
technique DifferenceBlendMaskEnabledSaturationFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderDifferenceFunction(true, 9)));
	}
}
technique DifferenceBlendMaskEnabledBrightnessFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderDifferenceFunction(true, 10)));
	}
}
technique DifferenceBlendMaskEnabledInvertFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderDifferenceFunction(true, 11)));
	}
}
technique DifferenceBlendMaskDisabledNoneFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderDifferenceFunction(false, -1)));
	}
}
technique DifferenceBlendMaskDisabledColorFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderDifferenceFunction(false, 0)));
	}
}
technique DifferenceBlendMaskDisabledMaxGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderDifferenceFunction(false, 1)));
	}
}
technique DifferenceBlendMaskDisabledMiddleGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderDifferenceFunction(false, 2)));
	}
}
technique DifferenceBlendMaskDisabledNTSCGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderDifferenceFunction(false, 3)));
	}
}
technique DifferenceBlendMaskDisabledHDTVGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderDifferenceFunction(false, 4)));
	}
}
technique DifferenceBlendMaskDisabledAverageGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderDifferenceFunction(false, 5)));
	}
}
technique DifferenceBlendMaskDisabledGreenGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderDifferenceFunction(false, 6)));
	}
}
technique DifferenceBlendMaskDisabledMedianGrayScaleFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderDifferenceFunction(false, 7)));
	}
}
technique DifferenceBlendMaskDisabledHueFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderDifferenceFunction(false, 8)));
	}
}
technique DifferenceBlendMaskDisabledSaturationFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderDifferenceFunction(false, 9)));
	}
}
technique DifferenceBlendMaskDisabledBrightnessFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderDifferenceFunction(false, 10)));
	}
}
technique DifferenceBlendMaskDisabledInvertFilter{
	pass p0 {
        SetGeometryShader( 0 );
		SetVertexShader(CompileShader(vs_5_0, VertexShaderFunction()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderDifferenceFunction(false, 11)));
	}
}