using System.Collections;
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

    private static string titleScene = "TitleScreen";
    private string losingScene = "LosingScreen";
    private string goodEndScene = "GoodEnd";
    private string badEndScene = "BadEnd";

    void Start()
    {
        currentRound = 0;
        currentYear = 0;
        playthroughStatistics.currentBudget = balanceParameters.budgetPerYear;
        playthroughStatistics.maxAnger = balanceParameters.maxAnger;
        playthroughStatistics.maxBudget = balanceParameters.moneyToWin;
    }

    public void NextRound()
    {
        // Advance the counters
        currentRound++;
        if (currentRound >= balanceParameters.roundsInAYear)
        {
            currentRound = 0;
            currentYear++;

            // Win if you've managed to survive long enough
            if (currentYear >= balanceParameters.maxYears)
            {
                Win();
                return;
            }

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

        // Lose if people are too angry
        if (playthroughStatistics.currentAnger > playthroughStatistics.maxAnger)
        {
            Lose();
            return;
        }

        // Age the potholes
        potholeController.AgeExistingPotholes();

        // Spawn potholes
        potholeController.createNewPotholes();
    }

    public void Retire()
    {
        WinBad();
    }

    public void Win()
    {
        if (playthroughStatistics.currentBudget < playthroughStatistics.maxBudget) WinGood(); 
        else WinBad();
    }

    public void Lose()
    {
        SceneManager.LoadSceneAsync(losingScene);
    }

    public void WinGood()
    {
        SceneManager.LoadSceneAsync(goodEndScene);
    }

    public void WinBad()
    {
        SceneManager.LoadSceneAsync(badEndScene);
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

    public static void LoadTitleScreen()
    {
        SceneManager.LoadSceneAsync(titleScene);
    }

    public void LoadTitleScreenUI()
    {
        GameManager.LoadTitleScreen();
    }
}
