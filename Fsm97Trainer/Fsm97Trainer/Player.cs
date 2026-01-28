using FSM97Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fsm97Trainer
{
    public class Player : IObjectWithPersonName
    {

        public int Speed
        {
            get { return (int)attributes[(int)PlayerAttribute.Speed]; }
            set { attributes[(int)PlayerAttribute.Speed] = (byte)value; }
        }
        public int Agility
        {
            get { return (int)attributes[(int)PlayerAttribute.Agility]; }
            set { attributes[(int)PlayerAttribute.Agility] = (byte)value; }
        }
        public int Acceleration
        {
            get { return (int)attributes[(int)PlayerAttribute.Acceleration]; }
            set { attributes[(int)PlayerAttribute.Acceleration] = (byte)value; }
        }
        public int Stamina
        {
            get { return (int)attributes[(int)PlayerAttribute.Stamina]; }
            set { attributes[(int)PlayerAttribute.Stamina] = (byte)value; }
        }
        public int Strength
        {
            get { return (int)attributes[(int)PlayerAttribute.Strength]; }
            set { attributes[(int)PlayerAttribute.Strength] = (byte)value; }
        }
        public int Fitness
        {
            get { return (int)attributes[(int)PlayerAttribute.Fitness]; }
            set { attributes[(int)PlayerAttribute.Fitness] = (byte)value; }
        }
        public int Shooting
        {
            get { return (int)attributes[(int)PlayerAttribute.Shooting]; }
            set { attributes[(int)PlayerAttribute.Shooting] = (byte)value; }
        }
        public int Passing
        {
            get { return (int)attributes[(int)PlayerAttribute.Passing]; }
            set { attributes[(int)PlayerAttribute.Passing] = (byte)value; }
        }
        public int Heading
        {
            get { return (int)attributes[(int)PlayerAttribute.Heading]; }
            set { attributes[(int)PlayerAttribute.Heading] = (byte)value; }
        }
        public int Control
        {
            get { return (int)attributes[(int)PlayerAttribute.Control]; }
            set { attributes[(int)PlayerAttribute.Control] = (byte)value; }
        }
        public int Dribbling
        {
            get { return (int)attributes[(int)PlayerAttribute.Dribbling]; }
            set { attributes[(int)PlayerAttribute.Dribbling] = (byte)value; }
        }
        public int TackleDetermination
        {
            get { return (int)attributes[(int)PlayerAttribute.TackleDetermination]; }
            set { attributes[(int)PlayerAttribute.TackleDetermination] = (byte)value; }
        }
        public int TackleSkill
        {
            get { return (int)attributes[(int)PlayerAttribute.TackleSkill]; }
            set { attributes[(int)PlayerAttribute.TackleSkill] = (byte)value; }
        }
        public int Coolness
        {
            get { return (int)attributes[(int)PlayerAttribute.Coolness]; }
            set { attributes[(int)PlayerAttribute.Coolness] = (byte)value; }
        }
        public int Awareness
        {
            get { return (int)attributes[(int)PlayerAttribute.Awareness]; }
            set { attributes[(int)PlayerAttribute.Awareness] = (byte)value; }
        }
        public int Flair
        {
            get { return (int)attributes[(int)PlayerAttribute.Flair]; }
            set { attributes[(int)PlayerAttribute.Flair] = (byte)value; }
        }
        public int Kicking
        {
            get { return (int)attributes[(int)PlayerAttribute.Kicking]; }
            set { attributes[(int)PlayerAttribute.Kicking] = (byte)value; }
        }
        public int Throwing
        {
            get { return (int)attributes[(int)PlayerAttribute.Throwing]; }
            set { attributes[(int)PlayerAttribute.Throwing] = (byte)value; }
        }
        public int Handling
        {
            get { return (int)attributes[(int)PlayerAttribute.Handling]; }
            set { attributes[(int)PlayerAttribute.Handling] = (byte)value; }
        }
        public int ThrowIn
        {
            get { return (int)attributes[(int)PlayerAttribute.ThrowIn]; }
            set { attributes[(int)PlayerAttribute.ThrowIn] = (byte)value; }
        }
        public int Leadership
        {
            get { return (int)attributes[(int)PlayerAttribute.Leadership]; }
            set { attributes[(int)PlayerAttribute.Leadership] = (byte)value; }
        }
        public int Consistency
        {
            get { return (int)attributes[(int)PlayerAttribute.Consistency]; }
            set { attributes[(int)PlayerAttribute.Consistency] = (byte)value; }
        }
        public int Determination
        {
            get { return (int)attributes[(int)PlayerAttribute.Determination]; }
            set { attributes[(int)PlayerAttribute.Determination] = (byte)value; }
        }
        public int Greed
        {
            get { return (int)attributes[(int)PlayerAttribute.Greed]; }
            set { attributes[(int)PlayerAttribute.Greed] = (byte)value; }
        }
        int position;
        public int Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                positionName = GetPositionName(position);
                PositionRating = GetPositionRating(Position);
            }
        }
        public string PositionName { get => positionName; }
        public int PositionRating { get; set; }
        public int BestPosition { get; set; }
        public string BestPositionName { get; set; }
        public int BestPositionRating { get; set; }

        int nationality;
        public int Nationality
        {
            get { return nationality; }
            set
            {
                nationality = value;
                NationalityName = GetNationalityName(value);
            }
        }
        public string NationalityName { get; set; }
        public string TeamName { get; set; }
        public string TeamAbbrivation { get; set; }
        public int Age { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public int Number { get; set; }
        public int Status { get; set; }
        public int ContractWeeks { get; set; }
        public int Form { get; set; }
        public int Moral { get; set; }
        public int Energy { get; set; }
        public double Salary { get; set; }
        public int Goals { get; set; }
        public int MVP { get; set; }
        public int GamesThisSeason { get; set; }
        public ushort BirthDateOffset { get; set; }
        public int Statistics
        {
            get
            {
                int sum = 0;
                for (int i = 0; i < (int)PlayerAttribute.Count; i++)
                {
                    sum += Attributes[i];
                }
                sum += Form + Moral + Energy;
                return sum / 24;
            }
        }
        Team team;
        public Team Team
        {
            get { return team; }
            set
            {
                team = value;
                TeamName = team.Name;
                TeamAbbrivation = team.Abbreviation;
            }
        }
        byte[] attributes = new byte[(int)PlayerAttribute.Count];
        private string positionName;

        public byte[] Attributes
        {
            get
            {
                return attributes;
            }
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("{0} {1} {2} {3}",PositionName,Number, LastName,FirstName);
            stringBuilder.AppendFormat(" age {0}, position {1} rating {2}", Age, positionName,PositionRating);
            stringBuilder.AppendFormat(" best position {0}, best position rating {1}", BestPositionName, BestPositionRating);
            stringBuilder.AppendFormat("\r\n Speeds: {0}, Agility: {1}, Acceleration: {2} , Healths: Stamina: {3}, Strength: {4}, Fitness: {5}",
                Speed, Agility, Acceleration, Stamina, Strength, Fitness);
            stringBuilder.AppendFormat(" Skills: Shooting: {0}, Passing: {1}, Heading: {2}, Control: {3}, Dribbling: {4}",
                Shooting, Passing, Heading, Control, Dribbling);
            stringBuilder.AppendFormat("\r\n tackles: Determination: {0}, Skill: {1}",
                TackleDetermination, TackleSkill);
            stringBuilder.AppendFormat(" Mentalities: Coolness: {0}, Awareness: {1}, Flair: {2}",
                Coolness, Awareness, Flair);
            stringBuilder.AppendFormat("\r\n Goalkeeping: Kicking: {0}, Throwing: {1}, Handling: {2}",
                Kicking, Throwing, Handling);
            stringBuilder.AppendFormat(" Others: ThrowIn: {0}, Leadership: {1}, Consistency: {2}, Determination: {3}, Greed: {4}",
                ThrowIn, Leadership, Consistency, Determination, Greed);
            return stringBuilder.ToString();
        }
        internal static bool CompareAttributes(Player fromPlayer, Player toPlayer)
        {
            return fromPlayer.Attributes.SequenceEqual(toPlayer.Attributes);
        }
        public static int CompareAttributesApproximately(Player fromPlayer, Player toPlayer)
        {
            for (int i = 0; i < (int)PlayerAttribute.Count; i++)
            {
                var result = fromPlayer.Attributes[i] - toPlayer.Attributes[i];
                if (result > 1 || result < -1) return result;

            }
            return 0;

        }
        public void UpdateBestPosition()
        {
            int bestPosition = 0;
            double bestPositionRating = 0;
            for (int i = 0; i < (int)PlayerPosition.Count; i++)
            {
                double testPositionRating = PositionRatings.GetPositionRatingDouble(i, attributes);
                if (bestPositionRating < testPositionRating)
                {
                    bestPosition = i;
                    bestPositionRating = testPositionRating;
                }
            }
            BestPosition = bestPosition;
            BestPositionName = GetPositionName(bestPosition);
            BestPositionRating = (int)bestPositionRating;
        }

        //see country,.txt for coutries
        private string GetNationalityName(int nationality)
        {
            switch (nationality)
            {
                case 0x1a:
                case 0x36:
                case 0x42:
                case 0x53: return "ENG";
                case 0x1f: return "FRA";
                case 0x21: return "GER";
                case 0x28: return "ITA";
                case 0x49: return "SPA";
                default: return "OTH";
            }
        }

        public double GetPositionRatingDouble(int position)
        {
            return PositionRatings.GetPositionRatingDouble
                (position, this.attributes);
            /*
            double playerRating = 0;
            switch ((PlayerPosition)position)
            {
                case PlayerPosition.GK:
                    playerRating = Speed * 2 + Agility * 25 +
                        Passing * 2 + Control * 4 +
                        Coolness * 7 + Awareness * 10 +
                        Kicking * 8 + Throwing * 6 + Handling * 30 +
                        Consistency * 6;
                    break;
                case PlayerPosition.LB:
                case PlayerPosition.RB:
                    playerRating = Speed * 3 +
                        Passing * 7 + Heading * 8 +
                        TackleDetermination * 10 + TackleSkill * 44 +
                        Coolness * 7 + Awareness * 15 +
                        Consistency * 4 + Determination * 2;
                    break;
                case PlayerPosition.CD:
                    playerRating = Speed * 3 +
                        Passing * 3 + Heading * 14 +
                        TackleDetermination * 10 + TackleSkill * 50 +
                        Coolness * 7 + Awareness * 8 +
                        Consistency * 2 + Leadership * 3;
                    break;
                case PlayerPosition.LWB:
                case PlayerPosition.RWB:
                    playerRating = Speed * 7 + Agility * 4 + Acceleration * 11 +
                        Passing * 12 + Dribbling * 26 +
                        TackleDetermination * 3 + TackleSkill * 26 +
                        Flair * 5 + Awareness * 6;
                    break;
                case PlayerPosition.SW:
                    playerRating = Speed * 12 + Acceleration * 6 +
                        Passing * 15 + Heading * 3 + Dribbling * 15 +
                        TackleDetermination * 3 + TackleSkill * 26 +
                        Awareness * 20;
                    break;
                case PlayerPosition.DM:
                    playerRating = Speed * 5 +
                        Passing * 40 + Heading * 5 +
                        TackleDetermination * 3 + TackleSkill * 27 +
                        Awareness * 20;
                    break;
                case PlayerPosition.LM:
                case PlayerPosition.RM:
                    playerRating = Speed * 10 + Acceleration * 5 +
                        Shooting * 3 + Passing * 42 + Control * 5 + Dribbling * 5 +
                        TackleSkill * 20 +
                        Flair * 5 + Awareness * 5;
                    break;
                case PlayerPosition.AM:
                    playerRating = Speed * 10 + Acceleration * 5 +
                        Shooting * 5 + Passing * 46 + Control * 5 + Dribbling * 5 +
                        TackleSkill * 14 +
                        Flair * 5 + Awareness * 5;
                    break;
                case PlayerPosition.LW:
                case PlayerPosition.RW:
                    playerRating = Speed * 10 + Agility * 3 + Acceleration * 10 +
                        Shooting * 3 + Passing * 31 + Control * 3 + Dribbling * 27 +
                        TackleSkill * 3 +
                         Awareness * 3 + Flair * 7;
                    break;
                case PlayerPosition.FR:
                    playerRating = Speed * 12 + Agility * 2 + Acceleration * 8 +
                        Shooting * 4 + Passing * 14 + Heading + Control * 10 + Dribbling * 27 +
                         Awareness * 12 + Flair * 10;
                    break;
                case PlayerPosition.FOR:
                    playerRating = Speed * 10 + Agility * 2 + Acceleration * 9 +
                        Shooting * 36 + Passing * 4 + Heading * 10 + Control * 10 + Dribbling * 3 +
                        +Coolness * 3 + Awareness * 4 + Flair * 9;
                    break;
                case PlayerPosition.SS:
                    playerRating = Speed * 6 + Agility * 2 + Acceleration * 6 +
                        Shooting * 29 + Passing * 16 + Heading * 7 + Control * 13 + Dribbling * 6 +
                        +Coolness * 2 + Awareness * 3 + Flair * 10;
                    break;
            }
            return playerRating / 100;*/
        }


        public int GetPositionRating(int position)
        {
            return (int)GetPositionRatingDouble(position);
        }
        public string GetPositionName(int position)
        {
            return Enum.GetName(typeof(PlayerPosition), position);
        }

        public int BestFitInFormation(Formation targetFormation)
        {
            double bestPositionRating = 0;
            int bestPosition = 0;
            for (int i = 0; i < targetFormation.PlayersInEachPosition.Length; i++)
            {
                if (targetFormation.PlayersInEachPosition[i] > 0)
                {
                    double testPositionRating = GetPositionRatingDouble(i);
                    if (bestPositionRating < testPositionRating)
                    {
                        bestPosition = i;
                        bestPositionRating = testPositionRating;
                    }
                }
            }
            return bestPosition;
        }
        internal int GetBestPositionExceptGKInFormation(Formation formation)
        {
            double bestPositionRating = 0;
            int bestPosition = 0;
            for (int i = 1; i < (int)PlayerPosition.Count; i++)
            {
                if (formation != null)
                {
                    if (formation.PlayersInEachPosition[i] == 0) continue;
                }
                double testPositionRating = GetPositionRatingDouble(i);
                if (bestPositionRating < testPositionRating)
                {
                    bestPosition = i;
                    bestPositionRating = testPositionRating;
                }
            }
            return bestPosition;
        }
        public double GetBestPositionRatingExceptGKInFormation(Formation formation)
        {
            double bestPositionRating = 0;
            int bestPosition = 0;
            for (int i = 1; i < (int)PlayerPosition.Count; i++)
            {
                if (formation != null)
                {
                    if (formation.PlayersInEachPosition[i] == 0) continue;
                }
                double testPositionRating = GetPositionRatingDouble(i);
                if (bestPositionRating < testPositionRating)
                {
                    bestPosition = i;
                    bestPositionRating = testPositionRating;
                }
            }
            return bestPositionRating;
        }

        public double GetBestPositionRating(PlayerPosition[] targetPositions)
        {
            double bestPositionRating = 0;
            PlayerPosition bestPosition = 0;
            foreach (var targetPosition in targetPositions)
            {
                double testPositionRating = GetPositionRatingDouble((int)targetPosition);
                if (bestPositionRating < testPositionRating)
                {
                    bestPosition = targetPosition;
                    bestPositionRating = testPositionRating;
                }
            }
            return bestPositionRating;
        }

        public int BestFitInPositions(PlayerPosition[] targetPositions)
        {
            double bestPositionRating = 0;
            PlayerPosition bestPosition = 0;
            foreach (var targetPosition in targetPositions)
            {
                double testPositionRating = GetPositionRatingDouble((int)targetPosition);
                if (bestPositionRating < testPositionRating)
                {
                    bestPosition = targetPosition;
                    bestPositionRating = testPositionRating;
                }
            }
            return (int)bestPosition;
        }

        public double GetAveragePositionRatingInFormationExceptTargetPositionAndGK(Player data, int position, Formation formation)
        {
            double sumPositionRating = 0;
            int countPositions = 0;

            for (int i = 1; i < (int)PlayerPosition.Count; i++)
            {
                if (formation != null)
                {
                    if (formation.PlayersInEachPosition[i] == 0) continue;
                }

                double testPositionRating = GetPositionRatingDouble(i);
                sumPositionRating += testPositionRating;
                countPositions++;
            }
            return sumPositionRating + countPositions;
        }

        public int GetBestPositionWithinLimit(int[] positionLimit)
        {
            double bestPositionRating = 0;
            PlayerPosition bestPosition = 0;
            List<double> positionRatings = new List<double>();
            for (int targetPosition = 0; targetPosition < (int)PlayerPosition.Count; targetPosition++)
            {
                var targetPositionQuota = positionLimit[targetPosition];
                if (targetPositionQuota == 0) continue;
                double testPositionRating = GetPositionRatingDouble(targetPosition);
                positionRatings.Add(testPositionRating);
                if (bestPositionRating < testPositionRating)
                {
                    bestPosition = (PlayerPosition)targetPosition;
                    bestPositionRating = testPositionRating;
                }
                else if (bestPositionRating == testPositionRating)
                {
                    //favor front court positions
                    //except for=>ss
                    if (targetPosition == (int)PlayerPosition.SS && bestPosition ==PlayerPosition.FOR)
                        continue;
                    bestPosition = (PlayerPosition)targetPosition;
                }
            }
            return (int)bestPosition;
        }
    }
}
