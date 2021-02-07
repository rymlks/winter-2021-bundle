using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    public class ThrowNRollEverythingPotholeStrategy : AutoplayerPotholeStrategy
    {
        public override void execute(GameManager manager)
        {
            List<Pothole> openPotholes = manager.potholeController.GetOpenPotholes();
            openPotholes.ForEach(pothole =>
            {
                base.fillPothole(pothole, manager.playthroughStatistics, "Throw 'n' Roll");
            });
        }

        public override string getDescription()
        {
            return "Throw 'n' Roll every open pothole until out of money";
        }
    }
}