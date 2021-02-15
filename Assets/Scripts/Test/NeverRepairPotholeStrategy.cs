using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    public class NeverRepairPotholeStrategy : AutoplayerPotholeStrategy
    {

        public NeverRepairPotholeStrategy()
        {
        }

        public override void execute(GameManager manager)
        {
        }

        public override string getDescription()
        {
            return "Never patch open potholes, ever";
        }
    }
}