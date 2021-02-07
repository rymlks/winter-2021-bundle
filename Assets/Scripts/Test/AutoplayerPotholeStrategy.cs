using System.Linq;

namespace Test
{
    public abstract class AutoplayerPotholeStrategy
    {
        public abstract void execute(GameManager manager);

        public abstract string getDescription();
        
        protected void fillPothole(Pothole toFill, PlaythroughStatistics stats, string methodToUse)
        {
            ContextMenuController.ContextMenuOption optionToUse =
                toFill.GetRepairOptions().First(option => option.label == methodToUse);
            if (stats.currentBudget >= optionToUse.cost || stats.currentLabor >= optionToUse.labor)
            {
                optionToUse.callback();
            }
            
        }
    }
}