using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSM97Lib
{
    public class PlayerModelDouble : IPlayerTrainableAttributes<double>,ITeamMember,IPerson
    {
        double[] preciseAttributes = new double[(int)PlayerAttribute.Count];

        public PlayerModelDouble(PlayerModel player)
        {
            preciseAttributes = player.Attributes.Select(x => (double)x).ToArray();
            FirstName = player.FirstName;
            LastName = player.LastName;
            BirthDateOffset = player.BirthDateOffset;
            Position = player.Position;
            Number = player.Number;
            Age = player.Age;
        }
        public PlayerModelDouble(PlayerModelDouble player)
        {
            preciseAttributes = player.Attributes.Select(x => x).ToArray();
            FirstName = player.FirstName;
            LastName = player.LastName;
            BirthDateOffset = player.BirthDateOffset;
            Position = player.Position;
            Number = player.Number;
            Age = player.Age;
        }

        public double[] Attributes
        {
            get
            {
                return preciseAttributes;
            }
        }

        public  double Speed
        {
            get { return preciseAttributes[(int)PlayerAttribute.Speed]; }
            set { preciseAttributes[(int)PlayerAttribute.Speed] = value; }
        }
        public  double Agility
        {
            get { return preciseAttributes[(int)PlayerAttribute.Agility]; }
            set { preciseAttributes[(int)PlayerAttribute.Agility] = value; }
        }
        public  double Acceleration
        {
            get { return preciseAttributes[(int)PlayerAttribute.Acceleration]; }
            set { preciseAttributes[(int)PlayerAttribute.Acceleration] = value; }
        }
        public  double Stamina
        {
            get { return preciseAttributes[(int)PlayerAttribute.Stamina]; }
            set { preciseAttributes[(int)PlayerAttribute.Stamina] = value; }
        }
        public  double Strength
        {
            get { return preciseAttributes[(int)PlayerAttribute.Strength]; }
            set { preciseAttributes[(int)PlayerAttribute.Strength] = value; }
        }
        public  double Fitness
        {
            get { return preciseAttributes[(int)PlayerAttribute.Fitness]; }
            set { preciseAttributes[(int)PlayerAttribute.Fitness] = value; }
        }
        public  double Shooting
        {
            get { return preciseAttributes[(int)PlayerAttribute.Shooting]; }
            set { preciseAttributes[(int)PlayerAttribute.Shooting] = value; }
        }
        public  double Passing
        {
            get { return preciseAttributes[(int)PlayerAttribute.Passing]; }
            set { preciseAttributes[(int)PlayerAttribute.Passing] = value; }
        }
        public  double Heading
        {
            get { return preciseAttributes[(int)PlayerAttribute.Heading]; }
            set { preciseAttributes[(int)PlayerAttribute.Heading] = value; }
        }
        public  double Control
        {
            get { return preciseAttributes[(int)PlayerAttribute.Control]; }
            set { preciseAttributes[(int)PlayerAttribute.Control] = value; }
        }
        public  double Dribbling
        {
            get { return preciseAttributes[(int)PlayerAttribute.Dribbling]; }
            set { preciseAttributes[(int)PlayerAttribute.Dribbling] = value; }
        }
        public  double TackleDetermination
        {
            get { return preciseAttributes[(int)PlayerAttribute.TackleDetermination]; }
            set { preciseAttributes[(int)PlayerAttribute.TackleDetermination] = value; }
        }
        public  double TackleSkill
        {
            get { return preciseAttributes[(int)PlayerAttribute.TackleSkill]; }
            set { preciseAttributes[(int)PlayerAttribute.TackleSkill] = value; }
        }
        public  double Coolness
        {
            get { return preciseAttributes[(int)PlayerAttribute.Coolness]; }
            set { preciseAttributes[(int)PlayerAttribute.Coolness] = value; }
        }
        public  double Awareness
        {
            get { return preciseAttributes[(int)PlayerAttribute.Awareness]; }
            set { preciseAttributes[(int)PlayerAttribute.Awareness] = value; }
        }
        public  double Flair
        {
            get { return preciseAttributes[(int)PlayerAttribute.Flair]; }
            set { preciseAttributes[(int)PlayerAttribute.Flair] = value; }
        }
        public  double Kicking
        {
            get { return preciseAttributes[(int)PlayerAttribute.Kicking]; }
            set { preciseAttributes[(int)PlayerAttribute.Kicking] = value; }
        }
        public  double Throwing
        {
            get { return preciseAttributes[(int)PlayerAttribute.Throwing]; }
            set { preciseAttributes[(int)PlayerAttribute.Throwing] = value; }
        }
        public  double Handling
        {
            get { return preciseAttributes[(int)PlayerAttribute.Handling]; }
            set { preciseAttributes[(int)PlayerAttribute.Handling] = value; }
        }
        public  double ThrowIn
        {
            get { return preciseAttributes[(int)PlayerAttribute.ThrowIn]; }
            set { preciseAttributes[(int)PlayerAttribute.ThrowIn] = value; }
        }
        public  double Leadership
        {
            get { return preciseAttributes[(int)PlayerAttribute.Leadership]; }
            set { preciseAttributes[(int)PlayerAttribute.Leadership] = value; }
        }
        public  double Consistency
        {
            get { return preciseAttributes[(int)PlayerAttribute.Consistency]; }
            set { preciseAttributes[(int)PlayerAttribute.Consistency] = value; }
        }
        public  double Determination
        {
            get { return preciseAttributes[(int)PlayerAttribute.Determination]; }
            set { preciseAttributes[(int)PlayerAttribute.Determination] = value; }
        }
        public  double Greed
        {
            get { return preciseAttributes[(int)PlayerAttribute.Greed]; }
            set { preciseAttributes[(int)PlayerAttribute.Greed] = value; }
        }

        public int Position { get ; set ; }
        public int BestPosition { get ; set ; }
        public string TeamName { get; set; }
        public string TeamAbbrivation { get; set; }
        public int Number { get; set; }
        public int Status { get; set; }
        public int ContractWeeks { get; set; }
        public double Salary { get; set; }
        public int Goals { get; set; }
        public int MVP { get; set; }
        public int Form { get; set; }
        public int Moral { get; set; }
        public int Energy { get; set; }
        public int GamesThisSeason { get; set; }
        public TeamModel Team { get; set; }

        public int Statistics { get {

                double sum = 0;
                for (int i = 0; i < (int)PlayerAttribute.Count; i++)
                {
                    sum += Attributes[i];
                }
                sum += Form + Moral + Energy;
                return (int)sum / 27;

            } 
        }

        public string PositionName {
            get
            {
                PlayerPosition position = (PlayerPosition)Position;
                return position.ToLocalizedString();
            }
        }

        public string NationalityName { get; set; }

        public int Age { get; set; }

        public string LastName { get ; set ; }
        public string FirstName { get ; set ; }
        int nationality;
        public int Nationality
        {
            get { return nationality; }
            set
            {
                nationality = value;
                NationalityName = PlayerModel.GetNationalityName(value);
            }
        }

        public DateTime BirthDay { get
            {
                return PlayerModel.dateOffsetBase.AddDays(BirthDateOffset);
            }
        }
        public int BirthDateOffset { get; set; }


        public int PositionRating
        {
            get
            {
                return (int)PositionRatings.GetPositionRatingDouble
                (Position, this);
            }
        }

        public string BestPositionName
        {
            get
            {
                PlayerPosition position = (PlayerPosition)BestPosition;
                return position.ToLocalizedString();
            }
        }
        public int BestPositionRating
        {
            get
            {
                return (int)PositionRatings.GetPositionRatingDouble
                (BestPosition, this);
            }
        }


        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("{0} yo/{1}:{2}/{3}:{4}/{5}: {6} {7}/{8}",
                Age, PositionName, PositionRating, BestPositionName, BestPositionRating,
                Number, LastName, FirstName, NationalityName);
            stringBuilder.AppendFormat("attributes: {0},{1},{2}/{3},{4},{5}/{6},{7},{8},{9},{10}/",
                Speed, Agility, Acceleration, Stamina, Strength, Fitness, Shooting, Passing, Heading, Control, Dribbling);
            stringBuilder.AppendFormat("{0},{1}/",
                TackleDetermination, TackleSkill);
            stringBuilder.AppendFormat("{0},{1},{2}/",
                Coolness, Awareness, Flair);
            stringBuilder.AppendFormat("{0},{1},{2}/",
                Kicking, Throwing, Handling);
            stringBuilder.AppendFormat("{0},{1},{2},{3},{4}",
                ThrowIn, Leadership, Consistency, Determination, Greed);
            return stringBuilder.ToString();
        }
    }
}
