using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Test;
using UnityEngine;

public class AutoPlayer : MonoBehaviour, GameManagerObserver
{

    private AutoplayerPotholeStrategy potholeStrategy;
    public 
    // Start is called before the first frame update
    void Start()
    {
        potholeStrategy = new ThrowNRollEverythingPotholeStrategy();
        FindObjectOfType<GameManager>().RegisterObserver(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NotifyRoundBeginning(GameManager manager)
    {
        potholeStrategy.execute(manager);
    }
}
