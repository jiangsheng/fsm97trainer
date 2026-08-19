using FSM97Lib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;

namespace Fsm97Trainer
{
    internal class EvaluateYoungPlayersResult
    {

        public EvaluateYoungPlayersResult(PlayerPosition position, List<PlayerModelDouble> youngPlayers, bool autoResetStatus, bool maxEnergy,  bool maxPower, bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier, bool alwaysTrainConsistency ,TrainingActivity[] trainingEffects)
        {
            Position = position;
            YoungPlayers = youngPlayers;
            AutoResetStatus = autoResetStatus;
            MaxEnergy = maxEnergy;
            MaxPower = maxPower;
            NoAlternativeTraining = noAlternativeTraining;
            TrainingEffectModifier = trainingEffectModifier;
            TrainingScheduleEffects= TrainingActivity.GetTrainingEffects(trainingEffectModifier);
            AttributeRelevanceForPosition = PositionRatings.Ratings[(int)Position];
            TrainingEffects = trainingEffects;
            AlwaysTrainConsistency = alwaysTrainConsistency;
        }
        byte[] TrainingScheduleEffects { get; set; }

        public List<YoungPlayerEvaluation> Grades { get; set; }
        public PlayerPosition Position { get; }
        public List<PlayerModelDouble> YoungPlayers { get; }
        public bool AutoResetStatus { get; }
        public bool MaxEnergy { get; }
        public bool MaxPower { get; }
        public bool NoAlternativeTraining { get; }
        public TrainingEffectModifier TrainingEffectModifier { get; }
        public bool AlwaysTrainConsistency { get; set; }
        byte[] AttributeRelevanceForPosition { get; set; }
        TrainingActivity[] TrainingEffects { get; set; }

