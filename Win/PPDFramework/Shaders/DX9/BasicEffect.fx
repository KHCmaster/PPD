float4 OverlayColor;
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

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
    output.Position = mul(input.Position, Projection);
	output.TexCoord = input.TexCoord;
	output.Color = input.Color;
	return output;
}

float4 GetSrcColor(float2 texCoord, uniform bool paEnabled) {
	float4 srcColor = tex2D(TextureSampler, texCoord);
    if(paEnabled){
        srcColor.rgb = srcColor.rgb * (1 - OverlayColor.a) + OverlayColor.rgb * OverlayColor.a * srcColor.a;
    }else{
        srcColor.rgb = srcColor.rgb * (1 - OverlayColor.a) + OverlayColor.rgb * OverlayColor.a;
    }
	return srcColor;
}

float4 PixelShaderFunction(VertexShaderOutput input, uniform bool paEnabled) : COLOR0{
    float4 srcColor = GetSrcColor(input.TexCoord, paEnabled);
    if(paEnabled){
        srcColor.rgb *= input.Color.a;
    }
    return srcColor * input.Color;
}


technique BasicEffectPAEnabled{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = true;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction(true);
	}
}
technique BasicEffectPADisabled{
	pass p0 {
		CullMode = none;
        AlphaBlendEnable = true;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction(false);
	}
}