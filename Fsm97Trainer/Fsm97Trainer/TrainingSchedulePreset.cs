using FSM97Lib;
using System.Collections.Generic;

namespace Fsm97Trainer
{
    internal static class TrainingSchedulePreset
    {
        static TrainingSchedulePreset(){
            defaultPresets = new List<TrainingActivityType[]>()
            {
                DefaultGK,
                DefaultLRB,
                DefaultCD,
                DefaultLRWB,
                DefaultDMSW,
                DefaultLRAMLRW,
                DefaultFRFORSS

            };

                allPresets = new List<TrainingActivityType[]>() {
                SprintingAllWeek,SprintingWithWeightTraining,
                SprintingWithTrainingMatch,
                TrainingMatchAllWeek,GKAgility,
                SprintingWithHeading,
                JoggingAllWeek,
                ExerciseAllWeek,
                ShootingAllWeek,
                PassingAllWeek,

                HeadingAllWeek,ImproveHeading,HeadingWithSprint,
                ControlAllWeek,
                MarkingAllWeek,ImproveMarking,
                TacklingSkillAllWeek,ImproveTacklingSkill,
                ImproveTacklingBalanced,ImproveTacklingBalancedWithTrainingMatch,
                ImproveAwareness,ZonalDefenceAllWeek,
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
            allPresets.AddRange(defaultPresets);
            
            noNegativePresets = new List<TrainingActivityType[]>() {
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
                ImproveAwareness,
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
        static List<TrainingActivityType[]> allPresets;
        public static List<TrainingActivityType[]> AllPresets
        {
            get {
                return allPresets;
            }
        }
        static List<TrainingActivityType[]> noNegativePresets;
        public static List<TrainingActivityType[]> NoNegativePresets
        {
            get
            {
                return noNegativePresets;
            }
        }
        static List<TrainingActivityType[]> defaultPresets;
        public static List<TrainingActivityType[]> DefaultPresets
        {
            get
            {
                return defaultPresets;
            }
        }
        public static TrainingActivityType[] SprintingAllWeek = new TrainingActivityType[] {
           TrainingActivityType.Sprinting, TrainingActivityType.Sprinting,
            TrainingActivityType.Sprinting, TrainingActivityType.Sprinting,
            TrainingActivityType.Sprinting, TrainingActivityType.Sprinting,
            TrainingActivityType.Sprinting
        };
        public static TrainingActivityType[] SprintingWithWeightTraining = new TrainingActivityType[] {
           TrainingActivityType.Sprinting, TrainingActivityType.Sprinting,
            TrainingActivityType.Sprinting, TrainingActivityType.Sprinting,
            TrainingActivityType.WeightTraining, TrainingActivityType.WeightTraining,
            TrainingActivityType.WeightTraining
        };

        public static TrainingActivityType[] SprintingWithTrainingMatch = new TrainingActivityType[] {
           TrainingActivityType.TrainingMatch, TrainingActivityType.Sprinting,
           TrainingActivityType.TrainingMatch, TrainingActivityType.Sprinting,
           TrainingActivityType.TrainingMatch, TrainingActivityType.Sprinting,
           TrainingActivityType.TrainingMatch
        };
        public static TrainingActivityType[] TrainingMatchAllWeek = new TrainingActivityType[] {
            TrainingActivityType.TrainingMatch, TrainingActivityType.TrainingMatch,
            TrainingActivityType.TrainingMatch, TrainingActivityType.TrainingMatch,
            TrainingActivityType.TrainingMatch, TrainingActivityType.TrainingMatch,
            TrainingActivityType.TrainingMatch
        };


        public static TrainingActivityType[] GKAgility = new TrainingActivityType[] {
           TrainingActivityType.GoalKeeping, TrainingActivityType.GoalKeeping,
            TrainingActivityType.GoalKeeping, TrainingActivityType.GoalKeeping,
            TrainingActivityType.Kicking, TrainingActivityType.Throwing,
            TrainingActivityType.TrainingMatch
        };
        public static TrainingActivityType[] SprintingWithHeading = new TrainingActivityType[] {
           TrainingActivityType.Sprinting, TrainingActivityType.Sprinting,
            TrainingActivityType.Sprinting, TrainingActivityType.Sprinting,
            TrainingActivityType.Heading, TrainingActivityType.Heading,
            TrainingActivityType.Heading
        };
        public static TrainingActivityType[] JoggingAllWeek = new TrainingActivityType[] {
           TrainingActivityType.Jogging, TrainingActivityType.Jogging,
            TrainingActivityType.Jogging, TrainingActivityType.Jogging,
            TrainingActivityType.Jogging, TrainingActivityType.Jogging,
            TrainingActivityType.Jogging
        };
        public static TrainingActivityType[] ExerciseAllWeek = new TrainingActivityType[] {
           TrainingActivityType.Exercise, TrainingActivityType.Exercise,
            TrainingActivityType.Exercise, TrainingActivityType.Exercise,
            TrainingActivityType.Exercise, TrainingActivityType.Exercise,
            TrainingActivityType.Exercise
        };

        public static TrainingActivityType[] ShootingAllWeek = new TrainingActivityType[] {
           TrainingActivityType.Shooting, TrainingActivityType.Shooting,
            TrainingActivityType.Shooting, TrainingActivityType.Shooting,
            TrainingActivityType.Shooting, TrainingActivityType.Shooting,
            TrainingActivityType.Shooting
        }; 
        
        public static TrainingActivityType[] PassingAllWeek = new TrainingActivityType[] {
           TrainingActivityType.Passing, TrainingActivityType.Passing,
            TrainingActivityType.Passing, TrainingActivityType.Passing,
            TrainingActivityType.Passing, TrainingActivityType.Passing,
            TrainingActivityType.Passing
        };
        public static TrainingActivityType[] HeadingAllWeek = new TrainingActivityType[] {
           TrainingActivityType.Heading, TrainingActivityType.Heading,
            TrainingActivityType.Heading, TrainingActivityType.Heading,
            TrainingActivityType.Heading, TrainingActivityType.Heading,
            TrainingActivityType.Heading
        };

        public static TrainingActivityType[] HeadingWithSprint= new TrainingActivityType[] {
           TrainingActivityType.Heading, TrainingActivityType.Heading,
            TrainingActivityType.Heading, TrainingActivityType.Heading,
            TrainingActivityType.Heading, TrainingActivityType.Heading,
            TrainingActivityType.Sprinting
        };
        public static TrainingActivityType[] ImproveHeading = new TrainingActivityType[] {
           TrainingActivityType.Heading, TrainingActivityType.Heading,
            TrainingActivityType.Heading, TrainingActivityType.Heading,
            TrainingActivityType.Sprinting, TrainingActivityType.TrainingMatch,
            TrainingActivityType.TrainingMatch
        };

        public static TrainingActivityType[] ControlAllWeek = new TrainingActivityType[] {
            TrainingActivityType.Control, TrainingActivityType.Control,
            TrainingActivityType.Control, TrainingActivityType.Control,
            TrainingActivityType.Control, TrainingActivityType.Control,
            TrainingActivityType.Control

        };
        public static TrainingActivityType[] MarkingAllWeek = new TrainingActivityType[] {
           TrainingActivityType.Marking, TrainingActivityType.Marking,
            TrainingActivityType.Marking, TrainingActivityType.Marking,
            TrainingActivityType.Marking, TrainingActivityType.Marking,
            TrainingActivityType.Marking
        };
        public static TrainingActivityType[] ImproveMarking = new TrainingActivityType[] {
           TrainingActivityType.Marking, TrainingActivityType.Marking,
            TrainingActivityType.Marking, TrainingActivityType.Marking,
            TrainingActivityType.Marking, TrainingActivityType.TrainingMatch,
            TrainingActivityType.TrainingMatch
        };
        public static TrainingActivityType[] TacklingSkillAllWeek = new TrainingActivityType[] {
           TrainingActivityType.Tackling, TrainingActivityType.Tackling,
            TrainingActivityType.Tackling, TrainingActivityType.Tackling,
            TrainingActivityType.Tackling, TrainingActivityType.Tackling,
            TrainingActivityType.Tackling
        };


        public static TrainingActivityType[] ImproveTacklingSkill = new TrainingActivityType[] {
           TrainingActivityType.Tackling, TrainingActivityType.Tackling,
            TrainingActivityType.Tackling, TrainingActivityType.Tackling,
            TrainingActivityType.Tackling, TrainingActivityType.TrainingMatch,
            TrainingActivityType.TrainingMatch
        };
        public static TrainingActivityType[] ImproveTacklingBalanced = new TrainingActivityType[] {
           TrainingActivityType.Marking, TrainingActivityType.Marking,
            TrainingActivityType.Marking, TrainingActivityType.Marking,
            TrainingActivityType.Tackling, TrainingActivityType.Tackling,
            TrainingActivityType.Tackling
        };
        public static TrainingActivityType[] ImproveTacklingBalancedWithTrainingMatch = new TrainingActivityType[] {
           TrainingActivityType.Marking, TrainingActivityType.Marking,
            TrainingActivityType.Marking, TrainingActivityType.Tackling,
            TrainingActivityType.Tackling, TrainingActivityType.Tackling,
            TrainingActivityType.TrainingMatch
        };
        public static TrainingActivityType[] ImproveAwareness = new TrainingActivityType[] {
           TrainingActivityType.ZonalDefence, TrainingActivityType.ZonalDefence,
            TrainingActivityType.ZonalDefence, TrainingActivityType.ZonalDefence,
            TrainingActivityType.ZonalDefence, TrainingActivityType.TrainingMatch,
            TrainingActivityType.TrainingMatch
        };
        public static TrainingActivityType[] ZonalDefenceAllWeek = new TrainingActivityType[] {
           TrainingActivityType.ZonalDefence, TrainingActivityType.ZonalDefence,
            TrainingActivityType.ZonalDefence, TrainingActivityType.ZonalDefence,
            TrainingActivityType.ZonalDefence, TrainingActivityType.ZonalDefence,
            TrainingActivityType.ZonalDefence
        };
        public static TrainingActivityType[] FiveASideAllWeek = new TrainingActivityType[] {
           TrainingActivityType.FiveASide, TrainingActivityType.FiveASide,
            TrainingActivityType.FiveASide, TrainingActivityType.FiveASide,
            TrainingActivityType.FiveASide, TrainingActivityType.FiveASide,
            TrainingActivityType.FiveASide
        };
        public static TrainingActivityType[] KickingAllWeek = new TrainingActivityType[] {
           TrainingActivityType.Kicking, TrainingActivityType.Kicking,
            TrainingActivityType.Kicking, TrainingActivityType.Kicking,
            TrainingActivityType.Kicking, TrainingActivityType.Kicking,
            TrainingActivityType.Kicking

        };
        public static TrainingActivityType[] ImproveKicking = new TrainingActivityType[] {
           TrainingActivityType.Kicking, TrainingActivityType.Kicking,
            TrainingActivityType.Kicking, TrainingActivityType.Kicking,
            TrainingActivityType.Kicking, TrainingActivityType.Throwing,
            TrainingActivityType.Kicking

        };
        public static TrainingActivityType[] ThrowingAllWeek = new TrainingActivityType[] {
           TrainingActivityType.Throwing, TrainingActivityType.Throwing,
            TrainingActivityType.Throwing, TrainingActivityType.Throwing,
            TrainingActivityType.Throwing, TrainingActivityType.Throwing,
            TrainingActivityType.Throwing
        };
        public static TrainingActivityType[] GoalkeepingAllWeek = new TrainingActivityType[] {
           TrainingActivityType.GoalKeeping, TrainingActivityType.GoalKeeping,
            TrainingActivityType.GoalKeeping, TrainingActivityType.GoalKeeping,
            TrainingActivityType.GoalKeeping, TrainingActivityType.GoalKeeping,
            TrainingActivityType.GoalKeeping
        };

        public static TrainingActivityType[] ImproveGoalkeeping = new TrainingActivityType[] {
           TrainingActivityType.GoalKeeping, TrainingActivityType.GoalKeeping,
            TrainingActivityType.GoalKeeping, TrainingActivityType.GoalKeeping,
            TrainingActivityType.GoalKeeping, TrainingActivityType.GoalKeeping,
            TrainingActivityType.Kicking
        };
        public static TrainingActivityType[] HandlingAllWeek = new TrainingActivityType[] {
           TrainingActivityType.Handling, TrainingActivityType.Handling,
            TrainingActivityType.Handling, TrainingActivityType.Handling,
            TrainingActivityType.Handling, TrainingActivityType.Handling,
            TrainingActivityType.Handling
        };
        public static TrainingActivityType[] ImproveHandling = new TrainingActivityType[] {
           TrainingActivityType.Handling, TrainingActivityType.Handling,
            TrainingActivityType.Handling, TrainingActivityType.Handling,
            TrainingActivityType.Handling, TrainingActivityType.Kicking,
            TrainingActivityType.Handling
        };

        public static TrainingActivityType[] WeightTrainingAllWeek = new TrainingActivityType[] {
           TrainingActivityType.WeightTraining, TrainingActivityType.WeightTraining,
            TrainingActivityType.WeightTraining, TrainingActivityType.WeightTraining,
            TrainingActivityType.WeightTraining, TrainingActivityType.WeightTraining,
            TrainingActivityType.WeightTraining
        };
        public static TrainingActivityType[] ImproveDetermination = new TrainingActivityType[] {
           TrainingActivityType.WeightTraining, TrainingActivityType.WeightTraining,
            TrainingActivityType.WeightTraining, TrainingActivityType.WeightTraining,
            TrainingActivityType.Sprinting, TrainingActivityType.WeightTraining,
            TrainingActivityType.WeightTraining
        };
        public static TrainingActivityType[] ImproveStrength = new TrainingActivityType[] {
           TrainingActivityType.WeightTraining, TrainingActivityType.WeightTraining,
            TrainingActivityType.WeightTraining, TrainingActivityType.WeightTraining,
            TrainingActivityType.Sprinting, TrainingActivityType.TrainingMatch,
            TrainingActivityType.WeightTraining
        };

        public static TrainingActivityType[] BalancedSpeedAndSkill = new TrainingActivityType[] {
           TrainingActivityType.Sprinting,TrainingActivityType.TrainingMatch,
           TrainingActivityType.Sprinting,TrainingActivityType.TrainingMatch, 
           TrainingActivityType.WeightTraining, TrainingActivityType.Sprinting,
           TrainingActivityType.FiveASide
        };

        public static TrainingActivityType[] MaintainShape = new TrainingActivityType[] {
           TrainingActivityType.WeightTraining, TrainingActivityType.Kicking,
            TrainingActivityType.Sprinting, TrainingActivityType.Handling,
            TrainingActivityType.TrainingMatch, TrainingActivityType.Throwing,
            TrainingActivityType.Control
        };
        public static TrainingActivityType[] None = new TrainingActivityType[] {
           TrainingActivityType.None, TrainingActivityType.None,
            TrainingActivityType.None, TrainingActivityType.None,
            TrainingActivityType.None, TrainingActivityType.None,
            TrainingActivityType.None
        };
        static TrainingActivityType[] DefaultGK = new TrainingActivityType[] {
           TrainingActivityType.Sprinting, TrainingActivityType.Control,
            TrainingActivityType.Handling, TrainingActivityType.Kicking,
            TrainingActivityType.Kicking, TrainingActivityType.Throwing,
            TrainingActivityType.TrainingMatch
        };
        static TrainingActivityType[] DefaultLRB = new TrainingActivityType[] {
           TrainingActivityType.WeightTraining, TrainingActivityType.WeightTraining,
            TrainingActivityType.WeightTraining, TrainingActivityType.WeightTraining,
            TrainingActivityType.Sprinting, TrainingActivityType.Heading,
            TrainingActivityType.TrainingMatch
        };
        static TrainingActivityType[] DefaultCD = new TrainingActivityType[] {
           TrainingActivityType.Sprinting, TrainingActivityType.Heading,
            TrainingActivityType.Sprinting, TrainingActivityType.Sprinting,
            TrainingActivityType.Control, TrainingActivityType.Control,
            TrainingActivityType.TrainingMatch
        };
        static TrainingActivityType[] DefaultLRWB = new TrainingActivityType[] {
           TrainingActivityType.Sprinting, TrainingActivityType.TrainingMatch,
            TrainingActivityType.Control, TrainingActivityType.Marking,
            TrainingActivityType.Sprinting, TrainingActivityType.TrainingMatch,
            TrainingActivityType.Control
        };
        static TrainingActivityType[] DefaultDMSW = new TrainingActivityType[] {
           TrainingActivityType.Sprinting, TrainingActivityType.Heading,
            TrainingActivityType.Sprinting, TrainingActivityType.Control,
            TrainingActivityType.Marking, TrainingActivityType.TrainingMatch,
            TrainingActivityType.TrainingMatch
        };


        static TrainingActivityType[] DefaultLRAMLRW = new TrainingActivityType[] {
           TrainingActivityType.Sprinting, TrainingActivityType.Tackling,
            TrainingActivityType.TrainingMatch, TrainingActivityType.Sprinting,
            TrainingActivityType.Tackling, TrainingActivityType.TrainingMatch,
            TrainingActivityType.TrainingMatch
        };
        static TrainingActivityType[] DefaultFRFORSS = new TrainingActivityType[] {
           TrainingActivityType.Sprinting, TrainingActivityType.Heading,
            TrainingActivityType.TrainingMatch, TrainingActivityType.Sprinting,
            TrainingActivityType.Heading, TrainingActivityType.TrainingMatch,
            TrainingActivityType.Control
        };


        public static TrainingActivityType[] GetDefaultTrainingSchedule(PlayerPosition playerPosition, TrainingEffectModifier trainingEffectModifier)
        {
            switch (playerPosition)
            {
                case PlayerPosition.GK:
                    return DefaultGK;
                case PlayerPosition.RB:
                case PlayerPosition.LB:
                    return DefaultLRB;
                case PlayerPosition.CD:
                    return DefaultCD;
                case PlayerPosition.RWB:
                case PlayerPosition.LWB:
                    return DefaultLRWB;
                case PlayerPosition.SW:
                case PlayerPosition.DM:
                    return DefaultDMSW;
                case PlayerPosition.RM:
                case PlayerPosition.LM:
                case PlayerPosition.AM:
                case PlayerPosition.RW:
                case PlayerPosition.LW:
                    return DefaultLRAMLRW;
                case PlayerPosition.FR:
                case PlayerPosition.FOR:
                case PlayerPosition.SS:
                    return DefaultFRFORSS;
                default:
                    return null;
            }
        }
    }
}
