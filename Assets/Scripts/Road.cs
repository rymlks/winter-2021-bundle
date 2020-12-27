using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class Road : MonoBehaviour
{
    public string roadName;
    public double traffic;
    public string material;
    public int lanes;

    public Material asphaltMaterial;
    public Material concreteMaterial;
    public Material asphaltMaterial1L;
    public Material concreteMaterial1L;

    public Color lowTrafficAsphalt;
    public Color highTrafficAsphalt;

    public Color lowTrafficConcrete;
    public Color highTrafficConcrete;

    public Color lowTrafficGravel;
    public Color highTrafficGravel;

    [HideInInspector]
    public float roadWidthMultiplier;

    private List<Vector3> points;
    private LineRenderer lineRenderer;

    public static float MaxX = float.MinValue;
    public static float MaxY = float.MinValue;
    public static float MinX = float.MaxValue;
    public static float MinY = float.MaxValue;
    private static int positionScale = 50;


    void OnMouseOver()
    {
        if(!EventSystem.current.IsPointerOverGameObject())
        {
            UIController.roadNameToolTipText = roadName;
        }
    }

    void OnMouseExit()
    {
        UIController.roadNameToolTipText = "";
    }

    /**
     * Load data from a GISRecord to represent this road. Update visuals
     */
    public void loadFromGIS(Assets.GISRecord record)
    {
        // Get GIS data from record
        Assets.DBNumeric aadt = (Assets.DBNumeric)record.DbfRecord.Record[1];
        Assets.DBCharacter roadname = (Assets.DBCharacter)record.DbfRecord.Record[36];
        Assets.DBCharacter surface = (Assets.DBCharacter)record.DbfRecord.Record[41];
        Assets.DBNumeric _lanes = (Assets.DBNumeric)record.DbfRecord.Record[42];

        // Get the line from the GIS record. Ignore if it doesn't have at least 2 points
        Assets.PolyLine pline = (Assets.PolyLine)record.ShpRecord.Contents;
        if (pline.Points.Length <= 1)
        {
            Destroy(this);
            return;
        }

        // Update public values
        roadName = roadname.Value.Trim();
        traffic = int.Parse(new string(aadt.Value));
        material = surface.Value.Trim().ToLower();
        lanes = int.Parse(new string(_lanes.Value));
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
        if (points == null)
        {
            points = new List<Vector3>();
        }

        // Update positions and line renderer
        float scaledTraffic = Utils.Sigmoid((traffic - 20000) / 10000.0f);
        Color color;

        switch(material)
        {
            case "asphalt":
                color = Color.Lerp(lowTrafficAsphalt, highTrafficAsphalt, scaledTraffic);
                break;
            case "concrete":
                color = Color.Lerp(lowTrafficConcrete, highTrafficConcrete, scaledTraffic);
                break;
            case "gravel":
                color = Color.Lerp(lowTrafficGravel, highTrafficGravel, scaledTraffic);
                break;
            default:
                color = Color.Lerp(lowTrafficAsphalt, highTrafficAsphalt, scaledTraffic);
                break;
        }
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        lineRenderer.widthMultiplier = lanes * roadWidthMultiplier;

        lineRenderer.positionCount = pline.NumPoints;
        for (int j = 0; j < pline.Points.Length; j++)
        {
            Assets.Point p = pline.Points[j];
            Vector3 v = new Vector3((float)(p.X * positionScale), (float)(p.Y * positionScale), 0);
            points.Add(v);

            MaxX = Mathf.Max(MaxX, v.x);
            MaxY = Mathf.Max(MaxY, v.y);
            MinX = Mathf.Min(MinX, v.x);
            MinY = Mathf.Min(MinY, v.y);
        }
        lineRenderer.SetPositions(points.ToArray());

        // Update collider to follow the line
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        Mesh mesh = new Mesh();
        lineRenderer.BakeMesh(mesh, true);

        meshCollider.sharedMesh = mesh;
    }
}
