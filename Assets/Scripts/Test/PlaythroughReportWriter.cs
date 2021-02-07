using System;
using UnityEngine;

namespace Test
{

    public class PlaythroughReportWriter
    {
        private struct PlaythroughReport
        {
            public int finalMonth;
            public int finalYear;
            public string gameResult;
            public string strategyUsed;
            public float finalAnger;
            public float finalBudget;
            public float finalLabor;
        }

        public static void WriteToFile(GameManager manager, GameEndingReason reason, AutoPlayer autoPlayer)
        {
            PlaythroughReport toWrite = FromStatistics(manager.playthroughStatistics);
            //on the next two lines, account for zero-indexing
            toWrite.finalMonth = manager.currentRound + 1;
            toWrite.finalYear = manager.currentYear + 1;
            toWrite.gameResult = reason.ToString();
            toWrite.strategyUsed = autoPlayer.GetStrategyDescription();
            System.IO.File.WriteAllText("Assets/Autoplaythroughs/"+manager.playthroughStatistics.cityName+Guid.NewGuid()+".json",
                JsonUtility.ToJson(toWrite,
                    true));
        }

        private static PlaythroughReport FromStatistics(PlaythroughStatistics statistics)
        {
            PlaythroughReport report = new PlaythroughReport();
            report.finalAnger = statistics.currentAnger;
            report.finalLabor = statistics.currentLabor;
            report.finalBudget = statistics.currentBudget;
            return report;
        }
    }
}