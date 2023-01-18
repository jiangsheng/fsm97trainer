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
        public int Position { get; set; }
        public string PositionName { get; set; }
        public int PositionRating { get; set; }
        public int BestPosition { get; set; }
        public string BestPositionName { get; set; }
        public int BestPositionRating { get; set; }
        public int Nationality { get; set; }
        public string NationalityName { get; set; }
        public string TeamName { get; set; }
        public string TeamAbbrivation { get; set; }
        public int Age { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public int Number { get; set; }
        public int Status { get; set; }
        public int ContractWeeks {get; set; }
        public int Form{ get; set; }
        public int Moral { get; set; }
        public int Energy { get; set; }
        public int Goals{ get; set; }
        public int MVP{ get; set; }
        public int GamesThisSeason{ get; set; }

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
    }
    public class NamesakePlayers:List<Player>
    {

    }
    public class PlayerCollectionWithIndex:Dictionary<string,Dictionary<string, NamesakePlayers>>
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
    public class WritePlayerTaskCollectionWithIndex:Dictionary<string,
        Dictionary<string, List<WritePlayerTask>>>
    {
        public List<WritePlayerTask> LookupByName(string lastName, string fistName )
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
                respawnFromPlayersWithSameLastName = respawnFromPlayersWithSameLastName.OrderBy(p=>p.Greed+ p.Leadership + p.ThrowIn).ToList();
                respawnToPlayersWithSameLastName = respawnToPlayersWithSameLastName.OrderByDescending(p => p.Greed + p.Leadership + p.ThrowIn).ToList();
                int i = 0;
                for (; i < respawnFromPlayersWithSameLastName.Count && i< respawnToPlayersWithSameLastName.Count; i++)
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
