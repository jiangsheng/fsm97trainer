using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fsm97Trainer
{
    public enum TrainingScheduleType
    {
        None,
        Shooting,
        Passing,
        Heading,
        Sprinting,
        Jogging,
        Control,
        Exercise,
        WeightTraining,
        ZonalDefence,
        Marking,
        Tackling,
        Physiotherapist,
        Handling,
        GoalKeeping,
        Kicking,
        Throwing,
        FiveASide,
        TrainingMatch
    }

    public class TrainingSchedule
    {
        public static TrainingScheduleType[] GetTrainingSchedule(Player player,bool autoResetStatus,bool maxEnergy, bool maxPower, bool noAlternativeTraining)
        {
            TrainingScheduleType[] schedule;
            if (player.Fitness < 99 && player.Position == (byte)PlayerPosition.GK && player.BestPosition != (byte)PlayerPosition.GK)
            {
                schedule = (TrainingScheduleType[])GetTrainingSchedule(player, (PlayerPosition)player.BestPosition, maxPower, noAlternativeTraining).Clone();
            }
            else
                schedule = (TrainingScheduleType[])GetTrainingSchedule(player, (PlayerPosition)player.Position, maxPower, noAlternativeTraining).Clone();

            if (!autoResetStatus && !maxEnergy)
            {
                if (player.Status == 0 && player.Position != (byte)PlayerPosition.GK && player.Fitness < 99)
                {
                    schedule[1] = schedule[3] = schedule[5] = TrainingScheduleType.Physiotherapist;
                }
            }
            return schedule;
        }
        public static TrainingScheduleType[] GetTrainingSchedule(Player player, PlayerPosition position, bool maxPower, bool noAlternativeTraining)
        {
            switch (position)
            {
                case PlayerPosition.GK: return GetGKTrainingSchedule(player, maxPower, noAlternativeTraining);
                case PlayerPosition.LB:
                case PlayerPosition.RB: return GetGLRBTrainingSchedule(player, maxPower, noAlternativeTraining);
                case PlayerPosition.CD: return GetCDTrainingSchedule(player, maxPower, noAlternativeTraining);
                case PlayerPosition.DM: return GetDMTrainingSchedule(player, maxPower, noAlternativeTraining);
                case PlayerPosition.SW: return GetSWTrainingSchedule(player, maxPower, noAlternativeTraining);

                case PlayerPosition.LWB:
                case PlayerPosition.RWB:
                     return GetLRWBTrainingSchedule(player, maxPower, noAlternativeTraining);
                case PlayerPosition.LM:
                case PlayerPosition.RM:
                case PlayerPosition.AM:
                        return GetLRAMTrainingSchedule(player, maxPower, noAlternativeTraining);
                case PlayerPosition.LW:
                case PlayerPosition.RW: return GetLRWTrainingSchedule(player, maxPower, noAlternativeTraining);
                case PlayerPosition.FR: return GetFRTrainingSchedule(player, maxPower, noAlternativeTraining);
                case PlayerPosition.SS: return GetSSTrainingSchedule(player, maxPower, noAlternativeTraining);
                case PlayerPosition.FOR: return GetFORTrainingSchedule(player, maxPower, noAlternativeTraining);
                default: return GenericTraining(player, maxPower);
            }
        }
        private static TrainingScheduleType[] GetLRWBTrainingSchedule(Player player, bool maxPower, bool noAlternativeTraining)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player);
            }
            if (player.Dribbling < 90)
                return TrainingSchedulePreset.ControlAllWeek;
            if (player.TackleSkill < 90) return ImproveTackle(player);
            if (player.Passing < 90)
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            if (player.Acceleration < 90)
            {
                return TrainingSchedulePreset.SprintingAllWeek;
            }
            return GetLRAMLRWBTrainingSchedule(player, maxPower, noAlternativeTraining);
        }
        private static TrainingScheduleType[] GetLRAMTrainingSchedule(Player player, bool maxPower, bool noAlternativeTraining)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player);
            }

            if (player.Passing < 90)
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            if (player.Awareness < 90) return TrainingSchedulePreset.TrainingMatchAllWeek;
            if (player.TackleSkill < 90) return ImproveTackle(player);
            return GetLRAMLRWBTrainingSchedule(player, maxPower, noAlternativeTraining);
        }

        private static TrainingScheduleType[] GetLRWTrainingSchedule(Player player, bool maxPower, bool noAlternativeTraining)
        {
            if (player.Control < 90)
                return TrainingSchedulePreset.ControlAllWeek;
            if (player.Passing< 90)
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            if (player.Speed < 90 || player.Acceleration < 90)
            {
                return TrainingSchedulePreset.SprintingAllWeek;
            }
            return GetFrontCourtTrainingSchedule(player, maxPower, noAlternativeTraining);
        }

        private static TrainingScheduleType[] GetFRTrainingSchedule(Player player, bool maxPower, bool noAlternativeTraining)
        {
            if (player.Control < 90)
                return TrainingSchedulePreset.ControlAllWeek;
            if (player.Shooting < 90)
                return TrainingSchedulePreset.TrainingMatchAllWeek;

            if (player.Speed < 90 || player.Acceleration < 90)
            {
                return ImproveSpeedWithHeading(player);
            }
            if (player.Flair < 90|| player.Awareness<90)
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            return GetFrontCourtTrainingSchedule(player, maxPower, noAlternativeTraining);
        }

        private static TrainingScheduleType[] GetSSTrainingSchedule(Player player, bool maxPower, bool noAlternativeTraining)
        {
            if (player.Shooting < 90)
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            if (player.Speed < 90 || player.Passing< 90)
            {
                return ImproveSpeedWithHeading(player);
            }
            if (player.Control < 90)
                return TrainingSchedulePreset.ControlAllWeek;
            if (player.Flair < 90)
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            return GetFrontCourtTrainingSchedule(player, maxPower, noAlternativeTraining);
        }

        private static TrainingScheduleType[] GetFORTrainingSchedule(Player player, bool maxPower, bool noAlternativeTraining)
        {
            if (player.Shooting<90)
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            if (player.Speed < 90 || player.Acceleration < 90)
            {
                return ImproveSpeedWithHeading(player);
            }
            if (player.Control < 90)
                return TrainingSchedulePreset.ControlAllWeek;
            if (player.Flair< 90)
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            return GetFrontCourtTrainingSchedule(player, maxPower, noAlternativeTraining);
        }

        private static TrainingScheduleType[] GetFrontCourtTrainingSchedule(Player player, bool maxPower, bool noAlternativeTraining)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player);
            }
            if (player.Speed < 99)
            {
                return ImproveSpeedWithHeading(player);                
            }
            if (player.Acceleration < 99)
            {
                return ImproveAccelerationWithHeading(player);
            }
            if (player.Shooting < 99 || player.Passing < 99) return TrainingSchedulePreset.TrainingMatchAllWeek;
            if (player.Heading < 99) return TrainingSchedulePreset.ImproveHeading;
            if (player.Control < 99 || player.Dribbling < 99 || player.Coolness < 99) return TrainingSchedulePreset.ControlAllWeek;
            if (player.Awareness < 99) return ImproveAwareness(player, maxPower);
            if (player.Agility < 99) return TrainingSchedulePreset.TrainingMatchAllWeek;
            if (player.Flair < 99) return TrainingSchedulePreset.FiveASideAllWeek;
            if (noAlternativeTraining)
            { 
                if(player.Consistency<99) return TrainingSchedulePreset.ControlAllWeek;
                return TrainingSchedulePreset.MaintainShape;
            }
            return GenericTraining(player, maxPower);
        }

        private static TrainingScheduleType[] ImproveAwareness(Player player, bool maxPower)
        {
            if (!maxPower)
                return TrainingSchedulePreset.ImproveAwarenessScheduleMaxPower;
            return TrainingSchedulePreset.ImproveAwarenessSchedule; 
        }

        private static TrainingScheduleType[] ImproveSpeedWithHeading(Player player)
        {
            if (player.Acceleration > player.Speed)
            {
                if (player.Heading < 90)
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
            if (player.Heading < 90)
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
        private static TrainingScheduleType[] ImproveFitness(Player player)
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

        private static TrainingScheduleType[] GetLRAMLRWBTrainingSchedule(Player player, bool maxPower, bool noAlternativeTraining)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player);
            }
            if (player.Speed < 99|| player.Acceleration < 99)
            {
                return TrainingSchedulePreset.SprintingAllWeek;
            }
            if (player.Shooting < 99 || player.Passing < 99) return TrainingSchedulePreset.TrainingMatchAllWeek;
            if (player.Control < 99 || player.Dribbling < 99) return TrainingSchedulePreset.ControlAllWeek;
            if (player.Flair < 99) return TrainingSchedulePreset.FiveASideAllWeek;
            if (player.Awareness < 99) return ImproveAwareness(player, maxPower);
            if (player.TackleDetermination < 99 || player.TackleSkill < 99) return ImproveTackle(player);
            if (noAlternativeTraining)
            {
                if (player.Consistency < 99) return TrainingSchedulePreset.ControlAllWeek;
                return TrainingSchedulePreset.MaintainShape;
            }
            return GenericTraining(player, maxPower);
        }

        private static TrainingScheduleType[] GetDMTrainingSchedule(Player player,bool maxPower, bool noAlternativeTraining)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player);
            }

            if (player.Passing < 90)
            {
                return TrainingSchedulePreset.TrainingMatchAllWeek;
            }
            if ( player.TackleSkill < 90) return ImproveTackle(player);
            if (player.Awareness < 90) return TrainingSchedulePreset.TrainingMatchAllWeek;

            if (player.Speed < 99)
            {
                return ImproveSpeedWithHeading(player);
            }
            if (player.Passing < 99) return TrainingSchedulePreset.TrainingMatchAllWeek;
            if (player.Heading < 99) return TrainingSchedulePreset.ImproveHeading;
            if (player.Awareness < 99) return ImproveAwareness(player,maxPower);
            if (player.TackleDetermination < 99 || player.TackleSkill < 99) return ImproveTackle(player);
            if (noAlternativeTraining)
            {
                if (player.Consistency < 99) return TrainingSchedulePreset.ControlAllWeek;
                return TrainingSchedulePreset.MaintainShape;
            }
            return GenericTraining(player, maxPower);
        }


        private static TrainingScheduleType[] GetSWTrainingSchedule(Player player,bool maxPower, bool noAlternativeTraining)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player);
            }
            if (player.TackleSkill < 90) return ImproveTackle(player);
            if (player.Awareness < 90) return TrainingSchedulePreset.TrainingMatchAllWeek;
            if (player.Dribbling < 90) return TrainingSchedulePreset.ControlAllWeek;
            if (player.Passing < 90) return TrainingSchedulePreset.TrainingMatchAllWeek;
            if (player.Speed < 99)
            {
                return ImproveSpeedWithHeading(player);
            }
            if (player.Acceleration < 99)
            {
                return ImproveAccelerationWithHeading(player);
            }
            if (player.Passing < 99) return TrainingSchedulePreset.TrainingMatchAllWeek;
            if (player.Heading < 99) return TrainingSchedulePreset.ImproveHeading;
            if (player.Dribbling < 99) return TrainingSchedulePreset.ControlAllWeek;
            if (player.Awareness < 99) return ImproveAwareness(player,maxPower);
            if (player.TackleDetermination < 99 || player.TackleSkill < 99) return ImproveTackle(player);
            if (noAlternativeTraining)
            {
                if (player.Consistency < 99) return TrainingSchedulePreset.ControlAllWeek;
                return TrainingSchedulePreset.MaintainShape;
            }
            return GenericTraining(player, maxPower);
        }

        private static TrainingScheduleType[] GetCDTrainingSchedule(Player player,bool maxPower, bool noAlternativeTraining)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player);
            }

            if (player.TackleSkill < 80)
            {
                return ImproveTackle(player);
            }
            if (player.Heading < 80)
            {
                return TrainingSchedulePreset.ImproveHeading;
            }
            if (player.TackleDetermination < 80)
            {
                return ImproveTackle(player);
            }
            if (player.Speed < 99)
            {
                return ImproveSpeedWithHeading(player);
            }
            if (player.Passing < 99) return TrainingSchedulePreset.TrainingMatchAllWeek;
            if (player.Heading < 99) return TrainingSchedulePreset.ImproveHeading;

            if (player.Awareness < 99) return ImproveAwareness(player, maxPower);
            if (player.TackleDetermination < 99 || player.TackleSkill < 99)
            {
                return ImproveTackle(player);
            }
            if (player.Coolness < 99|| player.Consistency < 99) return TrainingSchedulePreset.ControlAllWeek;
            if (noAlternativeTraining)
            {
                return TrainingSchedulePreset.MaintainShape;
            }
            return GenericTraining(player, maxPower);
        }

        private static TrainingScheduleType[] GetGLRBTrainingSchedule(Player player,bool maxPower, bool noAlternativeTraining)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player);
            }

            if (player.TackleSkill < 80)
            {
                return ImproveTackle(player);
            }
            if (player.Awareness < 80) return TrainingSchedulePreset.TrainingMatchAllWeek;

            if (player.TackleDetermination < 80)
            {
                return ImproveTackle(player);
            }

            if (player.Determination < 99) return TrainingSchedulePreset.ImproveDetermination;
            if (player.Speed < 99)
            {
                return ImproveSpeedWithHeading(player);
            }
            if (player.Passing < 99) return TrainingSchedulePreset.TrainingMatchAllWeek;
            if (player.Heading < 99) return TrainingSchedulePreset.ImproveHeading;

            if (player.Awareness < 99) return ImproveAwareness(player,maxPower);
            if (player.TackleDetermination < 99 || player.TackleSkill < 99)
            {
                return ImproveTackle(player);
            }
            if (player.Coolness < 99 || player.Consistency < 99) return TrainingSchedulePreset.ControlAllWeek;
            if (noAlternativeTraining)
            {
                return TrainingSchedulePreset.MaintainShape;
            }
            return GenericTraining(player, maxPower);
        }

        private static TrainingScheduleType[] GetGKTrainingSchedule(Player player,bool maxPower, bool noAlternativeTraining)
        {
            if (player.Fitness < player.Handling)
            {
                return ImproveFitness(player);
            }
            if (player.Handling < 99) return TrainingSchedulePreset.ImproveHandling;
            if (player.Kicking < 99) return TrainingSchedulePreset.ImproveKicking;
            if (player.Throwing < 99) return TrainingSchedulePreset.ImproveThrowing;

            if (player.Speed < 99)
            {
                return TrainingSchedulePreset.SprintingAllWeek;
            }
            if (player.Passing < 99 || player.Agility < 99) return TrainingSchedulePreset.TrainingMatchAllWeek;
            if (player.Consistency < 99 || player.Control < 99 || player.Coolness < 99) return TrainingSchedulePreset.ControlAllWeek;
            if (player.Awareness < 99) return ImproveAwareness(player,maxPower);
            if (noAlternativeTraining)
            {
                return TrainingSchedulePreset.MaintainShape;
            }
            return GenericTraining(player, maxPower);
        }

        private static TrainingScheduleType[] GenericTraining(Player player,bool maxPower)
        {
            if (player.Fitness < 99)
            {
                return ImproveFitness(player);
            }
            if (player.Acceleration < 99)
            {
                if (player.Heading > 90)
                {
                    if (player.Determination < 99)
                    {
                        return TrainingSchedulePreset.SprintingWithWeightTraining;
                    }
                    else
                        return TrainingSchedulePreset.SprintingAllWeek;
                }
                else 
                    return TrainingSchedulePreset.SprintingWithHeading;
            }
            if (player.Agility < 99 || player.Shooting < 99 || player.Passing < 99) return TrainingSchedulePreset.TrainingMatchAllWeek;
            if (player.Heading < 99) return TrainingSchedulePreset.ImproveHeading;
            if (player.Control < 99 || player.Dribbling < 99 || player.Coolness < 99) return TrainingSchedulePreset.ControlAllWeek;
            if (player.Flair < 99) return TrainingSchedulePreset.FiveASideAllWeek;
            if (player.Awareness < 99) return ImproveAwareness(player,maxPower);
            if (player.TackleDetermination < 99 || player.TackleSkill < 99) return ImproveTackle(player);
            if (player.Consistency < 99) return TrainingSchedulePreset.ControlAllWeek;
            if (player.Determination < 99) return TrainingSchedulePreset.ImproveDetermination;
            if (player.Handling < 99) return TrainingSchedulePreset.ImproveHandling;
            if (player.Kicking < 99) return TrainingSchedulePreset.ImproveKicking;
            if (player.Throwing < 99) return TrainingSchedulePreset.ImproveThrowing;
            if (player.Strength < 99) return TrainingSchedulePreset.ImproveStrength;
            return TrainingSchedulePreset.MaintainShape;

        }


        private static TrainingScheduleType[] ImproveTackle(Player player)
        {
            if (player.TackleDetermination < player.TackleSkill)
            {
                return TrainingSchedulePreset.ImproveMarking;
            }
            else if (player.TackleDetermination == player.TackleSkill)
            {
                return TrainingSchedulePreset.ImproveTacklingBalanced;
            }
            else
            {
                return TrainingSchedulePreset.ImproveTacklingSkill;
            }
        }
    }
}