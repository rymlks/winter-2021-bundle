using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO; 
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UI;

public class ShapefileImport : MonoBehaviour
{
    public string city;

    public float cameraMargin;

    public GameObject cityNameLabel;
    [Range(0.0f, 0.05f)]
    public float roadWidthMultiplier;

    public string shxPath;
    public GameObject roadPrefab;

    private int linesPerFrame = 100;
    private Assets.ShxFile shapeFile;

    void Start()
    {
        Debug.Log("Reading " + shxPath);
        shapeFile = new Assets.ShxFile(shxPath);

        cityNameLabel.GetComponent<Text>().text = city;

        // -83.2f, 42.6f, 0.0f
        StartCoroutine(ReadGIS());
    }

    IEnumerator ReadGIS()
    {
        // Load all the GIS data from file
        shapeFile.Load();
        yield return null;

        
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
            Assets.DBCharacter community = (Assets.DBCharacter)record.DbfRecord.Record[8];
            if (community.Value.ToLower().Contains(city.ToLower()))
            {
                // Some records aren't polyline and I don't know what they're for
                if (record.ShpRecord.Header.Type == Assets.ShapeType.PolyLine)
                {
                    GameObject road = Instantiate(roadPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                    road.GetComponent<Road>().roadWidthMultiplier = roadWidthMultiplier;
                    road.GetComponent<Road>().loadFromGIS(record);
                }
                if (count > linesPerFrame)
                {
                    count = 0;
                    yield return null;
                }
                count++;
            }
        }

        Camera.main.gameObject.GetComponent<CameraController>().JumpTo(new Vector3((Road.MaxX + Road.MinX) * 0.5f, (Road.MaxY + Road.MinY) * 0.5f, -10f));
        float maxExtent = Mathf.Max(Road.MaxX - Road.MinX, Road.MaxY - Road.MinY);
        Camera.main.orthographicSize = maxExtent * 0.5f + cameraMargin;
        Debug.Log("Done.");
    }
}