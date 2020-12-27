using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;



public class StatisticsUIController : MonoBehaviour
{
    
    private Text budgetText;
    private Text angerText;
    private PlaythroughStatistics statsModel;
    
    
    // Start is called before the first frame update
    void Start()
    {
        budgetText = GameObject.Find("BudgetText").GetComponent<Text>();
        angerText = GameObject.Find("AngerText").GetComponent<Text>();
        statsModel = GameObject.FindObjectOfType<PlaythroughStatistics>();
    }

    // Update is called once per frame
    void Update()
    {
        
        budgetText.text = statsModel.currentBudget.ToString("F0");
        angerText.text = statsModel.currentAnger.ToString("F0");
    }
}
