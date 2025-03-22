using FSM97Lib;
using System.Linq;

namespace Fsm97Trainer
{

    public class TrainingSchedule
    {
        static int[] stages = new int[] { 24, 29, 34, 39, 44, 49, 54, 59, 64, 69, 74, 79, 84, 89, 94, 99 };
        public static TrainingScheduleType[] GetTrainingSchedule(Player player, bool autoResetStatus, bool maxEnergy,
            bool maxPower, bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier)
        {
            TrainingScheduleType[] schedule;
            //is this a player a different best position only training as goalkeeper to avoid injury?
            if (player.Fitness < 99 && player.Position == (byte)PlayerPosition.GK && player.BestPosition != (byte)PlayerPosition.GK)
            {
                //train for the player's real position
                schedule = (TrainingScheduleType[])GetTrainingSchedule(player, (PlayerPosition)player.BestPosition, maxPower, noAlternativeTraining, trainingEffectModifier).Clone();
            }
            else
            {
                //train for the game player's preferred position
                schedule = (TrainingScheduleType[])GetTrainingSchedule(player, (PlayerPosition)player.Position, maxPower, noAlternativeTraining, trainingEffectModifier).Clone();
            }
            //see physiotherapist if injuries will not be auto reset
            //training injury happens due to low energy
            if (!autoResetStatus && !maxEnergy && !trainingEffectModifier.RemoveNegativeTraining)
            {
                //Only for on-field players
                //gks almost never get injured so exclude them
                //fitness < 99 means can be injured in training.
                //fitness = 99 can still injured in training once in a blue moon but we ignore that. 
                if (player.Status == 0 && player.Position != (byte)PlayerPosition.GK && player.Fitness < 99)
                {
                    schedule[1] = schedule[3] = schedule[5] = TrainingScheduleType.Physiotherapist;
                }
            }
            return schedule;
        }
        public static TrainingScheduleType[] GetTrainingSchedule(Player player, PlayerPosition position, bool maxPower,
            bool noAlternativeTraining,
            TrainingEffectModifier trainingEffectModifier)
        {
            if (!noAlternativeTraining)
                return GenericTraining(player, maxPower
                    , trainingEffectModifier);
            switch (position)
            {
                case PlayerPosition.GK: return GetGKTrainingSchedule(player, maxPower, trainingEffectModifier);
                case PlayerPosition.LB:
                case PlayerPosition.RB: return GetLRBTrainingSchedule(player, maxPower, trainingEffectModifier);
                case PlayerPosition.CD: return GetCDTrainingSchedule(player, maxPower, trainingEffectModifier);
                case PlayerPosition.DM: return GetDMTrainingSchedule(player, maxPower, trainingEffectModifier);
                case PlayerPosition.SW: return GetSWTrainingSchedule(player, maxPower, trainingEffectModifier);

                case PlayerPosition.LWB:
                case PlayerPosition.RWB:
                    return GetLRWBTrainingSchedule(player, maxPower, trainingEffectModifier);
                case PlayerPosition.LM:
                case PlayerPosition.RM:
                case PlayerPosition.AM:
                    return GetLRAMTrainingSchedule(player, position, maxPower, trainingEffectModifier);
                case PlayerPosition.LW:
                case PlayerPosition.RW: return GetLRWTrainingSchedule(player, maxPower, trainingEffectModifier);
                case PlayerPosition.FR: return GetFRTrainingSchedule(player, maxPower, trainingEffectModifier);
                case PlayerPosition.SS: return GetFORSSTrainingSchedule(player, position, maxPower, trainingEffectModifier);
                case PlayerPosition.FOR: return GetFORSSTrainingSchedule(player, position, maxPower, trainingEffectModifier);
                default: return TrainingSchedulePreset.None;
            }
        }
        private static TrainingScheduleType[] GetGKTrainingSchedule(Player player, bool maxPower
            , TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < player.Handling)
            {
                return ImproveFitness(player, trainingEffectModifier);
            }
            foreach (var stage in stages)
            {
                if (player.Agility < stage || player.Handling < stage || player.Kicking < stage || player.Throwing < stage ||
                    player.Consistency < stage || player.Control < stage || player.Passing < stage || player.Speed < stage)
                {
                    return GetGKTrainingScheduleStage(player, maxPower
                    , trainingEffectModifier, stage);
                }
            }
            return TrainingSchedulePreset.None;
        }

