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
        public int time;

        public List<Road.Material> compatibleRoadMaterials;
    }
    [System.Serializable]
    public class RoadOption
    {
        public string description;
        public float cost;
        public float labor;
        public float time;

        public Road.Material material;
        public Road.Condition condition;
    }

    public int roundsInAYear;
    public int maxYears;

    public int minimumNewPotholesPerRound;
    public int maximumNewPotholesPerRound;
    public float potholeGenerationPerTrafficWeight;
    public float potholeGenerationConditionWeight;
    public float potholeGenerationFactor;
    public float maximumPotholeAngerPerRound;

    public int minimumPotholePatchDuration;
    public int maximumPotholePatchDuration;

    public float budgetPerYear;
    public float moneyToWin;

    public float angerIncreaseThreshold;
    public float maxAnger;
    public float angerDecayPerRound;

    public float potholeAngerPerCar;
    public float potholeConstructionAngerPerCar;
    public float roadConstructionAngerPerCar;

    public float maxLabor;

    public List<RepairOption> potholeRepairs;
    public List<RoadOption> roadRepairs;
}
