using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    public class AlwaysRepairPotholeStrategy : AutoplayerPotholeStrategy
    {
        private string _repairMethod;

        public AlwaysRepairPotholeStrategy(string method)
        {
            _repairMethod = method;
        }

        public override void execute(GameManager manager)
        {
            List<Pothole> openPotholes = manager.potholeController.GetOpenPotholes();
            openPotholes.ForEach(pothole =>
            {
                base.fillPothole(pothole, manager.playthroughStatistics, _repairMethod);
            });
        }

        public override string getDescription()
        {
            return _repairMethod + " every open pothole until out of money";
        }
    }
}