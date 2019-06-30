float4 ColorFilter(float4 color, FilterInformation filterInfo, uniform bool paEnabled)
{
	return float4(
		color.r * filterInfo.Arg2.g + color.r * filterInfo.Arg1.r * filterInfo.Arg2.r,
		color.g * filterInfo.Arg2.g + color.g * filterInfo.Arg1.g * filterInfo.Arg2.r,
		color.b * filterInfo.Arg2.g + color.b * filterInfo.Arg1.b * filterInfo.Arg2.r,
		color.a * filterInfo.Arg2.g + color.a * filterInfo.Arg1.a * filterInfo.Arg2.r);
}

float4 MaxGrayScaleFilter(float4 color, FilterInformation filterInfo, uniform bool paEnabled)
{
	float c = max(max(color.r, color.g), color.b) * filterInfo.Arg1.r;
	return float4(
		color.r * filterInfo.Arg1.g + c,
		color.g * filterInfo.Arg1.g + c,
		color.b * filterInfo.Arg1.g + c,
		color.a);
}

float4 MiddleGrayScaleFilter(float4 color, FilterInformation filterInfo, uniform bool paEnabled)
{
	float c = (max(max(color.r, color.g), color.b) + min(min(color.r, color.g), color.b)) / 2 * filterInfo.Arg1.r;
	return float4(
		color.r * filterInfo.Arg1.g + c,
		color.g * filterInfo.Arg1.g + c,
		color.b * filterInfo.Arg1.g + c,
		color.a);
}

float4 NTSCGrayScaleFilter(float4 color, FilterInformation filterInfo, uniform bool paEnabled)
{
	float c = (0.298912 * color.r + 0.586611 * color.g + 0.114478 * color.b) * filterInfo.Arg1.r;
	return float4(
		color.r * filterInfo.Arg1.g + c,
		color.g * filterInfo.Arg1.g + c,
		color.b * filterInfo.Arg1.g + c,
		color.a);
}

float4 HDTVGrayScaleFilter(float4 color, FilterInformation filterInfo, uniform bool paEnabled) {
	float c = pow((pow(color.r, filterInfo.Arg1.b) * 0.222015 + pow(color.g, filterInfo.Arg1.b) * 0.706655 + pow(color.b, filterInfo.Arg1.b) * 0.071330), 1 / filterInfo.Arg1.b);
	return float4(
		color.r * filterInfo.Arg1.g + c,
		color.g * filterInfo.Arg1.g + c,
		color.b * filterInfo.Arg1.g + c,
		color.a);
}

float4 AverageGrayScaleFilter(float4 color, FilterInformation filterInfo, uniform bool paEnabled) {
	float c = (color.r + color.g + color.b) / 3 * filterInfo.Arg1.r;
	return float4(
		color.r * filterInfo.Arg1.g + c,
		color.g * filterInfo.Arg1.g + c,
		color.b * filterInfo.Arg1.g + c,
		color.a);
}

float4 GreenGrayScaleFilter(float4 color, FilterInformation filterInfo, uniform bool paEnabled) {
	float c = color.g * filterInfo.Arg1.r;
	return float4(
		color.r * filterInfo.Arg1.g + c,
		color.g * filterInfo.Arg1.g + c,
		color.b * filterInfo.Arg1.g + c,
		color.a);
}

float4 MedianGrayScaleFilter(float4 color, FilterInformation filterInfo, uniform bool paEnabled) {
	float c = clamp(color.r, color.g, color.b) * filterInfo.Arg1.r;
	return float4(
		color.r * filterInfo.Arg1.g + c,
		color.g * filterInfo.Arg1.g + c,
		color.b * filterInfo.Arg1.g + c,
		color.a);
}

float4 GetHSV(float4 color) {
	float _max = max(max(color.r, color.g), color.b);
	float _min = min(min(color.r, color.g), color.b);
	float h = _max - _min;
	if (h > 0.0) {
		if (_max == color.r) {
			h = (color.g - color.b) / h;
			if (h < 0) {
				h += 6.0;
			}
		}
		else if (_max == color.g) {
			h = 2.0 + (color.b - color.r) / h;
		}
		else {
			h = 4.0 + (color.r - color.g) / h;
		}
	}
	h /= 6.0;
	float s = _max - _min;
	if (_max != 0) {
		s /= _max;
	}
	float v = _max;
	return float4(h, s, v, color.a);
}

float4 GetRGB(float4 hsv) {
	float r = hsv.b;
	float g = hsv.b;
	float b = hsv.b;
	if (hsv.g > 0.0f) {
		float h = hsv.r;
		h *= 6.0f;
		int i = (int)h;
		float f = h - i;
		if (i == 1) {
			r *= 1 - hsv.g* f;
			b *= 1 - hsv.g;
		}
		else if (i == 2) {
			r *= 1 - hsv.g;
			b *= 1 - hsv.g* (1 - f);
		}
		else if (i == 3) {
			r *= 1 - hsv.g;
			g *= 1 - hsv.g * f;
		}
		else if (i == 4) {
			r *= 1 - hsv.g* (1 - f);
			g *= 1 - hsv.g;
		}
		else if (i == 5) {
			g *= 1 - hsv.g;
			b *= 1 - hsv.g* f;
		}
		else {
			// 0
			g *= 1 - hsv.g*(1 - f);
			b *= 1 - hsv.g;
		}
	}
	return float4(r, g, b, hsv.a);
}

