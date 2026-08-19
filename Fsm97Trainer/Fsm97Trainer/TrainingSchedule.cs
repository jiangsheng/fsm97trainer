using FSM97Lib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        public const int almostAttributeCap = 87;
        public const double ConstantFastCeiling = 0.9999;
        public const double EPS = 1e-9;
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
                        schedule[1].TrainingScheduleType = schedule[3].TrainingScheduleType = schedule[5].TrainingScheduleType = TrainingActivityType.Physiotherapist;
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

            bool trainAcceleration = trainingScheduleCalculationState.RequiredAttributes.Contains(PlayerAttribute.Acceleration);
            bool trainingShooting = trainingScheduleCalculationState.RequiredAttributes.Contains(PlayerAttribute.Shooting);
            bool trainingPassing = trainingScheduleCalculationState.RequiredAttributes.Contains(PlayerAttribute.Passing);
            bool trainHeading = trainingScheduleCalculationState.RequiredAttributes.Contains(PlayerAttribute.Heading);

            bool trainControl = trainingScheduleCalculationState.RequiredAttributes.Contains(PlayerAttribute.Control);
            bool trainDribbling = trainingScheduleCalculationState.RequiredAttributes.Contains(PlayerAttribute.Dribbling);


            bool trainTackleSkill = trainingScheduleCalculationState.RequiredAttributes.Contains(PlayerAttribute.TackleSkill);
            bool trainTackleDetermination = trainingScheduleCalculationState.RequiredAttributes.Contains(PlayerAttribute.TackleDetermination);

            bool trainCoolness = trainingScheduleCalculationState.RequiredAttributes.Contains(PlayerAttribute.Coolness);
            bool trainFlair = trainingScheduleCalculationState.RequiredAttributes.Contains(PlayerAttribute.Flair);

            bool trainDetermination = trainingScheduleCalculationState.RequiredAttributes.Contains(PlayerAttribute.Determination);
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
                    Debug.Assert(result[0].TrainingScheduleType != TrainingActivityType.TrainingMatch);
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
            var result = ImproveHandlingTo(player, almostAttributeCap, trainingScheduleCalculationState.TrainingEffectModifier);
            if(result != null)
                return result;
            result = ImproveKickingTo(player, almostAttributeCap, trainingScheduleCalculationState.TrainingEffectModifier);
            if (result != null)
                return result;
            result = ImproveThrowingTo(player, almostAttributeCap);
            if (result != null)
                return result;

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

            bool trainAcceleration = trainingScheduleCalculationState.RequiredAttributes.Contains(PlayerAttribute.Acceleration);
            
            bool trainingShooting = trainingScheduleCalculationState.RequiredAttributes.Contains(PlayerAttribute.Shooting);
            bool trainingPassing = trainingScheduleCalculationState.RequiredAttributes.Contains(PlayerAttribute.Passing);
            bool trainHeading = trainingScheduleCalculationState.RequiredAttributes.Contains(PlayerAttribute.Heading);
            bool trainControl= trainingScheduleCalculationState.RequiredAttributes.Contains(PlayerAttribute.Control);
            bool trainDribbling = trainingScheduleCalculationState.RequiredAttributes.Contains(PlayerAttribute.Dribbling);
            
            bool trainTackleSkill = trainingScheduleCalculationState.RequiredAttributes.Contains(PlayerAttribute.TackleSkill);
            bool trainTackleDetermination = trainingScheduleCalculationState.RequiredAttributes.Contains(PlayerAttribute.TackleDetermination);
            bool trainCoolness= trainingScheduleCalculationState.RequiredAttributes.Contains(PlayerAttribute.Coolness);
            bool trainFlair = trainingScheduleCalculationState.RequiredAttributes.Contains(PlayerAttribute.Flair);

            bool trainDetermination = trainingScheduleCalculationState.RequiredAttributes.Contains(PlayerAttribute.Determination);

            if (attributeLeftToTrain == null && bottleneckAttributeIndex == null)
            {                
                attributeLeftToTrain = trainingScheduleCalculationState.AttributesLeftToTrain;
                bottleneckAttributeIndex = trainingScheduleCalculationState.BottleneckAttributes;

                if (AreAllRevelantAttribtesAboveAlmostCap(trainingScheduleCalculationState, position, trainingEffectModifier))
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
                    if (repeat < 1) repeat = 1; 
                    if (repeat >7) repeat =7;

                    switch (topBottleneckAttribute.AttributeIndex)
                    {
                        case PlayerAttribute.Speed:
                        case PlayerAttribute.Acceleration:

                            AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.Sprinting);
                            AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingActivityType.TrainingMatch, trainingEffectModifier);

                            break;
                        case PlayerAttribute.Agility:

                            if (player.Position != (int)PlayerPosition.GK)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.TrainingMatch);
                            }
                            else
                            {
                                if (player.Kicking >= TrainingSchedule.attributeCap)
                                    AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.TrainingMatch);
                                else
                                {
                                    //it is usually not good to maxed this last
                                    AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.GoalKeeping);
                                    AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingActivityType.Kicking, trainingEffectModifier);
                                    AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingActivityType.Throwing, trainingEffectModifier);
                                    AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingActivityType.TrainingMatch, trainingEffectModifier);
                                }
                            }
                            break;
                        case PlayerAttribute.Shooting:                        
                        case PlayerAttribute.Leadership:
                            AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.TrainingMatch);
                            break;
                        case PlayerAttribute.Passing:
                            if (trainingShooting && attributeLeftToTrain.Attributes[(int)PlayerAttribute.Shooting] > 0)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.TrainingMatch);
                            }
                            else if (trainHeading && attributeLeftToTrain.Attributes[(int)PlayerAttribute.Heading] > 0)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.TrainingMatch);
                            }
                            else if (trainTackleDetermination && attributeLeftToTrain.Attributes[(int)PlayerAttribute.TackleDetermination] > 0)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.TrainingMatch);
                            }
                            else if (trainCoolness && attributeLeftToTrain.Attributes[(int)PlayerAttribute.Coolness] > 0)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.TrainingMatch);
                            }
                            else
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.FiveASide);
                            break;
                        case PlayerAttribute.Fitness:
                            if (Math.Max(attributeLeftToTrain.Speed, attributeLeftToTrain.Acceleration) > 0)
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.Sprinting);
                            else
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.TrainingMatch);
                            break;

                        case PlayerAttribute.Heading:
                            AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.Heading);
                            AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingActivityType.Sprinting, trainingEffectModifier);
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
                                    AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.TrainingMatch);
                                }
                                else if (trainHeading && attributeLeftToTrain.Attributes[(int)PlayerAttribute.Heading] > 0)
                                {
                                    AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.TrainingMatch);
                                }
                                else if (trainTackleDetermination && attributeLeftToTrain.Attributes[(int)PlayerAttribute.TackleDetermination] > 0)
                                {
                                    AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.TrainingMatch);
                                }
                                else if (trainCoolness && attributeLeftToTrain.Attributes[(int)PlayerAttribute.Coolness] > 0)
                                {
                                    AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.TrainingMatch);
                                }
                                else
                                    AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.FiveASide);
                            }
                            else if (attributeLeftToTrain.Coolness * 3 < attributeLeftToTrain.Awareness * 2)
                            {
                                if(!(player.Shooting == attributeCap && player.Passing == attributeCap && player.Heading == attributeCap))
                                //should use training match 
                                    AddGrind(
                                        topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.TrainingMatch);
                                else
                                    AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.Control);
                            }
                            else if (attributeLeftToTrain.Coolness == 0 && attributeLeftToTrain.Dribbling < attributeLeftToTrain.Control / 4)
                            {
                                //should use five a side instead
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.FiveASide);
                            }
                            else
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.Control);
                            }
                            break;
                        case PlayerAttribute.Dribbling:
                            if (player.Dribbling < projectedAttributesAfterTrainingMatch.Dribbling)
                            {
                                Debug.Assert(!(player.Shooting == attributeCap && player.Passing == attributeCap && player.Heading == attributeCap));
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.TrainingMatch);
                            }
                            else
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.Control);
                            }
                            break;
                        case PlayerAttribute.TackleDetermination:
                            if (hasTackleSkill) break;
                            hasTackleDetermination = true;
                            var tackleDeterminationLeftToTrain = attributeLeftToTrain.TackleDetermination;
                            if (player.TackleDetermination < projectedAttributesAfterTrainingMatch.TackleDetermination)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.TrainingMatch);
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
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.TrainingMatch);
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
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.FiveASide);
                            }
                            else if (tackleDeterminationLeftToTrain < attributeLeftToTrain.TackleSkill / 2)
                            {
                                //is it covered by tackling training?
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.Tackling);
                                AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingActivityType.TrainingMatch, trainingEffectModifier);
                                AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingActivityType.Control, trainingEffectModifier);
                            }
                            else
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.Marking);
                                AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingActivityType.TrainingMatch, trainingEffectModifier);
                                AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingActivityType.Control, trainingEffectModifier);
                            }

                            break;
                        case PlayerAttribute.TackleSkill:
                            if (hasTackleDetermination ) break;
                            hasTackleSkill = true;
                            var tackleSkillLeftToTrain = attributeLeftToTrain.TackleSkill;
                            //still in initial training match grinding?
                            if (player.TackleSkill < projectedAttributesAfterTrainingMatch.TackleSkill)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.TrainingMatch);
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
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.TrainingMatch);
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
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.FiveASide);
                            }
                            //is it fully covered by reqired marking?
                            else if (tackleSkillLeftToTrain < attributeLeftToTrain.TackleDetermination / 2)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.Marking);
                                AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingActivityType.TrainingMatch, trainingEffectModifier);
                                AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingActivityType.Control, trainingEffectModifier);
                            }
                            else
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.Tackling);
                                AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingActivityType.TrainingMatch, trainingEffectModifier);
                                AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingActivityType.Control, trainingEffectModifier);
                            }
                            break;

                        case PlayerAttribute.Coolness:
                            var awarenessLeftToTrain = attributeLeftToTrain.Awareness;

                            if (player.Coolness < projectedAttributesAfterTrainingMatch.Coolness)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.TrainingMatch);
                            }
                            else if (attributeLeftToTrain.Attributes[(int)PlayerAttribute.Consistency] > 0 && player.Consistency < almostAttributeCap)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.Control);
                            }
                            else if (attributeLeftToTrain.Coolness * 3 <= awarenessLeftToTrain * 2)
                            {
                                //should use training match 
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.TrainingMatch);
                            }
                            else
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.Control);
                            }
                            break;

                        case PlayerAttribute.Awareness:
                            Debug.Assert(player.Awareness < attributeCap);
                            if (player.Awareness < projectedAttributesAfterTrainingMatch.Awareness)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.TrainingMatch);
                            }
                            else if (attributeLeftToTrain.Attributes[(int)PlayerAttribute.Consistency] > 0 && player.Consistency < attributeCap)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.Control);
                            }
                            else if (attributeLeftToTrain.Attributes[(int)PlayerAttribute.Control] > 0 && trainControl)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.Control);
                            }
                            else if (attributeLeftToTrain.Attributes[(int)PlayerAttribute.Dribbling] > 0 && trainDribbling)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.Control);
                            }
                            else if (position == PlayerPosition.GK && player.Passing == attributeCap
                                    && player.Control == attributeCap
                                    && attributeLeftToTrain.Handling >=1
                                    && attributeLeftToTrain.Agility >= 1)
                            {
                                Debug.Assert(player.Kicking < TrainingSchedule.attributeCap);
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.GoalKeeping);
                                AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingActivityType.Kicking, trainingEffectModifier);
                            }
                            else if (attributeLeftToTrain.Coolness * 3 > attributeLeftToTrain.Awareness * 2)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.Control);
                            }
                            else if (attributeLeftToTrain.Flair * 3 > attributeLeftToTrain.Awareness * 4)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.FiveASide);
                            }
                            else if (attributeLeftToTrain.Coolness >= 1 || attributeLeftToTrain.Flair >= 1)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.TrainingMatch);
                            }
                            else
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.ZonalDefence);
                            }


                            break;
                        case PlayerAttribute.Flair:


                            if (player.Flair < projectedAttributesAfterTrainingMatch.Flair)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.TrainingMatch);
                            }
                            else if (attributeLeftToTrain.Attributes[(int)PlayerAttribute.Consistency] > 0 && player.Consistency < attributeCap)
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.Control);
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
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.TrainingMatch);
                            }
                            else if (flairLeftToTrain / (double)2 <= new[] {
                                    attributeLeftToTrain.Attributes[(int)PlayerAttribute.Control]/(double)8,
                                    attributeLeftToTrain.Attributes[(int)PlayerAttribute.Dribbling]/(double)6,
                                    attributeLeftToTrain.Attributes[(int)PlayerAttribute.Coolness]/(double)8,
                                    attributeLeftToTrain.Attributes[(int)PlayerAttribute.Consistency]/(double)6 }.Max())
                            {
                                //is it fully covered by control training?
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.Control);
                            }
                            else
                            {
                                AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.FiveASide);
                            }
                            break;
                        case PlayerAttribute.Kicking:
                            AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.Kicking);
                            AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingActivityType.Throwing, trainingEffectModifier);
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
                                AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingActivityType.TrainingMatch, trainingEffectModifier);
                            }
                            else
                            {
                                AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingActivityType.GoalKeeping, trainingEffectModifier);
                                AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingActivityType.Kicking, trainingEffectModifier);
                                AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingActivityType.Throwing, trainingEffectModifier);

                            }
                            break;
                        case PlayerAttribute.Handling:
                            AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.Handling);
                            AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingActivityType.Kicking, trainingEffectModifier);
                            break;
                        case PlayerAttribute.Throwing:
                        case PlayerAttribute.ThrowIn:
                            AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.Throwing);
                            break;
                        case PlayerAttribute.Consistency:
                            Debug.Assert(position == PlayerPosition.GK|| position == PlayerPosition.LB|| position == PlayerPosition.RB|| position == PlayerPosition.CD
                                || trainingScheduleCalculationState.AlwaysTrainConsistency);
                            AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.Control);
                            break;

                        case PlayerAttribute.Determination:
                            AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.WeightTraining);
                            AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingActivityType.TrainingMatch, trainingEffectModifier);
                            break;
                        case PlayerAttribute.Greed:
                            AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingActivityType.Shooting);
                            break;
                        default:
                            //for other attributes, just ignore them
                            continue;
                    }
                }
                if (bottleneckAttributeIndex[0].AttributeIndex == PlayerAttribute.Control)
                {
                    if (genericTrainingGrind[0].TrainingScheduleType == TrainingActivityType.TrainingMatch)
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
                        TrainingScheduleType = TrainingActivityType.None
                    });
                }
                return emptyResult;
            }
            bool addCounter =  AreAllRevelantAttribtesAboveAlmostCap(trainingScheduleCalculationState, position,trainingEffectModifier);
            var result = new List<TrainingScheduleSteps>();
            if (addCounter)
            {
                Debug.Assert(genericTrainingGrind.Count > 0);
                if (genericTrainingGrind.Count == 1)
                {
                    switch (genericTrainingGrind[0].TrainingScheduleType)
                    {
                        case TrainingActivityType.Heading:
                            break;
                        case TrainingActivityType.Sprinting:
                            break;
                        case TrainingActivityType.ZonalDefence:
                            break;
                        case TrainingActivityType.Marking:
                            break;
                        case TrainingActivityType.Tackling:
                            break;
                        case TrainingActivityType.Handling:
                            for (int i = 0; i < 4; i++)
                            {
                                genericTrainingGrind.Add(new TrainingScheduleSteps() { TrainingScheduleType = TrainingActivityType.Handling, ForPlayerAttribute = genericTrainingGrind[0].ForPlayerAttribute });
                            }
                            for (int i = 0; i < 2; i++)
                            {
                                genericTrainingCounter.Add(new TrainingScheduleSteps() { TrainingScheduleType = TrainingActivityType.Kicking, ForPlayerAttribute = genericTrainingGrind[0].ForPlayerAttribute });
                            }
                            break;
                        case TrainingActivityType.GoalKeeping:
                            break;
                        case TrainingActivityType.Kicking:
                            for (int i = 0; i < 4; i++)
                            {
                                genericTrainingGrind.Add(new TrainingScheduleSteps() { TrainingScheduleType = TrainingActivityType.Kicking, ForPlayerAttribute = genericTrainingGrind[0].ForPlayerAttribute });
                            }
                            for (int i = 0; i < 2; i++)
                            {
                                genericTrainingCounter.Add(new TrainingScheduleSteps() { TrainingScheduleType = TrainingActivityType.Throwing, ForPlayerAttribute = genericTrainingGrind[0].ForPlayerAttribute });
                            }

                            break;
                        case TrainingActivityType.Throwing:
                            break;
                    }
                    foreach (var item in genericTrainingGrind)
                    {
                        result.Add(item);
                    }
                    foreach (var item in genericTrainingCounter)
                    {
                        result.Add(item);
                    }
                }
                //Debug.Assert(counter.Count > 0);
                while (result.Count  < 7)
                {
                    foreach (var item in genericTrainingGrind)
                    {
                        result.Add(item);
                    }
                    foreach (var item in genericTrainingCounter)
                    {
                        result.Add(item);
                    }
                }
                if (result.Count  > 7)
                {
                    result.RemoveRange(7 , result.Count - 7);
                }
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
        static bool AreAllRevelantAttribtesAboveAlmostCap(TrainingScheduleCalculationState trainingScheduleCalculationState, PlayerPosition position, TrainingEffectModifier trainingEffectModifier)
        {   
            var player= trainingScheduleCalculationState.Player;
            for (int i = 0; i < (int)PlayerAttribute.Count; i++)
            {
                if (trainingScheduleCalculationState.RequiredAttributes.Contains((PlayerAttribute)i))
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
            var trainingShooting = trainingScheduleCalculationState.RequiredAttributes.Contains(PlayerAttribute.Shooting);
            var trainHeading = trainingScheduleCalculationState.RequiredAttributes.Contains(PlayerAttribute.Heading);

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
                    if (handlingNeeded >0)
                    {
                        AddGrind(PlayerAttribute.Handling, finalGrind, finalCounter, handlingNeeded + TrainingSchedule.ConstantFastCeiling, TrainingActivityType.Handling);
                    }
                    var kickingNeeded = finalAttributeLeftToTrain.Kicking;
                    if (handlingNeeded >0 && !trainingEffectModifier.RemoveNegativeTraining)
                    {
                        kickingNeeded += (handlingNeeded + 3) / 4;
                    }
                    if (kickingNeeded >0)
                    {
                        AddGrind(PlayerAttribute.Kicking, finalGrind, finalCounter, kickingNeeded + TrainingSchedule.ConstantFastCeiling, TrainingActivityType.Kicking);
                        AddCounter(PlayerAttribute.Kicking, finalCounter, TrainingActivityType.Throwing, trainingEffectModifier);
                        agilityLost+= kickingNeeded;
                    }
                    var throwingNeeded = finalAttributeLeftToTrain.ThrowIn;
                    if (kickingNeeded>0 && !trainingEffectModifier.RemoveNegativeTraining)
                    {
                        throwingNeeded += kickingNeeded / 4;
                    }
                    if (throwingNeeded >0)
                    {
                        AddGrind(PlayerAttribute.Throwing, finalGrind, finalCounter, throwingNeeded + TrainingSchedule.ConstantFastCeiling, TrainingActivityType.Throwing);
                    }
                    break;
                case PlayerPosition.LB:
                case PlayerPosition.RB:
                    var determinationNeeded= finalAttributeLeftToTrain.Determination;
                    if (determinationNeeded>0)
                    {
                        AddGrind(PlayerAttribute.Determination, finalGrind, finalCounter, determinationNeeded + TrainingSchedule.ConstantFastCeiling, TrainingActivityType.WeightTraining);
                        if (!trainingEffectModifier.RemoveNegativeTraining)
                        {
                            AddCounter(PlayerAttribute.Speed, finalCounter, TrainingActivityType.TrainingMatch, trainingEffectModifier);
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
                        AddGrind(PlayerAttribute.Consistency, finalGrind, finalCounter, consistencyNeeded + TrainingSchedule.ConstantFastCeiling, TrainingActivityType.Control);
                        controlAdded += consistencyNeeded;
                    }
                    break;
            }

            double sprintNeededForSpeed = finalAttributeLeftToTrain.Speed;
            if (sprintNeededForSpeed >0)
            {
                AddGrind(PlayerAttribute.Speed, finalGrind, finalCounter, sprintNeededForSpeed + TrainingSchedule.ConstantFastCeiling, TrainingActivityType.Sprinting);
                sprintAdded += sprintNeededForSpeed;                
                skillLost += sprintNeededForSpeed;
                if (finalCounter.Where(t => t.TrainingScheduleType == TrainingActivityType.TrainingMatch).Count() < (sprintNeededForSpeed + 1) / 2)
                {
                    AddCounter(PlayerAttribute.Speed, finalCounter, TrainingActivityType.TrainingMatch, trainingEffectModifier);
                    skillLost = 0;
                    agilityLost = 0;
                    trainingMatchesAdded++;
                }
            }

            double sprintNeededForAcceleration = finalAttributeLeftToTrain.Acceleration;
            if (sprintNeededForAcceleration > 0 && sprintNeededForAcceleration > sprintNeededForSpeed)
            {
                var repeat = (int)(sprintNeededForAcceleration - sprintNeededForSpeed + TrainingSchedule.ConstantFastCeiling);
                if (repeat == 0) repeat = 1;
                if (repeat > 7) repeat = 7;
                AddGrind(PlayerAttribute.Acceleration, finalGrind, finalCounter, repeat, TrainingActivityType.Sprinting);
                skillLost += sprintNeededForAcceleration - sprintNeededForSpeed;
                if (trainingMatchesAdded == 0)
                {
                    AddCounter(PlayerAttribute.Acceleration, finalCounter, TrainingActivityType.TrainingMatch, trainingEffectModifier);
                    skillLost = 0;
                    agilityLost = 0;
                    trainingMatchesAdded++;
                }
            }
            double agilityNeeded = finalAttributeLeftToTrain.Agility- trainingMatchesAdded/2+ agilityLost/2;
            if (agilityNeeded < 0) agilityNeeded = 0;
            if (agilityNeeded > TrainingSchedule.EPS)
            {
                AddGrind(PlayerAttribute.Agility, finalGrind, finalCounter, agilityNeeded + TrainingSchedule.ConstantFastCeiling, TrainingActivityType.TrainingMatch);
                trainingMatchesAdded += agilityNeeded;
            }
            double shootingNeeded = finalAttributeLeftToTrain.Shooting- trainingMatchesAdded;
            if (shootingNeeded < 0) shootingNeeded = 0;
            if (shootingNeeded > TrainingSchedule.EPS)
            {
                AddGrind(PlayerAttribute.Shooting, finalGrind, finalCounter, shootingNeeded + TrainingSchedule.ConstantFastCeiling, TrainingActivityType.TrainingMatch);
                trainingMatchesAdded += shootingNeeded;
            }
            double passingNeeded = finalAttributeLeftToTrain.Passing - trainingMatchesAdded;
            if (passingNeeded < 0) passingNeeded = 0;
            if (passingNeeded > TrainingSchedule.EPS && passingNeeded > shootingNeeded)
            {
                int repeat = (int)(passingNeeded - shootingNeeded + TrainingSchedule.ConstantFastCeiling);
                if (repeat == 0) repeat = 1;
                if (repeat > 7) repeat = 7;

                if (trainingShooting && finalAttributeLeftToTrain.Attributes[(int)PlayerAttribute.Shooting] > 0)
                {
                    AddGrind(PlayerAttribute.Passing, finalGrind, finalCounter, repeat, TrainingActivityType.TrainingMatch);
                }
                else if (trainHeading && finalAttributeLeftToTrain.Attributes[(int)PlayerAttribute.Heading] > 0)
                {
                    AddGrind(PlayerAttribute.Passing, finalGrind, finalCounter, repeat, TrainingActivityType.TrainingMatch);
                }
                else if (trainingScheduleCalculationState.RequiredAttributes.Contains(PlayerAttribute.TackleDetermination) && finalAttributeLeftToTrain.Attributes[(int)PlayerAttribute.TackleDetermination] > 0)
                {
                    AddGrind(PlayerAttribute.Passing, finalGrind, finalCounter, repeat, TrainingActivityType.TrainingMatch);
                }
                else if (trainingScheduleCalculationState.RequiredAttributes.Contains(PlayerAttribute.Coolness) && finalAttributeLeftToTrain.Attributes[(int)PlayerAttribute.Coolness] > 0)
                {
                    AddGrind(PlayerAttribute.Passing, finalGrind, finalCounter, repeat, TrainingActivityType.TrainingMatch);
                }
                else
                    AddGrind(PlayerAttribute.Passing, finalGrind, finalCounter, repeat, TrainingActivityType.FiveASide);
                trainingMatchesAdded += repeat;
            }
            double controlNeeded = finalAttributeLeftToTrain.Control- trainingMatchesAdded / 2;
            if (controlNeeded < 0) controlNeeded = 0;
            if (controlNeeded > TrainingSchedule.EPS)
            {
                if(finalAttributeLeftToTrain.Dribbling>0|| finalAttributeLeftToTrain.Coolness>0|| finalAttributeLeftToTrain.Consistency >0)
                    AddGrind(PlayerAttribute.Control, finalGrind, finalCounter, controlNeeded + TrainingSchedule.ConstantFastCeiling, TrainingActivityType.Control);
                else
                    AddGrind(PlayerAttribute.Control, finalGrind, finalCounter, controlNeeded + TrainingSchedule.ConstantFastCeiling, TrainingActivityType.FiveASide);
                controlAdded += controlNeeded;
            }
            double driblingNeeded = finalAttributeLeftToTrain.Dribbling - trainingMatchesAdded / 4;            
            if (driblingNeeded <0) driblingNeeded = 0;            
            if (driblingNeeded >0  && driblingNeeded > controlNeeded)
            {
                var repeat = (int)(driblingNeeded - controlNeeded + TrainingSchedule.ConstantFastCeiling);
                if (repeat == 0) repeat = 1;
                if (repeat >=7) repeat = 7;

                AddGrind(PlayerAttribute.Dribbling, finalGrind, finalCounter, repeat, TrainingActivityType.Control);
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
                AddGrind(PlayerAttribute.Coolness, finalGrind, finalCounter, coolnessNeeded + TrainingSchedule.ConstantFastCeiling, TrainingActivityType.Control);
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
                    AddGrind(PlayerAttribute.Flair, finalGrind, finalCounter, flairNeeded - awarenessNeeded + TrainingSchedule.ConstantFastCeiling, TrainingActivityType.FiveASide);
                    fiveaSideAdded += flairNeeded - awarenessNeeded;
                }
                else {
                    trainingMatchAddedForFlair = awarenessNeeded;
                    AddGrind(PlayerAttribute.Flair, finalGrind, finalCounter, awarenessNeeded + TrainingSchedule.ConstantFastCeiling, TrainingActivityType.TrainingMatch);
                    trainingMatchesAdded += trainingMatchAddedForFlair;
                }
            }
            awarenessNeeded -= (fiveaSideAdded  + trainingMatchAddedForFlair) / 2;
            if (awarenessNeeded < 0) awarenessNeeded = 0;
            if (awarenessNeeded >0)
            {
                if (skillLost >0)
                {
                    AddGrind(PlayerAttribute.Awareness, finalGrind, finalCounter, (skillLost+1)/2 + TrainingSchedule.ConstantFastCeiling, TrainingActivityType.TrainingMatch);
                    if(awarenessNeeded> (skillLost + 1) / 2)
                        AddGrind(PlayerAttribute.Awareness, finalGrind, finalCounter, awarenessNeeded-skillLost + TrainingSchedule.ConstantFastCeiling, TrainingActivityType.TrainingMatch);
                    skillLost = 0;
                }
                else if (controlNeeded >0)
                {
                    AddGrind(PlayerAttribute.Awareness, finalGrind, finalCounter, awarenessNeeded + TrainingSchedule.ConstantFastCeiling, TrainingActivityType.FiveASide);
                    fiveaSideAdded += awarenessNeeded;
                }
                else if (flairNeeded <1 && coolnessNeeded <1 )
                {
                    AddGrind(PlayerAttribute.Awareness, finalGrind, finalCounter, awarenessNeeded + TrainingSchedule.ConstantFastCeiling, TrainingActivityType.ZonalDefence);
                    if (trainingMatchesAdded == 0 && !trainingEffectModifier.RemoveNegativeTraining)
                    {
                        AddCounter(PlayerAttribute.Awareness, finalCounter, TrainingActivityType.TrainingMatch, trainingEffectModifier);
                        trainingMatchesAdded+= awarenessNeeded;
                    }
                }
                else if (coolnessNeeded * 3 > awarenessNeeded * 2 && finalAttributeLeftToTrain.Coolness >0)
                {
                    AddGrind(PlayerAttribute.Awareness, finalGrind, finalCounter, awarenessNeeded + TrainingSchedule.ConstantFastCeiling, TrainingActivityType.Control);
                    controlAdded+= awarenessNeeded;
                }
                else if (flairNeeded > awarenessNeeded && finalAttributeLeftToTrain.Flair >0)
                {
                    AddGrind(PlayerAttribute.Awareness, finalGrind, finalCounter, awarenessNeeded + TrainingSchedule.ConstantFastCeiling, TrainingActivityType.FiveASide);
                    fiveaSideAdded+= awarenessNeeded;
                }
                else
                {
                    AddGrind(PlayerAttribute.Awareness, finalGrind, finalCounter, awarenessNeeded + TrainingSchedule.ConstantFastCeiling, TrainingActivityType.TrainingMatch);
                    trainingMatchesAdded+= awarenessNeeded;
                }
            }
            var headingNeeded = finalAttributeLeftToTrain.Heading - trainingMatchesAdded - fiveaSideAdded / 4.0+skillLost/8;
            if (headingNeeded < 0) headingNeeded = 0;
            if (headingNeeded > 0)
            {
                if (headingNeeded > 4)
                    headingNeeded = 4;
                var repeat = (int)(headingNeeded + TrainingSchedule.ConstantFastCeiling);
                if (repeat == 0) repeat = 1;

                AddGrind(PlayerAttribute.Heading, finalGrind, finalCounter, headingNeeded + TrainingSchedule.ConstantFastCeiling, TrainingActivityType.Heading);
                AddCounter(PlayerAttribute.Heading, finalCounter, TrainingActivityType.Sprinting, trainingEffectModifier);
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
                    var repeat = (int)(markingNeeded + TrainingSchedule.ConstantFastCeiling);
                    if (repeat == 0) repeat = 1;
                    AddGrind(PlayerAttribute.TackleDetermination, finalGrind, finalCounter, repeat,TrainingActivityType.Marking);
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
                    var repeat = (int)(tacklingSkillNeeded + TrainingSchedule.ConstantFastCeiling);
                    if (repeat > 0)
                    {
                        AddGrind(PlayerAttribute.TackleSkill, finalGrind, finalCounter, repeat, TrainingActivityType.Tackling);
                        if (trainingMatchesAdded == 0)
                        {
                            AddCounter(PlayerAttribute.TackleSkill, finalCounter, TrainingActivityType.FiveASide, trainingEffectModifier);
                            fiveaSideAdded++;
                        }
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
                    AddGrind(PlayerAttribute.TackleSkill, finalGrind, finalCounter, tacklingSkillNeeded + TrainingSchedule.ConstantFastCeiling, TrainingActivityType.Tackling);
                    if (trainingMatchesAdded == 0)
                    {
                        AddCounter(PlayerAttribute.TackleSkill, finalCounter, TrainingActivityType.FiveASide, trainingEffectModifier);
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
                    AddGrind(PlayerAttribute.TackleDetermination, finalGrind, finalCounter, markingNeeded + TrainingSchedule.ConstantFastCeiling, TrainingActivityType.Marking);
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
            {
                Debug.Assert(false);
                finalGrind.Add(new TrainingScheduleSteps() { TrainingScheduleType = TrainingActivityType.TrainingMatch, ForPlayerAttribute = PlayerAttribute.Count });
            }
            else if (finalGrind.Count == 1)
            {
                switch (finalGrind[0].TrainingScheduleType)
                {                  
                    case TrainingActivityType.Heading:
                        for (int i = 0; i < 4; i++)
                        {
                            finalGrind.Add(new TrainingScheduleSteps() { TrainingScheduleType = TrainingActivityType.Heading, ForPlayerAttribute = finalGrind[0].ForPlayerAttribute });
                        }
                        finalCounter.Add(new TrainingScheduleSteps() { TrainingScheduleType = TrainingActivityType.Sprinting, ForPlayerAttribute = finalGrind[0].ForPlayerAttribute });
                        finalCounter.Add(new TrainingScheduleSteps() { TrainingScheduleType = TrainingActivityType.TrainingMatch, ForPlayerAttribute = finalGrind[0].ForPlayerAttribute });
                        break;
                    case TrainingActivityType.Sprinting:
                        for (int i = 0; i < 3; i++)
                        {
                            finalGrind.Add(new TrainingScheduleSteps() { TrainingScheduleType = TrainingActivityType.Sprinting, ForPlayerAttribute = finalGrind[0].ForPlayerAttribute});
                        }
                        for (int i = 0; i < 3; i++)
                        {
                            finalCounter.Add(new TrainingScheduleSteps() { TrainingScheduleType = TrainingActivityType.TrainingMatch, ForPlayerAttribute = finalGrind[0].ForPlayerAttribute });
                        }

                        break;
                    case TrainingActivityType.ZonalDefence:
                        for (int i = 0; i < 6; i++)
                        {
                            finalGrind.Add(new TrainingScheduleSteps() { TrainingScheduleType = TrainingActivityType.ZonalDefence, ForPlayerAttribute = finalGrind[0].ForPlayerAttribute });
                        }
                        finalCounter.Add(new TrainingScheduleSteps() { TrainingScheduleType = TrainingActivityType.TrainingMatch, ForPlayerAttribute = finalGrind[0].ForPlayerAttribute });
                        break;
                    case TrainingActivityType.Marking:
                        for (int i = 0; i < 4; i++)
                        {
                            finalGrind.Add(new TrainingScheduleSteps() { TrainingScheduleType = TrainingActivityType.Marking, ForPlayerAttribute = finalGrind[0].ForPlayerAttribute });
                        }
                        for (int i = 0; i < 2; i++)
                        {
                            finalCounter.Add(new TrainingScheduleSteps() { TrainingScheduleType = TrainingActivityType.TrainingMatch, ForPlayerAttribute = finalGrind[0].ForPlayerAttribute });
                        }
                        break;
                    case TrainingActivityType.Tackling:
                        for (int i = 0; i < 4; i++)
                        {
                            finalGrind.Add(new TrainingScheduleSteps() { TrainingScheduleType = TrainingActivityType.Tackling, ForPlayerAttribute = finalGrind[0].ForPlayerAttribute });
                        }
                        for (int i = 0; i < 2; i++)
                        {
                            finalCounter.Add(new TrainingScheduleSteps() { TrainingScheduleType = TrainingActivityType.TrainingMatch, ForPlayerAttribute = finalGrind[0].ForPlayerAttribute });
                        }
                        break;
                    case TrainingActivityType.Physiotherapist:
                        break;
                    case TrainingActivityType.Handling:
                        break;
                    case TrainingActivityType.GoalKeeping:
                        break;
                    case TrainingActivityType.Kicking:
                        break;
                    case TrainingActivityType.Throwing:
                        break;
                    case TrainingActivityType.FiveASide:
                        break;
                    case TrainingActivityType.TrainingMatch:
                        break;
                    case TrainingActivityType.Count:
                        break;
                    default:
                        break;
                }
            }
            finalResult.AddRange(finalGrind);
            finalResult.AddRange(finalCounter);

            while (finalResult.Count < 7)
            {
                foreach (var item in finalGrind)
                {
                    finalResult.Add(item);
                }
                foreach (var item in finalCounter)
                {
                    finalResult.Add(item);
                }
            }
            if (finalResult.Count > 7)
            {
                finalResult.RemoveRange(7, finalResult.Count - 7);
            }
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
        static void AddGrind(PlayerAttribute attributeIndex, List<TrainingScheduleSteps> grind, List<TrainingScheduleSteps> counter, double repeat, TrainingActivityType trainingScheduleType)
        {
            if (repeat > 7) repeat = 7;
            if (!counter.Any(c => c.TrainingScheduleType == trainingScheduleType)
                              && !grind.Any(c => c.TrainingScheduleType == trainingScheduleType))
            {
                for (int j = 0; j < repeat; j++)
                {
                    grind.Add(new TrainingScheduleSteps { ForPlayerAttribute = attributeIndex, TrainingScheduleType = trainingScheduleType });
                }
            }
        }
        static void AddCounter(PlayerAttribute attributeIndex, List<TrainingScheduleSteps> counter, TrainingActivityType trainingScheduleType, TrainingEffectModifier trainingEffectModifier)
        {
            
            if (!trainingEffectModifier.RemoveNegativeTraining)
            {
                if (!counter.Any(c => c.TrainingScheduleType == trainingScheduleType))
                    counter.Add(new TrainingScheduleSteps { ForPlayerAttribute = attributeIndex, TrainingScheduleType = trainingScheduleType });
            }
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

                var presets = new List<TrainingActivityType[]>();
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

                var presets = new List<TrainingActivityType[]>();
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