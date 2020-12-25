using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;

public class Pothole : MonoBehaviour
{
    private float _angerPerRound;
    
    void Start()
    {
        this._angerPerRound = Random.value * FindObjectOfType<PotholeController>().maximumPotholeAngerPerRound;
    }

    public float getAngerCausedPerRound()
    {
        return this._angerPerRound;
    }

}
