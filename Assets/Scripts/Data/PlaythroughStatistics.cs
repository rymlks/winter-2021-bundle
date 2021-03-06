﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaythroughStatistics : MonoBehaviour
{
    public string cityName;

    public float currentAnger;
    public float currentBudget;
    public float currentLabor;
    public float maxAnger;
    public float maxBudget;
    public float maxLabor;

    public void Awake()
    {
        /*
         * There can be only one
         *        /| ________________
         *  O|===|* >________________>
         *        \|
         */
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Stats");
        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        } else
        {
            transform.parent = null;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public float GetScore()
    {
        return currentBudget - currentAnger;
    }
}
