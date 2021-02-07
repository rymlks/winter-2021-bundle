using System.Linq;

namespace Test
{
    public abstract class AutoplayerRoadStrategy
    {
        public abstract void execute(GameManager manager);

        public abstract string getDescription();
    }
}