﻿#ifndef DEFERRED_DECALS_COMMON_INCLUDED
#define DEFERRED_DECALS_COMMON_INCLUDED

#define UNITY_SHADER_NO_UPGRADE

#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_instancing

#include "UnityCG.cginc"

sampler2D _MaskTex;
int _MaskNormals;

sampler2D _MainTex;
float4 _MainTex_ST;
float4 _Color;

sampler2D _EmissionTex;
float4 _EmissionColor;

sampler2D _NormalTex;
float _NormalMultiplier;

sampler2D _SpecularTex;
float4 _SpecularMultiplier;

sampler2D _SmoothnessTex;
float _SmoothnessMultiplier;

float _MaxAngle;
float _SmoothMaxAngle;
float _SmoothAngle;

int _DecalBlendMode;
int _DecalSrcBlend;
int _DecalDstBlend;
int _NormalBlendMode;

sampler2D _CameraDepthTexture;
sampler2D _CameraGBufferTexture2;
sampler2D _CameraGBufferTexture1Copy;
sampler2D _CameraGBufferTexture2Copy;
sampler2D _GameObjectIDTex;

struct appdata
{
	float4 vertex : POSITION;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
	float4 pos : SV_POSITION;
	float4 uv : TEXCOORD1;
	float3 ray : TEXCOORD2;
	half3 decalNormal : TEXCOORD3;
	half3 decalTangent : TEXCOORD4;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

UNITY_INSTANCING_CBUFFER_START(DecalProps)
UNITY_DEFINE_INSTANCED_PROP(float, _MaskMultiplier)
UNITY_DEFINE_INSTANCED_PROP(float, _Emission)
UNITY_DEFINE_INSTANCED_PROP(float, _LimitTo)
UNITY_INSTANCING_CBUFFER_END

v2f vert(appdata v)
{
	v2f o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v, o);

	o.pos = UnityObjectToClipPos(v.vertex);
	o.uv = ComputeScreenPos(o.pos);
	o.ray = mul(UNITY_MATRIX_MV, v.vertex).xyz * float3(-1, -1, 1);
	o.decalNormal  = normalize(mul((float3x3)unity_ObjectToWorld, float3(0, 1, 0)));
	o.decalTangent = normalize(mul((float3x3)unity_ObjectToWorld, float3(1, 0, 0)));
	return o;
}

#define DEFERRED_FRAG_HEADER															\
UNITY_SETUP_INSTANCE_ID (i);															\
i.ray = i.ray * (_ProjectionParams.z / i.ray.z);										\
float2 uv = i.uv.xy / i.uv.w;															\
float limitTo = UNITY_ACCESS_INSTANCED_PROP(_LimitTo);									\
if (!isnan(limitTo))																	\
	clip(abs(limitTo - tex2D(_GameObjectIDTex, uv).r) > 0.1f ? -1 : 1);					\
float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);							\
depth = Linear01Depth(depth);															\
float4 vpos = float4(i.ray * depth,1);													\
float3 wpos = mul(unity_CameraToWorld, vpos).xyz;										\
float3 clipPos = mul(unity_WorldToObject, float4(wpos, 1)).xyz;							\
clip(0.5 - abs(clipPos.xyz));															\
float2 texUV = TRANSFORM_TEX((clipPos.xz + 0.5), _MainTex);								\
float mask = tex2D(_MaskTex, texUV).r * UNITY_ACCESS_INSTANCED_PROP(_MaskMultiplier);	\
clip(mask - 0.0005f);

#define FACING_CLIP																		\
clip(dot(gbuffer_normal, i.decalNormal) - _MaxAngle);

#define FACING_CLIP_SMOOTH																\
float angle = acos(dot(gbuffer_normal, i.decalNormal));									\
float facing = -saturate((angle - _SmoothMaxAngle) / _SmoothAngle) + 1.0f;				\
mask *= facing;																			\
clip(facing - 0.0005f);

#endif