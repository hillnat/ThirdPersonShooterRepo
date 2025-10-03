Shader "Custom/Triplanar_URP_Lightmap"
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
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // Multi-compile variants for lightmaps
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 texcoord   : TEXCOORD0;
                float2 lightmapUV : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 worldPos   : TEXCOORD0;
                float3 worldNormal: TEXCOORD1;
                float3 worldTangent: TEXCOORD2;
                float3 worldBitangent: TEXCOORD3;
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 4);
                float fogCoord    : TEXCOORD5;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _BaseMap_ST;
                float4 _NormalMap_ST;
                float _NormalScale;
                float _Scale;
                float _BlendSharpness;
                float _Brightness;
            CBUFFER_END

            Varyings vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(v.normalOS, v.tangentOS);

                o.worldPos = vertexInput.positionWS;
                o.worldNormal = normalInput.normalWS;
                o.worldTangent = normalInput.tangentWS;
                o.worldBitangent = normalInput.bitangentWS;
                o.positionCS = vertexInput.positionCS;
                o.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);

                // Transfer lightmap UVs or calculate vertex lighting
                OUTPUT_LIGHTMAP_UV(v.lightmapUV, unity_LightmapST, o.lightmapUV);
                OUTPUT_SH(o.worldNormal, o.vertexSH);

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

                // Triplanar texture sampling
                float2 uvX = wp.zy * _BaseMap_ST.xy + _BaseMap_ST.zw;
                float2 uvY = wp.xz * _BaseMap_ST.xy + _BaseMap_ST.zw;
                float2 uvZ = wp.xy * _BaseMap_ST.xy + _BaseMap_ST.zw;

                float3 blend = pow(abs(n), _BlendSharpness);
                blend /= (blend.x + blend.y + blend.z + 1e-5);

                half4 colX = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvX);
                half4 colY = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvY);
                half4 colZ = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvZ);

                half3 albedo = (colX.rgb * blend.x + colY.rgb * blend.y + colZ.rgb * blend.z) * _BaseColor.rgb;

                // Sample triplanar normals
                float3 normalTS = SampleTriplanarNormal(n, i.worldPos);
                float3x3 TBN = float3x3(i.worldTangent, i.worldBitangent, i.worldNormal);
                float3 normalWS = normalize(mul(normalTS, TBN));

                // Sample baked lighting (lightmap or light probes)
                half3 bakedGI;
                #ifdef LIGHTMAP_ON
                    bakedGI = SAMPLE_GI(i.lightmapUV, i.vertexSH, normalWS);
                #else
                    bakedGI = SampleSH(normalWS);
                #endif

                // Calculate lighting
                Light mainLight = GetMainLight();
                half3 lighting = mainLight.color * mainLight.distanceAttenuation * saturate(dot(normalWS, mainLight.direction));
                
                // Combine albedo with baked and dynamic lighting
                half3 color = albedo * (bakedGI + lighting) * _Brightness;

                // Apply fog
                color = MixFog(color, i.fogCoord);

                return half4(color, 1.0);
            }

            ENDHLSL
        }

        // ShadowCaster pass for shadow casting
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            
            ENDHLSL
        }

        // DepthOnly pass for depth prepass
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            
            ENDHLSL
        }

        // Meta pass for lightmap baking
        Pass
        {
            Name "Meta"
            Tags { "LightMode" = "Meta" }

            Cull Off

            HLSLPROGRAM
            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMetaSimple
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _BaseMap_ST;
                float _Scale;
                float _BlendSharpness;
                float _Brightness;
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            half4 UniversalFragmentMetaSimple(Varyings input) : SV_Target
            {
                MetaInput metaInput = (MetaInput)0;
                metaInput.Albedo = _BaseColor.rgb * _Brightness;
                return UniversalFragmentMeta(input, metaInput);
            }
            
            ENDHLSL
        }
    }
}