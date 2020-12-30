using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PotholeController : MonoBehaviour
{
    public GameObject potholePrefab;
    public List<Road> roads;

    private GameObject _potholeParent;
    private BalanceParameters _parameters;

    private double _sumTraffic = 0;
    
    
    void Start()
    {
        roads = new List<Road>();
        if (potholePrefab == null)
        {
            potholePrefab = Resources.Load<GameObject>("Prefabs/Pothole");
        }

        if (_potholeParent == null)
        {
            _potholeParent = new GameObject("Potholes");
        }

        if (_parameters == null)
        {
            _parameters = FindObjectOfType<BalanceParameters>();
        }

    }

    void Update()
    {
        if (roundHasEndedThisFrame())
        {
            createAngerDueToExistingPotholes();
            ageExistingPotholes();
            createNewPotholes();
        }
    }

    private void ageExistingPotholes()
    {
        foreach (Pothole hole in _potholeParent.GetComponentsInChildren<Pothole>())
        {
            hole.NotifyRoundEnded();
        }
    }

    public void SortAndSumRoads()
    {
        roads.Sort(Road.Sort);
        foreach (Road road in roads )
        {
            _sumTraffic += road.trafficSum;
        }
    }

    public float getTotalAngerFromExistingPotholes()
    {
        return _potholeParent.GetComponentsInChildren<Pothole>().Sum(hole => hole.getAngerCausedPerRound());
    }

    public void createNewPotholes()
    {
        int newPotholesThisRound = new System.Random().Next(_parameters.minimumNewPotholesPerRound, _parameters.maximumNewPotholesPerRound + 1);    //Sysrand is exclusive of the maximum
        for (int i = 0; i < newPotholesThisRound; i++)
        {
            GameObject.Instantiate(potholePrefab, generatePotholeLocation(), Quaternion.identity, _potholeParent.transform);
        }
    }

    private Vector2 generatePotholeLocation()
    {
        int trafficThreshold = new System.Random().Next(0, (int)_sumTraffic);
        double sum = 0;
        Vector3 point = roads.Last().RandomPoint();
        foreach (Road road in roads)
        {
            sum += road.trafficSum;
            if (sum > trafficThreshold)
            {
                point = road.RandomPoint();
                break;
            }
        }
        return new Vector2(point.x, point.y);
    }

    private bool roundHasEndedThisFrame()
    {
        //integrate the real round system when we have one
        //for now, rounds can be faked as ending by the player pressing 'R'
        return Input.GetKeyUp(KeyCode.R);
    }
}
