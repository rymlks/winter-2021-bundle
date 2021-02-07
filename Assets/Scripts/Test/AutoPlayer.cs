using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Test;
using UnityEngine;

public class AutoPlayer : MonoBehaviour, GameManagerObserver
{

    private AutoplayerPotholeStrategy potholeStrategy;
    public bool autoEndRounds = true;
    public void Start()
    {
        potholeStrategy = new ThrowNRollEverythingPotholeStrategy();
        FindObjectOfType<GameManager>().RegisterObserver(this);
    }

    public void NotifyRoundBeginning(GameManager manager)
    {
        potholeStrategy.execute(manager);
        if(autoEndRounds){endRound(manager);}
    }

    protected void endRound(GameManager manager)
    {
        manager.NextRound();
    }
}
