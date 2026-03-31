using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace FSM97Lib
{
    public class PlayerModel:IPlayerTrainableAttributes<int>,ITeamMember, IPerson, IObjectWithPersonName
    {
        int[] attributes = new int[(int)PlayerAttribute.Count];
        public int[] Attributes
        {
            get
            {
                return attributes;
            }
        }

        public ReadOnlyCollection<byte> AttributesBytes
        {
            get
            {
                return attributes.Select(x=>(byte)x).ToList().AsReadOnly();
            }
        }
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

        public int Position { get; set; }
        public string PositionName { get { 
                PlayerPosition position = (PlayerPosition)Position;
                return position.ToLocalizedString();
            }
        }
        public double GetPositionRatingDouble(int position)
        {
            return PositionRatings.GetPositionRatingDouble
                (position, this);
        }

        public int PositionRating
        {
            get
            {
                return (int)PositionRatings.GetPositionRatingDouble
                (Position, this);
            }
        }
        public int BestPosition { 
            get {

                int bestPosition = 0;
                double bestPositionRating = 0;
                for (int i = 0; i < (int)PlayerPosition.Count; i++)
                {
                    double testPositionRating = PositionRatings.GetPositionRatingDouble(i, this);
                    if (bestPositionRating < testPositionRating)
                    {
                        //this would favor backcout and GK positions
                        bestPosition = i;
                        bestPositionRating = testPositionRating;
                    }
                    
                }
                return bestPosition;
            } 
        
        }
        public string BestPositionName { 
            get { 
                PlayerPosition position = (PlayerPosition)BestPosition;
                return position.ToLocalizedString();
            } 
        }
        public int BestPositionRating
        {
            get {
                return (int)PositionRatings.GetPositionRatingDouble
                (BestPosition, this);
            }
        }
        public string TeamName { get ; set ; }
        public string TeamAbbrivation { get ; set ; }
        public int Number { get ; set ; }
        public int Status { get ; set ; }
        public int ContractWeeks { get ; set ; }
        public double Salary { get ; set ; }
        public int Goals { get ; set ; }
        public int MVP { get ; set ; }
        public int Form { get ; set ; }
        public int Moral { get ; set ; }
        public int Energy { get ; set ; }

        public int GamesThisSeason { get ; set ; }

        TeamModel team;

        public TeamModel Team
        { 
            get { return team; }
            set
            {
                team = value;
                TeamName = team.Name;
                TeamAbbrivation = team.Abbreviation;
            }
        }

        public string NationalityName { get; set; }

        public int Age {get; set;}
        public string LastName { get ; set ; }
        public string FirstName { get ; set ; }
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
        //see country,.txt for coutries
        public static string GetNationalityName(int nationality)
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
                return sum / 27;
            }
        }
        public DateTime BirthDay 
        { 
            get
            {

                    return dateOffsetBase.AddDays(BirthDateOffset);
                
            }
        }
        public static DateTime dateOffsetBase = new DateTime(1899, 12, 30);

        public PlayerModel(PlayerModel player)
        {
            attributes = player.Attributes.Select(x =>x).ToArray();
            FirstName = player.FirstName;
            LastName = player.LastName;
            BirthDateOffset = player.BirthDateOffset;
            Position = player.Position;
            Number = player.Number;
            Age = player.Age;
        }

        public PlayerModel()
        {
        }
        public PlayerModel(byte[] newAttributes)
        {
            attributes = newAttributes.Select(x =>(int) x).ToArray();
        }

        public PlayerModel(List<int> attributeLeftToTrain)
        {
            attributes = attributeLeftToTrain.Select(x =>x).ToArray();
        }

        public int BirthDateOffset{ get; set; }
        public static int CompareAttributesApproximately(IPlayerTrainableAttributes<int> fromPlayer, IPlayerTrainableAttributes<int> toPlayer)
        {
            for (int i = 0; i < (int)PlayerAttribute.Count; i++)
            {
                var result = fromPlayer.Attributes[i] - toPlayer.Attributes[i];
                if (result > 1 || result < -1) return result;

            }
            return 0;

        }
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("{0} yo/{1}:{2}/{3}:{4}/{5}: {6} {7}/{8}", 
                Age,PositionName, PositionRating,BestPositionName, BestPositionRating,
                Number, LastName, FirstName,NationalityName);
            stringBuilder.AppendFormat("attributes: {0},{1},{2}/{3},{4},{5}/{6},{7},{8},{9},{10}/",
                Speed, Agility, Acceleration, Stamina, Strength, Fitness,Shooting, Passing, Heading, Control, Dribbling);
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
