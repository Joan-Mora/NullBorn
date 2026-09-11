Shader "NullBorn/InteractableRim"
{
    Properties
    {
        [HDR] _RimColor ("Rim Color", Color) = (0.24, 0.42, 0.55, 1)
        _RimPower ("Rim Power", Range(0.5, 8)) = 2.6
        _RimIntensity ("Rim Intensity", Range(0, 5)) = 1.25
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "InteractableRim"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha One
            ZWrite Off
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _RimColor;
                float _RimPower;
                float _RimIntensity;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewWS = GetWorldSpaceViewDir(positionWS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);
                float3 viewWS = normalize(input.viewWS);
                float fresnel = pow(saturate(1.0 - saturate(dot(normalWS, viewWS))), _RimPower);
                float rim = fresnel * _RimIntensity;
                return half4(_RimColor.rgb * rim, rim);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
