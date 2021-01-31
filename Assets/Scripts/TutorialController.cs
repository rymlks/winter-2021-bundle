using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.SmartFormat;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour
{

    [TextArea(15,20)]
    public string tutorialText;
    
    [TextArea(15,20)]
    public string tutorialIntroText;
    
    [TextArea(15,20)]
    public string tutorialHowToWinText;
    
    [TextArea(15,20)]
    public string tutorialAngerText;
    
    [TextArea(15,20)]
    public string tutorialBudgetText;
    
    [TextArea(15,20)]
    public string tutorialLaborAndOutroText;

    private string[] _tutorialTextsOrdered;
    protected int currentTextIndex = 0;

    protected bool shouldShowTutorial = true;
    protected GameObject tutorialMenuObject;
    protected GameManager manager;


    // Start is called before the first frame update
    void Start()
    {
        this.tutorialMenuObject = GameObject.Find("TutorialMenu");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OrderTutorialTexts()
    {
        _tutorialTextsOrdered = new string[5];
        _tutorialTextsOrdered[0] = tutorialIntroText;
        _tutorialTextsOrdered[1] = tutorialHowToWinText;
        _tutorialTextsOrdered[2] = tutorialAngerText;
        _tutorialTextsOrdered[3] = tutorialBudgetText;
        _tutorialTextsOrdered[4] = tutorialLaborAndOutroText;
        currentTextIndex = 0;
    }

    public void NotifyGameBeginning(GameManager manager)
    {
        SetManager(manager);
        OrderTutorialTexts();
        if (ShouldShowTutorial())
        {
            tutorialMenuObject.SetActive(true);
            UpdateText();
        }
    }

    private void UpdateText()
    {
        GetMessageText().text =
            Smart.Format(_tutorialTextsOrdered[currentTextIndex], this.manager);
    }

    private void SetManager(GameManager manager)
    {
        this.manager = manager;
    }

    private Text GetMessageText()
    {
        return tutorialMenuObject.transform.Find("BlackBorder/OrangeBorder/Body/Message").gameObject.GetComponent<Text>();
    }

    public void AdvanceTutorial()
    {
        this.currentTextIndex = (this.currentTextIndex + 1) % _tutorialTextsOrdered.Length;
        UpdateText();
        
    }
    
    private bool ShouldShowTutorial()
    {
        return shouldShowTutorial;
    }

    public void NeverShowAgain()
    {
        tutorialMenuObject.SetActive(false);
        this.shouldShowTutorial = false;
    }

    public void CloseTutorial()
    {
        tutorialMenuObject.SetActive(false);
    }
}