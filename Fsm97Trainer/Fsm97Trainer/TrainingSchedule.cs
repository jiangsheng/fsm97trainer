using FSM97Lib;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;

namespace Fsm97Trainer
{

    public static class TrainingSchedule
    {
        static int[] stages =
            Enumerable.Range(1,75).Select(x => x+24 ).ToArray();
        public static TrainingScheduleType[] GetTrainingSchedule(Player player, bool autoResetStatus, bool maxEnergy,
            bool maxPower, bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier, bool debugTraining)
        {
            TrainingScheduleType[] schedule;
            //is this a player a different best position only training as goalkeeper to avoid injury?
            if (player.Fitness < 99 && player.Position == (byte)PlayerPosition.GK
                && player.BestPosition != (byte)PlayerPosition.GK)
            {
                if (debugTraining)
                {
                    //Debug.WriteLine($"Player {player} is training as GK to avoid injury. Best position is {(PlayerPosition)player.BestPosition}");
                }
                //train for the player's real position
                schedule = (TrainingScheduleType[])GetTrainingSchedule(player, (PlayerPosition)player.BestPosition, maxPower, noAlternativeTraining, trainingEffectModifier, debugTraining).Clone();
            }
            else
            {
                //train for the game player's preferred position
                schedule = (TrainingScheduleType[])GetTrainingSchedule(player, (PlayerPosition)player.Position, maxPower, noAlternativeTraining, trainingEffectModifier, debugTraining).Clone();
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
            TrainingEffectModifier trainingEffectModifier, bool debugTraining)
        {
            if (!noAlternativeTraining)
            {
                //if (debugTraining) Debug.WriteLine($"Player {player} is using generic training.");
                return GenericTraining(player, position, maxPower
                    , trainingEffectModifier);
            }

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
            if (player.Fitness < player.Handling) return ImproveFitness(player, false,false,trainingEffectModifier);
            foreach (var stage in stages)
            {
                if (player.Agility < stage || player.Handling < stage || player.Kicking < stage || player.Throwing < stage || player.Coolness < stage || player.Awareness < stage ||

                    player.Consistency < stage || player.Control < stage || player.Passing < stage || player.Speed < stage)
                {
                    return GetGKTrainingScheduleStage(player, maxPower
                    , trainingEffectModifier, stage);
                }
            }
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.GK, player.Attributes) == 99);
            return TrainingSchedulePreset.None;
        }

        private static TrainingScheduleType[] GetGKTrainingScheduleStage(Player player,
            bool maxPower, TrainingEffectModifier trainingEffectModifier, int stageMinimum)
        {
            TrainingScheduleType[] result;

            result = ImproveHandlingTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;
            result = ImproveKickingTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;
            result = ImproveThrowingTo(player, stageMinimum); if (result != null) return result;
            if (stageMinimum < 99)
            {
                result = ImproveConsistencyTo(player, stageMinimum); if (result != null) return result;

                result = ImproveCoolnessAndAwarenessTo(player, stageMinimum, maxPower, trainingEffectModifier); if (result != null) return result;
            }
            if (!trainingEffectModifier.RemoveNegativeTraining)
            {
                result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier);
                if (result != null) return result;
            }

            result = ImprovePassingTo(player, stageMinimum); if (result != null) return result;

            result = ImproveConsistencyTo(player, stageMinimum); if (result != null) return result;

            result = ImproveCoolnessAndAwarenessTo(player, stageMinimum, maxPower, trainingEffectModifier); if (result != null) return result;

            result = ImproveControlTo(player, stageMinimum); if (result != null) return result;


