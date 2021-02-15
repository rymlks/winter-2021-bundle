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
                toFill.GetRepairOptions().FirstOrDefault(option => option.label == methodToUse);
            if (optionToUse != null && stats.currentBudget >= optionToUse.cost && stats.currentLabor >= optionToUse.labor)
            {
                optionToUse.callback();
            }
            
        }
    }
}