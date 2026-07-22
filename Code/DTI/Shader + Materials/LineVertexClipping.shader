Shader"Custom/LineVertexColorClipping"
{
    Properties
    {
        _Alpha ("Alpha", Range(0,1)) = 1.0
        _Radius ("Radius", Range(0,1)) = 0.5
        
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
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On
        //Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            //#pragma geometry geom
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _Alpha;
                float _Radius;
                int _clippingEnable;
                float3 _clippingNormal;
                float3 _clippingOrigin;
            CBUFFER_END

            struct Attributes
            {
                //float4 vertex : POSITION;
                float4 positionOS : POSITION;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 posWS : TEXCOORD0; // Weltposition für das Clipping
                float4 color : COLOR;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                // Wir berechnen die Weltposition, um gegen die Welt-Ebene zu prüfen
                OUT.posWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.color = IN.color;
                return OUT;
            }
            
            //Erstellt aus 2 vertices einer line ein tube.
            //Pro vertex 4 Außenpunkte. Zur Darstellung pro Seite 2 Dreiecke.
            //4 Seiten = 8 Dreiecke = 24 vertices insgesamt.
            /*[maxvertexcount(24)]
            void geom(line Varyings input[2], inout TriangleStream<Varyings> triStream)
            {
                float3 pos0 = input[0].posWS;
                float3 pos1 = input[1].posWS;

                float3 dir = normalize(pos1 - pos0);
                float3 ref = abs(dir.y) < 0.99 ? float3(0,1,0) : float3(1,0,0); //Referenzvektor für Kreuzprodukt
                float3 wallDir1 = normalize(cross(dir, ref)) * (_Radius/1000);         //Richtungsvektor1 zur Tubewand ("links-rechts")
                float3 wallDir2 = normalize(cross(wallDir1, dir)) * (_Radius/1000);    //Richtungsvektor2 zur Tubewand ("vorne-hinten")
                
                //Vertices der Ecken der Tube erstellen
                Varyings corners0[4];

                //Ecke p0 rechts 
                float3 corner0_0 = pos0 + wallDir1;
                corners0[0].positionHCS= TransformObjectToHClip(float4(corner0_0, 1));
                corners0[0].posWS = corner0_0;
                corners0[0].color = input[0].color;

                //Ecke p0 hinten 
                float3 corner0_1 = pos0 + wallDir2;
                corners0[1].positionHCS= TransformObjectToHClip(float4(corner0_1, 1));
                corners0[1].posWS = corner0_1;
                corners0[1].color = input[0].color;

                //Ecke p0 links
                float3 corner0_2 = pos0 - wallDir1;
                corners0[2].positionHCS= TransformObjectToHClip(float4(corner0_2, 1));
                corners0[2].posWS = corner0_2;
                corners0[2].color = input[0].color;

                //Ecke p0 vorne
                float3 corner0_3 = pos0 - wallDir2;
                corners0[3].positionHCS= TransformObjectToHClip(float4(corner0_3, 1));
                corners0[3].posWS = corner0_3;
                corners0[3].color = input[0].color;

                Varyings corners1[4];

                //Ecke p1 rechts
                float3 corner1_0 = pos1 + wallDir1;
                corners1[0].positionHCS= TransformObjectToHClip(float4(corner1_0, 1));
                corners1[0].posWS = corner1_0;
                corners1[0].color = input[1].color;

                //Ecke p1 hinten
                float3 corner1_1 = pos1 + wallDir2;
                corners1[1].positionHCS= TransformObjectToHClip(float4(corner1_1, 1));
                corners1[1].posWS = corner1_1;
                corners1[1].color = input[1].color;

                //Ecke p1 links
                float3 corner1_2 = pos1 - wallDir1;
                corners1[2].positionHCS= TransformObjectToHClip(float4(corner1_2, 1));
                corners1[2].posWS = corner1_2;
                corners1[2].color = input[1].color;

                //Ecke p1 vorne
                float3 corner1_3 = pos1 - wallDir2;
                corners1[3].positionHCS= TransformObjectToHClip(float4(corner1_3, 1));
                corners1[3].posWS = corner1_3;
                corners1[3].color = input[1].color;

                //4 Seiten der Tube definieren
                for (int i = 0; i < 4; i++)
                {
                    int next = (i + 1) % 4;

                    // Dreieck 1 (Keine Ahnung, warum das so funktioniert???)
                    triStream.Append(corners1[next]);
                    triStream.Append(corners0[next]);
                    triStream.Append(corners0[i]);

                    // Dreieck 2
                    triStream.Append(corners1[i]);
                    triStream.Append(corners1[next]);
                    triStream.Append(corners0[next]);
                }
            }*/
            

            half4 frag(Varyings IN) : SV_Target
            {
                if (_clippingEnable > 0)
                {
                    // Die Ebenengleichung: (Punkt - Ursprung) dot Normal
                    // Ein Wert < 0 bedeutet, der Punkt liegt hinter der Ebene
                    float dist = dot(IN.posWS - _clippingOrigin, _clippingNormal);
                    
                    // clip(x) verwirft den Pixel, wenn x < 0
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