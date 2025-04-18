Shader "MTE/Surface/TextureArray_DisplayNormal"
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

		#pragma surface surf Unlit keepalpha addshadow fullforwardshadows 

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

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		float4 WeightedBlend4( half4 Weight, float4 Layer1, float4 Layer2, float4 Layer3, float4 Layer4 )
		{
			return Layer1 * Weight.r + Layer2 * Weight.g + Layer3 * Weight.b + Layer4 * Weight.a;
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

		void MTE_BuiltinRP_TextureArrayCore_DisplayNormal(Input i, out float3 OutNormal)
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

			//sample from TextureArray1
			{
				float4 layer0 = MTE_SampleTextureArray1(LayerIndices[0], uv);
				float4 layer1 = MTE_SampleTextureArray1(LayerIndices[1], uv);
				float4 layer2 = MTE_SampleTextureArray1(LayerIndices[2], uv);
				float4 layer3 = MTE_SampleTextureArray1(LayerIndices[3], uv);

				//get normal
				float4 normal = WeightedBlend4(LayerWeights, layer0, layer1, layer2, layer3);
				OutNormal = UnpackScaleNormal(normal , _NormalIntensity).xyz;
			}
		}

		void surf(Input i, inout SurfaceOutput o)
		{
			float3 normal;
			MTE_BuiltinRP_TextureArrayCore_DisplayNormal(i, normal);
			o.Emission = (normal*0.5 + 0.5);
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "MTE.MTETextureArrayShaderGUI"
}
