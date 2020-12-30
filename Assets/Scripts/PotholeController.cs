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
    private float _sqrMinDistanceApart;
    
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

        Vector2 size = potholePrefab.GetComponent<BoxCollider2D>().size;
        _sqrMinDistanceApart = Mathf.Max(size.x * size.x, size.y * size.y);
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

    private void createAngerDueToExistingPotholes()
    {
        FindObjectOfType<PlaythroughStatistics>().currentAnger += getTotalAngerFromExistingPotholes();
    }

    private float getTotalAngerFromExistingPotholes()
    {
        return _potholeParent.GetComponentsInChildren<Pothole>().Sum(hole => hole.getAngerCausedPerRound());
    }

    private void createNewPotholes()
    {
        int newPotholesThisRound = new System.Random().Next(_parameters.minimumNewPotholesPerRound, _parameters.maximumNewPotholesPerRound + 1);    //Sysrand is exclusive of the maximum
        for (int i = 0; i < newPotholesThisRound; i++)
        {
            GameObject.Instantiate(potholePrefab, generatePotholeLocation(), Quaternion.identity, _potholeParent.transform);
        }
    }

    private Vector2 generatePotholeLocation()
    {
        Vector2 candidate = generateCandidateLocation();
        if (isTooCloseToAnExistingPothole(candidate))
        {
            Debug.Log("placing overlapping pothole; handling not in place yet");
        }
        return candidate;
    }
    
    private bool isTooCloseToAnExistingPothole(Vector2 possibleLocation)
    {
        return this._potholeParent.GetComponentsInChildren<Pothole>().Any(hole => Vector2.SqrMagnitude(possibleLocation - new Vector2(hole.transform.position.x, hole.transform.position.y)) < _sqrMinDistanceApart);
    }

    private Vector3 generateCandidateLocation()
    {
        int trafficThreshold = new System.Random().Next(0, (int) _sumTraffic);
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

        return point;
    }

    private bool roundHasEndedThisFrame()
    {
        //integrate the real round system when we have one
        //for now, rounds can be faked as ending by the player pressing 'R'
        return Input.GetKeyUp(KeyCode.R);
    }
}
