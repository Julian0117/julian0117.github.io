using UnityEngine;

public class AxisLineVisualizer : MonoBehaviour
{
    public float lineLength = 1.5f;
    public float lineWidth = 0.05f;

    private LineRenderer xLine;
    private LineRenderer yLine;
    private LineRenderer zLine;

    void Start()
    {
        xLine = CreateLineRenderer("X-Axis", Color.red);
        yLine = CreateLineRenderer("Y-Axis", Color.green);
        zLine = CreateLineRenderer("Z-Axis", Color.blue);
    }

    void Update()
    {
        Vector3 pos = transform.position;

        xLine.SetPosition(0, pos);
        xLine.SetPosition(1, pos + transform.right * lineLength);

        yLine.SetPosition(0, pos);
        yLine.SetPosition(1, pos + transform.up * lineLength);

        zLine.SetPosition(0, pos);
        zLine.SetPosition(1, pos + transform.forward * lineLength);
    }

    private LineRenderer CreateLineRenderer(string name, Color color)
    {
        GameObject lineObj = new GameObject(name);
        lineObj.transform.parent = this.transform;

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.useWorldSpace = true;

        lr.material = new Material(Shader.Find("Sprites/Default")); // transparenter Shader
        lr.startColor = color;
        lr.endColor = color;

        return lr;
    }
}