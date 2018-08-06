/************************************************************************************

Copyright   :   Copyright 2017 Oculus VR, LLC. All Rights reserved.

Licensed under the Oculus VR Rift SDK License Version 3.4.1 (the "License");
you may not use the Oculus VR Rift SDK except in compliance with the License,
which is provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at

https://developer.oculus.com/licenses/sdk-3.4.1

Unless required by applicable law or agreed to in writing, the Oculus VR SDK
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

************************************************************************************/

Shader "Custom/EdgeGlow"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_Thickness("Thickness", Range(0, 1)) = 0.5
		_EdgePosition("Edge position End", Range(0, 1)) = 0.6
		_FrontFactor("Front  factor", Range(0, 1)) = 0.6
		
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "IgnoreProjector"="True"  "Queue"="Transparent" }
		Blend SrcAlpha One
		Cull Off Lighting Off ZWrite Off
		
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float3 origNormal : NORMAL1;
				float3 eyeDir : DIRECTION;
			};

			fixed4 _TintColor;
			float _Thickness; 
			float _EdgePosition;
			float _FrontFactor;  
			
	
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.normal = normalize(mul(UNITY_MATRIX_IT_MV,v.normal).xyz);
				o.origNormal = v.normal;
				o.eyeDir = -normalize(mul(UNITY_MATRIX_MV, v.vertex).xyz);
				//o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float d = 	dot(i.normal,i.eyeDir);
				float p = smoothstep (_Thickness,0,abs(d - _EdgePosition))  + _FrontFactor * i.origNormal.x* d;	
				return float4((p * _TintColor).xyz,p) ;
			}
			ENDCG
		}
	}
}
