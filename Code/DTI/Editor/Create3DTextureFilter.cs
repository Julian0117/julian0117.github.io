/* Basiert auf https://docs.unity3d.com/6000.2/Documentation/Manual/class-Texture3D-create.html */

using UnityEditor;
using UnityEngine;
using System.IO;

public class Create3DTextureFilter : MonoBehaviour
{
    [MenuItem("Create/3D Texture from Data (Filtered)")]
    static void CreateTexture3DFromFile()
    {
        // Dateipfad für Rohdaten
        string filePath = "Assets/Data/dataTex.dat";
        // Dimensionen der Rohdaten müssen hier gesetzt werden
        int dimX = 176;
        int dimY = 176;
        int dimZ = 114;

        if (!File.Exists(filePath))
        {
            Debug.LogError("Datei nicht gefunden: " + filePath);
            return;
        }

        // Liest die Rohdaten als Byte-Array ein
        byte[] bytes = File.ReadAllBytes(filePath);
        int expectedFloats = dimX * dimY * dimZ * 4; // XYZ pro Voxel
        
        // Prüft, ob die Dateigröße unseren angegebenen Dimensionen entspricht
        if (bytes.Length != expectedFloats * sizeof(float))
        {
            Debug.LogError($"Dateigröße stimmt nicht. Erwartet: {expectedFloats * sizeof(float)} Bytes, gefunden: {bytes.Length}");
            return;
        }

        // Konvertiert eingelesene Bytes in Float-Werte für die Weiterverarbeitung
        float[] floatValues = new float[expectedFloats];
        for (int i = 0; i < expectedFloats; i++)
            floatValues[i] = System.BitConverter.ToSingle(bytes, i * sizeof(float));

        // Erstellt eine Unity 3D-Textur im RGBA-Format
        Texture3D texture = new Texture3D(dimX, dimY, dimZ, TextureFormat.RGBAFloat, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        Color[] colors = new Color[dimX * dimY * dimZ];

        // Bool-Maske aller aktiven Voxel
        // Filtert Nullvektoren aus dem Datensatz und markiert diese in der Maske als "False"
        bool[,,] mask = new bool[dimX, dimY, dimZ];
        int activeVoxels = 0; // Debug
        for (int z = 0; z < dimZ; z++)
            for (int y = 0; y < dimY; y++)
                for (int x = 0; x < dimX; x++)
                {
                    int idx = (x + y * dimX + z * dimX * dimY) * 4;
                    float vx = floatValues[idx];
                    float vy = floatValues[idx + 1];
                    float vz = floatValues[idx + 2];

                    mask[x, y, z] = (new Vector3(vx, vy, vz).sqrMagnitude > 1e-6f);
                    // Zähler erhöhen, wenn Voxel erhalten bleibt (Für Debug-Ausgabe)
                    if (new Vector3(vx, vy, vz).sqrMagnitude > 1e-6f) activeVoxels++;
                }

        // Prüft auf Mindestanzahl von aktiven Nachbarn und schreibt gültige Voxel-Werte in die 3D-Textur
        // (ungültige werden mit Alpha = 0 "unsichtbar" gemacht)
        int radius = 5;         // 11x11x11 Nachbarschaft (2r + 1)^3
        int threshold = 800;    // Minimum aktive Nachbarn im Radius
        int keptVoxels = 0;     // Zähler für behaltene Voxel
        for (int z = 0; z < dimZ; z++)
            for (int y = 0; y < dimY; y++)
                for (int x = 0; x < dimX; x++)
                {
                    int idx = (x + y * dimX + z * dimX * dimY) * 4;
                    float vx = floatValues[idx];
                    float vy = floatValues[idx + 1];
                    float vz = floatValues[idx + 2];

                    float r = Mathf.Abs(vx);
                    float g = Mathf.Abs(vy);
                    float b = Mathf.Abs(vz);
                    float alpha = 1f;

                    if (mask[x, y, z])
                    {
                        // Nachbarschaft prüfen
                        int neighbors = 0;
                        for (int dz = -radius; dz <= radius; dz++)
                            for (int dy = -radius; dy <= radius; dy++)
                                for (int dx = -radius; dx <= radius; dx++)
                                {
                                    int nx = x + dx;
                                    int ny = y + dy;
                                    int nz = z + dz;
                                    if (nx < 0 || nx >= dimX || ny < 0 || ny >= dimY || nz < 0 || nz >= dimZ) continue;
                                    if (dx == 0 && dy == 0 && dz == 0) continue;
                                    if (mask[nx, ny, nz]) neighbors++;
                                }

                        if (neighbors < threshold)
                            alpha = 0f;
                    }
                    else
                    {
                        alpha = 0f;
                    }

                    // Voxel mit Betrag in Texturarray speichern (Besser später bei Mesh-Erzeugung machen)
                    //colors[x + y * dimX + z * dimX * dimY] = new Color(r, g, b, alpha);
                    // Voxel ohne Betrag in Texturarray speichern
                    colors[x + y * dimX + z * dimX * dimY] = new Color(vx, vy, vz, alpha);

                    // Zähler erhöhen, wenn Voxel erhalten bleibt
                    if (alpha > 0f)
                        keptVoxels++;
                }

        Debug.Log($"Nach Nullvektor Filterung: {activeVoxels} von {dimX * dimY * dimZ} Voxel");
        Debug.Log($"Nach Neighbour-Filterung: {keptVoxels} von {activeVoxels} Voxel");

        texture.SetPixels(colors);
        texture.Apply();
        AssetDatabase.CreateAsset(texture, "Assets/3DTexture_From_dataTex_filtered_no_abs_5_800.asset");
        AssetDatabase.SaveAssets();

        Debug.Log("3D Texture from dataTex.dat created with neighbour filtering!");
    }
}