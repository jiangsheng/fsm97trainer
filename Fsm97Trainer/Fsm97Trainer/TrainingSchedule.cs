using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        static TrainingScheduleType[] TrainingMatchAllWeek = new TrainingScheduleType[] {
            TrainingScheduleType.TrainingMatch, TrainingScheduleType.TrainingMatch,
            TrainingScheduleType.TrainingMatch, TrainingScheduleType.TrainingMatch,
            TrainingScheduleType.TrainingMatch, TrainingScheduleType.TrainingMatch,
            TrainingScheduleType.TrainingMatch
        };
        static TrainingScheduleType[] ControlAllWeek = new TrainingScheduleType[] {
            TrainingScheduleType.Control, TrainingScheduleType.Control,
            TrainingScheduleType.Control, TrainingScheduleType.Control,
            TrainingScheduleType.Control, TrainingScheduleType.Control,
            TrainingScheduleType.Control

        };
        static TrainingScheduleType[] ImproveHandling = new TrainingScheduleType[] {
           TrainingScheduleType.Handling, TrainingScheduleType.Handling,
            TrainingScheduleType.Handling, TrainingScheduleType.Handling,
            TrainingScheduleType.Handling, TrainingScheduleType.Kicking,
            TrainingScheduleType.Handling
        };
        static TrainingScheduleType[] ImproveKicking = new TrainingScheduleType[] {
           TrainingScheduleType.Kicking, TrainingScheduleType.Kicking,
            TrainingScheduleType.Kicking, TrainingScheduleType.Kicking,
            TrainingScheduleType.GoalKeeping, TrainingScheduleType.Throwing,
            TrainingScheduleType.Kicking
        };
        static TrainingScheduleType[] ImproveThrowing = new TrainingScheduleType[] {
           TrainingScheduleType.Throwing, TrainingScheduleType.Throwing,
            TrainingScheduleType.Throwing, TrainingScheduleType.Throwing,
            TrainingScheduleType.Throwing, TrainingScheduleType.Throwing,
            TrainingScheduleType.Throwing
        };
        static TrainingScheduleType[] ImproveHeading = new TrainingScheduleType[] {
           TrainingScheduleType.Heading, TrainingScheduleType.Heading,
            TrainingScheduleType.Heading, TrainingScheduleType.Heading,
            TrainingScheduleType.Sprinting, TrainingScheduleType.Heading,
            TrainingScheduleType.Heading
        };
        static TrainingScheduleType[] ImproveAwareness = new TrainingScheduleType[] {
           TrainingScheduleType.ZonalDefence, TrainingScheduleType.ZonalDefence,
            TrainingScheduleType.ZonalDefence, TrainingScheduleType.Sprinting,
            TrainingScheduleType.TrainingMatch, TrainingScheduleType.WeightTraining,
            TrainingScheduleType.TrainingMatch
        };
        static TrainingScheduleType[] ImproveDetermination = new TrainingScheduleType[] {
           TrainingScheduleType.WeightTraining, TrainingScheduleType.WeightTraining,
            TrainingScheduleType.WeightTraining, TrainingScheduleType.WeightTraining,
            TrainingScheduleType.Sprinting, TrainingScheduleType.WeightTraining,
            TrainingScheduleType.WeightTraining
        };
        static TrainingScheduleType[] ImproveMarking = new TrainingScheduleType[] {
           TrainingScheduleType.Marking, TrainingScheduleType.Marking,
            TrainingScheduleType.Marking, TrainingScheduleType.Marking,
            TrainingScheduleType.Marking, TrainingScheduleType.Marking,
            TrainingScheduleType.TrainingMatch
        };
        static TrainingScheduleType[] ImproveTacklingSkill = new TrainingScheduleType[] {
           TrainingScheduleType.Tackling, TrainingScheduleType.Tackling,
            TrainingScheduleType.Tackling, TrainingScheduleType.Tackling,
            TrainingScheduleType.Tackling, TrainingScheduleType.Tackling,
            TrainingScheduleType.TrainingMatch
        };
        static TrainingScheduleType[] ImproveTacklingBalanced = new TrainingScheduleType[] {
           TrainingScheduleType.Marking, TrainingScheduleType.Marking,
            TrainingScheduleType.Marking, TrainingScheduleType.Tackling,
            TrainingScheduleType.Tackling, TrainingScheduleType.Tackling,
            TrainingScheduleType.TrainingMatch
        };

        static TrainingScheduleType[] FiveASideAllWeek = new TrainingScheduleType[] {
           TrainingScheduleType.FiveASide, TrainingScheduleType.FiveASide,
            TrainingScheduleType.FiveASide, TrainingScheduleType.FiveASide,
            TrainingScheduleType.FiveASide, TrainingScheduleType.FiveASide,
            TrainingScheduleType.FiveASide
        };
        static TrainingScheduleType[] SprintingAllWeek = new TrainingScheduleType[] {
           TrainingScheduleType.Sprinting, TrainingScheduleType.Sprinting,
            TrainingScheduleType.Sprinting, TrainingScheduleType.Sprinting,
            TrainingScheduleType.Sprinting, TrainingScheduleType.Sprinting,
            TrainingScheduleType.Sprinting
        };
        static TrainingScheduleType[] SprintingWithHeading = new TrainingScheduleType[] {
           TrainingScheduleType.Sprinting, TrainingScheduleType.Sprinting,
            TrainingScheduleType.Sprinting, TrainingScheduleType.Sprinting,
            TrainingScheduleType.Heading, TrainingScheduleType.Heading,
            TrainingScheduleType.Heading
        };

        static TrainingScheduleType[] SprintingWithWeightTraining = new TrainingScheduleType[] {
           TrainingScheduleType.Sprinting, TrainingScheduleType.Sprinting,
            TrainingScheduleType.Sprinting, TrainingScheduleType.Sprinting,
            TrainingScheduleType.WeightTraining, TrainingScheduleType.WeightTraining,
            TrainingScheduleType.WeightTraining
        };
        static TrainingScheduleType[] ImproveStrength = new TrainingScheduleType[] {
           TrainingScheduleType.WeightTraining, TrainingScheduleType.WeightTraining,
            TrainingScheduleType.WeightTraining, TrainingScheduleType.WeightTraining,
            TrainingScheduleType.Sprinting, TrainingScheduleType.TrainingMatch,
            TrainingScheduleType.WeightTraining
        };

        static TrainingScheduleType[] MaintainShape = new TrainingScheduleType[] {
           TrainingScheduleType.WeightTraining, TrainingScheduleType.Kicking,
            TrainingScheduleType.Sprinting, TrainingScheduleType.Handling,
            TrainingScheduleType.TrainingMatch, TrainingScheduleType.Throwing,
            TrainingScheduleType.Control
        };

        public static TrainingScheduleType[] GetTrainingSchedule(Player player)
        {
            TrainingScheduleType[] schedule;
            if (player.Fitness < 99 && player.Position == (byte)PlayerPosition.GK && player.BestPosition != (byte)PlayerPosition.GK)
            {
                schedule = (TrainingScheduleType[])GetTrainingSchedule(player, (PlayerPosition)player.BestPosition).Clone();
            }
            else
                schedule = (TrainingScheduleType[])GetTrainingSchedule(player, (PlayerPosition)player.Position).Clone();

            if (player.Status == 0 && player.Position != (byte)PlayerPosition.GK && player.Fitness < 99)
            {
                schedule[1] = schedule[3] = schedule[5] = TrainingScheduleType.Physiotherapist;
            }
            return schedule;
        }
        public static TrainingScheduleType[] GetTrainingSchedule(Player player, PlayerPosition position)
        {
            switch (position)
            {
                case PlayerPosition.GK: return GetGKTrainingSchedule(player);
                case PlayerPosition.LB:
                case PlayerPosition.RB: return GetGLRBTrainingSchedule(player);
                case PlayerPosition.CD: return GetCDTrainingSchedule(player);
                case PlayerPosition.LWB:
                case PlayerPosition.RWB: return GetLRWBTrainingSchedule(player);
                case PlayerPosition.SW: return GetSWTrainingSchedule(player);
                case PlayerPosition.DM: return GetDMTrainingSchedule(player);
                case PlayerPosition.LM:
                case PlayerPosition.RM: return GetLRMTrainingSchedule(player);
                case PlayerPosition.AM: return GetAMTrainingSchedule(player);
                case PlayerPosition.LW:
                case PlayerPosition.RW: return GetLRWTrainingSchedule(player);
                case PlayerPosition.FR: return GetFRTrainingSchedule(player);
                case PlayerPosition.SS: return GetSSTrainingSchedule(player);
                case PlayerPosition.FOR: return GetFORTrainingSchedule(player);
                default: return GenericTraining(player);
            }
        }

        private static TrainingScheduleType[] GetFORTrainingSchedule(Player player)
        {
            if (player.Speed < 99) return SprintingAllWeek;
            if (player.Acceleration < 99) return SprintingAllWeek;
            if (player.Agility < 99 || player.Passing < 99) return TrainingMatchAllWeek;
            if (player.Shooting < 99 || player.Passing < 99) return TrainingMatchAllWeek;
            if (player.Heading < 99) return ImproveHeading;
            if (player.Control < 99 || player.Dribbling < 99 || player.Coolness < 99) return ControlAllWeek;
            if (player.Awareness < 99) return ImproveAwareness;
            if (player.Flair < 99) return FiveASideAllWeek;
            return GenericTraining(player);
        }

        private static TrainingScheduleType[] GetSSTrainingSchedule(Player player)
        {
            if (player.Speed < 99) return SprintingAllWeek;
            if (player.Acceleration < 99) return SprintingAllWeek;
            if (player.Agility < 99 || player.Passing < 99) return TrainingMatchAllWeek;
            if (player.Shooting < 99 || player.Passing < 99) return TrainingMatchAllWeek;
            if (player.Heading < 99) return ImproveHeading;
            if (player.Control < 99 || player.Dribbling < 99||player.Coolness<99) return ControlAllWeek;            
            if (player.Awareness < 99) return ImproveAwareness;
            if (player.Flair < 99) return FiveASideAllWeek;
            return GenericTraining(player);
        }

        private static TrainingScheduleType[] GetFRTrainingSchedule(Player player)
        {
            if (player.Speed < 99) return SprintingAllWeek;
            if (player.Acceleration < 99) return SprintingAllWeek;
            if (player.Agility < 99 || player.Passing < 99) return TrainingMatchAllWeek;
            if (player.Shooting < 99 || player.Passing < 99) return TrainingMatchAllWeek;
            if (player.Heading < 99) return ImproveHeading;
            if (player.Control < 99 || player.Dribbling < 99) return ControlAllWeek;
            if (player.Awareness < 99) return ImproveAwareness;
            if (player.Flair < 99) return FiveASideAllWeek;
            return GenericTraining(player);
        }

        private static TrainingScheduleType[] GetLRWTrainingSchedule(Player player)
        {
            if (player.Speed < 99) return SprintingAllWeek;
            if (player.Acceleration < 99) return SprintingAllWeek;
            if (player.Agility < 99 || player.Passing < 99) return TrainingMatchAllWeek;
            if (player.Shooting < 99 || player.Passing < 99) return TrainingMatchAllWeek;
            if (player.Control < 99 || player.Dribbling < 99) return ControlAllWeek;
            if (player.Awareness < 99) return ImproveAwareness;
            if (player.Flair < 99) return FiveASideAllWeek;
            if (player.TackleDetermination < 99 || player.TackleSkill < 99) return ImproveTackle(player);
            return GenericTraining(player);
        }

        private static TrainingScheduleType[] GetAMTrainingSchedule(Player player)
        {
            if (player.Speed < 99) return SprintingAllWeek;
            if (player.Acceleration < 99) return SprintingAllWeek;
            if (player.Shooting < 99 || player.Passing < 99) return TrainingMatchAllWeek;
            if (player.Control < 99 || player.Dribbling < 99) return ControlAllWeek;
            if (player.Awareness < 99) return ImproveAwareness;
            if (player.Flair < 99) return FiveASideAllWeek;
            if (player.TackleDetermination < 99 || player.TackleSkill < 99) return ImproveTackle(player);
            return GenericTraining(player);

        }

        private static TrainingScheduleType[] GetDMTrainingSchedule(Player player)
        {
            if (player.Speed < 99) return SprintingAllWeek;
            if (player.Passing < 99) return TrainingMatchAllWeek;
            if (player.Heading < 99) return ImproveHeading;
            if (player.Awareness < 99) return ImproveAwareness;
            if (player.TackleDetermination < 99 || player.TackleSkill < 99) return ImproveTackle(player);
            return GenericTraining(player);
        }

        private static TrainingScheduleType[] GetLRMTrainingSchedule(Player player)
        {
            if (player.Speed < 99) return SprintingAllWeek;
            if (player.Acceleration < 99) return SprintingAllWeek;
            if (player.Shooting < 99 || player.Passing < 99) return TrainingMatchAllWeek;
            if (player.Control < 99 || player.Dribbling < 99) return ControlAllWeek;
            if (player.Awareness < 99) return ImproveAwareness;
            if (player.Flair < 99) return FiveASideAllWeek;
            if (player.TackleDetermination < 99 || player.TackleSkill < 99) return ImproveTackle(player);
            return GenericTraining(player);
        }

        private static TrainingScheduleType[] GetSWTrainingSchedule(Player player)
        {
            if (player.Speed < 99) return SprintingAllWeek;
            if (player.Acceleration < 99) return SprintingAllWeek;
            if (player.Passing < 99) return TrainingMatchAllWeek;
            if (player.Heading < 99) return ImproveHeading;
            if (player.Dribbling < 99) return ControlAllWeek;
            if (player.Awareness < 99) return ImproveAwareness;
            if (player.TackleDetermination < 99 || player.TackleSkill < 99) return ImproveTackle(player);
            return GenericTraining(player);
        }

        private static TrainingScheduleType[] GetLRWBTrainingSchedule(Player player)
        {
            if (player.Speed < 99) return SprintingAllWeek;
            if (player.Acceleration < 99) return SprintingAllWeek;
            if (player.Agility < 99 || player.Passing < 99) return TrainingMatchAllWeek;
            if (player.Dribbling < 99) return ControlAllWeek;
            if (player.Flair < 99) return FiveASideAllWeek;
            if (player.Awareness < 99) return ImproveAwareness;
            if (player.TackleDetermination < 99 || player.TackleSkill < 99) return ImproveTackle(player);
            return GenericTraining(player);
        }

        private static TrainingScheduleType[] GetCDTrainingSchedule(Player player)
        {
            if (player.TackleDetermination < 99 || player.TackleSkill < 99) return ImproveTackle(player);
            if (player.Speed < 99) return SprintingAllWeek;
            if (player.Passing < 99) return TrainingMatchAllWeek;
            if (player.Heading < 99) return ImproveHeading;
            
            if (player.Coolness < 99) return ControlAllWeek;
            if (player.Awareness < 99) return ImproveAwareness;
            if (player.Consistency < 99) return ControlAllWeek;
            return GenericTraining(player);
        }

        private static TrainingScheduleType[] GetGLRBTrainingSchedule(Player player)
        {
            if (player.TackleDetermination < 99 || player.TackleSkill < 99) return ImproveTackle(player);
            if (player.Awareness < 99) return ImproveAwareness;
            if (player.Heading < 99) return ImproveHeading;
            if (player.Passing < 99) return TrainingMatchAllWeek;
            if (player.Speed < 99)
            {
                if (player.Determination > 90)
                    return SprintingAllWeek;
                else
                    return SprintingWithWeightTraining;
            }
            if (player.Consistency < 99) return ControlAllWeek;
            if (player.Determination < 99) return ImproveDetermination;
            return GenericTraining(player);
        }

        private static TrainingScheduleType[] GetGKTrainingSchedule(Player player)
        {
            if (player.Handling < 99) return ImproveHandling;
            if (player.Kicking < 99) return ImproveKicking;
            if (player.Throwing < 99) return ImproveThrowing;
            if (player.Speed < 99) return SprintingAllWeek;
            if (player.Passing < 99 || player.Agility < 99) return TrainingMatchAllWeek;
            if (player.Consistency < 99 || player.Control < 99 || player.Coolness < 99) return ControlAllWeek;
            if (player.Awareness < 99) return ImproveAwareness;
            return GenericTraining(player);
        }

        private static TrainingScheduleType[] GenericTraining(Player player)
        {
            if (player.Speed < 99) return SprintingAllWeek;
            if (player.Acceleration < 99)
            {
                if (player.Heading > 90) return SprintingAllWeek;
                else return SprintingWithHeading;
            }
            if (player.Agility < 99 || player.Shooting < 99 || player.Passing < 99) return TrainingMatchAllWeek;
            if (player.Heading < 99) return ImproveHeading;
            if (player.Control < 99 || player.Dribbling < 99 || player.Coolness < 99) return ControlAllWeek;
            if (player.Flair < 99) return FiveASideAllWeek;
            if (player.Awareness < 99) return ImproveAwareness;
            if (player.TackleDetermination < 99 || player.TackleSkill < 99) return ImproveTackle(player);
            if (player.Consistency < 99) return ControlAllWeek;
            if (player.Determination < 99) return ImproveDetermination;
            if (player.Handling < 99) return ImproveHandling;
            if (player.Kicking < 99) return ImproveKicking;
            if (player.Throwing < 99) return ImproveThrowing;
            if (player.Strength < 99) return ImproveStrength;
            return MaintainShape;
        }


        private static TrainingScheduleType[] ImproveTackle(Player player)
        {
            if (player.TackleDetermination < player.TackleSkill)
            {
                return ImproveMarking;
            }
            else if (player.TackleDetermination == player.TackleSkill)
            {
                return ImproveTacklingBalanced;
            }
            else
            {
                return ImproveTacklingSkill;
            }
        }
    }
}