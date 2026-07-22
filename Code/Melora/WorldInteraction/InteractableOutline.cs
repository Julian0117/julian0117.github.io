using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class InteractableOutline : MonoBehaviour
{
    [SerializeField] private Color outlineColor = Color.yellow;
    [SerializeField] private float outlineThickness = 5f;
    [SerializeField] private Material outlineMaterial;

    private Material instanceMaterial;
    private SpriteRenderer sr;

    private static readonly int OutlineColorID = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineThicknessID = Shader.PropertyToID("_OutlineThickness");

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        // eigene Materialinstanz, damit sich Änderungen nicht auf alle Objekte auswirken
        instanceMaterial = new Material(outlineMaterial);
        sr.material = instanceMaterial;

        HideOutline(); 
    }

    public void ShowOutline()
    {
        //Debug.Log($"Show Outline{this.name}");
        instanceMaterial.SetFloat(OutlineThicknessID, outlineThickness);
        instanceMaterial.SetColor(OutlineColorID, outlineColor);
    }

    public void HideOutline()
    {
        //Debug.Log($"Hide Outline{this.name}");
        instanceMaterial.SetFloat(OutlineThicknessID, 0f);
    }
}