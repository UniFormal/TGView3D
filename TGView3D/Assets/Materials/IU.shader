Shader "Unlit/BasicInstancing"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
	}

		SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100

		Pass
	{
		Tags{ "LightMode" = "ForwardBase" }

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

		// Enable gpu instancing variants.
#pragma multi_compile_instancing

#include "UnityCG.cginc"

		struct appdata
	{
		float4 vertex : POSITION;

		// Need this for basic functionality.
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct v2f
	{
		float4 vertex : SV_POSITION;

	};

	fixed4 _Color;

	v2f vert(appdata v)
	{
		v2f o;

		// Need this for basic functionality.
		UNITY_SETUP_INSTANCE_ID(v);
		
		o.vertex = UnityObjectToClipPos(v.vertex);
		return o;
	}

	fixed4 frag() : SV_Target
	{
		

		return fixed4( _Color.rgb, 1);
	}
		ENDCG
	}
	}
}
