using FSM97Lib;
using System.Collections.Generic;

namespace Fsm97Trainer
{
    internal static class TrainingSchedulePreset
    {
        static TrainingSchedulePreset(){

            allPresets = new List<TrainingScheduleType[]>() {
                SprintingAllWeek,SprintingWithWeightTraining,
                SprintingWithTrainingMatch,
                TrainingMatchAllWeek,GKAgility,
                SprintingWithHeading,
                JoggingAllWeek,
                ExerciseAllWeek,
                ShootingAllWeek,
                PassingAllWeek,

                HeadingAllWeek,ImproveHeading,
                ControlAllWeek,
                MarkingAllWeek,ImproveMarking,
                TacklingSkillAllWeek,ImproveTacklingSkill,
                ImproveTacklingBalanced,ImproveTacklingBalancedWithTrainingMatch,
                ImproveAwarenessSchedule,ImproveAwarenessScheduleMaxPower,
                FiveASideAllWeek,
                KickingAllWeek,ImproveKicking,
                ThrowingAllWeek,
                GoalkeepingAllWeek,ImproveGoalkeeping,
                HandlingAllWeek,ImproveHandling,
                WeightTrainingAllWeek,ImproveDetermination,
                ImproveStrength,
                BalancedSpeedAndSkill,
                MaintainShape,
                None
            };
            noNegativePresets= new List<TrainingScheduleType[]>() {
                SprintingWithWeightTraining,
                SprintingWithTrainingMatch,
                TrainingMatchAllWeek,GKAgility,
                SprintingWithHeading,                
                ShootingAllWeek,
                PassingAllWeek,
                ImproveHeading,
                ControlAllWeek,
                ImproveMarking,
                ImproveTacklingSkill,
                ImproveTacklingBalanced,ImproveTacklingBalancedWithTrainingMatch,
                ImproveAwarenessSchedule,
                FiveASideAllWeek,
                ImproveKicking,
                ThrowingAllWeek,
                ImproveGoalkeeping,
                ImproveHandling,
                ImproveDetermination,
                ImproveStrength,
                BalancedSpeedAndSkill,
                MaintainShape,
                None
            };
        }
        static List<TrainingScheduleType[]> allPresets;
        public static List<TrainingScheduleType[]> AllPresets
        {
            get {
                return allPresets;
            }
        }
        static List<TrainingScheduleType[]> noNegativePresets;
        public static List<TrainingScheduleType[]> NoNegativePresets
        {
            get
            {
                return noNegativePresets;
            }
        }
        public static TrainingScheduleType[] SprintingAllWeek = new TrainingScheduleType[] {
           TrainingScheduleType.Sprinting, TrainingScheduleType.Sprinting,
            TrainingScheduleType.Sprinting, TrainingScheduleType.Sprinting,
            TrainingScheduleType.Sprinting, TrainingScheduleType.Sprinting,
            TrainingScheduleType.Sprinting
        };
        public static TrainingScheduleType[] SprintingWithWeightTraining = new TrainingScheduleType[] {
           TrainingScheduleType.Sprinting, TrainingScheduleType.Sprinting,
            TrainingScheduleType.Sprinting, TrainingScheduleType.Sprinting,
            TrainingScheduleType.WeightTraining, TrainingScheduleType.WeightTraining,
            TrainingScheduleType.WeightTraining
        };

        public static TrainingScheduleType[] SprintingWithTrainingMatch = new TrainingScheduleType[] {
           TrainingScheduleType.TrainingMatch, TrainingScheduleType.Sprinting,
           TrainingScheduleType.TrainingMatch, TrainingScheduleType.Sprinting,
           TrainingScheduleType.TrainingMatch, TrainingScheduleType.Sprinting,
           TrainingScheduleType.TrainingMatch
        };
        public static TrainingScheduleType[] TrainingMatchAllWeek = new TrainingScheduleType[] {
            TrainingScheduleType.TrainingMatch, TrainingScheduleType.TrainingMatch,
            TrainingScheduleType.TrainingMatch, TrainingScheduleType.TrainingMatch,
            TrainingScheduleType.TrainingMatch, TrainingScheduleType.TrainingMatch,
            TrainingScheduleType.TrainingMatch
        };


