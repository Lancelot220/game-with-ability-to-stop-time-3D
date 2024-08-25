// UNITY_SHADER_NO_UPGRADE
Shader "Hidden/MTE/PaintTexturePreview"
{
	Properties
	{
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_NormalTex ("Normalmap", 2D) = "bump" {}
		_MaskTex ("Mask (RGB) Trans (A)", 2D) = "white" {}
		_Rotation ("Rotation", Range(0.0, 6.283185307)) = 0
	}

	CGINCLUDE
		#pragma surface surf Lambert vertex:vert alpha:blend nodynlightmap nolightmap nofog
		#include <Lighting.cginc>
		#include <UnityCG.cginc>

		struct Input
		{
			float4 pos  : POSITION;
			float4 tex0 : TEXCOORD0;
			float4 tex1 : TEXCOORD1;
		};

		sampler2D _MainTex;
		sampler2D _MaskTex;
		sampler2D _NormalTex;

		float4x4 unity_Projector;
		float4 _MainTex_ST;

		float _Rotation;
	
		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.pos = UnityObjectToClipPos(v.vertex);
			o.tex0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
			o.tex1 = mul(unity_Projector, v.vertex);
		}

		void surf(Input IN, inout SurfaceOutput o)
		{
            float sin, cos;
			sincos(_Rotation, sin, cos);
     
			float4 maskUV = UNITY_PROJ_COORD(IN.tex1);
			float2 v = maskUV.xy - 0.5;
            float tx = v.x;
            float ty = v.y;
            v.x = (cos * tx) - (sin * ty);
            v.y = (sin * tx) + (cos * ty);
			maskUV.xy = v + 0.5;

			float4 color = tex2D(_MainTex, IN.tex0.xy);
			float4 mask = tex2Dproj(_MaskTex, maskUV);
			o.Albedo = color.rgb;
			o.Alpha = mask.a;
			o.Normal = UnpackNormal(tex2D(_NormalTex, IN.tex0.xy));
			o.Emission = 0;
		}
	ENDCG

	Category
	{
		Tags { "Queue"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha

		SubShader//for target 3.0+
		{
			CGPROGRAM
				#pragma target 3.0
			ENDCG
		}
		SubShader//for target 2.5
		{
			CGPROGRAM
				#pragma target 2.5
			ENDCG
		}
		SubShader//for target 2.0
		{
			CGPROGRAM
				#pragma target 2.0
			ENDCG
		}
	}
}
