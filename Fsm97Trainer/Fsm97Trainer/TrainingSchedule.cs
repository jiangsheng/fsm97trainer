using FSM97Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fsm97Trainer
{

    public class TrainingSchedule
    {
        static int[] stages = new int[] { 25, 40, 55, 70, 85, 99 };
        public static TrainingScheduleType[] GetTrainingSchedule(Player player, bool autoResetStatus, bool maxEnergy, bool maxPower, bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier)
        {
            TrainingScheduleType[] schedule;
            if (player.Fitness < 99 && player.Position == (byte)PlayerPosition.GK && player.BestPosition != (byte)PlayerPosition.GK)
            {
                schedule = (TrainingScheduleType[])GetTrainingSchedule(player, (PlayerPosition)player.BestPosition, maxPower, noAlternativeTraining, trainingEffectModifier).Clone();
            }
            else
                schedule = (TrainingScheduleType[])GetTrainingSchedule(player, (PlayerPosition)player.Position, maxPower, noAlternativeTraining, trainingEffectModifier).Clone();

            if (!autoResetStatus && !maxEnergy && !trainingEffectModifier.RemoveNegativeTraining)
            {
                if (player.Status == 0 && player.Position != (byte)PlayerPosition.GK && player.Fitness < 99)
                {
                    schedule[1] = schedule[3] = schedule[5] = TrainingScheduleType.Physiotherapist;
                }
            }
            return schedule;
        }
        public static TrainingScheduleType[] GetTrainingSchedule(Player player, PlayerPosition position, bool maxPower, bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier)
        {
            switch (position)
            {
                case PlayerPosition.GK: return GetGKTrainingSchedule(player, maxPower, noAlternativeTraining, trainingEffectModifier);
                case PlayerPosition.LB:
                case PlayerPosition.RB: return GetLRBTrainingSchedule(player, maxPower, noAlternativeTraining, trainingEffectModifier);
                case PlayerPosition.CD: return GetCDTrainingSchedule(player, maxPower, noAlternativeTraining, trainingEffectModifier);
                case PlayerPosition.DM: return GetDMTrainingSchedule(player, maxPower, noAlternativeTraining, trainingEffectModifier);
                case PlayerPosition.SW: return GetSWTrainingSchedule(player, maxPower, noAlternativeTraining, trainingEffectModifier);

                case PlayerPosition.LWB:
                case PlayerPosition.RWB:
                    return GetLRWBTrainingSchedule(player, maxPower, noAlternativeTraining, trainingEffectModifier);
                case PlayerPosition.LM:
                case PlayerPosition.RM:
                case PlayerPosition.AM:
                    return GetLRAMTrainingSchedule(player,position, maxPower, noAlternativeTraining, trainingEffectModifier);
                case PlayerPosition.LW:
                case PlayerPosition.RW: return GetLRWTrainingSchedule(player, maxPower, noAlternativeTraining, trainingEffectModifier);
                case PlayerPosition.FR: return GetFRTrainingSchedule(player, maxPower, noAlternativeTraining, trainingEffectModifier);
                case PlayerPosition.SS: return GetFORSSTrainingSchedule(player, position, maxPower, noAlternativeTraining, trainingEffectModifier);
                case PlayerPosition.FOR: return GetFORSSTrainingSchedule(player, position, maxPower, noAlternativeTraining, trainingEffectModifier);
                default: return GenericTraining(player, maxPower, noAlternativeTraining, trainingEffectModifier);
            }
        }
        private static TrainingScheduleType[] GetGKTrainingSchedule(Player player, bool maxPower, bool noAlternativeTraining
            , TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < player.Handling)
            {
                return ImproveFitness(player, trainingEffectModifier);
            }
            foreach (var  stage in stages)
            {
                if (player.Agility < stage || player.Handling < stage || player.Kicking < stage || player.Throwing < stage ||
                    player.Consistency < stage || player.Control < stage || player.Passing < stage || player.Speed < stage)
                {
                    return GetGKTrainingScheduleStage(player, maxPower, noAlternativeTraining
                    , trainingEffectModifier, stage);
                }
            }
            return GenericTraining(player, maxPower, noAlternativeTraining, trainingEffectModifier);
        }

        private static TrainingScheduleType[] GetGKTrainingScheduleStage(Player player,
            bool maxPower, bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier, int stageMinimum)
        {
            TrainingScheduleType[] result;

            result = ImproveHandlingTo(player, stageMinimum, trainingEffectModifier);
            if (result != null) return result;

            result = ImproveKickingTo(player, stageMinimum, trainingEffectModifier);
            if (result != null) return result;

            result = ImproveThrowingTo(player, stageMinimum);
            if (result != null) return result;

            if (stageMinimum < 99)
            {
                result = ImproveConsistencyTo(player, stageMinimum);
                if (result != null) return result;

                result = ImproveCoolnessTo(player, stageMinimum);
                if (result != null) return result;
            }

            if (!trainingEffectModifier.RemoveNegativeTraining)
            {
                result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier);
                if (result != null) return result;
            }
            result = ImprovePassingTo(player, stageMinimum, noAlternativeTraining);
            if (result != null) return result;

            result = ImproveConsistencyTo(player, stageMinimum);
            if (result != null) return result;

            result = ImproveCoolnessTo(player, stageMinimum);
            if (result != null) return result;

            result = ImproveAwarenessTo(player, stageMinimum, maxPower, trainingEffectModifier);
            if (result != null) return result;

            result = ImproveControlTo(player, stageMinimum);
            if (result != null) return result;

            result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier);
            if (result != null) return result;

            return GenericTraining(player, maxPower, noAlternativeTraining, trainingEffectModifier);
        }

        private static TrainingScheduleType[] GetLRBTrainingSchedule(Player player, bool maxPower, bool noAlternativeTraining
             , TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, trainingEffectModifier);
            }
            foreach (var stage in stages)
            {
                if (player.Speed < stage || player.Passing < stage || player.Heading < stage || player.TackleDetermination < stage ||
                player.TackleSkill < stage || player.Coolness < stage || player.Awareness < stage || player.Consistency < stage
                || player.Determination < stage)
                {
                    return GetLRBTrainingScheduleStage(player, maxPower, noAlternativeTraining
                    , trainingEffectModifier, 70);
                }
            }
            return GenericTraining(player, maxPower, noAlternativeTraining, trainingEffectModifier);
        }

        private static TrainingScheduleType[] GetLRBTrainingScheduleStage(Player player, bool maxPower,
            bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier, int stageMinimum)
        {
            TrainingScheduleType[] result;

            result = ImproveDeterminationTo(player, stageMinimum, trainingEffectModifier);
            if (result != null) return result;

            if (stageMinimum < 99)
            {
                result = ImproveAwarenessTo(player, stageMinimum, maxPower, trainingEffectModifier);
                if (result != null) return result;

                result = ImprovePassingTo(player, stageMinimum, noAlternativeTraining);
                if (result != null) return result;

                result = ImproveConsistencyTo(player, stageMinimum);
                if (result != null) return result;
            }
            if (!trainingEffectModifier.RemoveNegativeTraining && stageMinimum == 99)
            {
                result = ImproveSpeedTo(player, stageMinimum, true, trainingEffectModifier);
                if (result != null) return result;
            }

            result = ImprovePassingTo(player, stageMinimum, noAlternativeTraining);
            if (result != null) return result;

            result = ImproveHeadingTo(player, stageMinimum, trainingEffectModifier);
            if (result != null) return result;

            result = ImproveTackleTo(player, stageMinimum, trainingEffectModifier);
            if (result != null) return result;

            result = ImproveConsistencyTo(player, stageMinimum);
            if (result != null) return result;

            result = ImproveCoolnessTo(player, stageMinimum);
            if (result != null) return result;

            result = ImproveAwarenessTo(player, stageMinimum, maxPower, trainingEffectModifier);
            if (result != null) return result;

            result = ImproveSpeedTo(player, stageMinimum, true, trainingEffectModifier);
            if (result != null) return result;

            return GenericTraining(player, maxPower, noAlternativeTraining, trainingEffectModifier);
        }

        private static TrainingScheduleType[] GetCDTrainingSchedule(Player player, bool maxPower, bool noAlternativeTraining
            , TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, trainingEffectModifier);
            }
            foreach (var stage in stages)
            {
                if (player.Speed < stage || player.Passing < stage || player.Heading < stage || player.TackleDetermination < stage ||
                player.TackleSkill < stage || player.Coolness < stage || player.Awareness < stage || player.Consistency < stage
                || (player.Leadership < stage && trainingEffectModifier.PassingTrainLeadership))
                {
                    return GetCDTrainingScheduleStage(player, maxPower, noAlternativeTraining
                    , trainingEffectModifier, stage);
                }
            }
            return GenericTraining(player, maxPower, noAlternativeTraining, trainingEffectModifier);
        }

        private static TrainingScheduleType[] GetCDTrainingScheduleStage(Player player, bool maxPower,
            bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier, int stageMinimum)
        {
            TrainingScheduleType[] result;
            if (stageMinimum < 99)
            {
                result = ImproveTackleTo(player, stageMinimum, trainingEffectModifier);
                if (result != null) return result;

                result = ImproveHeadingTo(player, stageMinimum, trainingEffectModifier);
                if (result != null) return result;
            }
            if (!trainingEffectModifier.RemoveNegativeTraining && stageMinimum == 99)
            {
                result = ImproveSpeedTo(player, stageMinimum, true, trainingEffectModifier);
                if (result != null) return result;
            }
            result = ImprovePassingTo(player, stageMinimum, noAlternativeTraining);
            if (result != null) return result;

            result = ImproveHeadingTo(player, stageMinimum, trainingEffectModifier);
            if (result != null) return result;

            result = ImproveLeadershipTo(player, stageMinimum, noAlternativeTraining, trainingEffectModifier);
            if (result != null) return result;

            result = ImproveTackleTo(player, stageMinimum, trainingEffectModifier);
            if (result != null) return result;

            result = ImproveConsistencyTo(player, stageMinimum);
            if (result != null) return result;

            result = ImproveCoolnessTo(player, stageMinimum);
            if (result != null) return result;

            result = ImproveSpeedTo(player, stageMinimum, true, trainingEffectModifier);
            if (result != null) return result;

            return GenericTraining(player, maxPower, noAlternativeTraining, trainingEffectModifier);
        }
        private static TrainingScheduleType[] GetLRWBTrainingSchedule(Player player, bool maxPower, bool noAlternativeTraining
            , TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, trainingEffectModifier);
            }
            foreach (var stage in stages)
            {
                if (player.Speed < stage || player.Agility < stage || player.Acceleration < stage || player.Passing < stage ||
                player.Dribbling < stage || player.Awareness < stage || player.Flair < stage)
                {
                    return GetLRWBTrainingScheduleStage(player, maxPower, noAlternativeTraining
                    , trainingEffectModifier, stage);
                }
            }
            return GenericTraining(player, maxPower, noAlternativeTraining, trainingEffectModifier);
        }

        private static TrainingScheduleType[] GetLRWBTrainingScheduleStage(Player player, bool maxPower, bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier, int stageMinimum)
        {
            TrainingScheduleType[] result;
            if (stageMinimum < 99)
            {
                result = ImproveDribbleTo(player, stageMinimum);
                if (result != null) return result;

                result = ImproveTackleTo(player, stageMinimum, trainingEffectModifier);
                if (result != null) return result;

                result = ImprovePassingTo(player, stageMinimum, noAlternativeTraining);
                if (result != null) return result;
            }
            if (!trainingEffectModifier.RemoveNegativeTraining && stageMinimum == 99)
            {
                result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier);
                if (result != null) return result;

                result = ImproveAccelerationTo(player, stageMinimum,false, trainingEffectModifier);
                if (result != null) return result;
            }
            result = ImproveAgilityTo(player, stageMinimum);
            if (result != null) return result;
            result = ImprovePassingTo(player, stageMinimum, noAlternativeTraining);
            if (result != null) return result;
            result = ImproveDribbleTo(player, stageMinimum);
            if (result != null) return result;
            result = ImproveTackleTo(player, stageMinimum, trainingEffectModifier);
            if (result != null) return result;
            result = ImproveFlairTo(player, stageMinimum);
            if (result != null) return result;
            result = ImproveAwarenessTo(player, stageMinimum, maxPower, trainingEffectModifier);
            if (result != null) return result;
            result = ImproveSpeedTo(player, stageMinimum,false, trainingEffectModifier);
            if (result != null) return result;
            result = ImproveAccelerationTo(player, stageMinimum,false, trainingEffectModifier);
            if (result != null) return result;

            return GenericTraining(player, maxPower, noAlternativeTraining, trainingEffectModifier);
        }


        private static TrainingScheduleType[] GetSWTrainingSchedule(Player player, bool maxPower, bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, trainingEffectModifier);
            }
            foreach (var stage in stages)
            {
                if (player.Speed < stage || player.Acceleration < stage || player.Passing < stage || player.Dribbling < stage ||
               player.TackleDetermination < stage || player.TackleSkill < stage || player.Awareness < stage || player.Flair < stage)
                {
                    return GetSWTrainingScheduleStage(player, maxPower, noAlternativeTraining
                    , trainingEffectModifier, stage);
                }
            }

            return GenericTraining(player, maxPower, noAlternativeTraining, trainingEffectModifier);
        }

        private static TrainingScheduleType[] GetSWTrainingScheduleStage(Player player, bool maxPower,
            bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier, int stageMinimum)
        {
            TrainingScheduleType[] result;
            if (stageMinimum < 99)
            {
                result = ImproveAwarenessTo(player, stageMinimum, maxPower, trainingEffectModifier);
                if (result != null) return result;
                result = ImproveTackleTo(player, stageMinimum, trainingEffectModifier);
                if (result != null) return result;
                result = ImprovePassingTo(player, stageMinimum, noAlternativeTraining);
                if (result != null) return result;
                result = ImproveDribbleTo(player, stageMinimum);
                if (result != null) return result;
                result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier);
                if (result != null) return result;
            }
            if (!trainingEffectModifier.RemoveNegativeTraining && stageMinimum == 99)
            {
                result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier);
                if (result != null) return result;

                result = ImproveAccelerationTo(player, stageMinimum, true, trainingEffectModifier);
                if (result != null) return result;
            }

            result = ImprovePassingTo(player, stageMinimum, noAlternativeTraining);
            if (result != null) return result;

            result = ImproveHeadingTo(player, stageMinimum, trainingEffectModifier);
            if (result != null) return result;

            result = ImproveDribbleTo(player, stageMinimum);
            if (result != null) return result;

            result = ImproveTackleTo(player, stageMinimum, trainingEffectModifier);
            if (result != null) return result;

            result = ImproveAwarenessTo(player, stageMinimum, maxPower, trainingEffectModifier);
            if (result != null) return result;

            result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier);
            if (result != null) return result;

            result = ImproveAccelerationTo(player, stageMinimum, true, trainingEffectModifier);
            if (result != null) return result;

            return GenericTraining(player, maxPower, noAlternativeTraining, trainingEffectModifier);
        }

        private static TrainingScheduleType[] GetDMTrainingSchedule(Player player, bool maxPower, bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, trainingEffectModifier);
            }
            foreach (var stage in stages)
            {
                if (player.Speed < stage || player.Passing < stage || player.Heading < stage || player.Dribbling < stage ||
               player.TackleDetermination < stage || player.TackleSkill < stage || player.Awareness < stage)
                {
                    return GetDMTrainingScheduleStage(player, maxPower, noAlternativeTraining
                    , trainingEffectModifier, stage);
                }
            }
            return GenericTraining(player, maxPower, noAlternativeTraining, trainingEffectModifier);
        }

        private static TrainingScheduleType[] GetDMTrainingScheduleStage(Player player, bool maxPower, 
            bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier, int stageMinimum)
        {
            TrainingScheduleType[] result;
            if (stageMinimum < 99)
            {
                result = ImprovePassingTo(player, stageMinimum, noAlternativeTraining);
                if (result != null) return result;
                result = ImproveTackleTo(player, stageMinimum, trainingEffectModifier);
                if (result != null) return result;
                result = ImproveAwarenessTo(player, stageMinimum, maxPower, trainingEffectModifier);
                if (result != null) return result;
            }
            if (!trainingEffectModifier.RemoveNegativeTraining && stageMinimum == 99)
            {
                result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier);
                if (result != null) return result;
            }
            result = ImprovePassingTo(player, stageMinimum, noAlternativeTraining);
            if (result != null) return result;

            result = ImproveHeadingTo(player, stageMinimum, trainingEffectModifier);
            if (result != null) return result;

            result = ImproveTackleTo(player, stageMinimum, trainingEffectModifier);
            if (result != null) return result;
            result = ImproveAwarenessTo(player, stageMinimum, maxPower, trainingEffectModifier);
            if (result != null) return result;
            result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier);
            if (result != null) return result;

            return GenericTraining(player, maxPower, noAlternativeTraining, trainingEffectModifier);
        }

        private static TrainingScheduleType[] GetLRAMTrainingSchedule(Player player,
            PlayerPosition position,bool maxPower, bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, trainingEffectModifier);
            }
            foreach (var stage in stages)
            {
                if (player.Speed < stage || player.Acceleration < stage || player.Shooting < stage || player.Passing < stage
                    ||player.Control < stage || player.Dribbling < stage || player.TackleSkill < stage
                    || player.Awareness < stage || player.Flair < stage)
                {
                    return GetLRAMTrainingScheduleStage(player, position, maxPower, noAlternativeTraining
                    , trainingEffectModifier, stage);
                }
            }
            return GenericTraining(player, maxPower, noAlternativeTraining, trainingEffectModifier);
        }

        private static TrainingScheduleType[] GetLRAMTrainingScheduleStage(Player player, PlayerPosition position,
            bool maxPower, bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier, int stageMinimum)
        {
            TrainingScheduleType[] result;
            if (stageMinimum < 99)
            {
                if (position == PlayerPosition.AM)
                {
                    result = ImprovePassingTo(player, stageMinimum, noAlternativeTraining);
                    if (result != null) return result;
                    result = ImproveTackleSkillTo(player, stageMinimum - 1, trainingEffectModifier);
                    if (result != null) return result;

                }
                else
                {
                    result = ImproveTackleSkillTo(player, stageMinimum, trainingEffectModifier);
                    if (result != null) return result;
                    result = ImprovePassingTo(player, stageMinimum - 1, noAlternativeTraining);
                    if (result != null) return result;
                }
                result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier);
                if (result != null) return result;
            }
            if (!trainingEffectModifier.RemoveNegativeTraining && stageMinimum == 99)
            {
                result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier);
                if (result != null) return result;

                result = ImproveAccelerationTo(player, stageMinimum, false, trainingEffectModifier);
                if (result != null) return result;
            }
            result = ImproveShootingTo(player, stageMinimum);
            if (result != null) return result;
            if (stageMinimum == 99)
            {
                if (position == PlayerPosition.AM)
                {
                    result = ImprovePassingTo(player, stageMinimum, noAlternativeTraining);
                    if (result != null) return result;
                    result = ImproveTackleSkillTo(player, stageMinimum , trainingEffectModifier);
                    if (result != null) return result;

                }
                else
                {
                    result = ImproveTackleSkillTo(player, stageMinimum, trainingEffectModifier);
                    if (result != null) return result;
                    result = ImprovePassingTo(player, stageMinimum , noAlternativeTraining);
                    if (result != null) return result;
                }
            }
            result = ImproveControlTo(player, stageMinimum);
            if (result != null) return result;
            result = ImproveDribbleTo(player, stageMinimum);
            if (result != null) return result; 
            result = ImproveFlairTo(player, stageMinimum);
            if (result != null) return result;
            result = ImproveAwarenessTo(player, stageMinimum, maxPower, trainingEffectModifier);
            if (result != null) return result;
            result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier);
            if (result != null) return result;
            result = ImproveAccelerationTo(player, stageMinimum, false, trainingEffectModifier);
            if (result != null) return result;
            return GenericTraining(player, maxPower, noAlternativeTraining, trainingEffectModifier);
        }
        private static TrainingScheduleType[] GetLRWTrainingSchedule(Player player, bool maxPower, bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, trainingEffectModifier);
            }
            foreach (var stage in stages)
            {
                if (player.Speed < stage || player.Agility < stage || player.Acceleration < stage || player.Shooting < stage || player.Passing < stage
                    || player.Control < stage || player.Dribbling < stage || player.TackleSkill < stage
                    || player.Awareness < stage || player.Flair < stage)
                {
                    return GetLRWTrainingScheduleStage(player,  maxPower, noAlternativeTraining
                    , trainingEffectModifier, stage);
                }
            }
            return GenericTraining(player, maxPower, noAlternativeTraining, trainingEffectModifier);
        }

        private static TrainingScheduleType[] GetLRWTrainingScheduleStage(Player player, bool maxPower, 
            bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier, int stageMinimum)
        {
            TrainingScheduleType[] result;
            if (stageMinimum < 99)
            {
                result = ImprovePassingTo(player, stageMinimum, noAlternativeTraining);
                if (result != null) return result;
                result = ImproveDribbleTo(player, stageMinimum);
                if (result != null) return result;
            }
            result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier);
            if (result != null) return result;
            result = ImproveAccelerationTo(player, stageMinimum, false, trainingEffectModifier);
            if (result != null) return result;

            result = ImproveAgilityTo(player, stageMinimum);
            if (result != null) return result;

            result = ImproveShootingTo(player, stageMinimum);
            if (result != null) return result; 
            result = ImprovePassingTo(player, stageMinimum, noAlternativeTraining);
            if (result != null) return result;
            result = ImproveDribbleTo(player, stageMinimum);
            if (result != null) return result;
            result = ImproveControlTo(player, stageMinimum);
            if (result != null) return result;
            result = ImproveTackleSkillTo(player, stageMinimum, trainingEffectModifier);
            if (result != null) return result;

            result = ImproveFlairTo(player, stageMinimum);
            if (result != null) return result;
            result = ImproveAwarenessTo(player, stageMinimum, maxPower, trainingEffectModifier);
            if (result != null) return result; 
            result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier);
            if (result != null) return result;
            result = ImproveAccelerationTo(player, stageMinimum, false, trainingEffectModifier);
            if (result != null) return result;
            return GenericTraining(player, maxPower, noAlternativeTraining, trainingEffectModifier);
        }

        private static TrainingScheduleType[] GetFRTrainingSchedule(Player player, bool maxPower, bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, trainingEffectModifier);
            }
            foreach (var stage in stages)
            {
                if (player.Speed < stage || player.Agility < stage || player.Acceleration < stage 
                    || player.Shooting < stage || player.Passing < stage || player.Heading < stage
                    || player.Control < stage || player.Dribbling < stage 
                    || player.Awareness < stage || player.Flair < stage)
                {
                    return GetFRTrainingScheduleStage(player, maxPower, noAlternativeTraining
                    , trainingEffectModifier, stage);
                }
            }
            return GenericTraining(player, maxPower, noAlternativeTraining, trainingEffectModifier);
        }

        private static TrainingScheduleType[] GetFRTrainingScheduleStage(Player player, bool maxPower,
            bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier, int stageMinimum)
        {
            TrainingScheduleType[] result;
            if (stageMinimum < 99)
            {
                result = ImproveDribbleTo(player, stageMinimum);
                if (result != null) return result; 
                result = ImproveSpeedTo(player, stageMinimum, true, trainingEffectModifier);
                if (result != null) return result;
                result = ImprovePassingTo(player, stageMinimum, noAlternativeTraining);
                if (result != null) return result;
                result = ImproveFlairTo(player, stageMinimum);
                if (result != null) return result;
                result = ImproveAwarenessTo(player, stageMinimum, maxPower, trainingEffectModifier);
                if (result != null) return result;
                result = ImproveAccelerationTo(player, stageMinimum, true, trainingEffectModifier);
                if (result != null) return result;
            }
            result = ImproveSpeedTo(player, stageMinimum, true, trainingEffectModifier);
            if (result != null) return result;
            result = ImproveAccelerationTo(player, stageMinimum, true, trainingEffectModifier);
            if (result != null) return result;
            result = ImproveAgilityTo(player, stageMinimum);
            if (result != null) return result;
            result = ImproveShootingTo(player, stageMinimum);
            if (result != null) return result;
            result = ImprovePassingTo(player, stageMinimum, noAlternativeTraining);
            if (result != null) return result;
            result=ImproveHeadingTo(player, stageMinimum,trainingEffectModifier);
            if (result != null) return result;
            result = ImproveDribbleTo(player, stageMinimum);
            if (result != null) return result;
            result = ImproveControlTo(player, stageMinimum);
            if (result != null) return result;
            result = ImproveFlairTo(player, stageMinimum);
            if (result != null) return result;
            result = ImproveAwarenessTo(player, stageMinimum, maxPower, trainingEffectModifier);
            if (result != null) return result;
            return GenericTraining(player, maxPower, noAlternativeTraining, trainingEffectModifier);
        }

        private static TrainingScheduleType[] GetFORSSTrainingSchedule(Player player,PlayerPosition position, bool maxPower, bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, trainingEffectModifier);
            }
            foreach (var stage in stages)
            {
                if (player.Speed < stage || player.Agility < stage || player.Acceleration < stage
                    || player.Shooting < stage || player.Passing < stage || player.Heading < stage
                    || player.Control < stage || player.Dribbling < stage || player.TackleSkill < stage
                    || player.Awareness < stage || player.Flair < stage)
                {
                    return GetFORSSTrainingScheduleStage(player, position,maxPower, noAlternativeTraining
                    , trainingEffectModifier, stage);
                }
            }
            return GenericTraining(player, maxPower, noAlternativeTraining, trainingEffectModifier);
        }

        private static TrainingScheduleType[] GetFORSSTrainingScheduleStage(Player player, PlayerPosition position, bool maxPower, bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier, int stageMinimum)
        {
            TrainingScheduleType[] result;
            if (stageMinimum < 99)
            {
                result = ImproveShootingTo(player, stageMinimum);
                if (result != null) return result;
                if (position == PlayerPosition.FOR)
                {
                    result = ImproveSpeedTo(player, stageMinimum, true, trainingEffectModifier);
                    if (result != null) return result;
                    result = ImproveAccelerationTo(player, stageMinimum, true, trainingEffectModifier);
                    if (result != null) return result;
                    result = ImproveHeadingTo(player, stageMinimum, trainingEffectModifier);
                    if (result != null) return result;
                    result = ImproveControlTo(player, stageMinimum - 1);
                    if (result != null) return result;
                    result = ImproveFlairTo(player, stageMinimum - 1);
                    if (result != null) return result;
                }
                else {
                    result = ImprovePassingTo(player, stageMinimum, noAlternativeTraining);
                    if (result != null) return result;
                    result = ImproveControlTo(player, stageMinimum);
                    if (result != null) return result;
                    result = ImproveFlairTo(player, stageMinimum);
                    if (result != null) return result;
                }
            }
            if (!trainingEffectModifier.RemoveNegativeTraining && stageMinimum == 99)
            {
                result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier);
                if (result != null) return result;
                result = ImproveAccelerationTo(player, stageMinimum, true, trainingEffectModifier);
                if (result != null) return result;
            }
            result = ImproveAgilityTo(player, stageMinimum);
            if (result != null) return result;
            result = ImproveShootingTo(player, stageMinimum);
            if (result != null) return result;
            result = ImprovePassingTo(player, stageMinimum, noAlternativeTraining);
            if (result != null) return result;
            result = ImproveHeadingTo(player, stageMinimum, trainingEffectModifier);
            if (result != null) return result;
            result = ImproveDribbleTo(player, stageMinimum);
            if (result != null) return result;
            result = ImproveControlTo(player, stageMinimum);
            if (result != null) return result;

            result = ImproveCoolnessTo(player, stageMinimum);
            if (result != null) return result;

            result = ImproveFlairTo(player, stageMinimum);
            if (result != null) return result;

            result = ImproveAwarenessTo(player, stageMinimum, maxPower, trainingEffectModifier);
            if (result != null) return result;

            return GenericTraining(player, maxPower, noAlternativeTraining, trainingEffectModifier);
        }
        private static TrainingScheduleType[] GenericTraining(Player player, bool maxPower, bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier)
        {
            if (noAlternativeTraining)
            {
                if (player.Consistency < 99)
                    return TrainingSchedulePreset.ControlAllWeek;
                return TrainingSchedulePreset.MaintainShape;
            }
            if (player.Acceleration < 99)
            {
                if (player.Heading > 85)
                {
                    if (player.Determination < 99)
                    {
                        if (trainingEffectModifier.RemoveNegativeTraining)
                            return TrainingSchedulePreset.SprintingAllWeek;
                        return TrainingSchedulePreset.SprintingWithWeightTraining;
                    }
                    else
                        return TrainingSchedulePreset.SprintingAllWeek;
                }
                else
                {
                    if (trainingEffectModifier.RemoveNegativeTraining)
                        return TrainingSchedulePreset.SprintingAllWeek;
                    return TrainingSchedulePreset.SprintingWithHeading;
                }
            }
            if (player.Shooting < 99 || player.Passing < 99) return TrainingSchedulePreset.TrainingMatchAllWeek;
            if (player.Agility < 99) return TrainingSchedulePreset.TrainingMatchAllWeek;
            if (player.Heading < 99) return ImproveHeadingTo(player,99,trainingEffectModifier);
            if (player.Control < 99 || player.Dribbling < 99 || player.Coolness < 99) 
                return TrainingSchedulePreset.ControlAllWeek;
            if (player.Flair < 99) return TrainingSchedulePreset.FiveASideAllWeek;
            if (player.Awareness < 99) return ImproveAwareness(player, maxPower, trainingEffectModifier);
            if (player.TackleDetermination < 99 || player.TackleSkill < 99) 
                return ImproveTackle(player, trainingEffectModifier);
            if (player.Consistency < 99) return TrainingSchedulePreset.ControlAllWeek;
            if (player.Determination < 99) return ImproveDeterminationTo(player,99,trainingEffectModifier);
            if (player.Handling < 99) return ImproveHandlingTo(player, 99, trainingEffectModifier);
            if (player.Kicking < 99) return ImproveKickingTo(player,99,trainingEffectModifier);
            if (player.Throwing < 99) return TrainingSchedulePreset.ImproveThrowing;
            if (player.Strength < 99) return ImproveStrength(trainingEffectModifier);
            if (player.Leadership < 99 && trainingEffectModifier.PassingTrainLeadership)
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            if (player.ThrowIn < 99 && trainingEffectModifier.ThrowingTrainThrowIn)
                return TrainingSchedulePreset.ImproveThrowing;
            if (player.Greed < 99 && trainingEffectModifier.ShootingTrainGreed)
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            return TrainingSchedulePreset.MaintainShape;

        }


        private static TrainingScheduleType[] ImproveAwareness(Player player, bool maxPower, TrainingEffectModifier trainingEffectModifier)
        {
            if (!maxPower)
                return TrainingSchedulePreset.ImproveAwarenessScheduleMaxPower;
            if (trainingEffectModifier.RemoveNegativeTraining)
                return TrainingSchedulePreset.ImproveAwarenessScheduleMaxPower;
            return TrainingSchedulePreset.ImproveAwarenessSchedule;
        }

        private static TrainingScheduleType[] ImproveSpeedWithHeading(Player player)
        {
            if (player.Acceleration > player.Speed)
            {
                if (player.Heading < 85)
                {
                    return TrainingSchedulePreset.SprintingWithHeading;
                }
                else
                    return TrainingSchedulePreset.SprintingAllWeek;
            }
            else
                return TrainingSchedulePreset.SprintingAllWeek;
        }
        private static TrainingScheduleType[] ImproveAccelerationWithHeading(Player player)
        {
            if (player.Heading < 85)
            {
                return TrainingSchedulePreset.SprintingWithHeading;
            }
            else
                return TrainingSchedulePreset.SprintingAllWeek;
        }

        private static TrainingScheduleType[] ImproveSpeedWithWeightTraining(Player player)
        {
            if (player.Determination < 99)
            {
                return TrainingSchedulePreset.SprintingWithWeightTraining;
            }
            else
                return TrainingSchedulePreset.SprintingAllWeek;
        }
        private static TrainingScheduleType[] ImproveFitness(Player player, TrainingEffectModifier trainingEffectModifier)
        {
            var position = (PlayerPosition)player.Position;
            switch (position)
            {
                case PlayerPosition.GK:
                case PlayerPosition.LWB:
                case PlayerPosition.RWB:
                case PlayerPosition.LM:
                case PlayerPosition.RM:
                case PlayerPosition.AM:
                case PlayerPosition.LW:
                case PlayerPosition.RW:
                    if (player.Speed < 99)
                    {
                        return TrainingSchedulePreset.SprintingAllWeek;
                    }
                    break;
                case PlayerPosition.LB:
                case PlayerPosition.RB:
                    if (player.Speed < 99)
                    {
                        return ImproveSpeedWithWeightTraining(player);
                    }
                    break;
                default:
                    if (player.Speed < 99)
                    {
                        return ImproveSpeedWithHeading(player);
                    }
                    break;
            }
            switch (position)
            {
                case PlayerPosition.GK:
                case PlayerPosition.LB:
                case PlayerPosition.RB:
                case PlayerPosition.CD:
                case PlayerPosition.DM:
                    break;
                case PlayerPosition.LWB:
                case PlayerPosition.RWB:
                case PlayerPosition.LM:
                case PlayerPosition.RM:
                case PlayerPosition.AM:
                case PlayerPosition.LW:
                case PlayerPosition.RW:
                    if (player.Acceleration < 99)
                    {
                        return TrainingSchedulePreset.SprintingAllWeek;
                    }
                    break;
                default:
                    if (player.Acceleration < 99)
                    {
                        return ImproveAccelerationWithHeading(player);
                    }
                    break;
            }
            return TrainingSchedulePreset.TrainingMatchAllWeek;

        }

        private static TrainingScheduleType[] ImproveSpeedTo(Player player, int stageMinimum, bool trainHeading, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Speed < stageMinimum)
            {
                if (trainingEffectModifier.RemoveNegativeTraining)
                    return TrainingSchedulePreset.SprintingAllWeek;
                if (player.Heading < stageMinimum)
                    return TrainingSchedulePreset.SprintingWithHeading;
                return TrainingSchedulePreset.SprintingAllWeek;
            }
            return null;
        }
        private static TrainingScheduleType[] ImproveAgilityTo(Player player, int stageMinimum)
        {
            if (player.Agility < stageMinimum)
            {
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            }
            return null;
        }
        private static TrainingScheduleType[] ImproveGKAgilityTo(Player player, int stageMinimum, bool noAlternativeTraining)
        {
            if (player.Agility < stageMinimum)
            {
                if (noAlternativeTraining)
                {
                    return TrainingSchedulePreset.GoalkeepingAllWeek; ;
                }
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            }
            return null;
        }
        private static TrainingScheduleType[] ImproveAccelerationTo(Player player, int stageMinimum, bool trainHeading, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Acceleration < stageMinimum)
            {
                if (trainingEffectModifier.RemoveNegativeTraining)
                    return TrainingSchedulePreset.SprintingAllWeek;
                if (player.Heading < stageMinimum)
                    return TrainingSchedulePreset.SprintingWithHeading;
                return TrainingSchedulePreset.SprintingAllWeek;
            }
            return null;
        }
        private static TrainingScheduleType[] ImproveStrength(TrainingEffectModifier trainingEffectModifier)
        {
            if (trainingEffectModifier.RemoveNegativeTraining)
                return TrainingSchedulePreset.WeightTrainingAllWeek;
            return TrainingSchedulePreset.ImproveStrength;
        }

        private static TrainingScheduleType[] ImproveShootingTo(Player player, int stageMinimum)
        {
            if (player.Shooting < stageMinimum)
            {
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            }
            return null;
        }
        private static TrainingScheduleType[] ImprovePassingTo(Player player, int stageMinimum, bool noAlternativeTraining)
        {
            if (player.Passing < stageMinimum)
            {
                if (noAlternativeTraining)
                    return TrainingSchedulePreset.FiveASideAllWeek;
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            }
            return null;
        }

        private static TrainingScheduleType[] ImproveHeadingTo(Player player, int stageMinimum, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Heading < stageMinimum)
            {
                if (trainingEffectModifier.RemoveNegativeTraining)
                    return TrainingSchedulePreset.HeadingAllWeek;
                return TrainingSchedulePreset.ImproveHeading;
            }
            return null;
        }

        private static TrainingScheduleType[] ImproveControlTo(Player player, int stageMinimum)
        {
            if (player.Control < stageMinimum) return TrainingSchedulePreset.ControlAllWeek;
            return null;
        }
        private static TrainingScheduleType[] ImproveDribbleTo(Player player, int stageMinimum)
        {
            if (player.Dribbling < stageMinimum) return TrainingSchedulePreset.ControlAllWeek;
            return null;
        }
        private static TrainingScheduleType[] ImproveTackleTo(Player player, int stageMinimum,
            TrainingEffectModifier trainingEffectModifier)
        {
            if (player.TackleDetermination < stageMinimum
                || player.TackleSkill < stageMinimum)
            {
                return ImproveTackle(player, trainingEffectModifier);
            }
            return null;
        }
        private static TrainingScheduleType[] ImproveTackleSkillTo(Player player, int stageMinimum,
            TrainingEffectModifier trainingEffectModifier)
        {
            if (player.TackleSkill < stageMinimum)
            {
                    if (trainingEffectModifier.RemoveNegativeTraining)
                        return TrainingSchedulePreset.ImproveTacklingSkillAllWeek;
                    return TrainingSchedulePreset.ImproveTacklingSkill;
                }
            return null;
        }

        private static TrainingScheduleType[] ImproveTackle(Player player, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.TackleDetermination < player.TackleSkill)
            {
                if (trainingEffectModifier.RemoveNegativeTraining)
                    return TrainingSchedulePreset.MarkingAllWeek;
                return TrainingSchedulePreset.ImproveMarking;
            }
            else if (player.TackleDetermination == player.TackleSkill)
            {
                if (trainingEffectModifier.RemoveNegativeTraining)
                    return TrainingSchedulePreset.ImproveTacklingBalanced;
                return TrainingSchedulePreset.ImproveTacklingBalancedWithTrainingMatch;
            }
            else
            {
                if (trainingEffectModifier.RemoveNegativeTraining)
                    return TrainingSchedulePreset.ImproveTacklingSkillAllWeek;
                return TrainingSchedulePreset.ImproveTacklingSkill;
            }
        }
        private static TrainingScheduleType[] ImproveCoolnessTo(Player player, int stageMinimum)
        {
            if (player.Coolness < stageMinimum) return TrainingSchedulePreset.ControlAllWeek;
            return null;
        }
        private static TrainingScheduleType[] ImproveAwarenessTo(Player player, int stageMinimum, bool maxPower, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Awareness < stageMinimum)
            {
                return ImproveAwareness(player, maxPower, trainingEffectModifier);
            }
            return null;
        }
        private static TrainingScheduleType[] ImproveFlairTo(Player player, int stageMinimum)
        {
            if (player.Flair < stageMinimum) return TrainingSchedulePreset.FiveASideAllWeek;
            return null;
        }
        private static TrainingScheduleType[] ImproveKickingTo(Player player, int stageMinimum, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Kicking < stageMinimum)
            {
                if (trainingEffectModifier.RemoveNegativeTraining)
                    return TrainingSchedulePreset.KickingAllWeek;
                return TrainingSchedulePreset.ImproveKicking;
            }
            return null;
        }
        private static TrainingScheduleType[] ImproveThrowingTo(Player player, int stageMinimum)
        {
            if (player.Throwing < stageMinimum)
            {
                return TrainingSchedulePreset.ImproveThrowing;
            }
            return null;
        }
        private static TrainingScheduleType[] ImproveHandlingTo(Player player, int stageMinimum, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Handling < stageMinimum)
            {
                if (trainingEffectModifier.RemoveNegativeTraining)
                    return TrainingSchedulePreset.HandlingAllWeek;
                return TrainingSchedulePreset.ImproveHandling;
            }
            return null;
        }
        private static TrainingScheduleType[] ImproveLeadershipTo(Player player, int stageMinimum, bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Leadership < stageMinimum && trainingEffectModifier.PassingTrainLeadership)
            {
                if (noAlternativeTraining)
                    return TrainingSchedulePreset.FiveASideAllWeek;
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            }
            return null;
        }



        private static TrainingScheduleType[] ImproveConsistencyTo(Player player, int stageMinimum)
        {
            if (player.Consistency < stageMinimum) return TrainingSchedulePreset.ControlAllWeek;
            return null;
        }

        private static TrainingScheduleType[] ImproveDeterminationTo(Player player, int stageMinimum,
            TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Speed < stageMinimum)
            {
                if (trainingEffectModifier.RemoveNegativeTraining)
                    return TrainingSchedulePreset.WeightTrainingAllWeek;
                return TrainingSchedulePreset.ImproveDetermination;
            }
            return null;
        }
    }
}