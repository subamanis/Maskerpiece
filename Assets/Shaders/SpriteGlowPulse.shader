Shader "Custom/SpriteGlowPulse"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _GlowColor ("Glow Color", Color) = (1,1,1,1)
        _GlowSize ("Glow Size", Range(0,8)) = 2
        _GlowIntensity ("Glow Intensity", Range(0,5)) = 1
        _AlphaThreshold ("Alpha Threshold", Range(0,1)) = 0.1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "RenderPipeline"="UniversalPipeline"
            "CanUseSpriteAtlas"="True"
        }

        Blend One OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;
            float4 _Color;
            float4 _GlowColor;
            float _GlowSize;
            float _GlowIntensity;
            float _AlphaThreshold;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color * _Color;
                return output;
            }

            float SampleAlpha(float2 uv)
            {
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).a;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                float4 baseColor = tex * input.color;
                float baseAlpha = baseColor.a;

                float2 texel = _MainTex_TexelSize.xy * _GlowSize;
                float maxAlpha = baseAlpha;
                maxAlpha = max(maxAlpha, SampleAlpha(input.uv + float2(texel.x, 0)));
                maxAlpha = max(maxAlpha, SampleAlpha(input.uv + float2(-texel.x, 0)));
                maxAlpha = max(maxAlpha, SampleAlpha(input.uv + float2(0, texel.y)));
                maxAlpha = max(maxAlpha, SampleAlpha(input.uv + float2(0, -texel.y)));
                maxAlpha = max(maxAlpha, SampleAlpha(input.uv + texel));
                maxAlpha = max(maxAlpha, SampleAlpha(input.uv - texel));
                maxAlpha = max(maxAlpha, SampleAlpha(input.uv + float2(texel.x, -texel.y)));
                maxAlpha = max(maxAlpha, SampleAlpha(input.uv + float2(-texel.x, texel.y)));

                float glowMask = saturate(maxAlpha - baseAlpha);
                glowMask *= step(_AlphaThreshold, maxAlpha);

                float3 glow = _GlowColor.rgb * _GlowIntensity * glowMask * _GlowColor.a;
                float3 finalRgb = baseColor.rgb + glow;
                float finalA = saturate(baseAlpha + glowMask * _GlowColor.a);

                return half4(finalRgb, finalA);
            }
            ENDHLSL
        }
    }
}
