using FSM97Lib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

namespace Fsm97Trainer
{
    //the state is for each player and position
    //should not be shared for each player and position
    public class TrainingScheduleCalculationState
    {
        public TrainingScheduleCalculationState(PlayerModelDouble player, bool revertFromGK,PlayerPosition position, 
            bool autoResetStatus, bool maxEnergy, bool maxPower, 
            bool noAlternativeTraining, bool alwaysTrainConsistency, 
            TrainingEffectModifier trainingEffectModifier, TrainingActivity[] trainingEffects)
        {
            Player = player;
            AttributesLeftToTrain = new PlayerModelDouble(player);
            ProjectedAttributesAfterSprinting= new PlayerModelDouble(player);
            ProjectedAttributesAfterTrainingMatch= new PlayerModelDouble(player);

            RevertFromGK = revertFromGK;
            playerPosition = position;
            TrainingEffectModifier = trainingEffectModifier;
            AutoResetStatus = autoResetStatus;
            MaxEnergy = maxEnergy;
            MaxPower = maxPower;
            NoAlternativeTraining = noAlternativeTraining;
            AlwaysTrainConsistency = alwaysTrainConsistency;
           
            TrainingEffects = trainingEffects;

            for(int i = 0; i <(int) PlayerAttribute.Count; i++)
            {

                bool trainThisAttribute = (position==PlayerPosition.Count)?true: PositionRatings.Ratings[(int)position][i] > 0;
                switch (i)
                {
                    case (int)PlayerAttribute.Leadership:

                        if (TrainingEffectModifier.PassingTrainLeadership)
                            trainThisAttribute = true;
                        break;
                    case (int)PlayerAttribute.Consistency:
                        if (AlwaysTrainConsistency)
                            trainThisAttribute = true;
                        break;
                    case (int)PlayerAttribute.Stamina:
                    case (int)PlayerAttribute.Strength:
                        trainThisAttribute = false;
                        break;
                    case (int)PlayerAttribute.ThrowIn:
                        if (trainingEffectModifier.ThrowingTrainThrowIn)
                            trainThisAttribute = true;
                        break;
                    case (int)PlayerAttribute.Greed:
                        if (trainingEffectModifier.ShootingTrainGreed)
                            trainThisAttribute = true; break;
                }
                if (trainThisAttribute)
                {
                    RequiredAttributes.Add((PlayerAttribute)i);
                }
            }
        }
        public List<TrainingScheduleSteps> GenericTrainingGrind { get; } = new List<TrainingScheduleSteps>(20);
        public List<TrainingScheduleSteps> GenericTrainingCounter { get; } = new List<TrainingScheduleSteps>(20);
        public List<TrainingScheduleSteps> FinalResult { get; } = new List<TrainingScheduleSteps>(20);
        public List<TrainingScheduleSteps> FinalGrind { get; } = new List<TrainingScheduleSteps>(20);
        public List<TrainingScheduleSteps> FinalCounter { get; } = new List<TrainingScheduleSteps>(20);

        public List<PlayerAttribute> RequiredAttributes { get; } = new List<PlayerAttribute>();
        
        public bool AlwaysTrainConsistency { get; private set; }
        public TrainingEffectModifier TrainingEffectModifier { get; }
        public TrainingActivity[] TrainingEffects { get; private set; }
        public bool RevertFromGK { get; private set; }
        public bool AutoResetStatus { get; private set; }
        public bool MaxEnergy { get; private set; }
        public bool MaxPower { get; }
        public bool NoAlternativeTraining { get; }

        double[] roundsToMaxForSchedule = new double[(int)TrainingActivityType.Count];
        double[] fastestRoundsToMax = new double[(int)PlayerAttribute.Count];

        public PlayerModelDouble ProjectedAttributesAfterSprinting { get; private set; }


