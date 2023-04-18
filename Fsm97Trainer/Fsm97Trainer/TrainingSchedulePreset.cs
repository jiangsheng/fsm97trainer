using FSM97Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fsm97Trainer
{
    internal static class TrainingSchedulePreset
    {
        public static TrainingScheduleType[] TrainingMatchAllWeek = new TrainingScheduleType[] {
            TrainingScheduleType.TrainingMatch, TrainingScheduleType.TrainingMatch,
            TrainingScheduleType.TrainingMatch, TrainingScheduleType.TrainingMatch,
            TrainingScheduleType.TrainingMatch, TrainingScheduleType.TrainingMatch,
            TrainingScheduleType.TrainingMatch
        };
        public static TrainingScheduleType[] ControlAllWeek = new TrainingScheduleType[] {
            TrainingScheduleType.Control, TrainingScheduleType.Control,
            TrainingScheduleType.Control, TrainingScheduleType.Control,
            TrainingScheduleType.Control, TrainingScheduleType.Control,
            TrainingScheduleType.Control

        };
        public static TrainingScheduleType[] ImproveHandling = new TrainingScheduleType[] {
           TrainingScheduleType.Handling, TrainingScheduleType.Handling,
            TrainingScheduleType.Handling, TrainingScheduleType.Handling,
            TrainingScheduleType.Handling, TrainingScheduleType.Kicking,
            TrainingScheduleType.Handling
        };
        public static TrainingScheduleType[] HandlingAllWeek = new TrainingScheduleType[] {
           TrainingScheduleType.Handling, TrainingScheduleType.Handling,
            TrainingScheduleType.Handling, TrainingScheduleType.Handling,
            TrainingScheduleType.Handling, TrainingScheduleType.Handling,
            TrainingScheduleType.Handling
        };
        public static TrainingScheduleType[] ImproveKicking = new TrainingScheduleType[] {
           TrainingScheduleType.Kicking, TrainingScheduleType.Kicking,
            TrainingScheduleType.Kicking, TrainingScheduleType.Kicking,
            TrainingScheduleType.Kicking, TrainingScheduleType.Throwing,
            TrainingScheduleType.Kicking

        };
        public static TrainingScheduleType[] KickingAllWeek = new TrainingScheduleType[] {
           TrainingScheduleType.Kicking, TrainingScheduleType.Kicking,
            TrainingScheduleType.Kicking, TrainingScheduleType.Kicking,
            TrainingScheduleType.Kicking, TrainingScheduleType.Throwing,
            TrainingScheduleType.Kicking

        };
        public static TrainingScheduleType[] ImproveThrowing = new TrainingScheduleType[] {
           TrainingScheduleType.Throwing, TrainingScheduleType.Throwing,
            TrainingScheduleType.Throwing, TrainingScheduleType.Throwing,
            TrainingScheduleType.Throwing, TrainingScheduleType.Throwing,
            TrainingScheduleType.Throwing
        };
        public static TrainingScheduleType[] GoalkeepingAllWeek = new TrainingScheduleType[] {
           TrainingScheduleType.GoalKeeping, TrainingScheduleType.GoalKeeping,
            TrainingScheduleType.GoalKeeping, TrainingScheduleType.GoalKeeping,
            TrainingScheduleType.GoalKeeping, TrainingScheduleType.GoalKeeping,
            TrainingScheduleType.GoalKeeping
        };
        
        public static TrainingScheduleType[] ImproveHeading = new TrainingScheduleType[] {
           TrainingScheduleType.Heading, TrainingScheduleType.Heading,
            TrainingScheduleType.Heading, TrainingScheduleType.Heading,
            TrainingScheduleType.Sprinting, TrainingScheduleType.Heading,
            TrainingScheduleType.Heading
        };
        public static TrainingScheduleType[] HeadingAllWeek = new TrainingScheduleType[] {
           TrainingScheduleType.Heading, TrainingScheduleType.Heading,
            TrainingScheduleType.Heading, TrainingScheduleType.Heading,
            TrainingScheduleType.Heading, TrainingScheduleType.Heading,
            TrainingScheduleType.Heading
        };

        public static TrainingScheduleType[] ImproveAwarenessSchedule = new TrainingScheduleType[] {
           TrainingScheduleType.ZonalDefence, TrainingScheduleType.ZonalDefence,
            TrainingScheduleType.ZonalDefence, TrainingScheduleType.Sprinting,
            TrainingScheduleType.TrainingMatch, TrainingScheduleType.WeightTraining,
            TrainingScheduleType.TrainingMatch
        };
        public static TrainingScheduleType[] ImproveAwarenessScheduleMaxPower = new TrainingScheduleType[] {
           TrainingScheduleType.ZonalDefence, TrainingScheduleType.ZonalDefence,
            TrainingScheduleType.ZonalDefence, TrainingScheduleType.ZonalDefence,
            TrainingScheduleType.ZonalDefence, TrainingScheduleType.ZonalDefence,
            TrainingScheduleType.ZonalDefence
        };
        public static TrainingScheduleType[] ImproveDetermination = new TrainingScheduleType[] {
           TrainingScheduleType.WeightTraining, TrainingScheduleType.WeightTraining,
            TrainingScheduleType.WeightTraining, TrainingScheduleType.WeightTraining,
            TrainingScheduleType.Sprinting, TrainingScheduleType.WeightTraining,
            TrainingScheduleType.WeightTraining
        };
        public static TrainingScheduleType[] ImproveMarking = new TrainingScheduleType[] {
           TrainingScheduleType.Marking, TrainingScheduleType.Marking,
            TrainingScheduleType.Marking, TrainingScheduleType.Marking,
            TrainingScheduleType.Marking, TrainingScheduleType.Marking,
            TrainingScheduleType.TrainingMatch
        };
        public static TrainingScheduleType[] MarkingAllWeek = new TrainingScheduleType[] {
           TrainingScheduleType.Marking, TrainingScheduleType.Marking,
            TrainingScheduleType.Marking, TrainingScheduleType.Marking,
            TrainingScheduleType.Marking, TrainingScheduleType.Marking,
            TrainingScheduleType.TrainingMatch
        };
        public static TrainingScheduleType[] ImproveTacklingSkillAllWeek = new TrainingScheduleType[] {
           TrainingScheduleType.Tackling, TrainingScheduleType.Tackling,
            TrainingScheduleType.Tackling, TrainingScheduleType.Tackling,
            TrainingScheduleType.Tackling, TrainingScheduleType.Tackling,
            TrainingScheduleType.Tackling
        };

        
        public static TrainingScheduleType[] ImproveTacklingSkill = new TrainingScheduleType[] {
           TrainingScheduleType.Tackling, TrainingScheduleType.Tackling,
            TrainingScheduleType.Tackling, TrainingScheduleType.Tackling,
            TrainingScheduleType.Tackling, TrainingScheduleType.Tackling,
            TrainingScheduleType.TrainingMatch
        };
        public static TrainingScheduleType[] ImproveTacklingBalanced = new TrainingScheduleType[] {
           TrainingScheduleType.Marking, TrainingScheduleType.Marking,
            TrainingScheduleType.Marking, TrainingScheduleType.Marking,
            TrainingScheduleType.Tackling, TrainingScheduleType.Tackling,
            TrainingScheduleType.Tackling
        };
        public static TrainingScheduleType[] ImproveTacklingBalancedWithTrainingMatch = new TrainingScheduleType[] {
           TrainingScheduleType.Marking, TrainingScheduleType.Marking,
            TrainingScheduleType.Marking, TrainingScheduleType.Tackling,
            TrainingScheduleType.Tackling, TrainingScheduleType.Tackling,
            TrainingScheduleType.TrainingMatch
        };
        public static TrainingScheduleType[] FiveASideAllWeek = new TrainingScheduleType[] {
           TrainingScheduleType.FiveASide, TrainingScheduleType.FiveASide,
            TrainingScheduleType.FiveASide, TrainingScheduleType.FiveASide,
            TrainingScheduleType.FiveASide, TrainingScheduleType.FiveASide,
            TrainingScheduleType.FiveASide
        };
        public static TrainingScheduleType[] SprintingAllWeek = new TrainingScheduleType[] {
           TrainingScheduleType.Sprinting, TrainingScheduleType.Sprinting,
            TrainingScheduleType.Sprinting, TrainingScheduleType.Sprinting,
            TrainingScheduleType.Sprinting, TrainingScheduleType.Sprinting,
            TrainingScheduleType.Sprinting
        };
        public static TrainingScheduleType[] SprintingWithHeading = new TrainingScheduleType[] {
           TrainingScheduleType.Sprinting, TrainingScheduleType.Sprinting,
            TrainingScheduleType.Sprinting, TrainingScheduleType.Sprinting,
            TrainingScheduleType.Heading, TrainingScheduleType.Heading,
            TrainingScheduleType.Heading
        };

        public static TrainingScheduleType[] SprintingWithWeightTraining = new TrainingScheduleType[] {
           TrainingScheduleType.Sprinting, TrainingScheduleType.Sprinting,
            TrainingScheduleType.Sprinting, TrainingScheduleType.Sprinting,
            TrainingScheduleType.WeightTraining, TrainingScheduleType.WeightTraining,
            TrainingScheduleType.WeightTraining
        };
        public static TrainingScheduleType[] ImproveStrength = new TrainingScheduleType[] {
           TrainingScheduleType.WeightTraining, TrainingScheduleType.WeightTraining,
            TrainingScheduleType.WeightTraining, TrainingScheduleType.WeightTraining,
            TrainingScheduleType.Sprinting, TrainingScheduleType.TrainingMatch,
            TrainingScheduleType.WeightTraining
        };
        public static TrainingScheduleType[] WeightTrainingAllWeek = new TrainingScheduleType[] {
           TrainingScheduleType.WeightTraining, TrainingScheduleType.WeightTraining,
            TrainingScheduleType.WeightTraining, TrainingScheduleType.WeightTraining,
            TrainingScheduleType.WeightTraining, TrainingScheduleType.TrainingMatch,
            TrainingScheduleType.WeightTraining
        };


        public static TrainingScheduleType[] SprintingWithTrainingMatch = new TrainingScheduleType[] {
           TrainingScheduleType.Sprinting, TrainingScheduleType.Sprinting,
            TrainingScheduleType.Sprinting, TrainingScheduleType.Sprinting,
            TrainingScheduleType.TrainingMatch, TrainingScheduleType.TrainingMatch,
            TrainingScheduleType.TrainingMatch
        }; 
        public static TrainingScheduleType[] ShootingAllWeek = new TrainingScheduleType[] {
           TrainingScheduleType.Shooting, TrainingScheduleType.Shooting,
            TrainingScheduleType.Shooting, TrainingScheduleType.Shooting,
            TrainingScheduleType.Shooting, TrainingScheduleType.Shooting,
            TrainingScheduleType.Shooting
        };
        
        public static TrainingScheduleType[] MaintainShape = new TrainingScheduleType[] {
           TrainingScheduleType.WeightTraining, TrainingScheduleType.Kicking,
            TrainingScheduleType.Sprinting, TrainingScheduleType.Handling,
            TrainingScheduleType.TrainingMatch, TrainingScheduleType.Throwing,
            TrainingScheduleType.Control
        };
    }
}
