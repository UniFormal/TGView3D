using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrackerFrustum : MonoBehaviour
{
    public int SensorID { get; set; }

    public bool ShowLines
    {
        get { return m_showLines; }
        set
        {
            m_showLines = value;
            ShowFrustum(m_showFrustum);
        }
    }
    private bool m_showLines = true;
    public bool ShowGeo
    {
        get { return m_showGeo; }
        set
        {
            m_showGeo = value;
            ShowFrustum(m_showFrustum);
        }
    }
    private bool m_showGeo = false;

    // Material used for frustum LineRenderers
    public Material lineMaterial;

    // LineRenderer properties
    public Color lineColor = Color.white;
    public Color lineColor2 = Color.white;
    public float lineThickness = 0.002f;

    // List of sensor frustum wireframe line objects
    private List<GameObject> lines = new List<GameObject>();

    // Flags whether the sensor frustum has been generated yet
    private bool m_generated = false;

    // Flags whether to show the sensor frustum
    private bool m_showFrustum = true;

    // Sets whether to show the frustum
    public void ShowFrustum(bool show)
    {
        m_showFrustum = show;

        // Display all frustum lines
        foreach (GameObject l in lines)
        {
            l.SetActive(m_showFrustum && ShowLines);
        }
        GetComponent<MeshRenderer>().enabled = m_showFrustum && m_showGeo;
    }

    void Update()
    {
        transform.localPosition = OVRManager.tracker.GetPose(SensorID).position;
        Vector3 orientation = OVRManager.tracker.GetPose(SensorID).orientation.eulerAngles;
        transform.localRotation = Quaternion.Euler(0.0f, orientation.y, 0.0f);

        if(!m_generated && OVRManager.tracker.isPositionTracked)
        {
            GenerateFrustum();
        }
    }

    private void GenerateFrustum()
    {
        // Get frustum shape data
        Vector2 cameraFov = OVRManager.tracker.GetFrustum().fov;
        float near = OVRManager.tracker.GetFrustum().nearZ;
        float far = OVRManager.tracker.GetFrustum().farZ;

        // Calculate sensor horizontal and vertical FOV
        float fovHoriz = 0.5f * cameraFov.x * Mathf.PI / 180.0f;
        float fovVert = 0.5f * cameraFov.y * Mathf.PI / 180.0f;

        // Define corner points of the tracking frustum
        Vector3 nearTopLeft = new Vector3(near * Mathf.Tan(fovHoriz), near * Mathf.Tan(fovVert), -near);
        Vector3 nearBottomLeft = new Vector3(near * Mathf.Tan(fovHoriz), -near * Mathf.Tan(fovVert), -near);
        Vector3 nearTopRight = new Vector3(-near * Mathf.Tan(fovHoriz), near * Mathf.Tan(fovVert), -near);
        Vector3 nearBottomRight = new Vector3(-near * Mathf.Tan(fovHoriz), -near * Mathf.Tan(fovVert), -near);
        Vector3 farTopLeft = new Vector3(far * Mathf.Tan(fovHoriz), far * Mathf.Tan(fovVert), -far);
        Vector3 farBottomLeft = new Vector3(far * Mathf.Tan(fovHoriz), -far * Mathf.Tan(fovVert), -far);
        Vector3 farTopRight = new Vector3(-far * Mathf.Tan(fovHoriz), far * Mathf.Tan(fovVert), -far);
        Vector3 farBottomRight = new Vector3(-far * Mathf.Tan(fovHoriz), -far * Mathf.Tan(fovVert), -far);

        // Define vertex pairs for frustum wireframe rendering
        List<Vector3[]> vertices = new List<Vector3[]>();

        vertices.Add(new Vector3[] { nearTopLeft, nearTopRight });
        vertices.Add(new Vector3[] { nearTopRight, nearBottomRight });
        vertices.Add(new Vector3[] { nearBottomRight, nearBottomLeft });
        vertices.Add(new Vector3[] { nearBottomLeft, nearTopLeft });

        vertices.Add(new Vector3[] { nearTopLeft, farTopLeft });
        vertices.Add(new Vector3[] { nearTopRight, farTopRight });
        vertices.Add(new Vector3[] { nearBottomRight, farBottomRight });
        vertices.Add(new Vector3[] { nearBottomLeft, farBottomLeft });

        vertices.Add(new Vector3[] { farTopLeft, farTopRight });
        vertices.Add(new Vector3[] { farTopRight, farBottomRight });
        vertices.Add(new Vector3[] { farBottomRight, farBottomLeft });
        vertices.Add(new Vector3[] { farBottomLeft, farTopLeft });

        // Generate frustum wireframe lines
        int count = 0;
        foreach (Vector3[] pair in vertices)
        {
            GameObject obj = new GameObject("FrustumParent");
            obj.transform.SetParent(transform, false);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));

            LineRenderer lr = obj.AddComponent<LineRenderer>();

            float lineWidth = lineThickness;

            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.material = lineMaterial;
            lr.startColor = lineColor;
            lr.endColor = count == 0 ? lineColor2 : lineColor;
            lr.positionCount = 2;
            lr.SetPosition(0, pair[0]);
            lr.SetPosition(1, pair[1]);
            lr.useWorldSpace = false;
            lr.name = "FrustumLine";

            lines.Add(obj);

            count++;
        }

        CreateMeshCollider();

        m_generated = true;
    }

    private void CreateMeshCollider()
    {
        GameObject colObject = gameObject;
        colObject.transform.parent = transform;
        colObject.transform.localPosition = new Vector3(0, 0, 0);

        Mesh mesh = new Mesh();

        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        // Get frustum shape data
        Vector2 cameraFov = OVRManager.tracker.GetFrustum().fov;
        float near = -OVRManager.tracker.GetFrustum().nearZ;
        float far = -OVRManager.tracker.GetFrustum().farZ;

        // Calculate sensor horizontal and vertical FOV
        float fovHoriz = 0.5f * cameraFov.x * Mathf.PI / 180.0f;
        float fovVert = 0.5f * cameraFov.y * Mathf.PI / 180.0f;

        // Define corner points of the tracking frustum
        Vector3 nearTopLeft = new Vector3(near * Mathf.Tan(fovHoriz), near * Mathf.Tan(fovVert), -near);
        Vector3 nearBottomLeft = new Vector3(near * Mathf.Tan(fovHoriz), -near * Mathf.Tan(fovVert), -near);
        Vector3 nearTopRight = new Vector3(-near * Mathf.Tan(fovHoriz), near * Mathf.Tan(fovVert), -near);
        Vector3 nearBottomRight = new Vector3(-near * Mathf.Tan(fovHoriz), -near * Mathf.Tan(fovVert), -near);
        Vector3 farTopLeft = new Vector3(far * Mathf.Tan(fovHoriz), far * Mathf.Tan(fovVert), -far);
        Vector3 farBottomLeft = new Vector3(far * Mathf.Tan(fovHoriz), -far * Mathf.Tan(fovVert), -far);
        Vector3 farTopRight = new Vector3(-far * Mathf.Tan(fovHoriz), far * Mathf.Tan(fovVert), -far);
        Vector3 farBottomRight = new Vector3(-far * Mathf.Tan(fovHoriz), -far * Mathf.Tan(fovVert), -far);

        verts.Add(nearTopLeft);
        verts.Add(nearBottomLeft);
        verts.Add(nearTopRight);
        verts.Add(nearBottomRight);
        verts.Add(farTopLeft);
        verts.Add(farBottomLeft);
        verts.Add(farTopRight);
        verts.Add(farBottomRight);

        // Front
        tris.Add(2);
        tris.Add(3);
        tris.Add(0);
        tris.Add(0);
        tris.Add(3);
        tris.Add(1);

        // Back
        tris.Add(4);
        tris.Add(5);
        tris.Add(6);
        tris.Add(6);
        tris.Add(5);
        tris.Add(7);

        // Top
        tris.Add(0);
        tris.Add(4);
        tris.Add(2);
        tris.Add(2);
        tris.Add(4);
        tris.Add(6);

        // Bottom
        tris.Add(5);
        tris.Add(1);
        tris.Add(7);
        tris.Add(7);
        tris.Add(1);
        tris.Add(3);

        // Left
        tris.Add(0);
        tris.Add(1);
        tris.Add(4);
        tris.Add(4);
        tris.Add(1);
        tris.Add(5);

        // Right
        tris.Add(6);
        tris.Add(7);
        tris.Add(2);
        tris.Add(2);
        tris.Add(7);
        tris.Add(3);

        mesh.name = "Frustum";
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        colObject.GetComponent<MeshFilter>().mesh = mesh;
        colObject.GetComponent<MeshCollider>().sharedMesh = mesh;

        mesh = new Mesh();
        verts = new List<Vector3>();
        tris = new List<int>();

        float bottomY = 0.0f;
        nearTopLeft.y = bottomY; verts.Add(nearTopLeft);
        nearBottomLeft.y = bottomY;verts.Add(nearBottomLeft);
        nearTopRight.y = bottomY;verts.Add(nearTopRight);
        nearBottomRight.y = bottomY;verts.Add(nearBottomRight);
        farTopLeft.y = bottomY;verts.Add(farTopLeft);
        farBottomLeft.y = bottomY;verts.Add(farBottomLeft);
        farTopRight.y = bottomY;verts.Add(farTopRight);
        farBottomRight.y = bottomY;verts.Add(farBottomRight);

        verts.Add(nearTopLeft);
        verts.Add(nearBottomLeft);
        verts.Add(nearTopRight);
        verts.Add(nearBottomRight);
        verts.Add(farTopLeft);
        verts.Add(farBottomLeft);
        verts.Add(farTopRight);
        verts.Add(farBottomRight);

        // Bottom
        tris.Add(3);
        tris.Add(1);
        tris.Add(7);
        tris.Add(7);
        tris.Add(1);
        tris.Add(5);

        mesh.name = "Floor";
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}
