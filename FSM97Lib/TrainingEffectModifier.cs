using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSM97Lib
{
    public  class TrainingEffectModifier
    {
        public bool RemoveNegativeTraining { get;  set; }
        public bool TrainingEffectX2 { get;  set; }
        public bool ThrowingTrainThrowIn { get; set; }
        public bool ShootingTrainGreed { get; set; }
        public bool PassingTrainLeadership { get; set; }
        public bool ImproveSpeed { get; set; }
        public bool KickingImproveSpeed { get; set; }
        public bool HandlingImproveAgility { get; set; }
        public bool HeadingImproveDetermination { get; set; }
        public float[] RawData { get; set; }
    }
}
