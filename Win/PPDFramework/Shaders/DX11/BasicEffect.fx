#include "Common.fx"

cbuffer Parameters : register(b0)
{
    float4 OverlayColor;
    float4x4 Projection;
};

Texture2D Texture : register(t0);

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

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
    output.Position = mul(input.Position, Projection);
	output.TexCoord = input.TexCoord;
	output.Color = input.Color;
	return output;
}

float4 GetSrcColor(float2 texCoord, uniform bool paEnabled) {
	float4 srcColor = Texture.SampleLevel(LinearSampler, texCoord, 0);
    if(paEnabled){
        srcColor.rgb = srcColor.rgb * (1 - OverlayColor.a) + OverlayColor.rgb * OverlayColor.a * srcColor.a;
    }else{
        srcColor.rgb = srcColor.rgb * (1 - OverlayColor.a) + OverlayColor.rgb * OverlayColor.a;
    }
	return srcColor;
}

float4 PixelShaderFunction(VertexShaderOutput input, uniform bool paEnabled) : SV_TARGET{
    float4 srcColor = GetSrcColor(input.TexCoord, paEnabled);
    if(paEnabled){
        srcColor.rgb *= input.Color.a;
    }
    return srcColor * input.Color;
}

technique11 BasicEffectPAEnabled{
	pass p0 {
		SetBlendState(PremultipliedAlphaBlending, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderFunction(true)));
	}
}

technique11 BasicEffectPADisabled{
	pass p0 {
		SetBlendState(AlphaBlending, float4(0, 0, 0, 0), 0xFFFFFFFF);
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderFunction(false)));
	}
}