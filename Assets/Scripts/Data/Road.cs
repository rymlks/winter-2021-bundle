using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class Road : MonoBehaviour
{
    public string roadName;
    public double trafficRate; // cars/day aka "aadt"
    public double trafficSum;  // cars/day * segmentLength aka "vmt"
    public int lanes;
    public Material material;
    public Condition condition;

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
    [HideInInspector]
    public bool isValid = true;
    [HideInInspector]
    public List<Vector3> points;

    private LineRenderer lineRenderer;

    public static float MaxX = float.MinValue;
    public static float MaxY = float.MinValue;
    public static float MinX = float.MaxValue;
    public static float MinY = float.MaxValue;
    private static int positionScale = 50;

    // Useful indices in DbfRecord objects
    private const int I_AADT = 1;  // DBNumeric
    private const int I_ROAD_NAME = 36;  // DBCharacter
    private const int I_SURFACE = 41;  // DBCharacter
    private const int I_LANES = 42;  // DBNumeric
    private const int I_VMT = 35;  // DBNumeric
    private const int I_CONDITION = 40;  // DBCharacter

    public enum Condition
    {
        GOOD = 1,
        FAIR = 5,
        POOR = 10
    }

    public enum Material
    {
        ASPHALT,
        CONCRETE,
        GRAVEL
    }

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
        Assets.DBNumeric vmt = (Assets.DBNumeric)record.DbfRecord.Record[I_VMT];
        Assets.DBNumeric aadt = (Assets.DBNumeric)record.DbfRecord.Record[I_AADT];
        Assets.DBCharacter roadname = (Assets.DBCharacter)record.DbfRecord.Record[I_ROAD_NAME];
        Assets.DBCharacter surface = (Assets.DBCharacter)record.DbfRecord.Record[I_SURFACE];
        Assets.DBNumeric _lanes = (Assets.DBNumeric)record.DbfRecord.Record[I_LANES];
        Assets.DBCharacter _condition = (Assets.DBCharacter)record.DbfRecord.Record[I_CONDITION];

        // Get the line from the GIS record. Ignore if it doesn't have at least 2 points
        Assets.PolyLine pline = (Assets.PolyLine)record.ShpRecord.Contents;
        if (pline.Points.Length <= 1)
        {
            isValid = false;
            Destroy(gameObject);
            return;
        }

        // Update public values
        roadName = roadname.Value.Trim();
        trafficSum = double.Parse(new string(vmt.Value).Trim());
        trafficRate = double.Parse(new string(aadt.Value).Trim());
        lanes = int.Parse(new string(_lanes.Value));
        switch(_condition.Value.ToLower().Trim())
        {
            case "good":
                condition = Condition.GOOD;
                break;
            case "fair":
                condition = Condition.FAIR;
                break;
            case "poor":
                condition = Condition.POOR;
                break;
            default:
                condition = Condition.GOOD;
                break;
        }

        // trafficSum *= (int)condition;
        // trafficSum /= lanes;

        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
        if (points == null)
        {
            points = new List<Vector3>();
        }

        // Update positions and line renderer
        float scaledTrafficRate = Utils.Sigmoid((trafficRate - 20000) / 10000.0f);
        Color color;

        switch(surface.Value.Trim().ToLower())
        {
            case "asphalt":
                material = Material.ASPHALT;
                color = Color.Lerp(lowTrafficAsphalt, highTrafficAsphalt, scaledTrafficRate);
                break;
            case "concrete":
                material = Material.CONCRETE;
                color = Color.Lerp(lowTrafficConcrete, highTrafficConcrete, scaledTrafficRate);
                break;
            case "gravel":
                material = Material.GRAVEL;
                color = Color.Lerp(lowTrafficGravel, highTrafficGravel, scaledTrafficRate);
                break;
            default:
                color = Color.Lerp(lowTrafficAsphalt, highTrafficAsphalt, scaledTrafficRate);
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

        // Simplify the line renderer to remove unnecessary duplicate points
        lineRenderer.Simplify(0.0001f);
        if (lineRenderer.positionCount < 2)
        {
            isValid = false;
            Destroy(gameObject);
            return;
        }
        Vector3[] LRPoints = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(LRPoints);
        points = new List<Vector3>();
        foreach(Vector3 point in LRPoints)
        {
            points.Add(point);
        }

        // Update collider to follow the line
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        Mesh mesh = new Mesh();
        lineRenderer.BakeMesh(mesh, true);

        meshCollider.sharedMesh = mesh;
    }

    public Vector3 RandomPoint()
    {
        int randIdx = new System.Random().Next(points.Count - 1);
        Vector3 p1 = points[randIdx];
        Vector3 p2 = points[randIdx + 1];
        return Vector3.Lerp(p1, p2, UnityEngine.Random.value);
    }

    public static int Sort(Road r1, Road r2)
    {
        return r1.trafficSum.CompareTo(r2.trafficSum);
    }
}