        public event EventHandler OnEvalPlayerPositionComplete;
        public void Evaluate(int minRating)
        {
            Grades = new List<YoungPlayerEvaluation>(YoungPlayers.Count);
            foreach (var youngPlayer in YoungPlayers)
            {
                var positionRating = PositionRatings.GetPositionRatingDouble((int)this.Position, youngPlayer);
                if (positionRating > minRating)
                {
                    Grades.Add(EvaluatePlayer(youngPlayer, minRating, AlwaysTrainConsistency));
                }
                else {
                    OnEvalPlayerPositionComplete?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        YoungPlayerEvaluation EvaluatePlayer(PlayerModelDouble youngPlayer, int minRating, bool alwaysTrainConsistency)
        {
            YoungPlayerEvaluation result = new YoungPlayerEvaluation();


            result.Player = youngPlayer;

            //copy attibutes off but change position to the one we are evaluating
            PlayerModelDouble currentTrainingResult = new PlayerModelDouble(youngPlayer);

            currentTrainingResult.Position = (int)this.Position;


            int weeks = 0;
            bool maxedOut = false;
            List<TrainingScheduleSteps> lastWeeklyTrainingSchedule = null;
            var trainingCount = new int[(int)TrainingActivityType.Count];

            int lastScheduleLastedWeekCount = 0;
            PlayerModelDouble lastTrainingResult = null;
            TrainingScheduleCalculationState trainingScheduleCalculationState = new TrainingScheduleCalculationState(
                currentTrainingResult, false,Position, AutoResetStatus, MaxEnergy, MaxPower, NoAlternativeTraining, alwaysTrainConsistency, TrainingEffectModifier, TrainingEffects
                );
            while (!maxedOut)
            {
                var weeklyTrainingSchedule = TrainingSchedule.GetTrainingSchedule(trainingScheduleCalculationState);

                double[] weeklyTrainingScheduleEffectsOnPosition = new double[(int)PlayerAttribute.Count];

                if (lastWeeklyTrainingSchedule == null)
                {
                    //this is the first schedule
                    result.WeeklyTrainingSchedules.AddLast(new WeeklyTrainingSchedule
                    {
                        Steps = weeklyTrainingSchedule,
                        Weeks = 0,
                        BottleneckAttributes = trainingScheduleCalculationState.BottleneckAttributes,
                        Player = youngPlayer //initial attributes before training
                    });
                }
                else if (weeklyTrainingSchedule != null && !lastWeeklyTrainingSchedule.Select(x => x.TrainingScheduleType).SequenceEqual(weeklyTrainingSchedule.Select(x => x.TrainingScheduleType)))
                {
                    //this is a new schedule, so we need to add the last schedule to the result
                    if (result.WeeklyTrainingSchedules.Count > 0)
                    {
                        result.WeeklyTrainingSchedules.Last().Weeks = lastScheduleLastedWeekCount;
                    }
                    result.WeeklyTrainingSchedules.AddLast(new WeeklyTrainingSchedule
                    {
                        Steps = weeklyTrainingSchedule,
                        Weeks = 0,
                        BottleneckAttributes = trainingScheduleCalculationState.BottleneckAttributes,
                        Player = new PlayerModelDouble(currentTrainingResult)//copy the current progress off for reporting purposes
                    });
                    lastScheduleLastedWeekCount = 0;
                }
                if (weeklyTrainingSchedule != null)
                {
                    for (int dayOfWeek = 0; dayOfWeek < weeklyTrainingSchedule.Count; dayOfWeek++)
                    {
                        var dailyTrainingScheduleType = weeklyTrainingSchedule[dayOfWeek];
                        for (int attributeIndex = 0; attributeIndex < currentTrainingResult.Attributes.Length; attributeIndex++)
                        {
                            var dailyTrainingEffect = TrainingEffects[(int)dailyTrainingScheduleType.TrainingScheduleType].Effects[attributeIndex];
                            if (dailyTrainingEffect != 0)
                            {
                                var preciseAttributeBefore = currentTrainingResult.Attributes[attributeIndex];
                                var preciseAttributeAfter = preciseAttributeBefore + dailyTrainingEffect;
                                if (preciseAttributeAfter > TrainingSchedule.attributeCap + TrainingSchedule.ConstantFastCeiling)
                                    preciseAttributeAfter = TrainingSchedule.attributeCap + TrainingSchedule.ConstantFastCeiling;
                                if (preciseAttributeAfter < 0) //this game underflows to max
                                    preciseAttributeAfter = TrainingSchedule.attributeCap + TrainingSchedule.ConstantFastCeiling;
                                if (AttributeRelevanceForPosition[attributeIndex] > 0)
                                {
                                    weeklyTrainingScheduleEffectsOnPosition[attributeIndex] = preciseAttributeAfter - preciseAttributeBefore;
                                }
                                currentTrainingResult.Attributes[attributeIndex] = preciseAttributeAfter;
                            }
                            Debug.Assert(currentTrainingResult.Attributes[attributeIndex] < TrainingSchedule.attributeCap + 1);
                        }
                        trainingCount[(int)dailyTrainingScheduleType.TrainingScheduleType]++;
                    }
                }
                weeks++;
                Debug.Assert(weeks < 2600);
                lastScheduleLastedWeekCount++;

                maxedOut = true;
                for (int attributeIndex = 0; attributeIndex < currentTrainingResult.Attributes.Length; attributeIndex++)
                {
                    if (AttributeRelevanceForPosition[attributeIndex] > 0)
                    {
                        if (currentTrainingResult.Attributes[attributeIndex] < TrainingSchedule.attributeCap)
                        {
                            if (attributeIndex == (int)PlayerAttribute.Leadership)
                                maxedOut = !TrainingEffectModifier.PassingTrainLeadership;
                            else
                            {
                                maxedOut = false;
                                break;
                            }
                        }
                    }
                }
                if (maxedOut)
                {
                    if (result.WeeklyTrainingSchedules.Count > 0)
                    {
                        result.WeeklyTrainingSchedules.Last().Weeks = lastScheduleLastedWeekCount;
                        result.WeeklyTrainingSchedules.AddLast(new WeeklyTrainingSchedule
                        {
                            Steps = null,
                            Weeks = 0,
                            BottleneckAttributes = null,
                            Player = new PlayerModelDouble(currentTrainingResult)//copy the current progress off for reporting purposes
                        });
                    }

                    OnEvalPlayerPositionComplete?.Invoke(this, EventArgs.Empty);
                    lastScheduleLastedWeekCount = 0;
                }
                else
                {
                    Debug.Assert(weeklyTrainingSchedule != null);
                }
                lastWeeklyTrainingSchedule = weeklyTrainingSchedule;
                lastTrainingResult = new PlayerModelDouble(currentTrainingResult);
            }
            result.WeeksToMax = weeks;
            List<int> attributes = currentTrainingResult.Attributes.Select(x => (int)x).ToList();


            result.FinalRating =(int)(new PlayerModel(attributes).GetPositionRatingDouble(currentTrainingResult.Position));
            Debug.Assert(result.FinalRating >= TrainingSchedule.attributeCap
                ||this.Position==PlayerPosition.CD);
            return result;
        }
    }
}