float4 HueFilter(float4 color, FilterInformation filterInfo, uniform bool paEnabled) {
	if (paEnabled) {
		float4 hsv = GetHSV(float4(color.rgb / color.a, color.a));
		hsv.r = fmod(hsv.r + filterInfo.Arg1.b, 1);
		float4 newColor = GetRGB(hsv);
		return float4(
			(color.r * filterInfo.Arg1.g + newColor.r * filterInfo.Arg1.r) * color.a,
			(color.g * filterInfo.Arg1.g + newColor.g * filterInfo.Arg1.r) * color.a,
			(color.b * filterInfo.Arg1.g + newColor.b * filterInfo.Arg1.r) * color.a,
			color.a);
	}
	else {
		float4 hsv = GetHSV(color);
		hsv.r = fmod(hsv.r + filterInfo.Arg1.b, 1);
		float4 newColor = GetRGB(hsv);
		return float4(
			color.r * filterInfo.Arg1.g + newColor.r * filterInfo.Arg1.r,
			color.g * filterInfo.Arg1.g + newColor.g * filterInfo.Arg1.r,
			color.b * filterInfo.Arg1.g + newColor.b * filterInfo.Arg1.r,
			color.a);
	}
}

float4 SaturationFilter(float4 color, FilterInformation filterInfo, uniform bool paEnabled) {
	if (paEnabled) {
		float4 hsv = GetHSV(float4(color.rgb / color.a, color.a));
		hsv.g = min(1, hsv.g * filterInfo.Arg1.b);
		float4 newColor = GetRGB(hsv);
		return float4(
			(color.r * filterInfo.Arg1.g + newColor.r * filterInfo.Arg1.r) * color.a,
			(color.g * filterInfo.Arg1.g + newColor.g * filterInfo.Arg1.r) * color.a,
			(color.b * filterInfo.Arg1.g + newColor.b * filterInfo.Arg1.r) * color.a,
			color.a);
	}
	else {
		float4 hsv = GetHSV(color);
		hsv.g = min(1, hsv.g * filterInfo.Arg1.b);
		float4 newColor = GetRGB(hsv);
		return float4(
			color.r * filterInfo.Arg1.g + newColor.r * filterInfo.Arg1.r,
			color.g * filterInfo.Arg1.g + newColor.g * filterInfo.Arg1.r,
			color.b * filterInfo.Arg1.g + newColor.b * filterInfo.Arg1.r,
			color.a);
	}
}

float4 BrightnessFilter(float4 color, FilterInformation filterInfo, uniform bool paEnabled) {
	if (paEnabled) {
		float4 hsv = GetHSV(float4(color.rgb / color.a, color.a));
		hsv.b = min(1, hsv.b * filterInfo.Arg1.b);
		float4 newColor = GetRGB(hsv);
		return float4(
			(color.r * filterInfo.Arg1.g + newColor.r * filterInfo.Arg1.r) * color.a,
			(color.g * filterInfo.Arg1.g + newColor.g * filterInfo.Arg1.r) * color.a,
			(color.b * filterInfo.Arg1.g + newColor.b * filterInfo.Arg1.r) * color.a,
			color.a);
	}
	else {
		float4 hsv = GetHSV(color);
		hsv.b = min(1, hsv.b * filterInfo.Arg1.b);
		float4 newColor = GetRGB(hsv);
		return float4(
			color.r * filterInfo.Arg1.g + newColor.r * filterInfo.Arg1.r,
			color.g * filterInfo.Arg1.g + newColor.g * filterInfo.Arg1.r,
			color.b * filterInfo.Arg1.g + newColor.b * filterInfo.Arg1.r,
			color.a);
	}
}

float4 InvertFilter(float4 color, FilterInformation filterInfo, uniform bool paEnabled) {
	if (paEnabled) {
		if (color.a == 0) {
			return color;
		}
		float4 tempColor = float4(color.rgb / color.a, color.a);
		return float4(
			(tempColor.r * filterInfo.Arg1.g + (1 - tempColor.r) * filterInfo.Arg1.r) * color.a,
			(tempColor.g * filterInfo.Arg1.g + (1 - tempColor.g) * filterInfo.Arg1.r) * color.a,
			(tempColor.b * filterInfo.Arg1.g + (1 - tempColor.b) * filterInfo.Arg1.r) * color.a,
			color.a);
	}
	else {
		return float4(
			color.r * filterInfo.Arg1.g + (1 - color.r) * filterInfo.Arg1.r,
			color.g * filterInfo.Arg1.g + (1 - color.g) * filterInfo.Arg1.r,
			color.b * filterInfo.Arg1.g + (1 - color.b) * filterInfo.Arg1.r,
			color.a);
	}
}