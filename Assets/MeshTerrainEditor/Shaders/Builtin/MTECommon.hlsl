#ifndef MTE_COMMON_HLSL_INCLUDED
#define MTE_COMMON_HLSL_INCLUDED

#include "UnityGBuffer.cginc"

#ifdef MTE_STANDARD_SHADER
#define Output SurfaceOutputStandard
#else
#define Output SurfaceOutput
#endif

void MTE_SplatmapFinalColor(Input IN, Output o, inout fixed4 color)
{
	color *= o.Alpha;
	UNITY_APPLY_FOG(IN.fogCoord, color);
}

void MTE_SplatmapFinalPrepass(Input IN, Output o, inout fixed4 normalSpec)
{
	normalSpec *= o.Alpha;
}

void MTE_SplatmapFinalGBuffer(Input IN, Output o, inout half4 outGBuffer0, inout half4 outGBuffer1, inout half4 outGBuffer2, inout half4 emission)
{
    UnityStandardDataApplyWeightToGbuffer(outGBuffer0, outGBuffer1, outGBuffer2, o.Alpha);
	emission *= o.Alpha;
}

float3 MTE_NormalIntensity_fixed(float3 normal, float Strength)
{
    return lerp(normal, fixed3(0,0,1), - Strength + 1.0);
}

#endif // MTE_COMMON_HLSL_INCLUDED