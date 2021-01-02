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

    void Start()
    {
        GameObject[] found = GameObject.FindGameObjectsWithTag("GameController");
        if (found.Length > 0)
        {
            abortButton.SetActive(false);
            GameManager gameManager = found[0].GetComponent<GameManager>();
            string scene = gameManager.nextScene;
            Destroy(found[0]);

            StartCoroutine(BeginLoading(scene));
        } else
        {
            ShowError();
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
        sceneLoading = SceneManager.LoadSceneAsync(scene);
        yield return new WaitForSeconds(0.1f);
        while (true)
        {
            string message = "Loading";
            for (int i = 1; i <= 3; i++)
            {
                message += '.';
                if (!sceneLoading.isDone)
                {
                    loadingText.text = message;
                    yield return new WaitForSeconds(0.1f);
                }
                else
                {
                    yield return new WaitForSeconds(2);
                    ShowError();
                }
            }
        }
    }
}