        public void UpdateProjectedAttributesAfterSprinting()
        {
            var player = Player;
            double[] playerAttributes = new double[(int)PlayerAttribute.Count];
            Array.Copy(player.Attributes, playerAttributes, (int)PlayerAttribute.Count);

            var trainingEffects = TrainingEffects;
            var almostAttributeCap = TrainingSchedule.almostAttributeCap;
            var attributeCap = TrainingSchedule.attributeCap;
            var ConstantFastCeiling= TrainingSchedule.ConstantFastCeiling;

            if (RequiredAttributes.Contains(PlayerAttribute.Determination))
            {
                //determination can be safely maxed out 
                //as nothing decreases with determination
                //and its training costs skills
                if (attributeCap > Player.Determination)
                {
                    int weightLiftingRounds = TrainingEffectModifier.weightLiftingRoundsForEachDetermination[attributeCap - (int)Player.Determination];
                    int speedLostForWeightLifting = -(int)(ConstantFastCeiling + weightLiftingRounds * trainingEffects[(int)TrainingActivityType.WeightTraining].Effects[(int)PlayerAttribute.Speed]);
                    if (speedLostForWeightLifting < 0)
                        speedLostForWeightLifting = 0;
                    var sprintRoundsForDetermination = TrainingEffectModifier.sprintRoundsForEachSpeed[speedLostForWeightLifting];
                    for (int i = 0; i < (int)PlayerAttribute.Count; i++)
                    {
                        playerAttributes[i] += weightLiftingRounds * trainingEffects[(int)TrainingActivityType.WeightTraining].Effects[(int)i];
                        playerAttributes[i] += sprintRoundsForDetermination * trainingEffects[(int)TrainingActivityType.Sprinting].Effects[(int)i];
                    }
                }
            }

            //sprinting has negative effect on skills so we max it first
            //this may overflow on acceleration which is needed for heading
            //but pratically nobody's heading is so low at a position that requires heading to cause issues
            int speedNeeded= attributeCap - 1 - (int)Player.Speed;
            if(speedNeeded < 0)
                speedNeeded = 0;
            var sprintRoundsForSpeed = TrainingEffectModifier.sprintRoundsForEachSpeed[speedNeeded];
            if (sprintRoundsForSpeed > 0)
            {
                for (int i = 0; i < (int)PlayerAttribute.Count; i++)
                {
                    playerAttributes[i] += sprintRoundsForSpeed * trainingEffects[(int)TrainingActivityType.Sprinting].Effects[(int)i];
                }
            }
            if (RequiredAttributes.Contains(PlayerAttribute.Acceleration))
            {
                //for those who need acceleration, we will max it out as much as possible
                //as sprinting costs skills
                //this may overflow on acceleration which is needed for heading
                //so we will max out at almostAttributeCap then switch to other bottleneck attributes
                int accelerationNeeded = almostAttributeCap - 1 - (int)playerAttributes[(int)PlayerAttribute.Acceleration];
                if (accelerationNeeded < 0)
                    accelerationNeeded = 0;
                var sprintRoundsForAcceleration = TrainingEffectModifier.sprintRoundsForEachSpeed[accelerationNeeded];
                if (sprintRoundsForAcceleration > 0)
                {
                    for (int i = 0; i < (int)PlayerAttribute.Count; i++)
                    {
                        playerAttributes[i] += sprintRoundsForAcceleration * trainingEffects[(int)TrainingActivityType.Sprinting].Effects[(int)i];
                    }
                }
            }
            ProjectedAttributesAfterSprinting.CopyAttributes(playerAttributes);
        }
        public PlayerModelDouble ProjectedAttributesAfterTrainingMatch { get; private set; }
        public void UpdateProjectedAttributesAfterTrainingMatch()
        {
            double[] playerAttributes = new double[(int)PlayerAttribute.Count];

            Array.Copy(ProjectedAttributesAfterSprinting.Attributes, playerAttributes, (int)PlayerAttribute.Count);

            var position = playerPosition;

            var trainingEffects = TrainingEffects;
            var almostAttributeCap = TrainingSchedule.almostAttributeCap;

            bool trainShooting = RequiredAttributes.Contains(PlayerAttribute.Shooting);
            bool trainPassing = RequiredAttributes.Contains(PlayerAttribute.Passing);

            if (trainShooting)
            {
                int shootingNeeded = almostAttributeCap - (int)playerAttributes[(int)PlayerAttribute.Shooting];
                if (shootingNeeded < 0)
                    shootingNeeded = 0;
                var trainingMatchRoundsForShooting = TrainingEffectModifier.trainingMatchtRoundsForEachShooting[shootingNeeded];
                for (int i = 0; i < (int)PlayerAttribute.Count; i++)
                {
                    playerAttributes[i] += trainingMatchRoundsForShooting * trainingEffects[(int)TrainingActivityType.TrainingMatch].Effects[(int)i];
                }
            }
            if (trainPassing)
            {
                int passingNeeded = almostAttributeCap - (int)playerAttributes[(int)PlayerAttribute.Passing]    ;
                if (passingNeeded < 0)
                    passingNeeded = 0;
                var trainingMatchRoundsForPassing = TrainingEffectModifier.trainingMatchtRoundsForEachPassing[passingNeeded];
                for (int i = 0; i < (int)PlayerAttribute.Count; i++)
                {
                    playerAttributes[i] += trainingMatchRoundsForPassing * trainingEffects[(int)TrainingActivityType.TrainingMatch].Effects[(int)i];
                }
            }
            ProjectedAttributesAfterTrainingMatch.CopyAttributes(playerAttributes);
        }
        public PlayerModelDouble Player { get; }

