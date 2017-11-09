Shader "Custom/Terrain"
{
	Properties
    {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo", 2D) = "white" {}
		_OcclusionTex ("Occlusion", 2D) = "white" {}
        _NormalTex("Normal Map", 2D) = "white" {}
        _NormalDetail1Tex("Normal Detail 1", 2D) = "white" {}
        _NormalDetail1("Normal Detail 1 Str", Range(-1,1)) = 1
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Specular ("Specular", Color) = (0,0,0,1)
	}

	SubShader
    {
		Tags { "RenderType" = "Opaque" }

		CGPROGRAM
		#pragma surface surf StandardSpecular fullforwardshadows
		#pragma target 3.0

		sampler2D _MainTex;
        sampler2D _OcclusionTex;
        sampler2D _NormalTex;
        sampler2D _NormalDetail1Tex;

		struct Input
        {
            float2 uv_MainTex;
            float2 uv_NormalDetail1Tex;
		};

		half _Glossiness;
        half4 _Color;
		half3 _Specular;
        half _NormalDetail1;

        float3 rnmBlendUnpacked(float3 n1, float3 n2)
        {
            n1 = n1.xyz * float3(2, 2, -2) + float3(-1, -1, 1);
            n2 = n2 * 2 - 1;

            float3 r;
            r.x = dot(n1.zxx, n2.xyz);
            r.y = dot(n1.yzy, n2.xyz);
            r.z = dot(n1.xyz, n2.xyz);
            return normalize(r);
        }

		void surf (Input IN, inout SurfaceOutputStandardSpecular o)
        {
            float3 bn = tex2D(_NormalTex, IN.uv_MainTex);
            float3 dn1 = lerp(float3(0.5, 0.5, 1), tex2D(_NormalDetail1Tex, IN.uv_NormalDetail1Tex), _NormalDetail1);


            o.Normal = rnmBlendUnpacked(tex2D(_NormalTex, IN.uv_MainTex), tex2D(_NormalDetail1Tex, IN.uv_NormalDetail1Tex));

			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Specular = _Specular;
			o.Smoothness = _Glossiness;
            o.Occlusion = tex2D(_OcclusionTex, IN.uv_MainTex);
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
