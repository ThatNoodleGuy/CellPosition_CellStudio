using System.Collections.Generic;
using UnityEngine;
using System.Xml;

[RequireComponent(typeof(LineRenderer))]
public class SimulationAreaRenderer : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private List<Vector3> points = new List<Vector3>();

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        ParseXMLAndGeneratePoints();
        SetupLineRenderer();
        DrawShape();
    }

    void ParseXMLAndGeneratePoints()
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load("Assets/Resources/E0.xml");
        XmlNode areaSizeNode = xmlDoc.SelectSingleNode("//Environments/array_element_0/area_size");

        float x = float.Parse(areaSizeNode.SelectSingleNode("x").InnerText);
        float y = float.Parse(areaSizeNode.SelectSingleNode("y").InnerText);
        float z = float.Parse(areaSizeNode.SelectSingleNode("z").InnerText);

        // Generate the points based on area_size
        points.Add(new Vector3(0, 0, 0)); // Bottom Front Left
        points.Add(new Vector3(x, 0, 0)); // Bottom Front Right
        points.Add(new Vector3(x, 0, z)); // Bottom Back Right
        points.Add(new Vector3(0, 0, z)); // Bottom Back Left
        points.Add(new Vector3(0, 0, 0)); // Close bottom square
        points.Add(new Vector3(0, y, 0)); // Top Front Left (start drawing the vertical lines and top square)
        points.Add(new Vector3(x, y, 0)); // Top Front Right
        points.Add(new Vector3(x, 0, 0)); // Back to Bottom Front Right
        points.Add(new Vector3(x, y, 0)); // Top Front Right
        points.Add(new Vector3(x, y, z)); // Top Back Right
        points.Add(new Vector3(x, 0, z)); // Back to Bottom Back Right
        points.Add(new Vector3(x, y, z)); // Top Back Right
        points.Add(new Vector3(0, y, z)); // Top Back Left
        points.Add(new Vector3(0, 0, z)); // Back to Bottom Back Left
        points.Add(new Vector3(0, y, z)); // Top Back Left
        points.Add(new Vector3(0, y, 0)); // Top Front Left
    }

    void SetupLineRenderer()
    {
        // Configure your line renderer here (e.g., width, color)
        lineRenderer.startWidth = 10f;
        lineRenderer.endWidth = 10f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
    }

    void DrawShape()
    {
        if (points.Count < 2)
        {
            Debug.LogError("Need at least two points to draw a line.");
            return;
        }

        lineRenderer.positionCount = points.Count;
        
        for (int i = 0; i < points.Count; i++)
        {
            lineRenderer.SetPosition(i, points[i]);
        }
    }
}