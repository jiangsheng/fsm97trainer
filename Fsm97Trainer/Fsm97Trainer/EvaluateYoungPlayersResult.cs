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

        public EvaluateYoungPlayersResult(PlayerPosition position, List<Player> youngPlayers, bool autoResetStatus, bool maxEnergy,  bool maxPower, bool noAlternativeTraining, TrainingEffectModifier trainingEffectModifier)
        {
            Position = position;
            YoungPlayers = youngPlayers;
            AutoResetStatus = autoResetStatus;
            MaxEnergy = maxEnergy;
            MaxPower = maxPower;
            NoAlternativeTraining = noAlternativeTraining;
            TrainingEffectModifier = trainingEffectModifier;
            GetTrainingScheduleEffects= TrainingScheduleEffect.GetTrainingScheduleEffect(trainingEffectModifier);
        }
        byte[] GetTrainingScheduleEffects { get; set; }

        public List<YoungPlayerEvaluation> Grades { get; set; }
        public PlayerPosition Position { get; }
        public List<Player> YoungPlayers { get; }
        public bool AutoResetStatus { get; }
        public bool MaxEnergy { get; }
        public bool MaxPower { get; }
        public bool NoAlternativeTraining { get; }
        public TrainingEffectModifier TrainingEffectModifier { get; }
        public event EventHandler OnEvalPlayerPositionComplete;
        public void Evaluate()
        {
            Grades = new List<YoungPlayerEvaluation>();
            foreach (var youngPlayer in YoungPlayers)
            {
                Grades.Add(EvaluatePlayer(youngPlayer));
            }
        }
        YoungPlayerEvaluation EvaluatePlayer(Player youngPlayer)
        {
            YoungPlayerEvaluation result=new YoungPlayerEvaluation();
            result.Player = youngPlayer;
            Player playerClone = new Player();
            Buffer.BlockCopy(youngPlayer.Attributes, 0, playerClone.Attributes, 0, playerClone.Attributes.Length);
            playerClone.LastName = youngPlayer.LastName;

            playerClone.Position= (int) this.Position;

            List<double> preciseAttribute= 
                Enumerable.Range(0, playerClone.Attributes.Length).
                Select(n=>(double)0).ToList(); 

            for (int i = 0; i < playerClone.Attributes.Length; i++)
            {
                preciseAttribute[i] = playerClone.Attributes[i];
            }
            int weeks = 0;
            bool maxedOut = false;
            TrainingScheduleType[] lastWeeklyTrainingSchedule = null;
            int lastScheduleLastedWeekCount = 0;
            while (!maxedOut)
            {
                var weeklyTrainingSchedule = TrainingSchedule.GetTrainingSchedule(playerClone, AutoResetStatus, MaxEnergy, MaxPower, NoAlternativeTraining
                    , TrainingEffectModifier);

                double weeklyTrainingScheduleEffects = 0;
                for (int dayOfWeek = 0; dayOfWeek < weeklyTrainingSchedule.Length; dayOfWeek++)
                {
                    var dailyTrainingScheduleType = weeklyTrainingSchedule[dayOfWeek];                    
                    if (dailyTrainingScheduleType == TrainingScheduleType.None)
                    {
                        continue;
                    }
                    for (int attributeIndex = 0; attributeIndex < playerClone.Attributes.Length; attributeIndex++)
                    {
                        var dailyTrainingEffect = TrainingEffectModifier.RawData[(int)dailyTrainingScheduleType * 27 + attributeIndex];
                        var preciseAttributeBefore = preciseAttribute[attributeIndex];
                        var preciseAttributeAfter = preciseAttribute[attributeIndex] + dailyTrainingEffect;
                        if (preciseAttributeAfter >= 99)
                            preciseAttributeAfter = 99;
                        if (preciseAttributeAfter < 0)
                            preciseAttributeAfter = 99;

                        weeklyTrainingScheduleEffects += preciseAttributeAfter - preciseAttributeBefore;

                        playerClone.Attributes[attributeIndex] = (byte)Math.Round(preciseAttributeAfter, 1);
                        preciseAttribute[attributeIndex] = preciseAttributeAfter;
                    }
                }
                if (lastWeeklyTrainingSchedule == null || !lastWeeklyTrainingSchedule.SequenceEqual(weeklyTrainingSchedule))
                {
                    AddScheduleToOuput(result, playerClone, weeklyTrainingSchedule, lastScheduleLastedWeekCount);
                    lastScheduleLastedWeekCount = 0;
                }
                weeks++;
                lastScheduleLastedWeekCount++;
                playerClone.PositionRating =(int)PositionRatings.GetPositionRatingDouble(playerClone.Position, playerClone.Attributes);
                playerClone.UpdateBestPosition();

                if (playerClone.PositionRating == 99 || weeklyTrainingScheduleEffects == 0)
                {
                    Debug.Assert(weeks > 100);
                    maxedOut = true;                    
                    OnEvalPlayerPositionComplete?.Invoke(this, EventArgs.Empty);
                    AddScheduleToOuput(result, playerClone, weeklyTrainingSchedule, lastScheduleLastedWeekCount );
                    lastScheduleLastedWeekCount = 0;
                }
                lastWeeklyTrainingSchedule= weeklyTrainingSchedule;
            }
            result.WeeksToMax = weeks;
            result.FinalRating = playerClone.PositionRating;
            return result;
        }

        private static void AddScheduleToOuput(YoungPlayerEvaluation result, Player playerClone, TrainingScheduleType[] weeklyTrainingSchedule, int lastScheduleLastedWeekCount)
        {
            return;
            result.Schedules.AppendFormat("{0},{1},{2}\t{3},{4:2},{5}\t{6},{7},{8},{9},{10}\t{11},{12}\t{13},{14},{15}\t{16},{17},{18}\t{19},{20} ",
                playerClone.Speed, playerClone.Agility, playerClone.Acceleration,
                playerClone.Stamina, playerClone.Strength, playerClone.Fitness,
                playerClone.Shooting, playerClone.Passing, playerClone.Heading, playerClone.Control, playerClone.Dribbling,
                playerClone.TackleDetermination, playerClone.TackleSkill,
                playerClone.Coolness, playerClone.Awareness, playerClone.Flair,
                playerClone.Kicking, playerClone.Throwing, playerClone.Handling,
                playerClone.Consistency, playerClone.Determination
                );
            for (int dayOfWeek = 0; dayOfWeek < weeklyTrainingSchedule.Length; dayOfWeek++)
            {
                var dailyTrainingScheduleType = weeklyTrainingSchedule[dayOfWeek];
                if (result.Schedules.Length != 0)
                    result.Schedules.Append(",");
                result.Schedules.Append(dailyTrainingScheduleType);
            }
            result.Schedules.AppendFormat("\tWeeks:{0}", lastScheduleLastedWeekCount);
            result.Schedules.AppendLine();
        }
    }
}
