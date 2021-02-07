using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    public class NeverResurfaceRoadStrategy : AutoplayerRoadStrategy
    {
        public override void execute(GameManager manager)
        {
        }

        public override string getDescription()
        {
            return "Never resurface roads, ever";
        }
    }
}