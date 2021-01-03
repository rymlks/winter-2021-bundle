using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class PotholeController : MonoBehaviour
{
    public GameObject potholePrefab;
    public List<Road> roads;
    public ContextMenuController contextMenuControler;

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

        if (contextMenuControler != null)
        {
            contextMenuControler.gameObject.SetActive(false);
        }
    }

    public void AgeExistingPotholes()
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
        Random random = new System.Random();
        int newPotholesThisRound = random.Next(_parameters.minimumNewPotholesPerRound, _parameters.maximumNewPotholesPerRound + 1);    //Sysrand is exclusive of the maximum
        for (int i = 0; i < newPotholesThisRound; i++)
        {
            Road randomRoad = GetRandomRoad(random);
            Vector2 candidateLocation = generateCandidateLocation(randomRoad);
            Pothole intersect = GetIntersectingPothole(candidateLocation);
            if (intersect == null)
            {
                GameObject pothole = GameObject.Instantiate(potholePrefab, candidateLocation, Quaternion.identity, _potholeParent.transform);
                pothole.GetComponent<Pothole>().contextMenuControler = contextMenuControler;
                pothole.GetComponent<Pothole>().roadSegment = randomRoad;
                pothole.GetComponent<Pothole>().balanceParameters = _parameters;
                randomRoad.potholes.Add(pothole);
            } else
            {
                intersect.Degrade();
            }
        }
    }

    /**
     * Get a random road, weighted by trafficSum
     */
    private Road GetRandomRoad(Random random)
    {
        int trafficThreshold = random.Next(0, (int)_sumTraffic);
        double sum = 0;
        foreach (Road road in roads)
        {
            sum += road.trafficSum;
            if (sum > trafficThreshold)
            {
                return road;
            }
        }
        return roads.Last();
    }

    private Pothole GetIntersectingPothole(Vector2 possibleLocation)
    {
        Bounds candidateBounds = new Bounds(possibleLocation, potholePrefab.GetComponent<SpriteRenderer>().bounds.size);
        return System.Array.Find(_potholeParent.GetComponentsInChildren<Pothole>(), hole => hole.GetComponent<SpriteRenderer>().bounds.Intersects(candidateBounds));
    }

    private Vector3 generateCandidateLocation(Road road)
    {
        return road.RandomPoint();
    }
}
