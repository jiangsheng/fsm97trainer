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
                trainingEffects=new TrainingActivity[(int)TrainingActivityType.Count];
                for (int i = 0; i < trainingEffects.Length; i++)
                {
                    var trainingEffect=new TrainingActivity();
                    trainingEffect.Activity = (TrainingActivityType)i;
                    trainingEffect.Effects = new float[AttributesPerSchedule];
                    for (int j = 0; j < trainingEffect.Effects.Length; j++)
                    {
                        trainingEffect.Effects[j] = RawData[i* AttributesPerSchedule+j];
                    }
                    trainingEffects[i] = trainingEffect;
                }
                for (int i = 0; i < 100; i++)
                {
                    //round up for rounds required for each attribute gain, since we cannot train a fraction of a round
                    sprintRoundsForEachSpeed[i] = (int)(ConstantFastCeiling + i / rawData[(int)TrainingActivityType.Sprinting * 27 + (int)PlayerAttribute.Speed]);
                    weightLiftingRoundsForEachDetermination[i] = (int)(ConstantFastCeiling + i / rawData[(int)TrainingActivityType.WeightTraining * 27 + (int)PlayerAttribute.Determination]);
                    sprintRoundsForEachAcceleration[i] = (int)(ConstantFastCeiling + i / rawData[(int)TrainingActivityType.Sprinting * 27 + (int)PlayerAttribute.Acceleration]);
                    //round up for each loss of speed or acceleration, so that the player will lose at least 1 point of speed or acceleration for each round of weight lifting or heading training
                       
                    if(rawData[(int)TrainingActivityType.Heading * 27 + (int)PlayerAttribute.Acceleration]!=0)
                        accelerationLostForEachHeading[i] = -(int)(ConstantFastCeiling + i / rawData[(int)TrainingActivityType.Heading * 27 + (int)PlayerAttribute.Acceleration]);

                    trainingMatchtRoundsForEachShooting[i] = (int)(ConstantFastCeiling + i / rawData[(int)TrainingActivityType.Shooting * 27 + (int)PlayerAttribute.Shooting]);
                    trainingMatchtRoundsForEachPassing[i] = (int)(ConstantFastCeiling + i / rawData[(int)TrainingActivityType.Passing * 27 + (int)PlayerAttribute.Passing]);
                }
            }
        }
        TrainingActivity[] trainingEffects;
        public TrainingActivity[] TrainingEffects {
            get { return trainingEffects; } 
        }

        private const int AttributesPerSchedule = 27;

    }
}
