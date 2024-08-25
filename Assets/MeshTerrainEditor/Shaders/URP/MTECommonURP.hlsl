#ifndef MTECommonURP_INCLUDED
#define MTECommonURP_INCLUDED

void Max4WeightLayer(
    float4 splat_control_0,
    float4 splat_control_1,
    float4 splat_control_2,
    out float4 weights, out half4 indexes)
{
    float control[12];
	control[ 0] = splat_control_0.r;
	control[ 1] = splat_control_0.g;
	control[ 2] = splat_control_0.b;
	control[ 3] = splat_control_0.a;
	control[ 4] = splat_control_1.r;
	control[ 5] = splat_control_1.g;
	control[ 6] = splat_control_1.b;
	control[ 7] = splat_control_1.a;
	control[ 8] = splat_control_2.r;
	control[ 9] = splat_control_2.g;
	control[10] = splat_control_2.b;
	control[11] = splat_control_2.a;
		
	weights = 0;
	indexes = 0;

    int i = 0;
    for (i = 0; i < 12; ++i)
    {
       float w = control[i];
       if (w >= weights[0])
       {
          weights[3] = weights[2];
          indexes[3] = indexes[2];
          weights[2] = weights[1];
          indexes[2] = indexes[1];
          weights[1] = weights[0];
          indexes[1] = indexes[0];
          weights[0] = w;
          indexes[0] = i;
       }
       else if (w >= weights[1])
       {
          weights[3] = weights[2];
          indexes[3] = indexes[2];
          weights[2] = weights[1];
          indexes[2] = indexes[1];
          weights[1] = w;
          indexes[1] = i;
       }
       else if (w >= weights[2])
       {
          weights[3] = weights[2];
          indexes[3] = indexes[2];
          weights[2] = w;
          indexes[2] = i;
       }
       else if (w >= weights[3])
       {
          weights[3] = w;
          indexes[3] = i;
       }
	   //less weighted layers are ignored: not considered in weighted-blending
    }
    
    //re-normalize max 4 weights
    weights /= (weights[0] + weights[1] + weights[2] + weights[3]);
}

// Reoriented Normal Mapping
// http://blog.selfshadow.com/publications/blending-in-detail/
// Altered to take normals (-1 to 1 ranges) rather than unsigned normal maps (0 to 1 ranges)
half3 blend_rnm(half3 n1, half3 n2)
{
    n1.z += 1;
    n2.xy = -n2.xy;

    return n1 * dot(n1, n2) / n1.z - n2;
}

// Restore tangent space normal.xyz from normal in [0, 1].
float3 RestoreNormal(float2 normalXY, float intensity)
{
	//first scale to [-1, 1]: now normalXY.xy is in tangent space
	normalXY = normalXY * 2 - 1;
	//reconstruct normal.z from normal.xy
    float reconstructZ = sqrt(1.0 - saturate(dot(normalXY, normalXY)));
    float3 normal = float3(normalXY, reconstructZ);
    normal = normalize(normal);
#ifdef ENABLE_NORMAL_INTENSITY
	normal = lerp(normal, float3(0,0,1), - intensity + 1.0);//normal scaled by intensity
#endif
	return normal;
}

// Scale tangent space normal from [-1, 1] to [0, 1], to be represented as normal map texel color
half3 ScaleNormalTo01(half3 normalInTangentSpace)
{
	return normalInTangentSpace = normalInTangentSpace * 0.5 + 0.5;
}

float MipMapLevel(float2 uv, float uvScale, float textureSize)
{
    float2 dx = ddx(uv);
    float2 dy = ddy(uv);
    float md = max(dot(dx, dx), dot(dy, dy)) * textureSize * textureSize * uvScale * uvScale;
    float mipLevel = max(0.5 * log2(md) - 1, 0);
    return mipLevel;
}

float4 MTE_SampleTextureArray(Texture2DArray array, SamplerState SS, float textureSize, int layerIndex, float2 uv, float LayerUVScales[12], float UVScale)
{
#ifdef ENABLE_LAYER_UV_SCALE
	float layerUVScale = LayerUVScales[layerIndex];
	float mipLevel = MipMapLevel(uv, layerUVScale, textureSize);
	return SAMPLE_TEXTURE2D_ARRAY_LOD(array, SS, uv * layerUVScale, layerIndex, mipLevel);
#else
    float2 scaledUV = uv * UVScale;
    return SAMPLE_TEXTURE2D_ARRAY(array, SS, scaledUV, layerIndex);
#endif
}

float4 MTE_DebugMipLevel(float4 albedo, float uv, float layerUVScale, float textureSize)
{
    float mipLevel = MipMapLevel(uv, layerUVScale, textureSize);
    const float maxMipLevel = 9.0f;
    return 0.5f * albedo + 0.5f * float4(0, floor(mipLevel) / maxMipLevel, 0, 0);
}

#endif // MTECommonURP_INCLUDED