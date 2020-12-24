using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO; 
using UnityEngine;
using UnityEditor;


public class ShapefileImport : MonoBehaviour
{
    public bool debugNames;
    [Range(1, 100)]
    public int debugNameDensity;

    public string city;

    public string shxPath;

    private int linesPerFrame = 100;
    private Assets.ShxFile shapeFile;

    void Start()
    {
        Debug.Log("Reading " + shxPath);

        shapeFile = new Assets.ShxFile(shxPath);

        StartCoroutine(ReadGIS());
        // -83.2f, 42.6f, 0.0f
    }

    void OnDrawGizmos()
    {
        if (debugNames && shapeFile != null)
        {
            for (int i = 0; i < shapeFile.RecordSet.Count; i+= 1000/debugNameDensity)
            {
                Assets.Record record = shapeFile.GetData(i);
                Assets.DBCharacter community = (Assets.DBCharacter)record.DbfRecord.Record[8];
                if (record.ShpRecord.Header.Type == Assets.ShapeType.PolyLine)
                {
                    Assets.PolyLine pline = (Assets.PolyLine)record.ShpRecord.Contents;
                    Assets.Point p1 = pline.Points[0];
                    Handles.Label(new Vector3((float)p1.X, (float)p1.Y, 0), community.Value);
                }
            }
        }
    }

    IEnumerator ReadGIS()
    {
        shapeFile.Load();
        yield return null;

        int count = 0;
        /*
        Assets.Record record1 = shapeFile.GetData(0);
        for (int i=0; i< record1.DbfRecord.Record.Count; i++)
        {
            Debug.Log("" + i + ": " + record1.DbfRecord.Record[i].discriptor.FieldName);
        }
        */

        for (int i=0; i<shapeFile.RecordSet.Count; i++)
        {
            Assets.Record record = shapeFile.GetData(i);
            Assets.DBCharacter community = (Assets.DBCharacter)record.DbfRecord.Record[8];
            if (community.Value.ToLower().Contains(city.ToLower()))
            {
                if (record.ShpRecord.Header.Type == Assets.ShapeType.PolyLine)
                {
                    Assets.PolyLine pline = (Assets.PolyLine)record.ShpRecord.Contents;
                    for (int j = 0; j < pline.Points.Length - 1; j++)
                    {
                        Assets.Point p1 = pline.Points[j];
                        Assets.Point p2 = pline.Points[j + 1];
                        Vector3 v1 = new Vector3((float)p1.X, (float)p1.Y, 0);
                        Vector3 v2 = new Vector3((float)p2.X, (float)p2.Y, 0);
                        Debug.DrawLine(v1, v2, Color.red, 3600.0f, false);
                    }
                }
            }
            if (count > linesPerFrame)
            {
                count = 0;
                //Debug.Log("drew " + i + " lines.");
                yield return null;
            }
            count++;
        }
        Debug.Log("Done.");
    }
}