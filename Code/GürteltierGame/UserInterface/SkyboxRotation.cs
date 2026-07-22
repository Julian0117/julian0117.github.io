using UnityEngine;

public class SkyboxRotation : MonoBehaviour
{
    public float rotationSpeed = 1f;

    private void Update()
    {
        float currentRotation = RenderSettings.skybox.GetFloat("_Rotation");
        currentRotation += rotationSpeed * Time.deltaTime;
        RenderSettings.skybox.SetFloat("_Rotation", currentRotation);
    }
}