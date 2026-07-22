Shader "Custom/Clipping_Tubes_ColorFilter"
{
    Properties
    {
        _Alpha ("Alpha", Range(0,1)) = 1.0
        _Radius ("Radius", Range(0,1)) = 0.2
        
        [Header(Color Filtering)]
        [Toggle] _ShowRed("Show Red Only", Float) = 1
        [Toggle] _ShowGreen("Show Green Only", Float) = 1
        [Toggle] _ShowBlue("Show Blue Only", Float) = 1
        [Toggle] _ShowRest("Show Rest (Mixed/White/etc)", Float) = 1
        _ColorSensitivity("Color Sensitivity", Range(0, 1)) = 0.1
        
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
            
            // Filter Variablen
            float _ShowRed;
            float _ShowGreen;
            float _ShowBlue;
            float _ShowRest;
            float _ColorSensitivity;

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
                // FARBFILTER LOGIK (Performance: Cull before processing)
                // ---------------------------------------------------------
                
                // Wir nehmen die Durchschnittsfarbe der Linie
                float4 c = (input[0].color + input[1].color) * 0.5;
                
                // Ein Kanal gilt als dominant, wenn er größer ist als die anderen beiden 
                // plus einer Toleranz (Sensitivity).
                bool isRed   = (c.r > c.g + _ColorSensitivity) && (c.r > c.b + _ColorSensitivity);
                bool isGreen = (c.g > c.r + _ColorSensitivity) && (c.g > c.b + _ColorSensitivity);
                bool isBlue  = (c.b > c.r + _ColorSensitivity) && (c.b > c.g + _ColorSensitivity);

                bool visible = false;

                if (isRed)
                {
                    if (_ShowRed > 0.5) visible = true;
                }
                else if (isGreen)
                {
                    if (_ShowGreen > 0.5) visible = true;
                }
                else if (isBlue)
                {
                    if (_ShowBlue > 0.5) visible = true;
                }
                else 
                {
                    // Weder klar Rot, Grün noch Blau (z.B. Weiß, Gelb, Schwarz oder Grau)
                    if (_ShowRest > 0.5) visible = true;
                }

                // Wenn nicht sichtbar, Funktion sofort beenden -> Keine Geometrie erzeugen
                if (!visible) return;

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
                    corners0[j].color = c;

                    corners1[j].posWS = pos1 + offsets[j];
                    corners1[j].positionHCS = TransformWorldToHClip(corners1[j].posWS);
                    corners1[j].color = c;
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