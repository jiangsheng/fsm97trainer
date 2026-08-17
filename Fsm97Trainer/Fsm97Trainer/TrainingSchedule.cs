using FSM97Lib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using System.Web.UI.WebControls;
using System.Windows.Forms;

namespace Fsm97Trainer
{

    public static class TrainingSchedule
    {
        static int[] stages =
            Enumerable.Range(1, 75).Select(x => x + 24).ToArray();
        public const int attributeCap = 99;
        public const int almostAttributeCap = 85;
        public const double ConstantFastCeiling = 0.999999;
        internal static List<TrainingScheduleSteps> GetTrainingSchedule(TrainingScheduleCalculationState trainingScheduleCalculationState)
        {
            trainingScheduleCalculationState.UpdateProjectedAttributesAfterSprinting();
            trainingScheduleCalculationState.UpdateProjectedAttributesAfterTrainingMatch();
            trainingScheduleCalculationState.UpdateAttributesLeftToTrain();
            trainingScheduleCalculationState.UpdateBottleneckAttributes();

            var player = trainingScheduleCalculationState.Player;
            List<TrainingScheduleSteps> schedule;
            //is this a player a different best position only training as goalkeeper to avoid injury?
            if (trainingScheduleCalculationState.RevertFromGK && player.Fitness < attributeCap && player.Position == (byte)PlayerPosition.GK
                && player.BestPosition != (byte)PlayerPosition.GK)
            {
                //train for the player's real position
                schedule = GetTrainingSchedule(trainingScheduleCalculationState, (PlayerPosition)player.BestPosition);
            }
            else
            {
                //train for the game player's preferred position
                schedule = GetTrainingSchedule(trainingScheduleCalculationState, (PlayerPosition)player.Position);
            }
            if (schedule != null)
            {
                //see physiotherapist if injuries will not be auto reset
                //training injury happens due to low energy
                if (!trainingScheduleCalculationState.AutoResetStatus && !trainingScheduleCalculationState.MaxEnergy && !trainingScheduleCalculationState.TrainingEffectModifier.RemoveNegativeTraining)
                {
                    //Only for on-field players
                    //gks almost never get injured so exclude them
                    //fitness < attributeCap means can be injured in training.
                    //fitness = attributeCap can still injured in training once in a blue moon but we ignore that. 
                    if (player.Status == 0 && player.Position != (byte)PlayerPosition.GK && player.Fitness < attributeCap)
                    {
                        schedule[1].TrainingScheduleType = schedule[3].TrainingScheduleType = schedule[5].TrainingScheduleType = TrainingScheduleType.Physiotherapist;
                    }
                }
            }
            return schedule;
        }

