Shader "Unlit/AlienShaderLOD2"
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
			float _LOD1MaxDistance;
			float _LOD2MaxDistance;

			//float4x4 gNoiseXf = { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
			//	<
			//	string UIName = "Coordinate System for Noise";
			//> = { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };

			//float4 dd[5] = {
			//	0,2,3,1, 2,2,2,2, 3,3,3,3, 4,4,4,4, 5,5,5,5 };

			#define BSIZE 32
			#define FULLSIZE 66
			#define NOISEFRAC 0.03125

			const float4 NTab[FULLSIZE] = {
				-0.569811, 0.432591, -0.698699, 0,
				0.78118, 0.163006, 0.60265, 1,
				0.436394, -0.297978, 0.848982, 2,
				0.843762, -0.185742, -0.503554, 3,
				0.663712, -0.68443, -0.301731, 4,
				0.616757, 0.768825, 0.168875, 5,
				0.457153, -0.884439, -0.093694, 6,
				-0.956955, 0.110962, -0.268189, 7,
				0.115821, 0.77523, 0.620971, 8,
				-0.716028, -0.477247, -0.50945, 9,
				0.819593, -0.123834, 0.559404, 10,
				-0.522782, -0.586534, 0.618609, 11,
				-0.792328, -0.577495, -0.196765, 12,
				-0.674422, 0.0572986, 0.736119, 13,
				-0.224769, -0.764775, -0.60382, 14,
				0.492662, -0.71614, 0.494396, 15,
				0.470993, -0.645816, 0.600905, 16,
				-0.19049, 0.321113, 0.927685, 17,
				0.0122118, 0.946426, -0.32269, 18,
				0.577419, 0.408182, 0.707089, 19,
				-0.0945428, 0.341843, -0.934989, 20,
				0.788332, -0.60845, -0.0912217, 21,
				-0.346889, 0.894997, -0.280445, 22,
				-0.165907, -0.649857, 0.741728, 23,
				0.791885, 0.124138, 0.597919, 24,
				-0.625952, 0.73148, 0.270409, 25,
				-0.556306, 0.580363, 0.594729, 26,
				0.673523, 0.719805, 0.168069, 27,
				-0.420334, 0.894265, 0.153656, 28,
				-0.141622, -0.279389, 0.949676, 29,
				-0.803343, 0.458278, 0.380291, 30,
				0.49355, -0.402088, 0.77119, 31,
				-0.569811, 0.432591, -0.698699, 0,
				0.78118, 0.163006, 0.60265, 1,
				0.436394, -0.297978, 0.848982, 2,
				0.843762, -0.185742, -0.503554, 3,
				0.663712, -0.68443, -0.301731, 4,
				0.616757, 0.768825, 0.168875, 5,
				0.457153, -0.884439, -0.093694, 6,
				-0.956955, 0.110962, -0.268189, 7,
				0.115821, 0.77523, 0.620971, 8,
				-0.716028, -0.477247, -0.50945, 9,
				0.819593, -0.123834, 0.559404, 10,
				-0.522782, -0.586534, 0.618609, 11,
				-0.792328, -0.577495, -0.196765, 12,
				-0.674422, 0.0572986, 0.736119, 13,
				-0.224769, -0.764775, -0.60382, 14,
				0.492662, -0.71614, 0.494396, 15,
				0.470993, -0.645816, 0.600905, 16,
				-0.19049, 0.321113, 0.927685, 17,
				0.0122118, 0.946426, -0.32269, 18,
				0.577419, 0.408182, 0.707089, 19,
				-0.0945428, 0.341843, -0.934989, 20,
				0.788332, -0.60845, -0.0912217, 21,
				-0.346889, 0.894997, -0.280445, 22,
				-0.165907, -0.649857, 0.741728, 23,
				0.791885, 0.124138, 0.597919, 24,
				-0.625952, 0.73148, 0.270409, 25,
				-0.556306, 0.580363, 0.594729, 26,
				0.673523, 0.719805, 0.168069, 27,
				-0.420334, 0.894265, 0.153656, 28,
				-0.141622, -0.279389, 0.949676, 29,
				-0.803343, 0.458278, 0.380291, 30,
				0.49355, -0.402088, 0.77119, 31,
				-0.569811, 0.432591, -0.698699, 0,
				0.78118, 0.163006, 0.60265, 1 };

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
				float4 WPosition : WPOSITION;
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

				float4x4 gNoiseXf = { 1, 0, 0, 0, 
					         0, 1, 0, 0, 
							 0, 0, 1, 0, 
							 0, 0, 0, 1 };

				const float4 NTab[FULLSIZE] = {
					-0.569811, 0.432591, -0.698699, 0,
					0.78118, 0.163006, 0.60265, 1, 
					0.436394, -0.297978, 0.848982, 2,
					0.843762, -0.185742, -0.503554, 3,
					0.663712, -0.68443, -0.301731, 4,
					0.616757, 0.768825, 0.168875, 5,
					0.457153, -0.884439, -0.093694, 6,
					-0.956955, 0.110962, -0.268189, 7,
					0.115821, 0.77523, 0.620971, 8,
					-0.716028, -0.477247, -0.50945, 9,
					0.819593, -0.123834, 0.559404, 10,
					-0.522782, -0.586534, 0.618609, 11,
					-0.792328, -0.577495, -0.196765, 12,
					-0.674422, 0.0572986, 0.736119, 13,
					-0.224769, -0.764775, -0.60382, 14,
					0.492662, -0.71614, 0.494396, 15,
					0.470993, -0.645816, 0.600905, 16,
					-0.19049, 0.321113, 0.927685, 17,
					0.0122118, 0.946426, -0.32269, 18,
					0.577419, 0.408182, 0.707089, 19,
					-0.0945428, 0.341843, -0.934989, 20,
					0.788332, -0.60845, -0.0912217, 21,
					-0.346889, 0.894997, -0.280445, 22,
					-0.165907, -0.649857, 0.741728, 23,
					0.791885, 0.124138, 0.597919, 24,
					-0.625952, 0.73148, 0.270409, 25,
					-0.556306, 0.580363, 0.594729, 26,
					0.673523, 0.719805, 0.168069, 27,
					-0.420334, 0.894265, 0.153656, 28,
					-0.141622, -0.279389, 0.949676, 29,
					-0.803343, 0.458278, 0.380291, 30,
					0.49355, -0.402088, 0.77119, 31,
					-0.569811, 0.432591, -0.698699, 0,
					0.78118, 0.163006, 0.60265, 1,
					0.436394, -0.297978, 0.848982, 2,
					0.843762, -0.185742, -0.503554, 3,
					0.663712, -0.68443, -0.301731, 4,
					0.616757, 0.768825, 0.168875, 5,
					0.457153, -0.884439, -0.093694, 6,
					-0.956955, 0.110962, -0.268189, 7,
					0.115821, 0.77523, 0.620971, 8,
					-0.716028, -0.477247, -0.50945, 9,
					0.819593, -0.123834, 0.559404, 10,
					-0.522782, -0.586534, 0.618609, 11,
					-0.792328, -0.577495, -0.196765, 12,
					-0.674422, 0.0572986, 0.736119, 13,
					-0.224769, -0.764775, -0.60382, 14,
					0.492662, -0.71614, 0.494396, 15,
					0.470993, -0.645816, 0.600905, 16,
					-0.19049, 0.321113, 0.927685, 17,
					0.0122118, 0.946426, -0.32269, 18,
					0.577419, 0.408182, 0.707089, 19,
					-0.0945428, 0.341843, -0.934989, 20,
					0.788332, -0.60845, -0.0912217, 21,
					-0.346889, 0.894997, -0.280445, 22,
					-0.165907, -0.649857, 0.741728, 23,
					0.791885, 0.124138, 0.597919, 24,
					-0.625952, 0.73148, 0.270409, 25,
					-0.556306, 0.580363, 0.594729, 26,
					0.673523, 0.719805, 0.168069, 27,
					-0.420334, 0.894265, 0.153656, 28,
					-0.141622, -0.279389, 0.949676, 29,
					-0.803343, 0.458278, 0.380291, 30,
					0.49355, -0.402088, 0.77119, 31,
					-0.569811, 0.432591, -0.698699, 0,
					0.78118, 0.163006, 0.60265, 1 };



				float time = _Time.y; // _DebugTime;

				float4 noisePos = _TurbDensity*mul(IN.Position + (_Speed*time), gNoiseXf);
                float i = (noise(noisePos.xyz, NTab) + 1.0f) * 0.5f;
				//float i = 1.0f + time; // rand(IN.Position);
				
				float cr = 1.0 - (0.5 + _ColorRange*(i - 0.5));
				cr = pow(cr, _ColourSharpness);

				OUT.Color0 = float4((cr).xxx, 1.0f);
				//NewPos.w = 1.0f;
				OUT.HPosition = mul(UNITY_MATRIX_MVP, IN.Position);
				OUT.WPosition = mul(_Object2World, IN.Position);

				return OUT;
			}

			float4 hotPS(vbombVertexData IN) : COLOR
			{
				float2 nuv = float2(IN.Color0.x, 0.5);
				float4 nc = tex2D(_MainTex, nuv);
				// Use this to find the distance from the camera
				float distanceFromCam = distance(_WorldSpaceCameraPos, IN.WPosition);
				// Use this distance to calculate how far between LOD1 and LOD2 it is
				float modifier = ((distanceFromCam - _LOD1MaxDistance)/(_LOD2MaxDistance - _LOD1MaxDistance));
				// Use this modifier value to 'fade' the texture effect on the blob to just red as it goes from LOD2 to LOD3
				nc.r = lerp(nc.r, 1.0f, modifier);
				nc.g = lerp(nc.g, 0.0f, modifier);
				nc.b = lerp(nc.b, 0.0f, modifier);
				//return float4(IN.Color0.xxx,1.0);
				return nc;
			}
			ENDCG
		}
	}
}
