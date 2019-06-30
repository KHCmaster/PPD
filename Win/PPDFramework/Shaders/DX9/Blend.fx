float4 Normal(float4 destColor, float4 srcColor) {
	float invSrcAlpha = 1 - srcColor.a;
	float resultAlpha = srcColor.a + invSrcAlpha * destColor.a;
	if (resultAlpha == 0) {
		return float4(0, 0, 0, 0);
	}
	return float4((srcColor.rgb * srcColor.a + destColor.rgb * destColor.a * invSrcAlpha) / resultAlpha, resultAlpha);
}

float4 Multiply(float4 destColor, float4 srcColor) {
	float invSrcAlpha = 1 - srcColor.a;
	float resultAlpha = srcColor.a + invSrcAlpha * destColor.a;
	if (resultAlpha == 0) {
		return float4(0, 0, 0, 0);
	}
	return float4((srcColor.rgb * destColor.rgb * srcColor.a + destColor.rgb * destColor.a * invSrcAlpha) / resultAlpha, resultAlpha);
}

float4 Screen(float4 destColor, float4 srcColor) {
	float invSrcAlpha = 1 - srcColor.a;
	float resultAlpha = srcColor.a + invSrcAlpha * destColor.a;
	if (resultAlpha == 0) {
		return float4(0, 0, 0, 0);
	}	
	return float4(((1 - (1 - srcColor.rgb) * (1 - destColor.rgb)) * srcColor.a + destColor.rgb * destColor.a * invSrcAlpha) / resultAlpha, resultAlpha);
}

float4 Overlay(float4 destColor, float4 srcColor) {
	float invSrcAlpha = 1 - srcColor.a;
	float resultAlpha = srcColor.a + invSrcAlpha * destColor.a;
	if (resultAlpha == 0) {
		return float4(0, 0, 0, 0);
	}
	float4 tempColor = float4(
		srcColor.r < 0.5 ? (2 * srcColor.r * destColor.r) : (1 - 2 * (1 - srcColor.r)*(1 - destColor.r)),
		srcColor.g < 0.5 ? (2 * srcColor.g * destColor.g) : (1 - 2 * (1 - srcColor.g)*(1 - destColor.g)),
		srcColor.b < 0.5 ? (2 * srcColor.b * destColor.b) : (1 - 2 * (1 - srcColor.b)*(1 - destColor.b)),
		1.0);
	return float4((tempColor.rgb * srcColor.a + destColor.rgb * destColor.a * invSrcAlpha) / resultAlpha, resultAlpha);
}

float4 HardLight(float4 destColor, float4 srcColor) {
	float invSrcAlpha = 1 - srcColor.a;
	float resultAlpha = srcColor.a + invSrcAlpha * destColor.a;
	if (resultAlpha == 0) {
		return float4(0, 0, 0, 0);
	}
	float4 tempColor = float4(
		destColor.r < 0.5 ? (2 * srcColor.r * destColor.r) : (1 - 2 * (1 - srcColor.r)*(1 - destColor.r)),
		destColor.g < 0.5 ? (2 * srcColor.g * destColor.g) : (1 - 2 * (1 - srcColor.g)*(1 - destColor.g)),
		destColor.b < 0.5 ? (2 * srcColor.b * destColor.b) : (1 - 2 * (1 - srcColor.b)*(1 - destColor.b)),
		1.0);
	return float4((tempColor.rgb * srcColor.a + destColor.rgb * destColor.a * invSrcAlpha) / resultAlpha, resultAlpha);
}

float4 ColorDodge(float4 destColor, float4 srcColor) {
	float invSrcAlpha = 1 - srcColor.a;
	float resultAlpha = srcColor.a + invSrcAlpha * destColor.a;
	if (resultAlpha == 0) {
		return float4(0, 0, 0, 0);
	}
	float4 tempColor = float4(
		srcColor.r != 1 ? clamp(destColor.r / (1 - srcColor.r), 0, 1) : destColor.r,
		srcColor.g != 1 ? clamp(destColor.g / (1 - srcColor.g), 0, 1) : destColor.g,
		srcColor.b != 1 ? clamp(destColor.b / (1 - srcColor.b), 0, 1) : destColor.b,
		1.0);
	return float4((tempColor.rgb * srcColor.a + destColor.rgb * destColor.a * invSrcAlpha) / resultAlpha, resultAlpha);
}

float4 LinearDodge(float4 destColor, float4 srcColor) {
	float invSrcAlpha = 1 - srcColor.a;
	float resultAlpha = srcColor.a + invSrcAlpha * destColor.a;
	if (resultAlpha == 0) {
		return float4(0, 0, 0, 0);
	}
	float4 tempColor = float4(
		clamp(srcColor.r + destColor.r, 0, 1),
		clamp(srcColor.g + destColor.g, 0, 1),
		clamp(srcColor.b + destColor.b, 0, 1),
		1.0);
	return float4((tempColor.rgb * srcColor.a + destColor.rgb * destColor.a * invSrcAlpha) / resultAlpha, resultAlpha);
}

float4 Difference(float4 destColor, float4 srcColor) {
	float invSrcAlpha = 1 - srcColor.a;
	float resultAlpha = srcColor.a + invSrcAlpha * destColor.a;
	if (resultAlpha == 0) {
		return float4(0, 0, 0, 0);
	}
	float4 tempColor = float4(
		abs(srcColor.rgb - destColor.rgb),
		1.0);
	return float4((tempColor.rgb * srcColor.a + destColor.rgb * destColor.a * invSrcAlpha) / resultAlpha, resultAlpha);
}