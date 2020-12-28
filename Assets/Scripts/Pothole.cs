using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pothole : MonoBehaviour
{
    private float _angerPerRound;
    
    void Start()
    {
        this._angerPerRound = Random.value * FindObjectOfType<BalanceParameters>().maximumPotholeAngerPerRound;
    }

    public float getAngerCausedPerRound()
    {
        return this._angerPerRound;
    }

}