        PlayerPosition playerPosition;
        public PlayerPosition PlayerPosition
        {
            get
            {
                if (playerPosition == PlayerPosition.GK && RevertFromGK && Player.BestPosition != (int)playerPosition)
                {
                    //train for the player's real position
                    return (PlayerPosition)Player.BestPosition;

                }
                return playerPosition;
            }
        }
        public PlayerModelDouble AttributesLeftToTrain { get; private set; }

        public void UpdateAttributesLeftToTrain(int cap = TrainingSchedule.attributeCap)
        {
            double[] attributeLeftToTrain = new double[(int)PlayerAttribute.Count];

            for (int playerAttribute = 0; playerAttribute < (int)PlayerAttribute.Count; playerAttribute++)
            {
                bool countThisAttribute = RequiredAttributes.Contains((PlayerAttribute)playerAttribute);
                if (countThisAttribute)
                {
                    if (cap > Player.Attributes[playerAttribute])
                        attributeLeftToTrain[playerAttribute] = cap - Player.Attributes[playerAttribute];
                    else
                        attributeLeftToTrain[playerAttribute] = 0;
                }
                else
                    attributeLeftToTrain[playerAttribute] = 0;
            }

            this.AttributesLeftToTrain.CopyAttributes(attributeLeftToTrain);
        }

        public List<BottleneckAttributes> BottleneckAttributes { get; private set; }

        public void UpdateBottleneckAttributes()
        {
            BottleneckAttributes = GetBottleneckAttributes(this.PlayerPosition,null);
        }

