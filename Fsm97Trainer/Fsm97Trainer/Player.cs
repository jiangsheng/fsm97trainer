using CsvHelper.Configuration.Attributes;
using Fsm97Trainer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public int Statistics
        {
            get
            {
                return (Speed + Agility + Acceleration + Stamina + Strength + Fitness + Shooting + Passing + Heading + Control
                    + Dribbling + TackleDetermination + TackleSkill + Coolness + Awareness + Flair + Kicking + Throwing + Handling
                    + ThrowIn + Leadership + Consistency + Determination + Greed) / 24 + (Form + Moral + Energy) / 25;
            }
        }
        Team team;
        [Ignore]
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
            return String.Format("{0}, {1}:{2}, {3}, {4}", LastName, FirstName, ThrowIn, Leadership, Greed);
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
            int bestPositionRating = 0;
            for (int i = 0; i < 16; i++)
            {
                int testPositionRating = GetPositionRating(i);
                if (bestPositionRating < testPositionRating)
                {
                    bestPosition = i;
                    bestPositionRating = testPositionRating;
                }
            }
            BestPosition = bestPosition;
            BestPositionName = GetPositionName(bestPosition);
            BestPositionRating = bestPositionRating;
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

        public int GetPositionRating(int position)
        {
            int playerRating = 0;
            switch (position)
            {
                case 0:
                    playerRating = Speed * 2 + Agility * 25 +
                        Passing * 2 + Control * 4 +
                        Coolness * 7 + Awareness * 10 +
                        Kicking * 8 + Throwing * 6 + Handling * 30 +
                        Consistency * 6;
                    break;
                case 1:
                case 2:
                    playerRating = Speed * 3 +
                        Passing * 7 + Heading * 8 +
                        TackleDetermination * 10 + TackleSkill * 44 +
                        Coolness * 7 + Awareness * 15 +
                        Consistency * 4 + Determination * 2;
                    break;
                case 3:
                    playerRating = Speed * 3 +
                        Passing * 3 + Heading * 14 +
                        TackleDetermination * 10 + TackleSkill * 50 +
                        Coolness * 7 + Awareness * 8 +
                        Consistency * 2 + Leadership * 3;
                    break;
                case 4:
                case 5:
                    playerRating = Speed * 7 + Agility * 4 + Acceleration * 11 +
                        Passing * 12 + Dribbling * 26 +
                        TackleDetermination * 3 + TackleSkill * 26 +
                        Flair * 5 + Awareness * 6;
                    break;
                case 6:
                    playerRating = Speed * 12 + Acceleration * 6 +
                        Passing * 15 + Heading * 3 + Dribbling * 15 +
                        TackleDetermination * 3 + TackleSkill * 26 +
                        Awareness * 20;
                    break;
                case 7:
                    playerRating = Speed * 5 +
                        Passing * 40 + Heading * 5 +
                        TackleDetermination * 3 + TackleSkill * 27 +
                        Awareness * 20;
                    break;
                case 8:
                case 9:
                    playerRating = Speed * 10 + Acceleration * 5 +
                        Shooting * 3 + Passing * 42 + Control * 5 + Dribbling * 5 +
                        TackleSkill * 20 +
                        Flair * 5 + Awareness * 5;
                    break;
                case 10:
                    playerRating = Speed * 10 + Acceleration * 5 +
                        Shooting * 5 + Passing * 46 + Control * 5 + Dribbling * 5 +
                        TackleSkill * 14 +
                        Flair * 5 + Awareness * 5;
                    break;
                case 11:
                case 12:
                    playerRating = Speed * 10 + Agility * 3 + Acceleration * 10 +
                        Shooting * 3 + Passing * 31 + Control * 3 + Dribbling * 27 +
                        TackleSkill * 3 +
                         Awareness * 3 + Flair * 7;
                    break;
                case 13:
                    playerRating = Speed * 12 + Agility * 2 + Acceleration * 8 +
                        Shooting * 4 + Passing * 14 + Heading + Control * 10 + Dribbling * 27 +
                         Awareness * 12 + Flair * 10;
                    break;
                case 14:
                    playerRating = Speed * 10 + Agility * 2 + Acceleration * 9 +
                        Shooting * 36 + Passing * 4 + Heading * 10 + Control * 10 + Dribbling * 3 +
                        +Coolness * 3 + Awareness * 4 + Flair * 9;
                    break;
                case 15:
                    playerRating = Speed * 6 + Agility * 2 + Acceleration * 6 +
                        Shooting * 29 + Passing * 16 + Heading * 7 + Control * 13 + Dribbling * 6 +
                        +Coolness * 2 + Awareness * 3 + Flair * 10;
                    break;
            }
            return playerRating / 100;
        }
        public string GetPositionName(int position)
        {
            switch (position)
            {
                case 0: return "GK";
                case 1: return "RB";
                case 2: return "LB";
                case 3: return "CD";
                case 4: return "RWB";
                case 5: return "LWB";
                case 6: return "SW";
                case 7: return "DM";
                case 8: return "RM";
                case 9: return "LM";
                case 10: return "AM";
                case 11: return "RW";
                case 12: return "LW";
                case 13: return "FR";
                case 15: return "SS";
                case 14: return "FOR";
                default: return String.Empty;
            }
        }
    }
    public class NamesakePlayers : List<Player>
    {

    }
    public class PlayerCollectionWithIndex : Dictionary<string, Dictionary<string, NamesakePlayers>>
    {
        public PlayerCollectionWithIndex(List<Player> players)
        {
            foreach (var player in players)
            {
                if (!this.ContainsKey(player.LastName))
                {
                    this.Add(player.LastName, new Dictionary<string, NamesakePlayers>());
                }
                if (!this[player.LastName].ContainsKey(player.FirstName))
                {
                    this[player.LastName].Add(player.FirstName, new NamesakePlayers());
                }
                this[player.LastName][player.FirstName].Add(player);
            }
        }
        public NamesakePlayers LookupByName(string lastName, string fistName)
        {
            if (!this.ContainsKey(lastName)) return null;
            if (!this[lastName].ContainsKey(fistName)) return null;
            return this[lastName][fistName];
        }
        public List<Player> LookupByLastName(string lastName)
        {
            var result = new List<Player>();
            if (!this.ContainsKey(lastName)) return result;
            foreach (var item in this[lastName])
            {
                result.AddRange(item.Value);
            }
            return result;
        }
    }
    public enum WritePlayerTaskAction
    {
        None,
        Update,
        Respawn
    }
    public class WritePlayerTask
    {
        public WritePlayerTaskAction WritePlayerTaskAction { get; set; }
        public Player From { get; set; }
        public Player To { get; set; }
    }
    public class WritePlayerTaskCollectionWithIndex : Dictionary<string,
        Dictionary<string, List<WritePlayerTask>>>
    {
        public List<WritePlayerTask> LookupByName(string lastName, string fistName)
        {
            var result = new List<WritePlayerTask>();
            if (!this.ContainsKey(lastName)) return result;
            if (!this[lastName].ContainsKey(fistName)) return result;
            return this[lastName][fistName];
        }
        public List<WritePlayerTask> LookupByLastName(string lastName)
        {
            var result = new List<WritePlayerTask>();
            if (!this.ContainsKey(lastName)) return result;
            foreach (var item in this[lastName])
            {
                result.AddRange(item.Value);
            }
            return result;
        }
        public WritePlayerTaskCollectionWithIndex(PlayerCollectionWithIndex from, PlayerCollectionWithIndex to)
        {
            var respawnFromPlayers = new List<Player>();
            var respawnToPlayers = new List<Player>();
            foreach (var item in to)
            {
                var toLastNameValues = item.Value.Values.ToList();
                var respawnFromPlayersWithSameLastName = from.LookupByLastName(item.Key);
                var respawnToPlayersWithSameLastName = to.LookupByLastName(item.Key);
                foreach (var toPlayers in toLastNameValues)
                {
                    foreach (var toPlayer in toPlayers)
                    {
                        var fromPlayers = from.LookupByName(toPlayer.LastName, toPlayer.FirstName);
                        if (fromPlayers != null)
                        {
                            foreach (var fromPlayer in fromPlayers)
                            {
                                if (Player.CompareAttributes(fromPlayer, toPlayer) == 0)
                                {
                                    Debug.WriteLine(String.Format("Matched {0} with {1}", fromPlayer, toPlayer));
                                    Add(fromPlayer, toPlayer, WritePlayerTaskAction.None);
                                }
                                else if (toPlayer.Greed == fromPlayer.Greed && toPlayer.ThrowIn == fromPlayer.ThrowIn
                                && toPlayer.Leadership == fromPlayer.Leadership)
                                {
                                    Debug.WriteLine(String.Format("Matched {0} with {1}", fromPlayer, toPlayer));
                                    Add(fromPlayer, toPlayer, WritePlayerTaskAction.Update);
                                }

                                respawnFromPlayersWithSameLastName.RemoveAll(p => p.FirstName == fromPlayer.FirstName
                                    && p.Greed == fromPlayer.Greed && p.ThrowIn == fromPlayer.ThrowIn
                                    && p.Leadership == fromPlayer.Leadership);
                                respawnToPlayersWithSameLastName.RemoveAll(p => p.FirstName == fromPlayer.FirstName
                                    && p.Greed == fromPlayer.Greed && p.ThrowIn == fromPlayer.ThrowIn
                                    && p.Leadership == fromPlayer.Leadership);
                            }
                        }
                    }
                }
                respawnFromPlayersWithSameLastName = respawnFromPlayersWithSameLastName.OrderBy(p => p.Greed + p.Leadership + p.ThrowIn).ToList();
                respawnToPlayersWithSameLastName = respawnToPlayersWithSameLastName.OrderByDescending(p => p.Greed + p.Leadership + p.ThrowIn).ToList();
                int i = 0;
                for (; i < respawnFromPlayersWithSameLastName.Count && i < respawnToPlayersWithSameLastName.Count; i++)
                {
                    var fromPlayer = respawnFromPlayersWithSameLastName[i];
                    var toPlayer = respawnToPlayersWithSameLastName[i];
                    Debug.WriteLine(String.Format("Matched {0} with {1}", fromPlayer, toPlayer));
                    Add(fromPlayer, toPlayer, WritePlayerTaskAction.Respawn);
                }
                for (int j = i; j < respawnFromPlayersWithSameLastName.Count; j++)
                {
                    Debug.WriteLine(String.Format("From Player {0} cannot be matched with players with the same last name", respawnFromPlayersWithSameLastName[j]));
                    respawnFromPlayers.Add(respawnFromPlayersWithSameLastName[j]);
                }
                for (int j = i; j < respawnToPlayersWithSameLastName.Count; j++)
                {
                    Debug.WriteLine(String.Format("To Player {0} cannot be matched with players with the same last name", respawnToPlayersWithSameLastName[j]));
                    respawnToPlayers.Add(respawnToPlayersWithSameLastName[j]);
                }
            }

            for (int i = 0; i < respawnFromPlayers.Count && i < respawnToPlayers.Count; i++)
            {
                var fromPlayer = respawnFromPlayers[i];
                var toPlayer = respawnToPlayers[i];
                Debug.WriteLine(String.Format("Randomly Matched {0} with {1}", fromPlayer, toPlayer));
                Add(fromPlayer, toPlayer, WritePlayerTaskAction.Respawn);
            }

        }
        protected void Add(Player from, Player to, WritePlayerTaskAction writePlayerTaskAction)
        {
            WritePlayerTask writePlayerTask = new WritePlayerTask { From = from, To = to, WritePlayerTaskAction = writePlayerTaskAction };
            if (!this.ContainsKey(from.LastName))
                base.Add(from.LastName, new Dictionary<string, List<WritePlayerTask>>());
            if (!this[from.LastName].ContainsKey(from.FirstName))
                this[from.LastName].Add(from.FirstName, new List<WritePlayerTask>());
            this[from.LastName][from.FirstName].Add(writePlayerTask);
        }

    }
}
