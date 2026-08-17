using FSM97Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fsm97Trainer
{
    public class WeeklyTrainingSchedule
    {
        public List<TrainingScheduleSteps> Steps { get; set; }
        public int Weeks { get; set; }
        public List<BottleneckAttributes> BottleneckAttributes { get; set; }
        public PlayerModelDouble Player { get; set; }
    }
}