        public static TrainingScheduleType[] GKAgility = new TrainingScheduleType[] {
           TrainingScheduleType.GoalKeeping, TrainingScheduleType.GoalKeeping,
            TrainingScheduleType.GoalKeeping, TrainingScheduleType.GoalKeeping,
            TrainingScheduleType.Kicking, TrainingScheduleType.Throwing,
            TrainingScheduleType.TrainingMatch
        };
        public static TrainingScheduleType[] SprintingWithHeading = new TrainingScheduleType[] {
           TrainingScheduleType.Sprinting, TrainingScheduleType.Sprinting,
            TrainingScheduleType.Sprinting, TrainingScheduleType.Sprinting,
            TrainingScheduleType.Heading, TrainingScheduleType.Heading,
            TrainingScheduleType.Heading
        };
        public static TrainingScheduleType[] JoggingAllWeek = new TrainingScheduleType[] {
           TrainingScheduleType.Jogging, TrainingScheduleType.Jogging,
            TrainingScheduleType.Jogging, TrainingScheduleType.Jogging,
            TrainingScheduleType.Jogging, TrainingScheduleType.Jogging,
            TrainingScheduleType.Jogging
        };
        public static TrainingScheduleType[] ExerciseAllWeek = new TrainingScheduleType[] {
           TrainingScheduleType.Exercise, TrainingScheduleType.Exercise,
            TrainingScheduleType.Exercise, TrainingScheduleType.Exercise,
            TrainingScheduleType.Exercise, TrainingScheduleType.Exercise,
            TrainingScheduleType.Exercise
        };

        public static TrainingScheduleType[] ShootingAllWeek = new TrainingScheduleType[] {
           TrainingScheduleType.Shooting, TrainingScheduleType.Shooting,
            TrainingScheduleType.Shooting, TrainingScheduleType.Shooting,
            TrainingScheduleType.Shooting, TrainingScheduleType.Shooting,
            TrainingScheduleType.Shooting
        }; 
        
        public static TrainingScheduleType[] PassingAllWeek = new TrainingScheduleType[] {
           TrainingScheduleType.Passing, TrainingScheduleType.Passing,
            TrainingScheduleType.Passing, TrainingScheduleType.Passing,
            TrainingScheduleType.Passing, TrainingScheduleType.Passing,
            TrainingScheduleType.Passing
        };
        public static TrainingScheduleType[] HeadingAllWeek = new TrainingScheduleType[] {
           TrainingScheduleType.Heading, TrainingScheduleType.Heading,
            TrainingScheduleType.Heading, TrainingScheduleType.Heading,
            TrainingScheduleType.Heading, TrainingScheduleType.Heading,
            TrainingScheduleType.Heading
        };

        public static TrainingScheduleType[] ImproveHeading = new TrainingScheduleType[] {
           TrainingScheduleType.Heading, TrainingScheduleType.Heading,
            TrainingScheduleType.Heading, TrainingScheduleType.Heading,
            TrainingScheduleType.Sprinting, TrainingScheduleType.Heading,
            TrainingScheduleType.Heading
        };

        public static TrainingScheduleType[] ControlAllWeek = new TrainingScheduleType[] {
            TrainingScheduleType.Control, TrainingScheduleType.Control,
            TrainingScheduleType.Control, TrainingScheduleType.Control,
            TrainingScheduleType.Control, TrainingScheduleType.Control,
            TrainingScheduleType.Control

        };
        public static TrainingScheduleType[] MarkingAllWeek = new TrainingScheduleType[] {
           TrainingScheduleType.Marking, TrainingScheduleType.Marking,
            TrainingScheduleType.Marking, TrainingScheduleType.Marking,
            TrainingScheduleType.Marking, TrainingScheduleType.Marking,
            TrainingScheduleType.Marking
        };
        public static TrainingScheduleType[] ImproveMarking = new TrainingScheduleType[] {
           TrainingScheduleType.Marking, TrainingScheduleType.Marking,
            TrainingScheduleType.Marking, TrainingScheduleType.Marking,
            TrainingScheduleType.Marking, TrainingScheduleType.TrainingMatch,
            TrainingScheduleType.TrainingMatch
        };
        public static TrainingScheduleType[] TacklingSkillAllWeek = new TrainingScheduleType[] {
           TrainingScheduleType.Tackling, TrainingScheduleType.Tackling,
            TrainingScheduleType.Tackling, TrainingScheduleType.Tackling,
            TrainingScheduleType.Tackling, TrainingScheduleType.Tackling,
            TrainingScheduleType.Tackling
        };