        public static List<TrainingScheduleSteps> GetTrainingSchedule(TrainingScheduleCalculationState trainingScheduleCalculationState,
           PlayerPosition position)
        {
            var player = trainingScheduleCalculationState.Player;
            var trainingEffectModifier= trainingScheduleCalculationState.TrainingEffectModifier;
            var trainingEffects = trainingScheduleCalculationState.TrainingEffects;
            var projectedAttributesAfterSprinting = trainingScheduleCalculationState.ProjectedAttributesAfterSprinting;
            var projectedAttributesAfterTrainingMatch = trainingScheduleCalculationState.ProjectedAttributesAfterTrainingMatch;

            bool trainAcceleration = PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Acceleration] > 0;
            bool trainingShooting = position == PlayerPosition.Count ? true : PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Shooting] > 0;
            bool trainingPassing = position == PlayerPosition.Count ? true : PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Passing] > 0;
            bool trainHeading = PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Heading] > 0;

            bool trainControl = position == PlayerPosition.Count ? true : PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Control] > 0;
            bool trainDribbling = position == PlayerPosition.Count ? true : PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Dribbling] > 0;


            bool trainTackleSkill = PositionRatings.Ratings[(int)position][(int)PlayerAttribute.TackleSkill] > 0;
            bool trainTackleDetermination = PositionRatings.Ratings[(int)position][(int)PlayerAttribute.TackleDetermination] > 0;

            bool trainCoolness = position == PlayerPosition.Count ? true : PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Coolness] > 0;
            bool trainFlair = position == PlayerPosition.Count ? true : PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Flair] > 0;

            bool trainDetermination = position == PlayerPosition.Count ? true : PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Determination] > 0;
            var alwaysTrainConsistency = trainingScheduleCalculationState.AlwaysTrainConsistency;
            var genericTrainingGrind = trainingScheduleCalculationState.GenericTrainingGrind;
            var genericTrainingCounter=trainingScheduleCalculationState.GenericTrainingCounter;

            if (player.Fitness < attributeCap) return ImproveFitness(player, trainAcceleration, trainControl);

            if (trainDetermination && player.Determination < attributeCap)
            {
                return ImproveDeterminationTo(player, attributeCap, trainingEffectModifier);
            }

            if (position == PlayerPosition.GK)
            {
                var result= ImproveGoalkeepingTo(trainingScheduleCalculationState);
                
                if (result != null)
                {
                    Debug.Assert(result[0].TrainingScheduleType != TrainingScheduleType.TrainingMatch);
                    return result;
                }
            }
            if (player.Speed < attributeCap-1)
            {
                return ImproveSpeedTo(player, attributeCap-1, false, trainingEffectModifier);
            }
            if (player.Acceleration < almostAttributeCap )
            {
                return ImproveAccelerationTo(player, almostAttributeCap, false, trainingEffectModifier);
            }
            if (position == PlayerPosition.GK)
            {
                if (player.Consistency < attributeCap)
                    return ImproveConsistencyTo(player, attributeCap);
            }

            if (!ShouldStopDefaultTraining(player, position))
            {
                return TrainingSchedulePreset.GetDefaultTrainingSchedule(position, trainingEffectModifier).Select(x=>new TrainingScheduleSteps {TrainingScheduleType=x,ForPlayerAttribute=PlayerAttribute.Count }).ToList();
            }
            //this often overflow and is safe to max out first
            if (trainingShooting && player.Shooting<attributeCap-1)
            {
                return ImproveShootingTo(player, attributeCap - 1);
            }
            return GenericTraining(trainingScheduleCalculationState, position,null,null);
            /*
            switch (position)
            {
                case PlayerPosition.GK: return GetGKTrainingSchedule(player, maxPower, trainingEffectModifier, trainingEffects,projectedAttributesAfterSprinting,projectedAttributesAfterTrainingMatch);
                case PlayerPosition.LB:
                case PlayerPosition.RB: return GetLRBTrainingSchedule(player, maxPower, trainingEffectModifier,trainingEffects);
                case PlayerPosition.CD: return GetCDTrainingSchedule(player, maxPower, trainingEffectModifier,trainingEffects);
                case PlayerPosition.DM: return GetDMTrainingSchedule(player, maxPower, trainingEffectModifier,trainingEffects);
                case PlayerPosition.SW: return GetSWTrainingSchedule(player, maxPower, trainingEffectModifier, trainingEffects);

                case PlayerPosition.LWB:
                case PlayerPosition.RWB:
                    return GetLRWBTrainingSchedule(player, maxPower, trainingEffectModifier
                        ,trainingEffects);
                case PlayerPosition.LM:
                case PlayerPosition.RM:
                case PlayerPosition.AM:
                    return GetLRAMTrainingSchedule(player, position, maxPower, trainingEffectModifier, trainingEffects);
                case PlayerPosition.LW:
                case PlayerPosition.RW: return GetLRWTrainingSchedule(player, maxPower, trainingEffectModifier, trainingEffects);
                case PlayerPosition.FR: return GetFRTrainingSchedule(player, maxPower, trainingEffectModifier, trainingEffects);
                case PlayerPosition.SS: return GetFORSSTrainingSchedule(player, position, maxPower, trainingEffectModifier, trainingEffects);
                case PlayerPosition.FOR: return GetFORSSTrainingSchedule(player, position, maxPower, trainingEffectModifier, trainingEffects);
                default: return null;
            }*/
        }

        private static List<TrainingScheduleSteps> ImproveGoalkeepingTo(TrainingScheduleCalculationState trainingScheduleCalculationState)
        {
            var player = trainingScheduleCalculationState.Player;
            List<double> doubles = Enumerable.Repeat<double>(0, player.Attributes.Length).ToList();

            PlayerModelDouble attributeLeftToTrain = new PlayerModelDouble(player, doubles);
            attributeLeftToTrain.Handling = attributeCap - player.Handling;
            attributeLeftToTrain.Throwing = attributeCap - player.Throwing;
            attributeLeftToTrain.Kicking = attributeCap - player.Kicking;
            if (attributeLeftToTrain.Handling <= 0 && attributeLeftToTrain.Throwing <= 0 && attributeLeftToTrain.Kicking <= 0)
                return null;
            var bottleneckAttributeIndex = trainingScheduleCalculationState.GetBottleneckAttributes(PlayerPosition.GK, attributeLeftToTrain);
            
            var playerSchedule = GenericTraining(trainingScheduleCalculationState, PlayerPosition.GK, attributeLeftToTrain,bottleneckAttributeIndex);
            return playerSchedule;
        }
        #region generic training

        //static List<TrainingScheduleSteps> genericTrainingGrind = new List<TrainingScheduleSteps>(20);
        //static List<TrainingScheduleSteps> genericTrainingCounter = new List<TrainingScheduleSteps>(20);
        public static List<TrainingScheduleSteps> GenericTraining(TrainingScheduleCalculationState trainingScheduleCalculationState, PlayerPosition position, PlayerModelDouble attributeLeftToTrain, List<BottleneckAttributes> bottleneckAttributeIndex)
        {            
            var genericTrainingGrind = trainingScheduleCalculationState.GenericTrainingGrind;
            var genericTrainingCounter = trainingScheduleCalculationState.GenericTrainingCounter;
            var trainingEffectModifier=trainingScheduleCalculationState.TrainingEffectModifier;

            bool trainAcceleration = position==PlayerPosition.Count?true:PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Acceleration] > 0;
            
            bool trainingShooting = position == PlayerPosition.Count ? true : PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Shooting] > 0;
            bool trainingPassing = position == PlayerPosition.Count ? true : PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Passing] > 0;
            bool trainHeading = position == PlayerPosition.Count ? true : PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Heading] > 0;
            bool trainControl= position == PlayerPosition.Count ? true : PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Control] > 0;
            bool trainDribbling = position == PlayerPosition.Count ? true : PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Dribbling] > 0;
            
            bool trainTackleSkill = position == PlayerPosition.Count ? true : PositionRatings.Ratings[(int)position][(int)PlayerAttribute.TackleSkill] > 0;
            bool trainTackleDetermination = position == PlayerPosition.Count ? true : PositionRatings.Ratings[(int)position][(int)PlayerAttribute.TackleDetermination] > 0;
            bool trainCoolness= position == PlayerPosition.Count ? true : PositionRatings.Ratings[(int) position][(int)PlayerAttribute.Coolness] > 0;
            bool trainFlair = position == PlayerPosition.Count ? true : PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Flair] > 0;

            bool trainDetermination = position == PlayerPosition.Count ? true : PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Determination] > 0;

            if (attributeLeftToTrain == null && bottleneckAttributeIndex == null)
            {
                //gk training only
                attributeLeftToTrain = trainingScheduleCalculationState.AttributesLeftToTrain;
                bottleneckAttributeIndex = trainingScheduleCalculationState.BottleneckAttributes;

                if (AreAllRevelantAttribtesAboveAlmostCap(trainingScheduleCalculationState.Player, position, trainingEffectModifier))
                {
                    return GetFinalTrainingSchedule(trainingScheduleCalculationState, position);
                }
            }
            var projectedAttributesAfterTrainingMatch = trainingScheduleCalculationState.ProjectedAttributesAfterTrainingMatch;
            var player = trainingScheduleCalculationState.Player;
            genericTrainingGrind.Clear();
            genericTrainingCounter.Clear();


            var flairLeftToTrain = attributeLeftToTrain.Flair;
            if (bottleneckAttributeIndex != null)
            {
                bool hasTackleSkill = false;
                bool hasTackleDetermination= false;
                for (int i = 0; i < bottleneckAttributeIndex.Count; i++)
                {
                    if (genericTrainingGrind.Count + genericTrainingCounter.Count > 7)
                        break;
                    var topBottleneckAttribute = bottleneckAttributeIndex[i];
                    var repeat = topBottleneckAttribute.Repeat;

                    switch (topBottleneckAttribute.AttributeIndex)
                    {
                        case PlayerAttribute.Speed:
                        case PlayerAttribute.Acceleration:

                            AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.Sprinting);
                            AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingScheduleType.TrainingMatch, trainingEffectModifier);

                            break;
                        case PlayerAttribute.Agility:

                            if (player.Position != (int)PlayerPosition.GK)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.TrainingMatch);
                            }
                            else
                            {
                                if (player.Kicking >= TrainingSchedule.attributeCap)
                                    AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.TrainingMatch);
                                else
                                {
                                    //it is usually not good to maxed this last
                                    AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.GoalKeeping);
                                    AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingScheduleType.Kicking, trainingEffectModifier);
                                    AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingScheduleType.Throwing, trainingEffectModifier);
                                    AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingScheduleType.TrainingMatch, trainingEffectModifier);
                                }
                            }
                            break;
                        case PlayerAttribute.Shooting:                        
                        case PlayerAttribute.Leadership:
                            AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.TrainingMatch);
                            break;
                        case PlayerAttribute.Passing:
                            if (trainingShooting && attributeLeftToTrain.Attributes[(int)PlayerAttribute.Shooting] > 0)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.TrainingMatch);
                            }
                            else if (trainHeading && attributeLeftToTrain.Attributes[(int)PlayerAttribute.Heading] > 0)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.TrainingMatch);
                            }
                            else if (trainTackleDetermination && attributeLeftToTrain.Attributes[(int)PlayerAttribute.TackleDetermination] > 0)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.TrainingMatch);
                            }
                            else if (trainCoolness && attributeLeftToTrain.Attributes[(int)PlayerAttribute.Coolness] > 0)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.TrainingMatch);
                            }
                            else
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.FiveASide);
                            break;
                        case PlayerAttribute.Fitness:
                            if (Math.Max(attributeLeftToTrain.Speed, attributeLeftToTrain.Acceleration) > 0)
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.Sprinting);
                            else
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.TrainingMatch);
                            break;

                        case PlayerAttribute.Heading:
                            AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.Heading);
                            AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingScheduleType.Sprinting, trainingEffectModifier);
                            /*
                            //2 training match = 1 heading training
                            if (headingLeftToTrain- (Math.Max(almostAttributeCap- projectedShootingAfterTrainingMatch, almostAttributeCap - projectedPassingAfterTrainingMatch)/2) > 0)
                            {
                            }
                            else
                            {
                                //heading is overtrained
                                AddGrind(topBottleneckAttribute.AttributeIndex, grind, counter, repeat, TrainingScheduleType.TrainingMatch);
                            }*/
                            break;
                        case PlayerAttribute.Control:
                            if (player.Control < projectedAttributesAfterTrainingMatch.Control)
                            {
                                Debug.Assert(!(player.Shooting == attributeCap && player.Passing == attributeCap && player.Heading == attributeCap));
                                if (trainingShooting && attributeLeftToTrain.Attributes[(int)PlayerAttribute.Shooting] > 0)
                                {
                                    AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.TrainingMatch);
                                }
                                else if (trainHeading && attributeLeftToTrain.Attributes[(int)PlayerAttribute.Heading] > 0)
                                {
                                    AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.TrainingMatch);
                                }
                                else if (trainTackleDetermination && attributeLeftToTrain.Attributes[(int)PlayerAttribute.TackleDetermination] > 0)
                                {
                                    AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.TrainingMatch);
                                }
                                else if (trainCoolness && attributeLeftToTrain.Attributes[(int)PlayerAttribute.Coolness] > 0)
                                {
                                    AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.TrainingMatch);
                                }
                                else
                                    AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.FiveASide);
                            }
                            else if (attributeLeftToTrain.Coolness * 3 < attributeLeftToTrain.Awareness * 2)
                            {
                                if(!(player.Shooting == attributeCap && player.Passing == attributeCap && player.Heading == attributeCap))
                                //should use training match 
                                    AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.TrainingMatch);
                                else
                                    AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.Control);
                            }
                            else if (attributeLeftToTrain.Coolness == 0 && attributeLeftToTrain.Dribbling < attributeLeftToTrain.Control / 4)
                            {
                                //should use five a side instead
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.FiveASide);
                            }
                            else
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.Control);
                            }
                            break;
                        case PlayerAttribute.Dribbling:
                            if (player.Dribbling < projectedAttributesAfterTrainingMatch.Dribbling)
                            {
                                Debug.Assert(!(player.Shooting == attributeCap && player.Passing == attributeCap && player.Heading == attributeCap));
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.TrainingMatch);
                            }
                            else
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.Control);
                            }
                            break;
                        case PlayerAttribute.TackleDetermination:
                            if (hasTackleSkill) break;
                            hasTackleDetermination = true;
                            var tackleDeterminationLeftToTrain = attributeLeftToTrain.TackleDetermination;
                            if (player.TackleDetermination < projectedAttributesAfterTrainingMatch.TackleDetermination)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.TrainingMatch);
                            }
                            else if (tackleDeterminationLeftToTrain / (double)4 <= new[] {
                                //is it covered by training match?
                                    attributeLeftToTrain.Shooting/(double)4,
                                    attributeLeftToTrain.Passing/(double)8,
                                    attributeLeftToTrain.Heading/(double)4,
                                    attributeLeftToTrain.Dribbling/(double)4,
                                    attributeLeftToTrain.Control/(double)4,
                                    attributeLeftToTrain.Awareness/(double)6,
                                    attributeLeftToTrain.Flair/(double)6}.Max())
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.TrainingMatch);
                            }
                            else if (tackleDeterminationLeftToTrain / (double)2 <= new[] {
                                //is it covered by five a side?
                                    attributeLeftToTrain.Shooting/(double)4,
                                    attributeLeftToTrain.Passing/(double)8,
                                    attributeLeftToTrain.Control/(double)8,
                                    attributeLeftToTrain.Dribbling/(double)2,
                                    attributeLeftToTrain.Awareness/(double)4,
                                    attributeLeftToTrain.Flair/(double)8}.Max())
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.FiveASide);
                            }
                            else if (tackleDeterminationLeftToTrain < attributeLeftToTrain.TackleSkill / 2)
                            {
                                //is it covered by tackling training?
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.Tackling);
                                AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingScheduleType.TrainingMatch, trainingEffectModifier);
                                AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingScheduleType.Control, trainingEffectModifier);
                            }
                            else
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.Marking);
                                AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingScheduleType.TrainingMatch, trainingEffectModifier);
                                AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingScheduleType.Control, trainingEffectModifier);
                            }

                            break;
                        case PlayerAttribute.TackleSkill:
                            if (hasTackleDetermination ) break;
                            hasTackleSkill = true;
                            var tackleSkillLeftToTrain = attributeLeftToTrain.TackleSkill;
                            //still in initial training match grinding?
                            if (player.TackleSkill < projectedAttributesAfterTrainingMatch.TackleSkill)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.TrainingMatch);
                            }
                            else if (tackleSkillLeftToTrain / (double)4 <= new[]
                            {
                                //is it fully covered by reqired training match?
                                attributeLeftToTrain.Shooting/(double)4,
                                attributeLeftToTrain.Passing/(double)8,
                                attributeLeftToTrain.Dribbling/(double)4,
                                attributeLeftToTrain.Control/(double)4,
                                attributeLeftToTrain.Awareness/(double)6,
                                attributeLeftToTrain.Flair/(double)6}.Max())
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.TrainingMatch);
                            }
                            else if (tackleSkillLeftToTrain / (double)6 <= new[] {
                                //is it fully covered by reqired five a side?
                                attributeLeftToTrain.Shooting/(double)4,
                                attributeLeftToTrain.Passing/(double)8,
                                attributeLeftToTrain.Control/(double)8,
                                attributeLeftToTrain.Dribbling/(double)2,
                                attributeLeftToTrain.Awareness/(double)4,
                                attributeLeftToTrain.Flair/(double)8}.Max())
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.FiveASide);
                            }
                            //is it fully covered by reqired marking?
                            else if (tackleSkillLeftToTrain < attributeLeftToTrain.TackleDetermination / 2)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.Marking);
                                AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingScheduleType.TrainingMatch, trainingEffectModifier);
                                AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingScheduleType.Control, trainingEffectModifier);
                            }
                            else
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.Tackling);
                                AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingScheduleType.TrainingMatch, trainingEffectModifier);
                                AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingScheduleType.Control, trainingEffectModifier);
                            }
                            break;

                        case PlayerAttribute.Coolness:
                            var awarenessLeftToTrain = attributeLeftToTrain.Awareness;

                            if (player.Coolness < projectedAttributesAfterTrainingMatch.Coolness)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.TrainingMatch);
                            }
                            else if (attributeLeftToTrain.Attributes[(int)PlayerAttribute.Consistency] > 0 && player.Consistency < almostAttributeCap)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.Control);
                            }
                            else if (attributeLeftToTrain.Coolness * 3 <= awarenessLeftToTrain * 2)
                            {
                                //should use training match 
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.TrainingMatch);
                            }
                            else
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.Control);
                            }
                            break;

                        case PlayerAttribute.Awareness:
                            Debug.Assert(player.Awareness < attributeCap);
                            if (player.Awareness < projectedAttributesAfterTrainingMatch.Awareness)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.TrainingMatch);
                            }
                            else if (attributeLeftToTrain.Attributes[(int)PlayerAttribute.Consistency] > 0 && player.Consistency < attributeCap)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.Control);
                            }
                            else if (attributeLeftToTrain.Attributes[(int)PlayerAttribute.Control] > 0 && trainControl)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.Control);
                            }
                            else if (attributeLeftToTrain.Attributes[(int)PlayerAttribute.Dribbling] > 0 && trainDribbling)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.Control);
                            }
                            else if (position == PlayerPosition.GK && player.Passing == attributeCap
                                    && player.Control == attributeCap
                                    && attributeLeftToTrain.Handling >=1
                                    && attributeLeftToTrain.Agility >= 1)
                            {
                                Debug.Assert(player.Kicking < TrainingSchedule.attributeCap);
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.GoalKeeping);
                                AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingScheduleType.Kicking, trainingEffectModifier);
                            }
                            else if (attributeLeftToTrain.Coolness * 3 > attributeLeftToTrain.Awareness * 2)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.Control);
                            }
                            else if (attributeLeftToTrain.Flair * 3 > attributeLeftToTrain.Awareness * 4)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.FiveASide);
                            }
                            else if (attributeLeftToTrain.Coolness >= 1 || attributeLeftToTrain.Flair >= 1)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.TrainingMatch);
                            }
                            else
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.ZonalDefence);
                            }


                            break;
                        case PlayerAttribute.Flair:


                            if (player.Flair < projectedAttributesAfterTrainingMatch.Flair)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.TrainingMatch);
                            }
                            else if (attributeLeftToTrain.Attributes[(int)PlayerAttribute.Consistency] > 0 && player.Consistency < attributeCap)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.Control);
                            }
                            else if (flairLeftToTrain / (double)8 <= new[] {

                                attributeLeftToTrain.Shooting/(double)8,
                                attributeLeftToTrain.Passing/(double)8,
                                attributeLeftToTrain.Heading/(double)4,
                                attributeLeftToTrain.Control/(double)4,
                                attributeLeftToTrain.Dribbling/(double)2,
                                attributeLeftToTrain.Coolness/(double)4,
                                attributeLeftToTrain.Awareness/(double)6}.Max())
                            {
                                //is it fully covered by training match
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.TrainingMatch);
                            }
                            else if (flairLeftToTrain / (double)2 <= new[] {
                                    attributeLeftToTrain.Attributes[(int)PlayerAttribute.Control]/(double)8,
                                    attributeLeftToTrain.Attributes[(int)PlayerAttribute.Dribbling]/(double)6,
                                    attributeLeftToTrain.Attributes[(int)PlayerAttribute.Coolness]/(double)8,
                                    attributeLeftToTrain.Attributes[(int)PlayerAttribute.Consistency]/(double)6 }.Max())
                            {
                                //is it fully covered by control training?
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.Control);
                            }
                            else
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.FiveASide);
                            }
                            break;
                        case PlayerAttribute.Kicking:
                            AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.Kicking);
                            AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingScheduleType.Throwing, trainingEffectModifier);
                            //is training match better?
                            if ((attributeCap - player.Agility) / 4 < new[] {
                            attributeLeftToTrain.Attributes[(int)PlayerAttribute.Shooting] / (double)8,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Passing] / (double)8,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Heading] / (double)4,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Control] / (double)4,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Dribbling] / (double)2,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Coolness] / (double)4,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Awareness] / (double)6,
                            attributeLeftToTrain.Attributes[(int)PlayerAttribute.Flair] / (double)6}.Max())
                            {
                                AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingScheduleType.TrainingMatch, trainingEffectModifier);
                            }
                            else
                            {
                                AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingScheduleType.GoalKeeping, trainingEffectModifier);
                                AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingScheduleType.Kicking, trainingEffectModifier);
                                AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingScheduleType.Throwing, trainingEffectModifier);

                            }
                            break;
                        case PlayerAttribute.Handling:
                            AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.Handling);
                            AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingScheduleType.Kicking, trainingEffectModifier);
                            break;
                        case PlayerAttribute.Throwing:
                        case PlayerAttribute.ThrowIn:
                            AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.Throwing);
                            break;
                        case PlayerAttribute.Consistency:
                            Debug.Assert(position == PlayerPosition.GK|| position == PlayerPosition.LB|| position == PlayerPosition.RB|| position == PlayerPosition.CD
                                || trainingScheduleCalculationState.AlwaysTrainConsistency);
                            AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.Control);
                            break;

                        case PlayerAttribute.Determination:
                            AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.WeightTraining);
                            AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingScheduleType.TrainingMatch, trainingEffectModifier);
                            break;
                        case PlayerAttribute.Greed:
                            AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.Shooting);
                            break;
                        default:
                            //for other attributes, just ignore them
                            continue;
                    }
                }
                if (bottleneckAttributeIndex[0].AttributeIndex == PlayerAttribute.Control)
                {
                    if (genericTrainingGrind[0].TrainingScheduleType == TrainingScheduleType.TrainingMatch)
                    {
                        Debug.Assert(!(player.Shooting == 00 && player.Passing == attributeCap && player.Heading == attributeCap));
                    }
                }
            }
            else {
                var emptyResult = new List<TrainingScheduleSteps>();
                for (int i = 0; i < 7; i++)
                {
                    emptyResult.Add(new TrainingScheduleSteps
                    {
                        ForPlayerAttribute = PlayerAttribute.Count,
                        TrainingScheduleType = TrainingScheduleType.None
                    });
                }
                return emptyResult;
            }
            bool addCounter =  AreAllRevelantAttribtesAboveAlmostCap(player, position,trainingEffectModifier);
            var result = new List<TrainingScheduleSteps>();
            if (addCounter)
            {
                Debug.Assert(genericTrainingGrind.Count > 0);
                //Debug.Assert(counter.Count > 0);
                while (result.Count + genericTrainingCounter.Count < 7)
                {
                    result.AddRange(new List<TrainingScheduleSteps>(genericTrainingGrind));
                }
                if (result.Count + genericTrainingCounter.Count > 7)
                {
                    result.RemoveRange(7 - genericTrainingCounter.Count, result.Count + genericTrainingCounter.Count - 7);
                }
                result.AddRange(genericTrainingCounter);
            }
            else
            {
                Debug.Assert(genericTrainingGrind.Count > 0);
                while (result.Count < 7)
                {
                    result.AddRange(new List<TrainingScheduleSteps>(genericTrainingGrind));
                }
                result = result.Take(7).ToList();
            }
            Debug.Assert(result.Count == 7);

            return result;
        }
        static bool AreAllRevelantAttribtesAboveAlmostCap(PlayerModelDouble player, PlayerPosition position, TrainingEffectModifier trainingEffectModifier)
        {   
            for (int i = 0; i < (int)PlayerAttribute.Count; i++)
            {
                if (position==PlayerPosition.Count||PositionRatings.Ratings[(int)position][i] > 0)
                {
                    switch (i)
                    {
                        case (int)PlayerAttribute.Leadership:
                            if (trainingEffectModifier.PassingTrainLeadership)
                            {
                                if(player.Attributes[i] < almostAttributeCap)
                                    return false;
                            }
                            break;
                            
                        default:
                            if (player.Attributes[i] < almostAttributeCap)
                                return false;
                            break;
                    }
                }
            }
            return true;
        }
        static List<TrainingScheduleSteps> GetFinalTrainingSchedule(TrainingScheduleCalculationState trainingScheduleCalculationState, PlayerPosition playerPosition)
        {
            var finalGrind = trainingScheduleCalculationState.FinalGrind;
            var finalCounter = trainingScheduleCalculationState.FinalCounter;
            var finalResult = trainingScheduleCalculationState.FinalResult;
            var trainingEffectModifier = trainingScheduleCalculationState.TrainingEffectModifier;
            finalGrind.Clear();
            finalCounter.Clear();
            finalResult.Clear();
            var trainingShooting = trainingScheduleCalculationState.TrainShooting;
            var trainHeading = trainingScheduleCalculationState.TrainHeading;

            var finalAttributeLeftToTrain = trainingScheduleCalculationState.AttributesLeftToTrain;
            double controlAdded = 0;
            double trainingMatchesAdded = 0;
            double skillLost = 0;
            double agilityLost = 0;
            double sprintAdded = 0;
            switch (playerPosition)
            {
                case PlayerPosition.GK:
                    
                    var handlingNeeded = finalAttributeLeftToTrain.Handling;
                    if (handlingNeeded >= 1)
                    {
                        AddGrind(PlayerAttribute.Handling, finalGrind, finalCounter, handlingNeeded, TrainingScheduleType.Handling);
                    }
                    var kickingNeeded = finalAttributeLeftToTrain.Kicking;
                    if (handlingNeeded >= 1 && !trainingEffectModifier.RemoveNegativeTraining)
                    {
                        kickingNeeded += (handlingNeeded + 3) / 4;
                    }
                    if (kickingNeeded >= 1)
                    {
                        AddGrind(PlayerAttribute.Kicking, finalGrind, finalCounter, kickingNeeded, TrainingScheduleType.Kicking);
                        AddCounter(PlayerAttribute.Kicking, finalCounter, TrainingScheduleType.Throwing, trainingEffectModifier);
                        agilityLost+= kickingNeeded;
                    }
                    var throwingNeeded = finalAttributeLeftToTrain.ThrowIn;
                    if (kickingNeeded>=1 && !trainingEffectModifier.RemoveNegativeTraining)
                    {
                        throwingNeeded += kickingNeeded / 4;
                    }
                    if (throwingNeeded >= 1)
                    {
                        AddGrind(PlayerAttribute.Throwing, finalGrind, finalCounter, throwingNeeded, TrainingScheduleType.Throwing);
                    }
                    break;
                case PlayerPosition.LB:
                case PlayerPosition.RB:
                    var determinationNeeded= finalAttributeLeftToTrain.Determination;
                    if (determinationNeeded>0)
                    {
                        AddGrind(PlayerAttribute.Determination, finalGrind, finalCounter, determinationNeeded, TrainingScheduleType.WeightTraining);
                        if (!trainingEffectModifier.RemoveNegativeTraining)
                        {
                            AddCounter(PlayerAttribute.Speed, finalCounter, TrainingScheduleType.TrainingMatch, trainingEffectModifier);
                            trainingMatchesAdded += 1;
                        }
                    }
                    break;
            }

            switch (playerPosition)
            {
                case PlayerPosition.GK:
                case PlayerPosition.RB:
                case PlayerPosition.LB:
                case PlayerPosition.CD:
                    var consistencyNeeded = finalAttributeLeftToTrain.Consistency;
                    if (consistencyNeeded>0)
                    {
                        AddGrind(PlayerAttribute.Consistency, finalGrind, finalCounter, consistencyNeeded, TrainingScheduleType.Control);
                        controlAdded += consistencyNeeded;
                    }
                    break;
            }

            double sprintNeededForSpeed = finalAttributeLeftToTrain.Speed;
            if (sprintNeededForSpeed >0)
            {
                AddGrind(PlayerAttribute.Speed, finalGrind, finalCounter, sprintNeededForSpeed, TrainingScheduleType.Sprinting);
                sprintAdded += sprintNeededForSpeed;                
                skillLost += sprintNeededForSpeed;
                if (finalCounter.Where(t => t.TrainingScheduleType == TrainingScheduleType.TrainingMatch).Count() < (sprintNeededForSpeed + 1) / 2)
                {
                    AddCounter(PlayerAttribute.Speed, finalCounter, TrainingScheduleType.TrainingMatch, trainingEffectModifier);
                    skillLost = 0;
                    agilityLost = 0;
                    trainingMatchesAdded++;
                }
            }

            double sprintNeededForAcceleration = finalAttributeLeftToTrain.Acceleration;
            if (sprintNeededForAcceleration > 0 && sprintNeededForAcceleration > sprintNeededForSpeed)
            {
                AddGrind(PlayerAttribute.Acceleration, finalGrind, finalCounter, sprintNeededForAcceleration - sprintNeededForSpeed, TrainingScheduleType.Sprinting);
                skillLost += sprintNeededForAcceleration - sprintNeededForSpeed;
                if (trainingMatchesAdded == 0)
                {
                    AddCounter(PlayerAttribute.Acceleration, finalCounter, TrainingScheduleType.TrainingMatch, trainingEffectModifier);
                    skillLost = 0;
                    agilityLost = 0;
                    trainingMatchesAdded++;
                }
            }
            double agilityNeeded = (int)(finalAttributeLeftToTrain.Agility- trainingMatchesAdded/2+ (agilityLost+0.99)/2);
            if (agilityNeeded < 0) agilityNeeded = 0;
            if (agilityNeeded >0)
            {
                AddGrind(PlayerAttribute.Agility, finalGrind, finalCounter, agilityNeeded, TrainingScheduleType.TrainingMatch);
                trainingMatchesAdded += agilityNeeded;
            }
            double shootingNeeded = finalAttributeLeftToTrain.Shooting- trainingMatchesAdded;
            if (shootingNeeded < 0) shootingNeeded = 0;
            if (shootingNeeded >0)
            {
                AddGrind(PlayerAttribute.Shooting, finalGrind, finalCounter, shootingNeeded, TrainingScheduleType.TrainingMatch);
                trainingMatchesAdded += shootingNeeded;
            }
            double passingNeeded = finalAttributeLeftToTrain.Passing - trainingMatchesAdded;
            if (passingNeeded < 0) passingNeeded = 0;
            if (passingNeeded >0 && passingNeeded > shootingNeeded)
            {
                int repeat = (int)(passingNeeded - shootingNeeded);

                if (trainingShooting && finalAttributeLeftToTrain.Attributes[(int)PlayerAttribute.Shooting] > 0)
                {
                    AddGrind(PlayerAttribute.Passing, finalGrind, finalCounter, repeat, TrainingScheduleType.TrainingMatch);
                }
                else if (trainHeading && finalAttributeLeftToTrain.Attributes[(int)PlayerAttribute.Heading] > 0)
                {
                    AddGrind(PlayerAttribute.Passing, finalGrind, finalCounter, repeat, TrainingScheduleType.TrainingMatch);
                }
                else if (trainingScheduleCalculationState.TrainTackleDetermination && finalAttributeLeftToTrain.Attributes[(int)PlayerAttribute.TackleDetermination] > 0)
                {
                    AddGrind(PlayerAttribute.Passing, finalGrind, finalCounter, repeat, TrainingScheduleType.TrainingMatch);
                }
                else if (trainingScheduleCalculationState.TrainCoolness && finalAttributeLeftToTrain.Attributes[(int)PlayerAttribute.Coolness] > 0)
                {
                    AddGrind(PlayerAttribute.Passing, finalGrind, finalCounter, repeat, TrainingScheduleType.TrainingMatch);
                }
                else
                    AddGrind(PlayerAttribute.Passing, finalGrind, finalCounter, repeat, TrainingScheduleType.FiveASide);
                trainingMatchesAdded += passingNeeded - shootingNeeded;
            }
            double controlNeeded = finalAttributeLeftToTrain.Control- trainingMatchesAdded / 2;
            if (controlNeeded < 0) controlNeeded = 0;
            if (controlNeeded >0)
            {
                if(finalAttributeLeftToTrain.Dribbling>0|| finalAttributeLeftToTrain.Coolness>0|| finalAttributeLeftToTrain.Consistency >0)
                    AddGrind(PlayerAttribute.Control, finalGrind, finalCounter, controlNeeded, TrainingScheduleType.Control);
                else
                    AddGrind(PlayerAttribute.Control, finalGrind, finalCounter, controlNeeded, TrainingScheduleType.FiveASide);
                controlAdded += controlNeeded;
            }
            double driblingNeeded = finalAttributeLeftToTrain.Dribbling - trainingMatchesAdded / 4;            
            if (driblingNeeded <0) driblingNeeded = 0;            
            if (driblingNeeded >0  && driblingNeeded > controlNeeded)
            {
                AddGrind(PlayerAttribute.Dribbling, finalGrind, finalCounter, driblingNeeded-controlNeeded, TrainingScheduleType.Control);
                controlAdded += driblingNeeded - controlNeeded;
            }
            double coolnessNeeded = finalAttributeLeftToTrain.Coolness - controlAdded - trainingMatchesAdded / 2;
            double awarenessNeeded = finalAttributeLeftToTrain.Awareness - controlAdded / 4 - trainingMatchesAdded / 2;
            double flairNeeded = finalAttributeLeftToTrain.Flair - controlAdded / 2 - trainingMatchesAdded / 2;
            if (coolnessNeeded < 0) coolnessNeeded = 0;
            if (awarenessNeeded < 0) awarenessNeeded = 0;
            if (flairNeeded < 0) flairNeeded = 0;
            if (coolnessNeeded >0 && awarenessNeeded < coolnessNeeded)
            {
                AddGrind(PlayerAttribute.Coolness, finalGrind, finalCounter, coolnessNeeded, TrainingScheduleType.Control);
                controlAdded += coolnessNeeded;
            }
            awarenessNeeded = finalAttributeLeftToTrain.Awareness - controlAdded / 4 - trainingMatchesAdded / 2;
            if (awarenessNeeded < 0) awarenessNeeded = 0;

            flairNeeded = finalAttributeLeftToTrain.Flair - controlAdded / 2 - trainingMatchesAdded / 2;
            if (flairNeeded < 0) flairNeeded = 0;

            double fiveaSideAdded = 0;
            double trainingMatchAddedForFlair = 0;
            if (flairNeeded >0)
            {
                if (awarenessNeeded < flairNeeded)
                {
                    AddGrind(PlayerAttribute.Flair, finalGrind, finalCounter, flairNeeded - awarenessNeeded, TrainingScheduleType.FiveASide);
                    fiveaSideAdded += flairNeeded - awarenessNeeded;
                }
                else {
                    trainingMatchAddedForFlair = awarenessNeeded;
                    AddGrind(PlayerAttribute.Flair, finalGrind, finalCounter, trainingMatchAddedForFlair, TrainingScheduleType.TrainingMatch);
                    trainingMatchesAdded += trainingMatchAddedForFlair;
                }
            }
            awarenessNeeded -= (fiveaSideAdded  + trainingMatchAddedForFlair) / 2;
            if (awarenessNeeded < 0) awarenessNeeded = 0;
            if (awarenessNeeded >0)
            {
                if (skillLost >0)
                {
                    AddGrind(PlayerAttribute.Awareness, finalGrind, finalCounter, (skillLost+1)/2, TrainingScheduleType.TrainingMatch);
                    if(awarenessNeeded> (skillLost + 1) / 2)
                        AddGrind(PlayerAttribute.Awareness, finalGrind, finalCounter, awarenessNeeded-skillLost, TrainingScheduleType.TrainingMatch);
                    skillLost = 0;
                }
                else if (controlNeeded >0)
                {
                    AddGrind(PlayerAttribute.Awareness, finalGrind, finalCounter, awarenessNeeded, TrainingScheduleType.FiveASide);
                    fiveaSideAdded += awarenessNeeded;
                }
                else if (flairNeeded <1 && coolnessNeeded <1 )
                {
                    AddGrind(PlayerAttribute.Awareness, finalGrind, finalCounter, awarenessNeeded, TrainingScheduleType.ZonalDefence);
                    if (trainingMatchesAdded == 0 && !trainingEffectModifier.RemoveNegativeTraining)
                    {
                        AddCounter(PlayerAttribute.Awareness, finalCounter, TrainingScheduleType.TrainingMatch, trainingEffectModifier);
                        trainingMatchesAdded+= awarenessNeeded;
                    }
                }
                else if (coolnessNeeded * 3 > awarenessNeeded * 2 && finalAttributeLeftToTrain.Coolness >0)
                {
                    AddGrind(PlayerAttribute.Awareness, finalGrind, finalCounter, awarenessNeeded, TrainingScheduleType.Control);
                    controlAdded+= awarenessNeeded;
                }
                else if (flairNeeded > awarenessNeeded && finalAttributeLeftToTrain.Flair >0)
                {
                    AddGrind(PlayerAttribute.Awareness, finalGrind, finalCounter, awarenessNeeded, TrainingScheduleType.FiveASide);
                    fiveaSideAdded+= awarenessNeeded;
                }
                else
                {
                    AddGrind(PlayerAttribute.Awareness, finalGrind, finalCounter, awarenessNeeded, TrainingScheduleType.TrainingMatch);
                    trainingMatchesAdded+= awarenessNeeded;
                }
            }
            var headingNeeded =(int)( finalAttributeLeftToTrain.Heading - trainingMatchesAdded - fiveaSideAdded / 4+(skillLost+3)/8);
            if (headingNeeded < 0) headingNeeded = 0;
            if (headingNeeded >0)
            {
                if (headingNeeded > 4)
                    headingNeeded = 4;
                AddGrind(PlayerAttribute.Heading, finalGrind, finalCounter, headingNeeded, TrainingScheduleType.Heading);
                AddCounter(PlayerAttribute.Heading, finalCounter, TrainingScheduleType.Sprinting, trainingEffectModifier);
                skillLost += 1;
            }
            if (finalAttributeLeftToTrain.TackleSkill < finalAttributeLeftToTrain.TackleDetermination)
            {
                double markingsAdded = 0;
                double markingNeeded = finalAttributeLeftToTrain.TackleDetermination - trainingMatchesAdded / 2 - fiveaSideAdded / 4;
                if (markingNeeded < 0) markingNeeded = 0;
                if (markingNeeded > 0)
                {
                    if(markingNeeded>4)
                        markingNeeded = 4;
                    AddGrind(PlayerAttribute.TackleDetermination, finalGrind, finalCounter, markingNeeded, TrainingScheduleType.Marking);
                    markingsAdded += markingNeeded;
                    skillLost += 1;
                }
                double tacklingSkillAdded = 0;
                double tacklingSkillNeeded = finalAttributeLeftToTrain.TackleSkill - trainingMatchesAdded / 2 - fiveaSideAdded * 2 / 3 - markingsAdded / 2;
                if (tacklingSkillNeeded < 0) tacklingSkillNeeded = 0;
                if (tacklingSkillNeeded > 0)
                {
                    if (tacklingSkillNeeded > 4)
                        tacklingSkillNeeded = 4;
                    tacklingSkillAdded = tacklingSkillNeeded;

                    AddGrind(PlayerAttribute.TackleSkill, finalGrind, finalCounter, tacklingSkillNeeded, TrainingScheduleType.Tackling);
                    if (trainingMatchesAdded == 0)
                    {
                        AddCounter(PlayerAttribute.TackleSkill, finalCounter, TrainingScheduleType.FiveASide, trainingEffectModifier);
                        fiveaSideAdded++;
                    }
                }
            }
            else {
                double tacklingSkillAdded = 0;
                double tacklingSkillNeeded = finalAttributeLeftToTrain.TackleSkill - trainingMatchesAdded / 2 - fiveaSideAdded * 2 / 3;
                if (tacklingSkillNeeded < 0) tacklingSkillNeeded = 0;
                if (tacklingSkillNeeded > 0)
                {
                    if (tacklingSkillNeeded > 4)
                        tacklingSkillNeeded = 4;
                    tacklingSkillAdded = tacklingSkillNeeded;
                    AddGrind(PlayerAttribute.TackleSkill, finalGrind, finalCounter, tacklingSkillNeeded, TrainingScheduleType.Tackling);
                    if (trainingMatchesAdded == 0)
                    {
                        AddCounter(PlayerAttribute.TackleSkill, finalCounter, TrainingScheduleType.FiveASide, trainingEffectModifier);
                        fiveaSideAdded++;
                    }
                }
                double markingsAdded = 0;
                double markingNeeded = finalAttributeLeftToTrain.TackleDetermination - trainingMatchesAdded / 2 - fiveaSideAdded / 4 - tacklingSkillAdded / 2;
                if (markingNeeded < 0) markingNeeded = 0;
                if (markingNeeded > 0)
                {
                    if (markingNeeded > 4)
                        markingNeeded = 4;
                    AddGrind(PlayerAttribute.TackleDetermination, finalGrind, finalCounter, markingNeeded, TrainingScheduleType.Marking);
                    markingsAdded += markingNeeded;
                    skillLost += 1;
                }
            }
            /*
            if (controlAdded > 0 && !finalGrind.Any(t => t.TrainingScheduleType != TrainingScheduleType.Control && t.TrainingScheduleType != TrainingScheduleType.FiveASide))
            {
                for (int i = 0; i < finalGrind.Count; i++)
                {
                    if (finalGrind[i].TrainingScheduleType == TrainingScheduleType.FiveASide)
                        finalGrind[i].TrainingScheduleType = TrainingScheduleType.Control;
                }
            }*/
            if (finalGrind.Count == 0)
                finalGrind.Add(new TrainingScheduleSteps() { TrainingScheduleType = TrainingScheduleType.TrainingMatch, ForPlayerAttribute = PlayerAttribute.Count });
            finalResult.AddRange(finalGrind);
            while (finalResult.Count + finalCounter.Count < 7)
            {
                finalResult.AddRange(new List<TrainingScheduleSteps>(finalGrind));
            }
            if (finalResult.Count + finalCounter.Count > 7)
            {
                finalResult.RemoveRange(7 - finalCounter.Count, finalResult.Count + finalCounter.Count - 7);
            }
            finalResult.AddRange(finalCounter);
            return finalResult.Take(7).ToList();
        }

        #endregion

        private static List<TrainingScheduleSteps> GetGKTrainingSchedule(PlayerModelDouble player, bool maxPower
            , TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects, PlayerModelDouble projectedAttributesAfterSprinting, PlayerModelDouble projectedAttributesAfterTrainingMatch)
        {
            foreach (var stage in stages)
            {
                if (player.Agility < stage || player.Handling < stage || player.Kicking < stage || player.Throwing < stage || player.Coolness < stage || player.Awareness < stage ||

                    player.Consistency < stage || player.Control < stage || player.Passing < stage || player.Speed < stage)
                {
                    return GetGKTrainingScheduleStage(player, maxPower
                    , trainingEffectModifier, trainingEffects,stage, projectedAttributesAfterSprinting, projectedAttributesAfterTrainingMatch);
                }
            }
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.GK, player) == attributeCap);
            return null;
        }

        private static List<TrainingScheduleSteps> GetGKTrainingScheduleStage(PlayerModelDouble player,
            bool maxPower, TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects, int stageMinimum, PlayerModelDouble projectedAttributesAfterSprinting, PlayerModelDouble projectedAttributesAfterTrainingMatch)
        {
            List<TrainingScheduleSteps> result;
            result = ImproveHandlingTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;
            result = ImproveKickingTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;
            result = ImproveThrowingTo(player, stageMinimum); if (result != null) return result;
            if (stageMinimum < almostAttributeCap)
            {
                result = ImproveConsistencyTo(player, stageMinimum); if (result != null) return result;
                result = ImproveCoolnessAndAwarenessTo(player, stageMinimum, maxPower,trainingEffects); if (result != null) return result;
            }
            if (!trainingEffectModifier.RemoveNegativeTraining)
            {
                result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier); if (result != null) return result;
            }
            result = ImprovePassingTo(player, stageMinimum); if (result != null) return result;
            result = ImproveConsistencyTo(player, stageMinimum); if (result != null) return result;
            result = ImproveCoolnessAndAwarenessTo(player, stageMinimum, maxPower,trainingEffects); if (result != null) return result;
            result = ImproveControlTo(player, stageMinimum); if (result != null) return result;
            result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier); if (result != null) return result;
            result = ImproveGKAgilityTo(player, stageMinimum); if (result != null) return result;
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.GK, player) == attributeCap);
            return null;
        }

        private static List<TrainingScheduleSteps> GetLRBTrainingSchedule(PlayerModelDouble player, bool maxPower
             , TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects)
        {
            foreach (var stage in stages)
            {
                if (player.Speed < stage || player.Passing < stage || player.Heading < stage || player.TackleDetermination < stage ||
                player.TackleSkill < stage || player.Coolness < stage || player.Awareness < stage || player.Consistency < stage
                || player.Determination < stage)
                {
                    return GetLRBTrainingScheduleStage(player, maxPower
                    , trainingEffectModifier,trainingEffects, stage);
                }
            }
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.RB, player) >= attributeCap);
            return null;
        }

        private static List<TrainingScheduleSteps> GetLRBTrainingScheduleStage(PlayerModelDouble player, bool maxPower,
            TrainingEffectModifier trainingEffectModifier,  float[][] trainingEffects,int stageMinimum)
        {
            List<TrainingScheduleSteps> result;
            result = ImproveDeterminationTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;
            if (!trainingEffectModifier.RemoveNegativeTraining)
            {
                result = ImproveSpeedTo(player, stageMinimum, true, trainingEffectModifier); if (result != null) return result;
            }
            if (stageMinimum < attributeCap)
            {
                result = ImproveConsistencyTo(player, stageMinimum); if (result != null) return result;
            }
            result = ImprovePassingTo(player, stageMinimum); if (result != null) return result;
            result = ImproveHeadingTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;
            result = ImproveTackleDeterminationAndSkillTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;
            result = ImproveCoolnessAndAwarenessTo(player, stageMinimum, maxPower,trainingEffects); if (result != null) return result;
            result = ImproveConsistencyTo(player, stageMinimum); if (result != null) return result;
            result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier); if (result != null) return result;
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.RB, player) == attributeCap);
            return null;
        }

        private static List<TrainingScheduleSteps> GetCDTrainingSchedule(PlayerModelDouble player, bool maxPower
            , TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects)
        {
            foreach (var stage in stages)
            {
                if (player.Speed < stage || player.Passing < stage || player.Heading < stage || player.TackleDetermination < stage ||
                player.TackleSkill < stage || player.Coolness < stage || player.Awareness < stage || player.Consistency < stage
                || (player.Leadership < stage && trainingEffectModifier.PassingTrainLeadership))
                {
                    return GetCDTrainingScheduleStage(player, maxPower
                    , trainingEffectModifier,trainingEffects, stage);
                }
            }
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.CD, player) >= attributeCap +
               player.Leadership / 33 - 3);
            return null;
        }

        private static List<TrainingScheduleSteps> GetCDTrainingScheduleStage(PlayerModelDouble player, bool maxPower, TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects, int stageMinimum)
        {
            List<TrainingScheduleSteps> result;
            if (stageMinimum < almostAttributeCap)
            {
                result = ImproveTackleDeterminationAndSkillTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;
                result = ImproveHeadingTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;
                result = ImproveConsistencyTo(player, stageMinimum); if (result != null) return result;
            }
            if (!trainingEffectModifier.RemoveNegativeTraining)
            {
                result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier); if (result != null) return result;
            }
            result = ImprovePassingTo(player, stageMinimum); if (result != null) return result;
            result = ImproveLeadershipTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;
            result = ImproveHeadingTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;
            result = ImproveTackleDeterminationAndSkillTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;
            result = ImproveConsistencyTo(player, stageMinimum); if (result != null) return result;
            result = ImproveCoolnessAndAwarenessTo(player, stageMinimum, maxPower,trainingEffects); if (result != null) return result;
            result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier); if (result != null) return result;
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.CD, player) >= attributeCap +
               player.Leadership / 33 - 3);

            return null;
        }
        private static List<TrainingScheduleSteps> GetLRWBTrainingSchedule(PlayerModelDouble player, bool maxPower
            , TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects)
        {
            foreach (var stage in stages)
            {
                if (player.Speed < stage || player.Agility < stage || player.Acceleration < stage || player.Passing < stage ||
                player.Dribbling < stage || player.TackleDetermination < stage ||
                player.TackleSkill < stage || player.Awareness < stage || player.Flair < stage)
                {
                    return GetLRWBTrainingScheduleStage(player, maxPower
                    , trainingEffectModifier,trainingEffects, stage);
                }
            }
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.RWB, player) == attributeCap);
            return null;
        }

        private static List<TrainingScheduleSteps> GetLRWBTrainingScheduleStage(PlayerModelDouble player, bool maxPower, TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects, int stageMinimum)
        {
            List<TrainingScheduleSteps> result;
            if (stageMinimum < almostAttributeCap)
            {
                result = ImproveDribbleTo(player, stageMinimum); if (result != null) return result;
                result = ImproveTackleDeterminationAndSkillTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;
                result = ImprovePassingTo(player, stageMinimum); if (result != null) return result;
            }
            if (!trainingEffectModifier.RemoveNegativeTraining)
            {
                result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier); if (result != null) return result;
                result = ImproveAccelerationTo(player, stageMinimum, false, trainingEffectModifier); if (result != null) return result;
            }
            result = ImprovePassingTo(player, stageMinimum); if (result != null) return result;
            result = ImproveAgilityTo(player, stageMinimum); if (result != null) return result;
            result = ImproveTackleDeterminationAndSkillTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;
            result = ImproveDribbleTo(player, stageMinimum); if (result != null) return result;
            result = ImproveAwarenessAndFlairTo(player, stageMinimum, maxPower, false, true, true, trainingEffectModifier,trainingEffects); if (result != null) return result;
            result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier); if (result != null) return result;
            result = ImproveAccelerationTo(player, stageMinimum, false, trainingEffectModifier); if (result != null) return result;
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.RWB, player) == attributeCap);
            return null;
        }

        private static List<TrainingScheduleSteps> GetSWTrainingSchedule(PlayerModelDouble player, bool maxPower, TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects)
        {
            foreach (var stage in stages)
            {
                if (player.Speed < stage || player.Acceleration < stage || player.Passing < stage ||
                     player.Heading < stage || player.Dribbling < stage ||
               player.TackleDetermination < stage || player.TackleSkill < stage || player.Awareness < stage)
                {
                    return GetSWTrainingScheduleStage(player, maxPower
                    , trainingEffectModifier,trainingEffects, stage);
                }
            }
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.SW, player) == attributeCap);
            return null;
        }

        private static List<TrainingScheduleSteps> GetSWTrainingScheduleStage(PlayerModelDouble player, bool maxPower,
             TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects, int stageMinimum)
        {
            List<TrainingScheduleSteps> result;
            if (stageMinimum < almostAttributeCap)
            {
                result = ImproveTackleDeterminationAndSkillTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;
                result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier); if (result != null) return result;
                result = ImproveAccelerationTo(player, stageMinimum, true, trainingEffectModifier); if (result != null) return result;
                result = ImproveHeadingTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;
                result = ImprovePassingTo(player, stageMinimum); if (result != null) return result;
                result = ImproveDribbleTo(player, stageMinimum); if (result != null) return result;
                result = ImproveAwarenessTo(player, stageMinimum, maxPower, trainingEffectModifier,trainingEffects); if (result != null) return result;
            }
            if (!trainingEffectModifier.RemoveNegativeTraining)
            {
                result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier); if (result != null) return result;
                result = ImproveAccelerationTo(player, stageMinimum, true, trainingEffectModifier); if (result != null) return result;
            }
            result = ImprovePassingTo(player, stageMinimum); if (result != null) return result;
            result = ImproveHeadingTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;
            result = ImproveDribbleTo(player, stageMinimum); if (result != null) return result;
            result = ImproveTackleDeterminationAndSkillTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;
            result = ImproveAwarenessTo(player, stageMinimum, maxPower, trainingEffectModifier,trainingEffects); if (result != null) return result;
            result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier); if (result != null) return result;
            result = ImproveAccelerationTo(player, stageMinimum, false, trainingEffectModifier); if (result != null) return result;
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.SW, player) == attributeCap);
            return null;
        }

        private static List<TrainingScheduleSteps> GetDMTrainingSchedule(PlayerModelDouble player, bool maxPower, TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects)
        {
            foreach (var stage in stages)
            {
                if (player.Speed < stage || player.Passing < stage || player.Heading < stage ||
               player.TackleDetermination < stage || player.TackleSkill < stage || player.Awareness < stage)
                {
                    return GetDMTrainingScheduleStage(player, maxPower
                    , trainingEffectModifier,trainingEffects, stage);
                }
            }
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.DM, player) == attributeCap);
            return null;
        }

        private static List<TrainingScheduleSteps> GetDMTrainingScheduleStage(PlayerModelDouble player, bool maxPower,
            TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects, int stageMinimum)
        {
            List<TrainingScheduleSteps> result;
            if (stageMinimum < attributeCap)
            {
                result = ImproveTackleDeterminationAndSkillTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;
                result = ImprovePassingTo(player, stageMinimum); if (result != null) return result;
                result = ImproveAwarenessTo(player, stageMinimum, maxPower, trainingEffectModifier, trainingEffects); if (result != null) return result;
            }
            if (!trainingEffectModifier.RemoveNegativeTraining)
            {
                result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier); if (result != null) return result;
            }
            result = ImprovePassingTo(player, stageMinimum); if (result != null) return result;
            result = ImproveHeadingTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;
            result = ImproveTackleDeterminationAndSkillTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;
            result = ImproveAwarenessTo(player, stageMinimum, maxPower, trainingEffectModifier, trainingEffects); if (result != null) return result;
            result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier); if (result != null) return result;
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.DM, player) == attributeCap);
            return null;
        }

        private static List<TrainingScheduleSteps> GetLRAMTrainingSchedule(PlayerModelDouble player,
            PlayerPosition position, bool maxPower, TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects)
        {
            foreach (var stage in stages)
            {
                if (player.Speed < stage || player.Acceleration < stage || player.Shooting < stage || player.Passing < stage
                    || player.Control < stage || player.Dribbling < stage || player.TackleSkill < stage
                    || player.Awareness < stage || player.Flair < stage)
                {
                    return GetLRAMTrainingScheduleStage(player, position, maxPower
                    , trainingEffectModifier,trainingEffects, stage);
                }
            }
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.RM, player) == attributeCap);
            return null;
        }

        private static List<TrainingScheduleSteps> GetLRAMTrainingScheduleStage(PlayerModelDouble player, PlayerPosition position,
            bool maxPower, TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects, int stageMinimum)
        {
            List<TrainingScheduleSteps> result;
            if (stageMinimum < attributeCap)
            {
                if (position == PlayerPosition.AM)
                {
                    result = ImprovePassingTo(player, stageMinimum); if (result != null) return result;
                    result = ImproveTackleSkillTo(player, stageMinimum - 1, trainingEffectModifier,trainingEffects); if (result != null) return result;
                }
                else
                {
                    result = ImproveTackleSkillTo(player, stageMinimum, trainingEffectModifier, trainingEffects); if (result != null) return result;
                    result = ImprovePassingTo(player, stageMinimum - 1); if (result != null) return result;
                }
            }
            if (!trainingEffectModifier.RemoveNegativeTraining)
            {
                result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier); if (result != null) return result;
                result = ImproveAccelerationTo(player, stageMinimum, false, trainingEffectModifier); if (result != null) return result;
            }
            result = ImproveShootingTo(player, stageMinimum); if (result != null) return result;
            result = ImprovePassingTo(player, stageMinimum); if (result != null) return result;
            result = ImproveTackleSkillTo(player, stageMinimum, trainingEffectModifier, trainingEffects); if (result != null) return result;
            result = ImproveControlTo(player, stageMinimum); if (result != null) return result;
            result = ImproveDribbleTo(player, stageMinimum); if (result != null) return result;
            result = ImproveAwarenessAndFlairTo(player, stageMinimum, maxPower, true, false, false, trainingEffectModifier,trainingEffects); if (result != null) return result;
            result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier); if (result != null) return result;
            result = ImproveAccelerationTo(player, stageMinimum, false, trainingEffectModifier); if (result != null) return result;
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.RM, player) == attributeCap);
            return null;
        }
        private static List<TrainingScheduleSteps> GetLRWTrainingSchedule(PlayerModelDouble player, bool maxPower, TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects)
        {
            foreach (var stage in stages)
            {
                if (player.Speed < stage || player.Agility < (stage + attributeCap) / 2 || player.Acceleration < stage || player.Shooting < stage || player.Passing < stage || player.Control < stage || player.Dribbling < stage || player.TackleSkill < stage
                    || player.Awareness < stage || player.Flair < stage)
                {
                    return GetLRWTrainingScheduleStage(player, maxPower
                    , trainingEffectModifier,trainingEffects, stage);
                }
            }
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.RW, player) == attributeCap);
            return null;
        }

        private static List<TrainingScheduleSteps> GetLRWTrainingScheduleStage(PlayerModelDouble player, bool maxPower,
             TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects, int stageMinimum)
        {
            List<TrainingScheduleSteps> result;

            if (stageMinimum < attributeCap)
            {
                result = ImproveSpeedTo(player, attributeCap, false, trainingEffectModifier); if (result != null) return result;
                result = ImproveAccelerationTo(player, attributeCap, false, trainingEffectModifier); if (result != null) return result;
                result = ImproveAgilityTo(player, (stageMinimum + attributeCap) / 2); if (result != null) return result;
                result = ImprovePassingTo(player, stageMinimum); if (result != null) return result;
                result = ImproveControlTo(player, stageMinimum); if (result != null) return result;
                result = ImproveDribbleTo(player, stageMinimum); if (result != null) return result;
            }
            if (!trainingEffectModifier.RemoveNegativeTraining)
            {
                result = ImproveSpeedTo(player, attributeCap, false, trainingEffectModifier); if (result != null) return result;
                result = ImproveAccelerationTo(player, attributeCap, false, trainingEffectModifier); if (result != null) return result;
            }
            result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier); if (result != null) return result;
            result = ImproveAccelerationTo(player, stageMinimum, false, trainingEffectModifier); if (result != null) return result;
            result = ImproveAgilityTo(player, (stageMinimum + attributeCap) / 2); if (result != null) return result;
            result = ImproveShootingTo(player, stageMinimum); if (result != null) return result;
            result = ImprovePassingTo(player, stageMinimum); if (result != null) return result;
            result = ImproveAwarenessAndFlairTo(player, stageMinimum, maxPower, true, false, false, trainingEffectModifier,trainingEffects); if (result != null) return result;
            result = ImproveTackleSkillTo(player, stageMinimum, trainingEffectModifier,trainingEffects); if (result != null) return result;
            result = ImproveDribbleTo(player, stageMinimum); if (result != null) return result;
            result = ImproveControlTo(player, stageMinimum); if (result != null) return result;
            result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier); if (result != null) return result;
            result = ImproveAccelerationTo(player, stageMinimum, false, trainingEffectModifier); if (result != null) return result;
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.RW, player) == attributeCap);
            return null;
        }

        private static List<TrainingScheduleSteps> GetFRTrainingSchedule(PlayerModelDouble player, bool maxPower, TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects)
        {
            foreach (var stage in stages)
            {
                if (player.Speed < stage || player.Agility < stage || player.Acceleration < stage
                    || player.Shooting < stage || player.Passing < stage || player.Heading < stage
                    || player.Control < stage || player.Dribbling < stage
                    || player.Awareness < stage || player.Flair < stage)
                {
                    return GetFRTrainingScheduleStage(player, maxPower
                    , trainingEffectModifier,trainingEffects, stage);
                }
            }
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.FR, player) == attributeCap);
            return null;
        }

        private static List<TrainingScheduleSteps> GetFRTrainingScheduleStage(PlayerModelDouble player, bool maxPower,
            TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects, int stageMinimum)
        {
            List<TrainingScheduleSteps> result;
            if (stageMinimum < attributeCap)
            {
                result = ImproveAccelerationTo(player, stageMinimum, true, trainingEffectModifier); if (result != null) return result;
                result = ImproveAgilityTo(player, stageMinimum); if (result != null) return result;
                result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier); if (result != null) return result;
                result = ImproveHeadingTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;
                result = ImproveControlTo(player, stageMinimum); if (result != null) return result;
                result = ImproveDribbleTo(player, stageMinimum); if (result != null) return result;
                result = ImprovePassingTo(player, stageMinimum); if (result != null) return result;
                result = ImproveAwarenessAndFlairTo(player, stageMinimum, maxPower, true, true, false, trainingEffectModifier,trainingEffects); if (result != null) return result;
            }
            if (!trainingEffectModifier.RemoveNegativeTraining)
            {
                result = ImproveHeadingTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;
                result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier); if (result != null) return result;
                result = ImproveAccelerationTo(player, stageMinimum, true, trainingEffectModifier); if (result != null) return result;
            }
            result = ImproveAgilityTo(player, stageMinimum); if (result != null) return result;
            result = ImproveShootingTo(player, stageMinimum); if (result != null) return result;
            result = ImprovePassingTo(player, stageMinimum); if (result != null) return result;
            result = ImproveControlTo(player, stageMinimum); if (result != null) return result;
            result = ImproveDribbleTo(player, stageMinimum); if (result != null) return result;
            result = ImproveAwarenessAndFlairTo(player, stageMinimum, maxPower, true, true, false, trainingEffectModifier, trainingEffects); if (result != null) return result;
            result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier); if (result != null) return result;
            result = ImproveAccelerationTo(player, stageMinimum, true, trainingEffectModifier); if (result != null) return result;
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.FR, player) == attributeCap);
            return null;
        }

        private static List<TrainingScheduleSteps> GetFORSSTrainingSchedule(PlayerModelDouble player, PlayerPosition position, bool maxPower, TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects)
        {
            foreach (var stage in stages)
            {
                //balanced training will focus on weak points first
                if (player.Speed < stage
                || player.Acceleration < stage
                || player.Heading < stage
                || player.Dribbling < stage
                || player.Control < stage
                || player.Coolness < stage
                || player.Awareness < stage
                || player.Flair < stage
                || player.Shooting < stage
                || player.Passing < stage
                || player.Agility < stage
                )
                {
                    return GetFORSSTrainingScheduleStage(player, position, maxPower
                    , trainingEffectModifier,trainingEffects, stage);
                }
            }
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.FOR, player) == attributeCap);
            return null;
        }

        private static List<TrainingScheduleSteps> GetFORSSTrainingScheduleStage(PlayerModelDouble player, PlayerPosition position, bool maxPower, TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects,int stageMinimum)
        {
            List<TrainingScheduleSteps> result;
            if (stageMinimum < attributeCap)
            {
                result = ImproveShootingTo(player, stageMinimum); if (result != null) return result;
                result = ImproveHeadingTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;
                result = ImproveControlTo(player, stageMinimum); if (result != null) return result;
                result = ImproveDribbleTo(player, stageMinimum); if (result != null) return result;
                result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier); if (result != null) return result;
                result = ImproveAccelerationTo(player, stageMinimum, true, trainingEffectModifier); if (result != null) return result;
                result = ImproveCoolnessAwarenessAndFlairTo(player, stageMinimum, maxPower, trainingEffects); if (result != null) return result;
            }
            if (!trainingEffectModifier.RemoveNegativeTraining)
            {
                result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier); if (result != null) return result;
                result = ImproveAccelerationTo(player, stageMinimum, true, trainingEffectModifier); if (result != null) return result;
            }
            result = ImproveHeadingTo(player, stageMinimum, trainingEffectModifier); if (result != null) return result;
            result = ImproveAgilityTo(player, stageMinimum); if (result != null) return result;
            result = ImproveShootingTo(player, stageMinimum); if (result != null) return result;
            result = ImprovePassingTo(player, stageMinimum); if (result != null) return result;
            result = ImproveCoolnessAwarenessAndFlairTo(player, stageMinimum, maxPower,trainingEffects); if (result != null) return result;
            result = ImproveDribbleTo(player, stageMinimum); if (result != null) return result;
            result = ImproveControlTo(player, stageMinimum); if (result != null) return result;
            result = ImproveSpeedTo(player, stageMinimum, false, trainingEffectModifier); if (result != null) return result;
            result = ImproveAccelerationTo(player, stageMinimum, true, trainingEffectModifier); if (result != null) return result;
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.FOR, player) == attributeCap);
            return null;
        }
        static bool ShouldStopDefaultTraining(PlayerModelDouble player, PlayerPosition playerPosition)
        {
            switch (playerPosition)
            {
                case PlayerPosition.GK:
                    return player.Handling >= attributeCap ||
                        player.Kicking >= attributeCap ||
                        player.Throwing >= attributeCap;
                case PlayerPosition.LB:
                case PlayerPosition.RB:
                    return player.Speed >= attributeCap
                        || player.Determination >= attributeCap
                        || player.Passing >= attributeCap
                        || player.Heading >= attributeCap
                        || player.TackleDetermination >= attributeCap
                        || player.TackleSkill >= attributeCap
                        || player.Consistency >= attributeCap;
                case PlayerPosition.CD:
                    return player.Speed >= attributeCap
                        || player.Passing >= attributeCap
                        || player.Heading >= attributeCap
                        || player.TackleDetermination >= attributeCap
                        || player.TackleSkill >= attributeCap
                        || player.Consistency >= attributeCap;

                case PlayerPosition.LWB:
                case PlayerPosition.RWB:
                    return player.Speed >= attributeCap
                        || player.Acceleration >= attributeCap
                        || player.Passing >= attributeCap
                        || player.Dribbling >= attributeCap
                        || player.TackleDetermination >= attributeCap
                        || player.TackleSkill >= attributeCap;

                case PlayerPosition.SW:
                    return player.Speed >= attributeCap
                        || player.Acceleration >= attributeCap
                        || player.Passing >= attributeCap
                        || player.Heading >= attributeCap
                        || player.Dribbling >= attributeCap
                        || player.TackleDetermination >= attributeCap
                        || player.TackleSkill >= attributeCap;

                case PlayerPosition.DM:
                    return player.Speed >= attributeCap
                        || player.Passing >= attributeCap
                        || player.Heading >= attributeCap
                        || player.TackleDetermination >= attributeCap
                        || player.TackleSkill >= attributeCap;
                case PlayerPosition.LM:
                case PlayerPosition.RM:
                case PlayerPosition.AM:
                case PlayerPosition.LW:
                case PlayerPosition.RW:
                    return player.Speed >= attributeCap
                        || player.Acceleration >= attributeCap
                       || player.Passing >= attributeCap
                       || player.Control >= attributeCap
                       || player.Dribbling >= attributeCap
                       || player.TackleSkill >= attributeCap;
                case PlayerPosition.FR:
                case PlayerPosition.FOR:
                case PlayerPosition.SS:
                    return player.Speed >= attributeCap
                        || player.Acceleration >= attributeCap
                       || player.Passing >= attributeCap
                       || player.Heading >= attributeCap
                       || player.Control >= attributeCap
                       || player.Dribbling >= attributeCap;

            }
            return true;
        }
        static void AddGrind(PlayerAttribute attributeIndex, List<TrainingScheduleSteps> grind, List<TrainingScheduleSteps> counter, double repeat, TrainingScheduleType trainingScheduleType)
        {

            if (!counter.Any(c => c.TrainingScheduleType == trainingScheduleType)
                              && !grind.Any(c => c.TrainingScheduleType == trainingScheduleType))
            {
                for (int j = 0; j < repeat; j++)
                {
                    grind.Add(new TrainingScheduleSteps { ForPlayerAttribute = attributeIndex, TrainingScheduleType = trainingScheduleType });
                }
            }
        }
        static void AddCounter(PlayerAttribute attributeIndex, List<TrainingScheduleSteps> counter, TrainingScheduleType trainingScheduleType, TrainingEffectModifier trainingEffectModifier)
        {
            /*
            if (!trainingEffectModifier.RemoveNegativeTraining)
            {
                if (!counter.Any(c => c.TrainingScheduleType == trainingScheduleType))
                    counter.Add(new TrainingScheduleSteps { ForPlayerAttribute = attributeIndex, TrainingScheduleType = trainingScheduleType });
            }*/
        }
        static List<TrainingScheduleSteps> ImproveAwarenessAndFlairTo(PlayerModelDouble player, int stageMinimum, bool maxPower,
           bool needShooting, bool needHeading, bool needmarking, TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects)
        {
           
            var preset= TrainingSchedulePreset.TrainingMatchAllWeek;

            var flairLeftToTrain = stageMinimum - player.Flair;
            var awarenessLeftToTrain = stageMinimum - player.Awareness;
            if (flairLeftToTrain > 0)
            {
                if (flairLeftToTrain > awarenessLeftToTrain / 2)
                    preset = TrainingSchedulePreset.FiveASideAllWeek;
                else if (needShooting || needHeading || needmarking)
                    preset = TrainingSchedulePreset.TrainingMatchAllWeek;
                else
                    preset = TrainingSchedulePreset.TrainingMatchAllWeek;
            }
            else
            {
                if (player.Awareness < player.Awareness || player.Flair < player.Flair)
                {
                    preset = TrainingSchedulePreset.TrainingMatchAllWeek;
                }
                else
                    preset = TrainingSchedulePreset.ControlAllWeek;
            }
            return preset.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.Awareness, TrainingScheduleType = x }).ToList();
        }
        static List<TrainingScheduleSteps> ImproveCoolnessAndAwarenessTo(PlayerModelDouble player, int stageMinimum, bool maxPower, float[][] trainingEffects)
        {
            if (player.Coolness < stageMinimum || player.Awareness < stageMinimum)
            {
                double coolnessLeftToTrain = attributeCap - player.Coolness;
                double awarenessLeftToTrain = attributeCap - player.Awareness;

                var presets = new List<TrainingScheduleType[]>();
                presets.Add(TrainingSchedulePreset.ControlAllWeek);
                presets.Add(TrainingSchedulePreset.TrainingMatchAllWeek);
                presets.Add(TrainingSchedulePreset.FiveASideAllWeek);
                var scores = new List<double>();
                var attributeIndices = new List<int>();
                attributeIndices.Add((int)PlayerAttribute.Coolness);
                attributeIndices.Add((int)PlayerAttribute.Awareness);
                var attributesLeftToTrain = new List<double>();
                attributesLeftToTrain.Add(coolnessLeftToTrain);
                attributesLeftToTrain.Add(awarenessLeftToTrain);


                foreach (var preset in presets)
                {
                    double score = 0;
                    foreach (var scheduleType in preset)
                    {
                        for (int i = 0; i < attributeIndices.Count; i++)
                        {
                            score += attributesLeftToTrain[i] *
                                trainingEffects[(int)scheduleType][attributeIndices[i]];
                        }
                    }
                    scores.Add(score);
                }
                var maxScoreIndex = scores.IndexOf(scores.Max());
                return presets[maxScoreIndex].Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.Coolness, TrainingScheduleType = x }).ToList();
            }
            return null;
        }
        static List<TrainingScheduleSteps> ImproveCoolnessAwarenessAndFlairTo(PlayerModelDouble player, int stageMinimum, bool maxPower, float[][] trainingEffects)
        {

            if (player.Coolness < stageMinimum || player.Awareness < stageMinimum || player.Flair < stageMinimum)
            {
                var coolnessLeftToTrain = attributeCap - player.Coolness;
                var flairLeftToTrain = attributeCap - player.Flair;
                var awarenessLeftToTrain = attributeCap - player.Awareness;

                var presets = new List<TrainingScheduleType[]>();
                presets.Add(TrainingSchedulePreset.ControlAllWeek);
                presets.Add(TrainingSchedulePreset.TrainingMatchAllWeek);
                presets.Add(TrainingSchedulePreset.FiveASideAllWeek);
                presets.Add(TrainingSchedulePreset.ImproveAwareness);
                var scores = new List<double>();
                var attributeIndices = new List<int>();
                attributeIndices.Add((int)PlayerAttribute.Coolness);
                attributeIndices.Add((int)PlayerAttribute.Awareness);
                attributeIndices.Add((int)PlayerAttribute.Flair);
                var attributesLeftToTrain = new List<double>();
                attributesLeftToTrain.Add(coolnessLeftToTrain);
                attributesLeftToTrain.Add(awarenessLeftToTrain);
                attributesLeftToTrain.Add(flairLeftToTrain);


                foreach (var preset in presets)
                {
                    double score = 0;
                    foreach (var scheduleType in preset)
                    {
                        for (int i = 0; i < attributeIndices.Count; i++)
                        {
                            score += attributesLeftToTrain[i] *
                                trainingEffects[(int)scheduleType][attributeIndices[i]];
                        }
                    }
                    scores.Add(score);
                }
                var maxScoreIndex = scores.IndexOf(scores.Max());
                return presets[maxScoreIndex].Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.Coolness, TrainingScheduleType = x }).ToList();
            }
            return null;
        }

        private static List<TrainingScheduleSteps> ImproveFitness(PlayerModelDouble player, bool needAcceleration, bool trainControl)
        {
            var preset = trainControl?TrainingSchedulePreset.FiveASideAllWeek: TrainingSchedulePreset.TrainingMatchAllWeek;
            if (player.Speed < attributeCap-1)
            {
                preset = TrainingSchedulePreset.SprintingAllWeek;
            }
            if (needAcceleration && player.Acceleration < attributeCap - 1)
            {
                preset=TrainingSchedulePreset.SprintingAllWeek;
            }
            return preset.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.Fitness, TrainingScheduleType = x }).ToList(); 
        }

        private static List<TrainingScheduleSteps> ImproveSpeedTo(PlayerModelDouble player,
            int stageMinimum, bool trainWeightLifting, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Speed < stageMinimum)
            {
                var preset= TrainingSchedulePreset.SprintingAllWeek;
                if (trainingEffectModifier.RemoveNegativeTraining)
                    preset=TrainingSchedulePreset.SprintingAllWeek;

                //weight training reduces speed so train it first
                else if (trainWeightLifting && player.Determination < stageMinimum &&
                    PositionRatings.Ratings[player.Position][(int)PlayerAttribute.Determination] > 0)
                    preset=TrainingSchedulePreset.SprintingWithWeightTraining;
                return preset.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.Speed, TrainingScheduleType = x }).ToList();
            }
            return null;
        }
        private static List<TrainingScheduleSteps> ImproveAgilityTo(PlayerModelDouble player, int stageMinimum)
        {
            if (player.Agility < stageMinimum)
            {
                return TrainingSchedulePreset.TrainingMatchAllWeek.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.Agility, TrainingScheduleType = x }).ToList(); ;
            }
            return null;
        }
        //unused, training match is much more useful for GKs
        private static List<TrainingScheduleSteps> ImproveGKAgilityTo(PlayerModelDouble player, int stageMinimum)
        {
            if (player.Agility < stageMinimum)
            {
                return TrainingSchedulePreset.GKAgility.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.Agility, TrainingScheduleType = x }).ToList(); ; 

            }
            return null;
        }
        private static List<TrainingScheduleSteps> ImproveAccelerationTo(PlayerModelDouble player, int stageMinimum, bool trainHeading, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Acceleration < stageMinimum)
            {
                var preset = TrainingSchedulePreset.SprintingAllWeek;
                if (trainingEffectModifier.RemoveNegativeTraining)
                    preset=TrainingSchedulePreset.SprintingAllWeek;
                else if (player.Heading < stageMinimum && trainHeading)
                    preset = TrainingSchedulePreset.SprintingWithHeading;
                return preset.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.Acceleration, TrainingScheduleType = x }).ToList();
            }
            return null;
        }
        //rarely useful as shooting is trained during training match
        private static List<TrainingScheduleSteps> ImproveShootingTo(PlayerModelDouble player, int stageMinimum)
        {
            if (player.Shooting < stageMinimum) return TrainingSchedulePreset.TrainingMatchAllWeek.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.Shooting, TrainingScheduleType = x }).ToList(); ;
            return null;
        }
        private static List<TrainingScheduleSteps> ImprovePassingTo(PlayerModelDouble player, int stageMinimum)
        {
            if (player.Passing < stageMinimum) return TrainingSchedulePreset.TrainingMatchAllWeek.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.Passing, TrainingScheduleType = x }).ToList(); ;
            return null;
        }

        private static List<TrainingScheduleSteps> ImproveHeadingTo(PlayerModelDouble player, int stageMinimum, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Heading < stageMinimum)
            {
                var preset = TrainingSchedulePreset.HeadingAllWeek;
                if (trainingEffectModifier.RemoveNegativeTraining)
                {
                    if (player.Shooting == attributeCap
                        && player.Passing == attributeCap
                        && player.Control == attributeCap
                        && player.Dribbling == attributeCap)
                    {
                        if (player.TackleDetermination < attributeCap || player.TackleSkill < attributeCap)
                            preset= TrainingSchedulePreset.TrainingMatchAllWeek;
                        else if (player.Leadership < attributeCap && trainingEffectModifier.PassingTrainLeadership)
                            preset=TrainingSchedulePreset.TrainingMatchAllWeek;
                        else if (player.Greed < attributeCap && trainingEffectModifier.ShootingTrainGreed)
                            preset=TrainingSchedulePreset.TrainingMatchAllWeek;
                        else if (player.ThrowIn < attributeCap && trainingEffectModifier.ThrowingTrainThrowIn)
                            preset=TrainingSchedulePreset.TrainingMatchAllWeek;
                    }
                    else
                        preset=TrainingSchedulePreset.HeadingAllWeek;
                }
                else if (stageMinimum < attributeCap)
                    preset=TrainingSchedulePreset.HeadingAllWeek;
                else
                {
                    if (player.Shooting > almostAttributeCap && player.Passing > almostAttributeCap)
                        preset=TrainingSchedulePreset.ImproveHeading;
                    else 
                        preset=TrainingSchedulePreset.HeadingWithSprint;
                }
                return preset.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.Heading, TrainingScheduleType = x }).ToList();
            }
            return null;
        }

        private static List<TrainingScheduleSteps> ImproveControlTo(PlayerModelDouble player, int stageMinimum)
        {
            if (player.Control < stageMinimum) return TrainingSchedulePreset.ControlAllWeek.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.Control, TrainingScheduleType = x }).ToList();
            return null;
        }
        private static List<TrainingScheduleSteps> ImproveDribbleTo(PlayerModelDouble player, int stageMinimum)
        {
            if (player.Dribbling < stageMinimum) return TrainingSchedulePreset.ControlAllWeek.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.Dribbling, TrainingScheduleType = x }).ToList();
            return null;
        }
        static List<TrainingScheduleSteps> ImproveTackleDeterminationAndSkillTo(PlayerModelDouble player, int stageMinimum, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.TackleSkill < stageMinimum || player.TackleDetermination < stageMinimum)
            {
                var tackleDeterminatonLeftToTrain = attributeCap - player.TackleDetermination;
                var tackleSkillLeftToTrain = attributeCap - player.TackleSkill;
                var markScore = tackleDeterminatonLeftToTrain * 8 + tackleDeterminatonLeftToTrain * 4;
                var tacklingScore = tackleDeterminatonLeftToTrain * 4 + tackleDeterminatonLeftToTrain * 8;
                var preset = TrainingSchedulePreset.MarkingAllWeek;
                if (trainingEffectModifier.RemoveNegativeTraining || stageMinimum < attributeCap)
                {
                    if (markScore < tacklingScore)
                    {
                        preset =TrainingSchedulePreset.TacklingSkillAllWeek;
                    }
                    else
                        preset = TrainingSchedulePreset.MarkingAllWeek;
                }
                else
                {
                    if (markScore < tacklingScore)
                    {
                        preset = TrainingSchedulePreset.ImproveTacklingSkill;
                    }
                    else
                        preset = TrainingSchedulePreset.ImproveMarking;
                }
                return preset.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.TackleDetermination, TrainingScheduleType = x }).ToList();
            }
            return null;

        }
        private static List<TrainingScheduleSteps> ImproveTackleSkillTo(PlayerModelDouble player, int stageMinimum,
            TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects)
        {
            if (player.TackleSkill < stageMinimum)
            {
                var preset = TrainingSchedulePreset.TrainingMatchAllWeek;
                if (trainingEffectModifier.RemoveNegativeTraining)
                    preset =TrainingSchedulePreset.TacklingSkillAllWeek;
                if(player.TackleSkill<player.TackleDetermination)
                    preset = TrainingSchedulePreset.TacklingSkillAllWeek;
                return preset.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.TackleSkill, TrainingScheduleType = x }).ToList();
            }
            return null;
        }
        private static List<TrainingScheduleSteps> ImproveAwarenessTo(PlayerModelDouble player, int stageMinimum, bool maxPower, TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects)
        {
            bool trainConsistency = PositionRatings.Ratings[player.Position][(int)PlayerAttribute.Consistency] > 0;
            bool trainDribbling = PositionRatings.Ratings[player.Position][(int)PlayerAttribute.Dribbling] > 0;
            if (player.Awareness < stageMinimum)
            {
                var preset = TrainingSchedulePreset.TrainingMatchAllWeek;
                if (player.Passing < attributeCap || player.Heading < attributeCap || player.TackleDetermination < attributeCap || player.TackleSkill < attributeCap)
                {
                    preset = TrainingSchedulePreset.TrainingMatchAllWeek;
                }
                else if(trainDribbling && player.Dribbling<stageMinimum)
                {
                    preset = TrainingSchedulePreset.ControlAllWeek;
                }
                else
                {
                    //if (maxPower || trainingEffectModifier.RemoveNegativeTraining)
                        preset =TrainingSchedulePreset.ZonalDefenceAllWeek;                    
                }
                return preset.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.Awareness, TrainingScheduleType = x }).ToList();
            }
            return null;
        }
        private static List<TrainingScheduleSteps> ImproveKickingTo(PlayerModelDouble player, int stageMinimum, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Kicking < stageMinimum)
            {
                var preset= TrainingSchedulePreset.ImproveKicking;
                if (trainingEffectModifier.RemoveNegativeTraining || stageMinimum < attributeCap)
                    preset=TrainingSchedulePreset.KickingAllWeek;
                return preset.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.Kicking, TrainingScheduleType = x }).ToList();
            }
            return null;
        }
        private static List<TrainingScheduleSteps> ImproveThrowingTo(PlayerModelDouble player, int stageMinimum)
        {
            if (player.Throwing < stageMinimum)
            {
                return TrainingSchedulePreset.ThrowingAllWeek.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.Throwing, TrainingScheduleType = x }).ToList();
            }
            return null;
        }
        private static List<TrainingScheduleSteps> ImproveHandlingTo(PlayerModelDouble player, int stageMinimum, TrainingEffectModifier trainingEffectModifier)
        {
            var preset = TrainingSchedulePreset.ImproveHandling;
            if (player.Handling < stageMinimum)
            {
                if (trainingEffectModifier.RemoveNegativeTraining || stageMinimum < attributeCap)
                    preset = TrainingSchedulePreset.HandlingAllWeek;
                return preset.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.Handling, TrainingScheduleType = x }).ToList();
            }
            return null;
        }
        private static List<TrainingScheduleSteps> ImproveLeadershipTo(PlayerModelDouble player, int stageMinimum, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Leadership < stageMinimum && trainingEffectModifier.PassingTrainLeadership)
            {
                return TrainingSchedulePreset.FiveASideAllWeek.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.Leadership, TrainingScheduleType = x }).ToList(); 
            }
            return null;
        }

        private static List<TrainingScheduleSteps> ImproveConsistencyTo(PlayerModelDouble player, int stageMinimum)
        {
            if (player.Consistency < stageMinimum) return TrainingSchedulePreset.ControlAllWeek.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.Consistency, TrainingScheduleType = x }).ToList(); ;
            return null;
        }

        private static List<TrainingScheduleSteps> ImproveDeterminationTo(PlayerModelDouble player, int stageMinimum,
            TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Determination < stageMinimum)
            {
                var preset = TrainingSchedulePreset.ImproveDetermination;
                if (trainingEffectModifier.RemoveNegativeTraining || stageMinimum < attributeCap)
                    preset =TrainingSchedulePreset.WeightTrainingAllWeek;
                return preset.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.Determination, TrainingScheduleType = x }).ToList(); ;
            }
            return null;
        }

    } 
}