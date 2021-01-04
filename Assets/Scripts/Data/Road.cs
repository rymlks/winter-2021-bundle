using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class Road : MonoBehaviour
{
    public string roadName;
    public double trafficRate; // cars/day aka "aadt"
    public double trafficSum;  // cars/day * length aka "vmt"
    public double length;
    public int lanes;
    public int ID;
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
    [HideInInspector]
    public List<GameObject> potholes;
    [HideInInspector]
    public float rePaveCost;
    [HideInInspector]
    public ContextMenuController contextMenuController;
    [HideInInspector]
    public BalanceParameters balanceParameters;
    [HideInInspector]
    public PlaythroughStatistics playthroughStatistics;

    private LineRenderer lineRenderer;
    private bool _selected = false;
    private Vector3 _initialScale;

    public static float MaxX = float.MinValue;
    public static float MaxY = float.MinValue;
    public static float MinX = float.MaxValue;
    public static float MinY = float.MaxValue;
    private static int positionScale = 50;

    // Useful indices in DbfRecord objects
    private const int I_FID = 0;  // DBNumeric
    private const int I_AADT = 1;  // DBNumeric
    private const int I_ROAD_NAME = 36;  // DBCharacter
    private const int I_SURFACE = 41;  // DBCharacter
    private const int I_LANES = 42;  // DBNumeric
    private const int I_VMT = 35;  // DBNumeric
    private const int I_CONDITION = 40;  // DBCharacter
    private const int I_SHAPE_LEN = 50;  // DBNumeric

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
        GRAVEL,
    }

    public void Start()
    {
        _initialScale = new Vector3(transform.localScale.x, transform.localScale.y, 1);
    }

    public void Update()
    {
        if (_selected)
        {
            float scale = 1.0f + Mathf.Sin(Time.time * 5) * 0.125f;
            lineRenderer.widthMultiplier = lanes * roadWidthMultiplier * scale;
        }
    }

    void OnMouseOver()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            UIController.roadNameToolTipText = roadName;
        }
    }

    void OnMouseExit()
    {
        UIController.roadNameToolTipText = "";
    }

    void OnMouseUpAsButton()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        DoClick();
    }

    public void DoClick()
    {
        contextMenuController.Open(this);
        Select();
    }

    public void Select()
    {
        _selected = true;
        GetComponent<MeshCollider>().enabled = false;
        Colorize(Color.blue);

        foreach (GameObject pothole in potholes)
        {
            pothole.GetComponent<Pothole>().Select();
        }
    }

    public void Deselect()
    {
        _selected = false;
        GetComponent<MeshCollider>().enabled = true;
        Colorize();
        foreach (GameObject pothole in potholes)
        {
            pothole.GetComponent<Pothole>().Deselect();
        }
        lineRenderer.widthMultiplier = lanes * roadWidthMultiplier;
    }

    public string GetContextMessage()
    {
        string message = "";
        message += roadName;
        message += "\nSegment ID: " + ID;
        message += "\nDaily Drivers: " + trafficRate;
        message += "\nPotholes: " + potholes.Count;
        message += "\nMaterial: " + material.ToString();
        //message += "\nCondition: " + condition.ToString();
        message += "\nLanes: " + lanes;
        return message;
    }

    public void TryRePave(float cost, Material material, Condition condition)
    {

        if (CanAffordRePave(cost))
        {
            DeductCost(cost);
            RePave(material, condition);
        }
        else
        {
            Debug.Log("Cannot patch pothole: insufficient funds.  Have " + this.playthroughStatistics.currentBudget + ", need " + cost);
        }
    }

    private void RePave(Material material, Condition condition)
    {
        this.condition = condition;
        this.material = material;

        foreach (GameObject pothole in potholes)
        {
            Destroy(pothole);
        }

        potholes = new List<GameObject>();

        Colorize();
    }

    private float DeductCost(float cost)
    {
        return this.playthroughStatistics.currentBudget -= cost;
    }

    public bool CanAffordRePave(float cost)
    {
        return this.playthroughStatistics.currentBudget >= cost;
    }

    public float GetRePaveCost(Material material, Condition condition)
    {
        float baseCost;
        if (material == Material.ASPHALT && condition == Condition.FAIR)
        {
            baseCost = balanceParameters.lowGradeAsphaltCostPerFoot;
        }
        else if (material == Material.ASPHALT && condition == Condition.GOOD)
        {
            baseCost = balanceParameters.highGradeAsphaltCostPerFoot;
        }
        else if (material == Material.CONCRETE && condition == Condition.FAIR)
        {
            baseCost = balanceParameters.lowGradeConcreteCostPerFoot;
        }
        else if (material == Material.CONCRETE && condition == Condition.GOOD)
        {
            baseCost = balanceParameters.highGradeConcreteCostPerFoot;
        } else if (material == Material.GRAVEL)
        {
            baseCost = balanceParameters.gravelCostPerFoot;
        } else
        {
            Debug.LogError("Unexpected road repair combination: " + material + ", " + condition);
            baseCost = -1;
        }

        return baseCost * (float)length * lanes;
    }

    public List<ContextMenuController.ContextMenuOption> GetRepairOptions()
    {
        List<ContextMenuController.ContextMenuOption> options = new List<ContextMenuController.ContextMenuOption>
        {
            CreateOption("Shit asphalt", Material.ASPHALT, Condition.FAIR),
            CreateOption("Good asphalt", Material.ASPHALT, Condition.GOOD),

            CreateOption("Shit concrete", Material.CONCRETE, Condition.FAIR),
            CreateOption("Good concrete", Material.CONCRETE, Condition.GOOD),

            CreateOption("Gravel", Material.GRAVEL, Condition.GOOD)
        };

        return options;
    }

    private ContextMenuController.ContextMenuOption CreateOption(string label, Material material, Condition condition)
    {
        float cost = GetRePaveCost(material, condition);
        return new ContextMenuController.ContextMenuOption(label, cost, delegate { TryRePave(cost, material, condition); });
    }

    private void Colorize()
    {
        // Update positions and line renderer
        float scaledTrafficRate = Utils.Sigmoid((trafficRate - 20000) / 10000.0f);
        Color color;

        switch (material)
        {
            case Material.ASPHALT:
                color = Color.Lerp(lowTrafficAsphalt, highTrafficAsphalt, scaledTrafficRate);
                break;
            case Material.CONCRETE:
                color = Color.Lerp(lowTrafficConcrete, highTrafficConcrete, scaledTrafficRate);
                break;
            case Material.GRAVEL:
                color = Color.Lerp(lowTrafficGravel, highTrafficGravel, scaledTrafficRate);
                break;
            default:
                color = Color.Lerp(lowTrafficAsphalt, highTrafficAsphalt, scaledTrafficRate);
                break;
        }
        Colorize(color);
    }

    private void Colorize(Color color)
    {
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    /**
     * Load data from a GISRecord to represent this road. Update visuals
     */
    public void loadFromGIS(Assets.GISRecord record)
    {
        // Get GIS data from record
        Assets.DBNumeric FID = (Assets.DBNumeric)record.DbfRecord.Record[I_FID];
        Assets.DBNumeric vmt = (Assets.DBNumeric)record.DbfRecord.Record[I_VMT];
        Assets.DBNumeric aadt = (Assets.DBNumeric)record.DbfRecord.Record[I_AADT];
        Assets.DBCharacter roadname = (Assets.DBCharacter)record.DbfRecord.Record[I_ROAD_NAME];
        Assets.DBCharacter surface = (Assets.DBCharacter)record.DbfRecord.Record[I_SURFACE];
        Assets.DBNumeric _lanes = (Assets.DBNumeric)record.DbfRecord.Record[I_LANES];
        Assets.DBCharacter _condition = (Assets.DBCharacter)record.DbfRecord.Record[I_CONDITION];
        Assets.DBNumeric shapelen = (Assets.DBNumeric)record.DbfRecord.Record[I_SHAPE_LEN];

        // Update public values
        roadName = roadname.Value.Trim();
        trafficSum = double.Parse(new string(vmt.Value).Trim());
        trafficRate = double.Parse(new string(aadt.Value).Trim());
        length = double.Parse(new string(shapelen.Value).Trim());
        lanes = int.Parse(new string(_lanes.Value));
        ID = int.Parse(new string(FID.Value));
        try
        {
            condition = (Condition)System.Enum.Parse(typeof(Condition), _condition.Value.Trim().ToUpper());
        } catch (ArgumentException) {
            condition = Condition.POOR;
        }
        try
        {
            material = (Material)System.Enum.Parse(typeof(Material), surface.Value.Trim().ToUpper());
        } catch (ArgumentException)
        {
            material = Material.GRAVEL;
        }

        // Get the line from the GIS record. Ignore if it doesn't have at least 2 points and 1 lane
        Assets.PolyLine pline = (Assets.PolyLine)record.ShpRecord.Contents;
        if (pline.Points.Length <= 1 || lanes <= 0)
        {
            isValid = false;
            Destroy(gameObject);
            return;
        }

        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
        if (points == null)
        {
            points = new List<Vector3>();
        }

        // Set linerenderer color
        Colorize();

        // Fill linerenderer with points
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

        potholes = new List<GameObject>();
    }

    public Vector3 RandomPoint()
    {
        int randIdx = new System.Random().Next(points.Count - 1);
        Vector3 p1 = points[randIdx];
        Vector3 p2 = points[randIdx + 1];
        return Vector3.Lerp(p1, p2, UnityEngine.Random.value);
    }

    /**
     * Return a point that is roughly in the middle of the road segment
     */
    public Vector3 MidPoint()
    {
        if (points.Count % 2 == 0)
        {
            Vector3 p1 = points[points.Count / 2];
            Vector3 p2 = points[(points.Count / 2) - 1];

            return Vector3.Lerp(p1, p2, 0.5f);
        } else
        {
            return points[points.Count / 2];
        }
    }

    public static int Sort(Road r1, Road r2)
    {
        return r1.trafficSum.CompareTo(r2.trafficSum);
    }
}
