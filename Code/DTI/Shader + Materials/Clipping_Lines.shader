Shader "Custom/Clipping_Lines"
{
    Properties
    {
        _Alpha ("Alpha", Range(0,1)) = 1.0
        
        [Header(Clipping)]
        [Toggle] _clippingEnable("Enable Clipping", Int) = 0
        _clippingNormal("Clipping Normal", Vector) = (0,1,0,0)
        _clippingOrigin("Clipping Origin", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float _Alpha;
            float _Radius;

            int _clippingEnable;
            float3 _clippingNormal;
            float3 _clippingOrigin;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 posWS : TEXCOORD0;
                float4 color : COLOR;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.posWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.color = IN.color;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }
            
            half4 frag(Varyings IN) : SV_Target
            {
                if (_clippingEnable > 0)
                {
                    float dist = dot(IN.posWS - _clippingOrigin, _clippingNormal);
                    clip(dist);
                }

                half4 col = IN.color;
                col.a *= _Alpha;
                return col;
            }
            ENDHLSL
        }
    }
}