        private static TrainingScheduleType[] GetGKTrainingScheduleStage(Player player,
            bool maxPower, TrainingEffectModifier trainingEffectModifier, int stageMinimum)
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
            result = ImprovePassingTo(player, stageMinimum);
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

            return TrainingSchedulePreset.None;
        }

        private static TrainingScheduleType[] GetLRBTrainingSchedule(Player player, bool maxPower
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
                    return GetLRBTrainingScheduleStage(player, maxPower
                    , trainingEffectModifier, stage);
                }
            }
            return TrainingSchedulePreset.None;
        }

        private static TrainingScheduleType[] GetLRBTrainingScheduleStage(Player player, bool maxPower,
            TrainingEffectModifier trainingEffectModifier, int stageMinimum)
        {
            TrainingScheduleType[] result;

            result = ImproveDeterminationTo(player, stageMinimum, trainingEffectModifier);
            if (result != null) return result;

            if (stageMinimum < 99)
            {
                result = ImproveAwarenessTo(player, stageMinimum, maxPower, trainingEffectModifier);
                if (result != null) return result;

                result = ImprovePassingTo(player, stageMinimum);
                if (result != null) return result;

                result = ImproveConsistencyTo(player, stageMinimum);
                if (result != null) return result;
            }
            if (!trainingEffectModifier.RemoveNegativeTraining && stageMinimum == 99)
            {
                result = ImproveSpeedTo(player, stageMinimum, true, trainingEffectModifier);
                if (result != null) return result;
            }

            result = ImprovePassingTo(player, stageMinimum);
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

            return TrainingSchedulePreset.None;
        }

        private static TrainingScheduleType[] GetCDTrainingSchedule(Player player, bool maxPower
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
                    return GetCDTrainingScheduleStage(player, maxPower
                    , trainingEffectModifier, stage);
                }
            }
            return TrainingSchedulePreset.None;
        }

        private static TrainingScheduleType[] GetCDTrainingScheduleStage(Player player, bool maxPower, TrainingEffectModifier trainingEffectModifier, int stageMinimum)
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
            result = ImprovePassingTo(player, stageMinimum);
            if (result != null) return result;

            result = ImproveHeadingTo(player, stageMinimum, trainingEffectModifier);
            if (result != null) return result;

            result = ImproveLeadershipTo(player, stageMinimum, trainingEffectModifier);
            if (result != null) return result;

            result = ImproveTackleTo(player, stageMinimum, trainingEffectModifier);
            if (result != null) return result;

            result = ImproveConsistencyTo(player, stageMinimum);
            if (result != null) return result;

            result = ImproveCoolnessTo(player, stageMinimum);
            if (result != null) return result;

            result = ImproveSpeedTo(player, stageMinimum, true, trainingEffectModifier);
            if (result != null) return result;

            return TrainingSchedulePreset.None;
        }
        private static TrainingScheduleType[] GetLRWBTrainingSchedule(Player player, bool maxPower
            , TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, trainingEffectModifier);
            }
            foreach (var stage in stages)
            {
                if (player.Speed < stage || player.Agility < stage || player.Acceleration < stage || player.Passing < stage ||
                player.Dribbling < stage || player.TackleDetermination < stage ||
                player.TackleSkill < stage || player.Awareness < stage || player.Flair < stage)
                {
                    return GetLRWBTrainingScheduleStage(player, maxPower
                    , trainingEffectModifier, stage);
                }
            }
            return TrainingSchedulePreset.None;
        }

        private static TrainingScheduleType[] GetLRWBTrainingScheduleStage(Player player, bool maxPower, TrainingEffectModifier trainingEffectModifier, int stageMinimum)
        {
            TrainingScheduleType[] result;
            if (stageMinimum < 99)
            {
                result = ImproveDribbleTo(player, stageMinimum);
                if (result != null) return result;

                result = ImproveTackleTo(player, stageMinimum, trainingEffectModifier);
                if (result != null) return result;

                result = ImprovePassingTo(player, stageMinimum);
                if (result != null) return result;
            }
            if (!trainingEffectModifier.RemoveNegativeTraining && stageMinimum == 99)
            {
                result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier);
                if (result != null) return result;

                result = ImproveAccelerationTo(player, stageMinimum, false, trainingEffectModifier);
                if (result != null) return result;
            }
            result = ImproveAgilityTo(player, stageMinimum);
            if (result != null) return result;
            result = ImprovePassingTo(player, stageMinimum);
            if (result != null) return result;
            result = ImproveDribbleTo(player, stageMinimum);
            if (result != null) return result;
            result = ImproveFlairTo(player, stageMinimum);
            if (result != null) return result;
            result = ImproveTackleTo(player, stageMinimum, trainingEffectModifier);
            if (result != null) return result;
            result = ImproveAwarenessTo(player, stageMinimum, maxPower, trainingEffectModifier);
            if (result != null) return result;
            result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier);
            if (result != null) return result;
            result = ImproveAccelerationTo(player, stageMinimum, false, trainingEffectModifier);
            if (result != null) return result;

            return TrainingSchedulePreset.None;
        }


        private static TrainingScheduleType[] GetSWTrainingSchedule(Player player, bool maxPower, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, trainingEffectModifier);
            }
            foreach (var stage in stages)
            {
                if (player.Speed < stage || player.Acceleration < stage || player.Passing < stage ||
                     player.Heading < stage || player.Dribbling < stage ||
               player.TackleDetermination < stage || player.TackleSkill < stage || player.Awareness < stage)
                {
                    return GetSWTrainingScheduleStage(player, maxPower
                    , trainingEffectModifier, stage);
                }
            }

            return TrainingSchedulePreset.None;
        }

        private static TrainingScheduleType[] GetSWTrainingScheduleStage(Player player, bool maxPower,
             TrainingEffectModifier trainingEffectModifier, int stageMinimum)
        {
            TrainingScheduleType[] result;
            if (stageMinimum < 99)
            {
                result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier);
                if (result != null) return result;
                result = ImproveAwarenessTo(player, stageMinimum, maxPower, trainingEffectModifier);
                if (result != null) return result;
                result = ImproveTackleTo(player, stageMinimum, trainingEffectModifier);
                if (result != null) return result;
                result = ImproveDribbleTo(player, stageMinimum);
                if (result != null) return result;
                result = ImproveAccelerationTo(player, stageMinimum, true, trainingEffectModifier);
                if (result != null) return result;
                result = ImprovePassingTo(player, stageMinimum);
                if (result != null) return result;
            }
            if (!trainingEffectModifier.RemoveNegativeTraining && stageMinimum == 99)
            {
                result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier);
                if (result != null) return result;

                result = ImproveAccelerationTo(player, stageMinimum, true, trainingEffectModifier);
                if (result != null) return result;
            }

            result = ImprovePassingTo(player, stageMinimum);
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

            return TrainingSchedulePreset.None;
        }

        private static TrainingScheduleType[] GetDMTrainingSchedule(Player player, bool maxPower, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, trainingEffectModifier);
            }
            foreach (var stage in stages)
            {
                if (player.Speed < stage || player.Passing < stage || player.Heading < stage ||
               player.TackleDetermination < stage || player.TackleSkill < stage || player.Awareness < stage)
                {
                    return GetDMTrainingScheduleStage(player, maxPower
                    , trainingEffectModifier, stage);
                }
            }
            return TrainingSchedulePreset.None;
        }

        private static TrainingScheduleType[] GetDMTrainingScheduleStage(Player player, bool maxPower,
            TrainingEffectModifier trainingEffectModifier, int stageMinimum)
        {
            TrainingScheduleType[] result;
            if (stageMinimum < 99)
            {
                result = ImprovePassingTo(player, stageMinimum);
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
            result = ImprovePassingTo(player, stageMinimum);
            if (result != null) return result;

            result = ImproveHeadingTo(player, stageMinimum, trainingEffectModifier);
            if (result != null) return result;

            result = ImproveTackleTo(player, stageMinimum, trainingEffectModifier);
            if (result != null) return result;
            result = ImproveAwarenessTo(player, stageMinimum, maxPower, trainingEffectModifier);
            if (result != null) return result;
            result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier);
            if (result != null) return result;

            return TrainingSchedulePreset.None;
        }

        private static TrainingScheduleType[] GetLRAMTrainingSchedule(Player player,
            PlayerPosition position, bool maxPower, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, trainingEffectModifier);
            }
            foreach (var stage in stages)
            {
                if (player.Speed < stage || player.Acceleration < stage || player.Shooting < stage || player.Passing < stage
                    || player.Control < stage || player.Dribbling < stage || player.TackleSkill < stage
                    || player.Awareness < stage || player.Flair < stage)
                {
                    return GetLRAMTrainingScheduleStage(player, position, maxPower
                    , trainingEffectModifier, stage);
                }
            }
            return TrainingSchedulePreset.None;
        }

        private static TrainingScheduleType[] GetLRAMTrainingScheduleStage(Player player, PlayerPosition position,
            bool maxPower, TrainingEffectModifier trainingEffectModifier, int stageMinimum)
        {
            TrainingScheduleType[] result;
            if (stageMinimum < 99)
            {
                if (position == PlayerPosition.AM)
                {
                    result = ImprovePassingTo(player, stageMinimum);
                    if (result != null) return result;
                    result = ImproveTackleSkillTo(player, stageMinimum - 1, trainingEffectModifier);
                    if (result != null) return result;

                }
                else
                {
                    result = ImproveTackleSkillTo(player, stageMinimum, trainingEffectModifier);
                    if (result != null) return result;
                    result = ImprovePassingTo(player, stageMinimum - 1);
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
                    result = ImprovePassingTo(player, stageMinimum);
                    if (result != null) return result;
                    result = ImproveTackleSkillTo(player, stageMinimum, trainingEffectModifier);
                    if (result != null) return result;

                }
                else
                {
                    result = ImproveTackleSkillTo(player, stageMinimum, trainingEffectModifier);
                    if (result != null) return result;
                    result = ImprovePassingTo(player, stageMinimum);
                    if (result != null) return result;
                }
            }
            else
            {
                result = ImprovePassingTo(player, stageMinimum);
                if (result != null) return result;
                result = ImproveTackleSkillTo(player, stageMinimum, trainingEffectModifier);
                if (result != null) return result;
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
            return TrainingSchedulePreset.None;
        }
        private static TrainingScheduleType[] GetLRWTrainingSchedule(Player player, bool maxPower, TrainingEffectModifier trainingEffectModifier)
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
                    return GetLRWTrainingScheduleStage(player, maxPower
                    , trainingEffectModifier, stage);
                }
            }
            return TrainingSchedulePreset.None;
        }

        private static TrainingScheduleType[] GetLRWTrainingScheduleStage(Player player, bool maxPower,
             TrainingEffectModifier trainingEffectModifier, int stageMinimum)
        {
            TrainingScheduleType[] result;
            if (stageMinimum < 99)
            {
                result = ImprovePassingTo(player, stageMinimum);
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
            result = ImprovePassingTo(player, stageMinimum);
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
            return TrainingSchedulePreset.None;
        }

        private static TrainingScheduleType[] GetFRTrainingSchedule(Player player, bool maxPower, TrainingEffectModifier trainingEffectModifier)
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
                    return GetFRTrainingScheduleStage(player, maxPower
                    , trainingEffectModifier, stage);
                }
            }
            return TrainingSchedulePreset.None;
        }

        private static TrainingScheduleType[] GetFRTrainingScheduleStage(Player player, bool maxPower,
            TrainingEffectModifier trainingEffectModifier, int stageMinimum)
        {
            TrainingScheduleType[] result;
            if (stageMinimum < 99)
            {
                result = ImproveDribbleTo(player, stageMinimum);
                if (result != null) return result;
                result = ImproveSpeedTo(player, stageMinimum, true, trainingEffectModifier);
                if (result != null) return result;
                result = ImprovePassingTo(player, stageMinimum);
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
            result = ImprovePassingTo(player, stageMinimum);
            if (result != null) return result;
            result = ImproveHeadingTo(player, stageMinimum, trainingEffectModifier);
            if (result != null) return result;
            result = ImproveDribbleTo(player, stageMinimum);
            if (result != null) return result;
            result = ImproveControlTo(player, stageMinimum);
            if (result != null) return result;
            result = ImproveFlairTo(player, stageMinimum);
            if (result != null) return result;
            result = ImproveAwarenessTo(player, stageMinimum, maxPower, trainingEffectModifier);
            if (result != null) return result;
            return TrainingSchedulePreset.None;
        }

        private static TrainingScheduleType[] GetFORSSTrainingSchedule(Player player, PlayerPosition position, bool maxPower, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, trainingEffectModifier);
            }
            foreach (var stage in stages)
            {
                if (player.Speed < stage || player.Agility < stage || player.Acceleration < stage
                    || player.Shooting < stage || player.Passing < stage || player.Heading < stage
                    || player.Control < stage || player.Dribbling < stage || player.Coolness < stage
                    || player.Awareness < stage || player.Flair < stage)
                {
                    return GetFORSSTrainingScheduleStage(player, position, maxPower
                    , trainingEffectModifier, stage);
                }
            }
            return TrainingSchedulePreset.None;
        }

        private static TrainingScheduleType[] GetFORSSTrainingScheduleStage(Player player, PlayerPosition position, bool maxPower, TrainingEffectModifier trainingEffectModifier, int stageMinimum)
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
                else
                {
                    result = ImprovePassingTo(player, stageMinimum);
                    if (result != null) return result;
                    result = ImproveControlTo(player, stageMinimum);
                    if (result != null) return result;
                    result = ImproveFlairTo(player, stageMinimum);
                    if (result != null) return result;
                }
            }
            if (!trainingEffectModifier.RemoveNegativeTraining && stageMinimum == 99)
            {
                result = ImproveSpeedTo(player, stageMinimum, true, trainingEffectModifier);
                if (result != null) return result;
                result = ImproveAccelerationTo(player, stageMinimum, true, trainingEffectModifier);
                if (result != null) return result;
            }
            result = ImproveAgilityTo(player, stageMinimum);
            if (result != null) return result;
            result = ImproveShootingTo(player, stageMinimum);
            if (result != null) return result;
            result = ImprovePassingTo(player, stageMinimum);
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

            result = ImproveSpeedTo(player, stageMinimum, true, trainingEffectModifier);
            if (result != null) return result;
            result = ImproveAccelerationTo(player, stageMinimum, true, trainingEffectModifier);
            if (result != null) return result;


            return TrainingSchedulePreset.None;
        }
        private static TrainingScheduleType[] GenericTraining(Player player, bool maxPower, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, trainingEffectModifier);
            }
            int bestScheduleType = -1;
            double bestScheduleEffect = 0;
            bool isPlayerGK = (player.Position == (int)PlayerPosition.GK);

            for (int i = 0; i < (int)TrainingScheduleType.Count; i++)
            {
                // Skip GK specific training if not a GK
                if (!isPlayerGK && (i >= (int)TrainingScheduleType.Handling
                    && i <= (int)TrainingScheduleType.Throwing))
                    continue;
                double scheduleEffect = TrainingScheduleEffect.GetScheduleEffect(i, player.Attributes, trainingEffectModifier);
                if (scheduleEffect > bestScheduleEffect)
                {
                    bestScheduleType = i;
                    bestScheduleEffect = scheduleEffect;
                }
            }
            if (bestScheduleType == -1)
            {
                if (!isPlayerGK)
                {
                    //nothing else to train except GK
                    bestScheduleEffect = 0;
                    for (int i = (int)TrainingScheduleType.Handling; i < (int)TrainingScheduleType.Throwing; i++)
                    {                        
                        double scheduleEffect = TrainingScheduleEffect.GetScheduleEffect(i, player.Attributes, trainingEffectModifier);
                        if (scheduleEffect > bestScheduleEffect)
                        {
                            bestScheduleType = i;
                            bestScheduleEffect = scheduleEffect;
                        }
                    }
                    if (bestScheduleType == -1)
                        return TrainingSchedulePreset.MaintainShape;
                }
                else
                {
                    //nothing else to train except GK
                    return TrainingSchedulePreset.MaintainShape;
                }
            }
            return Enumerable.Repeat<TrainingScheduleType>((TrainingScheduleType)bestScheduleType, 7).ToArray();
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
        private static TrainingScheduleType[] ImproveGKAgilityTo(Player player, int stageMinimum)
        {
            if (player.Agility < stageMinimum)
            {
                return TrainingSchedulePreset.GoalkeepingAllWeek; ;

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
        private static TrainingScheduleType[] ImproveStrengthTo(Player player, int stageMinimum, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Strength < stageMinimum)
            {
                if (trainingEffectModifier.RemoveNegativeTraining)
                    return TrainingSchedulePreset.WeightTrainingAllWeek;
                return TrainingSchedulePreset.ImproveStrength;
            }
            return null;
        }

        private static TrainingScheduleType[] ImproveShootingTo(Player player, int stageMinimum)
        {
            if (player.Shooting < stageMinimum)
            {
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            }
            return null;
        }
        private static TrainingScheduleType[] ImprovePassingTo(Player player, int stageMinimum)
        {
            if (player.Passing < stageMinimum)
            {
                return TrainingSchedulePreset.FiveASideAllWeek;
            }
            return null;
        }

        private static TrainingScheduleType[] ImproveHeadingTo(Player player, int stageMinimum, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Heading < stageMinimum)
            {
                if (trainingEffectModifier.RemoveNegativeTraining)
                {
                    if (player.Shooting == 99
                        && player.Passing == 99
                        && player.Control == 99
                        && player.Dribbling == 99)
                    {
                        if (player.TackleDetermination < 99 || player.TackleSkill < 99)
                            return TrainingSchedulePreset.TrainingMatchAllWeek;
                        if (player.Leadership < 99 && trainingEffectModifier.PassingTrainLeadership)
                            return TrainingSchedulePreset.TrainingMatchAllWeek;
                        if (player.Greed < 99 && trainingEffectModifier.ShootingTrainGreed)
                            return TrainingSchedulePreset.TrainingMatchAllWeek;
                        if (player.ThrowIn < 99 && trainingEffectModifier.ThrowingTrainThrowIn)
                            return TrainingSchedulePreset.TrainingMatchAllWeek;
                    }
                    return TrainingSchedulePreset.HeadingAllWeek;
                }
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
        private static TrainingScheduleType[] ImproveThrowInTo(Player player, int stageMinimum, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.ThrowIn < stageMinimum)
            {

                if (trainingEffectModifier.ThrowingTrainThrowIn)
                {
                    if (player.TackleDetermination < 99 || player.TackleSkill < 99)
                        return TrainingSchedulePreset.TrainingMatchAllWeek;
                    if (player.Leadership < 99 && trainingEffectModifier.PassingTrainLeadership)
                        return TrainingSchedulePreset.TrainingMatchAllWeek;
                    return TrainingSchedulePreset.ImproveThrowing;
                }
            }
            return null;
        }
        private static TrainingScheduleType[] ImproveLeadershipTo(Player player, int stageMinimum, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Leadership < stageMinimum && trainingEffectModifier.PassingTrainLeadership)
            {
                return TrainingSchedulePreset.FiveASideAllWeek;
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
            if (player.Determination < stageMinimum)
            {
                if (trainingEffectModifier.RemoveNegativeTraining)
                    return TrainingSchedulePreset.WeightTrainingAllWeek;
                return TrainingSchedulePreset.ImproveDetermination;
            }
            return null;
        }
        private static TrainingScheduleType[] ImproveGreed(Player player, int stageMinimum, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Greed < stageMinimum && trainingEffectModifier.ShootingTrainGreed)
            {
                if (player.TackleDetermination < 99 || player.TackleSkill < 99)
                    return TrainingSchedulePreset.TrainingMatchAllWeek;
                if (player.Leadership < 99 && trainingEffectModifier.PassingTrainLeadership)
                    return TrainingSchedulePreset.TrainingMatchAllWeek;
                return TrainingSchedulePreset.ShootingAllWeek;
            }
            return null;
        }
    }
}