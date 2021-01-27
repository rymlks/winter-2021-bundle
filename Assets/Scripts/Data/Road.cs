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

    public UnityEngine.Material normalMaterial;
    public UnityEngine.Material constructionMaterial;
    public UnityEngine.Material constructionMaterialSelected;

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
    public bool underConstruction = false;
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

    protected Bounds bounds;
    
    private LineRenderer lineRenderer;
    private ParticleSystem _particleSystem;
    private bool _selected = false;
    private int _constructionTime = -1;
    private float _currentLaborCost = -1;
    private float _angerPerRound = -1;

    public static float MaxX = float.MinValue;
    public static float MaxY = float.MinValue;
    public static float MinX = float.MaxValue;
    public static float MinY = float.MaxValue;
    public static float positionScale = 50;

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
    public void NotifyRoundEnded()
    {
        if (underConstruction)
        {
            _constructionTime--;
            if (_constructionTime <= 0)
            {
                underConstruction = false;
                Colorize();
            } else
            {
                playthroughStatistics.currentLabor -= _currentLaborCost;
                _particleSystem.Play();
            }
        }
    }
    public float getAngerCausedPerRound()
    {
        return this.underConstruction ? _angerPerRound : 0;
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

        if (underConstruction)
        {
            lineRenderer.material = constructionMaterialSelected;
        }

        foreach (GameObject pothole in potholes)
        {
            pothole.GetComponent<Pothole>().FauxSelect();
        }
    }

    public void Deselect()
    {
        _selected = false;
        GetComponent<MeshCollider>().enabled = true;
        Colorize();

        if (underConstruction)
        {
            lineRenderer.material = constructionMaterial;
        }

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
        message += "\nMaterial: " + material.ToString();
        //message += "\nCondition: " + condition.ToString();
        message += "\nLanes: " + lanes;
        if (underConstruction)
        {
            message += "\n\n<b>Under construction.</b>";
            message += string.Format("\nLabor cost per month: {0:#,0} hrs.", _currentLaborCost);
            message += string.Format("\n<color=#ff2222><b>Annoyance: {0:#,0}</b></color>", _angerPerRound);
            message += string.Format("\nConstruction time remaining: {0:#,0} months.", _constructionTime);
        } else
        {
            message += "\nDaily Drivers: " + trafficRate;
            message += "\nPotholes: " + potholes.Count;
        }
        return message;
    }

    public void TryRePave(float cost, float labor, int time, Material material, Condition condition)
    {

        if (CanAffordRePave(cost, labor))
        {
            DeductCost(cost, labor);
            RePave(material, condition, time);
        }
        else
        {
            Debug.Log("Cannot patch pothole: insufficient funds.  Have " + this.playthroughStatistics.currentBudget + ", need " + cost);
        }
    }

    private void RePave(Material material, Condition condition, int time)
    {
        this.condition = condition;
        this.material = material;

        if (time > 0)
        {
            underConstruction = true;
            _constructionTime = time;
            _angerPerRound = balanceParameters.roadConstructionAngerPerCar;// * (float)trafficRate;
        }

        foreach (GameObject pothole in potholes)
        {
            Destroy(pothole);
        }

        potholes = new List<GameObject>();

        Colorize();
    }

    private void DeductCost(float cost, float labor)
    {
        this.playthroughStatistics.currentBudget -= cost;
        this.playthroughStatistics.currentLabor -= labor;
        _currentLaborCost = labor;
    }

    public bool CanAffordRePave(float cost, float labor)
    {
        return this.playthroughStatistics.currentBudget >= cost && playthroughStatistics.currentLabor >= labor;
    }

    public float GetRePaveCost(float baseCost)
    {
        return baseCost * (float)length * lanes;
    }

    public float GetRePaveLabor(float baseLabor)
    {
        return Mathf.Min(baseLabor * (float)length * lanes, balanceParameters.maxLabor);
    }

    public int GetRePaveTime(float baseTime)
    {
        return (int)(baseTime * length * lanes);
    }

    public List<ContextMenuController.ContextMenuOption> GetRepairOptions()
    {
        List<ContextMenuController.ContextMenuOption> options = new List<ContextMenuController.ContextMenuOption>();
        foreach (BalanceParameters.RoadOption option in balanceParameters.roadRepairs)
        {
            options.Add(CreateOption(option));
        }

        return options;
    }

    private ContextMenuController.ContextMenuOption CreateOption(BalanceParameters.RoadOption option)
    {
        float cost = GetRePaveCost(option.cost);
        float labor = GetRePaveLabor(option.labor);
        int time = GetRePaveTime(option.time);
        return new ContextMenuController.ContextMenuOption(option.description, cost, labor, delegate { TryRePave(cost, labor, time, option.material, option.condition); });
    }

    private void Colorize()
    {
        if (underConstruction)
        {
            lineRenderer.material = constructionMaterial;
            //GetComponent<ParticleSystem>().Play();
            //lineRenderer.material.mainTextureScale =  new Vector2((float)length, 1);
        } else
        {
            //GetComponent<ParticleSystem>().Stop();
            lineRenderer.material = normalMaterial;
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
        bounds = new Bounds();
        for (int j = 0; j < pline.Points.Length; j++)
        {
            Assets.Point p = pline.Points[j];
            Vector3 v = new Vector3((float)(p.X * positionScale), (float)(p.Y * positionScale), 0);
            points.Add(v);

            MaxX = Mathf.Max(MaxX, v.x);
            MaxY = Mathf.Max(MaxY, v.y);
            MinX = Mathf.Min(MinX, v.x);
            MinY = Mathf.Min(MinY, v.y);

            this.updateBounds(v);
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
        _particleSystem = GetComponent<ParticleSystem>();
        ParticleSystem.ShapeModule _editableShape = _particleSystem.shape;
        //_editableShape.position = MidPoint();
        _editableShape.mesh = mesh;
    }

    private void updateBounds(Vector3 newPoint)
    {
        if (bounds.center.Equals(Vector3.zero) && bounds.extents.Equals(Vector3.zero))
        {
            bounds = new Bounds(newPoint, Vector3.zero);
        }
        else
        {
            bounds.Encapsulate(newPoint);
        }
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

    public Bounds GetBounds()
    {
        return this.bounds;
    }

    public void Render(bool render)
    {
        this.lineRenderer.enabled = render;
    }
}
