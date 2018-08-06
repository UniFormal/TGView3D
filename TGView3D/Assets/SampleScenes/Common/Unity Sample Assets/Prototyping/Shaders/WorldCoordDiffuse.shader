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

Shader "Custom/WorldCoord Diffuse" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_BaseScale ("Base Tiling", Vector) = (1,1,1,0)
}

SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 150

CGPROGRAM
#pragma surface surf Lambert 

sampler2D _MainTex;

fixed4 _Color;
fixed3 _BaseScale;

struct Input {
	float2 uv_MainTex;
	float3 worldPos;
	float3 worldNormal;

};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 texXY = tex2D(_MainTex, IN.worldPos.xy * _BaseScale.z);// IN.uv_MainTex);
	fixed4 texXZ = tex2D(_MainTex, IN.worldPos.xz * _BaseScale.y);// IN.uv_MainTex);
	fixed4 texYZ = tex2D(_MainTex, IN.worldPos.yz * _BaseScale.x);// IN.uv_MainTex);
	fixed3 mask = fixed3(
		dot (IN.worldNormal, fixed3(0,0,1)),
		dot (IN.worldNormal, fixed3(0,1,0)),
		dot (IN.worldNormal, fixed3(1,0,0)));
	
	fixed4 tex = 
		texXY * abs(mask.x) +
		texXZ * abs(mask.y) +
		texYZ * abs(mask.z);
	fixed4 c = tex * _Color;
	o.Albedo = c.rgb;
}
ENDCG
}

FallBack "Diffuse"
}
