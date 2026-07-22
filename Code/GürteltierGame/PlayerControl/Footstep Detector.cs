using UnityEngine;

// NOT USED. Meant to enable different "footstep" sounds depending on the surface type
public class FootstepDetector : MonoBehaviour
{
    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("FootstepDetector aktiviert");
            int layerMask = ~LayerMask.GetMask("Player", "WaterSensor");

            Ray ray = new Ray(transform.position + Vector3.down * 0.1f, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, 2f, layerMask))
            {
                Debug.Log("Raycast hit!");
                MeshCollider meshCollider = hit.collider as MeshCollider;
                if (meshCollider == null)
                {
                    Debug.LogWarning("Collider: " + hit.collider + " is not a MeshCollider.");
                    return;
                }

                if (meshCollider.sharedMesh == null)
                {
                    Debug.LogWarning("MeshCollider has no sharedMesh assigned.");
                    return;
                }

                Mesh mesh = meshCollider.sharedMesh;
                int[] triangles = mesh.triangles;
                Color[] colors = mesh.colors;

                if (colors == null || colors.Length == 0)
                {
                    Debug.Log("Mesh has no vertex colors assigned.");
                    return;
                }
                Debug.Log("Still running...");
                // Get indices of the triangle that was hit
                int triangleIndex = hit.triangleIndex * 3;
                int i0 = triangles[triangleIndex + 0];
                int i1 = triangles[triangleIndex + 1];
                int i2 = triangles[triangleIndex + 2];

                // Get vertex colors
                Color c0 = colors[i0];
                Color c1 = colors[i1];
                Color c2 = colors[i2];

                // Interpolate color using barycentric coordinates
                Vector3 bary = hit.barycentricCoordinate;
                Color surfaceColor = c0 * bary.x + c1 * bary.y + c2 * bary.z;

                // Debug the weights
                Debug.Log($"Vertex blend at hit: R={surfaceColor.r}, G={surfaceColor.g}, B={surfaceColor.b}, A={surfaceColor.a}");

                // Determine which texture is most dominant
                float max = Mathf.Max(surfaceColor.r, surfaceColor.g, surfaceColor.b, surfaceColor.a);
                if (max == surfaceColor.r)
                    Debug.Log("surfaceColor: r");
                //AudioController.Instance.PlaySoundForAlbedo(1);
                else if (max == surfaceColor.g)
                    Debug.Log("surfaceColor: g");
                //AudioController.Instance.PlaySoundForAlbedo(2);
                else if (max == surfaceColor.b)
                    Debug.Log("surfaceColor: b");
                //AudioController.Instance.PlaySoundForAlbedo(3);
                else
                    Debug.Log("surfaceColor: undetermined");
                //AudioController.Instance.PlaySoundForAlbedo(4);
            }
            else
            {
                Debug.Log("No Raycast hit");
            }
        }
    }
}