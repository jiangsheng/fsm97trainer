using FSM97Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fsm97Trainer
{
    public class TrainingScheduleSteps
    {
        public PlayerAttribute ForPlayerAttribute { get; set; }
        public TrainingScheduleType TrainingScheduleType { get; set; }
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(TrainingScheduleType.ToLocalizedString());
            stringBuilder.Append(" (");
            stringBuilder.Append(ForPlayerAttribute.ToLocalizedString());
            stringBuilder.Append(")");
            return stringBuilder.ToString();
        }
    }
}
