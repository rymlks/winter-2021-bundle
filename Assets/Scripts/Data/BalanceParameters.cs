using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalanceParameters : MonoBehaviour
{

    [System.Serializable]
    public class RepairOption
    {
        public string description;
        public float cost;
        public float labor;
        public int durability;

        public List<Road.Material> compatibleRoadMaterials;
    }
    [System.Serializable]
    public class RoadOption
    {
        public string description;
        public float cost;
        public float labor;
        public Road.Material material;
        public Road.Condition condition;
    }

    public int roundsInAYear;
    public int maxYears;

    public int minimumNewPotholesPerRound;
    public int maximumNewPotholesPerRound;
    public float maximumPotholeAngerPerRound;

    public int minimumPotholePatchDuration;
    public int maximumPotholePatchDuration;
    public float budgetPerYear;
    public float moneyToWin;
    public float angerIncreaseThreshold;
    public float maxAnger;
    public float angerDecayPerRound;

    public float maxLabor;

    public List<RepairOption> potholeRepairs;
    public List<RoadOption> roadRepairs;
}
