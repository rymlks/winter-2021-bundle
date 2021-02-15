using System.Collections;
using UnityEngine;

namespace Test
{
    public class AutoPlayer : MonoBehaviour, GameManagerObserver
    {
        private AutoplayerPotholeStrategy _potholeStrategy;
        private AutoplayerRoadStrategy _roadStrategy;
        public bool autoEndRounds = true;
        public bool recordEndGameStats = true;

        public void Start()
        {
            _potholeStrategy = new AlwaysRepairPotholeStrategy("Asphalt Patch");
            _roadStrategy = new ResurfaceRoadByPotholeCountStrategy("Gravel", 3);
            FindObjectOfType<GameManager>().RegisterObserver(this);
        }

        public void NotifyRoundBeginning(GameManager manager)
        {
            StartCoroutine(ExecuteStrategy(manager));
        }

        protected IEnumerator ExecuteStrategy(GameManager manager)
        {
            _roadStrategy.execute(manager);
            yield return new WaitForSeconds(1);
            _potholeStrategy.execute(manager);
            yield return new WaitForSeconds(1);
            if (autoEndRounds)
            {
                
                endRound(manager);
            }
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
            return this._roadStrategy.getDescription() + ".  Then, " + this._potholeStrategy.getDescription();
        }
    }
}