// Updated Rain Shader for Unity URP (with fixed shadow handling)
Shader "Custom/URP_RainShader"
{
    Properties
    {
        _MainTex ("Color (RGB) Alpha (A)", 2D) = "white" {}
        _TintColor ("Tint Color (RGB)", Color) = (1, 1, 1, 1)
        _PointSpotLightMultiplier ("Point/Spot Light Multiplier", Range(0, 10)) = 2
        _DirectionalLightMultiplier ("Directional Light Multiplier", Range(0, 10)) = 1
        _InvFade ("Soft Particles Factor", Range(0.01, 100.0)) = 1.0
        _AmbientLightMultiplier ("Ambient light multiplier", Range(0, 1)) = 0.25
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "Pipeline"="UniversalRenderPipeline" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _ALPHATEST_ON
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _ADDITIONAL_LIGHTS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
            };

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			
			CBUFFER_START(UnityPerMaterial)
				float4 _MainTex_ST;
				float4 _TintColor;
				float _PointSpotLightMultiplier;
				float _DirectionalLightMultiplier;
				float _AmbientLightMultiplier;
			CBUFFER_END
			
            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = TransformObjectToWorld(v.positionOS);
                o.worldNormal = TransformObjectToWorldNormal(float3(0, 0, 1));
                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float3 ambientLight = _AmbientLightMultiplier * UNITY_LIGHTMODEL_AMBIENT.xyz;

                // Main light data retrieval
                Light mainLight = GetMainLight();
                float3 mainLightColor = mainLight.color;
                float3 mainLightDir = mainLight.direction;

                float3 normal = normalize(i.worldNormal);
                float mainLightIntensity = max(0.0, dot(mainLightDir, normal));
                float3 mainLightDiffuse = mainLightColor * mainLightIntensity * _DirectionalLightMultiplier;

                float3 diffuse = texColor.rgb * (ambientLight + mainLightDiffuse);
                float alpha = texColor.a;

                return float4(diffuse * _TintColor.rgb, alpha);
            }

            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}