            result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier); if (result != null) return result;


            result = ImproveGKAgilityTo(player, stageMinimum); if (result != null) return result;

            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.GK, player.Attributes) == 99);
            return TrainingSchedulePreset.None;
        }

        private static TrainingScheduleType[] GetLRBTrainingSchedule(Player player, bool maxPower
             , TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99) return ImproveFitness(player, true,true,trainingEffectModifier);
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
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.RB, player.Attributes) >= 97);
            return TrainingSchedulePreset.None;
        }

        private static TrainingScheduleType[] GetLRBTrainingScheduleStage(Player player, bool maxPower,
            TrainingEffectModifier trainingEffectModifier, int stageMinimum)
        {
            TrainingScheduleType[] result;

            result = ImproveDeterminationTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;

            if (stageMinimum < 99)
            {
                result = ImproveCoolnessAndAwarenessTo(player, stageMinimum, maxPower, trainingEffectModifier); if (result != null) return result;

                result = ImprovePassingTo(player, stageMinimum); if (result != null) return result;

                result = ImproveConsistencyTo(player, stageMinimum); if (result != null) return result;
            }
            if (!trainingEffectModifier.RemoveNegativeTraining && stageMinimum == 99)
            {
                result = ImproveSpeedTo(player, stageMinimum, true, trainingEffectModifier); if (result != null) return result;
            }

            result = ImprovePassingTo(player, stageMinimum); if (result != null) return result;

            result = ImproveHeadingTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;

            result = ImproveTackleDeterminationAndSkillTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;

            result = ImproveConsistencyTo(player, stageMinimum); if (result != null) return result;


            result = ImproveCoolnessAndAwarenessTo(player, stageMinimum, maxPower, trainingEffectModifier); if (result != null) return result;

            result = ImproveSpeedTo(player, stageMinimum, true, trainingEffectModifier); if (result != null) return result;
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.RB, player.Attributes) == 99);
            return TrainingSchedulePreset.None;
        }

        private static TrainingScheduleType[] GetCDTrainingSchedule(Player player, bool maxPower
            , TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99) return ImproveFitness(player, false,true,trainingEffectModifier);
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
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.CD, player.Attributes) >= 96);
            return TrainingSchedulePreset.None;
        }

        private static TrainingScheduleType[] GetCDTrainingScheduleStage(Player player, bool maxPower, TrainingEffectModifier trainingEffectModifier, int stageMinimum)
        {
            TrainingScheduleType[] result;
            if (stageMinimum < 99)
            {
                result = ImproveTackleDeterminationAndSkillTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;

                result = ImproveHeadingTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;
            }
            if (!trainingEffectModifier.RemoveNegativeTraining && stageMinimum == 99)
            {
                result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier); if (result != null) return result;
            }
            result = ImprovePassingTo(player, stageMinimum);
            if (result != null) return result;


            result = ImproveHeadingTo(player, stageMinimum, trainingEffectModifier);
            if (result != null) return result;

            result = ImproveLeadershipTo(player, stageMinimum, trainingEffectModifier);
            if (result != null) return result;

            result = ImproveTackleDeterminationAndSkillTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;

            result = ImproveConsistencyTo(player, stageMinimum);
            if (result != null) return result;


            result = ImproveCoolnessAndAwarenessTo(player, stageMinimum, maxPower, trainingEffectModifier); if (result != null) return result;

            result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier);
            if (result != null) return result;
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.CD, player.Attributes) >= 97);

            return TrainingSchedulePreset.None;
        }
        private static TrainingScheduleType[] GetLRWBTrainingSchedule(Player player, bool maxPower
            , TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, false,false,trainingEffectModifier);
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
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.RWB, player.Attributes) == 99);
            return TrainingSchedulePreset.None;
        }

        private static TrainingScheduleType[] GetLRWBTrainingScheduleStage(Player player, bool maxPower, TrainingEffectModifier trainingEffectModifier, int stageMinimum)
        {
            TrainingScheduleType[] result;
            if (stageMinimum < 99)
            {
                result = ImproveDribbleTo(player, stageMinimum);
                if (result != null) return result;

                result = ImproveTackleDeterminationAndSkillTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;

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
            result = ImproveAwarenessAndFlairTo(player, stageMinimum, maxPower, trainingEffectModifier);
            if (result != null) return result;
            result = ImproveTackleDeterminationAndSkillTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;
            result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier);
            if (result != null) return result;
            result = ImproveAccelerationTo(player, stageMinimum, false, trainingEffectModifier);
            if (result != null) return result;
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.RWB, player.Attributes) == 99);
            return TrainingSchedulePreset.None;
        }


        private static TrainingScheduleType[] GetSWTrainingSchedule(Player player, bool maxPower, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, false, true, trainingEffectModifier);
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

            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.SW, player.Attributes) == 99);

            return TrainingSchedulePreset.None;
        }

        private static TrainingScheduleType[] GetSWTrainingScheduleStage(Player player, bool maxPower,
             TrainingEffectModifier trainingEffectModifier, int stageMinimum)
        {
            TrainingScheduleType[] result;
            if (stageMinimum < 99)
            {
                result = ImproveSpeedTo(player, stageMinimum, true, trainingEffectModifier);
                if (result != null) return result;
                result = ImproveAwarenessTo(player, stageMinimum, maxPower, trainingEffectModifier);
                if (result != null) return result;
                result = ImproveTackleDeterminationAndSkillTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;
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

            result = ImproveTackleDeterminationAndSkillTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;

            result = ImproveAwarenessTo(player, stageMinimum, maxPower, trainingEffectModifier);
            if (result != null) return result;

            result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier);
            if (result != null) return result;

            result = ImproveAccelerationTo(player, stageMinimum, true, trainingEffectModifier);
            if (result != null) return result;
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.SW, player.Attributes) == 99);
            return TrainingSchedulePreset.None;
        }

        private static TrainingScheduleType[] GetDMTrainingSchedule(Player player, bool maxPower, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, false, true, trainingEffectModifier);
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
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.DM, player.Attributes) == 99);
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
                result = ImproveTackleDeterminationAndSkillTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;
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


            result = ImproveTackleDeterminationAndSkillTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;
            result = ImproveAwarenessTo(player, stageMinimum, maxPower, trainingEffectModifier);
            if (result != null) return result;
            result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier);
            if (result != null) return result;
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.DM, player.Attributes) == 99);
            return TrainingSchedulePreset.None;
        }

        private static TrainingScheduleType[] GetLRAMTrainingSchedule(Player player,
            PlayerPosition position, bool maxPower, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, false, false, trainingEffectModifier);
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
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.RM, player.Attributes) == 99);
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
            result = ImproveShootingTo(player, stageMinimum); if (result != null) return result;
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

            result = ImproveAwarenessAndFlairTo(player, stageMinimum, maxPower, trainingEffectModifier);
            if (result != null) return result;

            result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier);
            if (result != null) return result;
            result = ImproveAccelerationTo(player, stageMinimum, false, trainingEffectModifier);
            if (result != null) return result;
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.RM, player.Attributes) == 99);
            return TrainingSchedulePreset.None;
        }
        private static TrainingScheduleType[] GetLRWTrainingSchedule(Player player, bool maxPower, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, false, false, trainingEffectModifier);
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
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.RW, player.Attributes) == 99);
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
            result = ImproveAwarenessAndFlairTo(player, stageMinimum, maxPower, trainingEffectModifier);
            if (result != null) return result;
            result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier);
            if (result != null) return result;
            result = ImproveAccelerationTo(player, stageMinimum, false, trainingEffectModifier);
            if (result != null) return result;
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.RW, player.Attributes) == 99);
            return TrainingSchedulePreset.None;
        }

        private static TrainingScheduleType[] GetFRTrainingSchedule(Player player, bool maxPower, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, false, true, trainingEffectModifier);
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
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.FR, player.Attributes) == 99);
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
                result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier);
                if (result != null) return result;
                result = ImprovePassingTo(player, stageMinimum);
                if (result != null) return result;
                result = ImproveAwarenessAndFlairTo(player, stageMinimum, maxPower, trainingEffectModifier);
                if (result != null) return result;
                result = ImproveAccelerationTo(player, stageMinimum, true, trainingEffectModifier);
                if (result != null) return result;
            }
            result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier);
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
            result = ImproveAwarenessAndFlairTo(player, stageMinimum, maxPower, trainingEffectModifier);
            if (result != null) return result;
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.FR, player.Attributes) == 99);
            return TrainingSchedulePreset.None;
        }

        private static TrainingScheduleType[] GetFORSSTrainingSchedule(Player player, PlayerPosition position, bool maxPower, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, false, true, trainingEffectModifier);
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
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.FOR, player.Attributes) == 99);
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
                    result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier);
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
                result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier);
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


            result = ImproveCoolnessAwarenessAndFlairTo(player, stageMinimum, maxPower, trainingEffectModifier);
            if (result != null) return result;

            result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier);
            if (result != null) return result;
            result = ImproveAccelerationTo(player, stageMinimum, true, trainingEffectModifier);
            if (result != null) return result;

            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.FOR, player.Attributes) == 99);
            return TrainingSchedulePreset.None;
        }
        public static TrainingScheduleType[] GenericTraining(Player player,PlayerPosition playerPosition, bool maxPower, TrainingEffectModifier trainingEffectModifier)
        {
            bool shouldTrainAsGK = player.Kicking + player.Handling + player.ThrowIn > 210;
            bool shouldTrainAsLrb = player.Determination * 2 + player.Consistency > 200;
            bool shouldTrainAsCd = player.Leadership > 66 && player.Consistency > 70;
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, shouldTrainAsLrb, true, trainingEffectModifier);
            }
            if (player.Speed < 99)
            { 
                return ImproveSpeedTo(player,99,shouldTrainAsLrb, trainingEffectModifier);
            }
            if (player.Passing < 99)
            {
                return ImprovePassingTo(player, 99);
            }
            if (shouldTrainAsLrb && player.Determination<99)
            {
                return ImproveDeterminationTo(player, 99, trainingEffectModifier);
            }
            if (shouldTrainAsCd && trainingEffectModifier.PassingTrainLeadership && player.Leadership<99)
            {
                return ImproveLeadershipTo(player,99,trainingEffectModifier);
            }
            bool avoidNegative = false;
            if (playerPosition != PlayerPosition.Count)
            {
                avoidNegative = PositionRatings.GetPositionRatingDouble((int)playerPosition, player.Attributes) >= 97;
            }

            int bestPresetIndex = -1;
            double bestScheduleEffect = 0;
            //focus on weak points
            //99-player.Attribute is used to weight the weaker attributes
            List<double> scheduleEffects = new List<double>();
            var presets = avoidNegative ? TrainingSchedulePreset.NoNegativePresets : TrainingSchedulePreset.AllPresets;
            for (int presetIndex = 0; presetIndex < presets.Count; presetIndex++)
            {
                var preset = presets[presetIndex];
                double scheduleEffect = TrainingScheduleEffect.GetScheduleEffect(preset, playerPosition,player.Attributes, trainingEffectModifier, shouldTrainAsGK, shouldTrainAsLrb, shouldTrainAsCd);
                scheduleEffects.Add(scheduleEffect);

                // Skip GK specific training if not a GK
                if (!shouldTrainAsGK && (presetIndex >= (int)TrainingScheduleType.Handling
                    && presetIndex <= (int)TrainingScheduleType.Throwing))
                {
                    continue;
                }

                if (scheduleEffect >= bestScheduleEffect)
                {
                    if (scheduleEffect > 0)
                    {
                        bestPresetIndex = presetIndex;
                        bestScheduleEffect = scheduleEffect;
                    }
                }
            }
            /*//players rarely need to improve agility
            //as extra TrainingMatch is often needed
            //to compensate skill losses from sprinting/tackling/weightlifting
            if (bestScheduleType == (int)TrainingScheduleType.Exercise)
                 bestScheduleType = (int)TrainingScheduleType.TrainingMatch;
            */
            if (bestPresetIndex == -1)
            {
                /*
                if (!shouldTrainAsGK)
                {
                    //nothing else to train except GK
                    bestScheduleEffect = 0;
                    for (int i = (int)TrainingScheduleType.Handling; i < (int)TrainingScheduleType.Throwing; i++)
                    {
                        double scheduleEffect = TrainingScheduleEffect.GetScheduleEffect(i, player.Attributes, trainingEffectModifier, true, shouldTrainAsLrb, shouldTrainAsCd);
                        if (scheduleEffect >= bestScheduleEffect)
                        {
                            if (scheduleEffect > 0)
                            {
                                bestScheduleType = i;
                                bestScheduleEffect = scheduleEffect;
                            }
                        }
                    }
                    if (bestScheduleType == -1)
                        return TrainingSchedulePreset.MaintainShape;
                }*/
                //else
                {
                    //nothing else to train except GK
                    return TrainingSchedulePreset.MaintainShape;
                }
            }
            return presets[bestPresetIndex]; 
        }

        static TrainingScheduleType[] ImproveAwarenessAndFlairTo(Player player, int stageMinimum, bool maxPower, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Awareness < stageMinimum || player.Flair < stageMinimum)
            {
                int flairLeftToTrain = 99 - player.Flair;
                int awarenessLeftToTrain = 99 - player.Awareness;
                if (flairLeftToTrain > awarenessLeftToTrain / 2)
                    return TrainingSchedulePreset.FiveASideAllWeek;
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            }
            return null;
        }
        static TrainingScheduleType[] ImproveCoolnessAndAwarenessTo(Player player, int stageMinimum, bool maxPower, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Coolness< stageMinimum || player.Awareness < stageMinimum)
            {
                int CoolnessLeftToTrain = 99 - player.Coolness;
                int awarenessLeftToTrain = 99 - player.Awareness;
                if (CoolnessLeftToTrain > awarenessLeftToTrain)
                    return TrainingSchedulePreset.ControlAllWeek;
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            }
            return null;
        }
        static TrainingScheduleType[] ImproveCoolnessAwarenessAndFlairTo(Player player, int stageMinimum, bool maxPower, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Coolness < stageMinimum || player.Awareness < stageMinimum|| player.Flair< stageMinimum)
            {
                int CoolnessLeftToTrain = 99 - player.Coolness;
                int flairLeftToTrain = 99 - player.Flair;
                int awarenessLeftToTrain = 99 - player.Awareness;
                var controlScore = CoolnessLeftToTrain * 8 + awarenessLeftToTrain * 2 + flairLeftToTrain * 4;
                var fiveASideScore = awarenessLeftToTrain * 4 + flairLeftToTrain * 8;
                var traingMatchScore = CoolnessLeftToTrain * 4 + awarenessLeftToTrain * 6 + flairLeftToTrain * 6;
                if (traingMatchScore >= fiveASideScore)
                {
                    if (controlScore >= traingMatchScore)
                        return TrainingSchedulePreset.ControlAllWeek;
                    else
                        return TrainingSchedulePreset.TrainingMatchAllWeek;
                }
                else
                {
                    if (controlScore >= fiveASideScore)
                        return TrainingSchedulePreset.ControlAllWeek;
                    else
                        return TrainingSchedulePreset.FiveASideAllWeek;

                }
            }
            return null;
        }


                
        private static TrainingScheduleType[] ImproveAwareness(Player player, bool maxPower, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Position == (int)PlayerPosition.GK)
            {
                bool shouldTrainAsGK = player.Kicking + player.Handling + player.ThrowIn > 210;
                if (shouldTrainAsGK && player.Handling < 99)
                {
                    return TrainingSchedulePreset.HandlingAllWeek;
                }
            }
            //zone is just too bad to be used
            if (maxPower)
                return TrainingSchedulePreset.ImproveAwarenessScheduleMaxPower;
            if (trainingEffectModifier.RemoveNegativeTraining)
                return TrainingSchedulePreset.ImproveAwarenessScheduleMaxPower;
            return TrainingSchedulePreset.TrainingMatchAllWeek;
        }
        private static TrainingScheduleType[] ImproveFitness(Player player,bool trainWeightLifting, bool trainHeading, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Speed < 99|| player.Acceleration < 99)
            {
                return TrainingSchedulePreset.SprintingAllWeek;
            }
            //five a side technically better for fitness
            //but most of time, training match is better overall
            return TrainingSchedulePreset.TrainingMatchAllWeek;
        }

        private static TrainingScheduleType[] ImproveSpeedTo(Player player,
            int stageMinimum, bool trainWeightLifting, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Speed < stageMinimum)
            {
                if (trainingEffectModifier.RemoveNegativeTraining)
                    return TrainingSchedulePreset.SprintingAllWeek;

                //weight training reduces speed so train it first
                if (trainWeightLifting&&player.Determination < stageMinimum &&
                    PositionRatings.Ratings[player.Position][(int)PlayerAttribute.Determination] > 0)
                    return TrainingSchedulePreset.SprintingWithWeightTraining;
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
        //unused, training match is much more useful for GKs
        private static TrainingScheduleType[] ImproveGKAgilityTo(Player player, int stageMinimum)
        {
            if (player.Agility < stageMinimum)
            {
                return TrainingSchedulePreset.GKAgility; ;

            }
            return null;
        }
        private static TrainingScheduleType[] ImproveAccelerationTo(Player player, int stageMinimum, bool trainHeading, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Acceleration < stageMinimum)
            {
                if (trainingEffectModifier.RemoveNegativeTraining) 
                    return TrainingSchedulePreset.SprintingAllWeek;
                if (player.Heading < stageMinimum && trainHeading) 
                    return TrainingSchedulePreset.SprintingWithHeading;
                return TrainingSchedulePreset.SprintingAllWeek;
            }
            return null;
        }
        //rarely useful as shooting is trained during training match
        private static TrainingScheduleType[] ImproveShootingTo(Player player, int stageMinimum)
        {
            if (player.Shooting < stageMinimum) return TrainingSchedulePreset.TrainingMatchAllWeek;
            return null;
        }
        private static TrainingScheduleType[] ImprovePassingTo(Player player, int stageMinimum)
        {
            if (player.Passing < stageMinimum) return TrainingSchedulePreset.TrainingMatchAllWeek;
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
                if(stageMinimum<99)
                    return TrainingSchedulePreset.HeadingAllWeek;
                else
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
        static TrainingScheduleType[] ImproveTackleDeterminationAndSkillTo(Player player, int stageMinimum, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.TackleSkill < stageMinimum || player.TackleDetermination < stageMinimum)
            {
                var tackleDeterminatonLeftToTrain = 99 - player.TackleDetermination;
                var tackleSkillLeftToTrain = 99 - player.TackleSkill;
                var markScore = tackleDeterminatonLeftToTrain * 8 + tackleDeterminatonLeftToTrain * 4;
                var tacklingScore = tackleDeterminatonLeftToTrain * 4 + tackleDeterminatonLeftToTrain * 8;
                if (trainingEffectModifier.RemoveNegativeTraining || stageMinimum < 99)
                {
                    if (markScore < tacklingScore)
                    {
                        return TrainingSchedulePreset.TacklingSkillAllWeek;
                    }
                    else
                        return TrainingSchedulePreset.MarkingAllWeek;
                }
                else
                {
                    if (markScore < tacklingScore)
                    {
                        return TrainingSchedulePreset.ImproveTacklingSkill;
                    }
                    else
                        return TrainingSchedulePreset.ImproveMarking;
                }
            }
            return null;

        }
        private static TrainingScheduleType[] ImproveTackleSkillTo(Player player, int stageMinimum,
            TrainingEffectModifier trainingEffectModifier)
        {
            if (player.TackleSkill < stageMinimum)
            {
                if (trainingEffectModifier.RemoveNegativeTraining|| stageMinimum<99)
                    return TrainingSchedulePreset.TacklingSkillAllWeek;
                return TrainingSchedulePreset.ImproveTacklingSkill;
            }
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
            if (player.Flair < stageMinimum)
            {
                return TrainingSchedulePreset.FiveASideAllWeek;
            }
            return null;
        }
        private static TrainingScheduleType[] ImproveKickingTo(Player player, int stageMinimum, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Kicking < stageMinimum)
            {
                if (trainingEffectModifier.RemoveNegativeTraining|| stageMinimum<99)
                    return TrainingSchedulePreset.KickingAllWeek;
                return TrainingSchedulePreset.ImproveKicking;
            }
            return null;
        }
        private static TrainingScheduleType[] ImproveThrowingTo(Player player, int stageMinimum)
        {
            if (player.Throwing < stageMinimum)
            {
                return TrainingSchedulePreset.ThrowingAllWeek;
            }
            return null;
        }
        private static TrainingScheduleType[] ImproveHandlingTo(Player player, int stageMinimum, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Handling < stageMinimum)
            {
                if (trainingEffectModifier.RemoveNegativeTraining || stageMinimum < 99)
                    return TrainingSchedulePreset.HandlingAllWeek;
                return TrainingSchedulePreset.ImproveHandling;
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
                if (trainingEffectModifier.RemoveNegativeTraining || stageMinimum < 99)
                    return TrainingSchedulePreset.WeightTrainingAllWeek;
                return TrainingSchedulePreset.ImproveDetermination;
            }
            return null;
        }
    }
}