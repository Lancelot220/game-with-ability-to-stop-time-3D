Shader "MTE/Standard/TextureArray"
{
	Properties
	{
		_TextureArray0("TextureArray0", 2DArray) = "white" {}
		_TextureArray1("TextureArray1", 2DArray) = "white" {}
		_Control0("Control0", 2D) = "white" {}
		_Control1("Control1", 2D) = "white" {}
		_Control2("Control2", 2D) = "white" {}
		_NormalIntensity("Normal Intensity", Range( 0.01 , 10)) = 1
		_UVScale("UV Scale", Range( 0.01 , 100)) = 1
		[Toggle(ENABLE_METALLIC)] ENABLE_METALLIC("Enable Metallic", Float) = 1
		[Toggle(ENABLE_NORMAL_INTENSITY)] ENABLE_NORMAL_INTENSITY("Enable Normal Intensity", Float) = 1
		[Toggle(ENABLE_LAYER_UV_SCALE)] ENABLE_LAYER_UV_SCALE("Enable per-layer UV Scale", Float) = 1
		[Toggle(ENABLE_LAYER_UV_SCALE)] ENABLE_LAYER_UV_SCALE("ENABLE_LAYER_UV_SCALE", Float) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma shader_feature ENABLE_LAYER_UV_SCALE
		#pragma shader_feature_local _HasWeightMap1
		#pragma shader_feature_local _HasWeightMap2
		#pragma shader_feature ENABLE_NORMAL_INTENSITY
		#pragma shader_feature ENABLE_METALLIC

		#pragma surface surf Standard keepalpha addshadow fullforwardshadows

        #pragma require 2darray
		
		#include "../MTECommonBuiltinRP.hlsl"

		struct Input
		{
			float2 uv_Control0;
		};

		UNITY_DECLARE_TEX2DARRAY(_TextureArray0);
		UNITY_DECLARE_TEX2DARRAY(_TextureArray1);
		float4 _TextureArray0_TexelSize;
		float4 _TextureArray1_TexelSize;
		uniform float _UVScale;
		uniform float LayerUVScales[12];
		uniform sampler2D _Control0;
		uniform sampler2D _Control1;
		uniform sampler2D _Control2;
		uniform float _NormalIntensity;

		float4 WeightedBlend4( half4 Weight, float4 Layer1, float4 Layer2, float4 Layer3, float4 Layer4 )
		{
			return Layer1 * Weight.r + Layer2 * Weight.g + Layer3 * Weight.b + Layer4 * Weight.a;
		}

		inline float WeightedMax4( float4 Weights, float a, float b, float c, float d )
		{
			return max(max(max(Weights.x * a, Weights.y * b), Weights.z * c), Weights.w * d);
		}

		float4 MTE_SampleTextureArray0(int layerIndex, float2 uv)
		{
		#ifdef ENABLE_LAYER_UV_SCALE
			float textureSize = _TextureArray0_TexelSize.z;
			float layerUVScale = LayerUVScales[layerIndex];
			float mipLevel = MipMapLevel(uv, layerUVScale, textureSize);
			return UNITY_SAMPLE_TEX2DARRAY_LOD(_TextureArray0, float3(uv * layerUVScale, layerIndex), mipLevel);
		#else
			return UNITY_SAMPLE_TEX2DARRAY(_TextureArray0, float3(uv * _UVScale, layerIndex));
		#endif
		}

		float4 MTE_SampleTextureArray1(int layerIndex, float2 uv)
		{
		#ifdef ENABLE_LAYER_UV_SCALE
			float textureSize = _TextureArray1_TexelSize.z;
			float layerUVScale = LayerUVScales[layerIndex];
			float mipLevel = MipMapLevel(uv, layerUVScale, textureSize);
			return UNITY_SAMPLE_TEX2DARRAY_LOD(_TextureArray1, float3(uv * layerUVScale, layerIndex), mipLevel);
		#else
			return UNITY_SAMPLE_TEX2DARRAY(_TextureArray1, float3(uv * _UVScale, layerIndex));
		#endif
		}

		void MTE_BuiltinRP_TextureArrayCore(Input i, out float3 OutAlbedo, out float3 OutNormal, out float OutMetallic, out float OutSmoothness, out float OutAmbientOcclusion)
		{
			//fetch max 4 weighted layers
			float4 _Vector0 = float4(0,0,0,0);
			float2 uv = i.uv_Control0;
			float4 Control0 = tex2D( _Control0, uv );
			float4 Control1;
			float4 Control2;
			#ifdef _HasWeightMap1
				Control1 = tex2D( _Control1, uv );
			#else
				Control1 = _Vector0;
			#endif
			#ifdef _HasWeightMap2
				Control2 = tex2D( _Control2, uv );
			#else
				Control2 = _Vector0;
			#endif
			float4 LayerWeights = float4( 0,0,0,0 );
			float4 LayerIndices = float4( 0,0,0,0 );
			Max4WeightLayer(Control0, Control1, Control2, LayerWeights, LayerIndices);

			{
				//sample from TextureArray0
				float4 layer0 = MTE_SampleTextureArray0(LayerIndices[0], uv);
				float4 layer1 = MTE_SampleTextureArray0(LayerIndices[1], uv);
				float4 layer2 = MTE_SampleTextureArray0(LayerIndices[2], uv);
				float4 layer3 = MTE_SampleTextureArray0(LayerIndices[3], uv);

				//get albedo
				OutAlbedo = WeightedBlend4(LayerWeights, layer0, layer1, layer2, layer3).xyz;

				//get metallic
#ifdef ENABLE_METALLIC
					OutMetallic = WeightedMax4(LayerWeights, layer0.w, layer1.w, layer2.w, layer3.w);
#else
            		OutMetallic = 0.0f;
#endif
			}

			//sample from TextureArray1
			{
				float4 layer0 = MTE_SampleTextureArray1(LayerIndices[0], uv);
				float4 layer1 = MTE_SampleTextureArray1(LayerIndices[1], uv);
				float4 layer2 = MTE_SampleTextureArray1(LayerIndices[2], uv);
				float4 layer3 = MTE_SampleTextureArray1(LayerIndices[3], uv);

				//get smoothness, normal and AO
				float4 t = WeightedBlend4(LayerWeights, layer0, layer1, layer2, layer3);
				OutSmoothness = 1.0f - t.x;
				float2 NormalXY = t.yz;
				OutNormal = RestoreNormal(NormalXY , _NormalIntensity);
				OutAmbientOcclusion = t.w;
			}
		}

		void surf(Input i, inout SurfaceOutputStandard o)
		{
			MTE_BuiltinRP_TextureArrayCore(i, o.Albedo, o.Normal, o.Metallic, o.Smoothness, o.Occlusion);
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "MTE.MTETextureArrayShaderGUI"
}
