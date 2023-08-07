using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace FSM97Lib
{
    public static class TrainingScheduleEffect
    {
        public static float[] None = new float[] {
            0,0,0,//0-2 Speed Agility Acceleration
            0,0,0,//3-5 Stamina Strength Fitness
            0,0,0,0,0,//6-10 Shooting Passing Heading Control Dribbling
            0,0,//11 12 Cool Awareness
            0,0,0,//13-15 marking tackling Flair
            0,0,0,//16-18 Kicking Throwing Handling
            0,0,0,0,0,//19-23 Throwin Leadership Consistency  Determination Greed
            0,0,5//24-26 form moral energy
        };
        public static float[] Shooting = new float[] {
            0,0,0,//Speed Agility Acceleration
            -0.02f,0,0,//Stamina Strength Fitness
            0.04f,0,0,0,0,//Shooting Passing Heading Control Dribbling 
            0.02f,0,//11 12 Cool Awareness
            0,0,0,//13-15 marking tackling Flair
            0,0,0,//16-18 Kicking Throwing Handling
            0,0,0,0,0,//form moral energy
            0,0,-0.05f//Throwin Leadership Consistency  Determination Greed
        };
        public static float[] Passing = new float[] {
            0,0,0,//Speed Agility Acceleration
            0,0,0,//Stamina Strength Fitness
            0,0.04f,0,0,0,//Shooting Passing Heading Control Dribbling 
            0.02f,0,//11 12 Cool Awareness
            -0.005f,-0.005f,0,//13-15 marking tackling Flair
            0,0,0,//16-18 Kicking Throwing Handling
            0,0,0,0,0,//Throwin Leadership Consistency  Determination Greed
            0,0,-0.05f//form moral energy
        };
        public static float[] Heading = new float[] {
            0,0,-0.01f,//Speed Agility Acceleration
            0,-0.01f,0,//Stamina Strength Fitness
            0,0,0.04f,0,0,//Shooting Passing Heading Control Dribbling 
            0,0,//11 12 Cool Awareness
            0,0,0,//13-15 marking tackling Flair
            0,0,0,//16-18 Kicking Throwing Handling
            0,0,0,0,0,//Throwin Leadership Consistency  Determination Greed
            0,0,-0.10f//form moral energy
        };
        public static float[] Sprinting = new float[] {
            0.04f,0.02f,0.04f,//Speed Agility Acceleration
            0.02f,0,0.03f,//Stamina Strength Fitness
             -0.005f, -0.005f, -0.005f, -0.005f, -0.005f,//Shooting Passing Heading Control Dribbling 
            0,0,//11 12 Cool Awareness
             -0.005f, -0.005f,0,//13-15 marking tackling Flair
            0,0,0,//16-18 Kicking Throwing Handling
            0,0,0,0,0,//Throwin Leadership Consistency  Determination Greed
            0,0,-3f//form moral energy
        };
        public static float[] Jogging = new float[] {
            0,0,0,//Speed Agility Acceleration
            0.04f,0,0.02f,//Stamina Strength Fitness
             -0.005f, -0.005f, -0.005f, -0.005f, -0.005f,//Shooting Passing Heading Control Dribbling 
            0,0,//11 12 Cool Awareness
             -0.005f, -0.005f,0,//13-15 marking tackling Flair
            0,0,0,//16-18 Kicking Throwing Handling
            0,0,0,0,0,//Throwin Leadership Consistency  Determination Greed
            0,0,-2f//form moral energy
        };
        public static float[] Control = new float[] {
            0,0,0,//Speed Agility Acceleration
            0,0,0,//Stamina Strength Fitness
            0,0,0,0.04f,0.03f,//Shooting Passing Heading Control Dribbling 
            0.04f,0.01f,//11 12 Cool Awareness
            0,0,0.02f,//13-15 marking tackling Flair
            0,0,0,//16-18 Kicking Throwing Handling
            0,0,0.03f,0,0,//Throwin Leadership Consistency  Determination Greed
            0,0,-0.05f//form moral energy
        };
        public static float[] Exercise = new float[] {
            0,0.04f,0,//Speed Agility Acceleration
            0.02f,0.02f,0,//Stamina Strength Fitness
            0,0,0,0,0,//Shooting Passing Heading Control Dribbling 
            0,0,//Cool Awareness
            -0.003f,-0.003f,0,//marking tackling Flair
            0,0,0,//Kicking Throwing Handling
            0,0,0,0,0,//Throwin Leadership Consistency  Determination Greed
            0,0,-1f//form moral energy
        }; public static float[] WeightTraining = new float[] {
            -0.01f,0,0,//Speed Agility Acceleration
            0,0.05f,0.02f,//Stamina Strength Fitness
            -0.005f, -0.005f, -0.005f, -0.005f, -0.005f,//Shooting Passing Heading Control Dribbling 
            0,0,//Cool Awareness
            -0.005f,-0.005f,0,//marking tackling Flair
            0,0,0,//Kicking Throwing Handling
            0,0,0,0.04f,0,//Throwin Leadership Consistency  Determination Greed
            0,0,-3f//form moral energy
        };
        public static float[] ZonalDefence = new float[] {
            0,0,0,//Speed Agility Acceleration
            -0.005f,-0.005f,-0.005f,//Stamina Strength Fitness
            0,0,0,0,0,//Shooting Passing Heading Control Dribbling
            0,0.04f,//Cool Awareness
            0,0,0,//marking tackling Flair
            0,0,0,//Kicking Throwing Handling
            0,0,0,0,0,//Throwin Leadership Consistency  Determination Greed
            0,0.04f,-0.10f//form moral energy
        };
        public static float[] Marking = new float[] {
            0,0,0,//Speed Agility Acceleration
            0,0,0,//Stamina Strength Fitness
            -0.005f,-0.005f,-0.005f,-0.005f,-0.005f,//Shooting Passing Heading Control Dribbling
            0,0.02f,//Cool Awareness
            0.04f,0.02f,0,///marking tackling Flair 
            0,0,0,//Kicking Throwing Handling
            0,0,0,0,0,//Throwin Leadership Consistency  Determination Greed
            0,0,-0.10f//form moral energy
        };
        public static float[] Tackling = new float[] {
            0,0,0,//Speed Agility Acceleration
            0,0,0,//Stamina Strength Fitness
            -0.005f,-0.005f,-0.005f,-0.005f,-0.005f,//Shooting Passing Heading Control Dribbling
            0,0.02f,//Cool Awareness
            0.02f,0.04f,0,///marking tackling Flair 
            0,0,0,//Kicking Throwing Handling
            0,0,0,0,0,//Throwin Leadership Consistency  Determination Greed
            0,0,-0.10f//form moral energy
        };
        public static float[] Physiotherapist = new float[] {
            -0.005f,-0.005f,-0.005f,//Speed Agility Acceleration
            0,0,0,//Stamina Strength Fitness
            -0.005f,-0.005f,-0.005f,-0.005f,-0.005f,//Shooting Passing Heading Control Dribbling
            0,0,//Cool Awareness
            -0.005f,-0.005f,0,//marking tackling Flair
            0,0,0,//Kicking Throwing Handling
            0,0,0,0,0,//Throwin Leadership Consistency  Determination Greed
            0,0.1f,12//form moral energy
        };
        public static float[] Handling = new float[] {
            0,0,0,//Speed Agility Acceleration
            0,0,0,//Stamina Strength Fitness
            0,0,0,0,0,//Shooting Passing Heading Control Dribbling
            0,0,//Cool Awareness
            0,0,0,//marking tackling Flair
            -0.01f,0.02f,0.04f,//Kicking Throwing Handling
            0,0,0,0,0,//Throwin Leadership Consistency  Determination Greed
            0,0,-0.05f//form moral energy
        };

        public static float[] GoalKeeping = new float[] {
            0,0.04f,0,//Speed Agility Acceleration
            0,0,0,//Stamina Strength Fitness
            0,0,0,0,0,//Shooting Passing Heading Control Dribbling
            0,0.03f,//Cool Awareness
            0,0,0,//marking tackling Flair
            -0.01f,0,0.02f,//Kicking Throwing Handling
            0,0,0,0,0,//Throwin Leadership Consistency  Determination Greed
            0,0,-0.1f//form moral energy
        };
        public static float[] Kicking = new float[] {
           0, -0.01f,0,//Speed Agility Acceleration
            0,0,0,//Stamina Strength Fitness
            0,0,0,0,0,//Shooting Passing Heading Control Dribbling
            0,0,//Cool Awareness
            0,0,0,//marking tackling Flair
            0.04f,-0.01f,0,//Kicking Throwing Handling
            0,0,0,0,0,//Throwin Leadership Consistency  Determination Greed
            0,0,-0.05f//form moral energy
        };
        public static float[] Throwing = new float[] {
           0, 0,0,//Speed Agility Acceleration
            0,0,0,//Stamina Strength Fitness
            0,0,0,0,0,//Shooting Passing Heading Control Dribbling
            0,0,//Cool Awareness
            0,0,0,//marking tackling Flair
            0,0.04f,0,//Kicking Throwing Handling
            0,0,0,0,0,//Throwin Leadership Consistency  Determination Greed
            0,0,-0.05f//form moral energy
        };
        public static float[] FiveASide = new float[] {
            0,0.02f,0,//Speed Agility Acceleration
            0.02f,0,0.03f,//Stamina Strength Fitness
            0.02f,0.04f,0.01f,0.04f,0.01f,//Shooting Passing Heading Control Dribbling
            0,0.02f,//Cool Awareness
            0.01f,0.03f,0.04f,//marking tackling Flair
            0,0,0,//Kicking Throwing Handling
            0,0,0,0,0,//Throwin Leadership Consistency  Determination Greed
            0,0.09f,-6f//form moral energy
        };
        public static float[] TrainingMatch = new float[] {
            0,0.02f,0,//Speed Agility Acceleration
            0.04f,-0.02f,0.02f,//Stamina Strength Fitness
            0.04f,0.04f,0.02f,0.02f,0.01f,//Shooting Passing Heading Control Dribbling
            0.02f,0.03f,//Cool Awareness
            0.02f,0.02f,0.03f,//marking tackling Flair
            0,0,0,//Kicking Throwing Handling
            0,0,0,0,0,//Throwin Leadership Consistency  Determination Greed
            0,0.09f,-5f//form moral energy
        };


        public static byte[] GetTrainingScheduleEffect(TrainingEffectModifier trainingEffectModifier)
        {
            float[] resultInFloat = new float[27 * 19];
            int index = 0;

            Array.Copy(None, 0, resultInFloat, (index++) * 27, 27);
            Array.Copy(Shooting, 0, resultInFloat, (index++) * 27, 27);
            Array.Copy(Passing, 0, resultInFloat, (index++) * 27, 27);
            Array.Copy(Heading, 0, resultInFloat, (index++) * 27, 27);
            Array.Copy(Sprinting, 0, resultInFloat, (index++) * 27, 27);
            Array.Copy(Jogging, 0, resultInFloat, (index++) * 27, 27);
            Array.Copy(Control, 0, resultInFloat, (index++) * 27, 27);
            Array.Copy(Exercise, 0, resultInFloat, (index++) * 27, 27);
            Array.Copy(WeightTraining, 0, resultInFloat, (index++) * 27, 27);
            Array.Copy(ZonalDefence, 0, resultInFloat, (index++) * 27, 27);
            Array.Copy(Marking, 0, resultInFloat, (index++) * 27, 27);
            Array.Copy(Tackling, 0, resultInFloat, (index++) * 27, 27);
            Array.Copy(Physiotherapist, 0, resultInFloat, (index++) * 27, 27);
            Array.Copy(Handling, 0, resultInFloat, (index++) * 27, 27);
            Array.Copy(GoalKeeping, 0, resultInFloat, (index++) * 27, 27);
            Array.Copy(Kicking, 0, resultInFloat, (index++) * 27, 27);
            Array.Copy(Throwing, 0, resultInFloat, (index++) * 27, 27);
            Array.Copy(FiveASide, 0, resultInFloat, (index++) * 27, 27);
            Array.Copy(TrainingMatch, 0, resultInFloat, (index++) * 27, 27);
            if (trainingEffectModifier.RemoveNegativeTraining || trainingEffectModifier.TrainingEffectX2)
            {
                for (int i = 0; i < 27 * 19; i++)
                {
                    if (trainingEffectModifier.RemoveNegativeTraining)
                    {
                        if (resultInFloat[i] < 0)
                            resultInFloat[i] = 0;
                    }
                    if (trainingEffectModifier.TrainingEffectX2 && resultInFloat[i] != 0)
                        resultInFloat[i] = resultInFloat[i] * 2;
                }
            }
            if (trainingEffectModifier.ShootingTrainGreed)
            {
                resultInFloat[(int)TrainingScheduleType.Sprinting* 27 + (int)PlayerAttribute.Greed] = 0.02f;
                resultInFloat[(int)TrainingScheduleType.Shooting * 27 + (int)PlayerAttribute.Greed] = 0.08f;
                resultInFloat[(int)TrainingScheduleType.FiveASide * 27 + (int)PlayerAttribute.Greed] = 0.05f;
                resultInFloat[(int)TrainingScheduleType.TrainingMatch * 27 + (int)PlayerAttribute.Greed] = 0.04f;
            }
            if (trainingEffectModifier.PassingTrainLeadership)
            {
                resultInFloat[(int)TrainingScheduleType.Passing * 27 + (int)PlayerAttribute.Leadership] = 0.08f;
                resultInFloat[(int)TrainingScheduleType.FiveASide * 27 + (int)PlayerAttribute.Leadership] = 0.04f;
                resultInFloat[(int)TrainingScheduleType.TrainingMatch * 27 + (int)PlayerAttribute.Leadership] = 0.05f;
                resultInFloat[(int)TrainingScheduleType.FiveASide * 27 + (int)PlayerAttribute.Dribbling] = 0.04f;
                resultInFloat[(int)TrainingScheduleType.TrainingMatch * 27 + (int)PlayerAttribute.Dribbling] = 0.05f;
                resultInFloat[(int)TrainingScheduleType.FiveASide * 27 + (int)PlayerAttribute.Consistency] = 0.04f;
                resultInFloat[(int)TrainingScheduleType.TrainingMatch * 27 + (int)PlayerAttribute.Consistency] = 0.04f;
                resultInFloat[(int)TrainingScheduleType.Control * 27 + (int)PlayerAttribute.Dribbling] = 0.16f;
            }
            if (trainingEffectModifier.ThrowingTrainThrowIn)
            {
                resultInFloat[(int)TrainingScheduleType.Throwing * 27 + (int)PlayerAttribute.ThrowIn] = 0.08f;
                resultInFloat[(int)TrainingScheduleType.FiveASide * 27 + (int)PlayerAttribute.ThrowIn] = 0.04f;
                resultInFloat[(int)TrainingScheduleType.TrainingMatch * 27 + (int)PlayerAttribute.ThrowIn] = 0.05f;
            }
            if (trainingEffectModifier.ImproveSpeed)
            {
                resultInFloat[(int)TrainingScheduleType.Exercise* 27 + (int)PlayerAttribute.Speed] = 0.04f;
                resultInFloat[(int)TrainingScheduleType.Exercise * 27 + (int)PlayerAttribute.Acceleration] = 0.04f;
                resultInFloat[(int)TrainingScheduleType.Jogging * 27 + (int)PlayerAttribute.Speed] = 0.06f;
                resultInFloat[(int)TrainingScheduleType.Jogging * 27 + (int)PlayerAttribute.Acceleration] = 0.02f;
                resultInFloat[(int)TrainingScheduleType.Marking * 27 + (int)PlayerAttribute.Speed] = 0.02f;
                resultInFloat[(int)TrainingScheduleType.Marking * 27 + (int)PlayerAttribute.Agility] = 0.04f;
                resultInFloat[(int)TrainingScheduleType.Marking * 27 + (int)PlayerAttribute.Acceleration] = 0.06f;
                resultInFloat[(int)TrainingScheduleType.Tackling * 27 + (int)PlayerAttribute.Speed] = 0.06f;
                resultInFloat[(int)TrainingScheduleType.Tackling * 27 + (int)PlayerAttribute.Agility] = 0.04f;
                resultInFloat[(int)TrainingScheduleType.Tackling * 27 + (int)PlayerAttribute.Acceleration] = 0.02f;
                resultInFloat[(int)TrainingScheduleType.ZonalDefence * 27 + (int)PlayerAttribute.Speed] = 0.04f;
                resultInFloat[(int)TrainingScheduleType.ZonalDefence * 27 + (int)PlayerAttribute.Agility] = 0.04f;
                resultInFloat[(int)TrainingScheduleType.ZonalDefence * 27 + (int)PlayerAttribute.Acceleration] = 0.04f;
                resultInFloat[(int)TrainingScheduleType.FiveASide * 27 + (int)PlayerAttribute.Speed] = 0.02f;
                resultInFloat[(int)TrainingScheduleType.FiveASide * 27 + (int)PlayerAttribute.Acceleration] = 0.06f;
                resultInFloat[(int)TrainingScheduleType.TrainingMatch * 27 + (int)PlayerAttribute.Speed] = 0.04f;
                resultInFloat[(int)TrainingScheduleType.TrainingMatch * 27 + (int)PlayerAttribute.Acceleration] = 0.04f;
            }
            if (trainingEffectModifier.KickingImproveSpeed) {
                resultInFloat[(int)TrainingScheduleType.Kicking * 27 + (int)PlayerAttribute.Speed] = 0.04f;
                resultInFloat[(int)TrainingScheduleType.Kicking * 27 + (int)PlayerAttribute.Shooting] = 0.04f;
            }
            if (trainingEffectModifier.HandlingImproveAgility)
            {
                resultInFloat[(int)TrainingScheduleType.Handling * 27 + (int)PlayerAttribute.Speed] = 0.02f;
                resultInFloat[(int)TrainingScheduleType.Handling* 27 + (int)PlayerAttribute.Agility] = 0.04f;
                resultInFloat[(int)TrainingScheduleType.Handling * 27 + (int)PlayerAttribute.Acceleration] = 0.06f;
            }
            if (trainingEffectModifier.HeadingImproveDetermination)
            {
                resultInFloat[(int)TrainingScheduleType.Heading * 27 + (int)PlayerAttribute.Heading] = 0.16f;
                resultInFloat[(int)TrainingScheduleType.Heading* 27 + (int)PlayerAttribute.Determination] = 0.08f;
                resultInFloat[(int)TrainingScheduleType.FiveASide * 27 + (int)PlayerAttribute.Determination] = 0.06f;
                resultInFloat[(int)TrainingScheduleType.TrainingMatch * 27 + (int)PlayerAttribute.Determination] = 0.04f;
            }
            byte[] result = new byte[resultInFloat.Length * 4];
            Buffer.BlockCopy(resultInFloat, 0, result, 0, result.Length);
            return result;
        }
        public static TrainingEffectModifier DetectModifiers(byte[] trainingEffectBytes)
        {
            float[] trainingEffectFloat = new float[27 * 19];
            Buffer.BlockCopy(trainingEffectBytes, 0, trainingEffectFloat, 0, trainingEffectBytes.Length);
            TrainingEffectModifier result = new TrainingEffectModifier();
            if (trainingEffectFloat[26] == 10)
                result.TrainingEffectX2 = true;
            if (trainingEffectFloat[(int)TrainingScheduleType.Shooting* 27 + (int)PlayerAttribute.Stamina] >= 0)
                result.RemoveNegativeTraining = true;
            if (trainingEffectFloat[(int)TrainingScheduleType.Throwing * 27 + (int)PlayerAttribute.ThrowIn] > 0)
                result.ThrowingTrainThrowIn = true;
            if (trainingEffectFloat[(int)TrainingScheduleType.Passing * 27 + (int)PlayerAttribute.Leadership] > 0)
                result.PassingTrainLeadership = true;
            if (trainingEffectFloat[(int)TrainingScheduleType.Shooting * 27 + (int)PlayerAttribute.Greed] > 0)
                result.ShootingTrainGreed = true; 
            if (trainingEffectFloat[(int)TrainingScheduleType.Exercise * 27 + (int)PlayerAttribute.Speed] > 0)
                result.ImproveSpeed = true;
            if (trainingEffectFloat[(int)TrainingScheduleType.Kicking * 27 + (int)PlayerAttribute.Speed] > 0)
                result.KickingImproveSpeed = true;
            if (trainingEffectFloat[(int)TrainingScheduleType.Handling * 27 + (int)PlayerAttribute.Agility] > 0)
                result.HandlingImproveAgility = true;
            if (trainingEffectFloat[(int)TrainingScheduleType.Heading * 27 + (int)PlayerAttribute.Determination] > 0)
                result.HeadingImproveDetermination = true;
            result.RawData = trainingEffectFloat;
            return result;
        }

        public static double GetScheduleEffect(int scheduleType, byte[] attributes,TrainingEffectModifier trainingEffectModifier)
        {
            double sum = 0;
            for (int i = 0; i < (int)PlayerAttribute.Count; i++)
            {
                int attribute = 99 - attributes[i];
                if (attribute < 0) attribute = 0;
                sum += attribute*trainingEffectModifier.RawData[scheduleType * 27 + i];
            }
            return sum;
        }
    }
}
