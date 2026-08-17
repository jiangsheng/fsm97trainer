using FSM97Lib;
using System.Collections.Generic;
using System.Text;

namespace Fsm97Trainer
{
    public class YoungPlayerEvaluation
    {
        public PlayerModelDouble Player { get; set; }
        public int WeeksToMax { get; set; }
        public double FinalRating { get; set; }

        //public StringBuilder Schedules { get; set; }
        public LinkedList<WeeklyTrainingSchedule> WeeklyTrainingSchedules { get; set; } = new LinkedList<WeeklyTrainingSchedule>();
        public YoungPlayerEvaluation() { 
        }
    }
}
