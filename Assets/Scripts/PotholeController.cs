﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class PotholeController : MonoBehaviour
{
    public GameObject potholePrefab;
    public List<Road> roads;
    public ContextMenuController contextMenuControler;

    public Random RNG;

    private IEnumerator ageEnumerator;
    private IEnumerator createEnumerator;
    private IEnumerator roadEnumerator;

    private GameObject _potholeParent;
    private BalanceParameters _parameters;

    protected double _sumTraffic = 0;
    
    void Start()
    {
        roads = new List<Road>();
        RNG = new Random();
        if (potholePrefab == null)
        {
            potholePrefab = Resources.Load<GameObject>("Prefabs/Pothole");
        }

        if (_potholeParent == null)
        {
            _potholeParent = new GameObject("Potholes");
            _potholeParent.transform.parent = transform;
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

    public void AdvanceRound()
    {
        ageEnumerator = AgeExistingPotholes();
        createEnumerator = CreateNewPotholes();
        roadEnumerator = AgeRoads();

        StartCoroutine(ageEnumerator);
        StartCoroutine(createEnumerator);
        StartCoroutine(roadEnumerator);
    }

    public bool IsRoundAdvancing()
    {
        return ageEnumerator != null || createEnumerator != null || roadEnumerator != null;
    }

    private IEnumerator AgeRoads()
    {
        int i = 0;
        foreach (Road road in roads)
        {
            road.NotifyRoundEnded();
            if (++i % (roads.Count / 10) == 0) yield return null;
            //yield return null;
        }
        roadEnumerator = null;
        yield return null;

        SortAndSumRoads();
    }

    private IEnumerator AgeExistingPotholes()
    {
        //int i = 0;
        foreach (Pothole hole in _potholeParent.GetComponentsInChildren<Pothole>())
        {
            hole.NotifyRoundEnded();
            //if (i%10 == 0) yield return null;
            yield return null;
        }
        ageEnumerator = null;
    }

    public virtual void SortAndSumRoads()
    {
        roads.Sort(Road.Sort);
        foreach (Road road in roads )
        {
            _sumTraffic += road.trafficSum;
        }
    }

    public float GetTotalAngerFromExistingPotholes()
    {
        return _potholeParent.GetComponentsInChildren<Pothole>().Sum(hole => hole.getAngerCausedPerRound());
    }

    public float GetTotalAngerFromRoads()
    {
        return roads.Sum(road => road.getAngerCausedPerRound());
    }

    public float GetTotalAngerFromRound()
    {
        return GetTotalAngerFromExistingPotholes() + GetTotalAngerFromRoads();
    }

    private IEnumerator CreateNewPotholes()
    {
        Random random = new System.Random();
        int newPotholesThisRound = random.Next(_parameters.minimumNewPotholesPerRound, _parameters.maximumNewPotholesPerRound + 1);    //Sysrand is exclusive of the maximum
        for (int i = 0; i < newPotholesThisRound; i++)
        {
            Road randomRoad = GetRandomRoad(random);
            GeneratePothole(randomRoad);

            //if (i % 10 == 0) yield return null;
            yield return null;
        }
        createEnumerator = null;
    }

    public void GeneratePothole(Road road)
    {
        Vector2 candidateLocation = generateCandidateLocation(road);
        Pothole intersect = GetIntersectingPothole(candidateLocation);
        if (intersect == null)
        {
            GameObject pothole = GameObject.Instantiate(potholePrefab, candidateLocation, Quaternion.identity, _potholeParent.transform);
            pothole.GetComponent<Pothole>().contextMenuControler = contextMenuControler;
            pothole.GetComponent<Pothole>().roadSegment = road;
            pothole.GetComponent<Pothole>().balanceParameters = _parameters;
            road.potholes.Add(pothole);
        }
        else
        {
            intersect.Degrade();
        }
    }

    public List<Pothole> GetOpenPotholes()
    {
        return _potholeParent.GetComponentsInChildren<Pothole>().Where(ph => ph.isPatched == false).ToList();
    }

    /**
     * Get a random road, weighted by trafficSum
     */
    private Road GetRandomRoad(Random random)
    {
        // Return a random road weighted by daily drivers*length
        int trafficThreshold = random.Next(0, (int)_sumTraffic);
        double sum = 0;
        foreach (Road road in roads)
        {
            sum += road.trafficSum;
            if (sum > trafficThreshold && !road.underConstruction)
            {
                return road;
            }
        }
        // Finding none, find any road that isn't under construction, starting with the busiest roads
        for (int i=roads.Count-1; i>=0; i--)
        {
            Road road = roads[i];
            if (!road.underConstruction)
            {
                return road;
            }
        }
        // Finding none, return the busiest road
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
