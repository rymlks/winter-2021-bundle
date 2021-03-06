﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public Text scoreText;

    public void Start()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Stats");
        if (objs.Length == 1)
        {
            GameObject stats = objs[0];
            scoreText.text = "Score: " + Mathf.Round(stats.GetComponent<PlaythroughStatistics>().GetScore());
            Destroy(stats);
        } else
        {
            scoreText.text = "";
        }
    }
}
