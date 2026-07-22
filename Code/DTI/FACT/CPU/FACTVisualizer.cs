using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FACTVisualizer : MonoBehaviour
{
    public Texture3D volume;        // Diffusionsrichtung in RGBA
    public Material lineMaterial;   // Material für die Darstellung

    public float stepSize = 0.01f;
    public float faThreshold = 0.2f;
    public int maxSteps = 500;
    public float minFiberLength = 0.05f; // in Objekt-Raum-Einheiten
    public float resolution = 1f;
    public int factCutoffAngle = 45;
    public int axisToleranceAngle = 20;

    private Vector3[] seedPoints;

    public GameObject clippingPlane;
    public Renderer renderer;

    public String stopwatchText_GenerateSeeds;
    public String stopwatchText_TraceFiber;

    /// <summary>
    ///     getter and setter method for the uniform <c>clippingEnable</c> inside the shader.
    /// </summary>
    public bool shaderClippingEnable
    {
        get
        {
            if (renderer.material.GetInt("_clippingEnable") == 1)
                return true;
            else return false;
        }
        set
        {
            renderer.material.SetInt("_clippingEnable", value ? 1 : 0);
        }
    }

    /// <summary>
    ///     getter and setter method for the uniform <c>clippingOrigin</c> inside the shader.
    /// </summary>
    public Vector3 shaderClippingOrigin
    {
        get
        {
            return renderer.material.GetVector("_clippingOrigin");
        }
        set
        {
            renderer.material.SetVector("_clippingOrigin", value);
        }
    }

    /// <summary>
    ///     getter and setter method for the uniform <c>clippingNormal</c> inside the shader.
    /// </summary>
    public Vector3 shaderClippingNormal
    {
        get
        {
            return renderer.material.GetVector("_clippingNormal");
        }
        set
        {
            renderer.material.SetVector("_clippingNormal", value);
        }
    }

    // Aktualisiert die Werte für ClippingOrigin und ClippingNormal im Material / Shader
    void FixedUpdate()
    {
        if (clippingPlane != null)
        {
            this.shaderClippingOrigin = clippingPlane.transform.position;
            this.shaderClippingNormal = clippingPlane.transform.right;
            this.shaderClippingEnable = true;
        }
        else
            this.shaderClippingEnable = false;
    }

    void Start()
    {
        // Der generelle Ablauf des Scripts ist:
        // Resetten der Werte,
        // SeedPoints generieren
        // FiberKoordinaten berechnen
        // Fibers in ein Mesh geben
        // Bei "Generate" wird das neu aufgerufen.
        // Das UI greift direkt auf die Start() Methode zu.
        
        // Die sonstigen Methoden sind die von Niclas bereitgestellte ClippingPlane
        GenerateSeedPoints();

        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();
        List<Color> colors = new List<Color>();

        int vertCount = 0;

        Stopwatch sw = new Stopwatch();
        sw.Start();

        foreach (var seed in seedPoints)
        {
            List<Vector3> fiber = TraceFiber(seed);

            // Verwerfen wenn zu kurz
            if (fiber.Count < minFiberLength)
                continue;
            
            // Nur Fasern entlang der Hauptachsen (x, y, z) darstellen
            Vector3 fiberDir = fiber[fiber.Count - 1] - fiber[0];
            if (fiberDir.sqrMagnitude < 1e-6f) continue;

            fiberDir.Normalize();

            float axisDotThreshold = Mathf.Cos(axisToleranceAngle * Mathf.Deg2Rad);

            bool aligned =
                Mathf.Abs(Vector3.Dot(fiberDir, transform.right)) >= axisDotThreshold ||
                Mathf.Abs(Vector3.Dot(fiberDir, transform.up)) >= axisDotThreshold ||
                Mathf.Abs(Vector3.Dot(fiberDir, transform.forward)) >= axisDotThreshold;

            if (!aligned) continue;

            // Setzen der Indices/Vertices pro Fiber-Punkt
            for (int i = 0; i < fiber.Count; i++)
            {
                Vector3 pos = fiber[i];
                vertices.Add(pos);

                // Map [-0.5,0.5] → [0,1]
                Vector3 uvw = pos + Vector3.one * 0.5f;
                Color sample = volume.GetPixelBilinear(uvw.x, uvw.y, uvw.z);

                // Farbe direkt aus dem Volume
                Color c = new Color(Mathf.Abs(sample.r), Mathf.Abs(sample.g), Mathf.Abs(sample.b), sample.a);
                colors.Add(c);

                if (i > 0)
                {
                    indices.Add(vertCount - 1);
                    indices.Add(vertCount);
                }

                vertCount++;
            }
        }

        sw.Stop();
        stopwatchText_TraceFiber = $"TraceFiber:\n- Dauer: {sw.Elapsed.TotalMilliseconds:F2} ms";
        Debug.Log(
            $"TraceFiber:\n" +
            $"- Dauer: {sw.Elapsed.TotalMilliseconds:F2} ms"
        );

        // Mesh-Generierung
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(vertices);
        mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
        mesh.SetColors(colors);
        mesh.RecalculateBounds();

        var mf = GetComponent<MeshFilter>();
        mf.mesh = mesh;

        var mr = GetComponent<MeshRenderer>();
        mr.sharedMaterial = lineMaterial;
    }

    public void generateMesh()
    {
        Start();
    }

    void GenerateSeedPoints()
    {
        // Zeitmessung für Performance-Analyse starten
        Stopwatch sw = new Stopwatch();
        sw.Start();

        // Temporäre Liste für die gefundenen Startpunkte
        List<Vector3> seeds = new List<Vector3>();

        // Dimensionen der 3D-Textur abrufen
        int nx = volume.width;
        int ny = volume.height;
        int nz = volume.depth;

        // Laufvariablen für die Schleifen (als Float, um Auflösungs-Schritte < 1 zu erlauben)
        float xd;
        float yd;
        float zd = 0;

        // Berechnung der Schrittweite basierend auf der gewünschten Auflösung ('resolution')
        // nextVoxelF bestimmt, wie viele Voxel wir pro Schritt überspringen oder feiner abtasten
        float nextVoxelF = 1 / resolution;

        // Iteration durch das gesamte Volumen (Z, Y, X)
        for(int z = 0; z < nz; z = (int)zd){
            yd = 0;
            for(int y = 0; y < ny; y = (int)yd){
                xd = 0;
                for(int x = 0; x < nx; x = (int)xd){
                    
                    // Diffusionsdaten (RGBA) am aktuellen Voxel auslesen
                    Color c = volume.GetPixel(x, y, z);
                    
                    // Laufvariable für den nächsten Schritt erhöhen
                    xd += nextVoxelF;

                    // FA-Filter: Nur Voxel mit ausreichender Anisotropie (c.a) betrachten
                    if(c.a < faThreshold)
                        continue;

                    // Koordinaten-Transformation:
                    // 1. (x + 0.5f) / nx:  Normiert die Voxel-Koordinate auf [0, 1] (UV-Raum) und zentriert sie im Voxel.
                    // 2. - 0.5f:           Verschiebt den Bereich auf [-0.5, 0.5] (Objekt-Zentrum bei 0,0,0).
                    Vector3 pos = new Vector3(
                        (x + 0.5f) / nx - 0.5f,
                        (y + 0.5f) / ny - 0.5f,
                        (z + 0.5f) / nz - 0.5f
                    );
                    
                    // Validen Startpunkt zur Liste hinzufügen
                    seeds.Add(pos);
                }
                yd += nextVoxelF;
            }
            zd += nextVoxelF;
        }

        // Umwandlung der Liste in ein Array
        seedPoints = seeds.ToArray();

        // Zeitmessung stoppen und ausgeben
        sw.Stop();
        stopwatchText_GenerateSeeds = $"GenerateSeeds:\n- Dauer: {sw.Elapsed.TotalMilliseconds:F2} ms\n- Seed-Anzahl: {seedPoints.Length}";
        Debug.Log(
            $"GenerateSeeds:\n" +
            $"- Dauer: {sw.Elapsed.TotalMilliseconds:F2} ms" +
            $"- Seed-Anzahl: " + seedPoints.Length
        );
    }

    // CPU-Implementierung des FACT-Algorithmus
    List<Vector3> TraceFiber(Vector3 pos)
    {
        List<Vector3> fiber = new List<Vector3>();
        Vector3 lastDir = Vector3.zero; // Speichert die Richtung des vorherigen Schritts

        // Maximale Schritte begrenzen, um Endlosschleifen zu verhindern
        for (int i = 0; i < maxSteps; i++)
        {
            // Rückrechnung von Objekt-Koordinaten [-0.5, 0.5] in Textur-Koordinaten [0, 1]
            Vector3 uvw = pos + Vector3.one * 0.5f; 
        
            // Trilineare Interpolation: Liest geglättete Werte zwischen den Voxeln
            Color sample = volume.GetPixelBilinear(uvw.x, uvw.y, uvw.z);
            float fa = sample.a; // Alpha-Kanal enthält die fraktionale Anisotropie (FA)

            // Abbruchbedingung 1: Anisotropie zu gering
            if (fa < faThreshold) break;

            // RGB-Werte repräsentieren den Richtungsvektor (XYZ)
            Vector3 dir = new Vector3(sample.r, sample.g, sample.b);

            // Abbruchbedingung 2: Winkeländerung zu stark (Vermeidung unrealistischer Knicke)
            // Prüft den Winkel zwischen aktueller und letzter Richtung
            if (lastDir != Vector3.zero && Vector3.Angle(lastDir, dir) > factCutoffAngle) break;

            // Aktuelle Position zur Faser hinzufügen
            fiber.Add(pos);
        
            // Gehe eine stepSize in die ausgelesene Richtung
            pos += dir * stepSize;
        
            // Aktuelle Richtung für den nächsten Vergleich speichern
            lastDir = dir;
        }

        return fiber;
    }
}