        public List<BottleneckAttributes> GetBottleneckAttributes(PlayerPosition playerPosition, PlayerModelDouble attributeLeftToTrain)
        {
            if(attributeLeftToTrain==null)
                attributeLeftToTrain = this.AttributesLeftToTrain;

            var position = playerPosition;

            bool trainShooting = RequiredAttributes.Contains(PlayerAttribute.Shooting);
            bool trainPassing = RequiredAttributes.Contains(PlayerAttribute.Passing);
            bool trainAcceleration = RequiredAttributes.Contains(PlayerAttribute.Acceleration);
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
                    default:

                        Array.Clear(roundsToMaxForSchedule, 0, (int)TrainingActivityType.Count);
                        for (int trainingScheduleType = 0; trainingScheduleType < (int)TrainingActivityType.Count; trainingScheduleType++)
                        {
                            bool skipThisTraining = true;
                            double rounds = 0;
                            if (attributeLeftToTrain.Attributes[playerAttribute] > 0)
                            {
                                var trainingEffect = TrainingEffects[trainingScheduleType].Effects[playerAttribute];
                                if (trainingEffect > 0)
                                {
                                    skipThisTraining = ShouldSkipTraining(position, (PlayerAttribute)playerAttribute, (TrainingActivityType)trainingScheduleType);
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
                Debug.Assert(Player.PositionRating >= 96);
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
        int[][] almostAttributeCapGapRounds = null;
        bool ShouldSkipTraining(PlayerPosition position, PlayerAttribute playerAttribute, TrainingActivityType trainingScheduleType)
        {
            bool trainShooting = RequiredAttributes.Contains(PlayerAttribute.Shooting);
            bool trainPassing = RequiredAttributes.Contains(PlayerAttribute.Passing);
            bool trainAcceleration = RequiredAttributes.Contains(PlayerAttribute.Acceleration);
            bool trainDetermination = RequiredAttributes.Contains(PlayerAttribute.Determination);

            var almostAttributeCap = TrainingSchedule.almostAttributeCap;
            var attributeCap = TrainingSchedule.attributeCap;
            var ConstantFastCeiling = TrainingSchedule.ConstantFastCeiling;
            var trainingEffects = TrainingEffects;
            var player = Player;
            var attributeLeftToTrain = AttributesLeftToTrain;
            var projectedAttributesAfterTrainingMatch = ProjectedAttributesAfterTrainingMatch;


            if (almostAttributeCapGapRounds == null)
            {

                almostAttributeCapGapRounds = new int[(int)TrainingActivityType.Count][];

                for (int i = 0; i < (int)TrainingActivityType.Count; i++)
                {
                    almostAttributeCapGapRounds[i] = new int[(int)PlayerAttribute.Count];
                    for (int j = 0; j < (int)PlayerAttribute.Count; j++)
                    {
                        almostAttributeCapGapRounds[i][j] = (int)(
                            (attributeCap - almostAttributeCap) / trainingEffects[i].Effects[j]
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
                        && player.Speed < almostAttributeCap)
                    {
                        //don't max out shooting without enough speed
                        return true;
                    }

                    if (trainAcceleration && player.Acceleration < almostAttributeCap)
                    {
                        //don't max out shooting without enough acceleration
                        return true;
                    }
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
                    if (player.Speed < ProjectedAttributesAfterSprinting.Speed) return true;
                    if (trainAcceleration && player.Acceleration < ProjectedAttributesAfterSprinting.Acceleration) return true;
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
                    if (trainingScheduleType == TrainingActivityType.Heading)
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
                        case TrainingActivityType.Control:

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

                        case TrainingActivityType.FiveASide:
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
                        case TrainingActivityType.Control:

                            //skip specialized training if will covered by other training                    
                            if (player.Dribbling < projectedDribblingAfterTrainingMatch)
                            {
                                //use training match at the beginning
                                return true;
                            }
                            break;
                        case TrainingActivityType.FiveASide:
                            if (player.Control < projectedControlAfterTrainingMatch)
                            {
                                //use training match at the beginning
                                return true;
                            }
                            break;
                    }
                    break;
                case PlayerAttribute.Coolness:
                    switch ((TrainingActivityType)trainingScheduleType)
                    {
                        case TrainingActivityType.Control:
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
                        case TrainingActivityType.TrainingMatch: break;
                        default:
                            if (player.Awareness < projectedAwarenessAfterTrainingMatch)
                                return true;//use training match at the beginning
                            break;
                    }
                    switch (trainingScheduleType)
                    {
                        case TrainingActivityType.Control: break;
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
                        case TrainingActivityType.ZonalDefence:

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
                        case TrainingActivityType.GoalKeeping:
                            //it basically increase handlling and agility
                            //except if one of them is maxed then there are better options
                            if (attributeLeftToTrain.Handling == 0) return true;
                            if (attributeLeftToTrain.Agility == 0) return true;
                            break;
                        case TrainingActivityType.Control:
                            break;
                        case TrainingActivityType.TrainingMatch:
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
                        case TrainingActivityType.Marking:
                        case TrainingActivityType.Tackling:
                            return true;//we don't reall want to use this for awareness

                        case TrainingActivityType.FiveASide:
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
                        case TrainingActivityType.Marking:
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
                        case TrainingActivityType.TrainingMatch:

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
                        case TrainingActivityType.Tackling:
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
                        case TrainingActivityType.FiveASide:
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
                        case TrainingActivityType.Tackling:
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
                        case TrainingActivityType.FiveASide:
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
                        case TrainingActivityType.TrainingMatch:

                            if (tackleSkillLeftToTrain < tackleDeterminationLeftToTrain / 2)
                            {
                                //is it covered by marking?
                                return true;
                            }
                            break;
                    }

                    break;
                case PlayerAttribute.Flair:

                    switch ((TrainingActivityType)trainingScheduleType)
                    {
                        case TrainingActivityType.FiveASide:
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

                        case TrainingActivityType.TrainingMatch:
                            if (attributeLeftToTrain.Attributes[(int)PlayerAttribute.Consistency] > 0 && player.Consistency < almostAttributeCap)
                                return true;//still need control training
                            if (consistancyLeftToTrain > 0) return true;//max consistency first
                            break;
                        case TrainingActivityType.Control:
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
    }
}