#pragma pack_matrix(row_major)

SamplerState LinearSampler
{
	Filter = MIN_MAG_LINEAR_MIP_POINT;
	AddressU = Clamp;
	AddressV = Clamp;
};

BlendState NoBlend
{
	BlendEnable[0] = FALSE;
	RenderTargetWriteMask[0] = 0x0F;
};

BlendState AlphaBlending
{
	BlendEnable[0] = TRUE;
	SrcBlend = SRC_ALPHA;
	DestBlend = INV_SRC_ALPHA;
	BlendOp = ADD;
	SrcBlendAlpha = SRC_ALPHA;
	DestBlendAlpha = INV_SRC_ALPHA;
	BlendOpAlpha = ADD;
	RenderTargetWriteMask[0] = 0x0F;
};

BlendState PremultipliedAlphaBlending
{
	BlendEnable[0] = TRUE;
	SrcBlend = ONE;
	DestBlend = INV_SRC_ALPHA;
	BlendOp = ADD;
	SrcBlendAlpha = ONE;
	DestBlendAlpha = INV_SRC_ALPHA;
	BlendOpAlpha = ADD;
	RenderTargetWriteMask[0] = 0x0F;
};
