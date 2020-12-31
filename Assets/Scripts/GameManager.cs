﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public PotholeController potholeController;
    public PlaythroughStatistics playthroughStatistics;
    public BalanceParameters balanceParameters;

    private int currentRound;
    private int currentYear;

    private string titleScene = "TitleScreen";

    void Start()
    {
        currentRound = 0;
        currentYear = 0;
        playthroughStatistics.currentBudget = balanceParameters.budgetPerYear;
        playthroughStatistics.maxAnger = balanceParameters.maxAnger;
    }

    public void NextRound()
    {
        // Advance the counters
        currentRound++;
        if (currentRound >= balanceParameters.roundsInAYear)
        {
            currentRound = 0;
            currentYear++;
            // Get budget each year
            playthroughStatistics.currentBudget += balanceParameters.budgetPerYear;
        }

        // Adjust anger
        float roundAnger = potholeController.getTotalAngerFromExistingPotholes();
        if (roundAnger > balanceParameters.angerIncreaseThreshold)
        {
            playthroughStatistics.currentAnger += roundAnger;
        } else
        {
            playthroughStatistics.currentAnger = Mathf.Max(playthroughStatistics.currentAnger - balanceParameters.angerDecayPerRound, 0);
        }

        // Age the potholes
        potholeController.AgeExistingPotholes();

        // Spawn potholes
        potholeController.createNewPotholes();
    }

    public static void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }
    public void ExitGameUI()
    {
        GameManager.ExitGame();
    }

    public void LoadTitleScreen()
    {
        SceneManager.LoadSceneAsync(titleScene);
    }
}