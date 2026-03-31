using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        float[] rawData;
        public float[] RawData
        {
            get { return rawData; }
            set {
                rawData = value;
                trainingEffects=new float[(int)TrainingScheduleType.Count][];
                for (int i = 0; i < trainingEffects.Length; i++)
                {
                    trainingEffects[i]=new float[AttributesPerSchedule];
                    for (int j = 0; j < trainingEffects[i].Length; j++)
                    {
                        trainingEffects[i][j] = RawData[i* AttributesPerSchedule+j];
                    }
                }

            }
        }
        float[][] trainingEffects;
        public float[][] TrainingEffects {
            get { return trainingEffects; } 
        }

        private const int AttributesPerSchedule = 27;

    }
}
