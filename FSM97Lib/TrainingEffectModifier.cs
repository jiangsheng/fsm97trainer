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
        public int[] sprintRoundsForEachSpeed = new int[100];
        public double[] accelerationLostForEachHeading = new double[100];
        public int[] weightLiftingRoundsForEachDetermination = new int[100];
        //public int[] speedLostForEachWeightLiftingRound = new int[100];
        public int[] sprintRoundsForEachAcceleration = new int[100];
        public int[] trainingMatchtRoundsForEachShooting = new int[100];
        public int[] trainingMatchtRoundsForEachPassing = new int[100];
        public const double ConstantFastCeiling = 0.999999;
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
                for (int i = 0; i < 100; i++)
                {
                    //round up for rounds required for each attribute gain, since we cannot train a fraction of a round
                    sprintRoundsForEachSpeed[i] = (int)(ConstantFastCeiling + i / rawData[(int)TrainingScheduleType.Sprinting * 27 + (int)PlayerAttribute.Speed]);
                    weightLiftingRoundsForEachDetermination[i] = (int)(ConstantFastCeiling + i / rawData[(int)TrainingScheduleType.WeightTraining * 27 + (int)PlayerAttribute.Determination]);
                    sprintRoundsForEachAcceleration[i] = (int)(ConstantFastCeiling + i / rawData[(int)TrainingScheduleType.Sprinting * 27 + (int)PlayerAttribute.Acceleration]);
                    //round up for each loss of speed or acceleration, so that the player will lose at least 1 point of speed or acceleration for each round of weight lifting or heading training
                       
                    if(rawData[(int)TrainingScheduleType.Heading * 27 + (int)PlayerAttribute.Acceleration]!=0)
                        accelerationLostForEachHeading[i] = -(int)(ConstantFastCeiling + i / rawData[(int)TrainingScheduleType.Heading * 27 + (int)PlayerAttribute.Acceleration]);

                    trainingMatchtRoundsForEachShooting[i] = (int)(ConstantFastCeiling + i / rawData[(int)TrainingScheduleType.Shooting * 27 + (int)PlayerAttribute.Shooting]);
                    trainingMatchtRoundsForEachPassing[i] = (int)(ConstantFastCeiling + i / rawData[(int)TrainingScheduleType.Passing * 27 + (int)PlayerAttribute.Passing]);
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
