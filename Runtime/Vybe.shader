Shader "OccaSoftware/Vybe"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            ZWrite Off
            Cull Off
            ZClip On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float3 positionOS : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionOS = IN.positionOS.xyz;
                return OUT;
            }
            
            #define vybe_INV_TAU 0.15915
            #define vybe_INV_PI 0.31831

            CBUFFER_START(UnityPerMaterial)
                half _SUN_SIZE;
                half3 _SKY_COLOR;
                half3 _HORIZON_COLOR;
                Texture3D _STAR_TEXTURE;
                half _STAR_BRIGHTNESS;
            CBUFFER_END

            

            float3 GetSun(Light light, float3 positionOS)
            {
                float VoL = saturate(dot(light.direction, positionOS));
                return smoothstep(_SUN_SIZE, 1.0, VoL) * light.color * 5.0;
            }

            
            SamplerState linear_repeat_sampler;
            half3 frag(Varyings IN) : SV_Target
            {
                // Setup
                Light light = GetMainLight();
                float3 positionOS = normalize(IN.positionOS.xyz);
                float2 uv = 0;
                uv.x = 0.5 - atan2(positionOS.z, positionOS.x) * vybe_INV_TAU;
                uv.y = 0.5 + asin(positionOS.y) * vybe_INV_PI;

                // Sun
                float3 sun = GetSun(light, positionOS);
                
                // Sky
                float3 skyColor = lerp(_HORIZON_COLOR, _SKY_COLOR, saturate(uv.y));

                // Stars
                float stars = _STAR_TEXTURE.Sample(linear_repeat_sampler, positionOS * 25.0).r;
                stars = pow(stars, 10.0) * _STAR_BRIGHTNESS;
                return skyColor + sun + stars;
            }
            ENDHLSL
        }
    }
}