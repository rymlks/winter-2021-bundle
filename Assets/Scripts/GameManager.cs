using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public PotholeController potholeController;
    public PlaythroughStatistics playthroughStatistics;
    public BalanceParameters balanceParameters;
    public ContextMenuController contextMenu;
    public Button nextButton;

    public GameObject canvasCover;
    private int fadeInFrames = 30;

    private static string titleScene = "TitleScreen";
    private string losingScene = "LosingScreen";
    private string goodEndScene = "GoodEnd";
    private string badEndScene = "BadEnd";

    private string demoScene = "RickDev";

    private string loadingScene = "Loading";

    [HideInInspector]
    public string nextScene = null;
    [HideInInspector]
    public int currentRound;
    [HideInInspector]
    public int currentYear;

    void Start()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("GameController");
        if (objs.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        currentRound = 0;
        currentYear = 0;

        if (playthroughStatistics != null && balanceParameters != null)
        {
            playthroughStatistics.currentBudget = balanceParameters.budgetPerYear;
            playthroughStatistics.maxAnger = balanceParameters.maxAnger;
            playthroughStatistics.maxBudget = balanceParameters.moneyToWin;
            playthroughStatistics.currentLabor = balanceParameters.maxLabor;
            playthroughStatistics.maxLabor = balanceParameters.maxLabor;
        }

        transform.parent = null;
        DontDestroyOnLoad(gameObject);
    }

    private void Awake()
    {
        StartCoroutine(FadeIn());
    }

    public void NextRound()
    {
        if (contextMenu != null)
            contextMenu.Close();

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

        playthroughStatistics.currentLabor = playthroughStatistics.maxLabor;
        StartCoroutine(TransitionRounds());
    }

    private IEnumerator TransitionRounds()
    {
        canvasCover.GetComponent<Image>().raycastTarget = true;
        nextButton.interactable = false;
        potholeController.AdvanceRound();

        // Wait for all actions advancing the round to finish
        while (potholeController.IsRoundAdvancing())
            yield return null;



        // Adjust anger
        float roundAnger = potholeController.GetTotalAngerFromRound();
        if (roundAnger > balanceParameters.angerIncreaseThreshold)
        {
            playthroughStatistics.currentAnger += roundAnger;
        }
        else
        {
            playthroughStatistics.currentAnger = Mathf.Max(playthroughStatistics.currentAnger - balanceParameters.angerDecayPerRound, 0);
        }

        // Lose if people are too angry
        if (playthroughStatistics.currentAnger > playthroughStatistics.maxAnger)
        {
            Lose();
        } else
        {
            nextButton.interactable = true;
            canvasCover.GetComponent<Image>().raycastTarget = false;
        }
    }
    public void LoadDemoScene()
    {
        LoadingScreenTransition(demoScene);
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
        LoadingScreenTransition(losingScene);
    }

    public void WinGood()
    {
        LoadingScreenTransition(goodEndScene);
    }

    public void WinBad()
    {
        LoadingScreenTransition(badEndScene);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }

    public void LoadTitleScreen()
    {
        LoadingScreenTransition(titleScene);
    }

    public void LoadingScreenTransition(string scene)
    {
        nextScene = scene;
        StartCoroutine(FadeOutAndLoad());
    }

    private IEnumerator FadeOutAndLoad()
    {
        // Fade out
        if (canvasCover != null)
        {
            canvasCover.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            canvasCover.SetActive(true);
            canvasCover.GetComponent<Image>().raycastTarget = true;

            for (int i=0; i<fadeInFrames; i++)
            {
                canvasCover.GetComponent<Image>().color = new Color(0, 0, 0, i / (float)fadeInFrames);
                yield return null;
            }
        }

        // Load
        SceneManager.LoadScene(loadingScene);
        yield return null;
    }

    private IEnumerator FadeIn()
    {
        if (canvasCover != null)
        {
            canvasCover.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            canvasCover.SetActive(true);
            canvasCover.GetComponent<Image>().raycastTarget = false;

            for (int i = fadeInFrames; i >= 0; i--)
            {
                canvasCover.GetComponent<Image>().color = new Color(0, 0, 0, i / (float)fadeInFrames);
                yield return null;
            }
            canvasCover.SetActive(false);
            canvasCover.GetComponent<Image>().raycastTarget = true;
        }
        yield return null;
    }
}
