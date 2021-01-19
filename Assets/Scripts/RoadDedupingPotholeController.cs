using UnityEngine;

namespace DefaultNamespace
{
    public class RoadDedupingPotholeController : PotholeController
    {
        public override void SortAndSumRoads()
        {
            base.SortAndSumRoads();
            Debug.Log("Deduping roads...");
        }
    }
}