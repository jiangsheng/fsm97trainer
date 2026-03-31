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
        private const double ConstantFastCeiling = 0.999999;
        public static List<TrainingScheduleSteps> GetTrainingSchedule(PlayerModel player, bool autoResetStatus, bool maxEnergy,
            bool maxPower, bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects, bool debugTraining, PlayerModelDouble projectedAttributesAfterSprinting, PlayerModelDouble projectedAttributesAfterTrainingMatch, PlayerModel attributeLeftToTrain, List<BottleneckAttributes> bottleneckAttributeIndex)
        {
            List<TrainingScheduleSteps> schedule;
            //is this a player a different best position only training as goalkeeper to avoid injury?
            if (player.Fitness < attributeCap && player.Position == (byte)PlayerPosition.GK
                && player.BestPosition != (byte)PlayerPosition.GK)
            {
                if (debugTraining)
                {
                    //Debug.WriteLine($"Player {player} is training as GK to avoid injury. Best position is {(PlayerPosition)player.BestPosition}");
                }
                //train for the player's real position
                schedule = GetTrainingSchedule(player, (PlayerPosition)player.BestPosition, maxPower, noAlternativeTraining, trainingEffectModifier,trainingEffects, debugTraining, projectedAttributesAfterSprinting, projectedAttributesAfterTrainingMatch, attributeLeftToTrain, bottleneckAttributeIndex    );
            }
            else
            {
                //train for the game player's preferred position
                schedule = GetTrainingSchedule(player, (PlayerPosition)player.Position, maxPower, noAlternativeTraining, trainingEffectModifier,trainingEffects, debugTraining, projectedAttributesAfterSprinting, projectedAttributesAfterTrainingMatch, attributeLeftToTrain, bottleneckAttributeIndex);
            }
            if (schedule != null)
            {
                //see physiotherapist if injuries will not be auto reset
                //training injury happens due to low energy
                if (!autoResetStatus && !maxEnergy && !trainingEffectModifier.RemoveNegativeTraining)
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

        public static List<TrainingScheduleSteps> GetTrainingSchedule(PlayerModel player, PlayerPosition position, bool maxPower,
            bool noAlternativeTraining,
            TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects, bool debugTraining, PlayerModelDouble projectedAttributesAfterSprinting, PlayerModelDouble projectedAttributesAfterTrainingMatch, PlayerModel attributeLeftToTrain, List<BottleneckAttributes> bottleneckAttributeIndex)
        {          
            bool trainDetermination = PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Determination] > 0;
            bool trainHeading = PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Heading] > 0;
            bool trainAcceleration = PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Acceleration] > 0;
            bool trainTackleSkill = PositionRatings.Ratings[(int)position][(int)PlayerAttribute.TackleSkill] > 0;
            bool trainTackleDetermination = PositionRatings.Ratings[(int)position][(int)PlayerAttribute.TackleDetermination] > 0;

            if (!noAlternativeTraining)
            {
                //if (debugTraining) Debug.WriteLine($"Player {player} is using generic training.");
                return GenericTraining(player, position, maxPower
                    , trainingEffectModifier, trainingEffects, projectedAttributesAfterSprinting, projectedAttributesAfterTrainingMatch, attributeLeftToTrain, bottleneckAttributeIndex);
            }
            
            if (player.Fitness < almostAttributeCap) return ImproveFitness(player, trainAcceleration);

            if (!ShouldStopDefaultTraining(player, position))
            {
                return TrainingSchedulePreset.GetDefaultTrainingSchedule(position, trainingEffectModifier).Select(x=>new TrainingScheduleSteps {TrainingScheduleType=x,ForPlayerAttribute=PlayerAttribute.Count }).ToList();
            }
            if (AreAllRevelantAttribtesAboveAlmostCap(player, position, trainingEffectModifier))
            {
                return GetFinalTrainingSchedule(player, position, trainAcceleration, trainHeading, trainDetermination, trainTackleSkill, trainTackleDetermination,trainingEffectModifier);
            }
            switch (position)
            {
                case PlayerPosition.GK: return GetGKTrainingSchedule(player, maxPower, trainingEffectModifier,trainingEffects);
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
            }
        }
        #region generic training

        static List<TrainingScheduleSteps> genericTrainingGrind = new List<TrainingScheduleSteps>(20);
        static List<TrainingScheduleSteps> genericTrainingCounter = new List<TrainingScheduleSteps>(20);
        public static List<TrainingScheduleSteps> GenericTraining(PlayerModel player, PlayerPosition position, bool maxPower, TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects, PlayerModelDouble projectedAttributesAfterSprinting, PlayerModelDouble projectedAttributesAfterTrainingMatch, PlayerModel attributeLeftToTrain, List<BottleneckAttributes> topBottleneckAttributes)
        {

            bool trainAcceleration = PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Acceleration] > 0;
            
            bool trainingShooting = PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Shooting] > 0;
            bool trainingPassing = PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Passing] > 0;
            bool trainHeading = PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Heading] > 0;
            bool trainControl= PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Control] > 0;
            bool trainDribbling = PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Dribbling] > 0;
            
            bool trainTackleSkill = PositionRatings.Ratings[(int)position][(int)PlayerAttribute.TackleSkill] > 0;
            bool trainTackleDetermination = PositionRatings.Ratings[(int)position][(int)PlayerAttribute.TackleDetermination] > 0;
            bool trainCoolness= PositionRatings.Ratings[(int) position][(int)PlayerAttribute.Coolness] > 0;
            bool trainFlair = PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Flair] > 0;

            bool trainDetermination = PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Determination] > 0;

            genericTrainingGrind.Clear();
            genericTrainingCounter.Clear();


            if (AreAllRevelantAttribtesAboveAlmostCap(player, position, trainingEffectModifier))
            {
                return GetFinalTrainingSchedule(player, position, trainAcceleration, trainHeading, 
trainDetermination, trainTackleSkill, trainTackleDetermination,trainingEffectModifier);
            }

            var flairLeftToTrain = attributeLeftToTrain.Flair;

           

            for (int i = 0; i < topBottleneckAttributes.Count; i++)
            {
                if (genericTrainingGrind.Count + genericTrainingCounter.Count > 7)
                    break;
                var topBottleneckAttribute = topBottleneckAttributes[i];
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
                            //it is usually not good to maxed this last
                            AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind,genericTrainingCounter,  repeat, TrainingScheduleType.GoalKeeping);
                            AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingScheduleType.Kicking, trainingEffectModifier);
                            AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingScheduleType.Throwing, trainingEffectModifier);
                            AddCounter(topBottleneckAttribute.AttributeIndex, genericTrainingCounter, TrainingScheduleType.TrainingMatch, trainingEffectModifier);
                        }
                        break;
                    case PlayerAttribute.Shooting:
                    case PlayerAttribute.Passing:
                    case PlayerAttribute.Leadership:
                        AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.TrainingMatch);
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
                            AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.TrainingMatch);
                        }
                        else if (attributeLeftToTrain.Coolness * 3 < attributeLeftToTrain.Awareness * 2)
                        {
                            Debug.Assert(!(player.Shooting == attributeCap && player.Passing == attributeCap && player.Heading == attributeCap));
                            //should use training match 
                            AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.TrainingMatch);
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
                        var tackleSkillLeftToTrain = attributeLeftToTrain.TackleSkill;
                        if (player.TackleSkill < projectedAttributesAfterTrainingMatch.TackleSkill)
                        {
                            AddGrind(topBottleneckAttribute.AttributeIndex, genericTrainingGrind, genericTrainingCounter, repeat, TrainingScheduleType.TrainingMatch);
                        }
                        else if (tackleSkillLeftToTrain / (double)4 <= new[]{
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
                                && attributeLeftToTrain.Handling > 0
                                && attributeLeftToTrain.Agility > 0)
                        {
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
                        else if(attributeLeftToTrain.Coolness > 0|| attributeLeftToTrain.Flair > 0)
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
            if (topBottleneckAttributes[0].AttributeIndex == PlayerAttribute.Control)
            {
                if (genericTrainingGrind[0].TrainingScheduleType == TrainingScheduleType.TrainingMatch)
                {
                    Debug.Assert(!(player.Shooting == 00 && player.Passing == attributeCap && player.Heading == attributeCap));
                }
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
        static bool AreAllRevelantAttribtesAboveAlmostCap(PlayerModel player, PlayerPosition position, TrainingEffectModifier trainingEffectModifier)
        {
            for (int i = 0; i < (int)PlayerAttribute.Count; i++)
            {
                if (PositionRatings.Ratings[(int)position][i] > 0)
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
        static List<TrainingScheduleSteps> finalResult = new List<TrainingScheduleSteps>(20);
        static List<TrainingScheduleSteps> finalGrind = new List<TrainingScheduleSteps>(20);
        static List<TrainingScheduleSteps> finalCounter = new List<TrainingScheduleSteps>(20);
        static List<TrainingScheduleSteps> GetFinalTrainingSchedule(PlayerModel player, PlayerPosition playerPosition, bool trainAcceleration, bool trainHeading, bool trainDetermination, bool trainTackleSkill, bool trainTackleDetermination, TrainingEffectModifier trainingEffectModifier)
        {
            finalResult.Clear();
            finalGrind.Clear() ;
            finalCounter.Clear() ;

            var finalAttributeLeftToTrain = GetAttributesToTrain(player, playerPosition, attributeCap);
            int controlAdded = 0;
            int trainingMatchesAdded = 0;
            var skillLost = 0;
            var speedLost = 0;
            var agilityLost = 0;
            var sprintAdded = 0;
            switch (playerPosition)
            {
                case PlayerPosition.GK:
                    
                    var handlingNeeded = finalAttributeLeftToTrain.Handling;
                    if (handlingNeeded > 0)
                    {
                        AddGrind(PlayerAttribute.Handling, finalGrind, finalCounter, handlingNeeded, TrainingScheduleType.Handling);
                    }
                    var kickingNeeded = finalAttributeLeftToTrain.Kicking;
                    if (!trainingEffectModifier.RemoveNegativeTraining)
                    {
                        kickingNeeded += (handlingNeeded + 3) / 4;
                    }
                    if (kickingNeeded > 0)
                    {
                        AddGrind(PlayerAttribute.Kicking, finalGrind, finalCounter, kickingNeeded, TrainingScheduleType.Kicking);
                        AddCounter(PlayerAttribute.Kicking, finalCounter, TrainingScheduleType.Throwing, trainingEffectModifier);
                        agilityLost+= kickingNeeded;
                    }
                    var throwingNeeded = finalAttributeLeftToTrain.ThrowIn;
                    if (!trainingEffectModifier.RemoveNegativeTraining)
                    {
                        throwingNeeded += kickingNeeded / 4;
                    }
                    if (throwingNeeded > 0)
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

            switch ((PlayerPosition)player.Position)
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

            var sprintNeededForSpeed = finalAttributeLeftToTrain.Speed;
            if (sprintNeededForSpeed>0)
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

            var sprintNeededForAcceleration = finalAttributeLeftToTrain.Acceleration;
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
            var agilityNeeded = finalAttributeLeftToTrain.Agility- trainingMatchesAdded/2+ (agilityLost+1)/2;
            if (agilityNeeded < 0) agilityNeeded = 0;
            if (agilityNeeded > 0)
            {
                AddGrind(PlayerAttribute.Agility, finalGrind, finalCounter, agilityNeeded, TrainingScheduleType.TrainingMatch);
                trainingMatchesAdded += agilityNeeded;
            }
            var shootingNeeded = finalAttributeLeftToTrain.Shooting- trainingMatchesAdded;
            if (shootingNeeded < 0) shootingNeeded = 0;
            if (shootingNeeded > 0)
            {
                AddGrind(PlayerAttribute.Shooting, finalGrind, finalCounter, shootingNeeded, TrainingScheduleType.TrainingMatch);
                trainingMatchesAdded += shootingNeeded;
            }
            var passingNeeded = finalAttributeLeftToTrain.Passing - trainingMatchesAdded;
            if (passingNeeded < 0) passingNeeded = 0;
            if (passingNeeded > shootingNeeded)
            {
                AddGrind(PlayerAttribute.Passing, finalGrind, finalCounter, passingNeeded -shootingNeeded, TrainingScheduleType.TrainingMatch);
                trainingMatchesAdded += passingNeeded - shootingNeeded;
            }
            var controlNeeded= finalAttributeLeftToTrain.Control- trainingMatchesAdded / 2;
            if (controlNeeded < 0) controlNeeded = 0;
            if (controlNeeded > 0)
            {
                AddGrind(PlayerAttribute.Control, finalGrind, finalCounter, controlNeeded, TrainingScheduleType.Control);
                controlAdded += controlNeeded;
            }
            var driblingNeeded = finalAttributeLeftToTrain.Dribbling - trainingMatchesAdded / 4;            
            if (driblingNeeded <0) driblingNeeded = 0;            
            if (driblingNeeded > controlNeeded)
            {
                AddGrind(PlayerAttribute.Dribbling, finalGrind, finalCounter, driblingNeeded-controlNeeded, TrainingScheduleType.Control);
                controlAdded += driblingNeeded - controlNeeded;
            }
            var coolnessNeeded = finalAttributeLeftToTrain.Coolness - controlAdded - trainingMatchesAdded / 2;
            var awarenessNeeded = finalAttributeLeftToTrain.Awareness - controlAdded / 4 - trainingMatchesAdded / 2;
            var flairNeeded = finalAttributeLeftToTrain.Flair - controlAdded / 2 - trainingMatchesAdded / 2;
            if (coolnessNeeded < 0) coolnessNeeded = 0;
            if (coolnessNeeded>0 && awarenessNeeded < coolnessNeeded)
            {
                AddGrind(PlayerAttribute.Coolness, finalGrind, finalCounter, coolnessNeeded, TrainingScheduleType.Control);
                controlAdded += coolnessNeeded;
            }
            awarenessNeeded = finalAttributeLeftToTrain.Awareness - controlAdded / 4 - trainingMatchesAdded / 2;
            if (awarenessNeeded < 0) awarenessNeeded = 0;

            flairNeeded = finalAttributeLeftToTrain.Flair - controlAdded / 2 - trainingMatchesAdded / 2;
            if (flairNeeded < 0) flairNeeded = 0;

            var fiveaSideAdded = 0;
            var trainingMatchAddedForFlair = 0;
            if (flairNeeded>0 )
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
            if (awarenessNeeded > 0)
            {
                if (skillLost > 0)
                {
                    AddGrind(PlayerAttribute.Awareness, finalGrind, finalCounter, (skillLost+1)/2, TrainingScheduleType.TrainingMatch);
                    if(awarenessNeeded> (skillLost + 1) / 2)
                        AddGrind(PlayerAttribute.Awareness, finalGrind, finalCounter, awarenessNeeded-skillLost, TrainingScheduleType.TrainingMatch);
                    skillLost = 0;
                }
                else if (controlNeeded > 0)
                {
                    AddGrind(PlayerAttribute.Awareness, finalGrind, finalCounter, awarenessNeeded, TrainingScheduleType.FiveASide);
                    fiveaSideAdded += awarenessNeeded;
                }
                else if (flairNeeded == 0 && coolnessNeeded == 0)
                {
                    AddGrind(PlayerAttribute.Awareness, finalGrind, finalCounter, awarenessNeeded, TrainingScheduleType.ZonalDefence);
                    if (trainingMatchesAdded == 0 && !trainingEffectModifier.RemoveNegativeTraining)
                    {
                        AddCounter(PlayerAttribute.Awareness, finalCounter, TrainingScheduleType.TrainingMatch, trainingEffectModifier);
                        trainingMatchesAdded+= awarenessNeeded;
                    }
                }
                else if (coolnessNeeded * 3 > awarenessNeeded * 2 && finalAttributeLeftToTrain.Coolness>0)
                {
                    AddGrind(PlayerAttribute.Awareness, finalGrind, finalCounter, awarenessNeeded, TrainingScheduleType.Control);
                    controlAdded+= awarenessNeeded;
                }
                else if (flairNeeded > awarenessNeeded && finalAttributeLeftToTrain.Flair> 0)
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
            var headingNeeded = finalAttributeLeftToTrain.Heading - trainingMatchesAdded - fiveaSideAdded / 4+(skillLost+4)/8;
            if (headingNeeded < 0) headingNeeded = 0;
            if (headingNeeded > 0)
            {
                AddGrind(PlayerAttribute.Heading, finalGrind, finalCounter, headingNeeded, TrainingScheduleType.Heading);
                AddCounter(PlayerAttribute.Heading, finalCounter, TrainingScheduleType.Sprinting, trainingEffectModifier);
                skillLost += 1;
            }
            var markingsAdded = 0;
            var markingNeeded = finalAttributeLeftToTrain.TackleDetermination - trainingMatchesAdded/2-fiveaSideAdded/4;
            if (markingNeeded < 0) markingNeeded = 0;
            if (markingNeeded>0)
            {
                AddGrind(PlayerAttribute.TackleDetermination, finalGrind, finalCounter, markingNeeded, TrainingScheduleType.Marking);
                markingsAdded += markingNeeded;
                skillLost += 1;
            }
            var tacklingSkillAdded = 0;
            var tacklingSkillNeeded= finalAttributeLeftToTrain.TackleSkill - trainingMatchesAdded/2-fiveaSideAdded*2/3- markingsAdded/2;
            if (tacklingSkillNeeded < 0) tacklingSkillNeeded = 0;
            if (tacklingSkillNeeded>0)
            {
                tacklingSkillAdded = tacklingSkillNeeded;
                AddGrind(PlayerAttribute.TackleSkill, finalGrind, finalCounter, tacklingSkillNeeded, TrainingScheduleType.Tackling);
                if (trainingMatchesAdded == 0)
                {
                    AddCounter(PlayerAttribute.TackleSkill, finalCounter, TrainingScheduleType.FiveASide,trainingEffectModifier);
                    fiveaSideAdded++;
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

        static int[][] almostAttributeCapGapRounds = null;
        static bool ShouldSkipTraining(PlayerModel player, PlayerPosition position, PlayerModel attributeLeftToTrain, PlayerAttribute playerAttribute, TrainingScheduleType trainingScheduleType, float[][] trainingEffects, PlayerModelDouble projectedAttributesAfterSprinting, PlayerModelDouble projectedAttributesAfterTrainingMatch)
        {
            bool trainShooting = PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Shooting] > 0;
            bool trainPassing = PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Passing] > 0;
            bool trainAcceleration = PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Acceleration] > 0;
            bool trainDetermination = PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Determination] > 0;

            if (almostAttributeCapGapRounds == null)
            {

                almostAttributeCapGapRounds = new int[(int)TrainingScheduleType.Count][];

                for (int i = 0; i < (int)TrainingScheduleType.Count; i++)
                {
                    almostAttributeCapGapRounds[i] = new int[(int)PlayerAttribute.Count];
                    for (int j = 0; j < (int)PlayerAttribute.Count; j++)
                    {
                        almostAttributeCapGapRounds[i][j] = (int)(
                            (attributeCap - almostAttributeCap) / trainingEffects[i][j]
                            + ConstantFastCeiling);
                    }
                }
            }

            //max all listed attributes to almost attributeCap first
            switch (playerAttribute)
            {
                case PlayerAttribute.Speed:
                    //max out Determination before max out speed
                    if (trainDetermination && player.Determination < attributeCap)
                    {
                        return true;
                    }
                    break;
                case PlayerAttribute.Shooting:
                    if (trainShooting && player.Shooting >= almostAttributeCap - 1
                        && player.Speed < almostAttributeCap) return true;

                    if (trainAcceleration && player.Acceleration < almostAttributeCap) return true;
                    break;
                case PlayerAttribute.Passing:
                    if (trainPassing && player.Passing >= almostAttributeCap - 1 && player.Speed < almostAttributeCap) return true;
                    if (trainAcceleration && player.Acceleration < almostAttributeCap) return true;
                    break;
                case PlayerAttribute.Acceleration:
                case PlayerAttribute.Fitness:
                case PlayerAttribute.Determination:
                    break;
                default:
                    if (player.Speed < projectedAttributesAfterSprinting.Speed) return true;
                    if (trainAcceleration && player.Acceleration < projectedAttributesAfterSprinting.Acceleration) return true;
                    if (trainDetermination && player.Determination < attributeCap) return true;
                    if (trainShooting && player.Shooting < almostAttributeCap) return true;
                    if (trainPassing && player.Passing < almostAttributeCap) return true;
                    break;
            }
            var headingLeftToTrain = attributeLeftToTrain.Heading;
            var consistancyLeftToTrain = attributeLeftToTrain.Consistency;
            var coolnessLeftToTrain = attributeLeftToTrain.Coolness;
            var awarenessLeftToTrain = attributeLeftToTrain.Awareness;
            var flairLeftToTrain = attributeLeftToTrain.Flair;
            var dribblingLeftToTrain = attributeLeftToTrain.Dribbling;
            var controlLeftToTrain = attributeLeftToTrain.Control;
            var tackleDeterminationLeftToTrain = attributeLeftToTrain.TackleDetermination;
            var tackleSkillLeftToTrain = attributeLeftToTrain.TackleSkill;

            var projectedControlAfterTrainingMatch = projectedAttributesAfterTrainingMatch.Control;
            var projectedDribblingAfterTrainingMatch = projectedAttributesAfterTrainingMatch.Dribbling;
            var projectedCoolnessAfterTrainingMatch = projectedAttributesAfterTrainingMatch.Coolness;
            var projectedAwarenessAfterTrainingMatch = projectedAttributesAfterTrainingMatch.Awareness;

            switch (playerAttribute)
            {
                case PlayerAttribute.Speed:
                    break;
                case PlayerAttribute.Agility:
                    break;
                case PlayerAttribute.Acceleration:
                    break;
                case PlayerAttribute.Stamina:
                    break;
                case PlayerAttribute.Strength:
                    break;
                case PlayerAttribute.Fitness:
                    break;
                case PlayerAttribute.Shooting:
                    break;
                case PlayerAttribute.Passing:
                    break;
                case PlayerAttribute.Heading:

                    //skip specialized training if will covered by other training
                    if (trainingScheduleType == TrainingScheduleType.Heading)
                    {
                        if (headingLeftToTrain > 0)
                        {
                            if (player.Heading < projectedAttributesAfterTrainingMatch.Heading)
                            {
                                //use training match at the beginning
                                return true;
                            }
                        }
                    }
                    break;
                case PlayerAttribute.Control:
                    //dribbling training will increase control
                    if ((attributeCap - player.Dribbling) * 4 <= (attributeCap - player.Control * 3)) return true;

                    //the options are control, five a side and training match
                    switch (trainingScheduleType)
                    {
                        case TrainingScheduleType.Control:

                            //skip specialized training if will covered by other training                    
                            if (player.Control < projectedControlAfterTrainingMatch)
                            {
                                Debug.Assert(!(player.Shooting == 00 && player.Passing == attributeCap && player.Heading == attributeCap));
                                ///use training match at the beginning
                                return true;
                            }
                            if (consistancyLeftToTrain == 0)
                            {
                                if (attributeLeftToTrain.Coolness * 3 < awarenessLeftToTrain * 2)
                                {
                                    Debug.Assert(!(player.Shooting == 00 && player.Passing == attributeCap && player.Heading == attributeCap));
                                    //should use training match 
                                    return true;
                                }

                                //should use five a side instead
                                if (attributeLeftToTrain.Coolness == 0 && dribblingLeftToTrain < controlLeftToTrain / 4)
                                {
                                    return true;
                                }
                            }
                            break;

                        case TrainingScheduleType.FiveASide:
                            if (consistancyLeftToTrain > 0) return true;//max consistency first
                            if (player.Control < projectedControlAfterTrainingMatch)
                            {
                                //use training match at the beginning
                                return true;
                            }
                            break;
                    }
                    break;


                case PlayerAttribute.Dribbling:
                    switch (trainingScheduleType)
                    {
                        case TrainingScheduleType.Control:

                            //skip specialized training if will covered by other training                    
                            if (player.Dribbling < projectedDribblingAfterTrainingMatch)
                            {
                                //use training match at the beginning
                                return true;
                            }
                            break;
                        case TrainingScheduleType.FiveASide:
                            if (player.Control < projectedControlAfterTrainingMatch)
                            {
                                //use training match at the beginning
                                return true;
                            }
                            break;
                    }
                    break;
                case PlayerAttribute.Coolness:
                    switch ((TrainingScheduleType)trainingScheduleType)
                    {
                        case TrainingScheduleType.Control:
                            if (player.Coolness < projectedCoolnessAfterTrainingMatch)
                                return true;//use training match at the beginning

                            if (attributeLeftToTrain.Coolness * 3 <= awarenessLeftToTrain * 2)
                            {
                                //should use training match 
                                return true;
                            }
                            break;
                    }
                    break;
                case PlayerAttribute.Awareness:
                    switch (trainingScheduleType)
                    {
                        case TrainingScheduleType.TrainingMatch: break;
                        default:
                            if (player.Awareness < projectedAwarenessAfterTrainingMatch)
                                return true;//use training match at the beginning
                            break;
                    }
                    switch (trainingScheduleType)
                    {
                        case TrainingScheduleType.Control: break;
                        default:                            
                            if (attributeLeftToTrain.Control > 0)
                                return true;
                            if (attributeLeftToTrain.Dribbling > 0)
                                return true;
                            break;
                    }
                    if (position == PlayerPosition.GK)
                    {
                        if (player.Passing == attributeCap
                            && player.Control == attributeCap
                            && attributeLeftToTrain.Handling > 0
                            && attributeLeftToTrain.Agility > 0)
                        {
                            //should use goalkeeping instead
                            return true;
                        }
                    }
                    switch (trainingScheduleType)
                    {
                        case TrainingScheduleType.ZonalDefence:

                            if (attributeLeftToTrain.Coolness > 0)
                            {
                                //should use control instead
                                return true;
                            }
                            if (attributeLeftToTrain.Flair > 0)
                            {
                                //should use five a side or traning match instead
                                return true;
                            }
                            break;
                        case TrainingScheduleType.GoalKeeping:
                            //it basically increase handlling and agility
                            //except if one of them is maxed then there are better options
                            if (attributeLeftToTrain.Handling == 0) return true;
                            if (attributeLeftToTrain.Agility == 0) return true;
                            break;
                        case TrainingScheduleType.Control:
                            break;
                        case TrainingScheduleType.TrainingMatch:
                            if (attributeLeftToTrain.Coolness * 3 > attributeLeftToTrain.Awareness * 2)
                            {
                                //should use control instead
                                return true;
                            }
                            if (attributeLeftToTrain.Flair * 3 > attributeLeftToTrain.Awareness * 4)
                            {
                                //should use five a side instead
                                return true;
                            }

                            break;
                        case TrainingScheduleType.Marking:
                        case TrainingScheduleType.Tackling:
                            return true;//we don't reall want to use this for awareness

                        case TrainingScheduleType.FiveASide:
                            if (flairLeftToTrain > awarenessLeftToTrain)
                                return true;
                            break;
                    }
                    break;
                case PlayerAttribute.TackleDetermination:

                    //marking reduces skills by 1 while increase Tackling Determination skill by 8
                    //the alternative is training match, which increase Tackling Determination skill by 4
                    //and increase skills by at least 2
                    //the difference is 7,7,3,3,1 skill points per 4 Tackling skill 
                    //besides 4 added to coolness, 6 added to awareness and 6 added to flair
                    //only use marking when the player has a lot to train in Tackling Determination and not much to train in other skills

                    switch (trainingScheduleType)
                    {
                        case TrainingScheduleType.Marking:
                            //have too benefits and generally should be avoided
                            if (player.TackleDetermination < projectedAttributesAfterTrainingMatch.TackleDetermination)
                            {
                                //use training match at the beginning
                                return true;
                            }
                            if (tackleDeterminationLeftToTrain / (double)4 <= new[]{
                            //is it covered by training match?
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Shooting]/(double)4,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Passing]/(double)8,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Heading]/(double)4,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Dribbling]/(double)4,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Control]/(double)4,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Awareness]/(double)6,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Flair]/(double)6}.Max())
                            {
                                return true;
                            }

                            if (tackleDeterminationLeftToTrain / (double)2 <= new[] {
                            //is it covered by five a side?
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Shooting]/(double)4,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Passing]/(double)8,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Heading]/(double)2,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Control]/(double)8,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Dribbling]/(double)2,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Awareness]/(double)4,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Flair]/(double)8}.Max())
                            {
                                return true;
                            }
                            if (tackleDeterminationLeftToTrain < tackleSkillLeftToTrain / 2)
                            {
                                //is it covered by tackling training?
                                return true;
                            }

                            break;
                        case TrainingScheduleType.TrainingMatch:

                            if (tackleDeterminationLeftToTrain / (double)2 <= new[] {
                            //is it covered by five a side?
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Shooting]/(double)4,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Passing]/(double)8,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Heading]/(double)2,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Control]/(double)8,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Dribbling]/(double)2,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Awareness]/(double)4,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Flair]/(double)8}.Max())
                            {
                                return true;
                            }
                            if (tackleDeterminationLeftToTrain < tackleSkillLeftToTrain / 2)
                            {
                                //is it covered by tackling training?
                                return true;
                            }
                            break;
                        case TrainingScheduleType.Tackling:
                            if (player.TackleDetermination < projectedAttributesAfterTrainingMatch.TackleDetermination)
                            {
                                //use training match at the beginning
                                return true;
                            }
                            if (tackleDeterminationLeftToTrain / (double)2 <= new[] {
                            //is it covered by five a side?
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Shooting]/(double)4,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Passing]/(double)8,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Heading]/(double)2,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Control]/(double)8,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Dribbling]/(double)2,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Awareness]/(double)4,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Flair]/(double)8}.Max())
                            {
                                return true;
                            }
                            if (tackleSkillLeftToTrain < tackleDeterminationLeftToTrain / 2)
                            {
                                //is it covered by marking?
                                return true;
                            }
                            break;
                        case TrainingScheduleType.FiveASide:
                            if (player.TackleDetermination < projectedAttributesAfterTrainingMatch.TackleDetermination)
                            {
                                //use training match at the beginning
                                return true;
                            }
                            break;
                    }
                    break;
                case PlayerAttribute.TackleSkill:
                    switch (trainingScheduleType)
                    {
                        case TrainingScheduleType.Tackling:
                            if (player.TackleSkill < projectedAttributesAfterTrainingMatch.TackleSkill)
                            {
                                //use training match at the beginning
                                return true;
                            }

                            if (tackleSkillLeftToTrain / (double)4 <= new[]{
                            //is it fully covered by reqired training match?
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Shooting]/(double)4,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Passing]/(double)8,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Heading]/(double)4,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Dribbling]/(double)4,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Control]/(double)4,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Awareness]/(double)6,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Flair]/(double)6}.Max())
                            {
                                return true;
                            }
                            if (tackleSkillLeftToTrain / (double)6 <= new[] {
                            //is it fully covered by reqired five a side?
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Shooting]/(double)4,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Passing]/(double)8,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Heading]/(double)2,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Control]/(double)8,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Dribbling]/(double)2,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Awareness]/(double)4,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Flair]/(double)8}.Max())
                            {
                                return true;
                            }
                            //is it fully covered by reqired marking?
                            if (tackleSkillLeftToTrain < tackleDeterminationLeftToTrain / 2)
                                return true;
                            break;
                        case TrainingScheduleType.FiveASide:
                            if (player.TackleDetermination < projectedAttributesAfterTrainingMatch.TackleDetermination)
                            {
                                //use training match at the beginning
                                return true;
                            }
                            if (tackleSkillLeftToTrain / (double)4 <= new[]{
                            //is it fully covered by reqired training match?
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Shooting]/(double)4,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Passing]/(double)8,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Heading]/(double)4,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Dribbling]/(double)4,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Control]/(double)4,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Awareness]/(double)6,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Flair]/(double)6}.Max())
                            {
                                return true;
                            }
                            if (tackleSkillLeftToTrain < tackleDeterminationLeftToTrain / 2)
                            {
                                //is it covered by marking?
                                return true;
                            }
                            break;
                        case TrainingScheduleType.TrainingMatch:

                            if (tackleSkillLeftToTrain < tackleDeterminationLeftToTrain / 2)
                            {
                                //is it covered by marking?
                                return true;
                            }
                            break;
                    }

                    break;
                case PlayerAttribute.Flair:

                    switch ((TrainingScheduleType)trainingScheduleType)
                    {
                        case TrainingScheduleType.FiveASide:
                            if (player.Flair < projectedAttributesAfterTrainingMatch.Flair)
                            {
                                //use training match at the beginning
                                return true;
                            }
                            if (attributeLeftToTrain.Attributes[(int)PlayerAttribute.Consistency] > 0 && player.Consistency < almostAttributeCap)
                                return true;//still need control training
                            if (flairLeftToTrain / (double)8 <= new[] {
                            //is it fully covered by training match
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Shooting]/(double)8,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Passing]/(double)8,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Heading]/(double)4,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Control]/(double)4,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Dribbling]/(double)2,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Coolness]/(double)4,
                                attributeLeftToTrain.Attributes[(int)PlayerAttribute.Awareness]/(double)6}.Max())
                            {
                                return true;
                            }
                            //is it fully covered by control training?
                            if (flairLeftToTrain / (double)2 <= new[] {
                                    attributeLeftToTrain.Attributes[(int)PlayerAttribute.Control]/(double)8,
                                    attributeLeftToTrain.Attributes[(int)PlayerAttribute.Dribbling]/(double)6,
                                    attributeLeftToTrain.Attributes[(int)PlayerAttribute.Coolness]/(double)8,
                                    attributeLeftToTrain.Attributes[(int)PlayerAttribute.Consistency]/(double)6 }.Max())
                            {
                                return true;
                            }

                            break;

                        case TrainingScheduleType.TrainingMatch:
                            if (attributeLeftToTrain.Attributes[(int)PlayerAttribute.Consistency] > 0 && player.Consistency < almostAttributeCap)
                                return true;//still need control training
                            if (consistancyLeftToTrain > 0) return true;//max consistency first
                            break;
                        case TrainingScheduleType.Control:
                            if (player.Flair < projectedAttributesAfterTrainingMatch.Flair)
                            {
                                //use training match at the beginning
                                return true;
                            }
                            break;
                    }
                    break;
            }
            return false;
        }


        internal static PlayerModel GetAttributesToTrain(PlayerModel player, PlayerPosition position, int cap = attributeCap)
        {
            List<int> attributeLeftToTrain = new List<int>((int)PlayerAttribute.Count);
            for (int playerAttribute = 0; playerAttribute < (int)PlayerAttribute.Count; playerAttribute++)
            {
                if (position == PlayerPosition.Count || PositionRatings.Ratings[(int)position][playerAttribute] > 0)
                {
                    if (cap > player.Attributes[playerAttribute])
                        attributeLeftToTrain.Add(cap - player.Attributes[playerAttribute]);
                    else
                        attributeLeftToTrain.Add(0);
                }
                else
                    attributeLeftToTrain.Add(0);
            }

            return new PlayerModel(attributeLeftToTrain);
        }

        static double[] roundsToMaxForSchedule = new double[(int)TrainingScheduleType.Count];
        static double[] fastestRoundsToMax = new double[(int)PlayerAttribute.Count];
        public static List<BottleneckAttributes> GetTopBottleneckAttributes(PlayerModel player, PlayerPosition position, TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects, PlayerModel attributeLeftToTrain, PlayerModelDouble projectedAttributesAfterSprinting, PlayerModelDouble projectedAttributesAfterTrainingMatch)
        {
            bool trainShooting = PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Shooting] > 0;
            bool trainPassing = PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Passing] > 0;
            bool trainAcceleration = PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Acceleration] > 0;
            Array.Clear(fastestRoundsToMax, 0, (int)PlayerAttribute.Count);
            //find the top bottleneck attributes
            //attributes are weighted by how much they are left to train and how fast each training schedule can train them
            for (int playerAttribute = 0; playerAttribute < (int)PlayerAttribute.Count; playerAttribute++)
            {
                switch ((PlayerAttribute)playerAttribute)
                {
                    case PlayerAttribute.Stamina:
                    case PlayerAttribute.Fitness:
                    case PlayerAttribute.Strength:
                        //never consider these as bottlenecks
                        fastestRoundsToMax[playerAttribute] = 0;
                        break;
                    case PlayerAttribute.Heading:
                    default:

                        Array.Clear(roundsToMaxForSchedule, 0, (int)TrainingScheduleType.Count);
                        for (int trainingScheduleType = 0; trainingScheduleType < (int)TrainingScheduleType.Count; trainingScheduleType++)
                        {
                            bool skipThisTraining = true;
                            double rounds = 0;
                            if (attributeLeftToTrain.Attributes[playerAttribute] > 0)
                            {
                                var trainingEffect = trainingEffects[trainingScheduleType][playerAttribute];
                                if (trainingEffect > 0)
                                {
                                    skipThisTraining = ShouldSkipTraining(player, position, attributeLeftToTrain, (PlayerAttribute)playerAttribute, (TrainingScheduleType)trainingScheduleType, trainingEffects, projectedAttributesAfterSprinting, projectedAttributesAfterTrainingMatch);
                                    rounds = attributeLeftToTrain.Attributes[playerAttribute] / trainingEffect;
                                }
                            }
                            roundsToMaxForSchedule[trainingScheduleType] = rounds;
                        }

                        for (int trainingScheduleType = 0; trainingScheduleType < roundsToMaxForSchedule.Length; trainingScheduleType++)
                        {
                            if (roundsToMaxForSchedule[trainingScheduleType] == 0)
                                continue;
                            if (fastestRoundsToMax[playerAttribute] == 0)
                                fastestRoundsToMax[playerAttribute] = roundsToMaxForSchedule[trainingScheduleType];
                            else
                                fastestRoundsToMax[playerAttribute] = Math.Min(fastestRoundsToMax[playerAttribute], roundsToMaxForSchedule[trainingScheduleType]);
                        }
                        break;
                }
            }
            var topList = new List<BottleneckAttributes>();

            for (int i = 0; i < fastestRoundsToMax.Length; i++)
            {
                double rounds = fastestRoundsToMax[i];
                if (rounds <= 0) continue;

                topList.Add(new BottleneckAttributes { Rounds = rounds, AttributeIndex = (PlayerAttribute)i });
            }

            var topBottleneckAttributes = topList.OrderByDescending(x => x.Rounds).Take(7).ToList();
            var repeats = new List<int>();
            if (topBottleneckAttributes.Count == 0)
            {
                //might be a maxed out cd whose leadership cannot be trained
                Debug.Assert(position == PlayerPosition.CD);
                Debug.Assert(player.PositionRating >= 96);
                return null;
            }

            for (int i = 1; i < topBottleneckAttributes.Count; i++)
            {
                var test = topBottleneckAttributes[i];
                int repeatSum = 0;
                for (int j = 0; j < topBottleneckAttributes.Count; j++)
                {
                    var repeat = (int)(topBottleneckAttributes[j].Rounds / test.Rounds);
                    repeats.Add(repeat);
                    repeatSum += repeat;
                    //repeats[j] = 1;
                }
                if (repeatSum > 7)
                    break;
            }
            if (repeats.Count == 0)
                repeats.Add(1);
            for (int i = 0; i < topBottleneckAttributes.Count; i++)
            {
                var test = topBottleneckAttributes[i];
                test.Repeat = repeats[i];
            }
            /*
            //max shooting, passing and speeds first
            if (trainShooting && attributeLeftToTrain.Shooting > 0)
            {
                int forcedRounds = 1;
                var existing= topBottleneckAttributes.FirstOrDefault(t => t.AttributeIndex == PlayerAttribute.Shooting);
                if (existing != null)
                {
                    topBottleneckAttributes.Remove(existing);
                    forcedRounds += 1;
                }
                topBottleneckAttributes.Insert(0, new BottleneckAttributes { AttributeIndex = PlayerAttribute.Shooting, Rounds = 1, Repeat = forcedRounds });
            }
            else if (trainPassing && attributeLeftToTrain.Passing > 0 && !topBottleneckAttributes.Any(t => t.AttributeIndex == PlayerAttribute.Passing))
            {
                int forcedRounds = 1;
                var existing = topBottleneckAttributes.FirstOrDefault(t => t.AttributeIndex == PlayerAttribute.Passing);
                if (existing != null)
                {
                    topBottleneckAttributes.Remove(existing);
                    forcedRounds += 1;
                }
                topBottleneckAttributes.Insert(0, new BottleneckAttributes { AttributeIndex = PlayerAttribute.Passing, Rounds = 1, Repeat = forcedRounds });
            }
            
            if (trainAcceleration&& player.Acceleration < almostAttributeCap)
            {
                int forcedRounds = 1;
                var existing = topBottleneckAttributes.FirstOrDefault(t => t.AttributeIndex == PlayerAttribute.Acceleration);
                if (existing != null)
                {
                    topBottleneckAttributes.Remove(existing);
                    forcedRounds += 1;
                }
                topBottleneckAttributes.Insert(0, new BottleneckAttributes { AttributeIndex = PlayerAttribute.Acceleration, Rounds = 1, Repeat = forcedRounds });
            }
            else if (player.Speed < almostAttributeCap)
            {
                int forcedRounds = 1;
                var existing = topBottleneckAttributes.FirstOrDefault(t => t.AttributeIndex == PlayerAttribute.Speed);
                if (existing != null)
                {
                    topBottleneckAttributes.Remove(existing);
                    forcedRounds += 1;
                }
                topBottleneckAttributes.Insert(0, new BottleneckAttributes { AttributeIndex = PlayerAttribute.Speed, Rounds = 1, Repeat = forcedRounds });
            }*/
            return topBottleneckAttributes.Take(7).ToList();
        }

        #endregion

        private static List<TrainingScheduleSteps> GetGKTrainingSchedule(PlayerModel player, bool maxPower
            , TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects)
        {
            foreach (var stage in stages)
            {
                if (player.Agility < stage || player.Handling < stage || player.Kicking < stage || player.Throwing < stage || player.Coolness < stage || player.Awareness < stage ||

                    player.Consistency < stage || player.Control < stage || player.Passing < stage || player.Speed < stage)
                {
                    return GetGKTrainingScheduleStage(player, maxPower
                    , trainingEffectModifier, trainingEffects,stage);
                }
            }
            Debug.Assert((int)PositionRatings.GetPositionRatingDouble((int)PlayerPosition.GK, player) == attributeCap);
            return null;
        }

        private static List<TrainingScheduleSteps> GetGKTrainingScheduleStage(PlayerModel player,
            bool maxPower, TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects, int stageMinimum)
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

        private static List<TrainingScheduleSteps> GetLRBTrainingSchedule(PlayerModel player, bool maxPower
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

        private static List<TrainingScheduleSteps> GetLRBTrainingScheduleStage(PlayerModel player, bool maxPower,
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

        private static List<TrainingScheduleSteps> GetCDTrainingSchedule(PlayerModel player, bool maxPower
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

        private static List<TrainingScheduleSteps> GetCDTrainingScheduleStage(PlayerModel player, bool maxPower, TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects, int stageMinimum)
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
        private static List<TrainingScheduleSteps> GetLRWBTrainingSchedule(PlayerModel player, bool maxPower
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

        private static List<TrainingScheduleSteps> GetLRWBTrainingScheduleStage(PlayerModel player, bool maxPower, TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects, int stageMinimum)
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

        private static List<TrainingScheduleSteps> GetSWTrainingSchedule(PlayerModel player, bool maxPower, TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects)
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

        private static List<TrainingScheduleSteps> GetSWTrainingScheduleStage(PlayerModel player, bool maxPower,
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

        private static List<TrainingScheduleSteps> GetDMTrainingSchedule(PlayerModel player, bool maxPower, TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects)
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

        private static List<TrainingScheduleSteps> GetDMTrainingScheduleStage(PlayerModel player, bool maxPower,
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

        private static List<TrainingScheduleSteps> GetLRAMTrainingSchedule(PlayerModel player,
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

        private static List<TrainingScheduleSteps> GetLRAMTrainingScheduleStage(PlayerModel player, PlayerPosition position,
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
        private static List<TrainingScheduleSteps> GetLRWTrainingSchedule(PlayerModel player, bool maxPower, TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects)
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

        private static List<TrainingScheduleSteps> GetLRWTrainingScheduleStage(PlayerModel player, bool maxPower,
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

        private static List<TrainingScheduleSteps> GetFRTrainingSchedule(PlayerModel player, bool maxPower, TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects)
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

        private static List<TrainingScheduleSteps> GetFRTrainingScheduleStage(PlayerModel player, bool maxPower,
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

        private static List<TrainingScheduleSteps> GetFORSSTrainingSchedule(PlayerModel player, PlayerPosition position, bool maxPower, TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects)
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

        private static List<TrainingScheduleSteps> GetFORSSTrainingScheduleStage(PlayerModel player, PlayerPosition position, bool maxPower, TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects,int stageMinimum)
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
        static bool ShouldStopDefaultTraining(PlayerModel player, PlayerPosition playerPosition)
        {
            switch (playerPosition)
            {
                case PlayerPosition.GK:
                    return player.Handling == attributeCap ||
                        player.Kicking == attributeCap ||
                        player.Throwing == attributeCap;
                case PlayerPosition.LB:
                case PlayerPosition.RB:
                    return player.Speed == attributeCap
                        || player.Determination == attributeCap
                        || player.Passing == attributeCap
                        || player.Heading == attributeCap
                        || player.TackleDetermination == attributeCap
                        || player.TackleSkill == attributeCap
                        || player.Consistency == attributeCap;
                case PlayerPosition.CD:
                    return player.Speed == attributeCap
                        || player.Passing == attributeCap
                        || player.Heading == attributeCap
                        || player.TackleDetermination == attributeCap
                        || player.TackleSkill == attributeCap
                        || player.Consistency == attributeCap;

                case PlayerPosition.LWB:
                case PlayerPosition.RWB:
                    return player.Speed == attributeCap
                        || player.Acceleration == attributeCap
                        || player.Passing == attributeCap
                        || player.Dribbling == attributeCap
                        || player.TackleDetermination == attributeCap
                        || player.TackleSkill == attributeCap;

                case PlayerPosition.SW:
                    return player.Speed == attributeCap
                        || player.Acceleration == attributeCap
                        || player.Passing == attributeCap
                        || player.Heading == attributeCap
                        || player.Dribbling == attributeCap
                        || player.TackleDetermination == attributeCap
                        || player.TackleSkill == attributeCap;

                case PlayerPosition.DM:
                    return player.Speed == attributeCap
                        || player.Passing == attributeCap
                        || player.Heading == attributeCap
                        || player.TackleDetermination == attributeCap
                        || player.TackleSkill == attributeCap;
                case PlayerPosition.LM:
                case PlayerPosition.RM:
                case PlayerPosition.AM:
                case PlayerPosition.LW:
                case PlayerPosition.RW:
                    return player.Speed == attributeCap
                        || player.Acceleration == attributeCap
                       || player.Passing == attributeCap
                       || player.Control == attributeCap
                       || player.Dribbling == attributeCap
                       || player.TackleSkill == attributeCap;
                case PlayerPosition.FR:
                case PlayerPosition.FOR:
                case PlayerPosition.SS:
                    return player.Speed == attributeCap
                        || player.Acceleration == attributeCap
                       || player.Passing == attributeCap
                       || player.Heading == attributeCap
                       || player.Control == attributeCap
                       || player.Dribbling == attributeCap;

            }
            return true;
        }
        static void AddGrind(PlayerAttribute attributeIndex, List<TrainingScheduleSteps> grind, List<TrainingScheduleSteps> counter, int repeat, TrainingScheduleType trainingScheduleType)
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
            if (!trainingEffectModifier.RemoveNegativeTraining)
            {
                if (!counter.Any(c => c.TrainingScheduleType == trainingScheduleType))
                    counter.Add(new TrainingScheduleSteps { ForPlayerAttribute = attributeIndex, TrainingScheduleType = trainingScheduleType });
            }
        }
        static List<TrainingScheduleSteps> ImproveAwarenessAndFlairTo(PlayerModel player, int stageMinimum, bool maxPower,
           bool needShooting, bool needHeading, bool needmarking, TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects)
        {
            var projectedAttributesAfterSprinting = ProjectedAttributesAfterSprinting(player, (PlayerPosition)player.Position, trainingEffectModifier,trainingEffects);
            var projectedAttributesAfterTrainingMatch = ProjectedAttributesAfterTrainingMatch(player, (PlayerPosition)player.Position, trainingEffects,projectedAttributesAfterSprinting);
            
            var preset= TrainingSchedulePreset.TrainingMatchAllWeek;

            if (stageMinimum < Math.Max(projectedAttributesAfterTrainingMatch.Awareness, projectedAttributesAfterTrainingMatch.Flair))
            {
                preset = TrainingSchedulePreset.TrainingMatchAllWeek;
            }
            else
            {
                var flairLeftToTrain = stageMinimum - projectedAttributesAfterTrainingMatch.Flair;
                var awarenessLeftToTrain = stageMinimum - projectedAttributesAfterTrainingMatch.Awareness;
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
                    if (player.Awareness < projectedAttributesAfterTrainingMatch.Awareness || player.Flair < projectedAttributesAfterTrainingMatch.Flair)
                    {
                        preset = TrainingSchedulePreset.TrainingMatchAllWeek;
                    }
                    else
                        preset = TrainingSchedulePreset.ControlAllWeek;
                }
            }
            return preset.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.Awareness, TrainingScheduleType = x }).ToList();
        }
        static List<TrainingScheduleSteps> ImproveCoolnessAndAwarenessTo(PlayerModel player, int stageMinimum, bool maxPower, float[][] trainingEffects)
        {
            if (player.Coolness < stageMinimum || player.Awareness < stageMinimum)
            {
                int coolnessLeftToTrain = attributeCap - player.Coolness;
                int awarenessLeftToTrain = attributeCap - player.Awareness;

                var presets = new List<TrainingScheduleType[]>();
                presets.Add(TrainingSchedulePreset.ControlAllWeek);
                presets.Add(TrainingSchedulePreset.TrainingMatchAllWeek);
                presets.Add(TrainingSchedulePreset.FiveASideAllWeek);
                var scores = new List<double>();
                var attributeIndices = new List<int>();
                attributeIndices.Add((int)PlayerAttribute.Coolness);
                attributeIndices.Add((int)PlayerAttribute.Awareness);
                var attributesLeftToTrain = new List<int>();
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
        static List<TrainingScheduleSteps> ImproveCoolnessAwarenessAndFlairTo(PlayerModel player, int stageMinimum, bool maxPower, float[][] trainingEffects)
        {

            if (player.Coolness < stageMinimum || player.Awareness < stageMinimum || player.Flair < stageMinimum)
            {
                int coolnessLeftToTrain = attributeCap - player.Coolness;
                int flairLeftToTrain = attributeCap - player.Flair;
                int awarenessLeftToTrain = attributeCap - player.Awareness;

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
                var attributesLeftToTrain = new List<int>();
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

        private static List<TrainingScheduleSteps> ImproveFitness(PlayerModel player, bool needAcceleration)
        {
            var preset = TrainingSchedulePreset.TrainingMatchAllWeek;
            if (player.Speed < almostAttributeCap)
            {
                preset = TrainingSchedulePreset.SprintingAllWeek;
            }
            if (needAcceleration && player.Acceleration < almostAttributeCap)
            {
                preset=TrainingSchedulePreset.SprintingAllWeek;
            }
            return preset.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.Fitness, TrainingScheduleType = x }).ToList(); 
        }

        private static List<TrainingScheduleSteps> ImproveSpeedTo(PlayerModel player,
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
        private static List<TrainingScheduleSteps> ImproveAgilityTo(PlayerModel player, int stageMinimum)
        {
            if (player.Agility < stageMinimum)
            {
                return TrainingSchedulePreset.TrainingMatchAllWeek.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.Agility, TrainingScheduleType = x }).ToList(); ;
            }
            return null;
        }
        //unused, training match is much more useful for GKs
        private static List<TrainingScheduleSteps> ImproveGKAgilityTo(PlayerModel player, int stageMinimum)
        {
            if (player.Agility < stageMinimum)
            {
                return TrainingSchedulePreset.GKAgility.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.Agility, TrainingScheduleType = x }).ToList(); ; 

            }
            return null;
        }
        private static List<TrainingScheduleSteps> ImproveAccelerationTo(PlayerModel player, int stageMinimum, bool trainHeading, TrainingEffectModifier trainingEffectModifier)
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
        private static List<TrainingScheduleSteps> ImproveShootingTo(PlayerModel player, int stageMinimum)
        {
            if (player.Shooting < stageMinimum) return TrainingSchedulePreset.TrainingMatchAllWeek.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.Shooting, TrainingScheduleType = x }).ToList(); ;
            return null;
        }
        private static List<TrainingScheduleSteps> ImprovePassingTo(PlayerModel player, int stageMinimum)
        {
            if (player.Passing < stageMinimum) return TrainingSchedulePreset.TrainingMatchAllWeek.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.Passing, TrainingScheduleType = x }).ToList(); ;
            return null;
        }

        private static List<TrainingScheduleSteps> ImproveHeadingTo(PlayerModel player, int stageMinimum, TrainingEffectModifier trainingEffectModifier)
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

        private static List<TrainingScheduleSteps> ImproveControlTo(PlayerModel player, int stageMinimum)
        {
            if (player.Control < stageMinimum) return TrainingSchedulePreset.ControlAllWeek.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.Control, TrainingScheduleType = x }).ToList();
            return null;
        }
        private static List<TrainingScheduleSteps> ImproveDribbleTo(PlayerModel player, int stageMinimum)
        {
            if (player.Dribbling < stageMinimum) return TrainingSchedulePreset.ControlAllWeek.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.Dribbling, TrainingScheduleType = x }).ToList();
            return null;
        }
        static List<TrainingScheduleSteps> ImproveTackleDeterminationAndSkillTo(PlayerModel player, int stageMinimum, TrainingEffectModifier trainingEffectModifier)
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
        private static List<TrainingScheduleSteps> ImproveTackleSkillTo(PlayerModel player, int stageMinimum,
            TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects)
        {
            var projectedAttributesAfterSprinting = ProjectedAttributesAfterSprinting(player, (PlayerPosition)player.Position, trainingEffectModifier,trainingEffects);
            var projectedAttributesAfterTrainingMatch = ProjectedAttributesAfterTrainingMatch(player, (PlayerPosition)player.Position,trainingEffects, projectedAttributesAfterSprinting);

            var projectedTackleSkillWithTrainingMatch = projectedAttributesAfterTrainingMatch.TackleSkill;  

            if (player.TackleSkill < stageMinimum)
            {
                var preset = TrainingSchedulePreset.TrainingMatchAllWeek;
                if (trainingEffectModifier.RemoveNegativeTraining)
                    preset =TrainingSchedulePreset.TacklingSkillAllWeek;
                else if (projectedTackleSkillWithTrainingMatch < attributeCap)
                    preset =TrainingSchedulePreset.TacklingSkillAllWeek;
                return preset.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.TackleSkill, TrainingScheduleType = x }).ToList();

            }
            return null;
        }
        private static List<TrainingScheduleSteps> ImproveAwarenessTo(PlayerModel player, int stageMinimum, bool maxPower, TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects)
        {
            var projectedAttributesAfterSprinting = ProjectedAttributesAfterSprinting(player, (PlayerPosition)player.Position, trainingEffectModifier,trainingEffects);
            var projectedAttributesAfterTrainingMatch = ProjectedAttributesAfterTrainingMatch(player, (PlayerPosition)player.Position,trainingEffects, projectedAttributesAfterSprinting);
            bool trainConsistency = PositionRatings.Ratings[player.Position][(int)PlayerAttribute.Consistency] > 0;
            bool trainDribbling = PositionRatings.Ratings[player.Position][(int)PlayerAttribute.Dribbling] > 0;
            if (player.Awareness < stageMinimum)
            {
                var preset = TrainingSchedulePreset.TrainingMatchAllWeek;
                if (stageMinimum < projectedAttributesAfterTrainingMatch.Awareness)
                {
                    preset = TrainingSchedulePreset.TrainingMatchAllWeek;
                }
                else if (player.Passing < attributeCap || player.Heading < attributeCap || player.TackleDetermination < attributeCap || player.TackleSkill < attributeCap)
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
        private static List<TrainingScheduleSteps> ImproveKickingTo(PlayerModel player, int stageMinimum, TrainingEffectModifier trainingEffectModifier)
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
        private static List<TrainingScheduleSteps> ImproveThrowingTo(PlayerModel player, int stageMinimum)
        {
            if (player.Throwing < stageMinimum)
            {
                return TrainingSchedulePreset.ThrowingAllWeek.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.Throwing, TrainingScheduleType = x }).ToList();
            }
            return null;
        }
        private static List<TrainingScheduleSteps> ImproveHandlingTo(PlayerModel player, int stageMinimum, TrainingEffectModifier trainingEffectModifier)
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
        private static List<TrainingScheduleSteps> ImproveLeadershipTo(PlayerModel player, int stageMinimum, TrainingEffectModifier trainingEffectModifier)
        {
            if (player.Leadership < stageMinimum && trainingEffectModifier.PassingTrainLeadership)
            {
                return TrainingSchedulePreset.FiveASideAllWeek.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.Leadership, TrainingScheduleType = x }).ToList(); 
            }
            return null;
        }

        private static List<TrainingScheduleSteps> ImproveConsistencyTo(PlayerModel player, int stageMinimum)
        {
            if (player.Consistency < stageMinimum) return TrainingSchedulePreset.ControlAllWeek.Select(x => new TrainingScheduleSteps { ForPlayerAttribute = PlayerAttribute.Consistency, TrainingScheduleType = x }).ToList(); ;
            return null;
        }

        private static List<TrainingScheduleSteps> ImproveDeterminationTo(PlayerModel player, int stageMinimum,
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
        static Dictionary<int, int> sprintRoundsForEachSpeed=new Dictionary<int, int>();
        static Dictionary<int, double> accelerationLostForEachHeading = new Dictionary<int, double>();
        static Dictionary<int, int> weightLiftingRoundsForEachDetermination = new Dictionary<int, int>();
        static Dictionary<int, double> speedLostForEachWeightLiftingRound=new Dictionary<int, double>();
        static Dictionary<int, int> sprintRoundsForEachAcceleration= new Dictionary<int, int>();

        public static PlayerModelDouble ProjectedAttributesAfterSprinting(PlayerModel player, PlayerPosition position, TrainingEffectModifier trainingEffectModifier, float[][] trainingEffects)
        {
            bool trainAcceleration = PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Acceleration] > 0;
            bool trainDetermination = PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Determination] > 0;

            int sprintRoundsForDetermination =0;
            PlayerModelDouble result=new PlayerModelDouble(player);
            if (trainDetermination)
            {               
                if (attributeCap > player.Determination)
                {
                    int weightLiftingRounds = 0;
                    if (weightLiftingRoundsForEachDetermination.ContainsKey(player.Determination))
                    {
                        weightLiftingRounds = weightLiftingRoundsForEachDetermination[player.Determination];
                    }
                    else
                    {
                        weightLiftingRounds = (int)(
                        (attributeCap - player.Determination) / trainingEffects[(int)TrainingScheduleType.WeightTraining][(int)PlayerAttribute.Determination]
                        +ConstantFastCeiling);
                        weightLiftingRoundsForEachDetermination[player.Determination] = weightLiftingRounds;
                    }
                
                    for (int i = 0; i < (int)PlayerAttribute.Count; i++)
                    {
                        result.Attributes[i] = (int)(
                            weightLiftingRounds * trainingEffects[(int)TrainingScheduleType.WeightTraining][i]);
                    }
                    double speedLostForWeightLifting = 0;
                    if (speedLostForEachWeightLiftingRound.ContainsKey(weightLiftingRounds))
                    {
                        speedLostForWeightLifting = speedLostForEachWeightLiftingRound[weightLiftingRounds];
                    }
                    else
                    {
                        speedLostForWeightLifting = -weightLiftingRounds * trainingEffects[(int)TrainingScheduleType.WeightTraining][(int)PlayerAttribute.Speed];
                        speedLostForEachWeightLiftingRound[weightLiftingRounds] = speedLostForWeightLifting;

                    }
                    sprintRoundsForDetermination = (int)(speedLostForWeightLifting /
                        trainingEffects[(int)TrainingScheduleType.Sprinting][(int)PlayerAttribute.Acceleration]+ConstantFastCeiling);
                }
            }
            int sprintRoundsForSpeed=0;
            int currentSpeed = (int)result.Speed;
            if (sprintRoundsForEachSpeed.ContainsKey(currentSpeed))
            {
                sprintRoundsForSpeed = sprintRoundsForEachSpeed[currentSpeed];
            }
            else
            {
                if (almostAttributeCap > result.Speed)
                {
                    sprintRoundsForSpeed = (int)((almostAttributeCap - result.Speed) / trainingEffects[(int)TrainingScheduleType.Sprinting][(int)PlayerAttribute.Speed]+ ConstantFastCeiling);
                    sprintRoundsForEachSpeed[currentSpeed] = sprintRoundsForSpeed;
                }
            }
            int sprintRoundsForAcceleration=0;
            if (trainAcceleration)
            {
                var resultAcceleration = (int)result.Acceleration;
                if (almostAttributeCap > result.Acceleration)
                {
                    if (sprintRoundsForEachAcceleration.ContainsKey(resultAcceleration))
                    {
                        sprintRoundsForAcceleration = sprintRoundsForEachAcceleration[resultAcceleration];
                    }
                    else
                    {
                        sprintRoundsForAcceleration = (int)((almostAttributeCap - result.Acceleration) / trainingEffects[(int)TrainingScheduleType.Sprinting][(int)PlayerAttribute.Acceleration] + ConstantFastCeiling);
                        sprintRoundsForEachAcceleration[resultAcceleration] = sprintRoundsForAcceleration;
                    }
                }
            }
            int totalRounds = sprintRoundsForDetermination + Math.Max(sprintRoundsForSpeed, sprintRoundsForAcceleration);

            for (int i = 0; i < (int)PlayerAttribute.Count; i++)
            {
                result.Attributes[i] += totalRounds * trainingEffects[(int)TrainingScheduleType.Sprinting][(int)i];
            }
            return result;
        }
        static Dictionary<int, int> trainingMatchtRoundsForEachShootingOrPassing= new Dictionary<int, int>();
        public static PlayerModelDouble ProjectedAttributesAfterTrainingMatch(PlayerModel player, PlayerPosition position, float[][] trainingEffects, PlayerModelDouble projectedAttributesAfterSprinting)
        {
            var result = new PlayerModelDouble(projectedAttributesAfterSprinting);

            bool trainShooting = PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Shooting] > 0;
            bool trainPassing = PositionRatings.Ratings[(int)position][(int)PlayerAttribute.Passing] > 0;
            int shootingRounds=0;
            if (trainShooting)
            {
                int resultShooting = (int)result.Shooting;
                if (almostAttributeCap > resultShooting)
                {
                    if (trainingMatchtRoundsForEachShootingOrPassing.ContainsKey(resultShooting))
                        shootingRounds = trainingMatchtRoundsForEachShootingOrPassing[resultShooting];
                    else
                    {
                        shootingRounds = (int)((almostAttributeCap - resultShooting) / trainingEffects[(int)TrainingScheduleType.TrainingMatch][(int)PlayerAttribute.Shooting]+ConstantFastCeiling);
                        trainingMatchtRoundsForEachShootingOrPassing[resultShooting] = shootingRounds;
                    }
                }
            }
            int passingRounds=0;
            if (trainPassing)
            {
                int resultPassing = (int)result.Passing;
                if (almostAttributeCap > resultPassing)
                {
                    if (trainingMatchtRoundsForEachShootingOrPassing.ContainsKey(resultPassing))
                        passingRounds = trainingMatchtRoundsForEachShootingOrPassing[resultPassing];
                    else
                    {
                        passingRounds = (int)((almostAttributeCap - resultPassing) / trainingEffects[(int)TrainingScheduleType.TrainingMatch][(int)PlayerAttribute.Passing]+ ConstantFastCeiling);
                        trainingMatchtRoundsForEachShootingOrPassing[resultPassing] = passingRounds;
                    }
                }
            }
            var totalRounds = Math.Max(shootingRounds, passingRounds);
            for (int i = 0; i < (int)PlayerAttribute.Count; i++)
            {   
                result.Attributes[i] += totalRounds * trainingEffects[(int)TrainingScheduleType.TrainingMatch][(int)i];
            }
            return result;
        }
    } 
}