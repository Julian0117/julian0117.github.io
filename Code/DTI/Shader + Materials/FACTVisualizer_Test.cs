using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FACTVisualizer_Test : MonoBehaviour
{
    public Texture3D volume;        // Diffusionsrichtung RGBA
    public Material lineMaterial;

    public float stepSize = 0.01f;
    public float faThreshold = 0.2f;
    public int maxSteps = 500;
    public float minFiberLength = 0.05f; // in Objekt-Raum-Einheiten
    public float resolution = 1f;
    public int factCutoffAngle = 45;
    public int axisToleranceAngle = 20;

    private Vector3[] seedPoints;
    /*
    public GameObject clippingPlane;
    public Renderer renderer;

    
    /// <summary>
    ///     getter and setter method for the uniform <c>clippingEnable</c> inside the raycasting shader.
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
    ///     getter and setter method for the uniform <c>clippingOrigin</c> inside the raycasting shader.
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
    ///     getter and setter method for the uniform <c>clippingNormal</c> inside the raycasting shader.
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
    

    void FixedUpdate()
    {
        if (clippingPlane != null)
        {
            this.shaderClippingOrigin = clippingPlane.transform.position;
            this.shaderClippingNormal = clippingPlane.transform.right;
            //this.shaderClippingEnable = true;
        }
        else
            this.shaderClippingEnable = false;
    }
    */

    void Start()
    {
        GenerateSeedPoints();

        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();
        List<Color> colors = new List<Color>();
        
        // 1. Occupancy Grid initialisieren (gleiche Auflösung wie das Volume)
        bool[,,] occupiedVoxels = new bool[volume.width, volume.height, volume.depth];

        int vertCount = 0;
        int uniqueFiberCount = 0;
        
        // Variablen für die längste Faser
        List<Vector3> longestFiber = new List<Vector3>();
        float maxDistance = 0f;
        
        var timer = new Stopwatch();
        timer.Start();

        foreach (var seed in seedPoints)
        {
            // Check: Wurde der Startpunkt schon von einer anderen Faser "besetzt"?
            Vector3Int seedVoxel = WorldToVoxel(seed);
            if (IsVoxelOccupied(seedVoxel, occupiedVoxels)) continue;

            List<Vector3> fiber = TraceFiber(seed);

            // --- VALIDIERUNG & LÄNGENBERECHNUNG ---
            float currentFiberLength = 0;
            for (int i = 1; i < fiber.Count; i++)
            {
                currentFiberLength += Vector3.Distance(fiber[i], fiber[i - 1]);
            }

            // Filter: Zu kurz?
            if (currentFiberLength < minFiberLength || fiber.Count < 2)
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
            
            // --- TRACKING DER LÄNGSTEN FASER ---
            if (currentFiberLength > maxDistance)
            {
                maxDistance = currentFiberLength;
                longestFiber = new List<Vector3>(fiber);
            }

            // --- OCCUPANCY GRID BEFÜLLEN ---
            // Wir markieren jeden Voxel, durch den diese Faser läuft, als besetzt
            foreach (Vector3 pos in fiber)
            {
                Vector3Int v = WorldToVoxel(pos);
                if (IsValidVoxel(v))
                    occupiedVoxels[v.x, v.y, v.z] = true;
            }

            uniqueFiberCount++;

            for (int i = 0; i < fiber.Count; i++)
            {
                Vector3 pos = fiber[i];
                vertices.Add(pos);

                // Map [-0.5,0.5] → [0,1]
                Vector3 uvw = pos + Vector3.one * 0.5f;
                Color sample = volume.GetPixelBilinear(uvw.x, uvw.y, uvw.z);

                // Farbe direkt aus dem Volume
                Color c = new Color(Mathf.Abs(sample.r), Mathf.Abs(sample.g), Mathf.Abs(sample.b), sample.a);
                //Color c = new Color(sample.r, sample.g, sample.b, sample.a);
                colors.Add(c);

                if (i > 0)
                {
                    indices.Add(vertCount - 1);
                    indices.Add(vertCount);
                }

                vertCount++;
            }
        }
        
        TimeSpan timeTaken = timer.Elapsed;
        Debug.Log($"FACTVisualizer hat: {timeTaken}s gebraucht");
        Debug.Log($"Insgesamt wurden {uniqueFiberCount} Fasern gefunden");
        Debug.Log($"Längste Faser: {maxDistance:F2} Einheiten mit {longestFiber.Count} Punkten.");
        
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

    void GenerateSeedPoints()
    {
        List<Vector3> seeds = new List<Vector3>();

        int nx = volume.width;
        int ny = volume.height;
        int nz = volume.depth;

        int nextVoxel = (int)(1 / resolution);

        for (int z = 0; z < nz; z += nextVoxel)
            for (int y = 0; y < ny; y += nextVoxel)
                for (int x = 0; x < nx; x += nextVoxel)
                {
                    Color c = volume.GetPixel(x, y, z);

                    if (c.a < faThreshold)
                        continue;

                    // Voxelzentrum → Objekt-Raum [-0.5,0.5]
                    Vector3 pos = new Vector3(
                        (x + 0.5f) / nx - 0.5f,
                        (y + 0.5f) / ny - 0.5f,
                        (z + 0.5f) / nz - 0.5f
                    );

                    seeds.Add(pos);
                }

        seedPoints = seeds.ToArray();
    }

    // Hilfsmethode für das Tracking in EINE Richtung
    List<Vector3> TraceSingleDirection(Vector3 startPos, Vector3 startDir, bool invertInitialDir)
    {
        List<Vector3> line = new List<Vector3>();
        Vector3 pos = startPos;
        // Wenn wir in die Gegenrichtung wollen, drehen wir den Startvektor um
        Vector3 lastDir = invertInitialDir ? -startDir : startDir;

        // Startpunkt NICHT hinzufügen, das machen wir beim Mergen, 
        // sonst ist er doppelt vorhanden.

        for (int i = 0; i < maxSteps; i++)
        {
            // Integration: Schritt machen
            pos += lastDir * stepSize;

            // Bounds Check (sind wir noch im Würfel?)
            if (pos.x < -0.5f || pos.x > 0.5f ||
               pos.y < -0.5f || pos.y > 0.5f ||
               pos.z < -0.5f || pos.z > 0.5f) break;

            Vector3 uvw = pos + Vector3.one * 0.5f;
            Color sample = volume.GetPixelBilinear(uvw.x, uvw.y, uvw.z);

            if (sample.a < faThreshold) break; // FA Threshold Check

            Vector3 dir = new Vector3(sample.r, sample.g, sample.b);

            // Wenn der neue Vektor entgegengesetzt zum alten zeigt, drehen wir ihn um.
            if (Vector3.Dot(lastDir, dir) < 0)
            {
                dir = -dir;
            }

            // Winkel-Abbruchbedingung (Krümmung zu stark?)
            if (Vector3.Angle(lastDir, dir) > factCutoffAngle)
            {
                break;
            }

            line.Add(pos);
            lastDir = dir;
        }
        return line;
    }

    List<Vector3> TraceFiber(Vector3 seed)
    {
        // Initialen Richtungsvektor am Seed holen
        Vector3 uvw = seed + Vector3.one * 0.5f;
        Color seedSample = volume.GetPixelBilinear(uvw.x, uvw.y, uvw.z);

        if (seedSample.a < faThreshold) return new List<Vector3>();

        Vector3 seedDir = new Vector3(seedSample.r, seedSample.g, seedSample.b) * 2f - Vector3.one;
        seedDir.Normalize();

        // 1. Vorwärts tracen
        List<Vector3> forwardParams = TraceSingleDirection(seed, seedDir, false);

        // 2. Rückwärts tracen
        List<Vector3> backwardParams = TraceSingleDirection(seed, seedDir, true);

        // 3. Zusammenfügen: Rückwärts (umgedreht) + Seed + Vorwärts
        backwardParams.Reverse();
        backwardParams.Add(seed);
        backwardParams.AddRange(forwardParams);

        return backwardParams;
    }
    
    // --- HILFSFUNKTIONEN FÜR DAS GRID ---
    // Wandelt die Objekt-Raum Position [-0.5, 0.5] in Voxel-Indizes um
    Vector3Int WorldToVoxel(Vector3 pos)
    {
        return new Vector3Int(
            Mathf.FloorToInt((pos.x + 0.5f) * volume.width),
            Mathf.FloorToInt((pos.y + 0.5f) * volume.height),
            Mathf.FloorToInt((pos.z + 0.5f) * volume.depth)
        );
    }

    bool IsValidVoxel(Vector3Int v)
    {
        return v.x >= 0 && v.x < volume.width &&
               v.y >= 0 && v.y < volume.height &&
               v.z >= 0 && v.z < volume.depth;
    }

    bool IsVoxelOccupied(Vector3Int v, bool[,,] grid)
    {
        if (!IsValidVoxel(v)) return true; // Außerhalb ist "besetzt" oder ungültig
        return grid[v.x, v.y, v.z];
    }

    void CreateMesh(List<Vector3> v, List<int> ind, List<Color> c)
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(v);
        mesh.SetIndices(ind.ToArray(), MeshTopology.Lines, 0);
        mesh.SetColors(c);
        GetComponent<MeshFilter>().mesh = mesh;
    }
}