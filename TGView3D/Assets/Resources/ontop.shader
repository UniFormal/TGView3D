Shader "Unlit/ontop"
{
	Properties
	{

		_TintColor("Tint Color", Color) = (1,1,1,1)

	}

		SubShader
		{
			Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }
			LOD 100

			ZTest Always


			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
	
				};

				struct v2f
				{

					float4 vertex : SV_POSITION;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float4 _TintColor;
				float _Transparency;
				float _CutoutThresh;
				float _Distance;
				float _Amplitude;
				float _Speed;
				float _Amount;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);

					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					// sample the texture
					fixed4 col =  _TintColor;

					return col;
				}
				ENDCG
			}
		}
}
