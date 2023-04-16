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
        public static TrainingScheduleType[] GetTrainingSchedule(Player player, bool autoResetStatus, bool maxEnergy, bool maxPower, bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier)
        {
            TrainingScheduleType[] schedule;
            if (player.Fitness < 99 && player.Position == (byte)PlayerPosition.GK && player.BestPosition != (byte)PlayerPosition.GK)
            {
                schedule = (TrainingScheduleType[])GetTrainingSchedule(player, (PlayerPosition)player.BestPosition, maxPower, noAlternativeTraining, trainingEffectModifier).Clone();
            }
            else
                schedule = (TrainingScheduleType[])GetTrainingSchedule(player, (PlayerPosition)player.Position, maxPower, noAlternativeTraining, trainingEffectModifier).Clone();

            if (!autoResetStatus && !maxEnergy)
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
                    return GetLRWBTrainingSchedule(player, maxPower, noAlternativeTraining,  trainingEffectModifier);
                case PlayerPosition.LM:
                case PlayerPosition.RM:
                case PlayerPosition.AM:
                    return GetLRAMTrainingSchedule(player, maxPower, noAlternativeTraining,  trainingEffectModifier);
                case PlayerPosition.LW:
                case PlayerPosition.RW: return GetLRWTrainingSchedule(player, maxPower, noAlternativeTraining,trainingEffectModifier);
                case PlayerPosition.FR: return GetFRTrainingSchedule(player, maxPower, noAlternativeTraining, trainingEffectModifier);
                case PlayerPosition.SS: return GetSSTrainingSchedule(player, maxPower, noAlternativeTraining, trainingEffectModifier);
                case PlayerPosition.FOR: return GetFORTrainingSchedule(player, maxPower, noAlternativeTraining, trainingEffectModifier);
                default: return GenericTraining(player, maxPower, trainingEffectModifier);
            }
        }
        private static TrainingScheduleType[] GetGKTrainingSchedule(Player player, bool maxPower, bool noAlternativeTraining
            , TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < player.Handling)
            {
                return ImproveFitness(player, trainingEffectModifier);
            }
            if (player.Handling < 99) return ImproveHandling(trainingEffectModifier);
                
            if (player.Kicking < 99) return ImproveKicking(trainingEffectModifier); 
            if (player.Throwing < 99) return TrainingSchedulePreset.ImproveThrowing;
            if (player.Consistency < 85) return TrainingSchedulePreset.ControlAllWeek;
            if (player.Agility < 85)
            {
                if (noAlternativeTraining)
                    return TrainingSchedulePreset.FiveASideAllWeek;
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            }
            if (player.Awareness < 85)
            {
                if (noAlternativeTraining)
                    return TrainingSchedulePreset.FiveASideAllWeek;
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            }


            if (player.Speed < 99)
            {
                return TrainingSchedulePreset.SprintingAllWeek;
            }
            if (player.Passing < 99 || player.Agility < 99)
            {
                if (noAlternativeTraining)
                    return TrainingSchedulePreset.FiveASideAllWeek;
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            }
            if (player.Consistency < 99 || player.Control < 99 || player.Coolness < 99) return TrainingSchedulePreset.ControlAllWeek;
            if (player.Awareness < 99) return ImproveAwareness(player, maxPower, trainingEffectModifier);
            if (noAlternativeTraining)
            {
                return TrainingSchedulePreset.MaintainShape;
            }
            return GenericTraining(player, maxPower, trainingEffectModifier);
        }

        

        private static TrainingScheduleType[] GetLRBTrainingSchedule(Player player, bool maxPower, bool noAlternativeTraining
             , TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, trainingEffectModifier);
            }

            if (player.TackleSkill < 85)
            {
                if (noAlternativeTraining)
                    return TrainingSchedulePreset.FiveASideAllWeek;
                return ImproveTackle(player, trainingEffectModifier);
            }
            if (player.Awareness < 85)
            {
                if (noAlternativeTraining)
                    return TrainingSchedulePreset.FiveASideAllWeek;
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            }

            if (player.TackleDetermination < 85)
            {
                return ImproveTackle(player, trainingEffectModifier);
            }
            if (player.Heading < 85)
            {
                return TrainingSchedulePreset.ImproveHeading;
            }
            if (player.Determination < 99) return TrainingSchedulePreset.ImproveDetermination;
            return GenericTraining(player, maxPower, trainingEffectModifier);
        }

        private static TrainingScheduleType[] GetCDTrainingSchedule(Player player, bool maxPower, bool noAlternativeTraining
            , TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, trainingEffectModifier);
            }

            if (player.TackleSkill < 85)
            {
                return ImproveTackle(player, trainingEffectModifier);
            }
            if (player.Heading < 85)
            {
                return ImproveHeading(trainingEffectModifier);
                
            }
            if (player.TackleDetermination < 85)
            {
                return ImproveTackle(player, trainingEffectModifier);
            }
            if(player.Leadership<99 && trainingEffectModifier.PassingTrainLeadership)
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            return GenericTraining(player, maxPower, trainingEffectModifier);
        }


        private static TrainingScheduleType[] GetLRBCDTrainingSchedule(Player player, bool maxPower, bool noAlternativeTraining
            , TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Speed < 99)
            {
                if (trainingEffectModifier.RemoveNegativeTraining)
                    return TrainingSchedulePreset.SprintingAllWeek;
                return ImproveSpeedWithHeading(player);
            }
            if (player.Passing < 99) return TrainingSchedulePreset.TrainingMatchAllWeek;
            if (player.Heading < 99) return ImproveHeading(trainingEffectModifier);
            if (player.TackleDetermination < 99 || player.TackleSkill < 99)
            {
                return ImproveTackle(player, trainingEffectModifier);
            }
            if (player.Coolness < 99 || player.Consistency < 99) return TrainingSchedulePreset.ControlAllWeek;
            if (player.Awareness < 99) return ImproveAwareness(player, maxPower,trainingEffectModifier);
            if (noAlternativeTraining)
            {
                return TrainingSchedulePreset.MaintainShape;
            }
            return GenericTraining(player, maxPower, trainingEffectModifier);
        }

        private static TrainingScheduleType[] GetLRWBTrainingSchedule(Player player, bool maxPower, bool noAlternativeTraining
            , TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, trainingEffectModifier);
            }
            if (player.Dribbling < 85)
                return TrainingSchedulePreset.ControlAllWeek;
            if (player.TackleSkill < 85)
            {
                if (noAlternativeTraining)
                    return TrainingSchedulePreset.FiveASideAllWeek;
                return ImproveTackle(player, trainingEffectModifier);
            }   
            if (player.Passing < 85)
            {
                if (noAlternativeTraining)
                    return TrainingSchedulePreset.FiveASideAllWeek;
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            }

            if (player.Acceleration < 85)
            {
                return TrainingSchedulePreset.SprintingAllWeek;
            }
            if (player.Speed < 99 || player.Acceleration < 99)
            {
                return TrainingSchedulePreset.SprintingAllWeek;
            }
            if (player.Passing < 99)
            {
                if(noAlternativeTraining)
                    return TrainingSchedulePreset.FiveASideAllWeek;
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            }

            if (player.Agility < 99)
            {
                if (noAlternativeTraining)
                    return TrainingSchedulePreset.FiveASideAllWeek;
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            }
            if (player.Dribbling < 99) return TrainingSchedulePreset.ControlAllWeek;
            if (player.TackleDetermination < 99 || player.TackleSkill < 99) return ImproveTackle(player, trainingEffectModifier);
            if (player.Flair < 99) return TrainingSchedulePreset.FiveASideAllWeek;
            if (player.Awareness < 99) return ImproveAwareness(player, maxPower,trainingEffectModifier);

            if (noAlternativeTraining)
            {
                if (player.Consistency < 99) return TrainingSchedulePreset.ControlAllWeek;
                return TrainingSchedulePreset.MaintainShape;
            }
            return GenericTraining(player, maxPower, trainingEffectModifier);
        }

        private static TrainingScheduleType[] GetSWTrainingSchedule(Player player, bool maxPower, bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, trainingEffectModifier);
            }
            if (player.Awareness < 85)
            {
                if (noAlternativeTraining)
                    return TrainingSchedulePreset.FiveASideAllWeek;
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            }
            if (player.TackleSkill < 85) return ImproveTackle(player, trainingEffectModifier);

            if (player.Dribbling < 85) return TrainingSchedulePreset.ControlAllWeek;
            if (player.Passing < 85)
            {
                if (noAlternativeTraining)
                    return TrainingSchedulePreset.FiveASideAllWeek;
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            }
            if (player.Speed < 99)
            {
                if (trainingEffectModifier.RemoveNegativeTraining)
                    return TrainingSchedulePreset.SprintingAllWeek;
                return ImproveSpeedWithHeading(player);
            }
            if (player.Acceleration < 99)
            {
                if (trainingEffectModifier.RemoveNegativeTraining)
                    return TrainingSchedulePreset.SprintingAllWeek;
                return ImproveAccelerationWithHeading(player);
            }
            if (player.Passing < 99)
            {
                if (noAlternativeTraining)
                    return TrainingSchedulePreset.FiveASideAllWeek;
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            }
            if (player.Heading < 99) return ImproveHeading(trainingEffectModifier);
            if (player.Dribbling < 99) return TrainingSchedulePreset.ControlAllWeek;
            if (player.TackleDetermination < 99 || player.TackleSkill < 99) return ImproveTackle(player, trainingEffectModifier);
            if (player.Awareness < 99) return ImproveAwareness(player, maxPower,trainingEffectModifier);

            if (noAlternativeTraining)
            {
                if (player.Consistency < 99)
                    return TrainingSchedulePreset.ControlAllWeek;
                return TrainingSchedulePreset.MaintainShape;
            }
            return GenericTraining(player, maxPower, trainingEffectModifier);
        }
        private static TrainingScheduleType[] GetDMTrainingSchedule(Player player, bool maxPower, bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, trainingEffectModifier);
            }

            if (player.Passing < 85)
            {
                if (noAlternativeTraining)
                    return TrainingSchedulePreset.FiveASideAllWeek;
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            }
            if (player.TackleSkill < 85)
                return ImproveTackle(player, trainingEffectModifier);
            if (player.Awareness < 85)
            {
                if (noAlternativeTraining)
                    return TrainingSchedulePreset.FiveASideAllWeek;
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            }

            if (player.Speed < 99)
            {
                if (trainingEffectModifier.RemoveNegativeTraining)
                    return TrainingSchedulePreset.SprintingAllWeek;
                return ImproveSpeedWithHeading(player);
            }
            if (player.Passing < 99)
            {
                if (noAlternativeTraining)
                    return TrainingSchedulePreset.FiveASideAllWeek;
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            }
            if (player.Heading < 99) return ImproveHeading(trainingEffectModifier);
            if (player.TackleDetermination < 99 || player.TackleSkill < 99) return ImproveTackle(player, trainingEffectModifier);
            if (player.Awareness < 99) return ImproveAwareness(player, maxPower,trainingEffectModifier);
            if (noAlternativeTraining)
            {
                if (player.Consistency < 99) return TrainingSchedulePreset.ControlAllWeek;
                return TrainingSchedulePreset.MaintainShape;
            }
            return GenericTraining(player, maxPower, trainingEffectModifier);
        }
        private static TrainingScheduleType[] GetLRAMTrainingSchedule(Player player, bool maxPower, bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, trainingEffectModifier);
            }

            if (player.Passing < 85)
            {
                if (noAlternativeTraining)
                    return TrainingSchedulePreset.FiveASideAllWeek;
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            }

            if (player.TackleSkill < 85) return ImproveTackle(player, trainingEffectModifier);

            if (player.Speed < 99 || player.Acceleration < 99)
            {
                return TrainingSchedulePreset.SprintingAllWeek;
            }
            if (player.Passing < 99)
            {
                if (noAlternativeTraining)
                    return TrainingSchedulePreset.FiveASideAllWeek;
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            }
            if (player.Shooting < 99) return TrainingSchedulePreset.TrainingMatchAllWeek;
            if (player.Control < 99 || player.Dribbling < 99) return TrainingSchedulePreset.ControlAllWeek;
            if (player.Flair < 99) return TrainingSchedulePreset.FiveASideAllWeek;
            if (player.TackleDetermination < 99 || player.TackleSkill < 99) return ImproveTackle(player, trainingEffectModifier);
            if (player.Awareness < 99) return ImproveAwareness(player, maxPower,trainingEffectModifier);
            if (noAlternativeTraining)
            {
                if (player.Consistency < 99) return TrainingSchedulePreset.ControlAllWeek;
                return TrainingSchedulePreset.MaintainShape;
            }
            return GenericTraining(player, maxPower, trainingEffectModifier);
        }

        private static TrainingScheduleType[] GetLRWTrainingSchedule(Player player, bool maxPower, bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, trainingEffectModifier);
            }
            if (player.Passing < 85)
            {
                if (noAlternativeTraining)
                    return TrainingSchedulePreset.FiveASideAllWeek;
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            }
            if (player.Control < 85)
                return TrainingSchedulePreset.ControlAllWeek;

            if (player.Speed < 85 || player.Acceleration < 85)
            {
                return TrainingSchedulePreset.SprintingAllWeek;
            }
            if (player.Speed < 99)
            {
                if (noAlternativeTraining)
                    return TrainingSchedulePreset.SprintingAllWeek;
                if (trainingEffectModifier.RemoveNegativeTraining)
                    return TrainingSchedulePreset.SprintingAllWeek;
                return ImproveSpeedWithHeading(player);
            }
            if (player.Acceleration < 99)
            {
                if (noAlternativeTraining)
                    return TrainingSchedulePreset.SprintingAllWeek;
                if (trainingEffectModifier.RemoveNegativeTraining)
                    return TrainingSchedulePreset.SprintingAllWeek;
                return ImproveAccelerationWithHeading(player);
            }
            if (player.Passing < 99)
            {
                if (noAlternativeTraining)
                    return TrainingSchedulePreset.FiveASideAllWeek;
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            }
            if (player.Shooting < 99)
            {              
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            }
            if (player.Agility < 99)
            {
                if (noAlternativeTraining)
                    return TrainingSchedulePreset.FiveASideAllWeek;
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            }
            if (!noAlternativeTraining)
            {
                if (player.Heading < 99) 
                    return TrainingSchedulePreset.ImproveHeading;
            }
            if (player.Control < 99 || player.Dribbling < 99 || player.Coolness < 99) return TrainingSchedulePreset.ControlAllWeek;

            if (player.Flair < 99) return TrainingSchedulePreset.FiveASideAllWeek;
            if (player.Awareness < 99) return ImproveAwareness(player, maxPower,trainingEffectModifier);
            if (noAlternativeTraining)
            {
                if (player.Consistency < 99) return TrainingSchedulePreset.ControlAllWeek;
                return TrainingSchedulePreset.MaintainShape;
            }
            return GenericTraining(player, maxPower, trainingEffectModifier);
        }

        private static TrainingScheduleType[] GetFRTrainingSchedule(Player player, bool maxPower, bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, trainingEffectModifier);
            }
            if (player.Control < 85)
                return TrainingSchedulePreset.ControlAllWeek;
            if (player.Passing < 85)
            {
                if (noAlternativeTraining)
                    return TrainingSchedulePreset.FiveASideAllWeek;
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            }

            if (player.Speed < 85 || player.Acceleration < 85)
            {
                if (trainingEffectModifier.RemoveNegativeTraining)
                    return TrainingSchedulePreset.SprintingAllWeek;
                return ImproveSpeedWithHeading(player);
            }
            if (player.Flair < 85 || player.Awareness < 85)
            {
                if (noAlternativeTraining)
                    return TrainingSchedulePreset.FiveASideAllWeek;
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            }
            return GetFrontCourtTrainingSchedule(player, maxPower, noAlternativeTraining, trainingEffectModifier);
        }

        private static TrainingScheduleType[] GetSSTrainingSchedule(Player player, bool maxPower, bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, trainingEffectModifier);
            }
            if (player.Shooting < 85 || player.Passing < 85)
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            if (player.Flair < 85)
                return TrainingSchedulePreset.TrainingMatchAllWeek;

            if (player.Control < 85)
                return TrainingSchedulePreset.ControlAllWeek;

            return GetFrontCourtTrainingSchedule(player, maxPower, noAlternativeTraining, trainingEffectModifier);
        }

        private static TrainingScheduleType[] GetFORTrainingSchedule(Player player, bool maxPower, bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player, trainingEffectModifier);
            }
            if (player.Shooting < 85)
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            if (player.Speed < 85 || player.Acceleration < 85)
            {
                if (trainingEffectModifier.RemoveNegativeTraining)
                    return TrainingSchedulePreset.SprintingAllWeek;
                return ImproveSpeedWithHeading(player);
            }
            if (player.Heading < 85)
                return ImproveHeading(trainingEffectModifier);
            if (player.Control < 85)
                return TrainingSchedulePreset.ControlAllWeek;
            if (player.Flair < 85)
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            return GetFrontCourtTrainingSchedule(player, maxPower, noAlternativeTraining, trainingEffectModifier);
        }

        private static TrainingScheduleType[] GetFrontCourtTrainingSchedule(Player player, bool maxPower, bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Speed < 99)
            {
                if (trainingEffectModifier.RemoveNegativeTraining)
                    return TrainingSchedulePreset.SprintingAllWeek;
                return ImproveSpeedWithHeading(player);
            }
            if (player.Acceleration < 99)
            {
                if (trainingEffectModifier.RemoveNegativeTraining)
                    return TrainingSchedulePreset.SprintingAllWeek;
                return ImproveAccelerationWithHeading(player);
            }
            if (player.Shooting < 99 || player.Passing < 99) return TrainingSchedulePreset.TrainingMatchAllWeek;
            if (player.Agility < 99) return TrainingSchedulePreset.TrainingMatchAllWeek;
            if (player.Heading < 99) return ImproveHeading(trainingEffectModifier);
            if (player.Control < 99 || player.Dribbling < 99 || player.Coolness < 99) return TrainingSchedulePreset.ControlAllWeek;
            if (player.Flair < 99) return TrainingSchedulePreset.FiveASideAllWeek;
            if (player.Awareness < 99) return ImproveAwareness(player, maxPower,trainingEffectModifier);
            
            if (noAlternativeTraining)
            {
                if (player.Consistency < 99) return TrainingSchedulePreset.ControlAllWeek;
                return TrainingSchedulePreset.MaintainShape;
            }
            return GenericTraining(player, maxPower, trainingEffectModifier);
        }

        private static TrainingScheduleType[] GenericTraining(Player player, bool maxPower, TrainingEffectModifier trainingEffectModifier)
        {
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
            if (player.Heading < 99) return ImproveHeading(trainingEffectModifier);
            if (player.Control < 99 || player.Dribbling < 99 || player.Coolness < 99) return TrainingSchedulePreset.ControlAllWeek;
            if (player.Flair < 99) return TrainingSchedulePreset.FiveASideAllWeek;
            if (player.Awareness < 99) return ImproveAwareness(player, maxPower,trainingEffectModifier);
            if (player.TackleDetermination < 99 || player.TackleSkill < 99) return ImproveTackle(player, trainingEffectModifier);
            if (player.Consistency < 99) return TrainingSchedulePreset.ControlAllWeek;
            if (player.Determination < 99) return ImproveDetermination(trainingEffectModifier);
            if (player.Handling < 99) return  ImproveHandling(trainingEffectModifier);
            if (player.Kicking < 99) return ImproveKicking(trainingEffectModifier);
            if (player.Throwing < 99) return TrainingSchedulePreset.ImproveThrowing;
            if (player.Strength < 99) return ImproveStrength(trainingEffectModifier); 
            if (player.Leadership < 99 && trainingEffectModifier.PassingTrainLeadership)
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            if (player.ThrowIn< 99 && trainingEffectModifier.ThrowingTrainThrowIn)
                return TrainingSchedulePreset.ImproveThrowing;
            if (player.Greed< 99 && trainingEffectModifier.ShootingTrainGreed)
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

        private static TrainingScheduleType[] ImproveHandling(TrainingEffectModifier trainingEffectModifier)
        {
            if (trainingEffectModifier.RemoveNegativeTraining)
                return TrainingSchedulePreset.HandlingAllWeek;
            return TrainingSchedulePreset.ImproveHandling;
        }
        private static TrainingScheduleType[] ImproveKicking(TrainingEffectModifier trainingEffectModifier)
        {
            if (trainingEffectModifier.RemoveNegativeTraining)
                return TrainingSchedulePreset.KickingAllWeek;
            return TrainingSchedulePreset.ImproveKicking;
        }
        private static TrainingScheduleType[] ImproveHeading(TrainingEffectModifier trainingEffectModifier)
        {
            if (trainingEffectModifier.RemoveNegativeTraining)
                return TrainingSchedulePreset.HeadingAllWeek;
            return TrainingSchedulePreset.ImproveHeading;
        }
        private static TrainingScheduleType[] ImproveStrength(TrainingEffectModifier trainingEffectModifier)
        {
            if (trainingEffectModifier.RemoveNegativeTraining)
                return TrainingSchedulePreset.WeightTrainingAllWeek;
            return TrainingSchedulePreset.ImproveStrength;
        }

        private static TrainingScheduleType[] ImproveDetermination(TrainingEffectModifier trainingEffectModifier)
        {
            if(trainingEffectModifier.RemoveNegativeTraining)
                return TrainingSchedulePreset.WeightTrainingAllWeek;
            return TrainingSchedulePreset.ImproveDetermination;
        }
    }
}