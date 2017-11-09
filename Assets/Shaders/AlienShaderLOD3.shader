Shader "Unlit/AlienShaderLOD3"
{
	// This has been ported to Unity from a shader in the nVidia shader library

	Properties
	{
		[NoScaleOffset] _MainTex("Texture", 2D) = "white" {}
		_Displacement("Displacement", Range(0.0, 2.0)) = 1.6
	    _DebugTime("Debug Time", Range(0.0, 10.0)) = 2.0
		_Sharpness("Sharpness", Range(0.1, 5.0)) = 1.90
		_ColourSharpness("Colour Sharpness", Range(0.1, 5.0)) = 3.30
		_Speed("Speed", Range(0.001, 1.0)) = 0.3
		_TurbDensity("Turbulence Density", Range(0.01, 8.0)) = 2.27
		_ColorRange("Colour Range", Range(-6.0, 6.0)) = -2.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex mainVS
			#pragma fragment hotPS

			#include "UnityCG.cginc"

			//////////////////////////////////////////////////////////////
			// TWEAKABLES ////////////////////////////////////////////////
			//////////////////////////////////////////////////////////////

			float _Displacement;
			float _DebugTime;
			float _Sharpness;
			float _ColourSharpness;
			float _Speed;
			float _TurbDensity;
			float _ColorRange;

			//float4x4 gNoiseXf = { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
			//	<
			//	string UIName = "Coordinate System for Noise";
			//> = { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };

			//float4 dd[5] = {
			//	0,2,3,1, 2,2,2,2, 3,3,3,3, 4,4,4,4, 5,5,5,5 };

			#define BSIZE 32
			#define FULLSIZE 66
			#define NOISEFRAC 0.03125

			
			///////////// functions 

			// this is the smoothstep function f(t) = 3t^2 - 2t^3, without the normalization
			float3 s_curve(float3 t) { return t*t*(float3(3, 3, 3) - float3(2, 2, 2)*t); }
			float2 s_curve(float2 t) { return t*t*(float2(3, 3) - float2(2, 2)*t); }
			float  s_curve(float  t) { return t*t*(3.0 - 2.0*t); }

			/////////////////////////////

			struct appData
			{
				float4 Position     : POSITION;
				float4 Normal       : NORMAL;
				float4 TexCoord0    : TEXCOORD0;
			};

			// define outputs from vertex shader
			struct vbombVertexData
			{
				float4 HPosition	: POSITION;
				float4 Color0	: COLOR0;
			};

			////////

			float rand(float3 myVector) 
			{
				return frac(sin(_Time[0] * dot(myVector, float3(12.9898, 78.233, 45.5432))) * 43758.5453);
			}

			sampler2D _MainTex;

			float noise(float3 v, const uniform float4 pg[FULLSIZE])
			{
				v = v + (10000.0f).xxx;   // hack to avoid negative numbers

				float3 i = frac(v * NOISEFRAC) * BSIZE;   // index between 0 and BSIZE-1
				float3 f = frac(v);            // fractional position

				// lookup in permutation table
				float2 p;
				p.x = pg[i[0]].w;
				p.y = pg[i[0] + 1].w;
				p = p + i[1];

				float4 b;
				b.x = pg[p[0]].w;
				b.y = pg[p[1]].w;
				b.z = pg[p[0] + 1].w;
				b.w = pg[p[1] + 1].w;
				b = b + i[2];

				// compute dot products between gradients and vectors
				float4 r;
				r[0] = dot(pg[b[0]].xyz, f);
				r[1] = dot(pg[b[1]].xyz, f - float3(1.0f, 0.0f, 0.0f));
				r[2] = dot(pg[b[2]].xyz, f - float3(0.0f, 1.0f, 0.0f));
				r[3] = dot(pg[b[3]].xyz, f - float3(1.0f, 1.0f, 0.0f));

				float4 r1;
				r1[0] = dot(pg[b[0] + 1].xyz, f - float3(0.0f, 0.0f, 1.0f));
				r1[1] = dot(pg[b[1] + 1].xyz, f - float3(1.0f, 0.0f, 1.0f));
				r1[2] = dot(pg[b[2] + 1].xyz, f - float3(0.0f, 1.0f, 1.0f));
				r1[3] = dot(pg[b[3] + 1].xyz, f - float3(1.0f, 1.0f, 1.0f));

				// interpolate
				f = s_curve(f);
				r = lerp(r, r1, f[2]);
				r = lerp(r.xyyy, r.zwww, f[1]);
				return lerp(r.x, r.y, f[0]);
			}

			vbombVertexData mainVS(appData IN) 
			{
				vbombVertexData OUT;
				OUT.Color0 = float4(1.0f, 0.0f, 0.0f, 1.0f);
				OUT.HPosition = mul(UNITY_MATRIX_MVP, IN.Position);

				return OUT;
			}

			float4 hotPS(vbombVertexData IN) : COLOR
			{
				float2 nuv = float2(IN.Color0.x, 0.5);
				float4 nc = IN.Color0;
				//return float4(IN.Color0.xxx,1.0);
				return nc;
			}
			ENDCG
		}
	}
}
