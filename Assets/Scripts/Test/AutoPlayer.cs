using System.Collections;
using System.Collections.Generic;
using Test;
using UnityEngine;

public class AutoPlayer : MonoBehaviour, GameManagerObserver
{

    private AutoplayerPotholeStrategy _potholeStrategy;
    private AutoplayerRoadStrategy _roadStrategy;
    public bool autoEndRounds = true;
    public bool recordEndGameStats = true;
    public void Start()
    {
        _potholeStrategy = new AlwaysRepairPotholeStrategy("Throw 'n' Roll");
        _roadStrategy = new NeverResurfaceRoadStrategy();
        FindObjectOfType<GameManager>().RegisterObserver(this);
    }

    public void NotifyRoundBeginning(GameManager manager)
    {
        _roadStrategy.execute(manager);
        _potholeStrategy.execute(manager);
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
        return this._potholeStrategy.getDescription() + ".  " + this._roadStrategy.getDescription();
    }
}
