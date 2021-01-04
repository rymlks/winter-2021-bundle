using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalanceParameters : MonoBehaviour
{
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

    [Header("Pothole Durabilities")]
    public int throwAndGoDurability;
    public int throwAndRollDurability;
    public int asphaltPatchDurability;
    public int concretePatchDurability;
    public int gravelFillDurability;

    [Header("Pothole Costs")]
    public float throwAndGoCost;
    public float throwAndRollCost;
    public float asphaltPatchCost;
    public float concretePatchCost;
    public float gravelFillCost;

    [Header("Road Costs")]
    public float lowGradeConcreteCostPerFoot;
    public float highGradeConcreteCostPerFoot;
    public float lowGradeAsphaltCostPerFoot;
    public float highGradeAsphaltCostPerFoot;
    public float gravelCostPerFoot;
}
