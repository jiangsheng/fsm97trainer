using System.Text;

namespace Fsm97Trainer
{
    public class YoungPlayerEvaluation
    {
        public Player Player { get; set; }
        public int WeeksToMax { get; set; }
        public int FinalRating { get; set; }

        public StringBuilder Schedules { get; set; }
        public YoungPlayerEvaluation() { 
            Schedules = new StringBuilder();
        }
    }
}
