using FSM97Lib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Fsm97Trainer
{
    internal class EvaluateYoungPlayersResult
    {

        public EvaluateYoungPlayersResult(PlayerPosition position, List<PlayerModel> youngPlayers, bool autoResetStatus, bool maxEnergy,  bool maxPower, bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier, bool debugTraining, float[][] trainingEffects)
        {
            Position = position;
            YoungPlayers = youngPlayers;
            AutoResetStatus = autoResetStatus;
            MaxEnergy = maxEnergy;
            MaxPower = maxPower;
            NoAlternativeTraining = noAlternativeTraining;
            TrainingEffectModifier = trainingEffectModifier;
            GetTrainingScheduleEffects= TrainingScheduleEffect.GetTrainingScheduleEffect(trainingEffectModifier);
            DebugTraining = debugTraining; 
            AttributeRelevanceForPosition = PositionRatings.Ratings[(int)Position];
            TrainingEffects = trainingEffects;
        }
        byte[] GetTrainingScheduleEffects { get; set; }

        public List<YoungPlayerEvaluation> Grades { get; set; }
        public PlayerPosition Position { get; }
        public List<PlayerModel> YoungPlayers { get; }
        public bool AutoResetStatus { get; }
        public bool MaxEnergy { get; }
        public bool MaxPower { get; }
        public bool NoAlternativeTraining { get; }
        public TrainingEffectModifier TrainingEffectModifier { get; }
        public bool DebugTraining { get; set; }
        byte[] AttributeRelevanceForPosition { get; set; }
        float[][] TrainingEffects { get; set; }

        public event EventHandler OnEvalPlayerPositionComplete;
        public void Evaluate(int minRating)
        {
            Grades = new List<YoungPlayerEvaluation>(YoungPlayers.Count);
            foreach (var youngPlayer in YoungPlayers)
            {
                var positionRating = PositionRatings.GetPositionRatingDouble((int)this.Position, youngPlayer);
                if (positionRating > minRating)
                {
                    Grades.Add(EvaluatePlayer(youngPlayer, minRating));
                }
                else {
                    OnEvalPlayerPositionComplete?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        YoungPlayerEvaluation EvaluatePlayer(PlayerModel youngPlayer, int minRating)
        {
            YoungPlayerEvaluation result=new YoungPlayerEvaluation();

            
            result.Player = youngPlayer;

            //cut the lower half

            var player = new PlayerModel(youngPlayer);

            var playerModelDouble = new PlayerModelDouble(youngPlayer);
            playerModelDouble.Position = player.Position= (int) this.Position;


            int weeks = 0;
            bool maxedOut = false;
            List<TrainingScheduleSteps> lastWeeklyTrainingSchedule = null;
            var trainingCount = new int[(int)TrainingScheduleType.Count];

            var playerBeforeCurrentSchedule=youngPlayer;

            int lastScheduleLastedWeekCount = 0;
            while (!maxedOut)
            {
                var projectedAttributesAfterSprinting = TrainingSchedule.ProjectedAttributesAfterSprinting(player, Position, TrainingEffectModifier,TrainingEffects);
                var projectedAttributesAfterTrainingMatch = TrainingSchedule.ProjectedAttributesAfterTrainingMatch(player, Position,TrainingEffects,
                    projectedAttributesAfterSprinting);
                var attributeLeftToTrain = TrainingSchedule.GetAttributesToTrain(player, Position);
                var bottleneckAttributeIndex = TrainingSchedule.GetTopBottleneckAttributes(player, Position, TrainingEffectModifier,TrainingEffects, attributeLeftToTrain
                    , projectedAttributesAfterSprinting, projectedAttributesAfterSprinting);
                var weeklyTrainingSchedule = TrainingSchedule.GetTrainingSchedule(player, AutoResetStatus, MaxEnergy, MaxPower, NoAlternativeTraining
                    , TrainingEffectModifier,TrainingEffects, DebugTraining, projectedAttributesAfterSprinting, projectedAttributesAfterTrainingMatch, attributeLeftToTrain, bottleneckAttributeIndex);


                double weeklyTrainingScheduleEffectsOnPosition = 0;

                if (lastWeeklyTrainingSchedule == null)
                {
                    result.WeeklyTrainingSchedules.AddLast(new WeeklyTrainingSchedule
                    {
                        Steps = weeklyTrainingSchedule,
                        Weeks = 0,
                        BottleneckAttributes = bottleneckAttributeIndex,
                        Player= youngPlayer
                    });
                }
                else if (weeklyTrainingSchedule!=null && !lastWeeklyTrainingSchedule.Select(x=>x.TrainingScheduleType).SequenceEqual(weeklyTrainingSchedule.Select(x => x.TrainingScheduleType)))
                {
                    if (result.WeeklyTrainingSchedules.Count > 0)
                    {
                        result.WeeklyTrainingSchedules.Last().Weeks = lastScheduleLastedWeekCount;
                    }
                    result.WeeklyTrainingSchedules.AddLast(new WeeklyTrainingSchedule
                    { 
                        Steps = weeklyTrainingSchedule,
                        Weeks = 0,
                        BottleneckAttributes = bottleneckAttributeIndex ,
                        Player = new PlayerModel(player)
                    });
                    lastScheduleLastedWeekCount = 0;
                }
                if (weeklyTrainingSchedule != null)
                {
                    for (int dayOfWeek = 0; dayOfWeek < weeklyTrainingSchedule.Count; dayOfWeek++)
                    {
                        var dailyTrainingScheduleType = weeklyTrainingSchedule[dayOfWeek];
                        for (int attributeIndex = 0; attributeIndex < player.Attributes.Length; attributeIndex++)
                        {
                            var dailyTrainingEffect = TrainingEffects[(int)dailyTrainingScheduleType.TrainingScheduleType][attributeIndex];
                            if (dailyTrainingEffect != 0)
                            {
                                var preciseAttributeBefore = playerModelDouble.Attributes[attributeIndex];
                                var preciseAttributeAfter = preciseAttributeBefore + dailyTrainingEffect;
                                if (preciseAttributeAfter > TrainingSchedule.attributeCap)
                                    preciseAttributeAfter = TrainingSchedule.attributeCap;
                                if (preciseAttributeAfter < 0)
                                    preciseAttributeAfter = TrainingSchedule.attributeCap;
                                if (AttributeRelevanceForPosition[attributeIndex] > 0)
                                {
                                    weeklyTrainingScheduleEffectsOnPosition += preciseAttributeAfter - preciseAttributeBefore;
                                }
                                player.Attributes[attributeIndex] = (byte)(preciseAttributeAfter+0.1);
                                playerModelDouble.Attributes[attributeIndex] = preciseAttributeAfter;
                            }
                        }
                        trainingCount[(int)dailyTrainingScheduleType.TrainingScheduleType]++;
                    }
                }
                weeks++;
                Debug.Assert(weeks < 1800);
                lastScheduleLastedWeekCount++;
                player.Position = (int)this.Position;

                if (player.PositionRating == 99 || weeklyTrainingScheduleEffectsOnPosition == 0)
                {
                    //Debug.Assert(weeks > 100);
                    maxedOut = true;
                    if (result.WeeklyTrainingSchedules.Count > 0)
                    {
                        result.WeeklyTrainingSchedules.Last().Weeks = lastScheduleLastedWeekCount;
                        result.WeeklyTrainingSchedules.AddLast(new WeeklyTrainingSchedule
                        {
                            Steps = null,
                            Weeks = 0,
                            BottleneckAttributes = null,
                            Player = new PlayerModel(player)
                        });
                    }

                    OnEvalPlayerPositionComplete?.Invoke(this, EventArgs.Empty);
                    lastScheduleLastedWeekCount = 0;
                }
                lastWeeklyTrainingSchedule= weeklyTrainingSchedule;
            }
            result.WeeksToMax = weeks;
            result.FinalRating = player.PositionRating;
            return result;
        }
        /*
        private static void AddScheduleToOuput(YoungPlayerEvaluation result, PlayerModel playerClone, List<TrainingScheduleStep> weeklyTrainingSchedule, List<BottleneckAttributes> bottleneckAttributeIndex, bool debugTraining)
        {
            if (debugTraining)
            {
                result.Schedules.AppendFormat("{0},{1},{2}\t{3},{4:2},{5}\t{6},{7},{8},{9},{10}\t{11},{12}\t{13},{14},{15}\t{16},{17},{18}\t{19},{20} {21}",
                    playerClone.Speed, playerClone.Agility, playerClone.Acceleration,
                    playerClone.Stamina, playerClone.Strength, playerClone.Fitness,
                    playerClone.Shooting, playerClone.Passing, playerClone.Heading, playerClone.Control, playerClone.Dribbling,
                    playerClone.TackleDetermination, playerClone.TackleSkill,
                    playerClone.Coolness, playerClone.Awareness, playerClone.Flair,
                    playerClone.Kicking, playerClone.Throwing, playerClone.Handling,
                    playerClone.Consistency, playerClone.Determination, playerClone.Leadership
                    );
                if (weeklyTrainingSchedule != null)
                {
                    if (bottleneckAttributeIndex != null)
                    {
                        result.Schedules.Append(" Bottlenecks: ");
                        StringBuilder stringBuilder = new StringBuilder();
                        bool firstAttribute = true;
                        foreach (var item in bottleneckAttributeIndex)
                        {
                            if (firstAttribute)
                            {
                                firstAttribute = false;
                            }
                            else
                                stringBuilder.Append(",");

                            if (item.Repeat > 0)
                            {
                                stringBuilder.AppendFormat("{0} ({1})", item.AttributeIndex.ToLocalizedString(), item.Repeat);
                            }
                            else
                                stringBuilder.AppendFormat("{0}", item.AttributeIndex.ToLocalizedString());
                        }
                        result.Schedules.AppendFormat("{0,-60}", stringBuilder.ToString());
                    }                    
                    result.Schedules.Append(" Next: ");
                    bool firstdailyTrainingScheduleStep = true;
                    for (int dayOfWeek = 0; dayOfWeek < weeklyTrainingSchedule.Count; dayOfWeek++)
                    {
                        var dailyTrainingScheduleStep = weeklyTrainingSchedule[dayOfWeek];
                        if (firstdailyTrainingScheduleStep)
                        {
                            firstdailyTrainingScheduleStep = false;
                        }
                        else
                        {
                            result.Schedules.Append(",");                            
                        }
                        result.Schedules.AppendFormat("{0} ({1})", dailyTrainingScheduleStep.TrainingScheduleType.ToLocalizedString(), dailyTrainingScheduleStep.ForPlayerAttribute.ToLocalizedString() );
                    }
                }
            }
        }*/
    }
}
