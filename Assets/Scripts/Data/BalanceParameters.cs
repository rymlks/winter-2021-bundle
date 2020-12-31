using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalanceParameters : MonoBehaviour
{
    public int roundsInAYear;

    public int minimumNewPotholesPerRound;
    public int maximumNewPotholesPerRound;
    public float maximumPotholeAngerPerRound;
    public float patchMoneyCost;
    public int minimumPotholePatchDuration;
    public int maximumPotholePatchDuration;
    public float budgetPerYear;
    public float moneyToWin;
    public float angerIncreaseThreshold;
    public float maxAnger;
    public float angerDecayPerRound;
}
