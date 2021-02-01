using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingController : MonoBehaviour
{
    public Text loadingText;
    public GameObject abortButton;

    private AsyncOperation sceneLoading;
    private bool oneOff = true;

    void Start()
    {
        if (oneOff)
        {
            oneOff = false;
            GameObject[] found = GameObject.FindGameObjectsWithTag("GameController");
            if (found.Length > 0)
            {
                abortButton.SetActive(false);
                GameManager gameManager = found[0].GetComponent<GameManager>();
                string scene = gameManager.nextScene;
                Destroy(found[0]);

                StartCoroutine(BeginLoading(scene));
            }
            else
            {
                ShowError();
            }
        }
    }

    // Re-define this just in case something goes horribly wrong
    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }

    private void ShowError()
    {
        loadingText.text = "Something went wrong...";
        abortButton.SetActive(true);
    }

    private IEnumerator BeginLoading(string scene)
    {
        Scene thisScene = SceneManager.GetActiveScene();
        yield return new WaitForSeconds(0.1f);
        sceneLoading = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        //sceneLoading.allowSceneActivation = !GameManager.delayLoading;
        yield return null;
        while (true)
        {
            string dots = ".";
            for (int i = 1; i <= 3; i++)
            {
                string message = "Loading";
                if (GameManager.loadingRoadName != "")
                {
                    message += " " + GameManager.loadingRoadName;
                }
                dots += '.';
                
                if (!sceneLoading.isDone || GameManager.delayLoading)
                {
                    loadingText.text = message + dots;
                    yield return new WaitForSeconds(0.2f);
                }
                else
                {
                    yield return SceneManager.UnloadSceneAsync(thisScene);
                }
            }
        }
    }
}
