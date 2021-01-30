using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO; 
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using Assets;
using UnityEngine.UI;

public class ShapefileImport : MonoBehaviour
{
    public string city;

    public float cameraMargin;

    public GameObject cityNameLabel;
    public GameObject roadParent;
    [Range(0.0f, 0.05f)]
    public float roadWidthMultiplier;
    
    public string shxPath;
    public GameObject roadPrefab;
    public PotholeController potholeController;
    public BalanceParameters balanceParameters;
    public ContextMenuController contextMenuController;
    public PlaythroughStatistics playthroughStatistics;
    public GameManager gameManager;

    public float roadScale;

    private int linesPerFrame = 100;
    private Assets.ShxFile shapeFile;
    private List<int> _roadIdWhiteList;
    
    void Start()
    {
        Debug.Log("Reading " + shxPath);
        shapeFile = new Assets.ShxFile(Application.dataPath + "/" + shxPath);

        cityNameLabel.GetComponent<Text>().text = city;

        CreateNonDuplicateRoadWhitelist();
        // -83.2f, 42.6f, 0.0f
        //StartCoroutine(ReadGIS());

        Road.positionScale = roadScale;
        ReadGIS();
    }

    private void CreateNonDuplicateRoadWhitelist()
    {
        try
        {
            string dedupeJson = System.IO.File.ReadAllText("Assets/Dedupes/" + city + ".dedupe.json");
            _roadIdWhiteList = JsonHelper.FromJson<RoadDedupingPotholeController.RoadDeduplicationWrittenData>(dedupeJson)
                .Where(data => data.isBeingKept).Select(data => data.roadId).ToList();
        }
        catch (Exception e)
        {
            Debug.Log("Dedupe file not found for level! More info: "+ e.Message);
            _roadIdWhiteList = new List<int>();
        }
    }

    void ReadGIS()
    {
        playthroughStatistics.cityName = city;

        Road.MinX = float.MaxValue;
        Road.MaxX = float.MinValue;

        Road.MinY = float.MaxValue;
        Road.MaxY = float.MinValue;

        // Load all the GIS data from file
        shapeFile.Load();
        //yield return null;

        
        Assets.GISRecord record1 = shapeFile.GetData(0);
        for (int i=0; i< record1.DbfRecord.Record.Count; i++)
        {
            Debug.LogFormat("{0}: {1}, {2}", i, record1.DbfRecord.Record[i].discriptor.FieldName, record1.DbfRecord.Record[i].discriptor.FieldType);
        }

        // Iterate through the shapefile, instantiating roads that are within this city.
        int count = 0;

        for (int i=0; i<shapeFile.RecordSet.Count; i++)
        {
            Assets.GISRecord record = shapeFile.GetData(i);
            Assets.DBCharacter mcdname = (Assets.DBCharacter)record.DbfRecord.Record[17];
            if (mcdname.Value.ToLower().Contains(city.ToLower()) || city == "*")
            {
                // Some records aren't polyline and I don't know what they're for
                if (record.ShpRecord.Header.Type == Assets.ShapeType.PolyLine)
                {
                    createRoad(record, roadParent.transform);
                }
                if (count > linesPerFrame)
                {
                    count = 0;
                    //yield return null;
                }
                count++;
            }
        }

        configureMainCamera();
        potholeController.SortAndSumRoads();
        Debug.Log("Done.");

        Debug.Log("Number of roads: " + potholeController.roads.Count);

        gameManager.ShowTutorialText();
    }

    private void configureMainCamera()
    {
        Camera.main.gameObject.GetComponent<CameraController>()
            .JumpTo(new Vector3((Road.MaxX + Road.MinX) * 0.5f, (Road.MaxY + Road.MinY) * 0.5f, -10f));
        float maxExtent = Mathf.Max(Road.MaxX - Road.MinX, Road.MaxY - Road.MinY);
        Camera.main.orthographicSize = maxExtent * 0.5f + cameraMargin;
    }

    private void createRoad(GISRecord record, Transform parent)
    {
        GameObject roadObj = Instantiate(roadPrefab, new Vector3(0, 0, 0), Quaternion.identity, parent);
        Road road = roadObj.GetComponent<Road>();
        road.roadWidthMultiplier = roadWidthMultiplier;
        road.loadFromGIS(record);
        if (road.isValid && !IsADuplicate(road.ID))
        {
            potholeController.roads.Add(road);
            road.balanceParameters = balanceParameters;
            road.contextMenuController = contextMenuController;
            road.playthroughStatistics = playthroughStatistics;
            road.potholeController = potholeController;
        }
        else
        {
            Destroy(roadObj);
        }
    }

    private bool IsADuplicate(int roadId)
    {
        if (_roadIdWhiteList.Count < 1)
        {
            return false;
        }
        else if(_roadIdWhiteList.Contains(roadId)){
            return false;
        }
        return true;
    }
}