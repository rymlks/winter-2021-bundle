using System.Collections;
using System.Collections.Generic;
using Test;
using UnityEngine;

public class AutoPlayer : MonoBehaviour, GameManagerObserver
{

    private AutoplayerPotholeStrategy potholeStrategy;
    public bool autoEndRounds = true;
    public bool recordEndGameStats = true;
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

    public void NotifyGameEnding(GameManager manager, GameEndingReason reason)
    {
        if (recordEndGameStats)
        {
            PlaythroughReportWriter.WriteToFile(manager, reason, this);
        }
    }

    protected void endRound(GameManager manager)
    {
        manager.NextRound();
    }

    public string GetStrategyDescription()
    {
        return this.potholeStrategy.getDescription();
    }
}
