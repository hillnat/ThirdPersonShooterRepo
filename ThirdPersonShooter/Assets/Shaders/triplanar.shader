Shader "Custom/TriplanarFakeLambert_Brightness"
{
    Properties
    {
        _BaseMap("Base Map", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _NormalMap("Normal Map", 2D) = "bump" {}
        _NormalScale("Normal Scale", Float) = 1.0
        _Scale("Texture Scale", Float) = 1.0
        _BlendSharpness("Blend Sharpness", Float) = 4.0
        _Brightness("Brightness", Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalRenderPipeline" "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);

            float4 _BaseColor;
            float4 _BaseMap_ST;
            float4 _NormalMap_ST;
            float _NormalScale;
            float _Scale;
            float _BlendSharpness;
            float _Brightness;

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.worldPos = TransformObjectToWorld(v.positionOS.xyz);
                o.worldNormal = TransformObjectToWorldNormal(v.normalOS);
                o.positionCS = TransformWorldToHClip(o.worldPos);
                return o;
            }

            float3 SampleTriplanarNormal(float3 worldNormal, float3 worldPos)
            {
                float3 wp = worldPos * _Scale;

                float2 uvX = wp.zy * _NormalMap_ST.xy + _NormalMap_ST.zw;
                float2 uvY = wp.xz * _NormalMap_ST.xy + _NormalMap_ST.zw;
                float2 uvZ = wp.xy * _NormalMap_ST.xy + _NormalMap_ST.zw;

                float3 nX = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uvX));
                float3 nY = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uvY));
                float3 nZ = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uvZ));

                float3 blend = pow(abs(worldNormal), _BlendSharpness);
                blend /= (blend.x + blend.y + blend.z + 1e-5);

                return normalize(nX * blend.x + nY * blend.y + nZ * blend.z) * _NormalScale;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float3 n = normalize(i.worldNormal);
                float3 wp = i.worldPos * _Scale;

                // Base color triplanar
                float2 uvX = wp.zy * _BaseMap_ST.xy + _BaseMap_ST.zw;
                float2 uvY = wp.xz * _BaseMap_ST.xy + _BaseMap_ST.zw;
                float2 uvZ = wp.xy * _BaseMap_ST.xy + _BaseMap_ST.zw;

                float3 blend = pow(abs(n), _BlendSharpness);
                blend /= (blend.x + blend.y + blend.z + 1e-5);

                half4 colX = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvX);
                half4 colY = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvY);
                half4 colZ = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvZ);

                half3 baseColor = (colX.rgb * blend.x + colY.rgb * blend.y + colZ.rgb * blend.z) * _BaseColor.rgb;

                // Normal
                float3 normalWS = SampleTriplanarNormal(n, i.worldPos);

                // Fake Lambert diffuse
                float3 lightDir = normalize(float3(0.3, 0.8, 0.5)); // arbitrary light direction
                float NdotL = saturate(dot(normalWS, lightDir));
                half3 diffuse = baseColor * NdotL * _Brightness; // apply brightness

                return half4(diffuse, 1.0);
            }

            ENDHLSL
        }
    }
}
