using System.Linq;

namespace Test
{
    public abstract class AutoplayerRoadStrategy
    {
        public abstract void execute(GameManager manager);

        public abstract string getDescription();
        
        
        protected void resurfaceRoad(Road toResurface, PlaythroughStatistics stats, string methodToUse)
        {
            ContextMenuController.ContextMenuOption optionToUse =
                toResurface.GetRepairOptions().First(option => option.label == methodToUse);
            if (stats.currentBudget >= optionToUse.cost && stats.currentLabor >= optionToUse.labor)
            {
                optionToUse.callback();
            }
            
        }
    }
}