Shader "DefenderOfDreams/FogOverlay"
{
    Properties
    {
        _FogTex ("Fog Mask", 2D) = "white" {}
        _WorldOrigin ("World Origin", Vector) = (0, 0, 0, 0)
        _WorldSize ("World Size", Vector) = (64, 64, 0, 0)
        _FogColor ("Fog Color", Color) = (0.03, 0.03, 0.05, 1)
        _AccentColor ("Accent Color", Color) = (0.12, 0.1, 0.18, 1)
        _MaxOpacity ("Max Opacity", Range(0, 1)) = 0.97
        _DitherIntensity ("Dither Intensity", Range(0, 1)) = 0.35
        _DitherSpeed ("Dither Speed", Float) = 2
        _Simplified ("Simplified Fallback", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            TEXTURE2D(_FogTex);
            SAMPLER(sampler_FogTex);

            float4 _FogTex_ST;
            float4 _WorldOrigin;
            float4 _WorldSize;
            float4 _FogColor;
            float4 _AccentColor;
            float _MaxOpacity;
            float _DitherIntensity;
            float _DitherSpeed;
            float _Simplified;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.worldPos = TransformObjectToWorld(input.positionOS.xyz);
                return output;
            }

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float Bayer4(float2 p)
            {
                float2 p1 = frac(p * 0.5) * 2.0;
                float2 p2 = floor(p * 0.5);
                float b = p1.x * p1.x * 0.75 + p1.y * p1.y * 0.25;
                b += p2.x * 0.125 + p2.y * 0.0625;
                return frac(b);
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 worldXY = input.worldPos.xy;

                float2 mapUV = float2(
                    (worldXY.x - _WorldOrigin.x) / max(_WorldSize.x, 0.0001),
                    (worldXY.y - _WorldOrigin.y) / max(_WorldSize.y, 0.0001));

                float4 mask = SAMPLE_TEXTURE2D(_FogTex, sampler_FogTex, mapUV);
                float fog = mask.r;
                float lost = mask.g;

                float dither = 0.0;
                if (_Simplified < 0.5)
                {
                    float2 cell = floor(worldXY * 4.0);
                    float t = floor(_Time.y * _DitherSpeed);
                    float n = Hash21(cell + t);
                    float bayer = Bayer4(worldXY * 6.0 + float2(t * 0.37, t * 0.11));
                    dither = lerp(n, bayer, 0.5);
                    dither = (dither - 0.5) * _DitherIntensity;
                }

                float alpha = saturate(fog + dither) * _MaxOpacity;
                float3 baseColor = lerp(_FogColor.rgb, _AccentColor.rgb, saturate(fog * 0.85 + dither * 0.5));

                if (lost > 0.5)
                {
                    baseColor = lerp(baseColor, float3(0.0, 0.0, 0.0), 0.65);
                    alpha = max(alpha, _MaxOpacity);
                }

                if (fog < 0.02 && lost < 0.5)
                {
                    return half4(0, 0, 0, 0);
                }

                return half4(baseColor, alpha);
            }
            ENDHLSL
        }
    }
}
