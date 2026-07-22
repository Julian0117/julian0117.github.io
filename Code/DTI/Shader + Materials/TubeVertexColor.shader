Shader"Custom/TubeVertexColor"
{
    Properties
    {
        _Alpha ("Alpha", Range(0,1)) = 1.0
        _Radius ("Radius", Range(0,1)) = 0.6
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

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #include "UnityCG.cginc"

            float _Alpha;
            float _Radius;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                return o;
            }

            //Erstellt aus 2 vertices einer line ein tube.
            //Pro vertex 4 Außenpunkte. Zur Darstellung pro Seite 2 Dreiecke.
            //4 Seiten = 8 Dreiecke = 24 vertices insgesamt.
            [maxvertexcount(24)]
            void geom(line v2f input[2], inout TriangleStream<v2f> triStream)
            {
                float3 pos0 = input[0].worldPos;
                float3 pos1 = input[1].worldPos;

                float3 dir = normalize(pos1 - pos0);
                float3 ref = abs(dir.y) < 0.99 ? float3(0,1,0) : float3(1,0,0); //Referenzvektor für Kreuzprodukt
                float3 wallDir1 = normalize(cross(dir, ref)) * (_Radius/100);         //Richtungsvektor1 zur Tubewand ("links-rechts")
                float3 wallDir2 = normalize(cross(wallDir1, dir)) * (_Radius/100);    //Richtungsvektor2 zur Tubewand ("vorne-hinten")
                
                //Vertices der Ecken der Tube erstellen
                v2f corners0[4];

                //Ecke p0 rechts 
                float3 corner0_0 = pos0 + wallDir1;
                corners0[0].pos = UnityWorldToClipPos(float4(corner0_0, 1));
                corners0[0].worldPos = corner0_0;
                corners0[0].color = input[0].color;

                //Ecke p0 hinten 
                float3 corner0_1 = pos0 + wallDir2;
                corners0[1].pos = UnityWorldToClipPos(float4(corner0_1, 1));
                corners0[1].worldPos = corner0_1;
                corners0[1].color = input[0].color;

                //Ecke p0 links
                float3 corner0_2 = pos0 - wallDir1;
                corners0[2].pos = UnityWorldToClipPos(float4(corner0_2, 1));
                corners0[2].worldPos = corner0_2;
                corners0[2].color = input[0].color;

                //Ecke p0 vorne
                float3 corner0_3 = pos0 - wallDir2;
                corners0[3].pos = UnityWorldToClipPos(float4(corner0_3, 1));
                corners0[3].worldPos = corner0_3;
                corners0[3].color = input[0].color;

                v2f corners1[4];

                //Ecke p1 rechts
                float3 corner1_0 = pos1 + wallDir1;
                corners1[0].pos = UnityWorldToClipPos(float4(corner1_0, 1));
                corners1[0].worldPos = corner1_0;
                corners1[0].color = input[1].color;

                //Ecke p1 hinten
                float3 corner1_1 = pos1 + wallDir2;
                corners1[1].pos = UnityWorldToClipPos(float4(corner1_1, 1));
                corners1[1].worldPos = corner1_1;
                corners1[1].color = input[1].color;

                //Ecke p1 links
                float3 corner1_2 = pos1 - wallDir1;
                corners1[2].pos = UnityWorldToClipPos(float4(corner1_2, 1));
                corners1[2].worldPos = corner1_2;
                corners1[2].color = input[1].color;

                //Ecke p1 vorne
                float3 corner1_3 = pos1 - wallDir2;
                corners1[3].pos = UnityWorldToClipPos(float4(corner1_3, 1));
                corners1[3].worldPos = corner1_3;
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
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = i.color;
                col.a *= _Alpha;
                return col;
            }
            ENDHLSL
        }
    }
}