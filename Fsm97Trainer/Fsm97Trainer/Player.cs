using FSM97Lib;
using System;

namespace Fsm97Trainer
{
    public class Player
    {
        public int Speed { get; set; }
        public int Agility { get; set; }
        public int Acceleration { get; set; }
        public int Stamina { get; set; }
        public int Strength { get; set; }
        public int Fitness { get; set; }
        public int Shooting { get; set; }
        public int Passing { get; set; }
        public int Heading { get; set; }
        public int Control { get; set; }
        public int Dribbling { get; set; }
        public int TackleDetermination { get; set; }
        public int TackleSkill { get; set; }
        public int Coolness { get; set; }
        public int Awareness { get; set; }
        public int Flair { get; set; }
        public int Kicking { get; set; }
        public int Throwing { get; set; }
        public int Handling { get; set; }
        public int ThrowIn { get; set; }
        public int Leadership { get; set; }
        public int Consistency { get; set; }
        public int Determination { get; set; }
        public int Greed { get; set; }
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
                PositionName = GetPositionName(position);
                PositionRating = GetPositionRating(Position);
            }
        }
        public string PositionName { get; set; }
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
                return (Speed + Agility + Acceleration + Stamina + Strength + Fitness + Shooting + Passing + Heading + Control
                    + Dribbling + TackleDetermination + TackleSkill + Coolness + Awareness + Flair + Kicking + Throwing + Handling
                    + ThrowIn + Leadership + Consistency + Determination + Greed+ Form+ Moral+ Energy) / 24 ;
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
        public override string ToString()
        {
            return String.Format("{0}, {1}: age {2}, position {3}, rating {4},Throwin {5}, Lead {6}, Greed {7}", LastName, FirstName,
                Age,PositionName,PositionRating,                
                ThrowIn, Leadership, Greed);
        }
        internal static int CompareAttributes(Player fromPlayer, Player toPlayer)
        {
            var result = fromPlayer.Speed - toPlayer.Speed; if (result != 0) return result;
            result = fromPlayer.Agility - toPlayer.Agility; if (result != 0) return result;
            result = fromPlayer.Acceleration - toPlayer.Acceleration; if (result != 0) return result;
            result = fromPlayer.Stamina - toPlayer.Stamina; if (result != 0) return result;
            result = fromPlayer.Strength - toPlayer.Strength; if (result != 0) return result;
            result = fromPlayer.Fitness - toPlayer.Fitness; if (result != 0) return result;
            result = fromPlayer.Shooting - toPlayer.Shooting; if (result != 0) return result;
            result = fromPlayer.Passing - toPlayer.Passing; if (result != 0) return result;
            result = fromPlayer.Heading - toPlayer.Heading; if (result != 0) return result;
            result = fromPlayer.Control - toPlayer.Control; if (result != 0) return result;
            result = fromPlayer.Dribbling - toPlayer.Dribbling; if (result != 0) return result;
            result = fromPlayer.Coolness - toPlayer.Coolness; if (result != 0) return result;
            result = fromPlayer.Awareness - toPlayer.Awareness; if (result != 0) return result;
            result = fromPlayer.TackleDetermination - toPlayer.TackleDetermination; if (result != 0) return result;
            result = fromPlayer.TackleSkill - toPlayer.TackleSkill; if (result != 0) return result;
            result = fromPlayer.Flair - toPlayer.Flair; if (result != 0) return result;
            result = fromPlayer.Kicking - toPlayer.Kicking; if (result != 0) return result;
            result = fromPlayer.Throwing - toPlayer.Throwing; if (result != 0) return result;
            result = fromPlayer.Handling - toPlayer.Handling; if (result != 0) return result;
            result = fromPlayer.ThrowIn - toPlayer.ThrowIn; if (result != 0) return result;
            result = fromPlayer.Leadership - toPlayer.Leadership; if (result != 0) return result;
            result = fromPlayer.Consistency - toPlayer.Consistency; if (result != 0) return result;
            result = fromPlayer.Determination - toPlayer.Determination; if (result != 0) return result;
            result = fromPlayer.Greed - toPlayer.Greed; return result;
        }
        public void UpdateBestPosition()
        {
            int bestPosition = 0;
            double bestPositionRating = 0;
            for (int i = 0; i < 16; i++)
            {
                double testPositionRating = GetPositionRatingDouble(i);
                if (bestPositionRating < testPositionRating)
                {
                    bestPosition = i;
                    bestPositionRating = testPositionRating;
                }
            }
            BestPosition = bestPosition;
            BestPositionName = GetPositionName(bestPosition);
            BestPositionRating = (int) bestPositionRating;
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
            return playerRating / 100;
        }


        public int GetPositionRating(int position)
        {
            return (int)GetPositionRatingDouble(position);
        }
        public string GetPositionName(int position)
        {
            return Enum.GetName(typeof(PlayerPosition), position);
        }

        internal int BestFitInFormation(Formation targetFormation)
        {
            double bestPositionRating = 0;
            int bestPosition=0;
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
            for (int i = 1; i <=(int) PlayerPosition.SS; i++)
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
        internal double GetBestPositionRatingExceptGKInFormation(Formation formation)
        {
            double bestPositionRating = 0;
            int bestPosition = 0;
            for (int i = 1; i <=(int)PlayerPosition.SS; i++)
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

        internal double GetBestPositionRating(PlayerPosition[] targetPositions)
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

        internal int BestFitInPositions(PlayerPosition[] targetPositions)
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

        internal double  GetAveragePositionRatingInFormationExceptTargetPositionAndGK(Player data, int position, Formation formation)
        {
            double sumPositionRating = 0;
            int countPositions = 0;

            for (int i = 1; i <= (int)PlayerPosition.SS; i++)
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
    }
}
