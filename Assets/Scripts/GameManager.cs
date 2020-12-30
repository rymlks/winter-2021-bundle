using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float budgetPerYear;
    public float angerIncreaseThreshold;
    public float maxAnger;
    public float angerDecayPerRound;

    public PotholeController potholeController;
    public PlaythroughStatistics playthroughStatistics;

    public int roundsInAYear;

    private int currentRound;
    private int currentYear;


    void Start()
    {
        currentRound = 0;
        currentYear = 0;
        playthroughStatistics.currentBudget = budgetPerYear;
        playthroughStatistics.maxAnger = maxAnger;
    }

    public void NextRound()
    {
        // Advance the counters
        currentRound++;
        if (currentRound >= roundsInAYear)
        {
            currentRound = 0;
            currentYear++;
            // Get budget each year
            playthroughStatistics.currentBudget += budgetPerYear;
        }

        // Adjust anger
        float roundAnger = potholeController.getTotalAngerFromExistingPotholes();
        if (roundAnger > angerIncreaseThreshold)
        {
            playthroughStatistics.currentAnger += roundAnger;
        } else
        {
            playthroughStatistics.currentAnger = Mathf.Max(playthroughStatistics.currentAnger - angerDecayPerRound, 0);
        }

        // Spawn potholes
        potholeController.createNewPotholes();
    }
}
