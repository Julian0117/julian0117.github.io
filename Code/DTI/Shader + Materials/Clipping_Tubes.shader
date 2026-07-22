Shader "Custom/Clipping_Tubes"
{
    Properties
    {
        _Alpha ("Alpha", Range(0,1)) = 1.0
        _Radius ("Radius", Range(0,1)) = 0.2
        
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
            #pragma geometry geom
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

            [maxvertexcount(24)]
            void geom(line Varyings input[2], inout TriangleStream<Varyings> triStream)
            {
                // ---------------------------------------------------------
                // TUBE GENERIERUNG
                // ---------------------------------------------------------

                // Positionen vom Start- und Endpunkt der Linie
                float3 pos0 = input[0].posWS;
                float3 pos1 = input[1].posWS;

                // Richtungsvektor der Linie von pos0 zu pos1
                float3 dir = normalize(pos1 - pos0); 

                // Referenzvektor, um Kreuzprodukt zu berechnen. Wahl des Vektors, sodass er nicht parallel zu dir ist.
                float3 ref = abs(dir.y) < 0.99 ? float3(0,1,0) : float3(1,0,0); 
                
                // Skalierung des Radius und Berechnung der Wandrichtungen (Achsen des Rohrquerschnitts)
                float worldRadius = _Radius / 100.0;
                float3 wallDir1 = normalize(cross(dir, ref)) * worldRadius;
                float3 wallDir2 = normalize(cross(wallDir1, dir)) * worldRadius;
                
                // Eckpunkte des Rohrquerschnitts an Start- und Endpunkt der Linie
                Varyings corners0[4];
                Varyings corners1[4];

                // Offsets der Eckpunkte relativ zum Start-/Endpunkt
                float3 offsets[4] = { wallDir1, wallDir2, -wallDir1, -wallDir2 };

                // Berechnung der Eckpunkte in Welt- und Homogenen Clip-Koordinaten
                for (int j = 0; j < 4; j++)
                {
                    corners0[j].posWS = pos0 + offsets[j];
                    corners0[j].positionHCS = TransformWorldToHClip(corners0[j].posWS);
                    corners0[j].color = input[0].color;

                    corners1[j].posWS = pos1 + offsets[j];
                    corners1[j].positionHCS = TransformWorldToHClip(corners1[j].posWS);
                    corners1[j].color = input[1].color;
                }

                // Erzeugung der Rohr-Seitenflächen als Dreiecks-Streifen
                for (int i = 0; i < 4; i++)
                {
                    int next = (i + 1) % 4;
                    
                    triStream.Append(corners0[i]);
                    triStream.Append(corners0[next]);
                    triStream.Append(corners1[next]);

                    triStream.RestartStrip(); 

                    triStream.Append(corners0[i]);
                    triStream.Append(corners1[next]);
                    triStream.Append(corners1[i]);
                    triStream.RestartStrip();
                }
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