using UnityEngine;

public class GravityAreaFlat : GravityArea
{
    public bool canBeFlipped = false;
    private Material _material;

    [SerializeField] private Vector3 gravityDirection = Vector3.down;
    private float currentTexture = 0f;

    private void Awake()
    {
        // Renderer holen (MeshRenderer, SpriteRenderer etc.)
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            // Eigene Instanz des Materials erstellen, um nicht das Original zu ändern
            _material = renderer.material;
            UpdateShaderGravityDirection();
        }
    }

    public override Vector3 GetGravityDirection(GravityBody body)
    {
        return transform.TransformDirection(gravityDirection.normalized);
    }

    public void FlipGravityDirection()
    {
        gravityDirection = -gravityDirection;
        UpdateShaderGravityDirection();
    }

    private void UpdateShaderGravityDirection()
    {
        if (_material != null)
        {
            _material.SetVector("_GravityDirection", -gravityDirection.normalized);
            if (currentTexture == 0)
            {
                currentTexture = 1;
            } else
            {
                currentTexture = 0;
            }
            _material.SetFloat("_TextureSwitch", currentTexture);
        }
    }
}