        public static TrainingScheduleType[] ImproveTacklingSkill = new TrainingScheduleType[] {
           TrainingScheduleType.Tackling, TrainingScheduleType.Tackling,
            TrainingScheduleType.Tackling, TrainingScheduleType.Tackling,
            TrainingScheduleType.Tackling, TrainingScheduleType.TrainingMatch,
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
        public static TrainingScheduleType[] FiveASideAllWeek = new TrainingScheduleType[] {
           TrainingScheduleType.FiveASide, TrainingScheduleType.FiveASide,
            TrainingScheduleType.FiveASide, TrainingScheduleType.FiveASide,
            TrainingScheduleType.FiveASide, TrainingScheduleType.FiveASide,
            TrainingScheduleType.FiveASide
        };
        public static TrainingScheduleType[] KickingAllWeek = new TrainingScheduleType[] {
           TrainingScheduleType.Kicking, TrainingScheduleType.Kicking,
            TrainingScheduleType.Kicking, TrainingScheduleType.Kicking,
            TrainingScheduleType.Kicking, TrainingScheduleType.Kicking,
            TrainingScheduleType.Kicking

        };
        public static TrainingScheduleType[] ImproveKicking = new TrainingScheduleType[] {
           TrainingScheduleType.Kicking, TrainingScheduleType.Kicking,
            TrainingScheduleType.Kicking, TrainingScheduleType.Kicking,
            TrainingScheduleType.Kicking, TrainingScheduleType.Throwing,
            TrainingScheduleType.Kicking

        };
        public static TrainingScheduleType[] ThrowingAllWeek = new TrainingScheduleType[] {
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

        public static TrainingScheduleType[] ImproveGoalkeeping = new TrainingScheduleType[] {
           TrainingScheduleType.GoalKeeping, TrainingScheduleType.GoalKeeping,
            TrainingScheduleType.GoalKeeping, TrainingScheduleType.GoalKeeping,
            TrainingScheduleType.GoalKeeping, TrainingScheduleType.GoalKeeping,
            TrainingScheduleType.Kicking
        };
        public static TrainingScheduleType[] HandlingAllWeek = new TrainingScheduleType[] {
           TrainingScheduleType.Handling, TrainingScheduleType.Handling,
            TrainingScheduleType.Handling, TrainingScheduleType.Handling,
            TrainingScheduleType.Handling, TrainingScheduleType.Handling,
            TrainingScheduleType.Handling
        };
        public static TrainingScheduleType[] ImproveHandling = new TrainingScheduleType[] {
           TrainingScheduleType.Handling, TrainingScheduleType.Handling,
            TrainingScheduleType.Handling, TrainingScheduleType.Handling,
            TrainingScheduleType.Handling, TrainingScheduleType.Kicking,
            TrainingScheduleType.Handling
        };

        public static TrainingScheduleType[] WeightTrainingAllWeek = new TrainingScheduleType[] {
           TrainingScheduleType.WeightTraining, TrainingScheduleType.WeightTraining,
            TrainingScheduleType.WeightTraining, TrainingScheduleType.WeightTraining,
            TrainingScheduleType.WeightTraining, TrainingScheduleType.WeightTraining,
            TrainingScheduleType.WeightTraining
        };
        public static TrainingScheduleType[] ImproveDetermination = new TrainingScheduleType[] {
           TrainingScheduleType.WeightTraining, TrainingScheduleType.WeightTraining,
            TrainingScheduleType.WeightTraining, TrainingScheduleType.WeightTraining,
            TrainingScheduleType.Sprinting, TrainingScheduleType.WeightTraining,
            TrainingScheduleType.WeightTraining
        };
        public static TrainingScheduleType[] ImproveStrength = new TrainingScheduleType[] {
           TrainingScheduleType.WeightTraining, TrainingScheduleType.WeightTraining,
            TrainingScheduleType.WeightTraining, TrainingScheduleType.WeightTraining,
            TrainingScheduleType.Sprinting, TrainingScheduleType.TrainingMatch,
            TrainingScheduleType.WeightTraining
        };

        public static TrainingScheduleType[] BalancedSpeedAndSkill = new TrainingScheduleType[] {
           TrainingScheduleType.Sprinting,TrainingScheduleType.TrainingMatch,
           TrainingScheduleType.Sprinting,TrainingScheduleType.TrainingMatch, 
           TrainingScheduleType.WeightTraining, TrainingScheduleType.Sprinting,
           TrainingScheduleType.FiveASide
        };

        public static TrainingScheduleType[] MaintainShape = new TrainingScheduleType[] {
           TrainingScheduleType.WeightTraining, TrainingScheduleType.Kicking,
            TrainingScheduleType.Sprinting, TrainingScheduleType.Handling,
            TrainingScheduleType.TrainingMatch, TrainingScheduleType.Throwing,
            TrainingScheduleType.Control
        };
        public static TrainingScheduleType[] None = new TrainingScheduleType[] {
           TrainingScheduleType.None, TrainingScheduleType.None,
            TrainingScheduleType.None, TrainingScheduleType.None,
            TrainingScheduleType.None, TrainingScheduleType.None,
            TrainingScheduleType.None
        };
    }
}
