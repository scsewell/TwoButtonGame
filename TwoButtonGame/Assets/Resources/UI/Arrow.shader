Shader "Custom/Arrow"
{
	Properties
    {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Emission("Emission", Range(0,1)) = 0.5
        _RimThickness("Rim Thickness", Range(0, 0.1)) = 0.05
        _RimColor("Rim Color", Color) = (0,0,0,0)
	}

	SubShader
    {
		Tags { "RenderType"="Opaque" }
        LOD 300
		
        Pass
        {
            Cull Front

		    CGPROGRAM
		    #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            fixed _RimThickness;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex + _RimThickness * v.normal);
                return o;
            }

            fixed4 _RimColor;

            fixed4 frag(v2f i) : SV_Target
            {
                return _RimColor;
            }
            ENDCG
        }

        Pass
        {
            Cull Back

		    CGPROGRAM
		    #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 normal : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 _Color;
            fixed _Emission;
            
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                return ((1 - _Emission) * dot(i.normal, float3(0, 1, 0)) + _Emission) * col;
            }
            ENDCG
        }
	}
}
