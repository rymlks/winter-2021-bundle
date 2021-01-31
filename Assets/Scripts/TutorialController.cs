using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.SmartFormat;

public class TutorialController : MonoBehaviour
{

    [TextArea(15,20)]
    public string tutorialText;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void NotifyGameBeginning(GameManager manager)
    {
        if (ShouldShowTutorial())
        {
            manager.contextMenu.Open(Smart.Format(tutorialText, manager), GetRoadmapCenter());
        }
    }

    private static Vector3 GetRoadmapCenter()
    {
        return new Vector3((Road.MaxX + Road.MinX) * 0.5f, (Road.MaxY + Road.MinY) * 0.5f, 0.0f);
    }

    private bool ShouldShowTutorial()
    {
        return true;
    }
}