using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour
{
    public string demoScene;

    public void ExitGame()
    {
        GameManager.ExitGame();
    }

    public void LoadDemoScene()
    {
        SceneManager.LoadSceneAsync(demoScene);
    }
}
