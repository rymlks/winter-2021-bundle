using System.Collections.Generic;
using System.Linq;

namespace Test
{
    public class ResurfaceRoadByPotholeCountStrategy : AutoplayerRoadStrategy
    {
        private int _minimumRequiredPotholes;
        private string _resurfaceMaterial;

        public ResurfaceRoadByPotholeCountStrategy(string resurfaceMaterial, int minimumRequiredPotholes = 0)
        {
            _resurfaceMaterial = resurfaceMaterial;
            _minimumRequiredPotholes = minimumRequiredPotholes;
        }

        public override void execute(GameManager manager)
        {
            foreach (Road road in manager.potholeController.roads)
            {
                if (road && road.potholes.Count >= _minimumRequiredPotholes)
                {
                    base.resurfaceRoad(road, manager.playthroughStatistics, _resurfaceMaterial);
                }
            }
        }

        public override string getDescription()
        {
            return "Resurface every road with at least " + _minimumRequiredPotholes + "potholes with " +
                   _resurfaceMaterial;
        }
    }
}